using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business.Rating;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVRatingAdapterProviderTest : TestCaseWithFactory
	{
		public void TestGetAdapters_IncludeHVLVRatingAdapterProviderTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var ratingAdaptersProvider = new HVLVRatingAdapterProvider(shipment);
			var ratingAdapters = ratingAdaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue);
			AssertEquals("HVLVRatingAdapterProvider must content one HVLVShipmentRatingAdapter", 1, ratingAdapters.Where((adapter) => adapter.GetType() == typeof(HVLVShipmentRatingAdapter)).Count());
		}

		public void TestGetAdapters_IncludesOnlyLoadedItems()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var otherShipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			otherShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header1 = HVLVConsignmentHeader.GetOrCreate(shipment);
			var header2 = HVLVConsignmentHeader.GetOrCreate(otherShipment);

			var consignment1 = header1.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();

			var consignment2 = header1.Consignments.AddNew();
			var item2 = consignment2.Items.AddNew();
			item2.HVI_JS_LoadedOnShipment = otherShipment.PK;
			var item3 = consignment2.Items.AddNew();
			item3.HVI_JS_LoadedOnShipment = otherShipment.PK;

			var consignment3 = header2.Consignments.AddNew();
			var item4 = consignment3.Items.AddNew();
			item4.HVI_JS_LoadedOnShipment = otherShipment.PK;
			var item5 = consignment3.Items.AddNew();
			item5.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			var adaptersProvider = new HVLVRatingAdapterProvider(loadedShipment);

			AssertContainsExactElementsInAnyOrder(
				"Should contain an adapter for all loaded items on shipment1.",
				new[] { item1.PK, item5.PK },
				adaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue)
					.OfType<HVLVItemRatingAdapter>()
					.Select(item => item.Parent.PK));
		}

		public void TestGetAdapters_OnlyActiveItems()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = HVLVConsignmentHeader.GetOrCreate(shipment);
			var consignment1 = header.Consignments.AddNew();
			var item1 = consignment1.Items.AddNew();
			item1.HVI_IsActive = false;

			var consignment2 = header.Consignments.AddNew();
			var item2 = consignment2.Items.AddNew();
			var item3 = consignment2.Items.AddNew();
			var item4 = consignment2.Items.AddNew();
			item4.HVI_IsActive = false;

			var adaptersProvider = new HVLVRatingAdapterProvider(shipment);
			var otherShipmentAdaptersProvider = new HVLVRatingAdapterProvider(shipment);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				"Should contain an adapter for item2 and item3 since other items are inactive.",
				new[] { item2.PK, item3.PK },
				adaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue)
					.OfType<HVLVItemRatingAdapter>()
					.Select(item => item.Parent.PK));
		}

		public void TestGetAdapters_ConstantDBHitForHVLVConsignmentAndHVLVItem()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = HVLVConsignmentHeader.GetOrCreate(shipment);
			var consignment = header.Consignments.AddNew();
			consignment.Items.AddNew();
			consignment.Items.AddNew();
			consignment.Items.AddNew();

			var consignment2 = header.Consignments.AddNew();
			consignment2.Items.AddNew();

			var consignment3 = header.Consignments.AddNew();
			consignment3.Items.AddNew();

			var consignment4 = header.Consignments.AddNew();
			consignment4.Items.AddNew();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedShipment = newFactory.LoadTop1<ForwardingShipment>(new ZQuery());
			var adaptersProvider = new HVLVRatingAdapterProvider(loadedShipment);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ HVLVConsignmentSchema.Constants.TableName, 1 },
				{ HVLVItemSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var adapters = adaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue);
			}
		}

		public void TestHasAdapterForServiceIsFalse()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var ratingAdaptersProvider = new HVLVRatingAdapterProvider(shipment);
			var result = ratingAdaptersProvider.IsAdapterAvailable(AutoRateOptions.AutorateRevenue, ChargeCodeGroupList.Codes.CFSShipment, ChargeCodeSubGroupList.Storage);

			AssertEquals("HVLV Rating Adapter Provider doesn't support CFSShipment. The expected value is false", false, result);
		}
	}
}
