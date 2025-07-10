using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.GUI
{
	partial class CalculateExchangeRateForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		private new void InitializeComponent()
		{
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EqualLabel = new Enterprise.ZArchitecture.ZLabel();
			this.QuoteCurrencyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.QuoteCurrencyValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BaseCurrencyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BaseCurrencyValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefExchangeRateCalculator);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CalculateExchangeRateForm|EB28F7F1-6F6E-4A36-AB70-C793B9CE69E5", "Cancel");
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 74, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 24, true);
			this.ButtonCancel.TabIndex = 4;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOK.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CalculateExchangeRateForm|CDAB6723-8099-4EF5-98BE-9AD2AD664C94", "OK");
			this.ButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 74, true);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 24, true);
			this.ButtonOK.TabIndex = 3;
			this.ButtonOK.ToolTipCaption = null;
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// EqualLabel
			// 
			this.EqualLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CalculateExchangeRateForm|4D29E100-7AE8-42A2-8878-06FA4B5A53FD", "=");
			this.EqualLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EqualLabel, false);
			this.EqualLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 26, true);
			this.EqualLabel.Name = "EqualLabel";
			this.EqualLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 24, true);
			this.EqualLabel.TabIndex = 5;
			this.EqualLabel.UseMnemonic = false;
			// 
			// QuoteCurrencyLabel
			// 
			this.BindingSource.SetBindingMember(this.QuoteCurrencyLabel, "QuoteCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefExchangeRateCalculator)(null)).QuoteCurrency)));
			this.QuoteCurrencyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QuoteCurrencyLabel, false);
			this.QuoteCurrencyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 26, true);
			this.QuoteCurrencyLabel.Name = "QuoteCurrencyLabel";
			this.QuoteCurrencyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 24, true);
			this.QuoteCurrencyLabel.TabIndex = 6;
			this.QuoteCurrencyLabel.UseMnemonic = false;
			// 
			// QuoteCurrencyValueCalcEdit
			// 
			this.QuoteCurrencyValueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.QuoteCurrencyValueCalcEdit, "QuoteCurrencyValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefExchangeRateCalculator)(null)).QuoteCurrencyValue)));
			this.QuoteCurrencyValueCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QuoteCurrencyValueCalcEdit, false);
			this.QuoteCurrencyValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 28, true);
			this.QuoteCurrencyValueCalcEdit.Name = "QuoteCurrencyValueCalcEdit";
			this.QuoteCurrencyValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.QuoteCurrencyValueCalcEdit.TabIndex = 1;
			this.QuoteCurrencyValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.QuoteCurrencyValueCalcEdit.TrackDisposedAccess = true;
			// 
			// BaseCurrencyLabel
			// 
			this.BindingSource.SetBindingMember(this.BaseCurrencyLabel, "BaseCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefExchangeRateCalculator)(null)).BaseCurrency)));
			this.BaseCurrencyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BaseCurrencyLabel, false);
			this.BaseCurrencyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 26, true);
			this.BaseCurrencyLabel.Name = "BaseCurrencyLabel";
			this.BaseCurrencyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 24, true);
			this.BaseCurrencyLabel.TabIndex = 7;
			this.BaseCurrencyLabel.UseMnemonic = false;
			// 
			// BaseCurrencyValueCalcEdit
			// 
			this.BaseCurrencyValueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.BaseCurrencyValueCalcEdit, "BaseCurrencyValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefExchangeRateCalculator)(null)).BaseCurrencyValue)));
			this.BaseCurrencyValueCalcEdit.DecimalPlaces = 6;
			this.BaseCurrencyValueCalcEdit.Decimals = 6;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BaseCurrencyValueCalcEdit, false);
			this.BaseCurrencyValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 28, true);
			this.BaseCurrencyValueCalcEdit.Name = "BaseCurrencyValueCalcEdit";
			this.BaseCurrencyValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.BaseCurrencyValueCalcEdit.TabIndex = 2;
			this.BaseCurrencyValueCalcEdit.Text = "0.000000";
			this.BaseCurrencyValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.BaseCurrencyValueCalcEdit.TrackDisposedAccess = true;
			// 
			// CalculateExchangeRateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.ButtonCancel;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CalculateExchangeRateForm|E8032915-ABB0-46C3-A39E-04566D33865C", "Calculate Ex. Rate");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 138, true);
			this.ControlBox = false;
			this.Controls.Add(this.BaseCurrencyValueCalcEdit);
			this.Controls.Add(this.BaseCurrencyLabel);
			this.Controls.Add(this.QuoteCurrencyValueCalcEdit);
			this.Controls.Add(this.QuoteCurrencyLabel);
			this.Controls.Add(this.EqualLabel);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefExchangeRateCalculator);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 154, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 154, true);
			this.Name = "CalculateExchangeRateForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonOK, 0);
			this.Controls.SetChildIndex(this.ButtonCancel, 0);
			this.Controls.SetChildIndex(this.EqualLabel, 0);
			this.Controls.SetChildIndex(this.QuoteCurrencyLabel, 0);
			this.Controls.SetChildIndex(this.QuoteCurrencyValueCalcEdit, 0);
			this.Controls.SetChildIndex(this.BaseCurrencyLabel, 0);
			this.Controls.SetChildIndex(this.BaseCurrencyValueCalcEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZButton ButtonCancel;
		public ZArchitecture.GUI.ZButton ButtonOK;
		private ZArchitecture.ZLabel EqualLabel;
		internal ZArchitecture.ZLabel QuoteCurrencyLabel;
		internal ZArchitecture.ZCalcEdit QuoteCurrencyValueCalcEdit;
		internal ZArchitecture.ZLabel BaseCurrencyLabel;
		internal ZArchitecture.ZCalcEdit BaseCurrencyValueCalcEdit;
	}
}
