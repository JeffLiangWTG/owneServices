namespace Enterprise.MasterFiles.Module
{
	public partial class ABCCategoryWarehouseFilterControl
	{
		void InitializeComponent()
		{
			this.ABCCategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WarehouseFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.ABCCategoryWarehouseFilter);
			// 
			// ABCCategoryDropEdit
			// 
			this.ABCCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ABCCategoryDropEdit, "WJ_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.ABCCategoryWarehouseFilter)(null)).WJ_Category)));
			this.ABCCategoryDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("e6a38de8-1d00-4893-ad93-b1121b0cc8a4", "ABC Category");
			this.ABCCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 1, true);
			this.ABCCategoryDropEdit.Name = "ABCCategoryDropEdit";
			this.ABCCategoryDropEdit.PreBoundMaxLength = 7;
			this.ABCCategoryDropEdit.ShowDescriptionBox = false;
			this.ABCCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ABCCategoryDropEdit.TabIndex = 0;
			// 
			// WarehouseFindBox
			// 
			this.WarehouseFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseFindBox, "WJ_WW_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.ABCCategoryWarehouseFilter)(null)).WJ_WW_Warehouse)));
			this.WarehouseFindBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("5e76efe8-8e87-4b4f-8c72-7836969c7212", "Warehouse");
			this.WarehouseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 1, true);
			this.WarehouseFindBox.Name = "WarehouseFindBox";
			this.WarehouseFindBox.PreBoundMaxLength = 3;
			this.WarehouseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 20, true);
			this.WarehouseFindBox.TabIndex = 1;
			// 
			// ABCCategoryWarehouseFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.WarehouseFindBox);
			this.Controls.Add(this.ABCCategoryDropEdit);
			this.Name = "ABCCategoryWarehouseFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ABCCategoryDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox WarehouseFindBox;
	}
}
