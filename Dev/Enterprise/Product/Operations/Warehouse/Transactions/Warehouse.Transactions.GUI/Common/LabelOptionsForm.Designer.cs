using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class LabelOptionsForm
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
		protected override void InitializeComponent()
		{
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NumberOfLabelsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NumberOfLabelsDescription = new Enterprise.ZArchitecture.ZLabel();
			this.NumberOfLabelsEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionsGroupBox.SuspendLayout();
			this.NumberOfLabelsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 146, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDocketLabelControl);
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPrintButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("LabelOptionsForm|65122c00-382f-48ba-b79f-66d9d42ba5bf", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 116, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelPrintButton.TabIndex = 4;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("LabelOptionsForm|8648cf06-d0a5-443e-ba75-9d5a6178332c", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 116, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PrintButton.TabIndex = 3;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionsGroupBox, false);
			this.OptionsGroupBox.Controls.Add(this.NumberOfLabelsPanel);
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 12, true);
			this.OptionsGroupBox.Name = "OptionsGroupBox";
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 96, true);
			this.OptionsGroupBox.TabIndex = 5;
			this.OptionsGroupBox.TabStop = false;
			// 
			// NumberOfLabelsPanel
			// 
			this.NumberOfLabelsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.NumberOfLabelsPanel.Controls.Add(this.NumberOfLabelsDescription);
			this.NumberOfLabelsPanel.Controls.Add(this.NumberOfLabelsEdit);
			this.NumberOfLabelsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 30, true);
			this.NumberOfLabelsPanel.Name = "NumberOfLabelsPanel";
			this.NumberOfLabelsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 40, true);
			this.NumberOfLabelsPanel.TabIndex = 0;
			// 
			// NumberOfLabelsDescription
			// 
			this.BindingSource.SetBindingMember(this.NumberOfLabelsDescription, "NumberOfLabelsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLabelControl)(null)).NumberOfLabelsDescription)));
			this.NumberOfLabelsDescription.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("LabelOptionsForm|745cf286-6de4-4cf6-810c-793c55a779c1", "of");
			this.NumberOfLabelsDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 13, true);
			this.NumberOfLabelsDescription.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.NumberOfLabelsDescription.Name = "NumberOfLabelsDescription";
			this.NumberOfLabelsDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 14, true);
			this.NumberOfLabelsDescription.TabIndex = 3;
			// 
			// NumberOfLabelsEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfLabelsEdit, "NumberOfLabelsToPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLabelControl)(null)).NumberOfLabelsToPrint)));
			this.NumberOfLabelsEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("LabelOptionsForm|c8b1d896-245f-486c-88a1-f7f74b9dc754", "Number of Labels to Print");
			this.NumberOfLabelsEdit.Decimals = 0;
			this.NumberOfLabelsEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 10, true);
			this.NumberOfLabelsEdit.Name = "NumberOfLabelsEdit";
			this.NumberOfLabelsEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.NumberOfLabelsEdit.TabIndex = 2;
			this.NumberOfLabelsEdit.Text = "0";
			this.NumberOfLabelsEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NumberOfLabelsEdit.WordWrap = false;
			// 
			// LabelOptionsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 170, true);
			this.ControlBox = false;
			this.Controls.Add(this.OptionsGroupBox);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.PrintButton);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDocketLabelControl);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsDocketLabelControl";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "LabelOptionsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionsGroupBox.ResumeLayout(false);
			this.NumberOfLabelsPanel.ResumeLayout(false);
			this.NumberOfLabelsPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		private Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox OptionsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel NumberOfLabelsPanel;
		private Enterprise.ZArchitecture.ZCalcEdit NumberOfLabelsEdit;
		private Enterprise.ZArchitecture.ZLabel NumberOfLabelsDescription;
	}
}
