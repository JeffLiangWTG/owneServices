using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class RefExchangeRateForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.RE_RX_NKExCurrencyDropDownEdit = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RE_ExpiryDateDateTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RE_StartDateDateTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.RE_ExRateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RE_SellRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RE_AsPublishedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RE_OH_ClientGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RefExchangeRateCalculatorButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RE_RX_NKExCurrencyDropDownEdit.SuspendLayout();
			this.RE_ExpiryDateDateTime.SuspendLayout();
			this.RE_StartDateDateTime.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.RE_ExRateTypeDropEdit.SuspendLayout();
			this.RE_OH_ClientGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 263, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(897);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefExchangeRate);
			// 
			// RE_RX_NKExCurrencyDropDownEdit
			// 
			this.RE_RX_NKExCurrencyDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RE_RX_NKExCurrencyDropDownEdit, "RE_RX_NKExCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefExchangeRate)(null)).RE_RX_NKExCurrency)));
			this.RE_RX_NKExCurrencyDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 18, true);
			this.RE_RX_NKExCurrencyDropDownEdit.Name = "RE_RX_NKExCurrencyDropDownEdit";
			this.RE_RX_NKExCurrencyDropDownEdit.PreBoundMaxLength = 4;
			this.RE_RX_NKExCurrencyDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 18, true);
			this.RE_RX_NKExCurrencyDropDownEdit.TabIndex = 1;
			// 
			// RE_ExpiryDateDateTime
			// 
			this.RE_ExpiryDateDateTime.AllowDrop = true;
			this.RE_ExpiryDateDateTime.AutoCompleteMonthThreshold = 1;
			this.RE_ExpiryDateDateTime.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RE_ExpiryDateDateTime, "RE_ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefExchangeRate)(null)).RE_ExpiryDate)));
			this.RE_ExpiryDateDateTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 158, true);
			this.RE_ExpiryDateDateTime.Name = "RE_ExpiryDateDateTime";
			this.RE_ExpiryDateDateTime.TabIndex = 20;
			// 
			// RE_StartDateDateTime
			// 
			this.RE_StartDateDateTime.AllowDrop = true;
			this.RE_StartDateDateTime.AutoCompleteMonthThreshold = 1;
			this.RE_StartDateDateTime.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RE_StartDateDateTime, "RE_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefExchangeRate)(null)).RE_StartDate)));
			this.RE_StartDateDateTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 130, true);
			this.RE_StartDateDateTime.Name = "RE_StartDateDateTime";
			this.RE_StartDateDateTime.TabIndex = 15;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 236, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 4;
			// 
			// RE_ExRateTypeDropEdit
			// 
			this.RE_ExRateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RE_ExRateTypeDropEdit, "RE_ExRateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefExchangeRate)(null)).RE_ExRateType)));
			this.RE_ExRateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 46, true);
			this.RE_ExRateTypeDropEdit.Name = "RE_ExRateTypeDropEdit";
			this.RE_ExRateTypeDropEdit.PreBoundMaxLength = 4;
			this.RE_ExRateTypeDropEdit.ShouldResizeByMaxLength = true;
			this.RE_ExRateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 18, true);
			this.RE_ExRateTypeDropEdit.TabIndex = 5;
			// 
			// RE_SellRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RE_SellRateCalcEdit, "RE_SellRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefExchangeRate)(null)).RE_SellRate)));
			this.RE_SellRateCalcEdit.CaptionResourceString = null;
			this.RE_SellRateCalcEdit.DecimalPlaces = 6;
			this.RE_SellRateCalcEdit.Decimals = 6;
			this.RE_SellRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 74, true);
			this.RE_SellRateCalcEdit.Name = "RE_SellRateCalcEdit";
			this.RE_SellRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.RE_SellRateCalcEdit.TabIndex = 10;
			this.RE_SellRateCalcEdit.Text = "0,000000000";
			this.RE_SellRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RE_AsPublishedTextBox
			// 
			this.BindingSource.SetBindingMember(this.RE_AsPublishedTextBox, "RE_AsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefExchangeRate)(null)).RE_AsPublished)));
			this.RE_AsPublishedTextBox.CaptionResourceString = null;
			this.RE_AsPublishedTextBox.MaxLength = 35;
			this.RE_AsPublishedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 102, true);
			this.RE_AsPublishedTextBox.Name = "RE_AsPublishedTextBox";
			this.RE_AsPublishedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 18, true);
			this.RE_AsPublishedTextBox.TabIndex = 11;
			this.RE_AsPublishedTextBox.Text = "0,00000000";
			this.RE_AsPublishedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RE_AsPublishedTextBox.Visible = false;
			// 
			// RE_OH_ClientGuidFindBox
			// 
			RE_OH_ClientGuidFindBox.AllowDrop = true;
			BindingSource.SetBindingMember(RE_OH_ClientGuidFindBox, "RE_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefExchangeRate)(null)).RE_OH_Client)));
			RE_OH_ClientGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 186, true);
			RE_OH_ClientGuidFindBox.Name = "RE_OH_ClientGuidFindBox";
			RE_OH_ClientGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			RE_OH_ClientGuidFindBox.TabIndex = 25;
			// 
			// RefExchangeRateCalculatorButton
			// 
			this.RefExchangeRateCalculatorButton.Font = OFont.GetFontBold();
			this.RefExchangeRateCalculatorButton.IsCaptionOverridden = true;
			this.RefExchangeRateCalculatorButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 74, true);
			this.RefExchangeRateCalculatorButton.Name = "RefExchangeRateCalculatorButton";
			this.RefExchangeRateCalculatorButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 18, true);
			this.RefExchangeRateCalculatorButton.TabIndex = 26;
			this.RefExchangeRateCalculatorButton.Text = "...";
			this.RefExchangeRateCalculatorButton.ToolTipCaption = null;
			this.RefExchangeRateCalculatorButton.UseVisualStyleBackColor = true;
			this.RefExchangeRateCalculatorButton.Click += new System.EventHandler(this.RefExchangeRateCalculatorButton_Click);
			// 
			// RefExchangeRateForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(685, 287, true);
			this.Controls.Add(this.RefExchangeRateCalculatorButton);
			this.Controls.Add(this.RE_OH_ClientGuidFindBox);
			this.Controls.Add(this.RE_AsPublishedTextBox);
			this.Controls.Add(this.RE_SellRateCalcEdit);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.RE_ExRateTypeDropEdit);
			this.Controls.Add(this.RE_RX_NKExCurrencyDropDownEdit);
			this.Controls.Add(this.RE_ExpiryDateDateTime);
			this.Controls.Add(this.RE_StartDateDateTime);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefExchangeRate);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.RefExchangeRate";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 325, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 325, true);
			this.Name = "RefExchangeRateForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.RE_StartDateDateTime, 0);
			this.Controls.SetChildIndex(this.RE_ExpiryDateDateTime, 0);
			this.Controls.SetChildIndex(this.RE_RX_NKExCurrencyDropDownEdit, 0);
			this.Controls.SetChildIndex(this.RE_ExRateTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.RE_SellRateCalcEdit, 0);
			this.Controls.SetChildIndex(this.RE_AsPublishedTextBox, 0);
			this.Controls.SetChildIndex(this.RE_OH_ClientGuidFindBox, 0);
			this.Controls.SetChildIndex(this.RefExchangeRateCalculatorButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RE_RX_NKExCurrencyDropDownEdit.ResumeLayout(true);
			this.RE_RX_NKExCurrencyDropDownEdit.PerformLayout();
			this.RE_ExpiryDateDateTime.ResumeLayout(true);
			this.RE_ExpiryDateDateTime.PerformLayout();
			this.RE_StartDateDateTime.ResumeLayout(true);
			this.RE_StartDateDateTime.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.RE_ExRateTypeDropEdit.ResumeLayout(true);
			this.RE_ExRateTypeDropEdit.PerformLayout();
			this.RE_OH_ClientGuidFindBox.ResumeLayout(true);
			this.RE_OH_ClientGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZDateEdit RE_ExpiryDateDateTime;
		Enterprise.ZArchitecture.GUI.ZDateEdit RE_StartDateDateTime;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox RE_RX_NKExCurrencyDropDownEdit;
		internal ZArchitecture.GUI.ZDropEdit RE_ExRateTypeDropEdit;
		private ZArchitecture.ZCalcEdit RE_SellRateCalcEdit;
		internal ZArchitecture.ZTextBox RE_AsPublishedTextBox;
		internal ZArchitecture.GUI.ZGuidFindBox RE_OH_ClientGuidFindBox;
		internal ZButton RefExchangeRateCalculatorButton;
	}
}
