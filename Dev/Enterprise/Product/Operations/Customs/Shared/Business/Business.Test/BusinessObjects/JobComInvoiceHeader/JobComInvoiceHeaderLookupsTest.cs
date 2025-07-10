using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	public class JobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBills()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "1";

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "2";

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			var result = (IBillCollection<Bill, BaseJobDeclaration>)invoice.Lookups.Bills;
			result.Load();

			AssertEquals("There should be two elements", 2, result.Count);
			AssertEquals("housebill1 exists", true, result.Contains(houseBill1));
			AssertEquals("housebill2 exists", true, result.Contains(houseBill2));

			invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals(0, invoice.Lookups.Bills.Count);
		}

		public void TestOrganisationsFindBoxListDefaultFromUnmatchOrgNotes()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var helper = new UnmatchOrgRecordTestHelper(invoice, ImporterRec, SupplierRec);

			//Importer
			var importerList = invoice.Lookups.ImporterList;
			helper.PopulateOrgDefaultsFromUnmatchedNote(invoice.Lookups.GetType(), "ImporterList", importerList);
			AssertEquals("Importer should have conditional defaults from notes", true, importerList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(importerList.DefaultsForNewChild, ImporterRec);

			//Supplier
			var supplierList = invoice.Lookups.SupplierList;
			helper.PopulateOrgDefaultsFromUnmatchedNote(invoice.Lookups.GetType(), "SupplierList", supplierList);
			AssertEquals("Supplier should have conditional defaults from notes", true, supplierList.DefaultsForNewChild.Count > 0);
			helper.AssertOrgFieldDefaults(supplierList.DefaultsForNewChild, SupplierRec);
		}

		public virtual void TestMessageTypes()
		{
			BaseJobComInvoiceHeader header = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals(typeof(JobMessageTypeList), header.Lookups.MessageTypes.GetType());
		}

		public virtual void TestMessageTypesContainsASN()
		{
			BaseJobComInvoiceHeader header1 = Factory.New<BaseJobComInvoiceHeader>();
			Assert(header1.Lookups.MessageTypes.ContainsCode(JobMessageTypeList.MoreCodes.AdvanceShippingNotice));
			BaseJobComInvoiceHeader header2 = Factory.New<BaseJobComInvoiceHeader>();
			Assert(header2.Lookups.MessageTypes.ContainsCode(JobMessageTypeList.MoreCodes.AdvanceShippingNotice));
			AssertEquals("Lookups.MessageTypes should be cached", header1.Lookups.MessageTypes, header2.Lookups.MessageTypes);
		}

		public void TestGroupHeaderCollection()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			GroupHeaderCollection groupHeaders = invoice.Lookups.JZ_JZ_GroupInvoiceFK_List;
			AssertEquals("Two Group Headers", 2, groupHeaders.Count);
		}

		public virtual void TestDistributors()
		{
			var header = Factory.New<BaseJobComInvoiceHeader>();
			AssertType<OrgHeaderCollection>(header.Lookups.Distributors);
		}

		public virtual void TestShippers()
		{
			var header = Factory.New<BaseJobComInvoiceHeader>();
			AssertType<OrgHeaderCollection>(header.Lookups.Shippers);
		}

		public virtual void TestExporters()
		{
			var header = Factory.New<BaseJobComInvoiceHeader>();
			AssertType<ConsignorCollection>(header.Lookups.Exporters);
		}

		public virtual void TestPackagers()
		{
			var header = Factory.New<BaseJobComInvoiceHeader>();
			AssertType<OrgHeaderCollection>(header.Lookups.Packagers);
		}

		public virtual void TestPaymentMethodList()
		{
			var header = Factory.New<BaseJobComInvoiceHeader>();
			AssertType<CodeDescriptionPairList>("Type", header.Lookups.PaymentMethodList);
			AssertEquals("Count", 0, header.Lookups.PaymentMethodList.Count);
		}

		#region Implementation

		UnmatchOrgRecord ImporterRec
		{
			get
			{
				if (importerRec == null)
				{
					importerRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignee,
						nameof(OrganisationTypes.Consignee),
						"importer addr 1",
						"importer addr 2",
						"importerName",
						"2222",
						"NSW",
						"sydney",
						"importer",
						"impOwnerCode",
						"");
				}
				return importerRec;
			}
		}
		UnmatchOrgRecord importerRec;

		UnmatchOrgRecord SupplierRec
		{
			get
			{
				if (supplierRec == null)
				{
					supplierRec = UnmatchOrgRecordTestHelper.CreateUnmatchOrgRecord(OrganisationTypes.Consignor,
						nameof(OrganisationTypes.Consignor),
						"supplier addr 1",
						"supplier addr 2",
						"supplierName",
						"1111",
						"NSW",
						"city",
						"supplier",
						"SupownerCode",
						"");
				}
				return supplierRec;
			}
		}
		UnmatchOrgRecord supplierRec;

		#endregion
	}
}
