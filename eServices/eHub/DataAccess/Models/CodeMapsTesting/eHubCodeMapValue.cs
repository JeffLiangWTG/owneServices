using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
    public class eHubCodeMapValue
    {
        private eHubCodeMapKey p_eHubCodeMapKey;
        public eHubCodeMapKey eHubCodeMapKey
        {
            get { return p_eHubCodeMapKey; }
            set
            {
                p_eHubCodeMapKey = value;
                value.eHubCodeMapValues.Add(this);
            }
        }

        private eHubCodeSetResult p_eHubCodeSetResult;
        public eHubCodeSetResult eHubCodeSetResult
        {
            get { return p_eHubCodeSetResult; }
            set 
            { 
                p_eHubCodeSetResult = value;
                value.eHubCodeMapValues.Add(this);
            }
        }

        public string CV_OutputCode { get; set; }

        public int? CV_PassThroughKey { get; set; }
    }
}
