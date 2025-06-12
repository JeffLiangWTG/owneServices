using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubUSCustomsRegistry
	{
		#region Fields

		[Key]
		public virtual Guid ER_PK { get; set; }
		public virtual Guid? ER_CC_Client { get; set; }
		[MaxLength(6)]
		public virtual string ER_ApplicationCode { get; set; }
		[MaxLength(36)]
		public virtual string ER_Name { get; set; }
		[MaxLength(160)]
		public virtual string ER_Value { get; set; }
		public virtual bool? ER_IsProduction { get; set; }

		#endregion

		#region Relationships

		[ForeignKey("ER_CC_Client")] 
		public virtual eHubClient eHubClient { get; set; }

		#endregion

		#region DefaultConstructor

		public eHubUSCustomsRegistry()
		{
			ER_PK = Guid.NewGuid();
		}

		#endregion
	}
}
