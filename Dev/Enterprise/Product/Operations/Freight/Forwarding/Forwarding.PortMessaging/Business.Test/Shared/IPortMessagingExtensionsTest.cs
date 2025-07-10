using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	sealed class IPortMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestHasData()
		{
			PackLinePortMessaging portMessaging = null;
			AssertEquals(false, portMessaging.HasData());

			portMessaging = Factory.New<PackLinePortMessaging>();
			AssertEquals("Das not have data by default", false, portMessaging.HasData());

			portMessaging.JLM_Annex30AFailureProcess = true;
			AssertEquals(true, portMessaging.HasData());

			portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_Annex30AType = "X";
			AssertEquals(true, portMessaging.HasData());

			portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_ATBNumber = "X";
			AssertEquals(true, portMessaging.HasData());

			portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_EntryType = "X";
			AssertEquals(true, portMessaging.HasData());

			portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_ExemptionReason = "X";
			AssertEquals(true, portMessaging.HasData());

			portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_MovementReferenceNumber = "X";
			AssertEquals(true, portMessaging.HasData());

			portMessaging = Factory.New<PackLinePortMessaging>();
			portMessaging.JLM_MovementReferenceNumberComplete = true;
			AssertEquals(true, portMessaging.HasData());
		}
	}
}
