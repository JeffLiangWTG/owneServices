namespace Enterprise.Customs.ZA.Module
{
	partial class ProofOfPaymentFilterControl
	{
		private System.ComponentModel.IContainer components = null;

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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.grid.SuspendLayout();
            this.AddStripButton.SuspendLayout();
            this.RecentItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.BindingSource.SetBindingMember(this.grid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).Importer.OH_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).JobNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).FANumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).C9_PaymentDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).CustomsOffice)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).LRNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).MRNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).C9_PaymentAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).C9_PaymentReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).TotalVat)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).C9_ReceiptDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).AgentsReference)));
            zTextBoxColumnStyleInfo1.Caption = "";
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("fd494a49-63da-440f-986c-07bfaf8287e1", "Importer");
            zTextBoxColumnStyleInfo1.ColumnName = "Importer+OH_Code";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsMandatory = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo2.Caption = "";
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("bbf60bb5-6e12-4346-aac0-167925b4942c", "Job Number");
            zTextBoxColumnStyleInfo2.ColumnName = "JobNumber";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsMandatory = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zTextBoxColumnStyleInfo3.Caption = "";
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("334b9b04-1272-4117-aea3-08cd919e331b", "Financial Account Number");
            zTextBoxColumnStyleInfo3.ColumnName = "FANumber";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.IsMandatory = true;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDateEditColumnStyleInfo1.Caption = "";
            zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("6ae60794-7e58-496f-a2c0-ce9cf28c8ace", "Transaction Date");
            zDateEditColumnStyleInfo1.ColumnName = "C9_PaymentDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo4.Caption = "";
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("8aa95606-517f-40fa-9733-581fea8ce7fd", "Customs Office");
            zTextBoxColumnStyleInfo4.ColumnName = "CustomsOffice";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsMandatory = true;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo5.Caption = "";
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("d7a7c127-8e16-407a-8b32-2b6952921dda", "Local Reference Number");
            zTextBoxColumnStyleInfo5.ColumnName = "LRNumber";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.IsMandatory = true;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            zTextBoxColumnStyleInfo6.Caption = "";
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("83269846-ac4f-4183-bad3-6d25ce8c2e85", "MRN");
            zTextBoxColumnStyleInfo6.ColumnName = "MRNumber";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.IsMandatory = true;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.Caption = "";
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("5de5d2b8-239f-43bd-be6b-7c35e45107b5", "VAT Amount");
            zCalcEditColumnStyleInfo1.ColumnName = "C9_PaymentAmount";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo7.Caption = "";
            zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("6a52afc5-0870-4d05-91c0-2a05c51bbd34", "Receipt Number");
            zTextBoxColumnStyleInfo7.ColumnName = "C9_PaymentReference";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.IsMandatory = true;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.Caption = "";
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("8c49f4a2-0ea7-4b23-b2fd-7cba1db7d30d", "Total VAT on Receipt");
            zCalcEditColumnStyleInfo2.ColumnName = "TotalVat";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
            zDateEditColumnStyleInfo2.Caption = "";
            zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("485414cf-b29b-4468-90b1-6822d63530dc", "Receipt Date");
            zDateEditColumnStyleInfo2.ColumnName = "C9_ReceiptDate";
            zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo8.Caption = "";
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("c9d47ed8-ed2b-4505-8e67-30429669cd9d", "Agents Reference");
            zTextBoxColumnStyleInfo8.ColumnName = "AgentsReference";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.IsMandatory = true;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 22, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.CusEntryPayInfo);
            // 
            // ProofOfPaymentFilterControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Name = "ProofOfPaymentFilterControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 174, true);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.grid.ResumeLayout(false);
            this.grid.PerformLayout();
            this.AddStripButton.ResumeLayout(true);
            this.AddStripButton.PerformLayout();
            this.RecentItemsPanel.ResumeLayout(false);
            this.RecentItemsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
	}
}