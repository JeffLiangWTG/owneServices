using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace CargoWise.eHub.DataModel.eHubTransactions
{
	public partial class eHubClient
	{
		#region Fields
		[Key]
		public Guid CC_PK { get; set; }
		public string CC_ID { get; set; }
		public string CC_FriendlyName { get; set; }
		public Guid CC_Odyssey_OH { get; set; }
		public Nullable<Guid> CC_DistributionZone { get; set; }
		public string CC_EmailAddress { get; set; }
		public string CC_Password { get; set; }
		public string CC_OwnerCategory { get; set; }
		public string CC_SystemCategory { get; set; }
		public Nullable<Guid> CC_RR { get; set; }

		[JsonIgnore]
		public Boolean CC_PermitInboxSender { get; set; }
		[JsonIgnore]
		public Boolean CC_PermitInboxRecipient { get; set; }

		[StringLength(3)]
		public string CC_OW { get; set; }
		#endregion

		#region Relationships
		[ForeignKey("CC_DistributionZone")]
		public virtual eHubZone eHubZone { get; set; }
		[ForeignKey("CC_RR")]
		public virtual eHubRoutingRule eHubRoutingRule { get; set; }
		[ForeignKey("CC_OW")]
		public virtual eHubOwner eHubOwner { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubRoutingRule> eHubRoutingRules_Recipient { get; set; }
		[InverseProperty("eHubClient_Service")]
		public virtual List<eHubServiceProvider> eHubServiceProviders_Service { get; set; }
		[InverseProperty("eHubClient_Provider")]
		public virtual List<eHubServiceProvider> eHubServiceProviders_Provider { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubCodeSet> eHubCodeSets_Sender { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubCodeSet> eHubCodeSets_Provider { get; set; }
		[InverseProperty("eHubClient_Provider")]
		public virtual List<eHubSubscriptionValue> eHubSubscriptionValues_Sender { get; set; }
		[InverseProperty("eHubClient_Subscriber")]
		public virtual List<eHubSubscriptionValue> eHubSubscriptionValues_Subscriber { get; set; }
		[InverseProperty("eHubClient")]
		public virtual List<eHubClientRegistration> eHubClientRegistrations { get; set; }
		[InverseProperty("eHubClient")]
		public virtual List<eHubAsyncPollingRegistration> eHubAsyncPollingRegistrations { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubInboxMessageDisplay> eHubInboxMessageDisplay_Senders { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubInboxMessageDisplay> eHubInboxMessageDisplay_Recipients { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubOutboxMessageDisplay> eHubOutboxMessageDisplay_Senders { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubOutboxMessageDisplay> eHubOutboxMessageDisplay_Recipients { get; set; }
		[InverseProperty("eHubClient")]
		public virtual List<eHubITCustomsJobStatus> eHubITCustomsJobStatuses { get; set; }
		[InverseProperty("eHubClient")]
		public virtual List<eHubCertificate> eHubCertificates { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubInboxMessage> eHubInboxMessage_Senders { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubInboxMessage> eHubInboxMessage_Recipients { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubOutboxMessage> eHubOutboxMessage_Senders { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubOutboxMessage> eHubOutboxMessage_Recipients { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubInboxMessageArchive> eHubInboxMessageArchive_Senders { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubInboxMessageArchive> eHubInboxMessageArchive_Recipients { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubOutboxMessageArchive> eHubOutboxMessageArchive_Senders { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubOutboxMessageArchive> eHubOutboxMessageArchive_Recipients { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribes { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcasters_Senders { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcasters_Recipients { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubTransformationSet> eHubTransformationSet_Senders { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubTransformationSet> eHubTransformationSet_Recipients { get; set; }
		[InverseProperty("eHubClient_BillOther")]
		public virtual List<eHubTransformationSet> eHubTransformationSet_BillOthers { get; set; }
		[InverseProperty("eHubClient_Sender")]
		public virtual List<eHubClientAuthorisation> eHubClientAuthorisation_Senders { get; set; }
		[InverseProperty("eHubClient_Recipient")]
		public virtual List<eHubClientAuthorisation> eHubClientAuthorisation_Recipients { get; set; }
		public virtual List<eHubMonitor> eHubMonitors { get; set; }
		[InverseProperty("eHubClient")]
		public virtual List<eHubUSCustomsRegistry> eHubUSCustomsRegistry_Clients { get; set; }

		#endregion

		#region Default Constructor
		public eHubClient()
		{
			CC_PK = Guid.NewGuid();
			CC_FriendlyName = String.Empty;
			CC_Odyssey_OH = Guid.Empty;
			CC_EmailAddress = String.Empty;
			CC_Password = String.Empty;
			eHubRoutingRules_Recipient = new List<eHubRoutingRule>();
			eHubServiceProviders_Service = new List<eHubServiceProvider>();
			eHubServiceProviders_Provider = new List<eHubServiceProvider>();
			eHubClientRegistrations = new List<eHubClientRegistration>();
			eHubCodeSets_Sender = new List<eHubCodeSet>();
			eHubCodeSets_Provider = new List<eHubCodeSet>();
			eHubSubscriptionValues_Sender = new List<eHubSubscriptionValue>();
			eHubSubscriptionValues_Subscriber = new List<eHubSubscriptionValue>();
		}
		#endregion
	}
}
