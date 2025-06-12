using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubInterfaceCounter
	{
		#region Fields
		[Column(Order = 0), Key]
		public virtual Guid CT_TS_PK { get; set; }
		[Column(Order = 1), Key]
		public virtual string CT_Name { get; set; }
		public virtual long CT_Value { get; set; }
		public virtual DateTime CT_LastUpdateUTC { get; set; }
		public virtual Nullable<bool> CT_Cleanup { get; set; }

		#endregion

		#region Relationships
		[ForeignKey("CT_TS_PK")]
		public virtual eHubTransformationSet eHubTransformationSet { get; set; }
		#endregion
	}
}
