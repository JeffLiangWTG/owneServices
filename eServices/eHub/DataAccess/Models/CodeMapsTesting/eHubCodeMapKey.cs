using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
    public class eHubCodeMapKey
    {
        public int CK_Order { get; set; }
        public string CK_Key1Value { get; set; }
        public string CK_Key2Value { get; set; }
        public string CK_Key3Value { get; set; }
        public string CK_Key4Value { get; set; }
        public string CK_Key5Value { get; set; }
        public readonly List<eHubCodeMapValue> eHubCodeMapValues;

        private eHubCodeSet p_eHubCodeSet;
        public eHubCodeSet eHubCodeSet
        {
            get { return p_eHubCodeSet; }
            set
            {
                p_eHubCodeSet = value;
                value.eHubCodeMapKeys.Add(this);
            }
        }


        public eHubCodeMapKey()
        {
            eHubCodeMapValues = new List<eHubCodeMapValue>();
        }
    }
}
