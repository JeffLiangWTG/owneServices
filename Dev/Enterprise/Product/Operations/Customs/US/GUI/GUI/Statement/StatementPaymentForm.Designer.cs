
namespace Enterprise.Customs.US.GUI
{
	partial class StatementPaymentForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AccountNoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalAmountPayableLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AccountNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalAmountPayableCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACHPaymentTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaymentTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentPartyLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 153, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 24, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.StatementPaymentAction);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 123, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 23, true);
			this.CancelButton.TabIndex = 9;
			this.CancelButton.Text = "&Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 123, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.OKButton.TabIndex = 8;
			this.OKButton.Text = "&OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// AccountNoLabel
			// 
			this.AccountNoLabel.AutoSize = true;
			this.AccountNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 44, true);
			this.AccountNoLabel.Name = "AccountNoLabel";
			this.AccountNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 13, true);
			this.AccountNoLabel.TabIndex = 2;
			this.AccountNoLabel.Text = "Payer Unit. No :";
			// 
			// TotalAmountPayableLabel
			// 
			this.TotalAmountPayableLabel.AutoSize = true;
			this.TotalAmountPayableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 96, true);
			this.TotalAmountPayableLabel.Name = "TotalAmountPayableLabel";
			this.TotalAmountPayableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 13, true);
			this.TotalAmountPayableLabel.TabIndex = 6;
			this.TotalAmountPayableLabel.Text = "Total Amount Payable:";
			// 
			// AccountNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccountNoTextBox, "PayerUnitNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementPaymentAction)(null)).PayerUnitNo)));
			this.AccountNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 40, true);
			this.AccountNoTextBox.Name = "AccountNoTextBox";
			this.AccountNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.AccountNoTextBox.TabIndex = 3;
			// 
			// TotalAmountPayableCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalAmountPayableCalcEdit, "TotalAmountPayable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.StatementPaymentAction)(null)).TotalAmountPayable)));
			this.TotalAmountPayableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 92, true);
			this.TotalAmountPayableCalcEdit.Name = "TotalAmountPayableCalcEdit";
			this.TotalAmountPayableCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.TotalAmountPayableCalcEdit.TabIndex = 7;
			this.TotalAmountPayableCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACHPaymentTypeLabel
			// 
			this.ACHPaymentTypeLabel.AutoSize = true;
			this.ACHPaymentTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 18, true);
			this.ACHPaymentTypeLabel.Name = "ACHPaymentTypeLabel";
			this.ACHPaymentTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 13, true);
			this.ACHPaymentTypeLabel.TabIndex = 0;
			this.ACHPaymentTypeLabel.Text = "ACH Debit or Credit:";
			// 
			// PaymentTypeZDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PaymentTypeZDropEdit, "ACHPaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.StatementPaymentAction)(null)).ACHPaymentType)));
			this.PaymentTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 14, true);
			this.PaymentTypeZDropEdit.Name = "PaymentTypeZDropEdit";
			this.PaymentTypeZDropEdit.PreBoundMaxLength = 3;
			this.PaymentTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.PaymentTypeZDropEdit.TabIndex = 1;
			// 
			// PaymentPartyDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PaymentPartyDropEdit, "PaymentParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.StatementPaymentAction)(null)).PaymentParty)));
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 66, true);
			this.PaymentPartyDropEdit.Name = "PaymentPartyDropEdit";
			this.PaymentPartyDropEdit.PreBoundMaxLength = 3;
			this.PaymentPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.PaymentPartyDropEdit.TabIndex = 5;
			// 
			// PaymentPartyLabel
			// 
			this.PaymentPartyLabel.AutoSize = true;
			this.PaymentPartyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 70, true);
			this.PaymentPartyLabel.Name = "PaymentPartyLabel";
			this.PaymentPartyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 13, true);
			this.PaymentPartyLabel.TabIndex = 4;
			this.PaymentPartyLabel.Text = "Payment Party:";
			// 
			// StatementPaymentForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 177, true);
			this.Controls.Add(this.PaymentPartyDropEdit);
			this.Controls.Add(this.PaymentPartyLabel);
			this.Controls.Add(this.PaymentTypeZDropEdit);
			this.Controls.Add(this.ACHPaymentTypeLabel);
			this.Controls.Add(this.TotalAmountPayableCalcEdit);
			this.Controls.Add(this.AccountNoTextBox);
			this.Controls.Add(this.TotalAmountPayableLabel);
			this.Controls.Add(this.AccountNoLabel);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.StatementPaymentAction);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.StatementPaymentAction";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 177, true);
			this.Name = "StatementPaymentForm";
			this.Text = "StatementPaymentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.AccountNoLabel, 0);
			this.Controls.SetChildIndex(this.TotalAmountPayableLabel, 0);
			this.Controls.SetChildIndex(this.AccountNoTextBox, 0);
			this.Controls.SetChildIndex(this.TotalAmountPayableCalcEdit, 0);
			this.Controls.SetChildIndex(this.ACHPaymentTypeLabel, 0);
			this.Controls.SetChildIndex(this.PaymentTypeZDropEdit, 0);
			this.Controls.SetChildIndex(this.PaymentPartyLabel, 0);
			this.Controls.SetChildIndex(this.PaymentPartyDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.ZLabel AccountNoLabel;
		private Enterprise.ZArchitecture.ZLabel TotalAmountPayableLabel;
		private Enterprise.ZArchitecture.ZTextBox AccountNoTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit TotalAmountPayableCalcEdit;
		private Enterprise.ZArchitecture.ZLabel ACHPaymentTypeLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PaymentTypeZDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PaymentPartyDropEdit;
		private Enterprise.ZArchitecture.ZLabel PaymentPartyLabel;
	}
}
