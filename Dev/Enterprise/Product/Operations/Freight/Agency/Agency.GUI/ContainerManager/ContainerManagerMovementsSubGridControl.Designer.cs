namespace Enterprise.Freight.Agency.GUI
{
	partial class ContainerManagerMovementsSubGridControl
	{
		void InitializeComponent()
		{
			relatedBillGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			principalTextBox = new Enterprise.ZArchitecture.ZTextBox();
			localClientTextBox = new Enterprise.ZArchitecture.ZTextBox();
			consigneeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			consignorTextBox = new Enterprise.ZArchitecture.ZTextBox();
			billsOfLadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			shipmentNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			originTextBox = new Enterprise.ZArchitecture.ZTextBox();
			destinationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			detentionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			detentionFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			detentionFreeDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			detentionFreeDays = new Enterprise.ZArchitecture.ZCalcEdit();
			movementDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			detentionStartsDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			freeDaysStartDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			voyageInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			voyageNumber = new Enterprise.ZArchitecture.ZTextBox();
			dischargePortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			loadPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			vesselFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			detentionPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			depotDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			relatedBillGroupBox.SuspendLayout();
			detentionGroupBox.SuspendLayout();
			voyageInfoGroupBox.SuspendLayout();
			depotDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.ContainerMovement);
			// 
			// relatedBillGroupBox
			// 
			relatedBillGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerManagerMovementsSubGridControl|8791dea1-1a48-433e-8a76-1b72e9eaa1ce", "Related Bill Details");
			relatedBillGroupBox.Controls.Add(principalTextBox);
			relatedBillGroupBox.Controls.Add(localClientTextBox);
			relatedBillGroupBox.Controls.Add(consigneeTextBox);
			relatedBillGroupBox.Controls.Add(consignorTextBox);
			relatedBillGroupBox.Controls.Add(billsOfLadingTextBox);
			relatedBillGroupBox.Controls.Add(shipmentNumbersTextBox);
			relatedBillGroupBox.Controls.Add(originTextBox);
			relatedBillGroupBox.Controls.Add(destinationTextBox);
			relatedBillGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 0, true);
			relatedBillGroupBox.Name = "relatedBillGroupBox";
			relatedBillGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 160, true);
			relatedBillGroupBox.TabIndex = 2;
			relatedBillGroupBox.TabStop = false;
			// 
			// principalTextBox
			// 
			this.BindingSource.SetBindingMember(principalTextBox, "RelatedInfo+FallBackPrincipal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.FallBackPrincipal)));
			principalTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 64, true);
			principalTextBox.Name = "principalTextBox";
			principalTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			principalTextBox.TabIndex = 6;
			// 
			// localClientTextBox
			// 
			this.BindingSource.SetBindingMember(localClientTextBox, "RelatedInfo+FallBackLocalClient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.FallBackLocalClient)));
			localClientTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 64, true);
			localClientTextBox.Name = "localClientTextBox";
			localClientTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			localClientTextBox.TabIndex = 5;
			// 
			// consigneeTextBox
			// 
			this.BindingSource.SetBindingMember(consigneeTextBox, "RelatedInfo+Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.Consignee)));
			consigneeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 40, true);
			consigneeTextBox.Name = "consigneeTextBox";
			consigneeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			consigneeTextBox.TabIndex = 4;
			// 
			// consignorTextBox
			// 
			this.BindingSource.SetBindingMember(consignorTextBox, "RelatedInfo+Consignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.Consignor)));
			consignorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 40, true);
			consignorTextBox.Name = "consignorTextBox";
			consignorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			consignorTextBox.TabIndex = 3;
			// 
			// billsOfLadingTextBox
			// 
			this.BindingSource.SetBindingMember(billsOfLadingTextBox, "RelatedInfo+BillsOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.BillsOfLading)));
			billsOfLadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 16, true);
			billsOfLadingTextBox.Name = "billsOfLadingTextBox";
			billsOfLadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			billsOfLadingTextBox.TabIndex = 2;
			// 
			// shipmentNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(shipmentNumbersTextBox, "RelatedInfo+ShipmentNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.ShipmentNumbers)));
			shipmentNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			shipmentNumbersTextBox.Name = "shipmentNumbersTextBox";
			shipmentNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			shipmentNumbersTextBox.TabIndex = 1;
			// 
			// originTextBox
			// 
			this.BindingSource.SetBindingMember(originTextBox, "RelatedInfo+Origin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.Origin)));
			originTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 88, true);
			originTextBox.Name = "originTextBox";
			originTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			originTextBox.TabIndex = 7;
			// 
			// destinationTextBox
			// 
			this.BindingSource.SetBindingMember(destinationTextBox, "RelatedInfo+Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.Destination)));
			destinationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 88, true);
			destinationTextBox.Name = "destinationTextBox";
			destinationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			destinationTextBox.TabIndex = 8;
			// 
			// detentionGroupBox
			// 
			detentionGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerManagerMovementsSubGridControl|8b6f0abc-f91a-4c59-acff-740366a4c994", "Detention Details");
			detentionGroupBox.Controls.Add(detentionFindBox);
			detentionGroupBox.Controls.Add(detentionFreeDaysCalcEdit);
			detentionGroupBox.Controls.Add(detentionFreeDays);
			detentionGroupBox.Controls.Add(movementDate);
			detentionGroupBox.Controls.Add(detentionStartsDate);
			detentionGroupBox.Controls.Add(freeDaysStartDate);
			detentionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 0, true);
			detentionGroupBox.Name = "detentionGroupBox";
			detentionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 160, true);
			detentionGroupBox.TabIndex = 3;
			detentionGroupBox.TabStop = false;
			// 
			// detentionFindBox
			// 
			this.BindingSource.SetBindingMember(detentionFindBox, "E9_NC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).E9_NC)));
			detentionFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 136, true);
			detentionFindBox.Name = "detentionFindBox";
			detentionFindBox.ShowDescriptionBox = false;
			detentionFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			detentionFindBox.TabIndex = 5;
			// 
			// detentionFreeDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(detentionFreeDaysCalcEdit, "RelatedInfo+DetentionFreeDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.DetentionFreeDays)));
			detentionFreeDaysCalcEdit.DecimalPlaces = 2;
			detentionFreeDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 40, true);
			detentionFreeDaysCalcEdit.Name = "detentionFreeDaysCalcEdit";
			detentionFreeDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			detentionFreeDaysCalcEdit.TabIndex = 1;
			detentionFreeDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// detentionFreeDays
			// 
			this.BindingSource.SetBindingMember(detentionFreeDays, "E9_DetentionDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).E9_DetentionDays)));
			detentionFreeDays.DecimalPlaces = 2;
			detentionFreeDays.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 88, true);
			detentionFreeDays.Name = "detentionFreeDays";
			detentionFreeDays.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			detentionFreeDays.TabIndex = 3;
			detentionFreeDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// movementDate
			// 
			movementDate.AutoCompleteMonthThreshold = 1;
			movementDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(movementDate, "E9_MovementDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).E9_MovementDate)));
			movementDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 112, true);
			movementDate.Name = "movementDate";
			movementDate.TabIndex = 4;
			// 
			// detentionStartsDate
			// 
			detentionStartsDate.AutoCompleteMonthThreshold = 1;
			detentionStartsDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(detentionStartsDate, "RelatedInfo+StartOfDetentionPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.StartOfDetentionPeriod)));
			detentionStartsDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 64, true);
			detentionStartsDate.Name = "detentionStartsDate";
			detentionStartsDate.TabIndex = 2;
			// 
			// freeDaysStartDate
			// 
			freeDaysStartDate.AutoCompleteMonthThreshold = 1;
			freeDaysStartDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(freeDaysStartDate, "RelatedInfo+StartOfDetentionFreePeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.StartOfDetentionFreePeriod)));
			freeDaysStartDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			freeDaysStartDate.Name = "freeDaysStartDate";
			freeDaysStartDate.TabIndex = 0;
			// 
			// voyageInfoGroupBox
			// 
			voyageInfoGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerManagerMovementsSubGridControl|a4a742f3-464f-405a-ae1f-73115a54c704", "Voyage Details");
			voyageInfoGroupBox.Controls.Add(voyageNumber);
			voyageInfoGroupBox.Controls.Add(dischargePortFindBox);
			voyageInfoGroupBox.Controls.Add(loadPortFindBox);
			voyageInfoGroupBox.Controls.Add(vesselFindBox);
			voyageInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			voyageInfoGroupBox.Name = "voyageInfoGroupBox";
			voyageInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 112, true);
			voyageInfoGroupBox.TabIndex = 0;
			voyageInfoGroupBox.TabStop = false;
			// 
			// voyageNumber
			// 
			this.BindingSource.SetBindingMember(voyageNumber, "VoyageNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).VoyageNo)));
			voyageNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 40, true);
			voyageNumber.Name = "voyageNumber";
			voyageNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			voyageNumber.TabIndex = 1;
			// 
			// dischargePortFindBox
			// 
			this.BindingSource.SetBindingMember(dischargePortFindBox, "RelatedInfo+DischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.DischargePort)));
			dischargePortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88, true);
			dischargePortFindBox.Name = "dischargePortFindBox";
			dischargePortFindBox.PreBoundMaxLength = 5;
			dischargePortFindBox.ShowDescriptionBox = false;
			dischargePortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			dischargePortFindBox.TabIndex = 3;
			// 
			// loadPortFindBox
			// 
			this.BindingSource.SetBindingMember(loadPortFindBox, "RelatedInfo+LoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).RelatedInfo.LoadPort)));
			loadPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			loadPortFindBox.Name = "loadPortFindBox";
			loadPortFindBox.PreBoundMaxLength = 5;
			loadPortFindBox.ShowDescriptionBox = false;
			loadPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			loadPortFindBox.TabIndex = 2;
			// 
			// vesselFindBox
			// 
			this.BindingSource.SetBindingMember(vesselFindBox, "VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).VesselName)));
			vesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			vesselFindBox.Name = "vesselFindBox";
			vesselFindBox.PreBoundMaxLength = 15;
			vesselFindBox.ShowDescriptionBox = false;
			vesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			vesselFindBox.TabIndex = 0;
			// 
			// detentionPortFindBox
			// 
			this.BindingSource.SetBindingMember(detentionPortFindBox, "DepotPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerMovement)(null)).DepotPort)));
			detentionPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			detentionPortFindBox.Name = "detentionPortFindBox";
			detentionPortFindBox.PreBoundMaxLength = 5;
			detentionPortFindBox.ShowDescriptionBox = false;
			detentionPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			detentionPortFindBox.TabIndex = 0;
			// 
			// depotDetailsGroupBox
			// 
			depotDetailsGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerManagerMovementsSubGridControl|ddfb565c-dee7-4e41-bbd4-b92753af405e", "Depot Details");
			depotDetailsGroupBox.Controls.Add(detentionPortFindBox);
			depotDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 112, true);
			depotDetailsGroupBox.Name = "depotDetailsGroupBox";
			depotDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 48, true);
			depotDetailsGroupBox.TabIndex = 1;
			depotDetailsGroupBox.TabStop = false;
			// 
			// ContainerManagerMovementsSubGridControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(depotDetailsGroupBox);
			this.Controls.Add(voyageInfoGroupBox);
			this.Controls.Add(detentionGroupBox);
			this.Controls.Add(relatedBillGroupBox);
			this.Name = "ContainerManagerMovementsSubGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 169, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			relatedBillGroupBox.ResumeLayout(false);
			relatedBillGroupBox.PerformLayout();
			detentionGroupBox.ResumeLayout(false);
			detentionGroupBox.PerformLayout();
			voyageInfoGroupBox.ResumeLayout(false);
			voyageInfoGroupBox.PerformLayout();
			depotDetailsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZGroupBox relatedBillGroupBox;
		Enterprise.ZArchitecture.ZTextBox principalTextBox;
		Enterprise.ZArchitecture.ZTextBox localClientTextBox;
		Enterprise.ZArchitecture.ZTextBox consigneeTextBox;
		Enterprise.ZArchitecture.ZTextBox consignorTextBox;
		Enterprise.ZArchitecture.ZTextBox billsOfLadingTextBox;
		Enterprise.ZArchitecture.ZTextBox shipmentNumbersTextBox;
		Enterprise.ZArchitecture.ZTextBox originTextBox;
		Enterprise.ZArchitecture.ZTextBox destinationTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox detentionGroupBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox detentionFindBox;
		Enterprise.ZArchitecture.ZCalcEdit detentionFreeDaysCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit detentionFreeDays;
		Enterprise.ZArchitecture.GUI.ZDateEdit movementDate;
		Enterprise.ZArchitecture.GUI.ZDateEdit detentionStartsDate;
		Enterprise.ZArchitecture.GUI.ZDateEdit freeDaysStartDate;
		Enterprise.ZArchitecture.GUI.ZGroupBox voyageInfoGroupBox;
		Enterprise.ZArchitecture.ZTextBox voyageNumber;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox dischargePortFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox loadPortFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox vesselFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox detentionPortFindBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox depotDetailsGroupBox;
	}
}
