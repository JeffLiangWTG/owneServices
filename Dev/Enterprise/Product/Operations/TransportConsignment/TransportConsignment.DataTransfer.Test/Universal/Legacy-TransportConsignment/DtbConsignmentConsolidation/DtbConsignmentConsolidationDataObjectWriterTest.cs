using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentConsolidationDataObjectWriterTest : DtbBookingConsignmentUniversalTestCase
	{
		#region TestTopLevelDataContextType
		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransportConsignmentConsolidation, ((ITopLevelDataObjectWriter)new DtbConsignmentConsolidationDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion
		#region TestEDIMessageSubType
		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment, ((ITopLevelDataObjectWriter)new DtbConsignmentConsolidationDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion
		#region TestBasicFieldMappings
		public void TestBasicFieldMappings()
		{
			var consolidation = Helper.CreateConsolidation();
			var writer = new DtbConsignmentConsolidationDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			AssertNotNull(writer.GetDataObject(consolidation));
		}
		#endregion
	}
}
