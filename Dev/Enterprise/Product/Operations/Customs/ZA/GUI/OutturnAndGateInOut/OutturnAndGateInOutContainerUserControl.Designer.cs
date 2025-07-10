using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutContainerUserControl
	{
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            this.zGridContainer = new Enterprise.ZArchitecture.ZGrid();
            this.zGroupBoxContainerDetail = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zDateEditGateInOutTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zDropEditWithFixedWidthSealingPartyType = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zTextBoxSeal1 = new Enterprise.ZArchitecture.ZTextBox();
            this.zDateEditUnpackTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zGuidFindBoxContainerType = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.zDropEditWithFixedWidthEmptyFullIndicator = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zTextBoxContainerNumber = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zGridContainer)).BeginInit();
            this.zGridContainer.SuspendLayout();
            this.zGroupBoxContainerDetail.SuspendLayout();
            this.zDateEditGateInOutTime.SuspendLayout();
            this.zDropEditWithFixedWidthSealingPartyType.SuspendLayout();
            this.zDateEditUnpackTime.SuspendLayout();
            this.zGuidFindBoxContainerType.SuspendLayout();
            this.zDropEditWithFixedWidthEmptyFullIndicator.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.AsycudaManifestHeader);
            // 
            // zGridContainer
            // 
            this.zGridContainer.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.zGridContainer, "Containers");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_ContainerNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_EmptyFullIndicator)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_RC_ContainerType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ContUnpackTime)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_Seal1)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_SealingPartyType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).GateInOutDate)));
            this.zGridContainer.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "ACN_ContainerNumber";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "ACN_EmptyFullIndicator";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo1.ColumnName = "ACN_RC_ContainerType";
            zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.ColumnName = "ContUnpackTime";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo2.ColumnName = "ACN_Seal1";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo2.ColumnName = "ACN_SealingPartyType";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo2.ColumnName = "GateInOutDate";
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
            this.zGridContainer.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.zGridContainer.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.zGridContainer.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.zGridContainer.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.zGridContainer.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.zGridContainer.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.zGridContainer.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.zGridContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zGridContainer.GridId = "793878a7-5cbd-4a74-b3d3-07e4866914ad";
            this.zGridContainer.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.zGridContainer.LayoutKey = "zGridContainer";
            this.zGridContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zGridContainer.Name = "zGridContainer";
            this.zGridContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 321, true);
            this.zGridContainer.TabIndex = 0;
            // 
            // zGroupBoxContainerDetail
            // 
            this.zGroupBoxContainerDetail.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("106f591f-ffc2-49fc-a4b0-aeb4bbd2a1ef", "Container Detail");
            this.zGroupBoxContainerDetail.Controls.Add(this.zDateEditGateInOutTime);
            this.zGroupBoxContainerDetail.Controls.Add(this.zDropEditWithFixedWidthSealingPartyType);
            this.zGroupBoxContainerDetail.Controls.Add(this.zTextBoxSeal1);
            this.zGroupBoxContainerDetail.Controls.Add(this.zDateEditUnpackTime);
            this.zGroupBoxContainerDetail.Controls.Add(this.zGuidFindBoxContainerType);
            this.zGroupBoxContainerDetail.Controls.Add(this.zDropEditWithFixedWidthEmptyFullIndicator);
            this.zGroupBoxContainerDetail.Controls.Add(this.zTextBoxContainerNumber);
            this.zGroupBoxContainerDetail.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.zGroupBoxContainerDetail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 321, true);
            this.zGroupBoxContainerDetail.Name = "zGroupBoxContainerDetail";
            this.zGroupBoxContainerDetail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 132, true);
            this.zGroupBoxContainerDetail.TabIndex = 1;
            this.zGroupBoxContainerDetail.TabStop = false;
            this.zGroupBoxContainerDetail.Text = "Container Details";
            // 
            // zDateEditGateInOutTime
            // 
            this.zDateEditGateInOutTime.AllowDrop = true;
            this.zDateEditGateInOutTime.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEditGateInOutTime, "Containers.GateInOutDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).GateInOutDate)));
            this.zDateEditGateInOutTime.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.zDateEditGateInOutTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(878, 61, true);
            this.zDateEditGateInOutTime.Name = "zDateEditGateInOutTime";
            this.zDateEditGateInOutTime.TabIndex = 6;
            // 
            // zDropEditWithFixedWidthSealingPartyType
            // 
            this.zDropEditWithFixedWidthSealingPartyType.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthSealingPartyType, "Containers.ACN_SealingPartyType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_SealingPartyType)));
            this.zDropEditWithFixedWidthSealingPartyType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 87, true);
            this.zDropEditWithFixedWidthSealingPartyType.Name = "zDropEditWithFixedWidthSealingPartyType";
            this.zDropEditWithFixedWidthSealingPartyType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthSealingPartyType.TabIndex = 4;
            // 
            // zTextBoxSeal1
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxSeal1, "Containers.ACN_Seal1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_Seal1)));
            this.zTextBoxSeal1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 87, true);
            this.zTextBoxSeal1.Name = "zTextBoxSeal1";
            this.zTextBoxSeal1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
            this.zTextBoxSeal1.TabIndex = 3;
            // 
            // zDateEditUnpackTime
            // 
            this.zDateEditUnpackTime.AllowDrop = true;
            this.zDateEditUnpackTime.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.zDateEditUnpackTime, "Containers.ContUnpackTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ContUnpackTime)));
            this.zDateEditUnpackTime.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.zDateEditUnpackTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(878, 35, true);
            this.zDateEditUnpackTime.Name = "zDateEditUnpackTime";
            this.zDateEditUnpackTime.TabIndex = 5;
            // 
            // zGuidFindBoxContainerType
            // 
            this.zGuidFindBoxContainerType.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zGuidFindBoxContainerType, "Containers.ACN_RC_ContainerType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_RC_ContainerType)));
            this.zGuidFindBoxContainerType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 61, true);
            this.zGuidFindBoxContainerType.Name = "zGuidFindBoxContainerType";
            this.zGuidFindBoxContainerType.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zGuidFindBoxContainerType.ParentType = null;
            this.zGuidFindBoxContainerType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
            this.zGuidFindBoxContainerType.TabIndex = 2;
            // 
            // zDropEditWithFixedWidthEmptyFullIndicator
            // 
            this.zDropEditWithFixedWidthEmptyFullIndicator.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthEmptyFullIndicator, "Containers.ACN_EmptyFullIndicator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_EmptyFullIndicator)));
            this.zDropEditWithFixedWidthEmptyFullIndicator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 35, true);
            this.zDropEditWithFixedWidthEmptyFullIndicator.Name = "zDropEditWithFixedWidthEmptyFullIndicator";
            this.zDropEditWithFixedWidthEmptyFullIndicator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthEmptyFullIndicator.TabIndex = 1;
            // 
            // zTextBoxContainerNumber
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxContainerNumber, "Containers.ACN_ContainerNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_ContainerNumber)));
            this.zTextBoxContainerNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 35, true);
            this.zTextBoxContainerNumber.Name = "zTextBoxContainerNumber";
            this.zTextBoxContainerNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
            this.zTextBoxContainerNumber.TabIndex = 0;
            // 
            // OutturnAndGateInOutContainerUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.zGridContainer);
            this.Controls.Add(this.zGroupBoxContainerDetail);
            this.Name = "OutturnAndGateInOutContainerUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 453, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zGridContainer)).EndInit();
            this.zGridContainer.ResumeLayout(false);
            this.zGridContainer.PerformLayout();
            this.zGroupBoxContainerDetail.ResumeLayout(false);
            this.zGroupBoxContainerDetail.PerformLayout();
            this.zDateEditGateInOutTime.ResumeLayout(true);
            this.zDateEditGateInOutTime.PerformLayout();
            this.zDropEditWithFixedWidthSealingPartyType.ResumeLayout(true);
            this.zDropEditWithFixedWidthSealingPartyType.PerformLayout();
            this.zDateEditUnpackTime.ResumeLayout(true);
            this.zDateEditUnpackTime.PerformLayout();
            this.zGuidFindBoxContainerType.ResumeLayout(true);
            this.zGuidFindBoxContainerType.PerformLayout();
            this.zDropEditWithFixedWidthEmptyFullIndicator.ResumeLayout(true);
            this.zDropEditWithFixedWidthEmptyFullIndicator.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	}
}
