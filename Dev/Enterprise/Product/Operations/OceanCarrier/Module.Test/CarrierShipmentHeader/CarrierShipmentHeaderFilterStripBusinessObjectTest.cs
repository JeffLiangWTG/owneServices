using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Module.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderFilterStripBusinessObject))]
	sealed class CarrierShipmenHeadertFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestSearchCarrierShipmentByReferenceNumber

		public void TestSearchCarrierShipmentByReferenceNumber()
		{
			var carrierShipment1 = Factory.New<Business.CarrierShipmentHeader>();
			carrierShipment1.CSH_CarrierShipmentReference = "CSBNE001";

			var carrierShipment2 = Factory.New<Business.CarrierShipmentHeader>();
			carrierShipment2.CSH_CarrierShipmentReference = "CSHAM002";

			var carrierShipment3 = Factory.New<Business.CarrierShipmentHeader>();
			carrierShipment3.CSH_CarrierShipmentReference = "CSHON003";

			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();

			var filter = (ModuleFountainFilter)filters.ModuleFilters[CarrierShipmentHeaderFilterStripBusinessObject.Schema.ReferenceNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "CSBNE";
			AssertContainsExactElementsInAnyOrder($"Must only return Carrier Shipments with Reference starting with {filter.Property}",
				expected: new[] { carrierShipment1 },
				actual: Factory.Load<Business.CarrierShipmentHeader>(filters.Filter));

			filter.Property = "CSH";
			AssertContainsExactElementsInAnyOrder($"Must only return Carrier Shipments with Reference starting with {filter.Property}",
				expected: new[] { carrierShipment2, carrierShipment3 },
				actual: Factory.Load<Business.CarrierShipmentHeader>(filters.Filter));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "CSHAM002";
			AssertContainsExactElementsInAnyOrder($"Must return the Carrier Shipments matching exactly Reference {filter.Property}",
				expected: new[] { carrierShipment2 },
				actual: Factory.Load<Business.CarrierShipmentHeader>(filters.Filter));

			filter.Property = "";
			AssertContainsExactElementsInAnyOrder("Must return all Carrier Shipments",
				expected: new[] { carrierShipment1, carrierShipment2, carrierShipment3 },
				actual: Factory.Load<Business.CarrierShipmentHeader>(filters.Filter));
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CarrierShipmentHeaderFilterStripBusinessObject();

		#endregion
	}
}
