using System;
using Enterprise.DocumentVisualizer.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Testing
{
	sealed class AirBookingProgressFormManagerTest : TestCase
	{
		[SnailTest]
		public void TestCreatedFormShouldShowWorkingCancelButton()
		{
			var cancelledClicked = false;

			using (var manager = new AirBookingProgressFormManagerForTest(() => cancelledClicked = true))
			using (var progressForm = manager.CreateFormForTest())
			{
				progressForm.Show();

				AssertNotNull("Progress form has been found", progressForm);
				Assert("Should allow to cancel", progressForm.ShowCancelButton);

				progressForm.CancelButton.PerformClick();
				Assert("Should invoke cancel action on Cancel click", cancelledClicked);
			}
		}

		sealed class AirBookingProgressFormManagerForTest : AirBookingProgressFormManager
		{
			public AirBookingProgressFormManagerForTest(Action onUserRequestedCancel)
				: base(onUserRequestedCancel)
			{
			}

			public DocumentVisualizerProgressForm CreateFormForTest() => base.CreateForm();
		}
	}
}
