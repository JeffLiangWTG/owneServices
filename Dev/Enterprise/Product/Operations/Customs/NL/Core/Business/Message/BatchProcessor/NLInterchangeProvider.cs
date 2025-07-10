using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.NL.Business;

public class NLInterchangeProvider : InterchangeProviderBase
{
	public NLInterchangeProvider(NonDependentEDIMessageCollection readyMessages) : base(readyMessages)
	{
	}

	protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

	protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
	{
		var requestMessage = messages.Cast<NLEDIMessage>().Single();
		SetInterchangeValuesForTransmit(interchange, messages, requestMessage.EM_MessageType, NLConstants.EDIInterchange.NLCustoms, requestMessage.Company.LicenceKeyIdentifier);
		interchange.EI_TransportType = EDIInterchange.TransportType.xT;
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
		interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.NLCustoms;

		var orgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		var senderID = NLCustomsRegistry.Instance.SenderIDs.Value.Cast<SenderInfo>().FirstOrDefault(si => si.OrganizationPK == orgProxy)?.SenderID;
		var senderIDSuffix = GetSenderIdSuffix(requestMessage.EM_MessageType);
		var customsSubjectValue =
			$"[{NLConstants.CustomMsgAttributes.SubjectAttributes.V}={senderID}{senderIDSuffix}]" +
			$"[{NLConstants.CustomMsgAttributes.SubjectAttributes.A}={GetRecipientId(requestMessage.EM_MessageType)}]" +
			$"[{NLConstants.CustomMsgAttributes.SubjectAttributes.K}={interchange.eHubID.KeepAlphanumericCharacters()}]" +
			$"[{NLConstants.CustomMsgAttributes.SubjectAttributes.S}={NLConstants.CustomMsgValues.SubjectAttributeValues.S}]";

		var interfaceHeaderDictionary = new Dictionary<string, string>
		{
			{ NLConstants.CustomMsgAttributes.Subject, customsSubjectValue },
		};

		interchange.SetHeaderTextWithAttributeDictionary(interfaceHeaderDictionary);
	}

	string GetSenderIdSuffix(string messageType)
	{
		switch (messageType)
		{
			case NLEDIMessageTypes.Codes.NCT:
				return NLConstants.SenderIdSuffixes.NCTS;
			case NLEDIMessageTypes.Codes.DMS:
				return NLConstants.SenderIdSuffixes.DMS;
			default:
				return string.Empty;
		}
	}

	string GetRecipientId(string messageType)
	{
		switch (messageType)
		{
			case NLEDIMessageTypes.Codes.NCT:
				return NLCustomsRegistry.Instance.CustomsMessageVersion.Value.Cast<MessageVersionRegistry>().FirstOrDefault(si => si.DomainCode == MessageVersionRegistry.NCTSP5DomainCode)?.TargetSystemName;
			case NLEDIMessageTypes.Codes.DMS:
				return NLCustomsRegistry.Instance.CustomsMessageVersion.Value.Cast<MessageVersionRegistry>().FirstOrDefault(si => si.DomainCode == MessageVersionRegistry.DMSDomainCode)?.TargetSystemName;
			default:
				return string.Empty;
		}
	}

	protected override string GetCollationKey(EDIMessage message) => message.PK.ToString();

	protected override ZString GetInterchangeFooter(int messageCount) => ZString.Empty;
}
