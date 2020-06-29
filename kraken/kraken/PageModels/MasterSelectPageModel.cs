using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using System.Collections.Generic;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class MasterSelectPageModel : FreshBasePageModel
    {
        readonly IRequestStorageService _requestStorage;
        private Master _selectedMaster;

        public MasterSelectPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        public Request Request { get; private set; }
        public List<Master> Masters { get; private set; }

        public Master SelectedMaster
        {
            get { return _selectedMaster; }
            set
            {
                _selectedMaster = value;
                if (value != null)
                    SelectMaster(value);
            }
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            Request = (Request)initData;
            CurrentPage.Title = Request.Work;

            GetMastersAsync();
        }

        private async void GetMastersAsync()
        {
            Masters = await _requestStorage.GetMastersAsync(Request.uuid);
        }

        private async void SelectMaster(Master selectedMaster)
        {
            bool answer = await CoreMethods.DisplayAlert("Внимание", "Подтвердить выбор этого мастера?", "Да", "Нет");

            if (answer == true)
            {
                bool response = await _requestStorage.SelectMaster(Request.uuid, selectedMaster);

                if (response == true)
                {
                    await CoreMethods.DisplayAlert("Успех", "Мастеру отправлено уведомление о принятие заявки", "Ок");
                }
            }
        }
    }
}
