using System.Windows.Forms;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	[TestedType(typeof(MovementHeaderResetToOriginalForm))]
	sealed class MovementHeaderResetToOriginalFormTest : ZFormBasherTest
	{
		public void TestOkButton_Click()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader2 = header.MovementHeaders.AddNew();
			moveHeader2.AllocateInBondNumber("2111");
			moveHeader2.BM_CustomsStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var moveHeader3 = header.MovementHeaders.AddNew();
			moveHeader3.AllocateInBondNumber("3111");
			var moveHeader4 = header.MovementHeaders.AddNew();
			moveHeader4.AllocateInBondNumber("4111");
			var sendingObj = new InBondMessageSendingObject(moveHeader4, InBondMessageType.DepartureDelete);
			sendingObj.Send();
			var collection = new USMovementHeaderResetCollection(header);
			AssertEquals(2, collection.Count);
			collection[0].RO_ResetToOriginal = true;
			collection[1].RO_ResetToOriginal = false;
			using (MovementHeaderResetToOriginalForm form = new MovementHeaderResetToOriginalForm(collection))
			{
				form.Show();
				var okButton = (ZButton)form.Controls["OKButton"];
				okButton.PerformClick();
				AssertEquals("Please fix the errors first.", UnitTestUserNotification.Instance.LastMessage.Text);
				collection[0].RO_ResetReason = "reason1";
				okButton.PerformClick();
				AssertEquals("", header.MovementHeaders[0].BM_CustomsStatus);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<CusInBondHeader>();
			var movementHeadersToReset = new USMovementHeaderResetCollection(header);
			return new MovementHeaderResetToOriginalForm(movementHeadersToReset);
		}
	}
}
