using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AgencyPackingLineCollectionReader<AgencyShipmentPackLine, AgencyShipment>))]
	internal class AgencyPackingLineCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var links = new LinksManager();
			var container1 = shipment.ShippingContainers.AddNew();
			container1.JC_ContainerNum = "AAA";
			var container2 = shipment.ShippingContainers.AddNew();
			container2.JC_ContainerNum = "BBB";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_Description = "waffles";
			packLine1.JL_PackageCount = 33;
			packLine1.SetContainer(container1.PK);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_Description = "ketchup";
			packLine2.SetContainer(container2.PK);
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_Description = "paper cups";
			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.JL_Description = "jam";
			packLine4.SetContainer(container2.PK);
			Factory.SaveForTesting();
			var packLineDataObject1 = CreateDataObject(container1.JC_ContainerNum, "waffles", 66, null);
			var packLineDataObject2 = CreateDataObject("ZZZ", "chickens", 2, null);
			var packLineDataObject3 = CreateDataObject(string.Empty, "paper cups", 1, 5);
			var packLineDataObject4 = CreateDataObject(string.Empty, "jam", 1, 6);
			var packingLinesDataObjects = new[] { packLineDataObject1, packLineDataObject2, packLineDataObject3, packLineDataObject4 };
			AssertReadPackLinesWithoutPackingIntoContainer(shipment, packingLinesDataObjects, links);
			AssertCollectionNotContains("packlines hasn't been matched", packLine1, shipment.OuterPackLines);
			AssertCollectionNotContains("packlines hasn't been matched", packLine2, shipment.OuterPackLines);
			AssertCollectionNotContains("packlines hasn't been matched", packLine3, shipment.OuterPackLines);
			AssertCollectionNotContains("packlines hasn't been matched", packLine4, shipment.OuterPackLines);
			shipment.OuterPackLines.RemoveAndDeleteAll();
			Factory.SaveForTesting();
			packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_Description = "waffles";
			packLine1.JL_PackageCount = 66;
			packLine1.SetContainer(container1.PK);
			packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_Description = "mobile phones";
			packLine2.SetContainer(container2.PK);
			links.AddLink(LinksManager.LinkType.Container, 5, container1);
			AssertReadPackLinesWithPackingIntoContainer(shipment, packingLinesDataObjects, links);
			AssertCollectionNotContains("packlines hasn't been matched", packLine1, shipment.OuterPackLines);
			AssertCollectionNotContains("packlines hasn't been matched", packLine2, shipment.OuterPackLines);
		}

		void AssertReadPackLinesWithoutPackingIntoContainer(AgencyShipment shipment, PackingLine[] packingLinesDataObjects, LinksManager links)
		{
			var logger = new TestErrorLogger();
			var reader = new AgencyPackingLineCollectionReader<AgencyShipmentPackLine, AgencyShipment>(packingLinesDataObjects, logger, Factory, shipment, shipment.OuterPackLines, null, links);
			reader.ReadIntoCollection();
			foreach (PackLine packLine in shipment.OuterPackLines)
			{
				AssertEquals("packline has been allocated to parent shipment", shipment.PK, packLine.JL_JS);
			}

			AssertMultilineASCIIEquals("logs", @"
Information - No matching AgencyShipmentPackLine found, creating new AgencyShipmentPackLine.
Information - Populating AgencyShipmentPackLine...
Information - No matching AgencyShipmentPackLine found, creating new AgencyShipmentPackLine.
Information - Populating AgencyShipmentPackLine...
Information - No matching AgencyShipmentPackLine found, creating new AgencyShipmentPackLine.
Information - Populating AgencyShipmentPackLine...
Information - No matching AgencyShipmentPackLine found, creating new AgencyShipmentPackLine.
Information - Populating AgencyShipmentPackLine...
".Trim(), logger.Logs);
			AssertContainsExactElementsInAnyOrder(new[] { "|waffles|66", "|chickens|2", "|paper cups|1", "|jam|1" }, FormatPackLines(shipment));
		}

		void AssertReadPackLinesWithPackingIntoContainer(AgencyShipment shipment, PackingLine[] packingLinesDataObjects, LinksManager links)
		{
			var containersCollection = shipment.ShippingContainers.Cast<AgencyShipmentContainer>();
			var logger = new TestErrorLogger();
			var reader = new AgencyPackingLineCollectionReader<AgencyShipmentPackLine, AgencyShipment>(packingLinesDataObjects, logger, Factory, shipment, shipment.OuterPackLines, containersCollection, links);
			reader.ReadIntoCollection();
			AssertMultilineASCIIEquals("logs", @"
Information - No matching AgencyShipmentPackLine found, creating new AgencyShipmentPackLine.
Information - Populating AgencyShipmentPackLine...
Information - No matching AgencyShipmentPackLine found, creating new AgencyShipmentPackLine.
Information - Populating AgencyShipmentPackLine...
Information - No matching AgencyShipmentPackLine found, creating new AgencyShipmentPackLine.
Information - Populating AgencyShipmentPackLine...
Information - No matching AgencyShipmentPackLine found, creating new AgencyShipmentPackLine.
Information - Populating AgencyShipmentPackLine...
".Trim(), logger.Logs);
			AssertContainsExactElementsInAnyOrder(new[] { "AAA|waffles|66", "|chickens|2", "AAA|paper cups|1", "|jam|1" }, FormatPackLines(shipment));
		}

		#region Implementation
		BillOfLading shipment;
		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<BillOfLading>();
			ISupportDataImporting dataImporting = shipment;
			// prevent packing into a default container
			dataImporting.IsImportingData = true;
		}

		protected override void TearDown()
		{
			base.SetUp();
			ISupportDataImporting dataImporting = shipment;
			dataImporting.IsImportingData = false;
		}

		string[] FormatPackLines(CommonShipment shipment)
		{
			return shipment.OuterPackLines.Cast<PackLine>().Select(p => string.Format("{0}|{1}|{2}", GetContainerNumbers(p), p.JL_Description, p.JL_PackageCount)).ToArray();
		}

		string GetContainerNumbers(PackLine packLine)
		{
			var containerNumbers = packLine.Containers.Cast<CommonContainer>().Select(c => c.JC_ContainerNum).ToArray();
			return string.Join("*", containerNumbers);
		}

		PackingLine CreateDataObject(string containerNumber, string goodsDescription, int qty, ZInt? containerLink)
		{
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.Commodity = new Commodity { Code = "GEN" };
			dataObject.ContainerNumber = containerNumber;
			dataObject.GoodsDescription = goodsDescription;
			dataObject.PackQty = qty;
			dataObject.ContainerLink = containerLink;
			return dataObject;
		}
		#endregion
	}
}
