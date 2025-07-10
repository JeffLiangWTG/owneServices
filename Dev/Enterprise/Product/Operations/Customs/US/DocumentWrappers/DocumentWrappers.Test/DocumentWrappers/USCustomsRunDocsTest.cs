using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.Customs;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	sealed class USCustomsRunDocsTest : CustomsRunDocsTest
	{
		ZString storedCountry;

		protected override void SetUp()
		{
			storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(storedCountry);
			base.TearDown();
		}

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var declaration = Factory.New<Enterprise.Customs.US.Business.JobDeclaration>();
				declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var masterBill = declaration.Bills.AddNew();
				masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
				masterBill.CU_BillNum = "TestMB1";
				masterBill.ITNumber = "01234567890";
				return declaration;
			}
		}

		[ExpectNoExceptions]
		public void TestEntrySummary()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "7501 Entry Summary");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "7501 Entry Summary (SSN)");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}

		////Todo: Uncomment and change 'DeliveryGuidFromDocumentsXML' when MenuItem will be added
		//[ExpectNoExceptions]
		//public void TestDeliveryOrder()
		//{
		//    FilterForMenuItem = new ZQuery(StmMenuItemSchema.PK, new ZGuid( DeliveryGuidFromDocumentsXML));
		//    FilterForMenuItem.AddToFilter(StmMenuItemSchema.SU_MenuName, DeliveryOrderNameFromDocumentsXML);
		//    RunDocumentWithAllSections = ZBool.False;
		//    RunDocument();

		//    RunDocumentWithAllSections = ZBool.True;
		//    RunDocument();
		//}
		//const string DeliveryOrderNameFromDocumentsXML = "Delivery Order";
		//const string DeliveryGuidFromDocumentsXML = "46210183-3441-4d7d-baa7-b5ae7f7e63db";

		[ExpectNoExceptions]
		public void TestEntrySummaryImmediateDelivery()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "3461 Entry/Immediate Delivery");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "3461 Entry/Immediate Delivery (SSN)");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNonABI3461EntryImmediateDelivery()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Non-ABI 3461 Entry/Immediate Delivery");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Non-ABI 3461 Entry/Immediate Delivery (SSN)");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestPPQ368()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "PPQ Form 368 Notice of Arrival");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}
	}
}
