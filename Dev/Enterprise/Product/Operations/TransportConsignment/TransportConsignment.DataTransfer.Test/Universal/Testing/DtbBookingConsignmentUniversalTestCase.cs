using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportConsignment.Business.Testing;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbBookingConsignmentUniversalTestCase : OrganizationAddressTestHelper
	{
		#region Implementation
		protected TransportBookingConsignmentTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportBookingConsignmentTestHelper helper;
		#endregion
	}
}
