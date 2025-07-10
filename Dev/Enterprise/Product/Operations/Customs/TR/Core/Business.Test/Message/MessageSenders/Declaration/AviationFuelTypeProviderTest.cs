using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class AviationFuelTypeProviderTest : TestCaseWithFactory
	{
		public void TestAviationFuelTypesMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
				var entryLines = declaration.EntryLines.ToArray()[0];
				var aviationFuelTypes = entryLines.AviationFuelTypes.ToArray();

				CombineAssertions("Aviation Fuel Type Provider Test", () =>
				{
					AssertEquals("TaxId", "1234567890", aviationFuelTypes[0].TaxId);
					AssertEquals("InvoiceDate", "2021-02-24", aviationFuelTypes[0].InvoiceDate);
					AssertEquals("InvoiceNumber", "5478966", aviationFuelTypes[0].InvoiceNumber);
					AssertEquals("TotalInvoiceAmount", "15450", aviationFuelTypes[0].TotalInvoiceAmount);
					AssertEquals("FuelType", "uçak yakıtı", aviationFuelTypes[0].FuelType);
				});
			}
		}
	}
}
