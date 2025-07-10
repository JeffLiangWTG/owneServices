using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System.Text;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.TradeSingleWindow;
	using Enterprise.Freight.Forwarding.Business;
	using NUnit.Framework;

	[TestedType(typeof(AdditionalMessageInformation))]
	public class AdditionalMessageInformationTest : NonPersistentBusinessObjectTestCase
	{
		//TODO: confirm requirement for eDocs
		//public void TestPrePopulateSupportingDocuments()
		//{
		//    SupportingDocumentCollectionTest.eDocs docs = new SupportingDocumentCollectionTest.eDocs();

		//    SupportingDocumentCollectionTest.eDoc doc = new SupportingDocumentCollectionTest.eDoc();
		//    doc.FileName = "some document name.pdf";
		//    doc.DocType = "doc1";
		//    docs.Add(doc);

		//    SupportingDocumentCollectionTest.eDoc doc1 = new SupportingDocumentCollectionTest.eDoc();
		//    doc1.FileName = "some other name.xls";
		//    doc1.DocType = "doc2";
		//    docs.Add(doc1);

		//    var message = Factory.New<TSWMessage>();
		//    message.EM_MessageText = @"<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""> <TypeCode>OCR</TypeCode> <FunctionalReferenceID>XXXXXXXXXX</FunctionalReferenceID> <FunctionCode>9</FunctionCode> -<Submitter> <ID>XXXXXXXXXX</ID> </Submitter> -<AdditionalDocument> <ID>XXXXXXXXXX</ID> <TypeCode>XXX</TypeCode> <CategoryCode>XXX</CategoryCode> <ImageBinaryObject filename=""XXXXXXXXXX"" uri=""XXXXXXXXXXX"" mimeCode=""application/pdf"">ATTACHED</ImageBinaryObject> </AdditionalDocument> -<AdditionalInformation> <Content>XXXXXXXXXX</Content> <RequestOverrideCode>XXXXXXXXXX</RequestOverrideCode> <StatementCode>XXXXXXXXXX</StatementCode> <StatementDescription>XXXXXXXXXX</StatementDescription> <StatementTypeCode>AES</StatementTypeCode> -<Pointer> <SequenceNumeric>1</SequenceNumeric> <DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode> <TagID>XXXXXXXXXX</TagID> </Pointer> </AdditionalInformation> -<BorderTransportMeans> <Name>XXXXXXXXXX</Name> <ID>XXXXXXXXXX</ID> <TypeCode>1</TypeCode> <DepartureDateTime formatCode=102>XXXXXXXXXX</DepartureDateTime> <JourneyID>XXXXXXXXXX</JourneyID> -<Itinerary> <SequenceNumeric>1</SequenceNumeric> <RoutingCountryCode>XXXXXXXXXX</RoutingCountryCode> </Itinerary> </BorderTransportMeans> -<Carrier> <Name>XXXXXXXXXX</Name> <ID>XXXXXXXXXX</ID> </Carrier> -<Consignment> <SequenceNumeric>1</SequenceNumeric> -<AdditionalDocument> <ID>XXXXXXXXXX</ID> <TypeCode>XXX</TypeCode> </AdditionalDocument> -<AssociatedTransportDocument> <ID>XXXXXXXXXX</ID> <TypeCode>XXX</TypeCode> </AssociatedTransportDocument> -<ConsignmentItem> -<UCR> <ID>XXXXXXXXXX</ID> </UCR> </ConsignmentItem> -<NotifyParty> <Name>XXXXXXXXXX</Name> <ID>XXXXXXXXXX</ID> <RoleCode>XXX</RoleCode> -<Communication> <ID>XXXXXXXXXX</ID> <TypeID>XXXXXXXXXX</TypeID> </Communication> </NotifyParty> -<TransportContractDocument> <ID>XXXXXXXXXX</ID> <TypeCode>XXX</TypeCode> -<Consolidator> <Name>XXXXXXXXXX</Name> <ID>XXXXXXXXXX</ID> </Consolidator> </TransportContractDocument> -<TransportEquipment> <SequenceNumeric>1</SequenceNumeric> <FullnessCode>XXX</FullnessCode> <ID>XXXXXXXXXX</ID> </TransportEquipment> </Consignment> -<ExitOffice> <ID>XXXXXXXXXX</ID> </ExitOffice> </Declaration> </DocumentMetadata>";

		//    var additionalMessageInformation = new AdditionalMessageInformation(message, docs, TSWTransactionTypes.Original, Factory);

		//    AssertEquals(2, additionalMessageInformation.SupportingDocuments.Count);
		//    AssertEquals("001", additionalMessageInformation.SupportingDocuments[0].DocumentType);
		//    AssertEquals(doc.UniqueKey, additionalMessageInformation.SupportingDocuments[0].eDoc);
		//    AssertEquals("002", additionalMessageInformation.SupportingDocuments[1].DocumentType);
		//    AssertEquals(doc1.UniqueKey, additionalMessageInformation.SupportingDocuments[1].eDoc);
		//}

		public void TestAM_AdditionalStatementText()
		{
			var originalAdditionalMessageInformation = new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory);
			originalAdditionalMessageInformation.AM_AdditionalStatementText = "";
			AssertEquals(false, originalAdditionalMessageInformation.AM_AdditionalStatementTextInfo.HasErrors());

			var cancelAdditionalMessageInfo = new AdditionalMessageInformation(TSWTransactionTypes.Cancel, Factory);
			cancelAdditionalMessageInfo.AM_AdditionalStatementText = "";
			AssertEquals(true, cancelAdditionalMessageInfo.AM_AdditionalStatementTextInfo.HasErrors());
			cancelAdditionalMessageInfo.AM_AdditionalStatementText = "Cancellation reason entered";
			AssertEquals(false, cancelAdditionalMessageInfo.AM_AdditionalStatementTextInfo.HasErrors());
		}

		public void TestAdditionalStatementTextStripsCarriageReturns()
		{
			var newMessage = AdditionalMessageInformation;
			newMessage.AM_AdditionalStatementText = @"Additional
Statement
Text";
			var addInfo = IAdditionalMessageInformation;
			AssertEquals("AdditionalStatementText should strip out carriage returns and new lines", "Additional Statement Text", addInfo.AdditionalStatementText);
		}

		public void TestFreeTextStripsCarriageReturns()
		{
			var newMessage = AdditionalMessageInformation;
			newMessage.AM_FreeText = @"Free
Statement
Text";
			var addInfo = IAdditionalMessageInformation;
			AssertEquals("FreeText should strip out carriage returns and new lines", "Free Statement Text", addInfo.FreeText);
		}

		public void TestManualOverrideTextStripsCarriageReturns()
		{
			var newMessage = AdditionalMessageInformation;
			newMessage.AM_OverrideText = @"Manual
Override
Text";
			var addInfo = IAdditionalMessageInformation;
			AssertEquals("ManualOverrideText should strip out carriage returns and new lines", "Manual Override Text", addInfo.ManualOverrideText);
		}

		public void TestAM_OverrideText_DefaultTextForZeroRated()
		{
			var additionalMessageInformation = new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory);
			AssertEquals(string.Empty, additionalMessageInformation.AM_OverrideText);
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasMessageErrors());

			var testDeclaration = Factory.New<JobDeclaration>();
			var eDocsForSelection = new List<IStorageDocsBaseCollection>();
			eDocsForSelection.Add(testDeclaration.DocManagerInfo.AllEDocs);
			additionalMessageInformation = new AdditionalMessageInformation(null, eDocsForSelection, TSWTransactionTypes.Completion, Factory, "");
			AssertEquals(string.Empty, additionalMessageInformation.AM_OverrideText);
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasMessageErrors());
			additionalMessageInformation.AM_OverrideText = "Test";
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasMessageErrors());

			additionalMessageInformation = new AdditionalMessageInformation(null, eDocsForSelection, TSWTransactionTypes.Completion, Factory, "");
			AssertEquals("Zero Rating override text has been removed as it is not required.", "", additionalMessageInformation.AM_OverrideText);
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasMessageErrors());
			additionalMessageInformation.AM_OverrideText = "Test";
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasMessageErrors());
			additionalMessageInformation.AM_OverrideText = string.Empty;
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasErrors());
			AssertEquals(false, additionalMessageInformation.AM_OverrideTextInfo.HasMessageErrors());
			AssertEquals(0, additionalMessageInformation.AM_OverrideTextInfo.GetErrors().Count());
			AssertEquals(0, additionalMessageInformation.AM_OverrideTextInfo.GetMessageErrors().Count());
		}

		public void TestSupportingDocuments()
		{
			AssertNotNull(AdditionalMessageInformation.SupportingDocuments);
		}

		public virtual void TestPreSaveValidation()
		{
			var additionalMessageInfo = new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory);
			additionalMessageInfo.RunPreSaveValidation();
			AssertEquals(false, additionalMessageInfo.AM_AdditionalStatementTextInfo.HasErrors());
			AssertEquals(false, additionalMessageInfo.AM_OverrideTextInfo.HasErrors());

			additionalMessageInfo = new AdditionalMessageInformation(TSWTransactionTypes.Cancel, Factory);
			AssertEquals(true, additionalMessageInfo.AM_AdditionalStatementTextInfo.HasErrors());

			additionalMessageInfo.AM_AdditionalStatementText = "Additional information free text test value to advise reason for cancellation";
			AssertEquals(false, additionalMessageInfo.AM_AdditionalStatementTextInfo.HasErrors());
		}

		public void TestDocumentsFromShipmentAndDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "IM1_Delivery_Order.pdf", "FCT");
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <PackingList> %EOF\n"), "PackingList.pdf", "FCT");
			shipment.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <Invoice> %EOF\n"), "Invoice.pdf", "INV");

			var eDocsForSelection = new List<IStorageDocsBaseCollection>();
			eDocsForSelection.Add(declaration.DocManagerInfo.AllEDocs);
			if (declaration.Shipment != null)
			{
				eDocsForSelection.Add(declaration.Shipment.DocManagerInfo.AllEDocs);
			}

			var additionalMessageInformation = new AdditionalMessageInformation(null, eDocsForSelection, TSWTransactionTypes.Original, Factory, "");
			additionalMessageInformation.GatherPotentialSupportingDocuments();
			Factory.Save();
			AssertEquals("Documents from Shipment & Declaration should be available for selection in attachment drop down control", 3, additionalMessageInformation.SupportingDocuments.StorageDocs.Count);
		}

		#region IAdditionalMessageInformation

		public void TestSupportingDocuments_()
		{
			AdditionalMessageInformation.SupportingDocuments.AddNew();
			AdditionalMessageInformation.SupportingDocuments.AddNew();

			int i = 0;

			foreach (ITSWAttachment cusAttachment in IAdditionalMessageInformation.SupportingDocuments)
			{
				i++;
			}

			AssertEquals(2, i);
		}

		#endregion

		#region Implementation

		IAdditionalInformation IAdditionalMessageInformation
		{
			get { return AdditionalMessageInformation; }
		}

		AdditionalMessageInformation AdditionalMessageInformation
		{
			get { return additionalMessageInformation ?? (additionalMessageInformation = new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)); }
		}
		AdditionalMessageInformation additionalMessageInformation;

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory);
		}

		#endregion
	}
}
