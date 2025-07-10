using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(InternationalZonesController))]
	sealed class InternationalZonesControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			RefZoneHeader testObject = Factory.NewWithValidTestData<RefZoneHeader>();
			Factory.Save();
			return testObject;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.InternationalZone;
		}

		public void TestSecurityCheckpoints()
		{
			TestInternationalZonesController controller = new TestInternationalZonesController();
			AssertEquals("For New", Env.Security.ZoneNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.ZoneView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.ZoneModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.ZoneDelete, controller.CheckPointForDelete);
		}

		public override void TestDeleteForm()
		{
			RefZoneHeader tAXzone = Factory.New<RefZoneHeader>();
			tAXzone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.Tax.Code;
			tAXzone.FZ_Code = "TAXZ";
			tAXzone.FZ_Description = "Zone of TAX type";

			RefZoneHeader rATzone = Factory.New<RefZoneHeader>();
			rATzone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.Rating.Code;
			rATzone.FZ_Code = "RAT";
			rATzone.FZ_Description = "Zone of RAT type";

			InternationalZonesController controller = new InternationalZonesController();
			try
			{
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertNull(controller.ShowDeleteForm(tAXzone));
				AssertEquals("Prevent delete error should be shown", "Cannot delete zone of TAX type.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNotNull(controller.ShowDeleteForm(rATzone));
				AssertNull("No message should be shown", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				if (controller.LastShownForm != null)
				{
					controller.LastShownForm.Dispose();
				}
			}

			base.TestDeleteForm();
		}
	}
}
