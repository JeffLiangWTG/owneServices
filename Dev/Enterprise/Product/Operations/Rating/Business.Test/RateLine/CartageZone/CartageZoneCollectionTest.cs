using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CartageZoneCollection))]
	public class CartageZoneCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CartageZoneCollection>
	{
		public void TestLoad()
		{
			var providerWithNoSupplier = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "GZ1", "GZ2", "GZ3", "GZ4" });
			var supplier = Helper.NewOrgHeader();
			var providerWithSupplier = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia, zoneNames: new ZString[] { "CZ1", "CZ2", "CZ3" });

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entryWithNoSupplier = rate.AddRateEntry("DST", "AIR", "", "AUSYD");
			var entryWithSupplier = rate.AddRateEntry("DST", "AIR", "", "AUSYD");
			entryWithSupplier.TI_OH_Supplier = supplier.PK;

			var collection = new CartageZoneCollection(entryWithNoSupplier.AddRateLine("DCART", FlatCalculator.Code));
			collection.Load();

			AssertCartageZoneCollection(collection, ZGuid.Empty, providerWithNoSupplier.Zones[0].PK, providerWithNoSupplier.Zones[1].PK, providerWithNoSupplier.Zones[2].PK, providerWithNoSupplier.Zones[3].PK);

			var lineWithSupplier = entryWithSupplier.AddRateLine("DCART", FlatCalculator.Code);

			Factory.Save();

			collection = new CartageZoneCollection(lineWithSupplier);
			collection.Load();

			AssertCartageZoneCollection(collection, ZGuid.Empty, providerWithSupplier.Zones[0].PK, providerWithSupplier.Zones[1].PK, providerWithSupplier.Zones[2].PK);
			AssertEquals(4, collection.Count);

			collection.Load();
			AssertEquals(4, collection.Count);
		}

		public override void TestDelete()
		{
			var provider = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "GZ1", "GZ2", "GZ3" });

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("DST", "AIR", "", "AUSYD").AddRateLine("DCART", FlatCalculator.Code);

			var itemMin = line.Calculator.AddRateLineItemWithZone("MIN", 0m, 100m, ZGuid.Empty);
			var item1 = line.Calculator.AddRateLineItemWithZone("UNT", 0m, 1.1m, provider.Zones[0].PK);
			var item2 = line.Calculator.AddRateLineItemWithZone("UNT", 0m, 1.2m, provider.Zones[1].PK);
			var item3 = line.Calculator.AddRateLineItemWithZone("UNT", 0m, 1.3m, provider.Zones[2].PK);

			Factory.Save();

			var collection = new CartageZoneCollection(line);
			collection.Load();

			AssertCartageZoneCollection(collection, ZGuid.Empty, provider.Zones[0].PK, provider.Zones[1].PK, provider.Zones[2].PK);

			for (var i = collection.Count - 1; i >= 0; i--)
			{
				collection.RemoveAndDelete(collection[i]);
			}
			AssertCartageZoneCollection(collection, ZGuid.Empty, provider.Zones[0].PK, provider.Zones[1].PK, provider.Zones[2].PK);

			Assert(!itemMin.IsDeleted);
			Assert(!item1.IsDeleted);
			Assert(!item2.IsDeleted);
			Assert(!item3.IsDeleted);
		}

		void AssertCartageZoneCollection(CartageZoneCollection collection, params ZGuid[] zones)
		{
			AssertEquals(zones.Length, collection.Count);

			foreach (var zonePK in zones)
			{
				Assert(zonePK + " should be found in the collection.", collection.ContainsZone(zonePK));
			}
		}

		#region Implementation

		protected override CartageZoneCollection GetCollectionToTest()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("DST", "AIR", "", "AUSYD").AddRateLine("DCART", FlatCalculator.Code);
			return new CartageZoneCollection(line);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CartageZone(Factory);
		}

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}
