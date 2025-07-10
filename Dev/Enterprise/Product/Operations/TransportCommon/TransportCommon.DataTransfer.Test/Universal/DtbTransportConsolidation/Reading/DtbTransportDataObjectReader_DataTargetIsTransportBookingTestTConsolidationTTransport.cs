using System;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Testing
{
	public abstract class DtbTransportDataObjectReader_DataTargetIsTransportBookingTest<TConsolidation, TTransport> : OrganizationAddressTestHelper
			where TConsolidation : DtbTransportConsolidation
			where TTransport : DtbTransport
	{
		#region TestDataContextType

		public void TestDataContextType()
		{
			var reader = GetDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), new UniversalObjectFactory());
			AssertEquals(GetExpectedDataContextType(), reader.DataContextType);
		}

		protected abstract DataContextType GetExpectedDataContextType();

		#endregion

		#region TestPopulateBusinessObject

		public void TestPopulateBusinessObject()
		{
			var shipmentForTransport = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentForTransport.DataContext = DataContextFactory.New();

			var reader = (ITopLevelDataObjectReader)GetDataObjectReader(shipmentForTransport, Logger, Factory);
			var transportRead = reader.ReadIntoTopLevelBusinessObject();
			AssertNotNull(transportRead);
			AssertNotNull(((TTransport)transportRead).ConsolidationSingleJob);
		}

		public void TestReadIntoBusinessObjectThrows()
		{
			var shipmentForTransport = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentForTransport.DataContext = DataContextFactory.New();

			var reader = GetDataObjectReader(shipmentForTransport, Logger, Factory);
			AssertExceptionThrown<InvalidOperationException>("This method should always throw", "This reader should never be used to populate a business object, it should only be used to redirect to the consolidation reader.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		protected abstract DtbTransportDataObjectReader_DataTargetIsTransportBooking<TConsolidation, TTransport> GetDataObjectReader(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory);
	}
}
