using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyShipmentContainerReadStrategyTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReadPacklingLines()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var container = billOfLading.ShippingContainers.AddNew();
			var packLine1 = billOfLading.OuterPackLines.AddNew();
			packLine1.JL_Description = "monkeys";
			var packingLines = new List<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cups" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cheese" } };
			IAgencyShipmentReadStrategy strategy = new AgencyShipmentContainerReadStrategy(container, new TestErrorLogger(), Factory);
			strategy.ReadPackingLines(billOfLading, packingLines);
			AssertContainsExactElementsInAnyOrder("expected bill of lading packlines", new[] { "cups", "cheese", "monkeys" }, FormatPackLines(billOfLading.OuterPackLines.Cast<AgencyShipmentPackLine>()));
			AssertContainsExactElementsInAnyOrder("expected container packlines", new[] { "cups", "cheese" }, FormatPackLines(container.PackLines.Cast<AgencyShipmentPackLine>()));
		}

		public void TestReadPacklingLines_TopLevelPacks()
		{
			var billOfLading = Factory.New<BillOfLading>();
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				billOfLading.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
				billOfLading.ShippingContainers.RemoveAndDeleteAll();
				var topLevelPack = billOfLading.ShippingContainers.AddNew();
				var packingLines = new List<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{ GoodsDescription = "lada" } };
				IAgencyShipmentReadStrategy strategy = new AgencyShipmentContainerReadStrategy(topLevelPack, new TestErrorLogger(), Factory);
				strategy.ReadPackingLines(billOfLading, packingLines);
				AssertEquals("no packlines have been created", 0, billOfLading.OuterPackLines.Count);
				AssertEquals("top level pack matched", 1, billOfLading.ShippingContainers.Count);
				AssertEquals("JC_ContainerNum", "lada", topLevelPack.JC_Description);
			}
		}

		IEnumerable<string> FormatPackLines(IEnumerable<AgencyShipmentPackLine> packingLines)
		{
			return packingLines.Select(p => string.Format("{0}", p.JL_Description)).ToArray();
		}
	}
}
