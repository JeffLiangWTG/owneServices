using System;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Module.Testing
{
	[TestedType(typeof(ConsolidatedTransportBookingController))]
	public class ConsolidatedTransportBookingControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			ConsolidatedTransportBookingController controller = new ConsolidatedTransportBookingController();
			AssertEquals("Module ID should be Consolidated Booking", ModuleIDs.ConsolidatedTransportBooking, controller.ModuleID);
		}

		public void TestCRMSecurityCheckpoints()
		{
			var bizObj = Factory.NewWithValidTestData<CommonConsolidatedTransportBooking>();
			CRMSecurityProviderTest<CommonConsolidatedTransportBooking>.AssertController(new ConsolidatedTransportBookingController(), bizObj, Env.Security.ConsolidatedTransportBookingCRMSecurity);
		}

		#region Overrides

		protected override Type GetBusinessObjectType()
		{
			return typeof(CommonConsolidatedTransportBooking);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ConsolidatedTransportBooking;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
