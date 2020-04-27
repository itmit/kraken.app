using FreshMvvm;
using kraken.Models;
using kraken.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace kraken.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class CreateRequestPageModel : FreshBasePageModel
    {
        readonly IRequestStorageService _requestStorage;
        private byte[] byteImage;

        public List<WorkType> WorkTypes { get; set; } = new List<WorkType>();
        public List<Urgency> Urgency { get; set; } = new List<Urgency>();

        public WorkType SelectedType { get; set; }
        public Urgency SelectedUrgency { get; set; }
        public string Description { get; set; }

        public List<string> UserFiles { get; set; } = new List<string>();
        public string UserImage { get; set; }

        public ICommand CreateRequestCommand
        {
            get
            {
                return new FreshAwaitCommand(async (param, tcs) =>
                {
                    bool isRequestSuccesful = await CreateRequest();
                    if (isRequestSuccesful)
                    {
                        await CoreMethods.PushPageModel<MyRequestPageModel>();
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

        public CreateRequestPageModel(IRequestStorageService requestStorage)
        {
            _requestStorage = requestStorage;
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            WorkTypes = await GetWorkTypes();
            GetUrgencyTypes();
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
            Request NewRequest = new Request()
            {
                Work = SelectedType.id,
                Urgency = SelectedUrgency.Code,
                Description = Description,
                Address = "address",
                StartedAt = "",
            };

            var response = await _requestStorage.SendNewRequestAsync(NewRequest);

            return response;
        }

        private async Task OpenFileManager()
        {
            try
            {
                Plugin.FilePicker.Abstractions.FileData fileData = await Plugin.FilePicker.CrossFilePicker.Current.PickFile();
                if (fileData == null)
                    return; // user canceled file picking
                
                UserFiles.Add(fileData.FileName);

                byteImage = fileData.DataArray;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception choosing file: " + ex.ToString());
            }
        }
    }
}
