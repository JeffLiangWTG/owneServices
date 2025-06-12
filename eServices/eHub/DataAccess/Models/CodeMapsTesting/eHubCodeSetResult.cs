using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
    public class eHubCodeSetResult
    {
        public int CR_Order { get; set; }

        public string CR_Name { get; set; }

        private eHubCodeSet p_eHubCodeSet;
        public eHubCodeSet eHubCodeSet
        {
            get { return p_eHubCodeSet; }
            set
            {
                p_eHubCodeSet = value;
                value.eHubCodeSetResults.Add(this);
            }
        }

        public readonly List<eHubCodeMapValue> eHubCodeMapValues;

        public eHubCodeSetResult()
        {
            eHubCodeMapValues = new List<eHubCodeMapValue>();
        }
    }
}
