using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USFDAAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUS_FDAProductNumberList()
		{
			var testHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType,
			"24DCS18", "ALFALFA BEANS (SEEDS), JUICE OR DRINK;GLASS;ULTRAPASTEURIZED", CargoWise.Types.ZDateTime.MinSmallDateTimeValue, CargoWise.Types.ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var fda = invoiceLine.FDAs.AddNew();
			var productList = fda.AddInfoLookups.US_FDAProductNumberList;
			productList.Load();
			AssertEquals("24DCS18", productList[0].ZZD_Code);
		}

		public void TestUSCCountryList()
		{
			AssertNotNull(Lookups.USCCountryList);
		}

		public void TestUS_CargoStorageCodeList()
		{
			AssertNotNull(Lookups.US_CargoStorageCodeList);
		}

		public void TestFDABaseUQs()
		{
			AssertNotNull(Lookups.FDABaseUQs.Count);
		}

		public void TestFDAUQs()
		{
			AssertNotNull(Lookups.FDAUQs.Count);
			Assert(!Lookups.FDAUQs.ContainsCode(FDAUQList.Codes.MG));
		}

		public void TestUS_CylindricalRectangularList()
		{
			AssertNotNull(Lookups.US_CylindricalRectangularList);
			AssertNotNull(Lookups.DimensionUQs);
		}

		public void TestConsignors()
		{
			AssertNotNull(Lookups.Consignors);
			AssertEquals(typeof(ConsignorCollection), Lookups.Consignors.GetType());
		}

		USFDAAddInfoLookups Lookups
		{
			get { return FDA.AddInfoLookups; }
		}

		FDA FDA
		{
			get { return fda ?? (fda = Factory.New<FDA>()); }
		}
		FDA fda;
	}
}
