using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbConsignmentConfirmationTotalsHelperTest : TestCaseWithFactory
	{
		#region TestGetServicesExist

		public void TestGetServicesExist()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			AssertEquals(false, consignment.DeliveryInstruction.Confirmations.GetServicesExist());

			var serviceCons = consignment.Services.AddNew();
			AssertEquals(true, consignment.DeliveryInstruction.Confirmations.GetServicesExist());
		}

		#endregion

		#region Implementation

		protected TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
