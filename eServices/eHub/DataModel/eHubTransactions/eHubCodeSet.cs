using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubCodeSet
	{
		#region Fields
		[Key]
		public virtual Guid CS_PK { get; set; }
		public virtual string CS_Name { get; set; }
		public virtual Nullable<Guid> CS_TS { get; set; }
		public virtual Guid CS_CC_Sender { get; set; }
		public virtual Guid CS_CC_Recipient { get; set; }
		public virtual string CS_Key1Name { get; set; }
		public virtual string CS_Key2Name { get; set; }
		public virtual string CS_Key3Name { get; set; }
		public virtual string CS_Key4Name { get; set; }
		public virtual string CS_Key5Name { get; set; }
		[StringLength(3)]
		public string CS_OW { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("CS_CC_Sender")]
		public virtual eHubClient eHubClient_Sender { get; set; }
		[ForeignKey("CS_CC_Recipient")]
		public virtual eHubClient eHubClient_Recipient { get; set; }
		[ForeignKey("CS_TS")]
		public virtual eHubTransformationSet eHubTransformationSet { get; set; }
		[ForeignKey("CS_OW")]
		public virtual eHubOwner eHubOwner { get; set; }
		[InverseProperty("eHubCodeSet")]
		public virtual List<eHubCodeMapKey> eHubCodeMapKeys { get; set; }
		[InverseProperty("eHubCodeSet")]
		public virtual List<eHubCodeSetResult> eHubCodeSetResults { get; set; }
		#endregion

		#region Default Constructor
		public eHubCodeSet ()
		{
			eHubCodeMapKeys = new List<eHubCodeMapKey>();
			eHubCodeSetResults = new List<eHubCodeSetResult>();
		} 
		#endregion

	}
}
