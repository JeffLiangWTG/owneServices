using System;
using System.Collections.Generic;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class ImportPackingUserControl : Customs.GUI.BasePackingControl
	{
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				System.Diagnostics.Trace.Write("");
			}
		}

		public ImportPackingUserControl()
		{
			InitializeComponent();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void HookControlVisibilityChangeEvents(Customs.Business.BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);

			if (JobDeclaration != null)
			{
				JobDeclaration.JE_MessageTypeInfo.ValueChanged += new EventHandler(JE_MessageTypeInfo_ValueChanged);
				JobDeclaration.JE_ApplicationCodeInfo.ValueChanged += new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
				JobDeclaration.JE_TransportModeInfo.ValueChanged += new EventHandler(JE_TransportModeInfo_ValueChanged);
				JobDeclaration.US_CargoReleaseTypeInfo.ValueChanged += new EventHandler(US_CargoReleaseTypeInfo_ValueChanged);
				JobDeclaration.US_EntryTypeInfo.ValueChanged += US_EntryTypeInfo_ValueChanged;
				JobDeclaration.US_NonAMSInfo.ValueChanged += US_NonAMSInfo_ValueChanged;
				JobDeclaration.US_F_AdmissionTypeInfo.ValueChanged += US_F_AdmissionTypeInfo_ValueChanged;
			}
		}

		protected override void UnHookControlVisibilityChangeEvents(Customs.Business.BaseJobDeclaration declaration)
		{
			base.UnHookControlVisibilityChangeEvents(declaration);

			if (JobDeclaration != null)
			{
				JobDeclaration.JE_MessageTypeInfo.ValueChanged -= new EventHandler(JE_MessageTypeInfo_ValueChanged);
				JobDeclaration.JE_ApplicationCodeInfo.ValueChanged -= new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
				JobDeclaration.JE_TransportModeInfo.ValueChanged -= new EventHandler(JE_TransportModeInfo_ValueChanged);
				JobDeclaration.US_CargoReleaseTypeInfo.ValueChanged -= new EventHandler(US_CargoReleaseTypeInfo_ValueChanged);
				JobDeclaration.US_EntryTypeInfo.ValueChanged -= US_EntryTypeInfo_ValueChanged;
				JobDeclaration.US_NonAMSInfo.ValueChanged -= US_NonAMSInfo_ValueChanged;
				JobDeclaration.US_F_AdmissionTypeInfo.ValueChanged -= US_F_AdmissionTypeInfo_ValueChanged;
			}
		}

		void US_CargoReleaseTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideSplitDetails();
			ShowOrHideExpressTrackingNumber();
		}

		void JE_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideSplitDetails();
			ShowOrHideExpressTrackingNumber();
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideSplitDetails();
			ShowOrHideExpressTrackingNumber();
		}

		void ShowOrHideSplitDetails()
		{
			if (JobDeclaration != null)
			{
				var splitDetailsColumns = new string[] { ITAndSplitDetails.Schema.US_ArrivalDate, ITAndSplitDetails.Schema.US_CarrierCode, ITAndSplitDetails.Schema.US_FlightNumber };

				if (JobDeclaration.AreSplitDetailsRelevant)
				{
					HouseBillsGrid.AddToAvailableColumns(Bill.Schema.US_SESplitShip);

					ITNosSplitTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("75af7b84-e5c1-427e-92ba-ff409b78c4d7", "IT Numbers And Qty/Split Shipment Details");

					itAndSplitDetailsUserControl.ITNumbersGrid.AddToAvailableColumns(splitDetailsColumns);
				}
				else
				{
					HouseBillsGrid.RemoveFromAvailableColumns(Bill.Schema.US_SESplitShip);
					RemoveSplitDetailsFromGrid(splitDetailsColumns);
				}
			}
		}

		void ShowOrHideExpressTrackingNumber()
		{
			var isExpressTrackingNumberRelevant = JobDeclaration != null && JobDeclaration.IsExpressTrackingNumberRelevant;
			if (isExpressTrackingNumberRelevant)
			{
				HouseBillsGrid.SetAvailability(true, Bill.Schema.US_ExpressTracking);
			}
			else
			{
				HouseBillsGrid.RemoveFromAvailableColumns(Bill.Schema.US_ExpressTracking);
			}
		}

		void RemoveSplitDetailsFromGrid(string[] splitDetailsColumns)
		{
			ITNosSplitTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("CCD0F1A6-8D1E-4952-BCD4-B801B2F312A0", "IT Numbers And Qty");
			itAndSplitDetailsUserControl.ITNumbersGrid.RemoveFromAvailableColumns(splitDetailsColumns);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				ManageFTZControlsVisibility();
				ShowOrHideSplitDetails();
				ShowOrHideExpressTrackingNumber();
			}
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			ShowOrHideSplitDetails();
			ShowOrHideExpressTrackingNumber();
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideSplitDetails();
			ShowOrHideExpressTrackingNumber();
		}

		void US_F_AdmissionTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideSplitDetails();
		}

		void US_EntryTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideExpressTrackingNumber();
		}

		void US_NonAMSInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideExpressTrackingNumber();
		}

		void ManageFTZControlsVisibility()
		{
			FTZTabPage.TabVisible = JobDeclaration != null && JobDeclaration.IsFTZAdmission;

			if (JobDeclaration != null && JobDeclaration.IsFTZAdmission)
			{
				HouseBillsGrid.SetAvailability(true, Bill.Schema.US_UC_NKCountryOfExport);
				HouseBillsGrid.SetAvailability(true, Bill.Schema.US_US_NKLocationOfGoods);
				HouseBillsGrid.SetAvailability(true, Bill.Schema.US_SchDLoading);
			}
			else
			{
				HouseBillsGrid.RemoveFromAvailableColumns(Bill.Schema.US_UC_NKCountryOfExport);
				HouseBillsGrid.RemoveFromAvailableColumns(Bill.Schema.US_US_NKLocationOfGoods);
				HouseBillsGrid.RemoveFromAvailableColumns(Bill.Schema.US_SchDLoading);
			}
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			HouseBillsGrid.RemoveFromAvailableColumns(CusDecHouseBillSchema.CU_GUIPresentationRecord.Name);
			if (JobDeclaration.IsAir && JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.NonContainerised)
			{
				PackagesGrid.RemoveFromAvailableColumns(Package.Schema.CW_ContainerNoOrEquipmentNo);
			}
			else
			{
				PackagesGrid.SetAvailability(true, Package.Schema.CW_ContainerNoOrEquipmentNo);
				PackagesGrid.ReOrderColumns(new string[] { Package.Schema.CW_HouseBill, Package.Schema.CW_ContainerNoOrEquipmentNo });
			}
			HouseBillsGrid.SetColumnVisible(true, VisibleColumnsForGrid);
		}

		string[] VisibleColumnsForGrid
		{
			get
			{
				if (visibleColumnsForGrid == null)
				{
					List<string> columns = new List<string>();
					columns.Add(CusDecHouseBillSchema.Constants.CU_NoOfPacks);
					columns.Add(CusDecHouseBillSchema.Constants.CU_PackType);
					visibleColumnsForGrid = columns.ToArray();
				}

				return visibleColumnsForGrid;
			}
		}
		string[] visibleColumnsForGrid;
	}
}
