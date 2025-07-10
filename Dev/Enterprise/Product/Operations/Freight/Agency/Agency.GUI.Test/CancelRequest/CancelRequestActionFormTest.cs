using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	public class CancelRequestActionFormTest : TestCaseWithFactory
	{
		public void TestLabels()
		{
			using (var form = new CancelRequestActionForm())
			{
				var zButtonAccept = form.Controls.Find("zButtonAccept", true).First() as ZButton;
				AssertEquals("zButtonAccept", "Accept", zButtonAccept.CaptionResourceString.Caption);

				var zButtonReject = form.Controls.Find("zButtonReject", true).First() as ZButton;
				AssertEquals("zButtonReject", "Reject", zButtonReject.CaptionResourceString.Caption);

				var zButtonCancel = form.Controls.Find("zButtonCancel", true).First() as ZButton;
				AssertEquals("zButtonCancel", "Cancel", zButtonCancel.CaptionResourceString.Caption);

				var zLabelMessage = form.Controls.Find("zLabelMessage", true).First() as ZLabel;
				AssertEquals("zLabelMessage", "A Booking Request Withdrawal/Cancellation message was received for this booking. Do you accept the withdrawal/cancellation, reject the withdrawal/cancellation or do you want to cancel this dialog and review the booking before taking an action?", zLabelMessage.CaptionResourceString.Caption);

				var zLabelRejectionReason = form.Controls.Find("zLabelRejectionReason", true).First() as ZLabel;
				AssertEquals("zLabelRejectionReason", "Rejection Reason:", zLabelRejectionReason.CaptionResourceString.Caption);

				var zButtonSubmitRejectionReason = form.Controls.Find("zButtonSubmitRejectionReason", true).First() as ZButton;
				AssertEquals("zButtonSubmitRejectionReason", "Submit", zButtonSubmitRejectionReason.CaptionResourceString.Caption);

				AssertEquals("Form Title", "Booking Withdrawal/Cancellation", form.CaptionResourceString.Caption);
			}
		}
	}
}
