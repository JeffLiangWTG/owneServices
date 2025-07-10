using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	partial class GenerateRMAReferenceReceivesByOrdersControl
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
            this.HeaderLabel = new Enterprise.ZArchitecture.ZHeaderLabel();
            this.RMAReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.WarehouseGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.WarehouseGuidFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // HeaderLabel
            // 
            this.HeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.HeaderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(52)))), ((int)(((byte)(121)))));
            this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
            this.HeaderLabel.Name = "HeaderLabel";
            this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 23, true);
            this.HeaderLabel.TabIndex = 1;
            this.HeaderLabel.Text = "Please Enter RMA Reference";
            this.HeaderLabel.UseMnemonic = false;
            // 
            // RMAReferenceTextBox
            // 
            this.BindingSource.SetBindingMember(this.RMAReferenceTextBox, "RMAReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Module.GenerateRMAByOrdersActionMethodApplicator)(null)).RMAReference)));
            this.RMAReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.RMAReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 27, true);
            this.RMAReferenceTextBox.Name = "RMAReferenceTextBox";
            this.RMAReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 17, true);
            this.RMAReferenceTextBox.TabIndex = 2;
            // 
            // WarehouseGuidFindBox
            // 
            this.WarehouseGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WarehouseGuidFindBox, "WhsOverride");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Module.GenerateRMAByOrdersActionMethodApplicator)(null)).WhsOverride)));
            this.WarehouseGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 65, true);
            this.WarehouseGuidFindBox.Name = "WarehouseGuidFindBox";
            this.WarehouseGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.WarehouseGuidFindBox.ParentType = null;
            this.WarehouseGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 17, true);
            this.WarehouseGuidFindBox.TabIndex = 3;
            // 
            // GenerateRMAReferenceReceivesByOrdersControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.WarehouseGuidFindBox);
            this.Controls.Add(this.RMAReferenceTextBox);
            this.Controls.Add(this.HeaderLabel);
            this.Name = "GenerateRMAReferenceReceivesByOrdersControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 285, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.WarehouseGuidFindBox.ResumeLayout(true);
            this.WarehouseGuidFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZHeaderLabel HeaderLabel;
		private ZArchitecture.ZTextBox RMAReferenceTextBox;
		private ZArchitecture.GUI.ZGuidFindBox WarehouseGuidFindBox;

	}
}
