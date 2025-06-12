using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
    public class eHubClient
    {
        public string CC_ID { get; set; }
        public readonly List<eHubTransformationSet> eHubTransformationSets_Sender;
        public readonly List<eHubTransformationSet> eHubTransformationSets_Recipient;
        public readonly List<eHubCodeSet> eHubCodeSets_Sender;
        public readonly List<eHubCodeSet> eHubCodeSets_Recipient;

        public eHubClient()
        {
            eHubTransformationSets_Sender = new List<eHubTransformationSet>();
            eHubTransformationSets_Recipient = new List<eHubTransformationSet>();
            eHubCodeSets_Sender = new List<eHubCodeSet>();
            eHubCodeSets_Recipient = new List<eHubCodeSet>();
        }
    }
}
