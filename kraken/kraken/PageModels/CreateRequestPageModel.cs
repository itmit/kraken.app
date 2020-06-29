using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class CreateRequestPageModel : FreshBasePageModel
    {
        readonly IRequestStorageService _requestStorage;
        private Plugin.FilePicker.Abstractions.FileData FileData;
        private Urgency selectedUrgency;

        public ObservableCollection<WorkType> WorkTypes { get; set; }
        public List<Urgency> Urgency { get; set; }

        public string UserImage { get; set; }

        public bool IsFileUploaded { get; set; } = false;

        public WorkType SelectedType { get; set; }
        public Urgency SelectedUrgency 
        { 
            get => selectedUrgency;
            set
            {
                if (value.Code == "scheduled")
                    IsTimeVisible = true;
                else
                    IsTimeVisible = false;

                selectedUrgency = value;
            }
        }
        public string Description { get; set; }
        public string SelectedTime { get; set; }

        public ICommand CreateRequestCommand
        {
            get
            {
                return new FreshAwaitCommand(async (param, tcs) =>
                {
                    bool isRequestSuccesful = await CreateRequest();
                    if (isRequestSuccesful)
                    {
                        SelectedType = null;
                        selectedUrgency = null;
                        SelectedTime = null;
                        UserImage = null;
                        Description = String.Empty;
                        IsFileUploaded = false;
                        await CoreMethods.SwitchSelectedTab<MyRequestPageModel>();
                    }
                    tcs.SetResult(true);
                });
            }
        }

        public ICommand OpenFileManagerCommand
        {
            get
            {
                return new Xamarin.Forms.Command(async (param) =>
                {
                    await OpenFileManager();
                });
            }
        }

        public bool IsTimeVisible { get; private set; }

        public CreateRequestPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);


            if (WorkTypes == null)
            {
                var typeList = await GetWorkTypes();
                WorkTypes = new ObservableCollection<WorkType>(typeList);
            }

            if (Urgency == null)
            {
                GetUrgencyTypes();
            }
        }

        private async Task<List<WorkType>> GetWorkTypes()
        {
            List<WorkType> Types;

            Types = await _requestStorage.GetWorkTypesAsync();

            return Types;
        }

        private void GetUrgencyTypes()
        {
            Urgency = new List<Urgency>
            {
                new Urgency()
                {
                    Id = 0,
                    Code = "urgent",
                    Name = "Срочно"
                },
                new Urgency()
                {
                    Id = 0,
                    Code = "now",
                    Name = "Сейчас"
                },
                new Urgency()
                {
                    Id = 0,
                    Code = "scheduled",
                    Name = "Заданное время"
                },
            };
        }

        private async Task<bool> CreateRequest()
        {
            bool isValid = ValidateInputData();

            if (isValid == false)
            {
                await CoreMethods.DisplayAlert("Ошибка", "Неуказаны род работ, срочность или описание", "Ok");
                return false;
            }

            Request NewRequest = new Request()
            {
                Work = SelectedType.id,
                Urgency = SelectedUrgency.Code,
                Description = Description,
                StartedAt = "",
            };

            var response = await _requestStorage.SendNewRequestAsync(NewRequest, FileData);

            return response;
        }

        private async Task OpenFileManager()
        {
            try
            {
                Plugin.FilePicker.Abstractions.FileData fileData = await Plugin.FilePicker.CrossFilePicker.Current.PickFile();
                if (fileData == null)
                    return; // user canceled file picking

                FileData = fileData;
                IsFileUploaded = true;

                UserImage = fileData.FilePath;

                //UserFiles.Add(fileData.FileName);
                //UserFilesBase64.Add("data:image/png;base64," + Convert.ToBase64String(fileData.DataArray));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception choosing file: " + ex.ToString());
            }
        }

        private bool ValidateInputData()
        {
            if (SelectedType != null & SelectedUrgency != null & Description != null)
            {
                return true;
            }

            return false;
        }
    }
}
