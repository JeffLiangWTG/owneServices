using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(PackLinesFilterBusinessObject))]
	class PackLinesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PackLinesFilterBusinessObject();
		}

		#region Filters

		public void TestPackLineIDFilter()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var packLine1 = Factory.NewWithValidTestData<ForwardingPackLine>();
			packLine1.JL_PackLineId = "ABC";
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			packLine1.JL_JS = shipment.PK;

			var packLine2 = Factory.NewWithValidTestData<ForwardingPackLine>();
			packLine2.JL_PackLineId = "DEF";
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			packLine2.JL_JS = shipment.PK;

			var packLine3 = Factory.NewWithValidTestData<ForwardingPackLine>();
			packLine3.JL_PackLineId = "GHI";
			packLine3.JL_FreightMode = FreightConstants.OuterPackType;
			packLine3.JL_JS = shipment.PK;

			var packLine4 = Factory.NewWithValidTestData<ForwardingPackLine>();
			packLine4.JL_PackLineId = "JKL";
			packLine4.JL_FreightMode = FreightConstants.OuterPackType;
			packLine4.JL_JS = shipment.PK;

			var filter = new PackLinesFilterBusinessObject();
			var packLineIDFilter = (ModuleTextFilter)filter["Pack Line ID"];
			packLineIDFilter.Property = "ABC";
			packLineIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			packLineIDFilter.IsActive = true;

			Factory.Save();

			var collection = new ParentShipmentOuterPackLineCollection(Factory, shipment);
			collection.AdditionalFilter = filter.Filter;

			AssertEquals("1 Pack Line Loaded", 1, collection.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingPackLine[] { packLine1 }, collection);
		}

		#endregion
	}
}
