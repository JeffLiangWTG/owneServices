using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(PackingLineCollectionReader<,>))]
	sealed class PackingLineCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var consol = shipment.Consols.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAA";

			var container2 = consol.Containers.AddNew();
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

			var packLineDataObject1 = CreateDataObject(container1.JC_ContainerNum, "waffles", 66);
			var packLineDataObject2 = CreateDataObject("ZZZ", "chickens", 2);
			var packLineDataObject3 = CreateDataObject(string.Empty, "paper cups", 1);
			var packLineDataObject4 = CreateDataObject(string.Empty, "jam", 1);

			var packingLinesDataObjects = new[]
			{
				packLineDataObject1, packLineDataObject2, packLineDataObject3, packLineDataObject4
			};

			AssertReadPackLinesWithoutPackingIntoContainer(shipment, packingLinesDataObjects);

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

			AssertReadPackLinesWithPackingIntoContainer(shipment, packingLinesDataObjects);

			AssertCollectionNotContains("packlines hasn't been matched", packLine1, shipment.OuterPackLines);
			AssertCollectionNotContains("packlines hasn't been matched", packLine2, shipment.OuterPackLines);
		}

		void AssertReadPackLinesWithoutPackingIntoContainer(CommonShipment shipment, PackingLine[] packingLinesDataObjects)
		{
			var packLinesCollection = shipment.OuterPackLines;

			var logger = new TestErrorLogger();
			var reader = new PackingLineCollectionReader<PackLine, CommonShipment>(
				packingLinesDataObjects, logger, Factory, shipment, packLinesCollection);

			reader.ReadIntoCollection();

			foreach (PackLine packLine in packLinesCollection)
			{
				AssertEquals("packline has been allocated to parent shipment", shipment.PK, packLine.JL_JS);
			}

			AssertMultilineASCIIEquals("logs", @"
Information - No matching PackLine found, creating new PackLine.
Information - Populating PackLine...
Information - No matching PackLine found, creating new PackLine.
Information - Populating PackLine...
Information - No matching PackLine found, creating new PackLine.
Information - Populating PackLine...
Information - No matching PackLine found, creating new PackLine.
Information - Populating PackLine...
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"BBB|waffles|66", "BBB|chickens|2", "BBB|paper cups|1", "BBB|jam|1"
			},
			FormatPackLines(shipment));
		}

		void AssertReadPackLinesWithPackingIntoContainer(CommonShipment shipment, PackingLine[] packingLinesDataObjects)
		{
			var packLinesCollection = shipment.OuterPackLines;
			var containersCollection = shipment.Consols[0].Containers.Cast<CommonContainer>();

			var logger = new TestErrorLogger();
			var reader = new PackingLineCollectionReader<PackLine, CommonShipment>(
				packingLinesDataObjects, logger, Factory, shipment, packLinesCollection, containersCollection);

			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - No matching PackLine found, creating new PackLine.
Information - Populating PackLine...
Information - No matching PackLine found, creating new PackLine.
Information - Populating PackLine...
Information - No matching PackLine found, creating new PackLine.
Information - Populating PackLine...
Information - No matching PackLine found, creating new PackLine.
Information - Populating PackLine...
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AAA|waffles|66", "BBB|chickens|2", "BBB|paper cups|1", "BBB|jam|1"
			},
			FormatPackLines(shipment));
		}

		#region Implementation

		CommonShipment shipment;

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<CommonShipment>();
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
			return shipment.OuterPackLines
				.Cast<PackLine>()
				.Select(p => string.Format("{0}|{1}|{2}", GetContainerNumbers(p), p.JL_Description, p.JL_PackageCount))
				.ToArray();
		}

		string GetContainerNumbers(PackLine packLine)
		{
			var containerNumbers = packLine.Containers.Cast<CommonContainer>()
				.Select(c => c.JC_ContainerNum).ToArray();

			return string.Join("*", containerNumbers);
		}

		PackingLine CreateDataObject(string containerNumber, string goodsDescription, int qty)
		{
			var dataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.Commodity = new Commodity { Code = "GEN" };
			dataObject.ContainerNumber = containerNumber;
			dataObject.GoodsDescription = goodsDescription;
			dataObject.PackQty = qty;
			return dataObject;
		}

		#endregion
	}
}
