using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
    public class eHubCodeSet
    {
        public string CS_Name { get; set; }

        private eHubTransformationSet p_TransformationSet;
        public eHubTransformationSet eHubTransformationSet
        {
            get { return p_TransformationSet; }
            set
            {
                p_TransformationSet = value;
                value.eHubCodeSets.Add(this);
            }
        }

        private eHubClient p_eHubClient_Sender;
        public eHubClient eHubClient_Sender
        {
            get { return p_eHubClient_Sender; }
            set
            {
                p_eHubClient_Sender = value;
                value.eHubCodeSets_Sender.Add(this);
            }
        }

        private eHubClient p_eHubClient_Recipient;
        public eHubClient eHubClient_Recipient
        {
            get { return p_eHubClient_Recipient; }
            set
            {
                p_eHubClient_Recipient = value;
                value.eHubCodeSets_Recipient.Add(this);
            }
        }

        public readonly List<eHubCodeMapKey> eHubCodeMapKeys;

        public readonly List<eHubCodeSetResult> eHubCodeSetResults;

        public eHubCodeSet()
        {
            eHubCodeMapKeys = new List<eHubCodeMapKey>();
            eHubCodeSetResults = new List<eHubCodeSetResult>();
        }
    }
}
