using System;
using System.Collections.Generic;
using System.Text;

namespace kraken.Models
{
    public class WorkType
    {
        public string id { get; set; }

        public string work { get; set; }

        public string Name { get { return work; } }
    }
}
