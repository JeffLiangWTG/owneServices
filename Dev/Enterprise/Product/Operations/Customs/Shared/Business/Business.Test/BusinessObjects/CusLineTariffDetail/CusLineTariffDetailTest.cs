using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetail))]
	public class CusLineTariffDetailTest : CusLineTariffDetailAbstractTest
	{
		[ExpectNoExceptions]
		public void TestPopulateDataModelIfNeeded()
		{
			var tariffDetail = Factory.New<CusLineTariffDetail>();
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.CusLineTariffDetails.Add(tariffDetail);
			Factory.Save();

			AssertEquals(Env.CurrentCompany.Country.Code, tariffDetail.BZ_DataModel);
		}

		public void TestBZ_DataModel_SetOnSaving()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.FillWithValidTestData();
			AssertEquals("Not set", ZString.Empty, tariffDetail.BZ_DataModel);
			Factory.Save();
			AssertEquals("set", Env.CurrentCompany.Country.Code, tariffDetail.BZ_DataModel);

			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			tariffDetail = pivot.CusLineTariffDetails.AddNew();
			tariffDetail.FillWithValidTestData();
			AssertEquals("Not set", ZString.Empty, tariffDetail.BZ_DataModel);
			Factory.Save();
			AssertEquals("set", Env.CurrentCompany.Country.Code, tariffDetail.BZ_DataModel);
		}

		public void TestBZ_DataModel_ReportErrorWhenUpdated() =>
			DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<CusLineTariffDetail>(Factory);

		public void TestBZ_DataModel_CanSaveTwice() =>
			DataModelTestHelper.RunDataModelTest_CanSaveTwice<CusLineTariffDetail>(Factory);

		public void TestParent_JobComInvoiceLine()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			AssertEquals(invoiceLine, tariffDetail.Parent);
			AssertEquals(tariffDetail.InvoiceLine, invoiceLine);
			AssertEquals(tariffDetail.PartPivot, null);
		}

		public void TestParent_CusClassPartPivot()
		{
			var cusClassPartPivot = Factory.New<BaseCusClassPartPivot>();
			var tariffDetail = cusClassPartPivot.CusLineTariffDetails.AddNew();
			AssertEquals(cusClassPartPivot, tariffDetail.Parent);
			AssertEquals(tariffDetail.PartPivot, cusClassPartPivot);
			AssertEquals(tariffDetail.InvoiceLine, null);
		}

		public void TestParent_JobComInvoiceLine_NoExceptionFromAccessingAfterParentIsDeleted()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			_ = tariffDetail.InvoiceLine;
			_ = tariffDetail.Parent;

			AssertNoExceptionThrown(() =>
			{
				invoiceLine.Delete();
				Assert(!invoiceLine.SupportsAdditionalTariffs || tariffDetail.IsDeleted);
				_ = tariffDetail.InvoiceLine?.JI_Tariff;
				_ = tariffDetail.Parent?.Tariff;
			});
		}

		public void TestParent_CusClassPartPivotNoExceptionFromAccessingAfterParentIsDeleted()
		{
			var cusClassPartPivot = Factory.New<BaseCusClassPartPivot>();
			var tariffDetail = cusClassPartPivot.CusLineTariffDetails.AddNew();
			_ = tariffDetail.PartPivot;
			_ = tariffDetail.Parent;

			AssertNoExceptionThrown(() =>
			{
				cusClassPartPivot.Delete();
				Assert(!cusClassPartPivot.SupportsAdditionalTariffs || tariffDetail.IsDeleted);
				_ = tariffDetail.PartPivot?.CI_TariffNum;
				_ = tariffDetail.Parent?.Tariff;
			});
		}

		public void TestUniversalTariff()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
				var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Australia, "123");

				Factory.Save();
				var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Australia, tariffType.PK, "08091998", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

				tariffDetail.BZ_Tariff = "08091998";
				AssertEquals(null, tariffDetail.UniversalTariff);

				tariffDetail.BZ_Type = "123";
				AssertEquals(tariff, tariffDetail.UniversalTariff);
			}
		}

		public void TestUniversalTariffType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
				var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Australia, "123");
				Factory.Save();

				var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Australia, tariffType.PK, "08091998", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

				tariffDetail.BZ_Type = "123";
				AssertEquals(tariffType, tariffDetail.UniversalTariffType);
			}
		}

		public void TestEffectiveAssessmentDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var loneTariffDetail = Factory.New<CusLineTariffDetail>();
				AssertEquals(ZDateTime.Today, loneTariffDetail.EffectiveAssessmentDate);

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();

				declaration.JE_ExportDate = ZDateTime.Today.AddDays(3);
				AssertEquals(ZDateTime.Today.AddDays(3), tariffDetail.EffectiveAssessmentDate);
			}
		}

		public void TestCustomsCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Chad))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var loneTariffDetail = Factory.New<CusLineTariffDetail>();
				AssertEquals(Core.Constants.CountryCodes.Chad, loneTariffDetail.CustomsCountryCode);

				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Chile;
				var branch = Factory.New<GlbBranch>();
				branch.GB_GC = company.PK;
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_GB = branch.PK;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();

				AssertEquals(Core.Constants.CountryCodes.Chile, tariffDetail.CustomsCountryCode);
			}
		}

		public void TestBZ_Qty1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var loneTariffDetail = Factory.New<CusLineTariffDetail>();
			loneTariffDetail.BZ_Qty1 = 1.2345m;

			AssertEquals(4, loneTariffDetail.BZ_Qty1.DecimalPlaces);
		}
	}

	[TestedType(typeof(CusLineTariffDetail))]
	public abstract class CusLineTariffDetailAbstractTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAdditionalDutiesTariffTypeList()
		{
			var tariffDetail = (CusLineTariffDetail)GetNewBusinessObject();

			AssertSame(tariffDetail.GetAdditionalDutiesTariffTypeList(), tariffDetail.GetAdditionalDutiesTariffTypeList());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return (CusLineTariffDetail)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			return invoiceLine.CusLineTariffDetails.AddNew();
		}

		#endregion
	}
}
