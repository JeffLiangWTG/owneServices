using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubClient
    {
        public eHubClient()
        {
            InverseCC_AirServiceProviderNavigation = new HashSet<eHubClient>();
            eHubAirConnectionAC_CC_AirServiceProviderNavigation = new HashSet<eHubAirConnection>();
            eHubAirConnectionAC_CC_ClientNavigation = new HashSet<eHubAirConnection>();
            eHubAirDefaultServiceProviderAD_CC_AirServiceProviderNavigation = new HashSet<eHubAirDefaultServiceProvider>();
            eHubAirDefaultServiceProviderAD_CC_ClientNavigation = new HashSet<eHubAirDefaultServiceProvider>();
            eHubAirReferenceNumberAN_CC_AirServiceProviderNavigation = new HashSet<eHubAirReferenceNumber>();
            eHubAirReferenceNumberAN_CC_SenderNavigation = new HashSet<eHubAirReferenceNumber>();
            eHubAirServiceProviderMappingAM_CC_AirServiceProviderNavigation = new HashSet<eHubAirServiceProviderMapping>();
            eHubAirServiceProviderMappingAM_CC_AirlineNavigation = new HashSet<eHubAirServiceProviderMapping>();
            eHubAirServiceProviderMappingAM_CC_ClientNavigation = new HashSet<eHubAirServiceProviderMapping>();
            eHubAlertEA_CC_RecipientNavigation = new HashSet<eHubAlert>();
            eHubAlertEA_CC_SenderNavigation = new HashSet<eHubAlert>();
            eHubCertificate = new HashSet<eHubCertificate>();
            eHubClientAccessRestriction = new HashSet<eHubClientAccessRestriction>();
            eHubClientCode = new HashSet<eHubClientCode>();
            eHubClientRegistration = new HashSet<eHubClientRegistration>();
            eHubCodeSetCS_CC_RecipientNavigation = new HashSet<eHubCodeSet>();
            eHubCodeSetCS_CC_SenderNavigation = new HashSet<eHubCodeSet>();
            eHubITCustomsJobStatus = new HashSet<eHubITCustomsJobStatus>();
            eHubInboxMessageEI_CC_RecipientNavigation = new HashSet<eHubInboxMessage>();
            eHubInboxMessageEI_CC_SenderNavigation = new HashSet<eHubInboxMessage>();
            eHubMessageCopyRegistryMC_CC_OriginalMessage_Copy_RecipientNavigation = new HashSet<eHubMessageCopyRegistry>();
            eHubMessageCopyRegistryMC_CC_RecipientNavigation = new HashSet<eHubMessageCopyRegistry>();
            eHubMessageCopyRegistryMC_CC_SenderNavigation = new HashSet<eHubMessageCopyRegistry>();
            eHubMessageReferenceRegistry = new HashSet<eHubMessageReferenceRegistry>();
            eHubOutboxMessageOI_CC_RecipientNavigation = new HashSet<eHubOutboxMessage>();
            eHubOutboxMessageOI_CC_SenderNavigation = new HashSet<eHubOutboxMessage>();
            eHubReferenceFileQueryRF_CC_ProviderNavigation = new HashSet<eHubReferenceFileQuery>();
            eHubReferenceFileQueryRF_CC_RequestorNavigation = new HashSet<eHubReferenceFileQuery>();
            eHubRoutingRule = new HashSet<eHubRoutingRule>();
            eHubServiceProviderSP_CC_ProviderNavigation = new HashSet<eHubServiceProvider>();
            eHubServiceProviderSP_CC_ServiceNavigation = new HashSet<eHubServiceProvider>();
            eHubSubscriptionBroadcaster = new HashSet<eHubSubscriptionBroadcaster>();
            eHubSubscriptionValueSV_CC_RecipientNavigation = new HashSet<eHubSubscriptionValue>();
            eHubSubscriptionValueSV_CC_SenderNavigation = new HashSet<eHubSubscriptionValue>();
            eHubTransformationSetTS_CC_BillOtherNavigation = new HashSet<eHubTransformationSet>();
            eHubTransformationSetTS_CC_RecipientNavigation = new HashSet<eHubTransformationSet>();
            eHubTransformationSetTS_CC_SenderNavigation = new HashSet<eHubTransformationSet>();
            eHubUSCustomsRegistry = new HashSet<eHubUSCustomsRegistry>();
        }

        public Guid CC_PK { get; set; }
        public string CC_ID { get; set; }
        public string CC_FriendlyName { get; set; }
        public Guid CC_Odyssey_OH { get; set; }
        public Guid? CC_DistributionZone { get; set; }
        public string CC_EmailAddress { get; set; }
        public string CC_Password { get; set; }
        public bool? CC_IsAirServiceProvider { get; set; }
        public string CC_AirlineCode { get; set; }
        public Guid? CC_AirServiceProvider { get; set; }
        public string CC_AirlinePrefix { get; set; }
        public bool? CC_USCustomsRecipient { get; set; }
        public string CC_AS2_Code { get; set; }
        public string CC_SCAC_Code { get; set; }
        public string CC_OwnerCategory { get; set; }
        public string CC_SystemCategory { get; set; }
        public Guid? CC_RR { get; set; }
        public bool CC_RequireStatusResponse { get; set; }
        public bool CC_NotificationForInboxRecipient { get; set; }

        public eHubClient CC_AirServiceProviderNavigation { get; set; }
        public eHubRoutingRule CC_RRNavigation { get; set; }
        public eHubClientBatching eHubClientBatching { get; set; }
        public ICollection<eHubClient> InverseCC_AirServiceProviderNavigation { get; set; }
        public ICollection<eHubAirConnection> eHubAirConnectionAC_CC_AirServiceProviderNavigation { get; set; }
        public ICollection<eHubAirConnection> eHubAirConnectionAC_CC_ClientNavigation { get; set; }
        public ICollection<eHubAirDefaultServiceProvider> eHubAirDefaultServiceProviderAD_CC_AirServiceProviderNavigation { get; set; }
        public ICollection<eHubAirDefaultServiceProvider> eHubAirDefaultServiceProviderAD_CC_ClientNavigation { get; set; }
        public ICollection<eHubAirReferenceNumber> eHubAirReferenceNumberAN_CC_AirServiceProviderNavigation { get; set; }
        public ICollection<eHubAirReferenceNumber> eHubAirReferenceNumberAN_CC_SenderNavigation { get; set; }
        public ICollection<eHubAirServiceProviderMapping> eHubAirServiceProviderMappingAM_CC_AirServiceProviderNavigation { get; set; }
        public ICollection<eHubAirServiceProviderMapping> eHubAirServiceProviderMappingAM_CC_AirlineNavigation { get; set; }
        public ICollection<eHubAirServiceProviderMapping> eHubAirServiceProviderMappingAM_CC_ClientNavigation { get; set; }
        public ICollection<eHubAlert> eHubAlertEA_CC_RecipientNavigation { get; set; }
        public ICollection<eHubAlert> eHubAlertEA_CC_SenderNavigation { get; set; }
        public ICollection<eHubCertificate> eHubCertificate { get; set; }
        public ICollection<eHubClientAccessRestriction> eHubClientAccessRestriction { get; set; }
        public ICollection<eHubClientCode> eHubClientCode { get; set; }
        public ICollection<eHubClientRegistration> eHubClientRegistration { get; set; }
        public ICollection<eHubCodeSet> eHubCodeSetCS_CC_RecipientNavigation { get; set; }
        public ICollection<eHubCodeSet> eHubCodeSetCS_CC_SenderNavigation { get; set; }
        public ICollection<eHubITCustomsJobStatus> eHubITCustomsJobStatus { get; set; }
        public ICollection<eHubInboxMessage> eHubInboxMessageEI_CC_RecipientNavigation { get; set; }
        public ICollection<eHubInboxMessage> eHubInboxMessageEI_CC_SenderNavigation { get; set; }
        public ICollection<eHubMessageCopyRegistry> eHubMessageCopyRegistryMC_CC_OriginalMessage_Copy_RecipientNavigation { get; set; }
        public ICollection<eHubMessageCopyRegistry> eHubMessageCopyRegistryMC_CC_RecipientNavigation { get; set; }
        public ICollection<eHubMessageCopyRegistry> eHubMessageCopyRegistryMC_CC_SenderNavigation { get; set; }
        public ICollection<eHubMessageReferenceRegistry> eHubMessageReferenceRegistry { get; set; }
        public ICollection<eHubOutboxMessage> eHubOutboxMessageOI_CC_RecipientNavigation { get; set; }
        public ICollection<eHubOutboxMessage> eHubOutboxMessageOI_CC_SenderNavigation { get; set; }
        public ICollection<eHubReferenceFileQuery> eHubReferenceFileQueryRF_CC_ProviderNavigation { get; set; }
        public ICollection<eHubReferenceFileQuery> eHubReferenceFileQueryRF_CC_RequestorNavigation { get; set; }
        public ICollection<eHubRoutingRule> eHubRoutingRule { get; set; }
        public ICollection<eHubServiceProvider> eHubServiceProviderSP_CC_ProviderNavigation { get; set; }
        public ICollection<eHubServiceProvider> eHubServiceProviderSP_CC_ServiceNavigation { get; set; }
        public ICollection<eHubSubscriptionBroadcaster> eHubSubscriptionBroadcaster { get; set; }
        public ICollection<eHubSubscriptionValue> eHubSubscriptionValueSV_CC_RecipientNavigation { get; set; }
        public ICollection<eHubSubscriptionValue> eHubSubscriptionValueSV_CC_SenderNavigation { get; set; }
        public ICollection<eHubTransformationSet> eHubTransformationSetTS_CC_BillOtherNavigation { get; set; }
        public ICollection<eHubTransformationSet> eHubTransformationSetTS_CC_RecipientNavigation { get; set; }
        public ICollection<eHubTransformationSet> eHubTransformationSetTS_CC_SenderNavigation { get; set; }
        public ICollection<eHubUSCustomsRegistry> eHubUSCustomsRegistry { get; set; }
    }
}
