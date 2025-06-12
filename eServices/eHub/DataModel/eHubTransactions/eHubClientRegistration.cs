using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubClientRegistration
	{
		#region Fields
		[Key]
		public Guid CX_PK { get; set; }
		public Guid CX_CC { get; set; }
		public Guid CX_RT { get; set; }
		public string CX_Code { get; set; }
		public string CX_Qualifier { get; set; }
		public string CX_Attr1 { get; set; }
		public string CX_Password1 { get; set; }
		public byte? CX_Flag1 { get; set; }
		public byte? CX_Flag2 { get; set; }
		[Timestamp]
		public byte[] CX_RV { get; set; }
		[Column(TypeName = "xml")]
		public virtual string CX_ConfigXml { get; set; }
		public DateTime? CX_IssuedUTC { get; set; }
		public DateTime? CX_ExpiryUTC { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("CX_CC")]
		public virtual eHubClient eHubClient { get; set; }
		[ForeignKey("CX_RT")]
		public virtual eHubRegistrationType eHubRegistrationType { get; set; }
		#endregion

		#region DefaultConstructor
		public eHubClientRegistration()
		{
			CX_PK = Guid.NewGuid();
			CX_Code = String.Empty;
		}
		#endregion
	}
}
