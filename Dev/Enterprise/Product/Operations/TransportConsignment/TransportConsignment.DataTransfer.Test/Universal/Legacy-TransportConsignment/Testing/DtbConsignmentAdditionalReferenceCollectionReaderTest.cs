using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.DataTransfer.Universal.Testing;
using Enterprise.TransportConsignment.Business;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	[TestedType(typeof(TransportAdditionalReferenceCollectionReader<DtbBookingConsignment>))]
	class DtbConsignmentAdditionalReferenceCollectionReaderTest : DtbTransportAdditionalReferenceCollectionReaderTest<DtbBookingConsignment>
	{
		#region GetNewTransport
		protected override DtbBookingConsignment GetNewTransport()
		{
			return Factory.NewWithValidTestData<DtbBookingConsignment>();
		}
		#endregion
	}
}
