using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubTransformationMapping
	{
		#region Fields
		[Key, Column(Order = 1)]
		public virtual Guid TM_TS_PK { get; set; }
		[Key, Column(Order = 2)]
		public virtual byte TM_Order { get; set; }
		[Key, Column(Order = 3)]
		public virtual Guid TM_TT_PK { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("TM_TS_PK")]
		public virtual eHubTransformationSet eHubTransformationSet { get; set; }
		[ForeignKey("TM_TT_PK")]
		public virtual eHubTransformationType eHubTransformationType { get; set; }
		#endregion

		#region Default Constructor
		public eHubTransformationMapping()
		{

		}
		#endregion
	}
}
