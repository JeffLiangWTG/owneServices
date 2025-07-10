using Enterprise.Freight.Business;

namespace Enterprise.Freight.Module
{
	public partial class ContainersFilterControl
	{

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo emptyReturnReferenceColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo additionalNumbersColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo allocationIDColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			this.BindingSource.SetBindingMember(this.FilteredGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.CommonContainer)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ReleaseNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_DepartureSlotReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ArrivalSlotReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).Container.RC_ShippingMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_FCLAvailable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ArrivalCTOStorageStartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_FCLOnBoardVessel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_FCLUnloadFromVessel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_FCLWharfGateIn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_FCLWharfGateOut)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ContainerYardEmptyPickupGateOut)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ContainerYardEmptyReturnGateIn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ContainerStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ContainerQuality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_AdditionalSealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_Additional2SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_SealParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_AdditionalSealParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_Additional2SealParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_IsEmptyContainer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_IsDamaged)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_IsSealOk)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_IsControlledAtmosphere)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_SetPointTemp)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_SetPointTempUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_TempRecorderSerialNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_RH_NKContainerCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_GrossWeightVerificationStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_EmptyReturnReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.CommonContainer)(null)).JC_RCA_AllocationLine)));
			zTextBoxColumnStyleInfo1.ColumnName = "JC_ContainerNum";
			zTextBoxColumnStyleInfo2.ColumnName = "JC_SealNum";
			zTextBoxColumnStyleInfo3.ColumnName = "JC_ReleaseNum";
			zTextBoxColumnStyleInfo4.ColumnName = "JC_DepartureSlotReference";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.ColumnName = "JC_ArrivalSlotReference";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.ColumnName = "Container+RC_ShippingMode";
			zDateEditColumnStyleInfo1.ColumnName = "JC_FCLAvailable";
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo2.ColumnName = "JC_ArrivalCTOStorageStartDate";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo3.ColumnName = "JC_FCLOnBoardVessel";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo4.ColumnName = "JC_FCLUnloadFromVessel";
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo5.ColumnName = "JC_FCLWharfGateIn";
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo6.ColumnName = "JC_FCLWharfGateOut";
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo7.ColumnName = "JC_ContainerYardEmptyPickupGateOut";
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo8.ColumnName = "JC_ContainerYardEmptyReturnGateIn";
			zDateEditColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo7.ColumnName = "JC_ContainerStatus";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo8.ColumnName = "JC_ContainerQuality";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.ColumnName = "JC_AdditionalSealNum";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo10.ColumnName = "JC_Additional2SealNum";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "JC_DeliverySequence";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "JC_SealParty";
			zDropEditColumnStyleInfo2.ColumnName = "JC_AdditionalSealParty";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "JC_Additional2SealParty";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "JC_IsEmptyContainer";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo2.ColumnName = "JC_IsDamaged";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo3.ColumnName = "JC_IsSealOk";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("ContainersFilterControl|9dfcce77-9ca6-48ce-ac97-9763a28f14ea", "Temp. Controlled");
			zCheckBoxColumnStyleInfo4.ColumnName = "JC_IsControlledAtmosphere";
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo1.ColumnName = "JC_SetPointTemp";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo4.ColumnName = "JC_SetPointTempUnit";
			zDropEditColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo12.ColumnName = "JC_TempRecorderSerialNo";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo13.ColumnName = "JC_RH_NKContainerCommodityCode";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zDropEditColumnStyleInfo5.ColumnName = "JC_GrossWeightVerificationStatus";
			zDropEditColumnStyleInfo5.IsVisible = false;
			allocationIDColumnStyleInfo.ColumnName = "JC_RCA_AllocationLine";
			allocationIDColumnStyleInfo.IsVisible = false;
			emptyReturnReferenceColumnStyleInfo.ColumnName = "JC_EmptyReturnReference";
			emptyReturnReferenceColumnStyleInfo.IsVisible = false;
			additionalNumbersColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("ContainersFilterControl|75a9d384-2499-432a-ac33-008847d561e1", "Additional Reference");
			additionalNumbersColumnStyleInfo.ColumnName = "AdditionalReferenceNumbersAsString";
			additionalNumbersColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			additionalNumbersColumnStyleInfo.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(emptyReturnReferenceColumnStyleInfo);
			this.FilteredGrid.ColumnStyles.Add(additionalNumbersColumnStyleInfo);
			if (ContractsPermissions.IsAllocationsVisible())
			{
				this.FilteredGrid.ColumnStyles.Add(allocationIDColumnStyleInfo);
			}
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 56, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 360, true);
			this.FilteredGrid.TabIndex = 18;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommonContainer);
			// 
			// ContainersFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ContainersFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
