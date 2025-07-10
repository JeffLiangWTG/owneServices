using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetail))]
	public class CusLineTariffDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, CusLineTariffDetail.BZ_ParentTableCode);
		}

		[TestDate(2012, 1, 01)]
		public void TestBZ_Tariff()
		{
			InvoiceLine.JI_Tariff = "39269041";
			InvoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.NMB;
			CusLineTariffDetail.BZ_Tariff = "ANECON001";
			AssertEquals("ANECON001", CusLineTariffDetail.BZ_Tariff);
			AssertEquals("NMB", CusLineTariffDetail.BZ_UQ1);
		}

		[TestDate(2012, 1, 01)]
		public void TestBZ_Tariff1()
		{
			InvoiceLine.JI_Tariff = "03011110";
			InvoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.NMB;
			CusLineTariffDetail.BZ_Tariff = "FFO0ZX1FFRY";
			AssertEquals("PCS", CusLineTariffDetail.BZ_UQ1);
		}

		[TestDate(2012, 1, 01)]
		public void TestBZ_Tariff1Additional()
		{
			InvoiceLine.JI_Tariff = "84212910";
			InvoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.NMB;
			CusLineTariffDetail.BZ_Tariff = "HSAMDA01110";
			AssertEquals("NMB", CusLineTariffDetail.BZ_UQ1);
		}

		[TestDate(2012, 1, 01)]
		public void TestBZ_TariffQty()
		{
			var testValue = new ZDecimal(10);
			InvoiceLine.JI_Tariff = "01022910";
			InvoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.NMB;
			InvoiceLine.JI_InvoiceQuantity = testValue;
			CusLineTariffDetail.BZ_Tariff = "VMA0OX";
			AssertEquals("Should default qty", testValue, CusLineTariffDetail.BZ_Qty1);
		}

		[TestDate(2012, 1, 01)]
		public void TestBZ_TariffMISC()
		{
			InvoiceLine.JI_Tariff = "49011000";
			InvoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.NMB;
			CusLineTariffDetail.BZ_Tariff = "MISC";
			AssertEquals("NMB", CusLineTariffDetail.BZ_UQ1);
		}

		[TestDate(2012, 1, 01)]
		public void TestTariffCommodity()
		{
			InvoiceLine.JI_Tariff = "04015090";
			AssertEquals(1, CusLineTariffDetail.Lookups.TariffCommodities.Count);
		}

		public void TestGetNewLookups()
		{
			CusLineTariffDetail cusLineTariffDetail = Factory.New<CusLineTariffDetail>();
			Assert(cusLineTariffDetail.Lookups is CusLineTariffDetailLookups);
		}

		public void TestGetNewValidation()
		{
			CusLineTariffDetail cusLineTariffDetail = Factory.New<CusLineTariffDetail>();
			Assert(cusLineTariffDetail.Validation is CusLineTariffDetailValidation);
		}

		public void TestIsChemicalPurityRequired()
		{
			CusLineTariffDetail.BZ_Tariff = "HSACOSSMP";
			AssertEquals(false, CusLineTariffDetail.IsChemicalPurityRequired());
			CusLineTariffDetail.BZ_Tariff = "S3AC02";
			AssertEquals(true, CusLineTariffDetail.IsChemicalPurityRequired());
		}

		public void TestIsHSA_CPM()
		{
			CusLineTariffDetail.BZ_Tariff = "HSACOSSMP";
			AssertEquals(false, CusLineTariffDetail.IsHSA_CPM());
			CusLineTariffDetail.BZ_Tariff = "S3AC02";
			AssertEquals(false, CusLineTariffDetail.IsHSA_CPM());
			CusLineTariffDetail.BZ_Tariff = "HSACHD01000";
			AssertEquals(true, CusLineTariffDetail.IsHSA_CPM());
			CusLineTariffDetail.BZ_Tariff = "HSACHR01000";
			AssertEquals(true, CusLineTariffDetail.IsHSA_CPM());
			CusLineTariffDetail.BZ_Tariff = "HSACHP01000";
			AssertEquals(true, CusLineTariffDetail.IsHSA_CPM());
			CusLineTariffDetail.BZ_Tariff = "HSACHG01000";
			AssertEquals(true, CusLineTariffDetail.IsHSA_CPM());
			CusLineTariffDetail.BZ_Tariff = "HSACHM01000";
			AssertEquals(true, CusLineTariffDetail.IsHSA_CPM());
		}

		public void TestIsHSAProductCode()
		{
			CusLineTariffDetail.BZ_Tariff = "HSACOSSMP";
			AssertEquals(false, CusLineTariffDetail.IsHSAProductCode);
			CusLineTariffDetail.BZ_Tariff = "S3AC02";
			AssertEquals(false, CusLineTariffDetail.IsHSAProductCode);
			CusLineTariffDetail.BZ_Tariff = "HSACHR04000";
			AssertEquals(true, CusLineTariffDetail.IsHSAProductCode);
		}

		#region ICusProductCode
		public void TestProductCode()
		{
			CusLineTariffDetail.BZ_Tariff = "TEST";
			AssertEquals("TEST", CusProductCode.ProductCode);
		}

		public void TestProductCodeQty()
		{
			CusLineTariffDetail.BZ_Qty1 = 10m;
			AssertEquals(10m, CusProductCode.ProductCodeQty);
		}

		public void TestProductCodeUnitType()
		{
			CusLineTariffDetail.BZ_UQ1 = "LTR";
			AssertEquals("LTR", CusProductCode.ProductCodeUnitType);
		}

		#endregion
		#region Declaration
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
					declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		#endregion
		#region InvoiceHeader
		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		#endregion
		#region CusLineTariffDetail
		ICusProductCode CusProductCode
		{
			get
			{
				return cusLineTariffDetail;
			}
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = (JobComInvoiceLine)InvoiceHeader.InvoiceLines.AddNew());
			}
		}

		JobComInvoiceLine invoiceLine;
		CusLineTariffDetail CusLineTariffDetail
		{
			get
			{
				return cusLineTariffDetail ?? (cusLineTariffDetail = InvoiceLine.ProductCodes.AddNew());
			}
		}

		CusLineTariffDetail cusLineTariffDetail;
		#endregion
		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return CusLineTariffDetail;
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "39269041");
			helper.CreateTariffUOM(tariff1, Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.NMB);
			helper.CreateCommodity(tariff1, "ANECON001");
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "03011110");
			helper.CreateTariffUOM(tariff2, Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.NMB);
			var commodity2 = helper.CreateCommodity(tariff2, "FFO0ZX1FFRY");
			helper.CreateTariffUOM(commodity2, Constants.UnitOfMeasureTypes.StatisticalUOMType, "PCS");
			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "84212910");
			helper.CreateTariffUOM(tariff3, Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.NMB);
			helper.CreateCommodity(tariff3, "HSAMDA01110");
			var tariff4 = helper.LoadOrCreateNewTariff(tariffType, "01022910");
			helper.CreateTariffUOM(tariff4, Constants.UnitOfMeasureTypes.StatisticalUOMType, UnitOfQuantityCodeList.Codes.NMB);
			helper.CreateCommodity(tariff4, "VMA0OX");
			var tariff5 = helper.LoadOrCreateNewTariff(tariffType, "04015090");
			helper.CreateCommodity(tariff5, "123456");
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
		#endregion
	}
}
