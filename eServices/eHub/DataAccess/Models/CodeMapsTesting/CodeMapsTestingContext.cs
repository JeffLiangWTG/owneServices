using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
    public class CodeMapsTestingContext
    {
        public readonly List<eHubClient> eHubClients;
        public readonly List<eHubTransformationSet> eHubTransformationSets;
        public readonly List<eHubCodeSet> eHubCodeSets;
        public readonly List<eHubCodeMapKey> eHubCodeMapKeys;
        public readonly List<eHubCodeSetResult> eHubCodeSetResults;
        public readonly List<eHubCodeMapValue> eHubCodeMapValues;
        public readonly List<eHubUNLOCO> eHubUNLOCOList;
        public readonly List<eHubState> eHubStateList;
        public readonly List<ActionProcedure> ActionProcedures;

        public CodeMapsTestingContext()
        {
            eHubClients = new List<eHubClient>();
            eHubTransformationSets = new List<eHubTransformationSet>();
            eHubCodeSets = new List<eHubCodeSet>();
            eHubCodeMapKeys = new List<eHubCodeMapKey>();
            eHubCodeMapValues = new List<eHubCodeMapValue>();
            eHubCodeSetResults = new List<eHubCodeSetResult>();
            eHubCodeMapValues = new List<eHubCodeMapValue>();
            eHubUNLOCOList = new List<eHubUNLOCO>();
            eHubStateList = new List<eHubState>();
            ActionProcedures = new List<ActionProcedure>();
        }
    }
}
