using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AgencyContainerCollectionReader<AgencyShipmentContainer>))]
	internal class AgencyContainerCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public void TestReadIntoCollection_VerifiedGrossContainerWeight()
		{
			var links = new LinksManager();
			var logger = new TestErrorLogger();
			var booking = Factory.New<AgencyBooking>();
			var container1 = booking.RealContainers.AddNew();
			container1.JC_ContainerNum = "AAA";
			container1.JC_SealNum = "0000";
			var container2 = booking.BookedContainers.AddNew();
			container2.JC_ContainerNum = "BBB";
			container2.JC_SealNum = "1111";
			var container3 = booking.BookedContainers.AddNew();
			container3.JC_ContainerNum = "DDD";
			container3.JC_SealNum = "2222";
			Factory.SaveForTesting();
			var containerDataObject1 = CreateDataObject("AAA", 1);
			containerDataObject1.Seal = "5555";
			var containerDataObject2 = CreateDataObject("BBB", 2);
			containerDataObject2.Seal = "6666";
			var containerDataObject3 = CreateDataObject("CCC", 3);
			containerDataObject3.Seal = "7777";
			var containerInfo = new AgencyContainersInfo(booking, true);
			var containerList = new DataObjectList<Container> { containerDataObject1, containerDataObject2, containerDataObject3 };
			containerList.Content = CollectionContent.Partial;
			var reader = new AgencyContainerCollectionReader<AgencyShipmentContainer>(containerList, logger, new UniversalObjectFactory(), links, containerInfo);
			reader.ReadIntoCollection();
			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching AgencyBookingContainer.
Information - Populating AgencyBookingContainer...
Information - Successfully loaded matching AgencyBookingContainer.
Information - Populating AgencyBookingContainer...
Information - No matching AgencyBookingContainer found, creating new AgencyBookingContainer.
Information - Populating AgencyBookingContainer...
".Trim(), logger.Logs);
			var realContainers = FormatContainers(booking.RealContainers.ToArray<AgencyShipmentContainer>());
			var bookedContainers = FormatContainers(booking.BookedContainers.ToArray<AgencyShipmentContainer>());
			AssertContainsExactElementsInAnyOrder(new[] { "AAA|5555", "CCC|7777" }, realContainers);
			AssertContainsExactElementsInAnyOrder(new[] { "BBB|6666", "DDD|2222" }, bookedContainers);
		}

		public override void TestReadIntoCollection()
		{
			var links = new LinksManager();
			var billOfLading = Factory.New<BillOfLading>();
			var container1 = billOfLading.ShippingContainers.AddNew();
			container1.JC_ContainerNum = "AAA";
			container1.JC_SealNum = "SEAL";
			var container2 = billOfLading.ShippingContainers.AddNew();
			container2.JC_ContainerNum = "BBB";
			var containerDataObject1 = CreateDataObject(container1.JC_ContainerNum, 3);
			containerDataObject1.Seal = "ANGRYSEAL";
			var containerDataObject2 = CreateDataObject("ZZZ", 4);
			containerDataObject2.Seal = "KHERSON";
			var logger = new TestErrorLogger();
			var containerInfo = new AgencyContainersInfo(billOfLading, false);
			var reader = new AgencyContainerCollectionReader<AgencyShipmentContainer>(new DataObjectList<Container> { containerDataObject1, containerDataObject2 }, logger, new UniversalObjectFactory(), links, containerInfo);
			reader.ReadIntoCollection();
			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching BillOfLadingContainer.
Information - Populating BillOfLadingContainer...
Information - No matching BillOfLadingContainer found, creating new BillOfLadingContainer.
Information - Populating BillOfLadingContainer...
".Trim(), logger.Logs);
			var containers = billOfLading.ShippingContainers.ToArray<AgencyShipmentContainer>();
			AssertContainsExactElementsInAnyOrder(new[] { "AAA|ANGRYSEAL", "ZZZ|KHERSON" }, FormatContainers(containers));
			AssertEquals(true, links.ContainsLink(LinksManager.LinkType.Container, 3));
			AssertEquals(false, links.ContainsLink(LinksManager.LinkType.Container, 1));
			AssertEquals(true, links.ContainsLink(LinksManager.LinkType.Container, 4));
		}

		#region Implementation
		string[] FormatContainers(AgencyShipmentContainer[] containers)
		{
			return containers.Select(c => string.Format("{0}|{1}", c.JC_ContainerNum, c.JC_SealNum)).ToArray();
		}

		Container CreateDataObject(string containerNumber, ZInt? link)
		{
			var dataObject = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.ContainerNumber = containerNumber;
			dataObject.Link = link;
			return dataObject;
		}
		#endregion
	}
}
