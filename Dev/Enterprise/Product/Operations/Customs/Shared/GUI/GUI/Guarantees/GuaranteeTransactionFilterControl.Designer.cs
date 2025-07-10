using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI.Guarantees
{
	partial class GuaranteeTransactionFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, "CusGuaranteeLineTransactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).CPL_TransactionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).CPL_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).TransactionTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).CPL_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).CPL_TranValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).CPL_Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).CPL_AppId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).CPL_TransactionStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).TransactionStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeLineTransaction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeLineTransactions)).SyncRoot)).CPL_Procedure)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F98C3B7B-50B6-4E9B-9D95-ED0D8CB92BC0", "Transaction Date");
			zTextBoxColumnStyleInfo1.ColumnName = "CPL_TransactionDate";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("fab3a09f-e01b-4826-a4f2-3dc1b63498a1", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "CPL_TransactionType";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1c607667-207a-4ce8-8213-f9da06665b18", "Type Description");
			zTextBoxColumnStyleInfo2.ColumnName = "TransactionTypeDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("c00ed4aa-3da3-47ae-bdd5-f2c69cc23823", "Reference");
			zTextBoxColumnStyleInfo3.ColumnName = "CPL_Reference";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("f502c65e-44c6-4832-aa59-4c70995d9f70", "Value");
			zCalcEditColumnStyleInfo1.ColumnName = "CPL_TranValue";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("a4193f31-3aa4-4d0f-95ec-a6a517fba80b", "Comment");
			zTextBoxColumnStyleInfo4.ColumnName = "CPL_Comment";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("b59180fb-ab1b-4de5-a686-fb92f9546e03", "Application Id");
			zTextBoxColumnStyleInfo5.ColumnName = "CPL_AppId";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("9588adbe-8f13-4df2-aeb1-072b37aa5d38", "Status");
			zTextBoxColumnStyleInfo6.ColumnName = "CPL_TransactionStatus";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4A98EAC9-E91A-4D2F-A1A0-BCE5D04A0E28", "Status Description");
			zTextBoxColumnStyleInfo7.ColumnName = "TransactionStatusDescription";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7B4152C2 - 8B9F - 4932 - 9543 - 6D4B2A5FCE48", "Procedure");
			zTextBoxColumnStyleInfo8.ColumnName = "CPL_Procedure";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.GridId = "C715C60B-7969-4ADB-8885-976D0F3DE95B";
			this.grid.LayoutKey = "GuaranteeTransactionsGrid";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.grid.ReadOnly = false;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 82, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseCusGuaranteeHeader);
			// 
			// GuaranteeTransactionFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "GuaranteeTransactionFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 182, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
