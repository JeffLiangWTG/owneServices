using System;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(BulkRateUpdatesController))]
	public class BulkRateUpdatesControllerTest : ZSingletonControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(ClientRate);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BulkRateUpdates;
		}

		public void TestDisplayModeForNew()
		{
			BulkRateUpdatesController controller = new BulkRateUpdatesController();

			using (ZForm testForm = (ZForm)controller.ShowNewForm())
			{
				Assert(testForm.DisplayMode == ODisplayMode.Browse);
			}
		}

		public void TestBulkUpdateFormNoMessages()
		{
			BulkRateUpdatesController controller = new BulkRateUpdatesController();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (ZForm testForm = (ZForm)controller.ShowNewForm())
			{
				testForm.Show();
				testForm.Close();
			}

			Assert(string.Format("Got message {0} but expected none", UnitTestUserNotification.Instance.LastMessage.Text),
				UnitTestUserNotification.Instance.LastMessage.WasNone);

			using (BulkUpdateWizard testForm = new BulkUpdateWizard())
			{
				testForm.Show();

				var bizEntity = (BulkRateUpdater)testForm.BusinessEntity;
				bizEntity.HasChanges = true;

				testForm.Close();
			}

			Assert(string.Format("Got message {0} but expected none", UnitTestUserNotification.Instance.LastMessage.Text),
				UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestSecurityCheckPointForIntercompanyTariffs()
		{
			BulkRateUpdatesController result = (BulkRateUpdatesController)ZControllerFactory.Create(ControllerIDs.BulkRateUpdates);
			result.DefaultRateType = RatingConstants.RatingHeaderTypes.IntercompanyTariff;

			Env.Security.IntercompanyTariffsBulkUpdate.IsAllowed = true;
			using (result.ShowNewForm())
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.IntercompanyTariffsBulkUpdate.IsAllowed = false;
			AssertExceptionThrown("Should throw security exception!", typeof(SecurityAccessDeniedException),
				Env.Security.IntercompanyTariffsBulkUpdate.ErrorMessageForNotAllowed, () => result.ShowNewForm());
		}
	}
}
