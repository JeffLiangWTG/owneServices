using System;
using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class SendChangesMessageHelper : AmendmentMessageHelper
{
	public SendChangesMessageHelper() : base(null)
	{
	}

	public static SendChangesMessageHelper Instance => sendChangesMessageHelper.Value;
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	static readonly Lazy<SendChangesMessageHelper> sendChangesMessageHelper = new Lazy<SendChangesMessageHelper>(() => new SendChangesMessageHelper());

	protected override EDIMessage CreateNewMessageForComparison(WCOJobDeclarationMessageSendingObject objectToSend)
	{
		if (objectToSend is JobDeclarationMessageSendingObject declarationObjectToSend && declarationObjectToSend.Header is CusEntryHeader entry)
		{
			var newMessageXML = string.Empty;
			newMessageXML = XmlMessageHelper.RemoveEmptyXmlElements(newMessageXML);

			var newMessage = entry.Messages.AddNew(typeof(NLComparisonEDIMessage));

			switch (declarationObjectToSend.MessageType)
			{
				case ExportSendMessageTypes.Codes.AMD:
					newMessage.EM_MessageSubType = ExportSendMessageTypes.Codes.AMD;
					break;
				case ImportSendMessageTypes.Codes.CRI:
					newMessage.EM_MessageSubType = ImportSendMessageTypes.Codes.CRI;
					break;
			}
			newMessage.EM_MessageText = newMessageXML;
			newMessage.EM_ApplicationReference = NLConstants.EDIMessageApplicationReferences.Current;
			return newMessage;
		}

		return null;
	}

	protected override string MetaDataDeclarationXPathValue => (NoResString)"//node[@match=1]/node[@match='3']"; // XPath for DiffGram

	protected override List<AlwaysAdd> GetAlwaysAddElements(XDocument newXMLDoc)
	{
		var names = new List<string>();

		names.Add("FunctionalReferenceID");
		names.Add("ID");
		names.Add("DeclarationOffice");
		names.Add((NoResString)"Agent");
		names.Add((NoResString)"Declarant");

		return GetAlwaysAddElementsFromDocument(names, newXMLDoc, (NoResString)"Declaration");
	}
}
