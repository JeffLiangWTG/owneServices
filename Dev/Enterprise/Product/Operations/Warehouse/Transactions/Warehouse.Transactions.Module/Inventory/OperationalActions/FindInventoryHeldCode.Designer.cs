
namespace Enterprise.Warehouse.Transactions.Module
{
	partial class FindInventoryHeldCode
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
			this.zHeldCodes = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HoldReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zHeldCodes.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Module.UpdateInventoryHeldCodeActionMethodApplicator);
			// 
			// zHeldCodes
			// 
			this.zHeldCodes.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zHeldCodes, "SelectedInventoryHeldCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Transactions.Module.UpdateInventoryHeldCodeActionMethodApplicator)(null)).SelectedInventoryHeldCode)));
			this.zHeldCodes.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("654bcdff-ba1a-45fc-b188-e8c6a2ae40a0", "New Inventory Hold Code");
			this.zHeldCodes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 47, true);
			this.zHeldCodes.Name = "zHeldCodes";
			this.zHeldCodes.ShouldResizeByMaxLength = true;
			this.zHeldCodes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.zHeldCodes.TabIndex = 1;
			// 
			// HoldReasonTextBox
			// 
			this.HoldReasonTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HoldReasonTextBox, "HoldReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Module.UpdateInventoryHeldCodeActionMethodApplicator)(null)).HoldReason)));
			this.HoldReasonTextBox.CaptionResourceString = null;
			this.HoldReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HoldReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 73, true);
			this.HoldReasonTextBox.Name = "HoldReasonTextBox";
			this.HoldReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.HoldReasonTextBox.TabIndex = 2;
			// 
			// FindInventoryHeldCode
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zHeldCodes);
			this.Controls.Add(this.HoldReasonTextBox);
			this.Name = "FindInventoryHeldCode";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 125, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zHeldCodes.ResumeLayout(true);
			this.zHeldCodes.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit zHeldCodes;
		private ZArchitecture.ZTextBox HoldReasonTextBox;
	}
}
