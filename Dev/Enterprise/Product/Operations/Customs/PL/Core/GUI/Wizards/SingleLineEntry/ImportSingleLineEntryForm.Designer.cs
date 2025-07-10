namespace Enterprise.Customs.PL.GUI
{
	partial class ImportSingleLineEntryForm
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
		new private void InitializeComponent()
		{
			this.PreviousDocumentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PreviousDocumentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailsGroupBox.SuspendLayout();
			this.CPCFindBox.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsOriginCodeFindBox.SuspendLayout();
			this.PreviousDocumentDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 306, true);
			// 
			// CancelZButton
			// 
			this.CancelZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 306, true);
			// 
			// PriceCalcEdit
			// 
			this.PriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 152, true);
			this.PriceCalcEdit.TabIndex = 6;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.GoodsOriginCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.PreviousDocumentDropEdit);
			this.DetailsGroupBox.Controls.Add(this.PreviousDocumentNumberTextBox);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 230, true);
			this.DetailsGroupBox.Controls.SetChildIndex(this.PreviousDocumentNumberTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.PreviousDocumentDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.GoodsOriginCodeFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.PriceCalcEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.CPCFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.InvoiceNumberTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.zGuidFindBox1, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.zLabel2, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.CheckBoxLic99, 0);
			// 
			// CPCFindBox
			// 
			this.CPCFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 56, true);
			this.CPCFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.CPCFindBox.TabIndex = 2;
			// 
			// InvoiceNumberTextBox
			// 
			this.InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 80, true);
			this.InvoiceNumberTextBox.TabIndex = 3;
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 180, true);
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 14, true);
			this.zLabel2.TabIndex = 9;
			// 
			// NetWeightCalcEdit
			// 
			this.NetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 176, true);
			this.NetWeightCalcEdit.TabIndex = 8;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 152, true);
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 18, true);
			this.zGuidFindBox1.TabIndex = 7;
			// 
			// CheckBoxLic99
			// 
			this.CheckBoxLic99.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 200, true);
			this.CheckBoxLic99.TabIndex = 10;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 341, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.ImportSingleLineEntry);
			// 
			// PreviousDocumentNumberTextBox
			// 
			this.PreviousDocumentNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PreviousDocumentNumberTextBox, "PreviousDocumentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.ImportSingleLineEntry)(null)).PreviousDocumentNumber)));
			this.PreviousDocumentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 128, true);
			this.PreviousDocumentNumberTextBox.Name = "PreviousDocumentNumberTextBox";
			this.PreviousDocumentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 18, true);
			this.PreviousDocumentNumberTextBox.TabIndex = 5;
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "GoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.ImportSingleLineEntry)(null)).GoodsOrigin)));
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 33, true);
			this.GoodsOriginCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.PreBoundMaxLength = 2;
			this.GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 18, true);
			this.GoodsOriginCodeFindBox.TabIndex = 1;
			// 
			// PreviousDocumentDropEdit
			// 
			this.PreviousDocumentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreviousDocumentDropEdit, "PreviousDocument");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.ImportSingleLineEntry)(null)).PreviousDocument)));
			this.PreviousDocumentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 104, true);
			this.PreviousDocumentDropEdit.Name = "PreviousDocumentDropEdit";
			this.PreviousDocumentDropEdit.PreBoundMaxLength = 4;
			this.PreviousDocumentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 18, true);
			this.PreviousDocumentDropEdit.TabIndex = 4;
			// 
			// ImportSingleLineEntryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 365, true);
			this.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.ImportSingleLineEntry);
			this.Name = "ImportSingleLineEntryForm";
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.CPCFindBox.ResumeLayout(true);
			this.CPCFindBox.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsOriginCodeFindBox.ResumeLayout(true);
			this.GoodsOriginCodeFindBox.PerformLayout();
			this.PreviousDocumentDropEdit.ResumeLayout(true);
			this.PreviousDocumentDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox PreviousDocumentNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox GoodsOriginCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit PreviousDocumentDropEdit;
	}
}
