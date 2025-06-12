using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.DataAccess.Models.CodeMapsTesting
{
    public class eHubState
    {
        public Guid StatePK { get; set; }
        public string Code { get; set; }

        public eHubState()
        {
        }
    }
}
