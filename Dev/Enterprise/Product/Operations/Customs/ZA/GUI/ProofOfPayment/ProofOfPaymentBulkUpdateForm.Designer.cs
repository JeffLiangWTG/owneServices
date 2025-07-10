using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class ProofOfPaymentBulkUpdateForm
	{


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.posingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
            this.receiptDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.receiptNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.warningLabel = new Enterprise.ZArchitecture.ZLabel();
            this.descriptionLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.posingButtons.SuspendLayout();
            this.receiptDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 159, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 23, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.CusEntryPayInfoBulkUpdateBusinessObject);
            // 
            // posingButtons
            // 
            this.posingButtons.AllowDrop = true;
            this.posingButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.posingButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.posingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 110, true);
            this.posingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
            this.posingButtons.Name = "posingButtons";
            this.posingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 25, true);
            this.posingButtons.TabIndex = 13;
            // 
            // receiptDateEdit
            // 
            this.receiptDateEdit.AllowDrop = true;
            this.receiptDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.receiptDateEdit, "ReceiptDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.CusEntryPayInfoBulkUpdateBusinessObject)(null)).ReceiptDate)));
            this.receiptDateEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f7264fd4-0022-492f-bc00-8c36e0f61743", "Receipt Date");
            this.receiptDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 37, true);
            this.receiptDateEdit.Name = "receiptDateEdit";
            this.receiptDateEdit.TabIndex = 17;
            // 
            // receiptNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.receiptNumberTextBox, "ReceiptNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfoBulkUpdateBusinessObject)(null)).ReceiptNumber)));
            this.receiptNumberTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("60fe6366-74bc-4b82-bbf7-95b0d03ee3ab", "Receipt Number");
            this.receiptNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 37, true);
            this.receiptNumberTextBox.Name = "receiptNumberTextBox";
            this.receiptNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
            this.receiptNumberTextBox.TabIndex = 16;
            // 
            // warningLabel
            // 
            this.warningLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("3bce31a0-ea44-4f3b-8aaf-5b5677bd6125", "WARNING! The selected records have different Receipt Numbers. Proceeding will ove" +
        "rride these values.");
            this.warningLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.warningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.warningLabel.ForeColor = System.Drawing.Color.OrangeRed;
            this.warningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 17, true);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 17, true);
            this.warningLabel.TabIndex = 15;
            this.warningLabel.UseMnemonic = false;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("8930b68a-5bb8-4d74-bcac-6c2f7614f9a4", "You are about to perform a Bulk Update on {0} records.");
            this.descriptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.descriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.descriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 17, true);
            this.descriptionLabel.TabIndex = 14;
            this.descriptionLabel.UseMnemonic = false;
            // 
            // ProofOfPaymentBulkUpdateForm
            // 
            this.AutoAddPreviousNextButtons = false;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("8e109e9a-4603-4831-8e7a-b5ed9eb73a56", "VAT 404 - Bulk Update");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 135, true);
            this.Controls.Add(this.receiptDateEdit);
            this.Controls.Add(this.receiptNumberTextBox);
            this.Controls.Add(this.warningLabel);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.posingButtons);
            this.DataSourceType = typeof(Enterprise.Customs.ZA.Business.CusEntryPayInfoBulkUpdateBusinessObject);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.Name = "ProofOfPaymentBulkUpdateForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = Res.GetString("8e109e9a-4603-4831-8e7a-b5ed9eb73a56", "VAT 404 - Bulk Update");
            this.TopMost = true;
            this.Controls.SetChildIndex(this.posingButtons, 0);
            this.Controls.SetChildIndex(this.descriptionLabel, 0);
            this.Controls.SetChildIndex(this.warningLabel, 0);
            this.Controls.SetChildIndex(this.receiptNumberTextBox, 0);
            this.Controls.SetChildIndex(this.receiptDateEdit, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.posingButtons.ResumeLayout(true);
            this.posingButtons.PerformLayout();
            this.receiptDateEdit.ResumeLayout(true);
            this.receiptDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		ZArchitecture.ZLabel descriptionLabel;
		ZArchitecture.ZLabel warningLabel;
		ZArchitecture.ZTextBox receiptNumberTextBox;
		ZDateEdit receiptDateEdit;
		Core.Forms.ZPostingButtonsUserControl posingButtons;

		#endregion

	}
}
