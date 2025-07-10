using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Module
{
	partial class DGClassDGSubstanceFilterControl
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
			this.dgClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dgSubstanceFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.operatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.dgClassDropEdit.SuspendLayout();
			this.dgSubstanceFindBox.SuspendLayout();
			this.operatorDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Module.DGClassDGSubstanceFilter);
			// 
			// DGClassDropEdit
			// 
			this.dgClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dgClassDropEdit, "DGClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Module.DGClassDGSubstanceFilter)(null)).DGClass)));
			this.dgClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 1, true);
			this.dgClassDropEdit.Name = "DGClassDropEdit";
			this.dgClassDropEdit.ShouldResizeByMaxLength = false;
			this.dgClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.dgClassDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.dgClassDropEdit.CodeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.dgClassDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
			this.dgClassDropEdit.TabIndex = 1;
			// 
			// DGSubstanceFindBox
			// 
			this.dgSubstanceFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dgSubstanceFindBox, "DGSubstance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Module.DGClassDGSubstanceFilter)(null)).DGSubstance)));
			this.dgSubstanceFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("29E923E6-FD3E-4AAE-AE5E-68E6282FCF75", "DG Substance");
			this.dgSubstanceFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 25, true);
			this.dgSubstanceFindBox.Name = "DGSubstanceFindBox";
			this.dgSubstanceFindBox.ShowDescriptionBox = false;
			this.dgSubstanceFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.dgSubstanceFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.dgSubstanceFindBox.TabIndex = 2;
			// 
			// operatorDropEdit
			// 
			this.operatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.operatorDropEdit, "ComparisonOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Module.DGClassDGSubstanceFilter)(null)).ComparisonOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Module.DGClassDGSubstanceFilter)(null)).ComparisonOperator_List)));
			this.operatorDropEdit.BindToList = "ComparisonOperator_List";
			this.operatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 1, true);
			this.operatorDropEdit.Name = "operatorDropEdit";
			this.operatorDropEdit.ShouldResizeByMaxLength = true;
			this.operatorDropEdit.ShowDescriptionBox = false;
			this.operatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.operatorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.operatorDropEdit.CodeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.operatorDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
			this.operatorDropEdit.TabIndex = 0;
			// 
			// DGClassDGSubstanceFilterControl
			// 
			this.Controls.Add(this.operatorDropEdit);
			this.Controls.Add(this.dgSubstanceFindBox);
			this.Controls.Add(this.dgClassDropEdit);
			this.Name = "DGClassDGSubstanceFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 48, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.dgClassDropEdit.ResumeLayout(true);
			this.dgClassDropEdit.PerformLayout();
			this.dgSubstanceFindBox.ResumeLayout(true);
			this.dgSubstanceFindBox.PerformLayout();
			this.operatorDropEdit.ResumeLayout(true);
			this.operatorDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit dgClassDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox dgSubstanceFindBox;
		private ZArchitecture.GUI.ZDropEdit operatorDropEdit;
	}
}
