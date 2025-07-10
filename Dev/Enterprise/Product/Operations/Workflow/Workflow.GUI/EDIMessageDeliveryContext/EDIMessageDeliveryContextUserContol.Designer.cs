
namespace Enterprise.Workflow.GUI
{
	partial class EDIMessageDeliveryContextUserContol
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo zMacrosFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo();
			this.groupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.codeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.workflowTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBox.SuspendLayout();
			this.workflowTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.EDIMessageDeliveryContextSelector);
			// 
			// groupBox
			// 
			this.groupBox.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("971F771E-80F7-4512-9BE8-F7213805DE77", "Additional Event Context");
			this.groupBox.Controls.Add(this.codeTextBox);
			this.groupBox.Controls.Add(this.workflowTypeDropEdit);
			this.groupBox.Controls.Add(this.descriptionTextBox);
			this.groupBox.Controls.Add(this.grid);
			this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBox.Name = "groupBox";
			this.groupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 504, true);
			this.groupBox.TabIndex = 0;
			this.groupBox.TabStop = false;
			// 
			// codeTextBox
			// 
			this.BindingSource.SetBindingMember(this.codeTextBox, "ECS_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextSelector)(null)).ECS_Code)));
			this.codeTextBox.CaptionResourceString = null;
			this.codeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.codeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 25, true);
			this.codeTextBox.Name = "codeTextBox";
			this.codeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.codeTextBox.TabIndex = 1;
			// 
			// workflowTypeDropEdit
			// 
			this.workflowTypeDropEdit.AccessibleDescription = "";
			this.workflowTypeDropEdit.AccessibleName = "";
			this.workflowTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.workflowTypeDropEdit, "ECS_ProcessType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextSelector)(null)).ECS_ProcessType)));
			this.workflowTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 51, true);
			this.workflowTypeDropEdit.Name = "workflowTypeDropEdit";
			this.workflowTypeDropEdit.ShouldResizeByMaxLength = false;
			this.workflowTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.workflowTypeDropEdit.TabIndex = 2;
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "ECS_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextSelector)(null)).ECS_Description)));
			this.descriptionTextBox.CaptionResourceString = null;
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 77, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 20, true);
			this.descriptionTextBox.TabIndex = 3;
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.grid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextSelector)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextLine)(((System.Collections.IList)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextSelector)(null)).Lines)).SyncRoot)).ECL_ContextType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextLine)(((System.Collections.IList)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextSelector)(null)).Lines)).SyncRoot)).ECL_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextLine)(((System.Collections.IList)(((Enterprise.Workflow.Business.EDIMessageDeliveryContextSelector)(null)).Lines)).SyncRoot)).ECL_Value)));
			this.grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ECL_ContextType";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "ECL_Description";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMacrosFindBoxColumnStyleInfo.ColumnName = "ECL_Value";
			zMacrosFindBoxColumnStyleInfo.IsCustomColumn = false;
			zMacrosFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMacrosFindBoxColumnStyleInfo.IsUsedForExpressions = true;
			zMacrosFindBoxColumnStyleInfo.IncludeParentJobInRoots = false;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zMacrosFindBoxColumnStyleInfo);
			this.grid.GridId = "CF5EDFA5-3C34-4FED-8188-CF809E5D9129";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "grid";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 116, true);
			this.grid.Name = "grid";
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 385, true);
			this.grid.TabIndex = 4;
			// 
			// EDIMessageDeliveryContextUserContol
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.groupBox);
			this.Name = "EDIMessageDeliveryContextUserContol";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 504, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			this.workflowTypeDropEdit.ResumeLayout(true);
			this.workflowTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		Enterprise.ZArchitecture.GUI.ZGroupBox groupBox;
		ZArchitecture.ZTextBox codeTextBox;
		ZArchitecture.ZTextBox descriptionTextBox;
		ZArchitecture.GUI.ZDropEdit workflowTypeDropEdit;
		ZArchitecture.ZGrid grid;
	}
}
