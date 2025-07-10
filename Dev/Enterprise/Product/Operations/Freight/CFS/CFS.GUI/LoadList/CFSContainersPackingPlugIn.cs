using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public class CFSContainersPackingPlugIn : ContainersPackingPlugIn
	{
		public CFSContainersPackingPlugIn(CommonConsol hostEntity)
			: base(hostEntity)
		{
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.CFSManager; }
		}

		protected override void OnUserControlCreated()
		{
			base.OnUserControlCreated();

			ContainersModuleButtonGrid.BindToFindBoxList = "Containers_List";
			ContainersModuleButtonGrid.DetachMessage = Res.GetData("a1ca77fc-939a-408d-8383-89950ba395b7", "Are you sure you want to detach the selected Container?");
			ContainersModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;

			ZTextBoxColumnStyleInfo textBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo textBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo organisationFindBoxColumnStyleInfo1 = new ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo textBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo textBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo dateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo organisationFindBoxColumnStyleInfo2 = new ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo textBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo textBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo dateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo dateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo dateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo calcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo dropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();

			textBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|a424c3ba-e55c-4b31-879d-32862ee3ef49", "Carrier Booking Ref");
			textBoxColumnStyleInfo1.ColumnName = "JC_Calc_ConsolBookingRef";
			textBoxColumnStyleInfo1.IsVisible = false;
			textBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|49311857-7243-4c33-98d4-55d27aa8ba07", "Entry Number");
			textBoxColumnStyleInfo2.ColumnName = "JC_Calc_ConsolEntryNumber";
			textBoxColumnStyleInfo2.IsVisible = false;
			organisationFindBoxColumnStyleInfo1.BindToList = "LocalTransportList";
			organisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|ad02d967-ff4e-4803-a04b-a2010e4cf9a2", "Arv. Transport Co");
			organisationFindBoxColumnStyleInfo1.ColumnName = "CFSArrival+EU_OA_TransportProvider_ZAddress+OrgPK";
			organisationFindBoxColumnStyleInfo1.GroupName = PackingDetailsUserControl.ArrivalResourceData;
			textBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|a938b5e9-1c85-40d5-8e4d-6603cd49fe27", "Arv. Drivers License");
			textBoxColumnStyleInfo3.ColumnName = "JC_ArrivalTruckDriversLicense";
			textBoxColumnStyleInfo3.GroupName = PackingDetailsUserControl.ArrivalResourceData;
			textBoxColumnStyleInfo3.ToolTip = Res.GetString("f8759cb7-c795-4148-acb9-cd37d2e420ef", "Arrival Truck Drivers License");
			textBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|a6320854-e39d-4785-adc4-d6c5e34357a7", "Arv. Rego.");
			textBoxColumnStyleInfo4.ColumnName = "JC_ArrivalTruckRegistration";
			textBoxColumnStyleInfo4.GroupName = PackingDetailsUserControl.ArrivalResourceData;
			textBoxColumnStyleInfo4.ToolTip = Res.GetString("2fbb63c2-51b3-40f6-8033-f8a8b66cd7f2", "Arrival Truck Registration");
			dateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|d0ee1c12-41f9-4b00-b711-f97b7bc735af", "Arv. Date");
			dateEditColumnStyleInfo1.ColumnName = "JC_ArrivalTime";
			dateEditColumnStyleInfo1.GroupName = PackingDetailsUserControl.ArrivalResourceData;
			organisationFindBoxColumnStyleInfo2.BindToList = "LocalTransportList";
			organisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|5550ebe4-5433-4fc1-ab04-22cdc188ede7", "Dep. Transport Co");
			organisationFindBoxColumnStyleInfo2.ColumnName = "CFSDispatch+EU_OA_TransportProvider_ZAddress+OrgPK";
			organisationFindBoxColumnStyleInfo2.GroupName = Res.GetData("1b61c3c2-b11a-401b-a9af-4c464baa9fd7", "Departure");
			textBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|3a04eaae-838a-4c26-a68b-b6ebd3c3caca", "Dep. Drivers License");
			textBoxColumnStyleInfo5.ColumnName = "JC_DepartureTruckDriversLicense";
			textBoxColumnStyleInfo5.GroupName = Res.GetData("1b61c3c2-b11a-401b-a9af-4c464baa9fd7", "Departure");
			textBoxColumnStyleInfo5.ToolTip = Res.GetString("e6a30e34-91ee-46bd-a8ac-7203eebbd6d1", "Departure Driver License");
			textBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|347bd4a1-dbf2-458e-9016-4fd900e9104e", "Dep. Rego.");
			textBoxColumnStyleInfo6.ColumnName = "JC_DepartureTruckRegistration";
			textBoxColumnStyleInfo6.GroupName = Res.GetData("1b61c3c2-b11a-401b-a9af-4c464baa9fd7", "Departure");
			textBoxColumnStyleInfo6.ToolTip = Res.GetString("fbb2167a-16fc-4834-be9e-03e6ecb865bb", "Departure Truck Registration");
			dateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|85a3e34d-b6dd-4439-a68e-2582053003c8", "Dep. Date");
			dateEditColumnStyleInfo2.ColumnName = "JC_DepartureTime";
			dateEditColumnStyleInfo2.GroupName = Res.GetData("1b61c3c2-b11a-401b-a9af-4c464baa9fd7", "Departure");
			dateEditColumnStyleInfo2.ToolTip = Res.GetString("41fa4721-d113-473c-9e0e-25f31f6090ca", "Departure Time");
			dateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|907c4b60-c8f4-4609-944d-c021b14ebcd9", "Available From");
			dateEditColumnStyleInfo3.ColumnName = "JC_LCLAvailable_Readonly";
			dateEditColumnStyleInfo3.IsVisible = false;
			dateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|aa1ce0fc-3878-473d-8fd9-a28f9d6d8876", "Storage Date");
			dateEditColumnStyleInfo4.ColumnName = "JC_LCLStorageCommences_Readonly";
			dateEditColumnStyleInfo4.IsVisible = false;
			calcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			calcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|4f77fce0-1e17-479a-8784-c7a6fc2339ba", "Gross Weight");
			calcEditColumnStyleInfo1.ColumnName = "JC_GrossWeight";
			calcEditColumnStyleInfo1.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|9e9ae1a3-827c-42df-a582-c32e8459593a", "Gross Weight");
			dropEditColumnStyleInfo1.ColumnName = "JC_GrossWeightUQ";
			dropEditColumnStyleInfo1.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("ZModuleButtonGrid|9e9ae1a3-827c-42df-a582-c32e8459593a", "Gross Weight");
			dropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);

			ContainersModuleButtonGrid.ColumnStyles.Add(textBoxColumnStyleInfo1);
			ContainersModuleButtonGrid.ColumnStyles.Add(textBoxColumnStyleInfo2);
			ContainersModuleButtonGrid.ColumnStyles.Add(organisationFindBoxColumnStyleInfo1);
			ContainersModuleButtonGrid.ColumnStyles.Add(textBoxColumnStyleInfo3);
			ContainersModuleButtonGrid.ColumnStyles.Add(textBoxColumnStyleInfo4);
			ContainersModuleButtonGrid.ColumnStyles.Add(dateEditColumnStyleInfo1);
			ContainersModuleButtonGrid.ColumnStyles.Add(organisationFindBoxColumnStyleInfo2);
			ContainersModuleButtonGrid.ColumnStyles.Add(textBoxColumnStyleInfo5);
			ContainersModuleButtonGrid.ColumnStyles.Add(textBoxColumnStyleInfo6);
			ContainersModuleButtonGrid.ColumnStyles.Add(dateEditColumnStyleInfo2);
			ContainersModuleButtonGrid.ColumnStyles.Add(dateEditColumnStyleInfo3);
			ContainersModuleButtonGrid.ColumnStyles.Add(dateEditColumnStyleInfo4);
			ContainersModuleButtonGrid.ColumnStyles.Add(calcEditColumnStyleInfo1);
			ContainersModuleButtonGrid.ColumnStyles.Add(dropEditColumnStyleInfo1);

			userControlSetup = false;
		}

		bool userControlSetup;

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			if (!this.userControlSetup)
			{
				this.userControlSetup = true;
				PackingDetailsUserControl.SetupPlugIn();
				SetupVGMTabPage();
			}
		}

		void SetupVGMTabPage()
		{
			VGMTabPage = new ZTabPage();
			VerifiedByDocAddressControl = new ZDocAddressControl();
			VerifiedWeightGroupBox = new ZGroupBox();
			VerifiedDateEdit = new ZDateEdit();
			VerifiedMethodDropEdit = new ZDropEdit();
			TotalGrossWeightCalcEdit = new ZCalcEdit();
			GrossWeightUQDropEdit = new ZDropEdit();

			VGMTabPage.SuspendLayout();
			VerifiedByDocAddressControl.SuspendLayout();
			VerifiedWeightGroupBox.SuspendLayout();
			VerifiedDateEdit.SuspendLayout();
			VerifiedMethodDropEdit.SuspendLayout();
			TotalGrossWeightCalcEdit.SuspendLayout();
			GrossWeightUQDropEdit.SuspendLayout();

			PackingDetailsUserControl.ContainerDetailsTabControl.Controls.Add(VGMTabPage);

			// 
			// VGMTabPage
			// 
			VGMTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("9b7b1774-9515-49a6-a2cb-b62374b9f2f5", "VGM");
			VGMTabPage.Controls.Add(VerifiedByDocAddressControl);
			VGMTabPage.Controls.Add(VerifiedWeightGroupBox);
			VGMTabPage.Controls.Add(TotalGrossWeightCalcEdit);
			VGMTabPage.Controls.Add(GrossWeightUQDropEdit);
			VGMTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			VGMTabPage.Name = "VGMTabPage";
			VGMTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			VGMTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1058, 305, true);
			VGMTabPage.TabIndex = 1;
			// 
			// VerifiedByDocAddressControl
			// 
			VerifiedByDocAddressControl.AllowDrop = true;
			PackingDetailsUserControl.BindingSource.SetBindingMember(VerifiedByDocAddressControl, "Containers.GrossWeightVerifiedByAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonContainer)(((System.Collections.IList)(((CommonConsol)(null)).Containers)).SyncRoot)).GrossWeightVerifiedByAddress);
			VerifiedByDocAddressControl.BindToOrganisations = "Containers.Lookups.GrossWeightVerifiedByList";
			VerifiedByDocAddressControl.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("20d6a8b8-d9c3-48f0-b59c-a44829cc9548", "VGM Verified By");
			VerifiedByDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 6, true);
			VerifiedByDocAddressControl.Name = "VerifiedByDocAddressControl";
			VerifiedByDocAddressControl.ReadOnly = false;
			VerifiedByDocAddressControl.SingleLineNoGroupBoxPanelWidth = 370;
			VerifiedByDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			VerifiedByDocAddressControl.TabIndex = 1;
			// 
			// VerifiedWeightGroupBox
			// 
			VerifiedWeightGroupBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("0757dcfb-cb78-4f01-b86f-7a4e8d04a3a0", "Verified Weight");
			VerifiedWeightGroupBox.Controls.Add(VerifiedDateEdit);
			VerifiedWeightGroupBox.Controls.Add(VerifiedMethodDropEdit);
			VerifiedWeightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			VerifiedWeightGroupBox.Name = "VerifiedWeightGroupBox";
			VerifiedWeightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 64, true);
			VerifiedWeightGroupBox.TabIndex = 0;
			VerifiedWeightGroupBox.TabStop = false;
			// 
			// VerifiedDateEdit
			// 
			VerifiedDateEdit.AllowDrop = true;
			VerifiedDateEdit.AutoCompleteMonthThreshold = 1;
			VerifiedDateEdit.AutoCompleteYear = true;
			PackingDetailsUserControl.BindingSource.SetBindingMember(VerifiedDateEdit, "Containers.JC_GrossWeightVerificationDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonContainer)(((System.Collections.IList)(((CommonConsol)(null)).Containers)).SyncRoot)).JC_GrossWeightVerificationDateTime);
			VerifiedDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("d0c81f92-3551-4c00-832b-978e5ade4e45", "Verified Date");
			VerifiedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			VerifiedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 39, true);
			VerifiedDateEdit.Name = "VerifiedDateEdit";
			VerifiedDateEdit.TabIndex = 1;
			// 
			// VerifiedMethodDropEdit
			// 
			VerifiedMethodDropEdit.AllowDrop = true;
			PackingDetailsUserControl.BindingSource.SetBindingMember(VerifiedMethodDropEdit, "Containers.JC_GrossWeightVerificationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonContainer)(((System.Collections.IList)(((CommonConsol)(null)).Containers)).SyncRoot)).JC_GrossWeightVerificationType);
			VerifiedMethodDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("e61eac70-eb05-4cd9-a0f9-7099f440458d", "Verified Method");
			VerifiedMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 17, true);
			VerifiedMethodDropEdit.Name = "VerifiedMethodDropEdit";
			VerifiedMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			VerifiedMethodDropEdit.TabIndex = 0;
			// 
			// TotalGrossWeightCalcEdit
			// 
			PackingDetailsUserControl.BindingSource.SetBindingMember(this.TotalGrossWeightCalcEdit, "Containers.JC_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonContainer)(null)).JobContainer.JC_GrossWeight);
			this.TotalGrossWeightCalcEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("682d920a-363a-4164-adf8-0db4a9edcba3", "Gross Wgt.");
			this.TotalGrossWeightCalcEdit.DecimalPlaces = 2;
			this.TotalGrossWeightCalcEdit.IsCalculatorEnabled = false;
			this.TotalGrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 74, true);
			this.TotalGrossWeightCalcEdit.Name = "TotalGrossWeightCalcEdit";
			this.TotalGrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.TotalGrossWeightCalcEdit.TabIndex = 2;
			this.TotalGrossWeightCalcEdit.Text = "0.000";
			this.TotalGrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightUQDropEdit
			// 
			this.GrossWeightUQDropEdit.AllowDrop = true;
			PackingDetailsUserControl.BindingSource.SetBindingMember(this.GrossWeightUQDropEdit, "Containers.WeightUnitForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonContainer)(null)).WeightUnitForBinding);
			this.GrossWeightUQDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("72d4d60a-6b0c-4991-9f3d-d64800c464dc", "Weight UQ");
			this.GrossWeightUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 96, true);
			this.GrossWeightUQDropEdit.Name = "GrossWeightUQDropEdit";
			this.GrossWeightUQDropEdit.PreBoundMaxLength = 2;
			this.GrossWeightUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.GrossWeightUQDropEdit.TabIndex = 3;

			VGMTabPage.ResumeLayout(false);
			VGMTabPage.PerformLayout();
			VerifiedByDocAddressControl.ResumeLayout(true);
			VerifiedByDocAddressControl.PerformLayout();
			VerifiedWeightGroupBox.ResumeLayout(false);
			VerifiedWeightGroupBox.PerformLayout();
			VerifiedDateEdit.ResumeLayout(true);
			VerifiedDateEdit.PerformLayout();
			VerifiedMethodDropEdit.ResumeLayout(true);
			VerifiedMethodDropEdit.PerformLayout();
			GrossWeightUQDropEdit.ResumeLayout(true);
			GrossWeightUQDropEdit.PerformLayout();
		}

		ZTabPage VGMTabPage;
		ZDateEdit VerifiedDateEdit;
		ZDropEdit VerifiedMethodDropEdit;
		ZGroupBox VerifiedWeightGroupBox;
		ZDocAddressControl VerifiedByDocAddressControl;
		ZCalcEdit TotalGrossWeightCalcEdit;
		ZDropEdit GrossWeightUQDropEdit;

		protected override void OnUserControlAfterFirstBinding()
		{
			if (HostBusinessEntity is CFSLoadListConsol)
			{
				var packLines = UnAllocatedPackLines;
				if (packLines != null)
				{
					(HostBusinessEntity as CFSLoadListConsol).ValidatePackLinesRelatingToConsol(packLines);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (VerifiedDateEdit != null)
				{
					VerifiedDateEdit.Dispose();
				}

				if (VerifiedMethodDropEdit != null)
				{
					VerifiedMethodDropEdit.Dispose();
				}

				if (VerifiedWeightGroupBox != null)
				{
					VerifiedWeightGroupBox.Dispose();
				}

				if (VerifiedByDocAddressControl != null)
				{
					VerifiedByDocAddressControl.Dispose();
				}

				if (TotalGrossWeightCalcEdit != null)
				{
					TotalGrossWeightCalcEdit.Dispose();
				}

				if (GrossWeightUQDropEdit != null)
				{
					GrossWeightUQDropEdit.Dispose();
				}

				if (VGMTabPage != null)
				{
					VGMTabPage.Dispose();
				}
			}
		}

		protected override bool IsEligibleForUpdatingExportReferenceNumber => false;
	}
}
