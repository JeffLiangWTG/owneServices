using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubITCustomsJobStatus
	{
		[Key]
		public virtual Guid IT_PK { get; set; }
		public virtual Guid IT_EH_ClientSystem { get; set; }
		public virtual bool IT_ProdInd { get; set; }
		[MaxLength(250)]
		public virtual string IT_FileName { get; set; }
		[MaxLength(250)]
		public virtual string IT_MessageType { get; set; }
		public virtual DateTime IT_PollingStartUTC{ get; set; }
		public virtual DateTime? IT_FileLastModifiedUTC{ get; set; }
		[MaxLength(250)]
		public virtual string IT_JobID{ get; set; }
		[MaxLength(3)]
		public virtual string IT_LastStatus { get; set; }
		public virtual Guid IT_CC_Sender{ get; set; }
		public virtual Guid IT_MessageTrackingID{ get; set; }
		[MaxLength(250)]
		public virtual string IT_ReferenceID{ get; set; }
		public virtual bool? IT_NotifiedInvalidProfile{ get; set; }
		[Timestamp]
		public byte[] IT_Version { get; set; }
		public string IT_DeclarationContent { get; set; }

		[ForeignKey("IT_EH_ClientSystem")]
		public virtual eHubClientSystem eHubClientSystem { get; set; }

		[ForeignKey("IT_CC_Sender")]
		public virtual eHubClient eHubClient { get; set; }

		public eHubITCustomsJobStatus()
		{
			IT_PK = Guid.NewGuid();
		}
	}
}
