using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubClientSystemRegistration
	{
		[Key]
		public virtual Guid CD_PK { get; set; }
		public virtual Guid CD_RT { get; set; }
		public virtual Guid CD_EH { get; set; }
		public virtual string CD_Qualifier { get; set; }
		public virtual string CD_Code { get; set; }
		public virtual byte? CD_Flag1 { get; set; }
		public virtual String CD_Attr1 { get; set; }
		public virtual String CD_Attr2 { get; set; }
		[Column(TypeName = "xml")]
		public virtual string CD_ConfigXml { get; set; }
		public virtual DateTime? CD_IssuedUTC { get; set; }
		public virtual DateTime? CD_ExpiryUTC { get; set; }
		[Timestamp]
		public byte[] CD_RV { get; set; }

		[ForeignKey("CD_EH")]
		public virtual eHubClientSystem eHubClientSystem { get; set; }

		[ForeignKey("CD_RT")]
		public virtual eHubRegistrationType eHubRegistrationType { get; set; }

		public eHubClientSystemRegistration()
		{
			CD_PK = Guid.NewGuid();
		}
	}
}
