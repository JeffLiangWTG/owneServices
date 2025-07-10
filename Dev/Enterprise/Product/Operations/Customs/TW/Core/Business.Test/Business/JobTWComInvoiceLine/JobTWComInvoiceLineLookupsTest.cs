using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobTWComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCategoryCodesOfCAAAircraftPartsList()
		{
			var list = lookups.CategoryCodesOfCAAAircraftPartsList;
			AssertSame(TWRefCusCodeListTypes.GetCategoryCodesOfCAAAircraftParts(Factory), list);
		}

		public void TestCAAAircraftPartsCodesList()
		{
			tWComInvoiceLine.TWL_AircraftPartsCategory = "1";
			var list = lookups.CAAAircraftPartsCodesList;
			AssertSame(TWRefCusCodeListTypes.GetCAAAircraftPartsCodes(Factory, "1"), list);
		}

		protected override void SetUp()
		{
			base.SetUp();

			tWComInvoiceLine = Factory.New<JobTWComInvoiceLine>();
			lookups = new JobTWComInvoiceLineLookups(tWComInvoiceLine);
		}

		JobTWComInvoiceLine tWComInvoiceLine;
		JobTWComInvoiceLineLookups lookups;
	}
}
