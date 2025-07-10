using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CommonBookingInformationForm : ZChildForm
	{
		public CommonBookingInformationForm(CommonCartageBookingInformation businessEntity)
			: base(businessEntity)
		{
			using (businessEntity.SuspendSettingHasChanges())
			{
				businessEntity.BookingAccepted = true;
			}

			SetControlsVisibility();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected virtual void SetControlsVisibility()
		{
			bool canSelectStatus = BookingInformation.CanSelectStatus;
			BookingStatusLabel.Visible = canSelectStatus;
			BookingStatusDropEdit.Visible = canSelectStatus;
			BookingGroupBox.Enabled = !canSelectStatus;
			if (!BookingInformation.CanExposeBookingAction)
			{
				BookingGroupBox.Visible = false;
				BookingStatusLabel.Text = Res.GetString("77e70481-9638-4e37-a92c-32047bb58655", "Booking Request Type:");
			}
		}

		internal ZLabel BookingStatusLabel;
		internal ZDropEdit BookingStatusDropEdit;
		internal new ZButton CancelButton;
		internal ZButton OKButton;
		internal ZRadioButton BookingAcceptedRadioButton;
		internal ZRadioButton BookingRejectedRadioButton;
		internal ZGroupBox BookingGroupBox;
		internal ZLabel CommentLabel;
		internal ZTextBox zTextBox1;
		internal ZPanel zPanel1;
		internal System.ComponentModel.IContainer components;

		void OKButton_Click(object sender, EventArgs e)
		{
			OnOKButtonClick();
		}

		void OnOKButtonClick()
		{
			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.HasErrors())
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("a5304ab3-d486-49ea-9c92-7aecafe5549d", "Please fix the errors before continuing."));
			}
		}

		CommonCartageBookingInformation BookingInformation
		{
			get { return (CommonCartageBookingInformation)base.BusinessEntity; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
