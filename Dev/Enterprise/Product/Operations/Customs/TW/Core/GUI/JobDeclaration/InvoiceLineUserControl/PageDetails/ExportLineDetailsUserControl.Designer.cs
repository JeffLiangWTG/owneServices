namespace Enterprise.Customs.TW.GUI
{
	partial class ExportLineDetailsUserControl
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
			this.BondedGoodsCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineDetailsGroupBox.SuspendLayout();
			this.UNDGCodeFindBox.SuspendLayout();
			this.JI_ProcedureDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BondedGoodsCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// LineDetailsGroupBox
			// 
			this.LineDetailsGroupBox.Controls.Add(this.BondedGoodsCodeDropEdit);
			this.LineDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 342, true);
			this.LineDetailsGroupBox.Controls.SetChildIndex(this.UNDGCodeFindBox, 0);
			this.LineDetailsGroupBox.Controls.SetChildIndex(this.BondedGoodsCodeDropEdit, 0);
			this.LineDetailsGroupBox.Controls.SetChildIndex(this.JI_ProcedureDropEdit, 0);
			// 
			// UNDGCodeFindBox
			// 
			this.UNDGCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 276, true);
			this.UNDGCodeFindBox.TabIndex = 11;
			// 
			// JI_ProcedureDropEdit
			// 
			this.JI_ProcedureDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("3A8ECDCA-8348-4CE3-A012-97EB2AD679F6", "Mode of Statistics", "The customs statistics code that indicates the trading type of exported goods.");
			// 
			// BondedGoodsCodeDropEdit
			// 
			this.BondedGoodsCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondedGoodsCodeDropEdit, "FilteredInvoiceLines.JI_BondedGoodsCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BondedGoodsCode)));
			this.BondedGoodsCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 250, true);
			this.BondedGoodsCodeDropEdit.Name = "BondedGoodsCodeDropEdit";
			this.BondedGoodsCodeDropEdit.PreBoundMaxLength = 3;
			this.BondedGoodsCodeDropEdit.ShouldResizeByMaxLength = false;
			this.BondedGoodsCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 20, true);
			this.BondedGoodsCodeDropEdit.TabIndex = 10;
			// 
			// ExportLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ExportLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(969, 342, true);
			this.LineDetailsGroupBox.ResumeLayout(false);
			this.LineDetailsGroupBox.PerformLayout();
			this.UNDGCodeFindBox.ResumeLayout(true);
			this.UNDGCodeFindBox.PerformLayout();
			this.JI_ProcedureDropEdit.ResumeLayout(true);
			this.JI_ProcedureDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BondedGoodsCodeDropEdit.ResumeLayout(true);
			this.BondedGoodsCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit BondedGoodsCodeDropEdit;
	}
}
