using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutPackUserControl
	{
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
            Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
            Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo4 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo5 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            this.zGroupBoxPackDetail = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zPanelPackDetail = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.zLabelManifestedShouldBe = new Enterprise.ZArchitecture.ZLabel();
            this.zLabelActuallyFoundToBe = new Enterprise.ZArchitecture.ZLabel();
            this.zLabelDiscrepancyReport = new Enterprise.ZArchitecture.ZLabel();
            this.zCheckBoxSealIntactIndicator = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.zCalcEditWeightOutturned = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zTextBoxPackCondDesc = new Enterprise.ZArchitecture.ZTextBox();
            this.zDropEditPackageCondition = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zTextBoxGoodsDescription = new Enterprise.ZArchitecture.ZTextBox();
            this.zDropEditWithFixedWidthVolumeOutturnedUQ = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zCalcEditVolumeOutturned = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zCalcEditPackagesOutturned = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zDropEditWithFixedWidthWeightOutturnedUQ = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zDropEditWithFixedWidthCargoType = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zGuidDropEditWithFixedWidthContainerPK = new Enterprise.ZArchitecture.GUI.ZGuidDropEditWithFixedWidth();
            this.zTextBoxMarksAndNumbers = new Enterprise.ZArchitecture.ZTextBox();
            this.zTextBoxAPA_GoodsDescription = new Enterprise.ZArchitecture.ZTextBox();
            this.zDropEditWithFixedWidthVolumeUQ = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zTextBoxContShouldBe = new Enterprise.ZArchitecture.ZTextBox();
            this.zCalcEditWeight = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zCalcEditVolume = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zDropEditWithFixedWidthWeightUQ = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zCalcEditPackQty = new Enterprise.ZArchitecture.ZCalcEdit();
            this.zDropEditWithFixedWidthPackUQ = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zDropEditWithFixedWidthExcessShortInd = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.zGridPacks = new Enterprise.ZArchitecture.ZGrid();
            this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zGroupBoxPackDetail.SuspendLayout();
            this.zPanelPackDetail.SuspendLayout();
            this.zDropEditPackageCondition.SuspendLayout();
            this.zDropEditWithFixedWidthVolumeOutturnedUQ.SuspendLayout();
            this.zDropEditWithFixedWidthWeightOutturnedUQ.SuspendLayout();
            this.zDropEditWithFixedWidthCargoType.SuspendLayout();
            this.zGuidDropEditWithFixedWidthContainerPK.SuspendLayout();
            this.zDropEditWithFixedWidthVolumeUQ.SuspendLayout();
            this.zDropEditWithFixedWidthWeightUQ.SuspendLayout();
            this.zDropEditWithFixedWidthPackUQ.SuspendLayout();
            this.zDropEditWithFixedWidthExcessShortInd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGridPacks)).BeginInit();
            this.zGridPacks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.AsycudaManifestHeader);
            // 
            // zGroupBoxPackDetail
            // 
            this.zGroupBoxPackDetail.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("0b4a1f84-eb95-4237-9567-ca47869ec3d4", "Pack Details");
            this.zGroupBoxPackDetail.Controls.Add(this.zPanelPackDetail);
            this.zGroupBoxPackDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zGroupBoxPackDetail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zGroupBoxPackDetail.Name = "zGroupBoxPackDetail";
            this.zGroupBoxPackDetail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 308, true);
            this.zGroupBoxPackDetail.TabIndex = 1;
            this.zGroupBoxPackDetail.TabStop = false;
            // 
            // zPanelPackDetail
            // 
            this.zPanelPackDetail.AutoScroll = true;
            this.zPanelPackDetail.Controls.Add(this.zLabelManifestedShouldBe);
            this.zPanelPackDetail.Controls.Add(this.zLabelActuallyFoundToBe);
            this.zPanelPackDetail.Controls.Add(this.zLabelDiscrepancyReport);
            this.zPanelPackDetail.Controls.Add(this.zCheckBoxSealIntactIndicator);
            this.zPanelPackDetail.Controls.Add(this.zCalcEditWeightOutturned);
            this.zPanelPackDetail.Controls.Add(this.zTextBoxPackCondDesc);
            this.zPanelPackDetail.Controls.Add(this.zDropEditPackageCondition);
            this.zPanelPackDetail.Controls.Add(this.zTextBoxGoodsDescription);
            this.zPanelPackDetail.Controls.Add(this.zDropEditWithFixedWidthVolumeOutturnedUQ);
            this.zPanelPackDetail.Controls.Add(this.zCalcEditVolumeOutturned);
            this.zPanelPackDetail.Controls.Add(this.zCalcEditPackagesOutturned);
            this.zPanelPackDetail.Controls.Add(this.zDropEditWithFixedWidthWeightOutturnedUQ);
            this.zPanelPackDetail.Controls.Add(this.zDropEditWithFixedWidthCargoType);
            this.zPanelPackDetail.Controls.Add(this.zGuidDropEditWithFixedWidthContainerPK);
            this.zPanelPackDetail.Controls.Add(this.zTextBoxMarksAndNumbers);
            this.zPanelPackDetail.Controls.Add(this.zTextBoxAPA_GoodsDescription);
            this.zPanelPackDetail.Controls.Add(this.zDropEditWithFixedWidthVolumeUQ);
            this.zPanelPackDetail.Controls.Add(this.zTextBoxContShouldBe);
            this.zPanelPackDetail.Controls.Add(this.zCalcEditWeight);
            this.zPanelPackDetail.Controls.Add(this.zCalcEditVolume);
            this.zPanelPackDetail.Controls.Add(this.zDropEditWithFixedWidthWeightUQ);
            this.zPanelPackDetail.Controls.Add(this.zCalcEditPackQty);
            this.zPanelPackDetail.Controls.Add(this.zDropEditWithFixedWidthPackUQ);
            this.zPanelPackDetail.Controls.Add(this.zDropEditWithFixedWidthExcessShortInd);
            this.zPanelPackDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zPanelPackDetail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.zPanelPackDetail.Name = "zPanelPackDetail";
            this.zPanelPackDetail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1296, 291, true);
            this.zPanelPackDetail.TabIndex = 1;
            // 
            // zLabelManifestedShouldBe
            // 
            this.zLabelManifestedShouldBe.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("9ec99688-347c-44e8-bb18-9267a8402b0b", "Manifested should be");
            this.zLabelManifestedShouldBe.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.zLabelManifestedShouldBe.IsFontBold = true;
            this.zLabelManifestedShouldBe.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 11, true);
            this.zLabelManifestedShouldBe.Name = "zLabelManifestedShouldBe";
            this.zLabelManifestedShouldBe.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
            this.zLabelManifestedShouldBe.TabIndex = 21;
            this.zLabelManifestedShouldBe.UseMnemonic = false;
            // 
            // zLabelActuallyFoundToBe
            // 
            this.zLabelActuallyFoundToBe.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("cbab3c18-5fc5-4d2d-aef7-94e5085e7419", "Actually found to be");
            this.zLabelActuallyFoundToBe.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.zLabelActuallyFoundToBe.IsFontBold = true;
            this.zLabelActuallyFoundToBe.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 11, true);
            this.zLabelActuallyFoundToBe.Name = "zLabelActuallyFoundToBe";
            this.zLabelActuallyFoundToBe.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
            this.zLabelActuallyFoundToBe.TabIndex = 22;
            this.zLabelActuallyFoundToBe.UseMnemonic = false;
            // 
            // zLabelDiscrepancyReport
            // 
            this.zLabelDiscrepancyReport.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("0c0af857-d5a9-45f6-bc02-0ba8f98abb62", "Discrepancy Report");
            this.zLabelDiscrepancyReport.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.zLabelDiscrepancyReport.IsFontBold = true;
            this.zLabelDiscrepancyReport.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(873, 11, true);
            this.zLabelDiscrepancyReport.Name = "zLabelDiscrepancyReport";
            this.zLabelDiscrepancyReport.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
            this.zLabelDiscrepancyReport.TabIndex = 23;
            this.zLabelDiscrepancyReport.UseMnemonic = false;
            // 
            // zCheckBoxSealIntactIndicator
            // 
            this.BindingSource.SetBindingMember(this.zCheckBoxSealIntactIndicator, "Bills.Packs.Outturn.C5_SealIntactIndicator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_SealIntactIndicator)));
            this.zCheckBoxSealIntactIndicator.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("240997f3-1a0e-4b41-96b6-793848e8e3fd", "Seal Intact");
            this.zCheckBoxSealIntactIndicator.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.zCheckBoxSealIntactIndicator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 52, true);
            this.zCheckBoxSealIntactIndicator.Name = "zCheckBoxSealIntactIndicator";
            this.zCheckBoxSealIntactIndicator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 24, true);
            this.zCheckBoxSealIntactIndicator.TabIndex = 2;
            this.zCheckBoxSealIntactIndicator.UseVisualStyleBackColor = true;
            // 
            // zCalcEditWeightOutturned
            // 
            this.BindingSource.SetBindingMember(this.zCalcEditWeightOutturned, "Bills.Packs.Outturn.C5_WeightOutturned");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_WeightOutturned)));
            this.zCalcEditWeightOutturned.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEditWeightOutturned, false);
            this.zCalcEditWeightOutturned.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 155, true);
            this.zCalcEditWeightOutturned.Name = "zCalcEditWeightOutturned";
            this.zCalcEditWeightOutturned.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.zCalcEditWeightOutturned.TabIndex = 9;
            this.zCalcEditWeightOutturned.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEditWeightOutturned.TrackDisposedAccess = true;
            // 
            // zTextBoxPackCondDesc
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxPackCondDesc, "Bills.Packs.Outturn.PackCondDesc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.PackCondDesc)));
            this.zTextBoxPackCondDesc.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 253, true);
            this.zTextBoxPackCondDesc.Name = "zTextBoxPackCondDesc";
            this.zTextBoxPackCondDesc.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 17, true);
            this.zTextBoxPackCondDesc.TabIndex = 19;
            // 
            // zDropEditPackageCondition
            // 
            this.zDropEditPackageCondition.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditPackageCondition, "Bills.Packs.Outturn.C5_PackageCondition");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_PackageCondition)));
            this.zDropEditPackageCondition.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 229, true);
            this.zDropEditPackageCondition.Name = "zDropEditPackageCondition";
            this.zDropEditPackageCondition.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 17, true);
            this.zDropEditPackageCondition.TabIndex = 18;
            // 
            // zTextBoxGoodsDescription
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxGoodsDescription, "Bills.Packs.Outturn.C5_GoodsDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_GoodsDescription)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBoxGoodsDescription, false);
            this.zTextBoxGoodsDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 131, true);
            this.zTextBoxGoodsDescription.Name = "zTextBoxGoodsDescription";
            this.zTextBoxGoodsDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 17, true);
            this.zTextBoxGoodsDescription.TabIndex = 6;
            // 
            // zDropEditWithFixedWidthVolumeOutturnedUQ
            // 
            this.zDropEditWithFixedWidthVolumeOutturnedUQ.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthVolumeOutturnedUQ, "Bills.Packs.Outturn.C5_VolumeOutturnedUQ");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_VolumeOutturnedUQ)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEditWithFixedWidthVolumeOutturnedUQ, false);
            this.zDropEditWithFixedWidthVolumeOutturnedUQ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 179, true);
            this.zDropEditWithFixedWidthVolumeOutturnedUQ.Name = "zDropEditWithFixedWidthVolumeOutturnedUQ";
            this.zDropEditWithFixedWidthVolumeOutturnedUQ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthVolumeOutturnedUQ.TabIndex = 14;
            // 
            // zCalcEditVolumeOutturned
            // 
            this.BindingSource.SetBindingMember(this.zCalcEditVolumeOutturned, "Bills.Packs.Outturn.C5_VolumeOutturned");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_VolumeOutturned)));
            this.zCalcEditVolumeOutturned.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEditVolumeOutturned, false);
            this.zCalcEditVolumeOutturned.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 179, true);
            this.zCalcEditVolumeOutturned.Name = "zCalcEditVolumeOutturned";
            this.zCalcEditVolumeOutturned.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.zCalcEditVolumeOutturned.TabIndex = 13;
            this.zCalcEditVolumeOutturned.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEditVolumeOutturned.TrackDisposedAccess = true;
            // 
            // zCalcEditPackagesOutturned
            // 
            this.BindingSource.SetBindingMember(this.zCalcEditPackagesOutturned, "Bills.Packs.Outturn.C5_PackagesOutturned");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_PackagesOutturned)));
            this.zCalcEditPackagesOutturned.DecimalPlaces = 2;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEditPackagesOutturned, false);
            this.zCalcEditPackagesOutturned.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 203, true);
            this.zCalcEditPackagesOutturned.Name = "zCalcEditPackagesOutturned";
            this.zCalcEditPackagesOutturned.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.zCalcEditPackagesOutturned.TabIndex = 17;
            this.zCalcEditPackagesOutturned.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEditPackagesOutturned.TrackDisposedAccess = true;
            // 
            // zDropEditWithFixedWidthWeightOutturnedUQ
            // 
            this.zDropEditWithFixedWidthWeightOutturnedUQ.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthWeightOutturnedUQ, "Bills.Packs.Outturn.C5_WeightOutturnedUQ");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_WeightOutturnedUQ)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEditWithFixedWidthWeightOutturnedUQ, false);
            this.zDropEditWithFixedWidthWeightOutturnedUQ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 155, true);
            this.zDropEditWithFixedWidthWeightOutturnedUQ.Name = "zDropEditWithFixedWidthWeightOutturnedUQ";
            this.zDropEditWithFixedWidthWeightOutturnedUQ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthWeightOutturnedUQ.TabIndex = 10;
            // 
            // zDropEditWithFixedWidthCargoType
            // 
            this.zDropEditWithFixedWidthCargoType.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthCargoType, "Bills.Packs.Outturn.C5_CargoType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_CargoType)));
            this.zDropEditWithFixedWidthCargoType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 33, true);
            this.zDropEditWithFixedWidthCargoType.Name = "zDropEditWithFixedWidthCargoType";
            this.zDropEditWithFixedWidthCargoType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 17, true);
            this.zDropEditWithFixedWidthCargoType.TabIndex = 0;
            // 
            // zGuidDropEditWithFixedWidthContainerPK
            // 
            this.zGuidDropEditWithFixedWidthContainerPK.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zGuidDropEditWithFixedWidthContainerPK, "Bills.Packs.ContainerPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).ContainerPK)));
            this.zGuidDropEditWithFixedWidthContainerPK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 59, true);
            this.zGuidDropEditWithFixedWidthContainerPK.Name = "zGuidDropEditWithFixedWidthContainerPK";
            this.zGuidDropEditWithFixedWidthContainerPK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 17, true);
            this.zGuidDropEditWithFixedWidthContainerPK.TabIndex = 1;
            // 
            // zTextBoxMarksAndNumbers
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxMarksAndNumbers, "Bills.Packs.APA_MarksAndNumbers");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_MarksAndNumbers)));
            this.zTextBoxMarksAndNumbers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 107, true);
            this.zTextBoxMarksAndNumbers.Name = "zTextBoxMarksAndNumbers";
            this.zTextBoxMarksAndNumbers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 17, true);
            this.zTextBoxMarksAndNumbers.TabIndex = 4;
            // 
            // zTextBoxAPA_GoodsDescription
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxAPA_GoodsDescription, "Bills.Packs.APA_GoodsDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_GoodsDescription)));
            this.zTextBoxAPA_GoodsDescription.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("013deb4a-2890-4615-8e57-5a3dac3ca635", "", "Goods Desc.", "Goods Description", "");
            this.zTextBoxAPA_GoodsDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 83, true);
            this.zTextBoxAPA_GoodsDescription.Name = "zTextBoxAPA_GoodsDescription";
            this.zTextBoxAPA_GoodsDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 17, true);
            this.zTextBoxAPA_GoodsDescription.TabIndex = 3;
            // 
            // zDropEditWithFixedWidthVolumeUQ
            // 
            this.zDropEditWithFixedWidthVolumeUQ.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthVolumeUQ, "Bills.Packs.APA_VolumeUQ");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_VolumeUQ)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEditWithFixedWidthVolumeUQ, false);
            this.zDropEditWithFixedWidthVolumeUQ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 179, true);
            this.zDropEditWithFixedWidthVolumeUQ.Name = "zDropEditWithFixedWidthVolumeUQ";
            this.zDropEditWithFixedWidthVolumeUQ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthVolumeUQ.TabIndex = 12;
            // 
            // zTextBoxContShouldBe
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxContShouldBe, "Bills.Packs.Outturn.ContShouldBe");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.ContShouldBe)));
            this.zTextBoxContShouldBe.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 131, true);
            this.zTextBoxContShouldBe.Name = "zTextBoxContShouldBe";
            this.zTextBoxContShouldBe.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 17, true);
            this.zTextBoxContShouldBe.TabIndex = 5;
            // 
            // zCalcEditWeight
            // 
            this.BindingSource.SetBindingMember(this.zCalcEditWeight, "Bills.Packs.APA_Weight");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_Weight)));
            this.zCalcEditWeight.DecimalPlaces = 2;
            this.zCalcEditWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 155, true);
            this.zCalcEditWeight.Name = "zCalcEditWeight";
            this.zCalcEditWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.zCalcEditWeight.TabIndex = 7;
            this.zCalcEditWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEditWeight.TrackDisposedAccess = true;
            // 
            // zCalcEditVolume
            // 
            this.BindingSource.SetBindingMember(this.zCalcEditVolume, "Bills.Packs.APA_Volume");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_Volume)));
            this.zCalcEditVolume.DecimalPlaces = 2;
            this.zCalcEditVolume.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 179, true);
            this.zCalcEditVolume.Name = "zCalcEditVolume";
            this.zCalcEditVolume.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.zCalcEditVolume.TabIndex = 11;
            this.zCalcEditVolume.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEditVolume.TrackDisposedAccess = true;
            // 
            // zDropEditWithFixedWidthWeightUQ
            // 
            this.zDropEditWithFixedWidthWeightUQ.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthWeightUQ, "Bills.Packs.APA_WeightUQ");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_WeightUQ)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEditWithFixedWidthWeightUQ, false);
            this.zDropEditWithFixedWidthWeightUQ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 155, true);
            this.zDropEditWithFixedWidthWeightUQ.Name = "zDropEditWithFixedWidthWeightUQ";
            this.zDropEditWithFixedWidthWeightUQ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthWeightUQ.TabIndex = 8;
            // 
            // zCalcEditPackQty
            // 
            this.BindingSource.SetBindingMember(this.zCalcEditPackQty, "Bills.Packs.APA_PackQty");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_PackQty)));
            this.zCalcEditPackQty.DecimalPlaces = 2;
            this.zCalcEditPackQty.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 203, true);
            this.zCalcEditPackQty.Name = "zCalcEditPackQty";
            this.zCalcEditPackQty.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.zCalcEditPackQty.TabIndex = 15;
            this.zCalcEditPackQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.zCalcEditPackQty.TrackDisposedAccess = true;
            // 
            // zDropEditWithFixedWidthPackUQ
            // 
            this.zDropEditWithFixedWidthPackUQ.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthPackUQ, "Bills.Packs.APA_PackUQ");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_PackUQ)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEditWithFixedWidthPackUQ, false);
            this.zDropEditWithFixedWidthPackUQ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 203, true);
            this.zDropEditWithFixedWidthPackUQ.Name = "zDropEditWithFixedWidthPackUQ";
            this.zDropEditWithFixedWidthPackUQ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
            this.zDropEditWithFixedWidthPackUQ.TabIndex = 16;
            // 
            // zDropEditWithFixedWidthExcessShortInd
            // 
            this.zDropEditWithFixedWidthExcessShortInd.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditWithFixedWidthExcessShortInd, "Bills.Packs.Outturn.ExcessShortInd");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.ExcessShortInd)));
            this.zDropEditWithFixedWidthExcessShortInd.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(973, 277, true);
            this.zDropEditWithFixedWidthExcessShortInd.Name = "zDropEditWithFixedWidthExcessShortInd";
            this.zDropEditWithFixedWidthExcessShortInd.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 17, true);
            this.zDropEditWithFixedWidthExcessShortInd.TabIndex = 20;
            // 
            // zGridPacks
            // 
            this.zGridPacks.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.zGridPacks, "Bills.Packs");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).ContainerPK)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_PackQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_PackUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_CargoType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_PackagesOutturned)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_PackageCondition)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.PackCondDesc)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_GoodsDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.ExcessShortInd)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_GoodsDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.ContShouldBe)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_Weight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_WeightUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_WeightOutturned)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_WeightOutturnedUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_Volume)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_VolumeUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_VolumeOutturned)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_VolumeOutturnedUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).APA_MarksAndNumbers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ZA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).Packs)).SyncRoot)).Outturn.C5_SealIntactIndicator)));
            this.zGridPacks.CaptionVisible = false;
            zGuidDropEditColumnStyleInfo1.ColumnName = "ContainerPK";
            zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "APA_PackQty";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("12443cc7-9f20-4b5d-bbdc-6705b1327fff", "Pack Qty");
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "APA_PackUQ";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("12443cc7-9f20-4b5d-bbdc-6705b1327fff", "Pack Qty");
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo2.ColumnName = "Outturn+C5_CargoType";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "Outturn+C5_PackagesOutturned";
            zCalcEditColumnStyleInfo2.Decimals = 0;
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo3.ColumnName = "Outturn+C5_PackageCondition";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zMultiLineTextBoxColumnInfo1.ColumnName = "Outturn+PackCondDesc";
            zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
            zMultiLineTextBoxColumnInfo1.IsVisible = false;
            zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
            zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zMultiLineTextBoxColumnInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zMultiLineTextBoxColumnInfo2.ColumnName = "Outturn+C5_GoodsDescription";
            zMultiLineTextBoxColumnInfo2.DefaultCollectionIndex = 0;
            zMultiLineTextBoxColumnInfo2.IsVisible = false;
            zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
            zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo4.ColumnName = "Outturn+ExcessShortInd";
            zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zMultiLineTextBoxColumnInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zMultiLineTextBoxColumnInfo3.ColumnName = "APA_GoodsDescription";
            zMultiLineTextBoxColumnInfo3.DefaultCollectionIndex = 0;
            zMultiLineTextBoxColumnInfo3.IsVisible = false;
            zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 300;
            zMultiLineTextBoxColumnInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zMultiLineTextBoxColumnInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zMultiLineTextBoxColumnInfo4.ColumnName = "Outturn+ContShouldBe";
            zMultiLineTextBoxColumnInfo4.DefaultCollectionIndex = 0;
            zMultiLineTextBoxColumnInfo4.IsVisible = false;
            zMultiLineTextBoxColumnInfo4.MinimumEditControlWidth = 300;
            zMultiLineTextBoxColumnInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "APA_Weight";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("67b334b0-18df-4cf0-8985-08da5e4d14d1", "Weight Manifested");
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo5.ColumnName = "APA_WeightUQ";
            zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo5.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("67b334b0-18df-4cf0-8985-08da5e4d14d1", "Weight Manifested");
            zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.ColumnName = "Outturn+C5_WeightOutturned";
            zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("f9e9113e-2876-4655-be28-0782bd684d30", "Weight Outturned");
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo6.ColumnName = "Outturn+C5_WeightOutturnedUQ";
            zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo6.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("f9e9113e-2876-4655-be28-0782bd684d30", "Weight Outturned");
            zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo5.ColumnName = "APA_Volume";
            zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo5.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("82b1d779-eb45-4cfc-ac60-64b5081b7337", "Volume Manifested");
            zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo7.ColumnName = "APA_VolumeUQ";
            zDropEditColumnStyleInfo7.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo7.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("82b1d779-eb45-4cfc-ac60-64b5081b7337", "Volume Manifested");
            zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo6.ColumnName = "Outturn+C5_VolumeOutturned";
            zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("a5fd0c75-1e6c-4be6-a295-45c749e9896a", "Volume Found");
            zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo8.ColumnName = "Outturn+C5_VolumeOutturnedUQ";
            zDropEditColumnStyleInfo8.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo8.GroupName = Enterprise.Customs.ZA.GUI.Res.GetData("a5fd0c75-1e6c-4be6-a295-45c749e9896a", "Volume Found");
            zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zMultiLineTextBoxColumnInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zMultiLineTextBoxColumnInfo5.ColumnName = "APA_MarksAndNumbers";
            zMultiLineTextBoxColumnInfo5.DefaultCollectionIndex = 0;
            zMultiLineTextBoxColumnInfo5.IsVisible = false;
            zMultiLineTextBoxColumnInfo5.MinimumEditControlWidth = 300;
            zMultiLineTextBoxColumnInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo1.ColumnName = "Outturn+C5_SealIntactIndicator";
            zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.zGridPacks.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
            this.zGridPacks.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.zGridPacks.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.zGridPacks.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.zGridPacks.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.zGridPacks.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.zGridPacks.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
            this.zGridPacks.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
            this.zGridPacks.ColumnStyles.Add(zDropEditColumnStyleInfo4);
            this.zGridPacks.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
            this.zGridPacks.ColumnStyles.Add(zMultiLineTextBoxColumnInfo4);
            this.zGridPacks.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.zGridPacks.ColumnStyles.Add(zDropEditColumnStyleInfo5);
            this.zGridPacks.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.zGridPacks.ColumnStyles.Add(zDropEditColumnStyleInfo6);
            this.zGridPacks.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
            this.zGridPacks.ColumnStyles.Add(zDropEditColumnStyleInfo7);
            this.zGridPacks.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
            this.zGridPacks.ColumnStyles.Add(zDropEditColumnStyleInfo8);
            this.zGridPacks.ColumnStyles.Add(zMultiLineTextBoxColumnInfo5);
            this.zGridPacks.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.zGridPacks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zGridPacks.GridId = "845573da-c8d5-40bf-9b10-6c80cc574e46";
            this.zGridPacks.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.zGridPacks.LayoutKey = "zGrid1";
            this.zGridPacks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zGridPacks.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 67, true);
            this.zGridPacks.Name = "zGridPacks";
            this.zGridPacks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 282, true);
            this.zGridPacks.TabIndex = 0;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.zGridPacks);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.zGroupBoxPackDetail);
            this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 600, true);
            this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(282);
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.TabIndex = 1;
            // 
            // OutturnAndGateInOutPackUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.splitContainer);
            this.Name = "OutturnAndGateInOutPackUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 600, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zGroupBoxPackDetail.ResumeLayout(false);
            this.zGroupBoxPackDetail.PerformLayout();
            this.zPanelPackDetail.ResumeLayout(false);
            this.zPanelPackDetail.PerformLayout();
            this.zDropEditPackageCondition.ResumeLayout(true);
            this.zDropEditPackageCondition.PerformLayout();
            this.zDropEditWithFixedWidthVolumeOutturnedUQ.ResumeLayout(true);
            this.zDropEditWithFixedWidthVolumeOutturnedUQ.PerformLayout();
            this.zDropEditWithFixedWidthWeightOutturnedUQ.ResumeLayout(true);
            this.zDropEditWithFixedWidthWeightOutturnedUQ.PerformLayout();
            this.zDropEditWithFixedWidthCargoType.ResumeLayout(true);
            this.zDropEditWithFixedWidthCargoType.PerformLayout();
            this.zGuidDropEditWithFixedWidthContainerPK.ResumeLayout(true);
            this.zGuidDropEditWithFixedWidthContainerPK.PerformLayout();
            this.zDropEditWithFixedWidthVolumeUQ.ResumeLayout(true);
            this.zDropEditWithFixedWidthVolumeUQ.PerformLayout();
            this.zDropEditWithFixedWidthWeightUQ.ResumeLayout(true);
            this.zDropEditWithFixedWidthWeightUQ.PerformLayout();
            this.zDropEditWithFixedWidthPackUQ.ResumeLayout(true);
            this.zDropEditWithFixedWidthPackUQ.PerformLayout();
            this.zDropEditWithFixedWidthExcessShortInd.ResumeLayout(true);
            this.zDropEditWithFixedWidthExcessShortInd.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGridPacks)).EndInit();
            this.zGridPacks.ResumeLayout(false);
            this.zGridPacks.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.splitContainer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	}
}
