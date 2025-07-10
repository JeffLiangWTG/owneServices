using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Moq;

namespace Enterprise.Freight.Agency.Documents.DataTransfer.Testing
{
	public class AgencyDocDataObjectUXmlWriterTest : TestCaseWithFactory
	{
		public void TestGetDataObject()
		{
			var document = new EmptyDocument("Test Document Name", "Test Data Context");
			var expectedShipment = new UniversalDataBuss.DataObjects.Universal.Shipment();
			expectedShipment.GoodsDescription = "Expected Shipment";

			AssertNull(new AgencyDocDataObjectUXmlWriter().GetDataObject(DefaultDataObjectWriterStrategy.Instance, document, MessageType.Unspecified));

			var agencyDocDataObjectUXmlWriter = new Mock<IAgencyDocDataObjectUXmlWriter>();
			agencyDocDataObjectUXmlWriter.Setup(d => d.GetDataObject(DefaultDataObjectWriterStrategy.Instance, document, MessageType.Unspecified)).Returns(expectedShipment);
			ObjectFactory.Substitute("IAgencyDocDataObjectUXmlWriter", agencyDocDataObjectUXmlWriter.Object);

			AssertEquals(expectedShipment, ObjectFactory.Get<IAgencyDocDataObjectUXmlWriter>().GetDataObject(DefaultDataObjectWriterStrategy.Instance, document));
		}
	}
}
