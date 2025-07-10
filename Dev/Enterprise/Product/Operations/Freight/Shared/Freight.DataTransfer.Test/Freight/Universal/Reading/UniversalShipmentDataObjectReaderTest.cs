using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public abstract class UniversalShipmentDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestFieldMappings()
		{
			var dataObject = GetDataObject();
			AssertNotNull("data object to read should not be null", dataObject);

			var reader = GetReader(dataObject);
			BusinessObject businessObject = null;
			reader.ReadIntoBusinessObject(ref businessObject);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			var actualShipmentMap = CreateFieldMap(businessObject).Trim();
			var expectedShipmentMap = GetExpectedShipmentMap().Trim();

			AssertMultilineASCIIEquals("Shipment field map", expectedShipmentMap, actualShipmentMap);
		}

		protected abstract ITopLevelDataObjectReader GetReader(UniversalShipment dataObject);
		protected abstract UniversalShipment GetDataObject();
		protected abstract string CreateFieldMap(BusinessObject businessObject);
		protected abstract string GetExpectedShipmentMap();
	}
}
