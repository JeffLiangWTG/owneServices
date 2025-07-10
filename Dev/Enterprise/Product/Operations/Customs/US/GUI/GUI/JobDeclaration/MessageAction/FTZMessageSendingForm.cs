using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class FTZMessageSendingForm : ZChildForm
	{
		public FTZMessageSendingForm()
		{
			InitializeComponent();
		}

		public FTZMessageSendingForm(FTZMessageSendingObject ftzData)
			: base(ftzData)
		{
			InitializeComponent();

			this.sendingObject = ftzData;
			sendingObject.US_OtherReasonInfo.ValueChanged += US_OtherReasonInfo_ValueChanged;
			SetVisibility();
		}

		readonly FTZMessageSendingObject sendingObject;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				if (sendingObject != null)
				{
					sendingObject.US_OtherReasonInfo.ValueChanged -= US_OtherReasonInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		void US_OtherReasonInfo_ValueChanged(object sender, EventArgs e)
		{
			SetVisibility();
		}

		void SetVisibility()
		{
			RemarksTextBox.Visible = sendingObject.US_OtherReason;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			if (sendingObject != null)
			{
				var numberOfReasonSelected = GetNumberOfReasonSelected();
				if (numberOfReasonSelected == 0 || numberOfReasonSelected > 8)
				{
					Globals.Message.ShowInformation(Res.GetString("15012375-E5E4-4B94-B19A-47508EF7B390", "A reason is required; please select a maximum of 8 reasons."));
				}
				else
				{
					sendingObject.RunPreSaveValidation();
					if (sendingObject.HasNotifications(CargoWise.EntityFramework.NotificationType.MessageError) &&
							!Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
					{
						Globals.Message.ShowInformation(Customs.Business.MessageSendingValidation.MessageErrorsExistWithNoSecurityRight);
					}
					else if (!sendingObject.HasNotifications() || Globals.Message.Show(ThereIsANotification, "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
					{
						sendingObject.ShouldSendMessage = true;
						Close();
					}
				}
			}
		}
		public const string ThereIsANotification = "There is a notification. Are you sure you wish to continue?";

		int GetNumberOfReasonSelected()
		{
			var result = 0;
			IncrementIfTrue(sendingObject.US_ChangeOrAddConveyance, ref result);
			IncrementIfTrue(sendingObject.US_DeleteConveyance, ref result);
			IncrementIfTrue(sendingObject.US_ChangeOrAddBillOfLading, ref result);
			IncrementIfTrue(sendingObject.US_DeleteBillOfLading, ref result);
			IncrementIfTrue(sendingObject.US_ChangeOrAddHTSLine, ref result);
			IncrementIfTrue(sendingObject.US_DeleteHTSLine, ref result);
			IncrementIfTrue(sendingObject.US_ChangeAdmittedQuantity, ref result);
			IncrementIfTrue(sendingObject.US_CancelOrAddPTT, ref result);
			IncrementIfTrue(sendingObject.US_OtherReason, ref result);
			return result;
		}

		void IncrementIfTrue(bool selected, ref int value)
		{
			if (selected)
			{
				value++;
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
