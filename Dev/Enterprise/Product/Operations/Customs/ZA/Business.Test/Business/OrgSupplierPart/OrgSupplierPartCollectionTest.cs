using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	sealed class OrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		public void TestAddPivotWithAdditionalLineDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.Both;
			cusClass.CC_TariffNum = "0121323122";
			invoiceLine.JI_Tariff = "1234567890";
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Set Tariff Code for new pivot", "1234567890", pivot.CI_TariffNum);
			invoiceLine.JI_CC = cusClass.PK;
			pivot = collection.AddNew().PivotsForBinding[0];
			AssertEquals("Set Class. Lookup for new pivot", cusClass.PK, pivot.CI_CC);
		}

		public override void TestAdditionalAddNewByOrgHeader()
		{
			Assert("It just test in Base class for create new product", true);
		}

		public override void TestAddingNewPart()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			cusClass.CC_TariffNum = "84314901";
			invoiceLine.JI_Tariff = "84314905";
			invoiceLine.JI_CC = cusClass.PK;
			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1234";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = invoiceLine.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "ABC";
			invoiceLine.JI_RH_NKCommodity_Code = commodity.RH_Code;
			var part = collection.AddNew();
			AssertEquals("RefCommodityCode", "ABC", part.OP_RH_NKCommodityCode);
			AssertEquals("UNDGSubstance", "1234", part.UNDGs[0].UNDGSubstance.DG_Code);
			AssertEquals("PivotCount", 1, part.PivotsForBinding.Count);
			var pivot = part.PivotsForBinding[0];
			AssertEquals("Child Type", ClassificationTypeList.Codes.HTB, pivot.CI_ChildType);
			AssertEquals("Lookup", cusClass.PK, pivot.CI_CC);
			AssertEquals("Tariff Number", "", pivot.CI_TariffNum);
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "84314905";
			collection = new OrgSupplierPartCollection(Factory, invoiceLine, invoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
			part = collection.AddNew();
			AssertEquals(1, part.PivotsForBinding.Count);
			pivot = part.PivotsForBinding[0];
			AssertEquals(ClassificationTypeList.Codes.HTB, pivot.CI_ChildType);
			AssertEquals(ZGuid.Empty, pivot.CI_CC);
			AssertEquals("84314905", pivot.CI_TariffNum);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupplierPartCollection(Factory);
		}
	}
}
