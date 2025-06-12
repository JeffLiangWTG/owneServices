using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubMessageType
	{
		#region Fields
		[Key]
		public virtual Guid DT_PK { get; set; }
		public virtual string DT_Code { get; set; }
		public virtual bool DT_IsFlatFile { get; set; }
		public virtual bool DT_IsEDI { get; set; }
		public virtual string DT_Charset { get; set; }
		public virtual string DT_EnvelopeXpath { get; set; }
		public virtual Nullable<Guid> DT_DT_InnerType { get; set; }
		public virtual bool DT_ReprocessSubMessage { get; set; }
		#endregion

		#region Relationships
		[InverseProperty("eHubMessageType")]
		public virtual List<eHubTransformationSet> eHubTransformationSets { get; set; }
		[InverseProperty("eHubMessageType_Source")]
		public virtual List<eHubTransformationType> eHubTransformationTypes_Source { get; set; }
		[InverseProperty("eHubMessageType_Target")]
		public virtual List<eHubTransformationType> eHubTransformationTypes_Target { get; set; }
		[InverseProperty("eHubMessageType")]
		public virtual List<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribes { get; set; }
		[InverseProperty("eHubMessageType")]
		public virtual List<eHubSubscriptionLookup> eHubSubscriptionLookups { get; set; }
		[InverseProperty("eHubMessageType_InnerType")]
		public virtual List<eHubMessageType> eHubMessageType_Envelopes { get; set; }
		[ForeignKey("DT_DT_InnerType")]
		public virtual eHubMessageType eHubMessageType_InnerType { get; set; }
		[InverseProperty("eHubMessageType")]
		public virtual List<eHubInboxXmlContent> eHubInboxXmlContents { get; set; }
		[InverseProperty("eHubMessageType")]
		public virtual List<eHubOutboxMessageDisplay> eHubOutboxMessageDisplays { get; set; }
		[InverseProperty("eHubMessageType")]
		public virtual List<eHubOutboxMessage> eHubOutboxMessages { get; set; }
		[InverseProperty("eHubMessageType")]
		public virtual List<eHubOutboxMessageArchive> eHubOutboxMessageArchives { get; set; }
		#endregion

		#region Default Constructor
		public eHubMessageType()
		{
			eHubTransformationSets = new List<eHubTransformationSet>();
			eHubTransformationTypes_Source = new List<eHubTransformationType>();
			eHubTransformationTypes_Target = new List<eHubTransformationType>();
			eHubSubscriptionAutoSubscribes = new List<eHubSubscriptionAutoSubscribe>();
			eHubSubscriptionLookups = new List<eHubSubscriptionLookup>();
			eHubMessageType_Envelopes = new List<eHubMessageType>();
		}
		#endregion
	}
}
