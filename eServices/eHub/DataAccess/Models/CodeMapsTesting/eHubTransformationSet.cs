using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
    public class eHubTransformationSet
    {
        public string TS_Name { get; set; }

        public readonly List<eHubCodeSet> eHubCodeSets;

        private eHubClient p_eHubClient_Sender;
        public eHubClient eHubClient_Sender
        {
            get { return p_eHubClient_Sender; }
            set 
            { 
                p_eHubClient_Sender = value;
                value.eHubTransformationSets_Sender.Add(this);
            }
        }

        private eHubClient p_eHubClient_Recipient;
        public eHubClient eHubClient_Recipient
        {
            get { return p_eHubClient_Recipient; }
            set 
            { 
                p_eHubClient_Recipient = value;
                value.eHubTransformationSets_Recipient.Add(this);
            }
        }

        public eHubTransformationSet()
        {
            eHubCodeSets = new List<eHubCodeSet>();
        }
    }
}
