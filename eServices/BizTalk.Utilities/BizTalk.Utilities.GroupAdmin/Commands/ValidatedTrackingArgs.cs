using System.Collections.Generic;

namespace BizTalk.Utilities.GroupAdmin.Commands
{
    internal struct ValidatedTrackingArgs
    {
        internal bool Valid;
        internal string NameMask;
        internal List<string> Options;
    }
}
