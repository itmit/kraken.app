using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using Realms;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class RequestDetailPageModel : FreshBasePageModel
    {
        readonly IRequestStorageService _requestStorage;
        Request Request;
        private Master _selectedMaster;

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
                    SendAcceptMasterRequest(value);
            }
        }

        public bool IsMasterSelected { get; private set; }
        public bool IsCurrentUserMaster { get; private set; }
        public bool IsCloseRequestAvaliable { get; private set; }
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

                    await CoreMethods.PushPageModel<MasterSelectPageModel>(Request);
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
            Request = (Request)initData;
            CurrentPage.Title = Request.Work;

            FillRequestValues();
            GetRequestMastersAsync();
        }

        private void FillRequestValues()
        {
            RequestName = Request.Work;
            RequestDescription = Request.Description;
            RequestStatus = "Статус: " + Request.StatusText;
            MasterDistance = Request.MasterDistance + " км";
            RequestAddress = Request.Address;
            RequestPhone = Request.Phone;
            RequestClientName = Request.Name;

            if(Request.File == null)
            {
                IsRequestHasImage = false;
            }
            else
            {
                RequestFileUrl = string.Format(Constants.StorageUrl, Request.File);
                IsRequestHasImage = true;
            }

            IsCurrentUserMaster = App.IsUserMaster;

            if(IsCurrentUserMaster)
            {
                IsCloseRequestAvaliable = (Request.Status == "performer appointed" | Request.Status == "appointed");
                IsSelectMasterButtonVisible = false;
            }
            else
            {
                IsCloseRequestAvaliable = true;
                if (Request.Status == "performer appointed" | Request.Status == "appointed")
                {
                    IsSelectMasterButtonVisible = false;
                }
                else
                {
                    IsSelectMasterButtonVisible = true;
                }
            }

            if(Request.IsMasterRequestExists != null)
            {
                var isRequestExists = System.Boolean.Parse(Request.IsMasterRequestExists);
                if(isRequestExists == true & IsCloseRequestAvaliable == false)
                {
                    IsMasterSelected = false;
                }
                else
                {
                    IsMasterSelected = isRequestExists;
                }
            }

        }

        private async void GetRequestMastersAsync()
        {
            Masters = await _requestStorage.GetRequestMastersAsync(Request.uuid);
            TotalMasters = Masters.Count;
        }

        private async void SendAcceptRequest()
        {
            bool response = await _requestStorage.SendAcceptRequest(Request.uuid);

            if (response == true)
            {
                IsMasterSelected = true;
            }
        }

        private async void SendAcceptMasterRequest(Master selectedMaster)
        {
            if(Request.Status == "appointed" | Request.Status == "performer appointed" | Request.Status == "active")
            {
                await CoreMethods.DisplayAlert("Не выполнено", "У заявки уже назначен исполнитель", "Ок");
                return;
            }

            bool answer = await CoreMethods.DisplayAlert("Внимание", "Подтвердить выбор этого мастера?", "Да", "Нет");

            if (answer == true)
            {
                bool response = await _requestStorage.SendAcceptMasterRequest(Request.uuid, selectedMaster);

                if (response == true)
                {
                    Request.Status = "performer appointed";
                    IsMasterSelected = true;
                    await CoreMethods.DisplayAlert("Успех", "Мастеру отправлено уведомление о принятие заявки", "Ок");
                    _selectedMaster = null;
                }
            }
        }

        private async void SendDeclineRequest()
        {
            bool response = await _requestStorage.SendDeclineRequest(Request.uuid);

            if (response == true)
            {
                IsMasterSelected = false;
            }
        }

        private async void CloseRequest()
        {
            bool result = await _requestStorage.CloseRequest(Request.uuid);

            if (result == true)
            {
                await CoreMethods.DisplayAlert("Успех", "Заявка успешно завершена", "Ок");
                Request.IsFinished = "1";
                Request.Status = "closed";
            }
        }
    }
}
