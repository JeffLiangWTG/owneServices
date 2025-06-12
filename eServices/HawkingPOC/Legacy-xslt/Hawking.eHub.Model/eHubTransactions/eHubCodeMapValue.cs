using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubCodeMapValue
    {
        public Guid CV_CK { get; set; }
        public Guid CV_CR { get; set; }
        public string CV_OutputCode { get; set; }
        public int? CV_PassThroughKey { get; set; }

        public eHubCodeMapKey CV_CKNavigation { get; set; }
        public eHubCodeSetResult CV_CRNavigation { get; set; }
    }
}
