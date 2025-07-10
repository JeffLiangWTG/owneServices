using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	partial class AsycudaContainerBillLinkUserControl
	{
		#region InitializeComponent

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AsycudaBillLinkAsycudaContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AsycudaBillLinkAsycudaContainersGrid)).BeginInit();
			this.AsycudaBillLinkAsycudaContainersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Manifest.Business.AsycudaBill);
			// 
			// AsycudaBillLinkAsycudaContainersGrid
			// 
			this.AsycudaBillLinkAsycudaContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AsycudaBillLinkAsycudaContainersGrid, "AsycudaBillLinkAsycudaContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).AsycudaBillLinkAsycudaContainers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBillLinkAsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).AsycudaBillLinkAsycudaContainers)).SyncRoot)).Link)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBillLinkAsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).AsycudaBillLinkAsycudaContainers)).SyncRoot)).ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBillLinkAsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).AsycudaBillLinkAsycudaContainers)).SyncRoot)).ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBillLinkAsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).AsycudaBillLinkAsycudaContainers)).SyncRoot)).ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBillLinkAsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).AsycudaBillLinkAsycudaContainers)).SyncRoot)).Seal1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBillLinkAsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).AsycudaBillLinkAsycudaContainers)).SyncRoot)).Seal2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBillLinkAsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).AsycudaBillLinkAsycudaContainers)).SyncRoot)).Seal3)));
			this.AsycudaBillLinkAsycudaContainersGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Link";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "ContainerNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ContainerType";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo1.ColumnName = "ContainerMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo2.ColumnName = "Seal1";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "Seal2";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "Seal3";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AsycudaBillLinkAsycudaContainersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AsycudaBillLinkAsycudaContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AsycudaBillLinkAsycudaContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AsycudaBillLinkAsycudaContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AsycudaBillLinkAsycudaContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AsycudaBillLinkAsycudaContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AsycudaBillLinkAsycudaContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AsycudaBillLinkAsycudaContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AsycudaBillLinkAsycudaContainersGrid.GridId = "CBF77BF9-CCE8-4153-893F-0E452166DA57";
			this.AsycudaBillLinkAsycudaContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AsycudaBillLinkAsycudaContainersGrid.LayoutKey = "PacksGrid";
			this.AsycudaBillLinkAsycudaContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AsycudaBillLinkAsycudaContainersGrid.Name = "AsycudaBillLinkAsycudaContainersGrid";
			this.AsycudaBillLinkAsycudaContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 482, true);
			this.AsycudaBillLinkAsycudaContainersGrid.TabIndex = 0;
			// 
			// AsycudaContainerBillLinkUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AsycudaBillLinkAsycudaContainersGrid);
			this.Name = "AsycudaContainerBillLinkUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 482, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AsycudaBillLinkAsycudaContainersGrid)).EndInit();
			this.AsycudaBillLinkAsycudaContainersGrid.ResumeLayout(false);
			this.AsycudaBillLinkAsycudaContainersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZGrid AsycudaBillLinkAsycudaContainersGrid;
	}
}
