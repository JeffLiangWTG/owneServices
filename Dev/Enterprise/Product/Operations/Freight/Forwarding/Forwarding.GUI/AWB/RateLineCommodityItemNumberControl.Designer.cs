using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	partial class RateLineCommodityItemNumberControl
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
			this.CommodityNumberItemTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CommodityItemNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommodityItemNumberFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommodityItemNumberDropEdit.SuspendLayout();
			this.CommodityItemNumberFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine);
			// 
			// CommodityNumberItemTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommodityNumberItemTextBox, "ER_CommodityItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine)(null)).ER_CommodityItemNumber)));
			this.CommodityNumberItemTextBox.CaptionResourceString = null;
			this.CommodityNumberItemTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityNumberItemTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityNumberItemTextBox.Name = "CommodityNumberItemTextBox";
			this.CommodityNumberItemTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CommodityNumberItemTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityNumberItemTextBox.TabIndex = 0;
			// 
			// CommodityItemNumberDropEdit
			// 
			this.CommodityItemNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityItemNumberDropEdit, "ER_CommodityItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine)(null)).ER_CommodityItemNumber)));
			this.CommodityItemNumberDropEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityItemNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityItemNumberDropEdit.Name = "CommodityItemNumberDropEdit";
			this.CommodityItemNumberDropEdit.PreBoundMaxLength = 3;
			this.CommodityItemNumberDropEdit.ShouldResizeByMaxLength = true;
			this.CommodityItemNumberDropEdit.ShowDescriptionBox = false;
			this.CommodityItemNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CommodityItemNumberDropEdit.TabIndex = 1;
			this.CommodityItemNumberDropEdit.Visible = false;
			// 
			// CommodityItemNumberFindBox
			// 
			this.CommodityItemNumberFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityItemNumberFindBox, "ER_CommodityItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine)(null)).ER_CommodityItemNumber)));
			this.CommodityItemNumberFindBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityItemNumberFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityItemNumberFindBox.Name = "CommodityItemNumberFindBox";
			this.CommodityItemNumberFindBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CommodityItemNumberFindBox.TabIndex = 2;
			this.CommodityItemNumberFindBox.Visible = false;
			this.CommodityItemNumberFindBox.CodeBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			// 
			// RateLineCommodityItemNumberControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommodityItemNumberDropEdit);
			this.Controls.Add(this.CommodityItemNumberFindBox);
			this.Controls.Add(this.CommodityNumberItemTextBox);
			this.Name = "RateLineCommodityItemNumberControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommodityItemNumberDropEdit.ResumeLayout(true);
			this.CommodityItemNumberDropEdit.PerformLayout();
			this.CommodityItemNumberFindBox.ResumeLayout(true);
			this.CommodityItemNumberFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CommodityNumberItemTextBox;
		private ZArchitecture.GUI.ZDropEdit CommodityItemNumberDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CommodityItemNumberFindBox;
	}
}
