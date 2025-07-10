using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module
{
	public partial class EntryHeaderFilterUserControl : Customs.Module.EntryHeaderFilterUserControl
	{
		public EntryHeaderFilterUserControl()
		{
			InitializeComponent();
		}

		public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			var zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();

			zTextBoxColumnStyleInfo1.Caption = "Surety";
			zTextBoxColumnStyleInfo1.ColumnName = "US_SuretyCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.Caption = "Recon Issue";
			zTextBoxColumnStyleInfo2.ColumnName = "Declaration+US_OtherReconIndicator";
			zCheckBoxColumnStyleInfo1.Caption = "NAFTA Recon?";
			zCheckBoxColumnStyleInfo1.ColumnName = "NAFTAReconciliation";
			zDateEditColumnStyleInfo1.Caption = "Import Date";
			zDateEditColumnStyleInfo1.ColumnName = "JE_DateOfArrival";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo3.Caption = "Entry Port";
			zTextBoxColumnStyleInfo3.ColumnName = "US_SchDEntry";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Customs.US.Module.Res.GetData("EntryHeaderFilter|c7bd283c-7026-4708-b667-17f9911b42e8", "Port of Entry");
			zOrganisationFindBoxColumnStyleInfo1.Caption = "IOR";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "IOROrgPK";
			zOrganisationFindBoxColumnStyleInfo1.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zTextBoxColumnStyleInfo4.Caption = "Filer";
			zTextBoxColumnStyleInfo4.ColumnName = "EntryFilerCode";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.Caption = "Entry Type";
			zTextBoxColumnStyleInfo5.ColumnName = "EntryType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo6.Caption = "Pay Type";
			zTextBoxColumnStyleInfo6.ColumnName = "US_PaymentType";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo2.Caption = "Prelim Statement Date";
			zDateEditColumnStyleInfo2.ColumnName = "US_PreliminaryStatementPrintDate";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);

			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
		}

		protected override void InitializeGridLayoutCore()
		{
			FilteredGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryReleaseDate).ColumnName = "JE_EntryAuthorisationDate";
			FilteredGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber).CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("705E467D-1D82-4197-ADBF-A0F20CE4C8C3", "Entry Number");
			FilteredGrid.SetAvailability(false, UnAvailableColumnNames.ToArray());
			UpdateColumnVisibility();
			base.InitializeGridLayoutCore();
		}

		void UpdateColumnVisibility()
		{
			if (((EntryHeaderModule)FilterModule)?.ParentModalFormOwner?.GetType() != typeof(ReconDeclarationForm))
			{
				FilteredGrid.SetAllColumnsVisible(false);
				FilteredGrid.SetColumnVisible(true, ColumnNamesInSortOrder.ToArray());
			}
		}

		protected override List<string> ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columns = new List<string>();

					columns.Add(CusEntryHeader.Schema.EntryFilerCode);
					columns.Add(CusEntryHeader.Schema.EntryNumber);
					columns.Add(CusEntryHeader.Schema.DeclarationReference);
					columns.Add(CusEntryHeader.Schema.CH_EntryStatus);
					columns.Add(CusEntryHeader.Schema.JE_EntryAuthorisationDate);
					columns.Add(CusEntryHeader.Schema.US_SchDEntry);
					columns.Add(Schema.DateOfArrival);
					columns.Add(CusEntryHeader.Schema.EntryType);
					columns.Add(Schema.BranchName);
					columns.Add(Schema.ImporterName);
					columns.Add(Schema.SupplierName);
					columns.Add(CusEntryHeader.Schema.CH_Status);
					columns.Add(CusEntryHeader.Schema.MessageStatusDescription);
					columns.Add(CusEntryHeader.Schema.CH_MessageType);

					columnNamesInSortOrder = columns;
				}
				return columnNamesInSortOrder;
			}
		}
		List<string> columnNamesInSortOrder;

		protected List<string> UnAvailableColumnNames
		{
			get
			{
				if (unAvailableColumnNames == null)
				{
					var columns = new List<string>();

					columns.Add(CusEntryHeader.Schema.CH_BGMReference);
					columns.Add(CusEntryHeader.Schema.CH_WarehouseTransactionStatus);
					columns.Add(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription);
					columns.Add(Schema.ExportDate);
					columns.Add(Schema.CustomsAgentCode);
					columns.Add(Schema.CustomsAgentName);
					columns.Add(Schema.ControllingAgentCode);
					columns.Add(Schema.ControllingAgentName);
					columns.Add(Schema.ControllingCustomerCode);
					columns.Add(Schema.ControllingCustomerName);
					columns.Add(Schema.OwnerReference);
					columns.Add(Schema.MasterBill);
					columns.Add(Schema.HouseBill);
					columns.Add(Schema.Vessel);
					columns.Add(Schema.VoyageFlightNo);
					columns.Add(Schema.OriginETD);
					columns.Add(Schema.FinalDestinationETA);
					columns.Add(Schema.Origin);
					columns.Add(Schema.Destination);
					columns.Add(Schema.Loading);
					columns.Add(Schema.Discharge);
					columns.Add(Schema.ShipmentType);
					columns.Add(Schema.DeclarantCode);
					columns.Add(Schema.DeclarantName);
					columns.Add(Schema.TransportMode);
					columns.Add(Schema.DeclarationType);

					unAvailableColumnNames = columns;
				}
				return unAvailableColumnNames;
			}
		}
		List<string> unAvailableColumnNames;
	}
}
