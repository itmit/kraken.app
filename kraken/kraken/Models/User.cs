﻿using PropertyChanged;
using Realms;

namespace kraken.Models
{
    [AddINotifyPropertyChangedInterface]
    public class User : RealmObject
    {
        public string MasterId { get; set; }

        public string Name { get; set; }

        public string Organization { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Password { get; set; }

        public string DeviceToken { get; set; }

        public string ClientType { get; set; }

		public string Token
		{
			get; 
			internal set;
		}

        public string Distance
        {
			get;
			set;
		}

		public string DrivingMode
		{
			get;
			set;
		}

		public string Status
		{
			get;
			set;
		}
	}
}
