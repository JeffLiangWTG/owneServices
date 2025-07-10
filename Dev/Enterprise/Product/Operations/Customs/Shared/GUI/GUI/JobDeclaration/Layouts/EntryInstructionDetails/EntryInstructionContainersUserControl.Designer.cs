namespace Enterprise.Customs.GUI;

partial class EntryInstructionContainersUserControl
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
		Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
		this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
		this.ContainersGrid.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusEntryInstruction);
		// 
		// ContainersGrid
		// 
		this.ContainersGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.ContainersGrid, "ContainersForInstructionForBindingOnly");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).ContainersForInstructionForBindingOnly)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusContainerOnEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).ContainersForInstructionForBindingOnly)).SyncRoot)).ContainerNumber)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.CusContainerOnEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).ContainersForInstructionForBindingOnly)).SyncRoot)).IsForEntry)));
		this.ContainersGrid.CaptionVisible = false;
		zTextBoxColumnStyleInfo1.ColumnName = "ContainerNumber";
		zTextBoxColumnStyleInfo1.IsReadOnly = true;
		zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		zCheckBoxColumnStyleInfo1.ColumnName = "IsForEntry";
		zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		this.ContainersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
		this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ContainersGrid.GridId = "019FA684-0526-41B2-BB2D-92B9BA73EF2E";
		this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.ContainersGrid.LayoutKey = "ContainersGrid";
		this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.ContainersGrid.Name = "ContainersGrid";
		this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
		this.ContainersGrid.TabIndex = 0;
		// 
		// EntryInstructionContainersUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.ContainersGrid);
		this.Name = "EntryInstructionContainersUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 274, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
		this.ContainersGrid.ResumeLayout(false);
		this.ContainersGrid.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	protected Enterprise.ZArchitecture.ZGrid ContainersGrid;
}
