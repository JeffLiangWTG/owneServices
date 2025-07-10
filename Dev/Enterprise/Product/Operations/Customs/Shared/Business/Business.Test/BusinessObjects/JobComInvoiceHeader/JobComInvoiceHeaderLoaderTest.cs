using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceHeader.Loader))]
	sealed class JobComInvoiceHeaderLoaderTest : LoaderTestCase
	{
		public void TestLoadWithoutDeletedLines()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			var line2 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line2.JI_LineNo = 2;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newInvoice = newFactory.New<BaseJobComInvoiceHeader>();
			var newLine1 = newFactory.Load<BaseJobComInvoiceLine>(line1.PK);
			var newLine2 = newFactory.Load<BaseJobComInvoiceLine>(line2.PK);
			newLine1.JI_JZ = newInvoice.PK;
			newLine2.JI_JZ = newInvoice.PK;

			line2.Delete();
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				newInvoice.JZ_JE = dec.PK;
			});
			AssertEquals(1, newInvoice.JobComInvoiceLines.Count);
		}

		public void TestPreDeclarationPk()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();

			Assert("Should default to empty.", invoice.JZ_JE.IsEmpty);
			Assert("Should default to empty.", invoice.PreDeclarationPk.IsEmpty);

			invoice.JZ_JE = declaration1.PK;

			AssertEquals("Should be the PK of declaration1.", declaration1.PK, invoice.JZ_JE);
			Assert("Should be empty.", invoice.PreDeclarationPk.IsEmpty);

			invoice.JZ_JE = declaration2.PK;

			AssertEquals("Should be the PK of declaration2.", declaration2.PK, invoice.JZ_JE);
			AssertEquals("Should be the PK of declaration1.", declaration1.PK, invoice.PreDeclarationPk);

			var standaloneInvoice = Factory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = (BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(standaloneInvoice).HeaderData;

			AssertEquals("Shoud change to the PK of fake declaration.", fakeDeclaration.PK, standaloneInvoice.JZ_JE);
			Assert("Should default to empty.", standaloneInvoice.PreDeclarationPk.IsEmpty);

			Factory.Save();

			AssertEquals("Shoud keep the PK of fake declaration.", fakeDeclaration.PK, standaloneInvoice.JZ_JE);
			AssertEquals("Shoud change to the PK of fake declaration, then change to empty.", ZGuid.Empty, standaloneInvoice.PreDeclarationPk);
		}

		public void TestDistinctPackageTypes()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			bill.PackingGroups.AddNew();

			var pac1 = bill.PackingGroups[0].Packages.AddNew();
			pac1.CW_PackType = Core.Constants.PkgUnit.Bag;
			var pac2 = bill.PackingGroups[0].Packages.AddNew();
			pac2.CW_PackType = Core.Constants.PkgUnit.BaleCompressed;
			var pac3 = bill.PackingGroups[0].Packages.AddNew();
			pac3.CW_PackType = Core.Constants.PkgUnit.Carton;

			invoice.ToggleLinkageWithPackage(pac1, true);
			invoice.ToggleLinkageWithPackage(pac2, true);
			invoice.ToggleLinkageWithPackage(pac3, true);
			AssertContainsExactElementsInAnyOrder(new ZString[] { Core.Constants.PkgUnit.Bag, Core.Constants.PkgUnit.BaleCompressed, Core.Constants.PkgUnit.Carton }, ((ICusLinkPackageSupporter)invoice).DistinctPackageTypes);

			var pac4 = bill.PackingGroups[0].Packages.AddNew();
			pac4.CW_PackType = Core.Constants.PkgUnit.Bag;
			invoice.ToggleLinkageWithPackage(pac4, true);
			AssertContainsExactElementsInAnyOrder(new ZString[] { Core.Constants.PkgUnit.Bag, Core.Constants.PkgUnit.BaleCompressed, Core.Constants.PkgUnit.Carton }, ((ICusLinkPackageSupporter)invoice).DistinctPackageTypes);

			pac2.CW_MarksAndNos = pac3.CW_MarksAndNos = "N/M";
			AssertContainsExactElementsInAnyOrder(new ZString[] { Core.Constants.PkgUnit.Bag, pac2.PK.ToString(), pac3.PK.ToString() }, ((ICusLinkPackageSupporter)invoice).DistinctPackageTypes);
		}

		public void TestSupportsRelatedBill()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, invoice.SupportsRelatedBill);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, invoice.SupportsRelatedBill);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(true, invoice.SupportsRelatedBill);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			AssertEquals(true, invoice.SupportsRelatedBill);
		}

		public void TestGetQueryWithDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice2 = declaration2.Invoices.AddNew();

			ZQuery query = new BaseJobComInvoiceHeader.Loader(Factory).GetQuery(declaration);
			AssertEquals("group header does not match the query", false, groupHeader.MatchesFilter(query));
			AssertEquals("invoice does match the query", true, invoice.MatchesFilter(query));
			AssertEquals("invoice2 belongs to other declaration", false, invoice2.MatchesFilter(query));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new BaseJobComInvoiceHeader.Loader(Factory);
		}
	}
}
