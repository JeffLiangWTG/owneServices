using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusVehicleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGearsList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(GearsList.Codes._0, GearsList.Descriptions._0),
				new CodeDescriptionPair(GearsList.Codes._1, GearsList.Descriptions._1),
				new CodeDescriptionPair(GearsList.Codes._2, GearsList.Descriptions._2),
				new CodeDescriptionPair(GearsList.Codes._3, GearsList.Descriptions._3),
			}, Lookups.GearsList);
		}

		public void TestGearsDescriptionList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(GearsList.Descriptions._1, GearsList.Descriptions._1),
				new CodeDescriptionPair(GearsList.Descriptions._2, GearsList.Descriptions._2),
				new CodeDescriptionPair(GearsList.Descriptions._3, GearsList.Descriptions._3),
			}, Lookups.GearsDescriptionList);
		}

		CusVehicleLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					var vehicle = Factory.New<CusVehicle>();
					lookups = new CusVehicleLookups(vehicle);
				}
				return lookups;
			}
		}
		CusVehicleLookups lookups;
	}
}
