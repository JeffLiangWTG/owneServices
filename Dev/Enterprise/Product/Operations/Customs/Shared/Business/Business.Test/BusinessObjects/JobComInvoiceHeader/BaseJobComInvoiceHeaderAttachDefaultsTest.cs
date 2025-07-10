using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceHeaderAttachDefaultsTest : TestCaseWithFactory
	{
		public void TestDefaultsRefreshedWhenUnamendedInvoiceAttached()
		{
			AssertEquals("precondition description", "", line.JI_Description);
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			var pivot = part.PivotsForBinding != null ? part.PivotsForBinding.Cast<BaseCusClassPartPivot>().FirstOrDefault() : null;
			if (pivot != null)
			{
				pivot.CI_DateStart = ZDateTime.Now.AddDays(-10);
				pivot.CI_TariffNum = "985.152.100";
			}
			invoice.JZ_JE = declaration.PK;
			AssertEquals("description refreshed", "TESTPARTDESCRIPTION", line.JI_Description);
		}

		public void TestDefaultsNotRefreshedWhenAmendedInvoiceAttached()
		{
			AssertEquals("precondition description", "", line.JI_Description);
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = false;
			invoice.JZ_JE = declaration.PK;
			AssertEquals("description", "", line.JI_Description);
		}

		public void TestRoutingWhenInvoiceAttached()
		{
			var factory2 = new BusinessObjectFactory();
			var invoice = factory2.New<BaseJobComInvoiceHeader>();

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var transport = invoice.Transports.AddNew();
			transport.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = "DCV VESSEL";
			transport.JW_VoyageFlight = "DF2";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2010, 1, 10);
			transport.JW_ATD = new ZDateTime(2010, 1, 11);
			transport.JW_ETA = new ZDateTime(2010, 1, 12);
			transport.JW_ATA = new ZDateTime(2010, 1, 13);
			invoice.Factory.Save();
			AssertEquals("precondition Routings", 1, invoice.Transports.Count);
			AssertEquals("Standalone Invoice", ZGuid.Empty, invoice.JZ_JE);

			var newDeclaration = factory2.New<BaseJobDeclaration>();
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_JE = newDeclaration.PK; //attach
			AssertEquals("Routings on Declaration", 1, newDeclaration.Transports.Count);
			AssertEquals("Routings not deleted from Invoice", 1, invoice.Transports.Count);

			factory2.Save();
			AssertEquals("Routings on Declaration", 1, newDeclaration.Transports.Count);
			AssertEquals("Routings deleted from Invoice", 0, invoice.Transports.Count);

			var factory3 = new BusinessObjectFactory();
			var loadedDeclaration = factory3.Load<BaseJobDeclaration>(newDeclaration.PK);
			var loadedHeader = factory3.Load<BaseJobComInvoiceHeader>(invoice.PK);
			AssertEquals("Routings on Declaration", 1, loadedDeclaration.Transports.Count);
			AssertEquals("Routings deleted from Invoice", 0, loadedHeader.Transports.Count);
		}

		public void TestRoutingWhenInvoiceAttachedDetached()
		{
			var factory2 = new BusinessObjectFactory();
			var invoice = factory2.New<BaseJobComInvoiceHeader>();

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			var transport = invoice.Transports.AddNew();
			transport.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = "DCV VESSEL";
			transport.JW_VoyageFlight = "DF2";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2010, 1, 10);
			transport.JW_ATD = new ZDateTime(2010, 1, 11);
			transport.JW_ETA = new ZDateTime(2010, 1, 12);
			transport.JW_ATA = new ZDateTime(2010, 1, 13);

			var newDeclaration = factory2.New<BaseJobDeclaration>();
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_JE = newDeclaration.PK; //attach
			AssertEquals("Routings on Declaration", 1, newDeclaration.Transports.Count);
			AssertEquals("Routings not deleted from Invoice yet", 1, invoice.Transports.Count);

			invoice.JZ_JE = ZGuid.Empty; //detach
			AssertEquals("Routings on Declaration", 0, newDeclaration.Transports.Count);
			AssertEquals("Routings not deleted from Invoice yet", 1, invoice.Transports.Count);

			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var loadedDeclaration = factory3.Load<BaseJobDeclaration>(newDeclaration.PK);
			var loadedInvoiceHeader = factory3.Load<BaseJobComInvoiceHeader>(invoice.PK);
			AssertEquals("Routings deleted from Declaration", 0, loadedDeclaration.Transports.Count);
			AssertEquals("Routings not deleted from Invoice", 1, loadedInvoiceHeader.Transports.Count);
		}

		public void TestAttachedRoutingUpdateDeclTransportData()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			Transport transport = invoice.Transports.AddNew();
			transport.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_Vessel = "DCV VESSEL";
			transport.JW_VoyageFlight = "DF2";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = new ZDateTime(2010, 1, 10);
			transport.JW_ATD = new ZDateTime(2010, 1, 11);
			transport.JW_ETA = new ZDateTime(2010, 1, 12);
			transport.JW_ATA = new ZDateTime(2010, 1, 13);

			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_JE = declaration.PK;

			Assert("export date should be updated", !declaration.JE_ExportDate.IsEmpty);
			Assert("arrival date should be updated", !declaration.JE_DateOfArrival.IsEmpty);
			AssertEquals("Port Of Arrival should be updated", "USLAX", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Port Of Loading should be updated", "AUSYD", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Transport Mode should be updated", TransportTypeList.Codes.Sea, declaration.JE_TransportMode);
			AssertEquals("Vessel should be updated", "DCV VESSEL", declaration.JE_VesselName);
			AssertEquals("Voyage Flight No should be updated", "DF2", declaration.JE_VoyageFlightNo);
		}

		public virtual void TestAddHeaderRefsToDeclaration()
		{
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT007";
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB800450";
			AssertEquals("Precondition: 1 container for declaration", 1, declaration.CusContainers.Count);
			AssertEquals("CNT007", declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Precondition: 1 MB for declaration", 1, declaration.Bills.Count);
			AssertEquals("MB800450", declaration.Bills[0].CU_BillNum);

			JobComInvoiceHeaderRefs ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			ref1.J2_ReferenceNumber = "CNT002";

			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			ref1.J2_ReferenceNumber = "CNT007";

			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.MB;
			ref1.J2_ReferenceNumber = "MB150";

			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.MB;
			ref1.J2_ReferenceNumber = "MB800450";

			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.HB;
			ref1.J2_ReferenceNumber = "HB002";

			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.SH;
			ref1.J2_ReferenceNumber = "SubHouseBill005";

			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.HB;
			ref1.J2_ReferenceNumber = "HB008";

			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = "ERT";
			ref1.J2_ReferenceNumber = "123456";

			invoice.JZ_JE = declaration.PK;

			AssertEquals(2, declaration.CusContainers.Count);
			AssertNotNull(declaration.CusContainers.Find("CNT007"));
			AssertNotNull(declaration.CusContainers.Find("CNT002"));

			AssertEquals(4, declaration.Bills.Count);
			bill = declaration.Bills.FindByBillNumberAndType("MB800450", BillTypeList.Codes.MasterBill);
			AssertEquals(ZGuid.Empty, bill.CU_CU_ParentBill);

			Bill bill1 = declaration.Bills.FindByBillNumberAndType("MB150", BillTypeList.Codes.MasterBill);
			AssertEquals(ZGuid.Empty, bill1.CU_CU_ParentBill);

			Bill bill2 = declaration.Bills.FindByBillNumberAndType("HB002", BillTypeList.Codes.HouseBill);
			AssertEquals(bill1.PK, bill2.CU_CU_ParentBill);

			Bill bill3 = declaration.Bills.FindByBillNumberAndType("HB008", BillTypeList.Codes.HouseBill);
			AssertEquals(bill1.PK, bill3.CU_CU_ParentBill);

			Bill bill4 = declaration.Bills.FindByBillNumberAndType("SubHouseBill005", BillTypeList.Codes.SubHouseBill);
			AssertEquals("Bill not found. because SH not allowed at AU", null, bill4);

			bill = declaration.Bills.FindByBillNumberAndType("123456", "ERT");
			AssertEquals("Unknown Reference Code - object not added", null, bill);
		}

		public virtual void TestAddHeaderRefsToDeclarationDoNotIncludeContainerRefsUnlessRequired()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB800450";
			AssertEquals("Precondition: No containers on declaration", 0, declaration.CusContainers.Count);
			AssertEquals("Precondition: 1 MB for declaration", 1, declaration.Bills.Count);
			AssertEquals("MB800450", declaration.Bills[0].CU_BillNum);

			var ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			ref1.J2_ReferenceNumber = "MSKU0049387";

			var ref2 = invoice.InvoiceHeaderRefs.AddNew();
			ref2.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			ref2.J2_ReferenceNumber = "CHSU4284471";

			var ref3 = invoice.InvoiceHeaderRefs.AddNew();
			ref3.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.HB;
			ref3.J2_ReferenceNumber = "HB002";

			var ref4 = invoice.InvoiceHeaderRefs.AddNew();
			ref4.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.HB;
			ref4.J2_ReferenceNumber = "HB008";

			invoice.JZ_JE = declaration.PK;

			AssertEquals(0, declaration.CusContainers.Count);
			AssertNull("Do not create Container references if they are going to be deleted on saving", declaration.CusContainers.Find("MSKU0049387"));
			AssertNull("Do not create Container references if they are going to be deleted on saving", declaration.CusContainers.Find("CHSU4284471"));

			AssertEquals(3, declaration.Bills.Count);
			bill = declaration.Bills.FindByBillNumberAndType("MB800450", BillTypeList.Codes.MasterBill);
			AssertEquals(ZGuid.Empty, bill.CU_CU_ParentBill);

			var bill2 = declaration.Bills.FindByBillNumberAndType("HB002", BillTypeList.Codes.HouseBill);
			AssertEquals(bill.PK, bill2.CU_CU_ParentBill);

			var bill3 = declaration.Bills.FindByBillNumberAndType("HB008", BillTypeList.Codes.HouseBill);
			AssertEquals(bill.PK, bill3.CU_CU_ParentBill);
		}

		public void TestNotesWhenInvoiceAttachedDetached()
		{
			var factory2 = new BusinessObjectFactory();
			var invoice = factory2.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;

			var notes = new Notes(invoice);
			invoice.Notes.AddNew(true, "Test Note", "Note added for the test");
			invoice.Notes.AddNew(true, "Test Note 2", "Note 2 added for the test");

			invoice.Factory.Save();
			AssertEquals("Precondition: should be 2 notes for invoice", 2, invoice.Notes.GetAllNotes().Count);
			AssertEquals("Precondition: standalone Invoice", ZGuid.Empty, invoice.JZ_JE);

			var newDeclaration = factory2.New<BaseJobDeclaration>();
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_JE = newDeclaration.PK; //attach
			invoice.JZ_ClusterKey = newDeclaration.JE_ClusterKey;
			AssertEquals("Declaration has Related Notes", true, newDeclaration.Notes.HasRelatedNotes);
			AssertEquals("Notes on Declaration", 2, newDeclaration.Notes.VisibleNotes.Count);
			AssertEquals("Notes not deleted from Invoice", 2, invoice.Notes.VisibleNotes.Count);

			invoice.JZ_JE = ZGuid.Empty; //detach without saving
			invoice.JZ_ClusterKey = 0;
			AssertEquals("Declaration has no Related Notes", false, newDeclaration.Notes.HasRelatedNotes);
			AssertEquals("Notes not deleted from Invoice", 2, invoice.Notes.VisibleNotes.Count);

			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
			invoice.JZ_JE = newDeclaration.PK; //attach
			invoice.JZ_ClusterKey = newDeclaration.JE_ClusterKey;
			factory2.Save();
			AssertEquals("Declaration has Related Notes", true, newDeclaration.Notes.HasRelatedNotes);
			AssertEquals("Notes on Declaration", 2, newDeclaration.Notes.VisibleNotes.Count);
			AssertEquals("Notes not deleted from Invoice", 2, invoice.Notes.VisibleNotes.Count);

			var newFactoryForLoad = new BusinessObjectFactory();
			var loadedDeclaration = newFactoryForLoad.Load<BaseJobDeclaration>(newDeclaration.PK);
			var loadedInvoice = newFactoryForLoad.Load<BaseJobComInvoiceHeader>(invoice.PK);
			AssertEquals("Declaration has Related Notes", true, loadedDeclaration.Notes.HasRelatedNotes);
			AssertEquals("Notes on Declaration", 2, loadedDeclaration.Notes.VisibleNotes.Count);
			AssertEquals("Notes not deleted from Invoice", 2, loadedInvoice.Notes.VisibleNotes.Count);
		}

		public void TestLinkRemovedWhenInvoiceLineDetached()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceline1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceline2 = invoice.JobComInvoiceLines.AddNew();
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine11 = entry.MergedLines.AddNew();
			var entryLine12 = entry.MergedLines.AddNew();
			invoiceline1.JI_CL = entryLine11.PK;
			invoiceline2.JI_CL = entryLine12.PK;

			var link1 = invoiceline1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine11.PK;

			var link2 = invoiceline2.AdditionalEntryLineLinks.AddNew();
			link2.BU_CL = entryLine12.PK;
			AssertEquals(1, invoiceline1.AdditionalEntryLineLinks.Count);
			AssertEquals(1, invoiceline2.AdditionalEntryLineLinks.Count);

			invoice.JZ_JE = ZGuid.Empty;
			AssertEquals(0, invoiceline1.AdditionalEntryLineLinks.Count);
			AssertEquals(0, invoiceline2.AdditionalEntryLineLinks.Count);
		}

		public void TestAllEntriesLinkToInvoiceLineRemovedWhenAttachToDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var decalration2 = Factory.New<BaseJobDeclaration>();
			invoice.JZ_JE = decalration2.PK;
			Assert(invoiceLine.JI_CL.IsEmpty);
			Assert(invoiceLine2.JI_CL.IsEmpty);
		}

		public void TestInvoiceLineNumbersAreNotReorderedWhenAttachingINVTypeASNToDeclaration()
		{
			var factory = new BusinessObjectFactory();

			var importer = factory.New<OrgHeader>();
			importer.OH_Code = "TSTORG01";
			importer.OH_FullName = "TSTORG01";

			var supplier = factory.New<OrgHeader>();
			supplier.OH_Code = "TSTORG02";
			supplier.OH_FullName = "TSTORG02";

			var invoice = factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_OH_Buyer = importer.PK;
			invoice.JZ_OH_Supplier = supplier.PK;

			new FakeDeclarationCreatorForInvoice(invoice);

			var invoiceLine1 = CreateJobComInvoiceLine(invoice, 1);
			var invoiceLine2 = CreateJobComInvoiceLine(invoice, 3);
			var invoiceLine3 = CreateJobComInvoiceLine(invoice, 5);
			var invoiceLine4 = CreateJobComInvoiceLine(invoice, 7);
			var invoiceLine5 = CreateJobComInvoiceLine(invoice, 9);
			var invoiceLine6 = CreateJobComInvoiceLine(invoice, 11);
			var invoiceLine7 = CreateJobComInvoiceLine(invoice, 2);
			var invoiceLine8 = CreateJobComInvoiceLine(invoice, 4);
			var invoiceLine9 = CreateJobComInvoiceLine(invoice, 6);
			var invoiceLine10 = CreateJobComInvoiceLine(invoice, 8);
			var invoiceLine11 = CreateJobComInvoiceLine(invoice, 10);
			var invoiceLine12 = CreateJobComInvoiceLine(invoice, 12);

			factory.Save();

			var declaration = factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			invoice.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;

			invoice.JZ_JE = declaration.PK;

			var filteredInvoiceLines = declaration.FilteredInvoiceLines;
			AssertEquals("JI_LineNo for 1st invoice should be 1", (short)1, filteredInvoiceLines[0].JI_LineNo);
			AssertEquals("PK for 1st invoice should be invoiceLine1", invoiceLine1.PK, filteredInvoiceLines[0].PK);

			AssertEquals("JI_LineNo for 2nd invoice should be 2", (short)2, filteredInvoiceLines[1].JI_LineNo);
			AssertEquals("PK for 2nd invoice should be invoiceLine7", invoiceLine7.PK, filteredInvoiceLines[1].PK);

			AssertEquals("JI_LineNo for 3rd invoice should be 3", (short)3, filteredInvoiceLines[2].JI_LineNo);
			AssertEquals("PK for 3rd invoice should be invoiceLine2", invoiceLine2.PK, filteredInvoiceLines[2].PK);

			AssertEquals("JI_LineNo for 4th invoice should be 4", (short)4, filteredInvoiceLines[3].JI_LineNo);
			AssertEquals("PK for 4th invoice should be invoiceLine8", invoiceLine8.PK, filteredInvoiceLines[3].PK);

			AssertEquals("JI_LineNo for 5th invoice should be 5", (short)5, filteredInvoiceLines[4].JI_LineNo);
			AssertEquals("PK for 5th invoice should be invoiceLine3", invoiceLine3.PK, filteredInvoiceLines[4].PK);

			AssertEquals("JI_LineNo for 6th invoice should be 6", (short)6, filteredInvoiceLines[5].JI_LineNo);
			AssertEquals("PK for 6th invoice should be invoiceLine9", invoiceLine9.PK, filteredInvoiceLines[5].PK);

			AssertEquals("JI_LineNo for 7th invoice should be 7", (short)7, filteredInvoiceLines[6].JI_LineNo);
			AssertEquals("PK for 7th invoice should be invoiceLine4", invoiceLine4.PK, filteredInvoiceLines[6].PK);

			AssertEquals("JI_LineNo for 8th invoice should be 8", (short)8, filteredInvoiceLines[7].JI_LineNo);
			AssertEquals("PK for 8th invoice should be invoiceLine10", invoiceLine10.PK, filteredInvoiceLines[7].PK);

			AssertEquals("JI_LineNo for 9th invoice should be 9", (short)9, filteredInvoiceLines[8].JI_LineNo);
			AssertEquals("PK for 9th invoice should be invoiceLine5", invoiceLine5.PK, filteredInvoiceLines[8].PK);

			AssertEquals("JI_LineNo for 10th invoice should be 10", (short)10, filteredInvoiceLines[9].JI_LineNo);
			AssertEquals("PK for 10th invoice should be invoiceLine11", invoiceLine11.PK, filteredInvoiceLines[9].PK);

			AssertEquals("JI_LineNo for 11th invoice should be 11", (short)11, filteredInvoiceLines[10].JI_LineNo);
			AssertEquals("PK for 11th invoice should be invoiceLine6", invoiceLine6.PK, filteredInvoiceLines[10].PK);

			AssertEquals("JI_LineNo for 12th invoice should be 12", (short)12, filteredInvoiceLines[11].JI_LineNo);
			AssertEquals("PK for 12th invoice should be invoiceLine12", invoiceLine12.PK, filteredInvoiceLines[11].PK);
		}

		BaseJobComInvoiceLine CreateJobComInvoiceLine(BaseJobComInvoiceHeader invoice, ZShort lineNo)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = lineNo;

			return invoiceLine;
		}

		#region Implementation
		protected BaseJobDeclaration declaration;
		protected BaseJobComInvoiceHeader invoice;
		protected BaseJobComInvoiceLine line;
		protected OrgSupplierPart part;
		protected OrgHeader supplier;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			supplier = Factory.LoadTop1<OrgHeader>(new ZQuery());
			supplier.OH_RL_NKClosestPort = "AUSYD";
			invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_OH_Supplier = supplier.PK;
			line = invoice.JobComInvoiceLines.AddNew();
			line.JI_PartNo = "TestPartNum";

			part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TestPartNum";
			part.OP_Desc = "TESTPARTDESCRIPTION";

			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = "SUP";
			relation.OU_OH = supplier.PK;
		}
		#endregion
	}
}
