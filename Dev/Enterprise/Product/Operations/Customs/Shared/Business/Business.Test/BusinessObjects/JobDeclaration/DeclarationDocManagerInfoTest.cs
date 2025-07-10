using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.Business.BaseJobDeclaration;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DeclarationDocManagerInfo))]
	public class DeclarationDocManagerInfoTest : MasterFiles.Business.Testing.DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New(typeof(BaseJobDeclaration));
		}

		//protected JobCartage pickupCartage;
		//protected JobCartage deliveryCartage;
		protected BaseJobComInvoiceHeader invoice1;
		protected BaseJobComInvoiceHeader invoice2;
		protected CusEntryHeader entry1;
		protected CusEntryHeader entry2;
		protected AccTransactionHeader invoice;
		protected AccTransactionHeader nonMatchingInvoice;
		protected BaseCusContainer container;
		protected Order order1;
		protected Order order2;
		protected CusPackingList packingList;
		protected BusinessObject quote;

		public class TestEDIMessage : EDIMessage
		{
			public TestEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber()
			{
				return "888";
			}
		}

		protected TestEDIMessage ediMessage;

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var jobDeclaration = GetJobDeclaration();
			jobDeclaration.JE_DeclarationReference = "B00000001";
			invoice1 = jobDeclaration.Invoices.AddNew();
			invoice2 = jobDeclaration.Invoices.AddNew();
			entry1 = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry2 = jobDeclaration.CustomsEntryHeaders.AddNew();
			order1 = jobDeclaration.AttachedOrders.AddNew();
			order2 = jobDeclaration.AttachedOrders.AddNew();

			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			buyer1.OH_Code = "BUYER1";
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			buyer2.OH_Code = "BUYER2";

			order1.BuyerPK = buyer1.PK;
			order2.BuyerPK = buyer2.PK;

			ediMessage = (TestEDIMessage)entry1.Messages.AddNew(typeof(TestEDIMessage));
			ediMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;

			//pickupCartage = JobDeclaration.DocsAndCartage.Cartages.AddNew();
			//pickupCartage.JJ_ConsignmentID = JobDeclaration.JE_DeclarationReference + "\\E";
			//deliveryCartage = JobDeclaration.DocsAndCartage.Cartages.AddNew();
			//deliveryCartage.JJ_ConsignmentID = JobDeclaration.JE_DeclarationReference + "\\I";

			var debtor = Factory.New<OrgHeader>();
			debtor.OH_IsDebtor = ZBool.True;
			debtor.OH_Code = "DEBTOR";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_Code = "consignee";
			jobDeclaration.JE_OH_Importer = consignee.PK;
			jobDeclaration.JE_OH_Supplier = consignee.PK;

			container = jobDeclaration.CusContainers.AddNew();

			invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_OH = debtor.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_ConsolidatedInvoiceRef = jobDeclaration.JE_DeclarationReference;

			// shouldn't be included in the related objects
			nonMatchingInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			nonMatchingInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			nonMatchingInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			nonMatchingInvoice.AH_OH = debtor.PK;
			nonMatchingInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

			packingList = jobDeclaration.CreateCusPackingList(Factory);

			quote = AddQuote(jobDeclaration);

			Factory.Save();

			return jobDeclaration;
		}

		static BusinessObject AddQuote(BaseJobDeclaration jobDeclaration)
		{
			var factory = jobDeclaration.Factory;

			var quote = factory.New(ObjectFactory.Get<IRating>().QuoteType);
			quote[RatingHeaderSchema.TH_QuoteNumber] = "12345/A";

			var job = new JobHeader.Loader(jobDeclaration).TryCreateWithoutMutexForTestOnly();
			job.JH_TH_NKQuoteNumber = (ZString)quote[RatingHeaderSchema.TH_QuoteNumber];

			return quote;
		}

		public void TestAllRelatedObjectsRetrieved()
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)GetPopulatedParentBusinessObject();
			IList relatedObjects = declaration.DocManagerInfo.RelatedObjects;
			AssertEquals("Should have Invoice 1 in the related business objects", true, relatedObjects.Contains(invoice1));
			AssertEquals("Should have Invoice 2 in the related business objects", true, relatedObjects.Contains(invoice2));
			AssertEquals("Should have Entry 1 in the related business objects", true, relatedObjects.Contains(entry1));
			AssertEquals("Should have Entry 2 in the related business objects", true, relatedObjects.Contains(entry2));
			AssertEquals("Should have Order 1 in the related business objects", true, relatedObjects.Contains(order1));
			AssertEquals("Should have Order 2 in the related business objects", true, relatedObjects.Contains(order2));
			AssertEquals("Should have Invoice in the related business objects", true, relatedObjects.Contains(invoice));
			Assert(relatedObjects.Contains(ediMessage));
			AssertEquals("Should not have the NonMatchingInvoice in the related business objects", false, relatedObjects.Contains(nonMatchingInvoice));
			Assert(relatedObjects.Contains(declaration.Consignee));
			Assert(relatedObjects.Contains(declaration.Consignor));
			Assert(relatedObjects.Contains(container));
			Assert(relatedObjects.Contains(packingList));
			Assert("Quote", relatedObjects.Contains(quote));

			var childGetter = ((Freight.Business.ShipmentDocManagerInfo.IHaveEDocsChildren)declaration.DocManagerInfo);
			AssertCollectionContains("EDIMessages are shown to freight as well as to customs", ediMessage, childGetter.GetEDocsChildrenForAFreightJobToDisplay());
		}

		public void TestMarksAndNumbersFromDeclaration()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_MarksAndNumbersShort = "Test Marks and Numbers";

			StmNote[] marksAndNumbersNotes = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
			AssertEquals("At least one note", 1, marksAndNumbersNotes.Length);
			AssertEquals("Note is not empty", "Test Marks and Numbers", marksAndNumbersNotes[0].ST_NoteText);

			marksAndNumbersNotes[0].ST_NoteText = "Edited Note Marks and Numbers";
			AssertEquals("Short marks and numbers is updated", "Edited Note Marks and Numbers", declaration.JE_MarksAndNumbersShort);
		}

		public void TestMarksAndNumbersFromShipment()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.JS_MarksAndNumbersShort = "Short Marks and Numbers";
			StmNote[] marksAndNumbersNotes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
			AssertEquals("At least one note", 1, marksAndNumbersNotes.Length);
			AssertEquals("Note is not empty", "Short Marks and Numbers", marksAndNumbersNotes[0].ST_NoteText);
			AssertEquals("Declaration's Marks and numbers not empty", "Short Marks and Numbers", declaration.JE_MarksAndNumbersShort);

			marksAndNumbersNotes[0].ST_NoteText = "Edited Note Marks and Numbers";
			AssertEquals("Shipment short marks and numbers is updated", "Edited Note Marks and Numbers", shipment.JS_MarksAndNumbersShort);
			AssertEquals("Declaration short marks and numbers is updated", "Edited Note Marks and Numbers", declaration.JE_MarksAndNumbersShort);

			declaration.JE_MarksAndNumbersShort = "Dec short Marks and numbers";
			AssertEquals("Shipment short marks and numbers is updated", "Dec short Marks and numbers", shipment.JS_MarksAndNumbersShort);
			AssertEquals("Note is updated", "Dec short Marks and numbers", marksAndNumbersNotes[0].ST_NoteText);
		}

		public void TestIsDocManagerInfoLoaded()
		{
			var declaration = GetJobDeclaration();
			CombineAssertions(() =>
			{
				Assert("before load", !declaration.IsDocManagerInfoLoaded);
				AssertNotNull("DocManagerInfo", declaration.DocManagerInfo);
				Assert("after load", declaration.IsDocManagerInfoLoaded);
			});
		}

		public void TestDeclarationAllRelatedObjectsIncludeRelatedDeclarationsAndTheirActiveEntryHeaders()
		{
			var declaration = GetJobDeclaration();
			declaration.Invoices.RemoveAll();
			declaration.ActiveEntryHeaders.RemoveAll();
			var relatedDeclaration1 = declaration.RelatedDeclarations.AddNew();
			var entry1 = relatedDeclaration1.ActiveEntryHeaders.AddNew();
			var entry2 = relatedDeclaration1.ActiveEntryHeaders.AddNew();
			var relatedDeclaration2 = declaration.RelatedDeclarations.AddNew();
			var entry3 = relatedDeclaration2.ActiveEntryHeaders.AddNew();
			var entry4 = relatedDeclaration2.ActiveEntryHeaders.AddNew();
			var relatedObjects = new DeclarationDocManagerInfo(declaration).RelatedObjects;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { relatedDeclaration1, entry1, entry2, relatedDeclaration2, entry3, entry4 }, relatedObjects);
		}

		public void TestShipmentAllRelatedObjectsIncludeRelatedDeclarationsAndTheirActiveEntryHeaders()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var declaration = GetJobDeclaration();
			declaration.Invoices.RemoveAll();
			declaration.ActiveEntryHeaders.RemoveAll();
			declaration.JE_JS = shipment.PK;
			var relatedDeclaration1 = declaration.RelatedDeclarations.AddNew();
			var entry1 = relatedDeclaration1.ActiveEntryHeaders.AddNew();
			var entry2 = relatedDeclaration1.ActiveEntryHeaders.AddNew();
			var relatedDeclaration2 = declaration.RelatedDeclarations.AddNew();
			var entry3 = relatedDeclaration2.ActiveEntryHeaders.AddNew();
			var entry4 = relatedDeclaration2.ActiveEntryHeaders.AddNew();
			var relatedObjects = new ShipmentDocManagerInfo(shipment).RelatedObjects;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { declaration, relatedDeclaration1, entry1, entry2, relatedDeclaration2, entry3, entry4 }, relatedObjects);
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}
	}
}
