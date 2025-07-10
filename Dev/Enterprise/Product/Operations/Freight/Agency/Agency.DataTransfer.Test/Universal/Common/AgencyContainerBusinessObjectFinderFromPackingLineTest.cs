using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AgencyContainerBusinessObjectFinderFromPackingLine<>))]
	internal class AgencyContainerBusinessObjectFinderFromPackingLineTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			var links = new LinksManager();
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ContainerNumber = "AAA";
			var container1 = Factory.New<AgencyShipmentContainer>();
			container1.JC_ContainerNum = "111";
			links.AddLink(LinksManager.LinkType.Container, 1, container1);
			var container2 = Factory.New<AgencyShipmentContainer>();
			container2.JC_ContainerNum = "222";
			links.AddLink(LinksManager.LinkType.Container, 2, container2);
			var container3 = Factory.New<AgencyShipmentContainer>();
			container3.JC_ContainerNum = "333";
			links.AddLink(LinksManager.LinkType.Container, 3, container3);
			var finder = new AgencyContainerBusinessObjectFinderFromPackingLine<AgencyShipmentContainer>(dataObject, links);
			AssertEquals(null, finder.Find(new[] { container1, container2, container3 }));
			dataObject.ContainerLink = 1;
			finder = new AgencyContainerBusinessObjectFinderFromPackingLine<AgencyShipmentContainer>(dataObject, links);
			AssertEquals(container1, finder.Find(new[] { container1, container2, container3 }));
			dataObject.ContainerLink = 2;
			finder = new AgencyContainerBusinessObjectFinderFromPackingLine<AgencyShipmentContainer>(dataObject, links);
			AssertEquals(container2, finder.Find(new[] { container1, container2, container3 }));
			dataObject.ContainerLink = 7;
			finder = new AgencyContainerBusinessObjectFinderFromPackingLine<AgencyShipmentContainer>(dataObject, links);
			AssertEquals(null, finder.Find(new[] { container1, container2, container3 }));
		}

		public void TestFindByContainerNumber_NoLink()
		{
			var links = new LinksManager();
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ContainerNumber = "AAA";
			var container1 = Factory.New<AgencyShipmentContainer>();
			container1.JC_ContainerNum = "111";
			var container2 = Factory.New<AgencyShipmentContainer>();
			container2.JC_ContainerNum = "222";
			var container3 = Factory.New<AgencyShipmentContainer>();
			container3.JC_ContainerNum = "333";
			var finder = new AgencyContainerBusinessObjectFinderFromPackingLine<AgencyShipmentContainer>(dataObject, links);
			AssertEquals(null, finder.Find(new[] { container1, container2, container3 }));
			dataObject.ReferenceNumber = "222";
			finder = new AgencyContainerBusinessObjectFinderFromPackingLine<AgencyShipmentContainer>(dataObject, links);
			AssertEquals(container2, finder.Find(new[] { container1, container2, container3 }));
		}

		public void TestFindByContainerNumber_NoLink_NoNumber()
		{
			var links = new LinksManager();
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			var container1 = Factory.New<AgencyShipmentContainer>();
			container1.JC_ContainerNum = "111";
			var container2 = Factory.New<AgencyShipmentContainer>();
			container2.JC_ContainerNum = "222";
			var container3 = Factory.New<AgencyShipmentContainer>();
			container3.JC_ContainerNum = "333";
			dataObject.ContainerNumber = ZString.Empty;
			var finder = new AgencyContainerBusinessObjectFinderFromPackingLine<AgencyShipmentContainer>(dataObject, links);
			AssertEquals(null, finder.Find(new[] { container1, container2, container3 }));
			dataObject.ContainerNumber = null;
			finder = new AgencyContainerBusinessObjectFinderFromPackingLine<AgencyShipmentContainer>(dataObject, links);
			AssertEquals(null, finder.Find(new[] { container1, container2, container3 }));
		}
	}
}
