using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbBookingConsignmentDataObjectWriterTest : DtbBookingConsignmentUniversalTestCase
	{
		#region TestTopLevelDataContextType
		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransportConsignment, ((ITopLevelDataObjectWriter)new DtbBookingConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion
		#region TestEDIMessageSubType
		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment, ((ITopLevelDataObjectWriter)new DtbBookingConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion
		#region TestBasicFieldMappings
		public void TestBasicFieldMappings()
		{
			var consignment = Helper.CreateBookingConsignment();
			var writer = new DtbBookingConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			AssertNotNull(writer.GetDataObject(consignment));
		}

		#endregion
		#region TestAdditionalReferences
		public void TestAdditionalReferences()
		{
			var factory = Factory.BOFactory;
			var consignmentBO = Factory.NewWithValidTestData<DtbBookingConsignment>();
			consignmentBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(factory, "TRF"));
			consignmentBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(factory, "CIN"));
			consignmentBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(factory, "UCR"));
			consignmentBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(factory, "CLR"));
			var writer = new DtbBookingConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var consignmentDataObject = writer.GetDataObject(consignmentBO);
			AssertNotNull("Precondition", consignmentDataObject);
			AssertNotNull("Precondition", consignmentDataObject.AdditionalReferenceCollection);
			CombineAssertions(delegate
			{
				AssertEquals(4, consignmentDataObject.AdditionalReferenceCollection.Count);
				AdditionalReferenceDataObjectWriterTest.AssertContents(consignmentDataObject.AdditionalReferenceCollection[0], "TRF", "Transport Reference Number");
				AdditionalReferenceDataObjectWriterTest.AssertContents(consignmentDataObject.AdditionalReferenceCollection[1], "CIN", "Commercial Invoice Number");
				AdditionalReferenceDataObjectWriterTest.AssertContents(consignmentDataObject.AdditionalReferenceCollection[2], "UCR", "External (3rd Party) Unique Consignment Reference");
				AdditionalReferenceDataObjectWriterTest.AssertContents(consignmentDataObject.AdditionalReferenceCollection[3], "CLR", "Customer Reference Number");
			});
		}
		#endregion
	}
}
