using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class PickFaceViewTestData : Assertion
	{
		public PickFaceViewTestData(BusinessObjectFactory factory,
			int warehouses = 1,
			int clients = 1,
			int locationsPerWarehouse = 1,
			int maxProductsPerLocation = 1)
		{
			const short whsLevels = 1;
			var helper = new WhsTestHelperFunctionsEnv(factory);
			var whsList = new List<WhsWarehouse>();
			var clientList = new List<OrgHeader>();
			var partList = new List<OrgSupplierPart>();
			var locDict = new Dictionary<WhsWarehouse, IReadOnlyList<WhsLocation>>();

			var fixedLocation = helper.CreateLocationType("FIX", "FixLocation", false, maxProductsPerLocation, LocationClasses.Codes.FIX);

			for (int i = 0; i < warehouses; i++)
			{
				var warehouse = helper.CreateWarehouse(i.ToString(), "A", (short)locationsPerWarehouse, whsLevels);
				whsList.Add(warehouse);
			}

			for (int i = 0; i < clients; i++)
			{
				var name = $"{i}{i}{i}";
				var client = helper.CreateClient(name, name);
				clientList.Add(client);

				var part = helper.CreateProduct(client, $"P{i}");
				partList.Add(part);
			}

			factory.Save();

			for (int i = 0; i < warehouses; i++)
			{
				var locations = new List<WhsLocation>();

				for (int j = 0; j < locationsPerWarehouse; j++)
				{
					var location = WhsLocation.FindLocation(factory, GetLocationString(locationsPerWarehouse, j), whsList[i].PK);
					AssertNotNull("Precondition: need to find locations.", location);
					location.WLV_WLT_LocationType = fixedLocation.PK;
					locations.Add(location);
				}
				locDict[whsList[i]] = locations;
			}

			Warehouses = whsList;
			Clients = clientList;
			Parts = partList;
			Locations = locDict;
		}

		static string GetLocationString(int numberOfColumns, int column)
		{
			Assert("Precondition: which must be less than or equal to cols", column <= numberOfColumns);
			if (numberOfColumns == 1)
			{
				return "A";
			}
			return $"A-{column + 1}";
		}

		public readonly IReadOnlyList<WhsWarehouse> Warehouses;
		public readonly IReadOnlyList<OrgHeader> Clients;
		public readonly IReadOnlyList<OrgSupplierPart> Parts;
		public readonly IReadOnlyDictionary<WhsWarehouse, IReadOnlyList<WhsLocation>> Locations;
	}
}
