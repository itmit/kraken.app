using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using Realms;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using static System.Boolean;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class RequestDetailPageModel : FreshBasePageModel
    {
		private readonly IRequestStorageService _requestStorage;
		private Request _request;
        private Master _selectedMaster;
		private bool _isMasterSelected;
		private bool _isCloseRequestAvaliable;

		#region Public Variables
        public string RequestName { get; private set; }
        public string RequestDescription { get; private set; }
        public string RequestStatus { get; private set; }
        public string MasterDistance { get; private set; }
        public string RequestAddress { get; private set; }
        public string RequestPhone { get; private set; }
        public string RequestClientName { get; private set; }
        public string RequestUrgency { get; private set; }
        public string RequestFileUrl { get; private set; }
        public List<Master> Masters { get; private set; }
        public int TotalMasters { get; private set; }

        public Master SelectedMaster
        {
            get { return _selectedMaster; }
            set
            {
                _selectedMaster = value;
                if (value != null)
				{
					SendAcceptMasterRequest(value);
				}
			}
        }

		public bool IsMasterSelected
		{
			get => _isMasterSelected;
			private set 
			{
				_isMasterSelected = value;
				RaisePropertyChanged(nameof(CanAcceptRequest));
				RaisePropertyChanged(nameof(CanCancelRequest));
			}
        }

		public bool IsCurrentUserMaster { get; private set; }

		public bool IsCloseRequestAvaliable
		{
			get => _isCloseRequestAvaliable;
			private set
			{
				_isCloseRequestAvaliable = value;
				RaisePropertyChanged(nameof(CanAcceptRequest));
				RaisePropertyChanged(nameof(CanCancelRequest));
			}
        }

		public bool IsRequestHasImage { get; private set; }
        public bool IsSelectMasterButtonVisible { get; private set; }
        #endregion

        #region Commands
        public ICommand ShowMasterListCommand
        {
            get
            {
                return new Command(async (param) =>
                {

                    await CoreMethods.PushPageModel<MasterSelectPageModel>(_request);
                });
            }
        }

        public ICommand AcceptRequestCommand
        {
            get
            {
                return new Command((param) =>
                {
                    SendAcceptRequest();
                });
            }
        }

        public ICommand DeclineRequestCommand
        {
            get
            {
                return new Command((param) =>
                {
                    SendDeclineRequest();
                });
            }
        }

        public ICommand CloseRequestCommand
        {
            get
            {
                return new Command((param) =>
                {
                    CloseRequest();
                });
            }
        }
        #endregion

        public RequestDetailPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        public override void Init(object initData)
        {
            base.Init(initData);

			var users = Realm.All<User>();
			if (!users.Any())
			{
				return;
			}

			CurrentUser = users.Single();

            _request = (Request)initData;
            CurrentPage.Title = _request.Work;

            FillRequestValues();
            GetRequestMastersAsync();
		}

		private User CurrentUser
		{
			get;
			set;
		}

		private static Realm Realm => Realm.GetInstance();

        private void FillRequestValues()
        {
            RequestName = _request.Work;
            RequestDescription = _request.Description;
            RequestStatus = "Статус: " + _request.StatusText;
            MasterDistance = _request.MasterDistance + " км";
            RequestAddress = _request.Address;
            RequestPhone = _request.Phone;
            RequestClientName = _request.Name;

            if(_request.File == null)
            {
                IsRequestHasImage = false;
            }
            else
            {
                RequestFileUrl = string.Format(Constants.StorageUrl, _request.File);
                IsRequestHasImage = true;
            }

            IsCurrentUserMaster = App.IsUserMaster;

            if(IsCurrentUserMaster)
            {
                IsCloseRequestAvaliable = (_request.Status == "performer appointed" | _request.Status == "appointed");
                IsSelectMasterButtonVisible = false;
            }
            else
            {
                IsCloseRequestAvaliable = true;
                if (_request.Status == "performer appointed" | _request.Status == "appointed")
                {
                    IsSelectMasterButtonVisible = false;
                }
                else
                {
                    IsSelectMasterButtonVisible = true;
                }
            }

			if (_request.IsMasterRequestExists == null)
			{
				return;
			}

			IsMasterSelected = Parse(_request.IsMasterRequestExists);
		}

		public bool CanAcceptRequest => (!IsMasterSelected || _request.Status == "1X customer chose master" && IsMasterSelected) && CurrentUser.Status == "free" && !IsCloseRequestAvaliable;

		public bool CanCancelRequest => IsMasterSelected  & IsCloseRequestAvaliable;

        private async void GetRequestMastersAsync()
        {
            Masters = await _requestStorage.GetRequestMastersAsync(_request.uuid);
            TotalMasters = Masters.Count;
        }

        private async void SendAcceptRequest()
        {
            var response = await _requestStorage.SendAcceptRequest(_request.uuid);

			if (!response)
			{
				return;
			}

			IsMasterSelected = true;
			if (_request.Status != "1X customer chose master")
			{
				return;
			}

			_request.Status = "performer appointed";
			RequestStatus = "Статус: " + _request.StatusText;
            RaisePropertyChanged(nameof(CanAcceptRequest));
            RaisePropertyChanged(nameof(CanCancelRequest));
		}

        private async void SendAcceptMasterRequest(Master selectedMaster)
        {
            if(_request.Status == "appointed" | _request.Status == "performer appointed" | _request.Status == "active")
            {
                await CoreMethods.DisplayAlert("Не выполнено", "У заявки уже назначен исполнитель", "Ок");
                return;
            }

            var answer = await CoreMethods.DisplayAlert("Внимание", "Подтвердить выбор этого мастера?", "Да", "Нет");

			if (!answer)
			{
				return;
			}

			var response = await _requestStorage.SendAcceptMasterRequest(_request.uuid, selectedMaster);

			if (!response)
			{
				return;
			}

			_request.Status = "performer appointed";
			IsMasterSelected = true;
			await CoreMethods.DisplayAlert("Успех", "Мастеру отправлено уведомление о принятие заявки", "Ок");
			_selectedMaster = null;
		}

        private async void SendDeclineRequest()
        {
            var response = await _requestStorage.SendDeclineRequest(_request.uuid);

            if (response)
            {
                IsMasterSelected = false;
            }
        }

        private async void CloseRequest()
        {
            var result = await _requestStorage.CloseRequest(_request.uuid);

			if (!result)
			{
				return;
			}

			await CoreMethods.DisplayAlert("Успех", "Заявка успешно завершена", "Ок");
			_request.IsFinished = "1";
			_request.Status = "closed";
		}
    }
}
