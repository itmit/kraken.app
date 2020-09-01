using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using Realms;

namespace kraken.PageModels
{
	[AddINotifyPropertyChangedInterface]
	public class MyProfilePageModel : FreshBasePageModel
	{
		#region Data
		#region Fields
		private KeyValuePair<string, string> _drivingMode;

		private KeyValuePair<string, string> _selectedDistance;
		private KeyValuePair<string, string> _selectedStatus;
		private readonly IUserService _userService;
		#endregion
		#endregion

		#region .ctor
		public MyProfilePageModel(IUserService userService)
		{
			_userService = userService;

			UserName = "";
			UserOrganization = "";
			UserEmail = "";
			UserPhone = "";
			UserAddress = "";
		}
		#endregion

		#region Properties
		public User CurrentUser
		{
			get;
			set;
		}

		public List<KeyValuePair<string, string>> DistanceList
		{
			get;
			set;
		}

		public List<KeyValuePair<string, string>> DrivingList
		{
			get;
			set;
		}

		public bool IsUserMaster
		{
			get;
			set;
		}

		public List<KeyValuePair<string, string>> StatusList
		{
			get;
			set;
		}

		public string UserAddress
		{
			get;
			set;
		}

		public string UserEmail
		{
			get;
			set;
		}

		public string UserName
		{
			get;
			set;
		}

		public string UserOrganization
		{
			get;
			set;
		}

		public string UserPhone
		{
			get;
			set;
		}

		public ICommand OnRequestsButtonClicked
		{
			get
			{
				return new FreshAwaitCommand((param, tcs) =>
				{
					OpenRequestsPage();
					tcs.SetResult(true);
				});
			}
		}

		private static Realm Realm => Realm.GetInstance();

		public KeyValuePair<string, string> SelectedDistance
		{
			get => _selectedDistance;
			set
			{
				_selectedDistance = value;
				if (_selectedDistance.Key != null)
				{
					ChangeRange(_selectedDistance.Key);
				}
			}
		}

		public KeyValuePair<string, string> SelectedStatus
		{
			get => _selectedStatus;
			set
			{
				_selectedStatus = value;
				if (_selectedStatus.Key != null)
				{
					ChangeStatus(_selectedStatus.Key);
				}
			}
		}

		public KeyValuePair<string, string> SelectedVehicle
		{
			get => _drivingMode;
			set
			{
				_drivingMode = value;
				if (_drivingMode.Key != null)
				{
					ChangeDrivingMode(_drivingMode.Key);
				}
			}
		}
		#endregion

		#region Overrided
		public override void Init(object initData)
		{
			base.Init(initData);
			SetUpDictionaries();
			LoadUserInfo();
		}

		protected override void ViewIsAppearing(object sender, EventArgs e)
		{
			LoadUserInfo();
			IsUserMaster = App.IsUserMaster;
			base.ViewIsAppearing(sender, e);
		}
		#endregion

		#region Private
		private void ChangeDrivingMode(string value)
		{
			_userService.SetDrivingModeAsync(value);
		}

		private void ChangeRange(string value)
		{
			_userService.SetSearchFilterAsync(value);
		}

		private void ChangeStatus(string value)
		{
			_userService.SetStatusAsync(value);
		}

		private void LoadUserInfo()
		{
			var users = Realm.All<User>();
			if (!users.Any())
			{
				return;
			}

			CurrentUser = users.Last();

			UserName = CurrentUser.Name;
			UserOrganization = CurrentUser.Organization;
			UserEmail = CurrentUser.Email;
			UserPhone = CurrentUser.Phone;
			UserAddress = CurrentUser.Address;

			_selectedDistance = DistanceList.SingleOrDefault(d => CurrentUser.Distance != null && d.Key.Equals(CurrentUser.Distance));
			_drivingMode = DrivingList.SingleOrDefault(d => CurrentUser.DrivingMode != null && d.Key.Equals(CurrentUser.DrivingMode));
			_selectedStatus = StatusList.SingleOrDefault(d => CurrentUser.Status != null && d.Key.Equals(CurrentUser.Status));
			
			RaisePropertyChanged(nameof(SelectedDistance));
			RaisePropertyChanged(nameof(SelectedVehicle));
			RaisePropertyChanged(nameof(SelectedStatus));
		}

		private async void OpenRequestsPage()
		{
			if (IsUserMaster)
			{
				await CoreMethods.SwitchSelectedTab<AcceptedRequestPageModel>();
			}
			else
			{
				await CoreMethods.SwitchSelectedTab<MyRequestPageModel>();
			}
		}

		private void SetUpDictionaries()
		{
			var DistanceItems = new Dictionary<string, string>
			{
				{
					"0", "Ближайшие"
				},
				{
					"1", "1км"
				},
				{
					"5", "5км"
				},
				{
					"10", "10км"
				}
			};

			var DrivingModeItems = new Dictionary<string, string>
			{
				{
					"driving", "на автомобиле"
				},
				{
					"walking", "пешком"
				}
			};

			var StatusItems = new Dictionary<string, string>
			{
				{
					"free", "свободен"
				},
				{
					"busy", "занят"
				},
				{
					"offline", "оффлайн"
				}
			};

			DistanceList = DistanceItems.ToList();
			DrivingList = DrivingModeItems.ToList();
			StatusList = StatusItems.ToList();
		}
		#endregion
	}
}
