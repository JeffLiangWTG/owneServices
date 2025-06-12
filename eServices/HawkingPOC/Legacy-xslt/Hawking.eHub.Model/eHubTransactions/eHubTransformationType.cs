using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubTransformationType
    {
        public eHubTransformationType()
        {
            eHubTransformationMapping = new HashSet<eHubTransformationMapping>();
        }

        public Guid TT_PK { get; set; }
        public Guid TT_DT_Source { get; set; }
        public Guid TT_DT_Target { get; set; }
        public string TT_TransformationType { get; set; }
        public byte? TT_Target_Version { get; set; }

        public eHubMessageType TT_DT_SourceNavigation { get; set; }
        public eHubMessageType TT_DT_TargetNavigation { get; set; }
        public ICollection<eHubTransformationMapping> eHubTransformationMapping { get; set; }
    }
}
