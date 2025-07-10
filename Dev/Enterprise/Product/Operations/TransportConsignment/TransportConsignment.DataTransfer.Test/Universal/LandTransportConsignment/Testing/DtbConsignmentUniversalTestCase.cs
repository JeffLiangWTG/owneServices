using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportConsignment.Business.Testing;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentUniversalTestCase : OrganizationAddressTestHelper
	{
		#region Implementation
		protected TransportConsignmentTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new TransportConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportConsignmentTestHelper helper;
		#endregion
	}
}
