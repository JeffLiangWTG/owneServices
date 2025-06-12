using System;
using System.Collections.Generic;

namespace Hawking.eHub.Model.eHubTransactions
{
    public partial class eHubMessageType
    {
        public eHubMessageType()
        {
            InverseDT_DT_InnerTypeNavigation = new HashSet<eHubMessageType>();
            InverseDT_DT_PostAssembleWrapperNavigation = new HashSet<eHubMessageType>();
            eHubAirDefaultServiceProvider = new HashSet<eHubAirDefaultServiceProvider>();
            eHubAirServiceProviderMapping = new HashSet<eHubAirServiceProviderMapping>();
            eHubInboxXmlContent = new HashSet<eHubInboxXmlContent>();
            eHubOutboxMessage = new HashSet<eHubOutboxMessage>();
            eHubSubscriptionAutoSubscribe = new HashSet<eHubSubscriptionAutoSubscribe>();
            eHubSubscriptionLookup = new HashSet<eHubSubscriptionLookup>();
            eHubTransformationSet = new HashSet<eHubTransformationSet>();
            eHubTransformationTypeTT_DT_SourceNavigation = new HashSet<eHubTransformationType>();
            eHubTransformationTypeTT_DT_TargetNavigation = new HashSet<eHubTransformationType>();
        }

        public Guid DT_PK { get; set; }
        public string DT_Code { get; set; }
        public bool DT_IsFlatFile { get; set; }
        public bool DT_IsEDI { get; set; }
        public string DT_Charset { get; set; }
        public string DT_EnvelopeXpath { get; set; }
        public Guid? DT_DT_InnerType { get; set; }
        public bool DT_ReprocessSubMessage { get; set; }
        public bool DT_PostAssembleMapping { get; set; }
        public Guid? DT_DT_PostAssembleWrapper { get; set; }
        public bool DT_IsJson { get; set; }

        public eHubMessageType DT_DT_InnerTypeNavigation { get; set; }
        public eHubMessageType DT_DT_PostAssembleWrapperNavigation { get; set; }
        public ICollection<eHubMessageType> InverseDT_DT_InnerTypeNavigation { get; set; }
        public ICollection<eHubMessageType> InverseDT_DT_PostAssembleWrapperNavigation { get; set; }
        public ICollection<eHubAirDefaultServiceProvider> eHubAirDefaultServiceProvider { get; set; }
        public ICollection<eHubAirServiceProviderMapping> eHubAirServiceProviderMapping { get; set; }
        public ICollection<eHubInboxXmlContent> eHubInboxXmlContent { get; set; }
        public ICollection<eHubOutboxMessage> eHubOutboxMessage { get; set; }
        public ICollection<eHubSubscriptionAutoSubscribe> eHubSubscriptionAutoSubscribe { get; set; }
        public ICollection<eHubSubscriptionLookup> eHubSubscriptionLookup { get; set; }
        public ICollection<eHubTransformationSet> eHubTransformationSet { get; set; }
        public ICollection<eHubTransformationType> eHubTransformationTypeTT_DT_SourceNavigation { get; set; }
        public ICollection<eHubTransformationType> eHubTransformationTypeTT_DT_TargetNavigation { get; set; }
    }
}
