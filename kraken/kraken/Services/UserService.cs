using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using kraken.Messages;
using kraken.Models;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using Realms;
using Xamarin.Forms;

namespace kraken.Services
{
	public class UserService : IUserService
	{
		#region Data
		#region Fields
		private readonly HttpClient _client;
		#endregion
		#endregion

		#region .ctor
		public UserService()
		{
			_client = new HttpClient
			{
				MaxResponseContentBufferSize = 209715200 // 200 MB
			};
			_client.DefaultRequestHeaders.Add("Accept", "application/json");

			SetAuthenticationHeader();
		}
		#endregion

		#region Properties
		private static bool AuthenticationHeaderIsSet
		{
			get;
			set;
		}

		private static Realm Realm => Realm.GetInstance();
		#endregion

		#region IUserService members
		public async Task<bool> LoginUserAsync(string emailEntry, string passwordEntry)
		{
			if (IsThereInternet() == false)
			{
				return false;
			}

			if (!AuthenticationHeaderIsSet)
			{
				SetAuthenticationHeader();
			}

			const string restMethod = "login";
			var uri = new Uri(string.Format(Constants.RestUrl, restMethod));

			try
			{
				var jMessage = new JObject
				{
					{
						"email", emailEntry
					},
					{
						"password", passwordEntry
					},
					{
						"deviceToken", App.DeviceToken
					}
				};

				var json = jMessage.ToString();
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = _client.PostAsync(uri, content)
									  .Result;

				if (response.IsSuccessStatusCode)
				{
					var userInfo = await response.Content.ReadAsStringAsync();
					var userObj = JObject.Parse(userInfo);

					var isMaster = userObj["data"]["client_type"]
									   .ToString() ==
								   "master";

					var currentUser = new User
					{
						Name = userObj["data"]["client_info"]["name"]
							.ToString(),
						Phone = userObj["data"]["client_info"]["phone"]
							.ToString(),
						ClientType = userObj["data"]["client_type"]
							.ToString(),
						Token = userObj["data"]["access_token"]
							.ToString(),
						Email = emailEntry,
						DeviceToken = App.DeviceToken
					};

					if (isMaster)
					{
						currentUser.MasterId = userObj["data"]["client_info"]["master_id"]
							.ToString();
						currentUser.Distance = userObj["data"]["client_info"]["radius"]
							.ToString();
						currentUser.Status = userObj["data"]["client_info"]["status"]
							.ToString();
						currentUser.DrivingMode = userObj["data"]["client_info"]["way"]
							.ToString();
						App.IsUserMaster = true;
					}
					else
					{
						currentUser.Organization = userObj["data"]["client_info"]["organization"]
							.ToString();
						currentUser.Address = userObj["data"]["client_info"]["address"]
							.ToString();
					}

					Realm.Write(() =>
					{
						Realm.Add(currentUser, true);
					});

					if (isMaster)
					{
						StartSendingCoordinates();
					}

					return true;
				}

				var errorMessage = "";
				var errorInfo = await response.Content.ReadAsStringAsync();
				var errorObj = JObject.Parse(errorInfo);

				if (errorObj.ContainsKey("error"))
				{
					errorMessage = (string) errorObj["error"];
				}
				else if (errorObj.ContainsKey("errors"))
				{
					var errors = errorObj["errors"];

					if (errors["email"] != null)
					{
						errorMessage = (string) errors["email"]
							.First;
					}
					else if (errors["password"] != null)
					{
						errorMessage = (string) errors["password"]
							.First;
					}
				}

				await Application.Current.MainPage.DisplayAlert("Ошибка", errorMessage, "OK");
				return false;
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Не выполнено",
																ex.GetType()
																  .Name +
																"\n" +
																ex.Message,
																"OK");
			}

			return false;
		}

		public async Task<bool> SetDrivingModeAsync(string modeCode)
		{
			if (IsThereInternet() == false)
			{
				return false;
			}

			if (!AuthenticationHeaderIsSet)
			{
				SetAuthenticationHeader();
			}

			var restMethod = "masters/changeWayToTravel";
			var uri = new Uri(string.Format(Constants.RestUrl, restMethod));

			try
			{
				var jmessage = new JObject
				{
					{
						"way", modeCode
					}
				};
				var json = jmessage.ToString();

				var content = new StringContent(json, Encoding.UTF8, "application/json");
				HttpResponseMessage response = null;
				response = _client.PostAsync(uri, content)
								  .Result;

				if (response.IsSuccessStatusCode)
				{
					var responseMessage = await response.Content.ReadAsStringAsync();
					
					Realm.Write(() =>
					{
						Realm.All<User>().Single().DrivingMode = modeCode;
					});

					return true;
				}

				var errorInfo = await response.Content.ReadAsStringAsync();
				var errorMessage = ParseErrorMessage(errorInfo);

				Device.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.DisplayAlert("Не выполнено", errorMessage, "OK");
				});

