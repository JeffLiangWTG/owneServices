using Enterprise.Customs.GUI;

namespace Enterprise.Customs.NO.GUI
{
	partial class EntryDutiesUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.VerticalSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryDutiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TotalCalcEdit = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.VATCalcEdit = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.ExciseDutiesCalcEdit = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.CustomsDutyCalcEdit = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VerticalSplitContainer)).BeginInit();
			this.VerticalSplitContainer.Panel1.SuspendLayout();
			this.VerticalSplitContainer.Panel2.SuspendLayout();
			this.VerticalSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryDutiesGrid)).BeginInit();
			this.EntryDutiesGrid.SuspendLayout();
			this.TotalCalcEdit.SuspendLayout();
			this.VATCalcEdit.SuspendLayout();
			this.ExciseDutiesCalcEdit.SuspendLayout();
			this.CustomsDutyCalcEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			// 
			// VerticalSplitContainer
			// 
			this.VerticalSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.VerticalSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VerticalSplitContainer.Name = "VerticalSplitContainer";
			// 
			// VerticalSplitContainer.Panel1
			// 
			this.VerticalSplitContainer.Panel1.Controls.Add(this.EntryDutiesGrid);
			// 
			// VerticalSplitContainer.Panel2
			// 
			this.VerticalSplitContainer.Panel2.Controls.Add(this.TotalCalcEdit);
			this.VerticalSplitContainer.Panel2.Controls.Add(this.VATCalcEdit);
			this.VerticalSplitContainer.Panel2.Controls.Add(this.ExciseDutiesCalcEdit);
			this.VerticalSplitContainer.Panel2.Controls.Add(this.CustomsDutyCalcEdit);
			this.VerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 632, true);
			this.VerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(882);
			this.VerticalSplitContainer.TabIndex = 0;
			// 
			// EntryDutiesGrid
			// 
			this.EntryDutiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryDutiesGrid, "CustomsEntryHeaders.Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryHeaderFee)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).Fees)).SyncRoot)).Duty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.CusEntryHeaderFee)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).Fees)).SyncRoot)).Amount)));
			this.EntryDutiesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Duty";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryDutiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryDutiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryDutiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryDutiesGrid.GridId = "fc30847f-af1a-40b6-acc8-6c78cc221750";
			this.EntryDutiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryDutiesGrid.LayoutKey = "zGrid1";
			this.EntryDutiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryDutiesGrid.Name = "EntryDutiesGrid";
			this.EntryDutiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 632, true);
			this.EntryDutiesGrid.TabIndex = 0;
			// 
			// TotalCalcEdit
			// 
			this.TotalCalcEdit.AllowDrop = true;
			this.TotalCalcEdit.BindToAmount = "CustomsEntryHeaders.TotalAmount";
			this.TotalCalcEdit.BindToList = "Lookups+CurrencyList";
			this.TotalCalcEdit.BindToUnit = "LocalCurrencyCode";
			this.TotalCalcEdit.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("d3b24d58-aa08-4210-a5b7-89f7d4d15cf9", "Total");
			this.TotalCalcEdit.Decimals = 0;
			this.TotalCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 83, true);
			this.TotalCalcEdit.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.TotalCalcEdit.Name = "TotalCalcEdit";
			this.TotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalCalcEdit.TabIndex = 3;
			// 
			// VATCalcEdit
			// 
			this.VATCalcEdit.AllowDrop = true;
			this.VATCalcEdit.BindToAmount = "CustomsEntryHeaders.VatAmount";
			this.VATCalcEdit.BindToList = "Lookups+CurrencyList";
			this.VATCalcEdit.BindToUnit = "LocalCurrencyCode";
			this.VATCalcEdit.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("7eb15f72-332d-40d1-ae51-7b7aee090e30", "VAT");
			this.VATCalcEdit.Decimals = 0;
			this.VATCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.VATCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 57, true);
			this.VATCalcEdit.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.VATCalcEdit.Name = "VATCalcEdit";
			this.VATCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.VATCalcEdit.TabIndex = 2;
			// 
			// ExciseDutiesCalcEdit
			// 
			this.ExciseDutiesCalcEdit.AllowDrop = true;
			this.ExciseDutiesCalcEdit.BindToAmount = "CustomsEntryHeaders.ExciseDutyAmount";
			this.ExciseDutiesCalcEdit.BindToList = "Lookups+CurrencyList";
			this.ExciseDutiesCalcEdit.BindToUnit = "LocalCurrencyCode";
			this.ExciseDutiesCalcEdit.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("587be366-4711-4aaa-84e7-aadb8d4a6686", "Excise duties");
			this.ExciseDutiesCalcEdit.Decimals = 0;
			this.ExciseDutiesCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ExciseDutiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 31, true);
			this.ExciseDutiesCalcEdit.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.ExciseDutiesCalcEdit.Name = "ExciseDutiesCalcEdit";
			this.ExciseDutiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExciseDutiesCalcEdit.TabIndex = 1;
			// 
			// CustomsDutyCalcEdit
			// 
			this.CustomsDutyCalcEdit.AllowDrop = true;
			this.CustomsDutyCalcEdit.BindToAmount = "CustomsEntryHeaders.CustomsDutyAmount";
			this.CustomsDutyCalcEdit.BindToList = "Lookups+CurrencyList";
			this.CustomsDutyCalcEdit.BindToUnit = "LocalCurrencyCode";
			this.CustomsDutyCalcEdit.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("22e43a51-dba0-48ac-b8e0-8c8da680a92d", "Customs duty");
			this.CustomsDutyCalcEdit.Decimals = 0;
			this.CustomsDutyCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.CustomsDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 5, true);
			this.CustomsDutyCalcEdit.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.CustomsDutyCalcEdit.Name = "CustomsDutyCalcEdit";
			this.CustomsDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CustomsDutyCalcEdit.TabIndex = 0;
			// 
			// EntryDutiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.VerticalSplitContainer);
			this.Name = "EntryDutiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 635, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VerticalSplitContainer.Panel1.ResumeLayout(false);
			this.VerticalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.VerticalSplitContainer)).EndInit();
			this.VerticalSplitContainer.ResumeLayout(false);
			this.VerticalSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryDutiesGrid)).EndInit();
			this.EntryDutiesGrid.ResumeLayout(false);
			this.EntryDutiesGrid.PerformLayout();
			this.TotalCalcEdit.ResumeLayout(true);
			this.TotalCalcEdit.PerformLayout();
			this.VATCalcEdit.ResumeLayout(true);
			this.VATCalcEdit.PerformLayout();
			this.ExciseDutiesCalcEdit.ResumeLayout(true);
			this.ExciseDutiesCalcEdit.PerformLayout();
			this.CustomsDutyCalcEdit.ResumeLayout(true);
			this.CustomsDutyCalcEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer VerticalSplitContainer;
		public ZArchitecture.ZGrid EntryDutiesGrid;
		public ConvertToLocalCurrencyControl TotalCalcEdit;
		public ConvertToLocalCurrencyControl VATCalcEdit;
		public ConvertToLocalCurrencyControl ExciseDutiesCalcEdit;
		public ConvertToLocalCurrencyControl CustomsDutyCalcEdit;
	}
}
