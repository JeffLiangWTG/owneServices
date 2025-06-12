using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{

	public partial class eHubOwner
    {
        [Key]
        [MaxLength(3)]
        public string OW_ID { get; set; }

        [MaxLength(250)]
        public string OW_Name { get; set; }

        public string OW_Description { get; set; }

        [MaxLength(250)]
        public string OW_AccessGroup_ReadWrite { get; set; }

        [MaxLength(250)]
        public string OW_AccessGroup_ReadOnly { get; set; }

        [InverseProperty("eHubOwner")]
        public virtual List<eHubClient> eHubClients { get; set; } = new List<eHubClient>();

        [InverseProperty("eHubOwner")]
        public virtual List<eHubCodeSet> eHubCodeSets { get; set; } = new List<eHubCodeSet>();

        [InverseProperty("eHubOwner")]
        public virtual List<eHubRegistrationType> eHubRegistrationTypes { get; set; } = new List<eHubRegistrationType>();
    }
}