				return false;
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Не выполнено",
																ex.GetType()
																  .Name +
																"\n" +
																ex.Message,
																"OK");
				return false;
			}
		}

		public async Task<bool> SetSearchFilterAsync(string radiusValue)
		{
			if (IsThereInternet() == false)
			{
				return false;
			}

			if (!AuthenticationHeaderIsSet)
			{
				SetAuthenticationHeader();
			}

			var restMethod = "client/changeRadius";
			var uri = new Uri(string.Format(Constants.RestUrl, restMethod));

			try
			{
				var jmessage = new JObject
				{
					{
						"radius", radiusValue
					}
				};
				var json = jmessage.ToString();

				var content = new StringContent(json, Encoding.UTF8, "application/json");
				var response = _client.PostAsync(uri, content)
								  .Result;

				if (response.IsSuccessStatusCode)
				{
					var responseMessage = await response.Content.ReadAsStringAsync();
					Realm.Write(() =>
					{
						Realm.All<User>().Single().Distance = radiusValue;
					});
					return true;
				}

				var errorInfo = await response.Content.ReadAsStringAsync();
				var errorMessage = ParseErrorMessage(errorInfo);

				Device.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.DisplayAlert("Не выполнено", errorMessage, "OK");
				});

				return false;
			}
			catch (AggregateException ex)
			{
				foreach (var exc in ex.InnerExceptions)
				{
					if (exc is NullReferenceException)
					{
						// handle NullReferenceException
						await Application.Current.MainPage.DisplayAlert("Не выполнено", string.Format("TimeoutException occured: {0}", ex.Message), "OK");
					}

					if (exc is TimeoutException)
					{
						// handle timeout
						await Application.Current.MainPage.DisplayAlert("Не выполнено", string.Format("TimeoutException occured: {0}", ex.Message), "OK");
					}
					else
					{
						await Application.Current.MainPage.DisplayAlert("Не выполнено",
																		ex.GetType()
																		  .Name +
																		"\n" +
																		ex.Message,
																		"OK");
					}

					// catch another Exception
				}

				return false;
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Не выполнено",
																ex.GetType()
																  .Name +
																"\n" +
																ex.Message,
																"OK");
				return false;
			}
		}

		public async Task<bool> SetStatusAsync(string statusValue)
		{
			if (IsThereInternet() == false)
			{
				return false;
			}

			if (!AuthenticationHeaderIsSet)
			{
				SetAuthenticationHeader();
			}

			var restMethod = "masters/changeStatus";
			var uri = new Uri(string.Format(Constants.RestUrl, restMethod));

			try
			{
				var jmessage = new JObject
				{
					{
						"status", statusValue
					}
				};
				var json = jmessage.ToString();

				var content = new StringContent(json, Encoding.UTF8, "application/json");
				HttpResponseMessage response = null;
				response = _client.PostAsync(uri, content)
								  .Result;

				if (response.IsSuccessStatusCode)
				{
					var responseMessage = await response.Content.ReadAsStringAsync();
					Realm.Write(() =>
					{
						Realm.All<User>().Single().Status = statusValue;
					});
					return true;
				}

				var errorInfo = await response.Content.ReadAsStringAsync();
				var errorMessage = ParseErrorMessage(errorInfo);

				Device.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.DisplayAlert("Не выполнено", errorMessage, "OK");
				});

				return false;
			}
			catch (Exception ex)
			{
				await Application.Current.MainPage.DisplayAlert("Не выполнено",
																ex.GetType()
																  .Name +
																"\n" +
																ex.Message,
																"OK");
				return false;
			}
		}
		#endregion

		#region Utility private methods
		public void StartSendingCoordinates()
		{
			var message = new StartLongRunningTaskMessage();
			MessagingCenter.Send(message, "StartLongRunningTaskMessage");
		}

		public void StopSendingCoordinates()
		{
			var message = new StopLongRunningTaskMessage();
			MessagingCenter.Send(message, "StopLongRunningTaskMessage");
		}

		// TODO: if no internet, show alert
		private bool IsThereInternet() => CrossConnectivity.Current.IsConnected;

		private string ParseErrorMessage(string errorInfo)
		{
			var errorMessage = "";
			var errorObj = JObject.Parse(errorInfo);

			if (errorObj.ContainsKey("error"))
			{
				errorMessage = (string) errorObj["error"];
			}
			else if (errorObj.ContainsKey("message"))
			{
				errorMessage = (string) errorObj["message"];
			}
			else if (errorObj.ContainsKey("errors"))
			{
				var errors = errorObj["errors"];

				if (errors["data"] != null)
				{
					errorMessage = (string) errors["data"]
						.First;
				}
				else if (errors["name"] != null)
				{
					errorMessage = (string) errors["name"]
						.First;
				}
			}

			return errorMessage;
		}

		private void SetAuthenticationHeader()
		{
			var realm = Realm.GetInstance();
			var users = realm.All<User>();

			if (users.Any())
			{
				var user = users.Last();
				_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
				AuthenticationHeaderIsSet = true;
			}
			else
			{
				AuthenticationHeaderIsSet = false;
			}
		}
		#endregion
	}
}
