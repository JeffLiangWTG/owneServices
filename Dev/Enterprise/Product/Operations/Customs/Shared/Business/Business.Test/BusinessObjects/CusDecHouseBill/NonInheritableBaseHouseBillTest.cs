using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(Bill))]
	sealed class NonInheritableBaseHouseBillTest : BaseHouseBillTest<Bill, BaseJobDeclaration>
	{
		public void TestDefaultPackingInformation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_TotalNoOfPacks = 150;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TURE1234560";

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			bill1.CU_BillNum = "HB123";
			AssertEquals(1, bill1.PackingGroups.Count);
			AssertEquals(1, bill1.Containers.Count);

			var packingGroup = bill1.PackingGroups[0];
			AssertEquals(1, packingGroup.Packages.Count);
			var package = packingGroup.Packages[0];
			AssertEquals("TURE1234560", package.CW_ContainerNoOrEquipmentNo);
			AssertEquals(150, package.CW_PackQty);
			AssertEquals("PK", package.CW_PackType);

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "HB456";
			AssertEquals(1, bill2.PackingGroups.Count);
			var packingGroup2 = bill2.PackingGroups[0];
			AssertEquals(1, packingGroup2.Packages.Count);
			var package2 = packingGroup2.Packages[0];
			AssertEquals("TURE1234560", package.CW_ContainerNoOrEquipmentNo);
			AssertEquals(0, package2.CW_PackQty);
			AssertEquals("", package2.CW_PackType);
			bill2.Delete();

			var bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.MasterBill;
			bill3.CU_BillNum = "MB123";
			AssertEquals(0, bill3.PackingGroups.Count);

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "APLU342342";
			AssertEquals("Containers should be refreshed", 2, bill1.Containers.Count);

			container.Delete();
			AssertEquals("Containers should be refreshed", 1, bill1.Containers.Count);
		}

		public void TestDelete_ChildBills_ReBuild()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillNum = "MB1";
			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.CU_BillNum = "HB1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var houseBill2 = newFactory.New<Bill>();
			houseBill2.CU_ClusterKey = declaration.JE_ClusterKey;
			houseBill2.CU_JE = declaration.PK;
			houseBill2.CU_CU_ParentBill = masterBill.PK;
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "HB2";
			newFactory.Save();

			masterBill.Delete();
			AssertNoExceptionThrown(Factory.Save);
			declaration.Bills.Reload(true);
			AssertEquals("declaration.Bills.Count", 2, declaration.Bills.Count);
			AssertEquals("houseBill1 should be in declaration.Bills", houseBill1, declaration.Bills.FindByPK(houseBill1.PK));
			AssertEquals("houseBill1.CU_CU_ParentBill", ZGuid.Empty, houseBill1.CU_CU_ParentBill);
			houseBill2 = (Bill)declaration.Bills.FindByPK(houseBill2.PK);
			AssertEquals("houseBill2.CU_CU_ParentBill", ZGuid.Empty, houseBill2.CU_CU_ParentBill);
		}

		public void TestBillCodeDescriptionAttribute()
		{
			var bill = Factory.New<Bill>();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = "111";
			var codeDescription = bill as ICodeDescription;
			AssertEquals("HB:111", codeDescription.Code);
			AssertEquals("HB:111", codeDescription.Description);
		}

		public void TestEntries()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Bill bill1 = declaration.Bills.AddNew();
			Bill bill2 = declaration.Bills.AddNew();

			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_ValuationDateOverride = ZDateTime.Today;
			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-1);
			BaseJobComInvoiceHeader invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-2);
			BaseJobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();

			invoice1.JZ_CU_RelatedHouseBill = bill1.PK;
			invoice2.JZ_CU_RelatedHouseBill = bill1.PK;
			invoice3.JZ_CU_RelatedHouseBill = bill1.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			List<CusEntryHeader> entries1 = new List<CusEntryHeader>(bill1.Entries);
			List<CusEntryHeader> entries2 = new List<CusEntryHeader>(bill2.Entries);

			CusEntryHeader entry1 = invoiceLine1.CusEntryLine.Header;
			CusEntryHeader entry2 = invoiceLine2.CusEntryLine.Header;
			CusEntryHeader entry3 = invoiceLine3.CusEntryLine.Header;

			AssertEquals(true, entries1.Contains(entry1));
			AssertEquals(true, entries1.Contains(entry2));
			AssertEquals(true, entries1.Contains(entry3));
			AssertEquals(0, entries2.Count);

			invoice3.JZ_CU_RelatedHouseBill = bill2.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entries1 = new List<CusEntryHeader>(bill1.Entries);
			entries2 = new List<CusEntryHeader>(bill2.Entries);

			AssertEquals(2, entries1.Count);

			AssertEquals(true, entries1.Contains(entry1));
			AssertEquals(true, entries1.Contains(entry2));
			AssertEquals(false, entries1.Contains(entry3));

			AssertEquals(1, entries2.Count);
			AssertEquals(true, entries2.Contains(entry3));
		}

		public void TestCU_BillTypeDescription()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = declarationMock.Object;
			declarationMock.Protected().Setup<bool>("ShouldIncludeSubBillInBillTypeList").Returns(true);

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = ZString.Empty;
			AssertEquals(ZString.Empty, bill.CU_BillTypeDescription);

			var list = new BillTypeList();
			foreach (CodeDescriptionPair pair in list)
			{
				bill.CU_BillType = pair.Code;
				AssertEquals(pair.Description, bill.CU_BillTypeDescription);
			}
			bill.CU_BillType = "ZZZ";
			AssertEquals(ZString.Empty, bill.CU_BillTypeDescription);
		}

		public void TestCU_MessageStatus()
		{
			var billMock = Factory.NewMoq<Bill>();
			var bill = billMock.Object;
			var billLookupsMock = new Mock<CusDecHouseBillLookups>(bill);
			var list = new BillTypeList();
			billLookupsMock.Setup(m => m.MessageStatusList).Returns(new BillTypeList());
			var billLookups = billLookupsMock.Object;
			billMock.Protected().Setup<CusDecHouseBillLookups>("GetNewLookups").Returns(billLookups);
			bill.CU_Status = ZString.Empty;
			AssertEquals(ZString.Empty, bill.CU_MessageStatusDescription);
			foreach (CodeDescriptionPair pair in list)
			{
				bill.CU_Status = pair.Code;
				AssertEquals(pair.Description, bill.CU_MessageStatusDescription);
			}
			bill.CU_Status = "ZZZ";
			AssertEquals(ZString.Empty, bill.CU_MessageStatusDescription);

			billMock.VerifyAll();
			billLookupsMock.VerifyAll();
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<Bill>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var bill = declaration.Bills.AddNew();
				AssertEquals("From Declaration", "NZ", (bill as ITypeDeciderContext).Country);
			});
		}
	}
}
