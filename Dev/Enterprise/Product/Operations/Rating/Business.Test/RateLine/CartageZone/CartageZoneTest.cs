using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CartageZone))]
	public class CartageZoneTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParent()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("DST", "AIR", "", "AUSYD").AddRateLine("DCART", CartageCalculator.Code, "KG");
			var collection = new CartageZoneCollection(line);
			AssertEquals(line, collection.AddNew().Parent);
		}

		public void TestZoneRateLineItems()
		{
			var provider = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "GZ1", "GZ2", "GZ3" });
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("DST", "AIR", "", "AUSYD").AddRateLine("DCART", CartageCalculator.Code, "KG");
			var itemMin = line.Calculator.AddRateLineItemWithZone("MIN", 0m, 100m, ZGuid.Empty);
			var itemBas = line.Calculator.AddRateLineItemWithZone("BAS", 0m, 50m, ZGuid.Empty);
			var item1a = line.Calculator.AddRateLineItemWithZone("UNT", 0m, 1.1m, provider.Zones[0].PK);
			var item3a = line.Calculator.AddRateLineItemWithZone("-", 50m, 1.4m, provider.Zones[2].PK);
			var item3b = line.Calculator.AddRateLineItemWithZone("+", 50m, 1.5m, provider.Zones[2].PK);

			var collection = new CartageZoneCollection(line);
			collection.Load();

			AssertEquals(4, collection.Count);

			AssertCartageZone(collection[0], ZGuid.Empty, itemMin, itemBas);
			AssertCartageZone(collection[1], provider.Zones[0].PK, item1a);
			AssertCartageZone(collection[2], provider.Zones[1].PK);
			AssertCartageZone(collection[3], provider.Zones[2].PK, item3a, item3b);

			AssertEquals(provider.Zones[1].PK, collection[2].ZoneRateLineItems.AddNew().TM_TZ_DomesticZone);

			collection[3].Delete();
			Assert(!itemMin.IsDeleted);
			Assert(!itemBas.IsDeleted);
			Assert(!item1a.IsDeleted);
			Assert(item3a.IsDeleted);
			Assert(item3b.IsDeleted);
		}

		void AssertCartageZone(CartageZone cartageZone, ZGuid zonePK, params RateLineItem[] items)
		{
			AssertEquals(zonePK, cartageZone.ZonePK);
			AssertEquals(items.Length, cartageZone.ZoneRateLineItems.Count);

			foreach (var item in items)
			{
				Assert(cartageZone.ZoneRateLineItems.Contains(item.PK));
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var line = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("DST", "AIR", "", "AUSYD").AddRateLine("DCART", FlatCalculator.Code);
			var collection = new CartageZoneCollection(line);

			return collection.AddNew();
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
