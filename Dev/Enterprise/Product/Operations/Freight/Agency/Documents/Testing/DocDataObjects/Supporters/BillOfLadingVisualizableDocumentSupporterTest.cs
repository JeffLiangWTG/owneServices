using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	public class BillOfLadingVisualizableDocumentSupporterTest : AgencyShipmentVisualizableDocumentSupporterTest<BillOfLading>
	{
		public override AgencyShipmentVisualizableDocumentSupporter<BillOfLading> GetVisualizableDocumentSupporter(BillOfLading shipment)
		{
			return new BillOfLadingVisualizableDocumentSupporter(shipment);
		}

		public void TestGetEventParent_BillOfLading()
		{
			var shipment = Factory.NewWithValidTestData<BillOfLading>();

			var documentData = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData[JobDocumentDataSchema.JDD_ParentTableCode] = shipment.TablePrefix;
			documentData[JobDocumentDataSchema.JDD_ParentID] = shipment.PK;
			documentData[JobDocumentDataSchema.JDD_Name] = BillOfLadingDocumentDataStoreNames.AgencyBillOfLading;
			Factory.Save();

			var supporter = GetVisualizableDocumentSupporter(shipment);

			var xmlEventValueObject = new Mock<IXmlEventValueObject>();
			xmlEventValueObject.Setup(x => x.DataContext.DocumentaryOverride.DocumentName).Returns(BillOfLadingDocumentNames.BillOfLading);

			AssertEquals(documentData, supporter.GetEventParent(xmlEventValueObject.Object));
		}
	}
}
