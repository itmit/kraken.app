using FreshMvvm;
using PropertyChanged;
using kraken.Models;
using Realms;
using System.Linq;
using System.Windows.Input;
using kraken.Services;
using System.Collections.Generic;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class MyProfilePageModel : FreshBasePageModel
    {
        readonly IUserService _userService;

        public Realm Realm { get { return Realm.GetInstance(); } }
        public User CurrentUser { get; set; }
        public bool IsUserMaster { get; set; } = false;

        public string UserName { get; set; }
        public string UserOrganization { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string UserAddress { get; set; }

        public List<KeyValuePair<string, string>> DistanceList { get; set; }
        public List<KeyValuePair<string, string>> DrivingList { get; set; }
        public List<KeyValuePair<string, string>> StatusList { get; set; }

        private KeyValuePair<string, string> _selectedDistance;
        private KeyValuePair<string, string> _drivingMode;
        private KeyValuePair<string, string> _selectedStatus;

        public KeyValuePair<string, string> SelectedDistance
        {
            get { return _selectedDistance; }
            set
            {
                _selectedDistance = value;
                if (_selectedDistance.Key != null)
                    ChangeRange(_selectedDistance.Key);
            }
        }

        public KeyValuePair<string, string> SelectedVehicle
        {
            get { return _drivingMode; }
            set
            {
                _drivingMode = value;
                if (_drivingMode.Key != null)
                    ChangeDrivingMode(_drivingMode.Key);
            }
        }

        public KeyValuePair<string, string> SelectedStatus
        {
            get { return _selectedStatus; }
            set
            {
                _selectedStatus = value;
                if (_selectedStatus.Key != null)
                    ChangeStatus(_selectedStatus.Key);
            }
        }

        private void ChangeRange(string value)
        {
            _userService.SetSearchFilterAsync(value);
        }

        private void ChangeDrivingMode(string value)
        {
            _userService.SetDrivingModeAsync(value);
        }

        private void ChangeStatus(string value)
        {
            _userService.SetStatusAsync(value);
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

        async void OpenRequestsPage()
        {
            await CoreMethods.SwitchSelectedTab<MyRequestPageModel>();
        }

        public MyProfilePageModel(IUserService userService)
        {
            _userService = userService;

            UserName = "";
            UserOrganization = "";
            UserEmail = "";
            UserPhone = "";
            UserAddress = "";
        }

        protected override void ViewIsAppearing(object sender, System.EventArgs e)
        {
            LoadUserInfo();
            IsUserMaster = App.IsUserMaster;
            base.ViewIsAppearing(sender, e);
        }

        public override void Init(object initData)
        {
            base.Init(initData);

            LoadUserInfo();
            SetUpDictionaries();
        }

        private void LoadUserInfo()
        {
            IQueryable<User> users = Realm.All<User>();
            if (users.Count() <= 0)
                return;

            CurrentUser = users.Last();

            UserName = CurrentUser.Name;
            UserOrganization = CurrentUser.Organization;
            UserEmail = CurrentUser.Email;
            UserPhone = CurrentUser.Phone;
            UserAddress = CurrentUser.Address;
        }

        private void SetUpDictionaries()
        {
            Dictionary<string, string> DistanceItems = new Dictionary<string, string>() {
                { "0", "Ближайшие" }, 
                { "1", "1км" },
                { "5", "5км" },
                { "10", "10км" } 
            };

            Dictionary<string, string> DrivingModeItems = new Dictionary<string, string>() {
                { "driving", "на автомобиле" },
                { "walking", "пешком" }
            };

            Dictionary<string, string> StatusItems = new Dictionary<string, string>() {
                { "free", "свободен" },
                { "busy", "занят" },
                { "offline", "оффлайн" }
            };

            DistanceList = DistanceItems.ToList();
            DrivingList = DrivingModeItems.ToList();
            StatusList = StatusItems.ToList();
        }
    }
}
