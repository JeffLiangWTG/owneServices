namespace Enterprise.Customs.NL.GUI
{
	partial class ContainerDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalSealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WeightsGroupBox.SuspendLayout();
			this.ExportContainerModeDropEdit.SuspendLayout();
			this.ExportContainerTypeGuidFindBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.DeliveryModeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SealPartyDropEdit.SuspendLayout();
			this.AdditionalSealPartyDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// edSecondSealNum
			// 
			this.edSecondSealNum.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("45F223AA-3282-4236-85CB-9D3B4F11D7DF", "2nd Seal", "[UCC 7/18] 2nd Seal");
			this.edSecondSealNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 67, true);
			this.edSecondSealNum.TabIndex = 6;
			// 
			// edContainerNum
			// 
			this.edContainerNum.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("C6BA4E5E-F367-451C-B6D6-2386F4BCC0C3", "Container", "[UCC 7/10] Container");
			// 
			// edSealNum
			// 
			this.edSealNum.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("A9389253-C47B-4308-8166-D9140254C2F0", "Seal", "[7/18] Seal Number.");
			this.edSealNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 46, true);
			this.edSealNum.TabIndex = 4;
			// 
			// WeightsGroupBox
			// 
			this.WeightsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 97, true);
			this.WeightsGroupBox.TabIndex = 8;
			// 
			// ExportContainerTypeGuidFindBox
			// 
			this.ExportContainerTypeGuidFindBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("82578BE8-E1B9-4BDB-9FA2-EB8670B0285C", "Type", "[UCC 7/11] Type");
			// 
			// ImportTabPage
			// 
			this.ImportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 341, true);
			this.ImportTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ImportTabPage_InitializeTab));
			// 
			// ExportTabPage
			// 
			this.ExportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 341, true);
			this.ExportTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ExportTabPage_InitializeTab));
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 235, true);
			this.DetailsGroupBox.TabIndex = 9;
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.TabIndex = 10;
			// 
			// OutturnTabPage
			// 
			this.OutturnTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 341, true);
			this.OutturnTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.OutturnTabPage_InitializeTab));
			// 
			// DeliveryModeDropEdit
			// 
			this.DeliveryModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 25, true);
			this.DeliveryModeDropEdit.TabIndex = 2;
			// 
			// VGMTabPage
			// 
			this.VGMTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 341, true);
			this.VGMTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.VGMTabPage_InitializeTab));
			// 
			// FreightRatesTabPage
			// 
			this.FreightRatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 341, true);
			this.FreightRatesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.FreightRatesTabPage_InitializeTab));
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.CusContainer);
			// 
			// SealPartyDropEdit
			// 
			this.SealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SealPartyDropEdit, "SealPartyForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.Declaration.CusContainer)(null)).SealPartyForBinding)));
			this.SealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 46, true);
			this.SealPartyDropEdit.Name = "SealPartyDropEdit";
			this.SealPartyDropEdit.ShowDescriptionBox = false;
			this.SealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 25, true);
			this.SealPartyDropEdit.TabIndex = 5;
			// 
			// AdditionalSealPartyDropEdit
			// 
			this.AdditionalSealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalSealPartyDropEdit, "AdditionalSealPartyForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.Declaration.CusContainer)(null)).AdditionalSealPartyForBinding)));
			this.AdditionalSealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 67, true);
			this.AdditionalSealPartyDropEdit.Name = "AdditionalSealPartyDropEdit";
			this.AdditionalSealPartyDropEdit.ShowDescriptionBox = false;
			this.AdditionalSealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 25, true);
			this.AdditionalSealPartyDropEdit.TabIndex = 7;
			// 
			// ContainerDetailsUserControl
			// 
			this.Controls.Add(this.AdditionalSealPartyDropEdit);
			this.Controls.Add(this.SealPartyDropEdit);
			this.Name = "ContainerDetailsUserControl";
			this.Controls.SetChildIndex(this.edSecondSealNum, 0);
			this.Controls.SetChildIndex(this.SealPartyDropEdit, 0);
			this.Controls.SetChildIndex(this.AdditionalSealPartyDropEdit, 0);
			this.Controls.SetChildIndex(this.edContainerNum, 0);
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.WeightsGroupBox, 0);
			this.Controls.SetChildIndex(this.DetailTabControl, 0);
			this.Controls.SetChildIndex(this.edSealNum, 0);
			this.Controls.SetChildIndex(this.ExportContainerTypeGuidFindBox, 0);
			this.Controls.SetChildIndex(this.ExportContainerModeDropEdit, 0);
			this.Controls.SetChildIndex(this.DeliveryModeDropEdit, 0);
			this.WeightsGroupBox.ResumeLayout(false);
			this.WeightsGroupBox.PerformLayout();
			this.ExportContainerModeDropEdit.ResumeLayout(true);
			this.ExportContainerModeDropEdit.PerformLayout();
			this.ExportContainerTypeGuidFindBox.ResumeLayout(true);
			this.ExportContainerTypeGuidFindBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DeliveryModeDropEdit.ResumeLayout(true);
			this.DeliveryModeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SealPartyDropEdit.ResumeLayout(true);
			this.SealPartyDropEdit.PerformLayout();
			this.AdditionalSealPartyDropEdit.ResumeLayout(true);
			this.AdditionalSealPartyDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZDropEdit SealPartyDropEdit;
		ZArchitecture.GUI.ZDropEdit AdditionalSealPartyDropEdit;

	}
}
