using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public partial class EntryHeaderFilterUserControl : ZFilterStripControl
	{
		public EntryHeaderFilterUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override ZFilterStrip NewZFilterStrip() => new EntryHeaderModuleStrip();

		void InitializeGridLayout()
		{
			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				InitializeGridLayoutCore();
				FilteredGrid.ReOrderColumns(ColumnNamesInSortOrder.ToArray());
			}
		}

		protected virtual void InitializeGridLayoutCore()
		{
		}

		public static class Schema
		{
			public const string BranchName = "Declaration+Branch+GB_BranchName";
			public const string ImporterName = "Declaration+ImporterName";
			public const string SupplierName = "Declaration+SupplierName";
			public const string AgentsReference = "Declaration+JE_AgentsReference";
			public const string DateOfArrival = "Declaration+JE_DateOfArrival";
			public const string BranchCode = "Declaration+Branch+GB_Code";
			public const string SupplierCode = "Declaration+Supplier+OH_Code";
			public const string ImporterCode = "Declaration+Importer+OH_Code";
			public const string CustomsAgentCode = "Declaration+CusAgent+GS_Code";
			public const string CustomsAgentName = "Declaration+CusAgent+GS_FullName";
			public const string ControllingAgentCode = "Declaration+ControllingAgent+OH_Code";
			public const string ControllingAgentName = "Declaration+ControllingAgent+OH_FullName";
			public const string ControllingCustomerCode = "Declaration+ControllingCustomer+OH_Code";
			public const string ControllingCustomerName = "Declaration+ControllingCustomer+OH_FullName";
			public const string OwnerReference = "Declaration+JE_OwnerRef";
			public const string MasterBill = "Declaration+JE_MasterBill";
			public const string HouseBill = "Declaration+JE_HouseBill";
			public const string Vessel = "Declaration+JE_VesselName";
			public const string VoyageFlightNo = "Declaration+JE_VoyageFlightNo";
			public const string OriginETD = "Declaration+JE_DateAtOrigin";
			public const string FinalDestinationETA = "Declaration+JE_DateAtFinalDestination";
			public const string Origin = "Declaration+JE_RL_NKOrigin";
			public const string Destination = "Declaration+JE_RL_NKFinalDestination";
			public const string Loading = "Declaration+JE_RL_NKPortOfLoading";
			public const string Discharge = "Declaration+JE_RL_NKPortOfArrival";
			public const string ExportDate = "Declaration+JE_ExportDate";
			public const string ShipmentType = "Declaration+JE_MessageType";
			public const string DeclarantCode = "Declaration+DeclarantAddress+OA_Code";
			public const string DeclarantName = "Declaration+DeclarantAddress+CompanyName";
			public const string TransportMode = "Declaration+JE_TransportMode";
			public const string DeclarationType = "EntryInstruction+CEI_Style";
		}

		protected virtual List<string> ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					List<string> columns = new List<string>();

					columns.Add(CusEntryHeader.Schema.EntryNumber);
					columns.Add(CusEntryHeader.Schema.DeclarationReference);
					columns.Add(CusEntryHeaderSchema.Constants.CH_BGMReference);
					columns.Add(CusEntryHeaderSchema.Constants.CH_EntryStatus);
					columns.Add(CusEntryHeader.Schema.EntryHeaderStatusDescription);
					columns.Add(CusEntryHeader.Schema.CH_EntrySubmittedDate);
					columns.Add(CusEntryHeader.Schema.CH_EntryReleaseDate);
					columns.Add(CusEntryHeaderSchema.Constants.CH_Status);
					columns.Add(CusEntryHeader.Schema.MessageStatusDescription);
					columns.Add(CusEntryHeaderSchema.Constants.CH_MessageType);
					columns.Add(CusEntryHeader.Schema.CH_MessageTypeDescription);
					columns.Add(Schema.BranchName);
					columns.Add(Schema.ImporterName);
					columns.Add(Schema.SupplierName);
					columns.Add(Schema.AgentsReference);
					columns.Add(Schema.DateOfArrival);
					columns.Add(CusEntryHeader.Schema.CH_TotalPaid);
					columns.Add(CusEntryHeader.Schema.CH_WarehouseTransactionStatus);
					columns.Add(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription);

					columnNamesInSortOrder = columns;
				}
				return columnNamesInSortOrder;
			}
		}
		List<string> columnNamesInSortOrder;
	}
}
