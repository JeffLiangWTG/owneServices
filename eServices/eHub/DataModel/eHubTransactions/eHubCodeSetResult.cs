using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubCodeSetResult
	{
		#region Fields
		[Key]
		public virtual Guid CR_PK { get; set; }
		public virtual Guid CR_CS { get; set; }
		public virtual int CR_Order { get; set; }
		public virtual string CR_Name { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("CR_CS")]
		public virtual eHubCodeSet eHubCodeSet { get; set; }
		[InverseProperty("eHubCodeSetResult")]
		public virtual List<eHubCodeMapValue> eHubCodeMapValues { get; set; }
		#endregion

		#region Default Constructor
		public eHubCodeSetResult()
		{
			eHubCodeMapValues = new List<eHubCodeMapValue>();
		}
		#endregion
	}
}
