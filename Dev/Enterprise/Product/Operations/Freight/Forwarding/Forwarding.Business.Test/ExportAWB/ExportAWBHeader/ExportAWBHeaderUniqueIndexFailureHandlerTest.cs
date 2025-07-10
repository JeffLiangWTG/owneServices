using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ExportAWBHeaderUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestUniqueIndexConflictResolution()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var consol1 = factory1.New<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_UniqueConsignRef = "123456";

			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var consol2 = factory2.Load<ForwardingConsol>(consol1.PK);

			var awbHeader1 = consol1.AWBHeader;
			consol1.IsAWBValuesOverriddenProperty = true;

			var awbHeader2 = consol2.AWBHeader;
			consol2.IsAWBValuesOverriddenProperty = true;

			factory1.Save();

			try
			{
				factory2.Save();
				Fail("Save should fail due to another AWBHeader already being saved by another edi instance for the same parent consol.");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
			}

			AssertEquals("Consol should still have AWBHeader attached", 1, consol2.AWBHeaderManager.Count);
			AssertEquals("User should be notified about the unique index conflict.", "'CargoWise Support (E)' has created an Air Waybill form for 'MASTER HAWB:123456' while this form was open. Your conflicting changes have been discarded and the created Air Waybill form has been loaded. Review changes to the Air Waybill form and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNoExceptionThrown("No exception expected, as AWBHeader was reloaded.", () => factory2.Save());
			AssertEquals("AWBHeader should be reloaded.", consol1.AWBHeader.PK, consol2.AWBHeader.PK);
		}
	}
}
