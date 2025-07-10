namespace Enterprise.Customs.TW.GUI
{
	partial class CustomsEntryConfirmedAmountUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BusinessTaxBaseAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalCashTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalNonCashTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ConfirmedBusinessTaxBaseAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ConfirmedTotalCashTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ConfirmedTotalNonCashTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CalculatedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConfirmedLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// BusinessTaxBaseAmountCalcEdit
			// 
			this.BusinessTaxBaseAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BusinessTaxBaseAmountCalcEdit, "CustomsEntryHeaders.BusinessTaxBaseAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).BusinessTaxBaseAmount)));
			this.BusinessTaxBaseAmountCalcEdit.DecimalPlaces = 0;
			this.BusinessTaxBaseAmountCalcEdit.Decimals = 0;
			this.BusinessTaxBaseAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 32, true);
			this.BusinessTaxBaseAmountCalcEdit.Name = "BusinessTaxBaseAmountCalcEdit";
			this.BusinessTaxBaseAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.BusinessTaxBaseAmountCalcEdit.TabIndex = 2;
			this.BusinessTaxBaseAmountCalcEdit.Text = "0";
			this.BusinessTaxBaseAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.BusinessTaxBaseAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalCashTaxAmountCalcEdit
			// 
			this.TotalCashTaxAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalCashTaxAmountCalcEdit, "CustomsEntryHeaders.TotalCashTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalCashTaxAmount)));
			this.TotalCashTaxAmountCalcEdit.DecimalPlaces = 0;
			this.TotalCashTaxAmountCalcEdit.Decimals = 0;
			this.TotalCashTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 58, true);
			this.TotalCashTaxAmountCalcEdit.Name = "TotalCashTaxAmountCalcEdit";
			this.TotalCashTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.TotalCashTaxAmountCalcEdit.TabIndex = 4;
			this.TotalCashTaxAmountCalcEdit.Text = "0";
			this.TotalCashTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCashTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalNonCashTaxAmountCalcEdit
			// 
			this.TotalNonCashTaxAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalNonCashTaxAmountCalcEdit, "CustomsEntryHeaders.TotalNonCashTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalNonCashTaxAmount)));
			this.TotalNonCashTaxAmountCalcEdit.DecimalPlaces = 0;
			this.TotalNonCashTaxAmountCalcEdit.Decimals = 0;
			this.TotalNonCashTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 84, true);
			this.TotalNonCashTaxAmountCalcEdit.Name = "TotalNonCashTaxAmountCalcEdit";
			this.TotalNonCashTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.TotalNonCashTaxAmountCalcEdit.TabIndex = 6;
			this.TotalNonCashTaxAmountCalcEdit.Text = "0";
			this.TotalNonCashTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalNonCashTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// ConfirmedBusinessTaxBaseAmountCalcEdit
			// 
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ConfirmedBusinessTaxBaseAmountCalcEdit, "CustomsEntryHeaders.CH_ConfirmedBusinessTaxBase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_ConfirmedBusinessTaxBase)));
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.DecimalPlaces = 0;
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConfirmedBusinessTaxBaseAmountCalcEdit, false);
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 32, true);
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.Name = "ConfirmedBusinessTaxBaseAmountCalcEdit";
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.TabIndex = 3;
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.Text = "0";
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ConfirmedBusinessTaxBaseAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// ConfirmedTotalCashTaxAmountCalcEdit
			// 
			this.ConfirmedTotalCashTaxAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ConfirmedTotalCashTaxAmountCalcEdit, "CustomsEntryHeaders.CH_ConfirmedTotalDutyTaxFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_ConfirmedTotalDutyTaxFee)));
			this.ConfirmedTotalCashTaxAmountCalcEdit.DecimalPlaces = 0;
			this.ConfirmedTotalCashTaxAmountCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConfirmedTotalCashTaxAmountCalcEdit, false);
			this.ConfirmedTotalCashTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 58, true);
			this.ConfirmedTotalCashTaxAmountCalcEdit.Name = "ConfirmedTotalCashTaxAmountCalcEdit";
			this.ConfirmedTotalCashTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.ConfirmedTotalCashTaxAmountCalcEdit.TabIndex = 5;
			this.ConfirmedTotalCashTaxAmountCalcEdit.Text = "0";
			this.ConfirmedTotalCashTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ConfirmedTotalCashTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// ConfirmedTotalNonCashTaxAmountCalcEdit
			// 
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ConfirmedTotalNonCashTaxAmountCalcEdit, "CustomsEntryHeaders.CH_ConfirmedTotalDutyTaxFeeDeferred");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_ConfirmedTotalDutyTaxFeeDeferred)));
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.DecimalPlaces = 0;
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConfirmedTotalNonCashTaxAmountCalcEdit, false);
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 84, true);
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.Name = "ConfirmedTotalNonCashTaxAmountCalcEdit";
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.TabIndex = 7;
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.Text = "0";
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ConfirmedTotalNonCashTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// CalculatedLabel
			// 
			this.CalculatedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CalculatedLabel.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("be16a155-1d27-42e4-99e9-e400c04c11e3", "Calculated");
			this.CalculatedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CalculatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 6, true);
			this.CalculatedLabel.Name = "CalculatedLabel";
			this.CalculatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CalculatedLabel.TabIndex = 0;
			this.CalculatedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.CalculatedLabel.UseMnemonic = false;
			// 
			// ConfirmedLabel
			// 
			this.ConfirmedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ConfirmedLabel.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("aaf6ed50-6cb2-41c2-90e7-234c9df002c0", "Confirmed");
			this.ConfirmedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ConfirmedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 6, true);
			this.ConfirmedLabel.Name = "ConfirmedLabel";
			this.ConfirmedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ConfirmedLabel.TabIndex = 1;
			this.ConfirmedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ConfirmedLabel.UseMnemonic = false;
			// 
			// CustomsEntryConfirmedAmountUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConfirmedLabel);
			this.Controls.Add(this.CalculatedLabel);
			this.Controls.Add(this.ConfirmedBusinessTaxBaseAmountCalcEdit);
			this.Controls.Add(this.ConfirmedTotalCashTaxAmountCalcEdit);
			this.Controls.Add(this.ConfirmedTotalNonCashTaxAmountCalcEdit);
			this.Controls.Add(this.BusinessTaxBaseAmountCalcEdit);
			this.Controls.Add(this.TotalCashTaxAmountCalcEdit);
			this.Controls.Add(this.TotalNonCashTaxAmountCalcEdit);
			this.Name = "CustomsEntryConfirmedAmountUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 114, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit BusinessTaxBaseAmountCalcEdit;
		private ZArchitecture.ZCalcEdit TotalCashTaxAmountCalcEdit;
		private ZArchitecture.ZCalcEdit TotalNonCashTaxAmountCalcEdit;
		private ZArchitecture.ZCalcEdit ConfirmedBusinessTaxBaseAmountCalcEdit;
		private ZArchitecture.ZCalcEdit ConfirmedTotalCashTaxAmountCalcEdit;
		private ZArchitecture.ZCalcEdit ConfirmedTotalNonCashTaxAmountCalcEdit;
		private ZArchitecture.ZLabel CalculatedLabel;
		private ZArchitecture.ZLabel ConfirmedLabel;
	}
}
