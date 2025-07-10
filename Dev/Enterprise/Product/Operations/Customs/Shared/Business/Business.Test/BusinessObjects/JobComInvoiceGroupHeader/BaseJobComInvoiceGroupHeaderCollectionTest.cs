using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobComInvoiceGroupHeaderCollectionTest : CountrySpecificTestCase
	{
		public void TestCacheJZ_JEWhenDeleting()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroupHeader = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroupHeader = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			AssertEquals("PreCondition: JZ_JE", testDec.PK, subGroupHeader.JZ_JE);

			topGroupHeader.JobComInvoiceGroupHeaders.RemoveAndDelete(subGroupHeader);
			AssertEquals("JZ_JE Cached", testDec.PK, subGroupHeader.HiddenOriginalParentGuid);
		}

		public void TestClone()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			BaseJobComInvoiceHeader invoiceHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);

			BaseJobDeclaration decCloned = (BaseJobDeclaration)testDec.TemplateCopy();
			AssertEquals("Cloned GroupHeader", 1, decCloned.JobComInvoiceGroupHeaders.Count);
			AssertEquals("Cloned invoice", 1, decCloned.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals("Cloned charge", 1, decCloned.JobComInvoiceGroupHeaders[0].Charges.Count);
		}

		public void TestCloneHasChanges()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);

			BaseJobDeclaration decCloned = (BaseJobDeclaration)testDec.Clone();
			AssertEquals("HasChanges", false, decCloned.JobComInvoiceGroupHeaders.HasChanges);
		}

		public void TestLoadOnlyGroupHeaders()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			AssertEquals("PreCondition: GroupHeader is groupInvoice", true, groupHeader.JZ_GroupInvoice);

			BaseJobComInvoiceGroupHeader subGroup = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			AssertEquals("PreCondition: SubGroup is also Groupinvoice", true, subGroup.JZ_GroupInvoice);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration decLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);

			AssertEquals("Groupheader should have been loaded", 1, decLoaded.JobComInvoiceGroupHeaders.Count);
			AssertEquals("Groupheader should have been loaded", groupHeader.PK, decLoaded.JobComInvoiceGroupHeaders[0].PK);
			AssertEquals("SubGroup should have been loaded in GroupHeader's Collection", subGroup.PK, decLoaded.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].PK);
		}

		public void TestAdditionalGroupHeaderComparer()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var groupHeader1 = declaration1.JobComInvoiceGroupHeaders[0];
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var groupHeader2 = declaration2.JobComInvoiceGroupHeaders[0];
			var pivot1 = Factory.New<GroupRelatedDeclarationGenPivot>();
			pivot1.XX_Relation2ID = declaration1.PK;
			pivot1.XX_Relation1ID = groupHeader2.PK;
			var pivot2 = Factory.New<GroupRelatedDeclarationGenPivot>();
			pivot2.XX_Relation2ID = declaration2.PK;
			pivot2.XX_Relation1ID = groupHeader1.PK;
			Factory.Save();

			declaration1 = Factory.CreateNewFactory().Load<JobDeclarationSupportAdditionalInvoices>(declaration1.PK);
			AssertEquals(groupHeader1.PK, declaration1.JobComInvoiceGroupHeaders[0].PK);
			AssertEquals(groupHeader2.PK, declaration1.JobComInvoiceGroupHeaders[1].PK);
			declaration2 = Factory.CreateNewFactory().Load<JobDeclarationSupportAdditionalInvoices>(declaration2.PK);
			AssertEquals(groupHeader2.PK, declaration2.JobComInvoiceGroupHeaders[0].PK);
			AssertEquals(groupHeader1.PK, declaration2.JobComInvoiceGroupHeaders[1].PK);
		}
	}
}
