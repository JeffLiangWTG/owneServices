using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.Customs.ZA.Manifest.Business.EDIFACT;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using static System.FormattableString;
using CUSCAR = Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR;
using EDIMessageCollection = Enterprise.Messaging.Business.EDIMessageCollection;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class MessagingProvider : ASYCUDA.Business.MessagingProvider
	{
		public override ASYCUDA.Business.MessageStatusProvider MessageStatusProvider => new MessageStatusProvider();

		public override EDIFACTMessageStatusCalculator GetEDIFACTStatusCalculator()
		{
			return new EDIFACTStatusCalculator("CUSCAR");
		}

		public override ZString GetInterchangeSenderID(BusinessObjectFactory factory)
		{
			var agentCode = ZString.Empty;
			var orgProxy = AsycudaManifestHeaderExtensions.ZaOrgProxyForManifestMessaging(factory); // Need to be logged in under a ZA company otherwise the messages created are not found by the message sender task ZCS
			if (orgProxy != null)
			{
				agentCode = orgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.AgentCode, Core.Constants.CountryCodes.SouthAfrica);
				if (agentCode.IsEmpty)
				{
					agentCode = orgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.SouthAfrica);
				}
			}
			var agentDualProfileCode = orgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;

			return agentCode + agentDualProfileCode;
		}

		#region Static Helpers

		public static void RequestLatestResponse(AsycudaManifestHeader header, CUSCAREDIMessage message, IMessageNotificationCollector notification)
		{
			new REQDOCMessageManager(message.Factory, new ReqDoc(message, header), header, notification).Send();
		}

		public static ZBool CanSendMessage(IEDIMessageCollectionProvider source, ZStringBuilder sb)
		{
			var result = true;
			var cusCarHeader = ConvertEDIMessageCollectionProviderToCusCarHeader(source);
			var manifestType = cusCarHeader?.ManifestDocumentType ?? ManifestDocumentType.None;
			var messageBuilder = GetMessageBuilder(source, ZString.Empty, ZString.Empty);
			if (messageBuilder == null)
			{
				sb.Append(Invariant($"Cannot create manifest message for type {manifestType}. Most likely this means you have not supplied an acceptable value in Manifest Type."));
				result = false;
			}
			return result;
		}

		public static IMessageBuilder GetMessageBuilder(IEDIMessageCollectionProvider source, string messageSubType, ZString issuer)
		{
			var cusCarHeader = ConvertEDIMessageCollectionProviderToCusCarHeader(source);
			var manifestType = cusCarHeader?.ManifestDocumentType ?? ManifestDocumentType.None;
			return CUSCAR.CUSCARMessageBuilder.IsManifestTypeSupported(manifestType) ? new CUSCAR.CUSCARMessageBuilder(cusCarHeader, messageSubType, issuer) : null;
		}

		static ICusCarHeader ConvertEDIMessageCollectionProviderToCusCarHeader(IEDIMessageCollectionProvider source)
		{
			if (source is AsycudaManifestHeader header)
			{
				return new CusCarHeader(header);
			}

			return (ICusCarHeader)source;
		}

		public static ZString GetMRNForAmendOrDeleteFromMessages(BusinessObjectFactory factory, ZString countryCode, EDIMessageCollection messageCollection)
		{
			var messages = messageCollection.GetMatchingMessages(EDIMessage.ApplicationCodes.SouthAfricanCustoms, new[] { (ZString)ZA.Business.SARSEDIMessage.MessageTypes.CUSRES }, EDIMessage.Direction.Receive);
			var codes = ZZRefCusCodeListCombined.Loader.Load(factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, ZDateTime.Now);
			return
				(from message in messages.OfType<CUSRESEDIMessage>()
				 join code in codes on message.EntryStatus equals code.ZZD_Code
				 where code.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.CustomsCleared)
				 orderby message.EM_SystemCreateTimeUtc descending
				 select message.LocalReferenceNumber).FirstOrDefault();
		}

		#endregion
	}
}
