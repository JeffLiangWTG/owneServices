using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class CusEngineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestFuelTypeList()
	{
		var list = Lookups.FuelTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "B, BE, BG, E, G, O, OE, W, ZZ", list.CodesAsString);
			AssertSame("Cached", list, Lookups.FuelTypeList);
		});
	}

	CusEngineLookups Lookups
	{
		get
		{
			if (lookups == null)
			{
				var dec = Factory.New<JobDeclaration>();
				var vehicle = (CusVehicle)dec.Invoices.AddNew().InvoiceLines.AddNew().Vehicles.AddNew();
				lookups = vehicle.Engine.Lookups;
			}
			return lookups;
		}
	}
	CusEngineLookups lookups;
}
