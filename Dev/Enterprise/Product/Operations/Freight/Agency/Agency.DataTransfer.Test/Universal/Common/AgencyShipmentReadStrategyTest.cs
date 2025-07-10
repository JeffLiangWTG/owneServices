using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyShipmentReadStrategyTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReadContainers_AgencyBooking()
		{
			var booking = Factory.New<AgencyBooking>();
			var containers = new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" }, Seal = "SEAL1" }, new Container { ContainerNumber = "BBB", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" }, Seal = "SEAL2" } };
			IAgencyShipmentReadStrategy<AgencyBooking> strategy = new AgencyShipmentReadStrategy<AgencyBooking, AgencyBookingContainer, AgencyBookingPackLine>(new TestErrorLogger(), Factory, null);
			strategy.ReadContainers(booking, containers);
			AssertContainsExactElementsInAnyOrder("", new[] { "AAA|SEAL1", "BBB|SEAL2" }, FormatContainers(booking));
		}

		public void TestReadContainers_BillOfLading()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var containers = new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" }, Seal = "SEAL1" }, new Container { ContainerNumber = "BBB", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" }, Seal = "SEAL2" } };
			IAgencyShipmentReadStrategy<BillOfLading> strategy = new AgencyShipmentReadStrategy<BillOfLading, BillOfLadingContainer, BillOfLadingPackLine>(new TestErrorLogger(), Factory, null);
			strategy.ReadContainers(billOfLading, containers);
			AssertContainsExactElementsInAnyOrder("", new[] { "AAA|SEAL1", "BBB|SEAL2" }, FormatContainers(billOfLading));
		}

		public void TestReadPacklingLines_AgencyBooking()
		{
			var booking = Factory.New<AgencyBooking>();
			var packingLines = new List<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cups" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cheese" } };
			IAgencyShipmentReadStrategy<AgencyBooking> strategy = new AgencyShipmentReadStrategy<AgencyBooking, AgencyBookingContainer, AgencyBookingPackLine>(new TestErrorLogger(), Factory, null);
			strategy.ReadPackingLines(booking, packingLines);
			AssertContainsExactElementsInAnyOrder("", new[] { "cups", "cheese" }, FormatPackLines(booking));
		}

		public void TestReadPacklingLines_BillOfLading()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var packingLines = new List<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cups" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "AAA", GoodsDescription = "cheese" } };
			IAgencyShipmentReadStrategy<BillOfLading> strategy = new AgencyShipmentReadStrategy<BillOfLading, BillOfLadingContainer, BillOfLadingPackLine>(new TestErrorLogger(), Factory, null);
			strategy.ReadPackingLines(billOfLading, packingLines);
			AssertContainsExactElementsInAnyOrder("", new[] { "cups", "cheese" }, FormatPackLines(billOfLading));
		}

		IEnumerable<string> FormatContainers(AgencyShipment agencyShipment)
		{
			return agencyShipment.ShippingContainers.Cast<AgencyShipmentContainer>().Select(c => string.Format("{0}|{1}", c.JC_ContainerNum, c.JC_SealNum)).ToArray();
		}

		IEnumerable<string> FormatPackLines(AgencyShipment agencyShipment)
		{
			return agencyShipment.OuterPackLines.Cast<AgencyShipmentPackLine>().Select(p => string.Format("{0}", p.JL_Description)).ToArray();
		}
	}
}
