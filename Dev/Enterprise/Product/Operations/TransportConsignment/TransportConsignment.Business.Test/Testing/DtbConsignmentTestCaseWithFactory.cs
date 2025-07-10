using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbConsignmentTestCaseWithFactory : TestCaseWithFactory
	{
		#region Implementation

		internal TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion
	}
}
