using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TR.NCTS.Module
{
	public partial class NctsMovementFilterControl : EU.NCTS.Module.NctsMovementFilterControl
	{
		public NctsMovementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			AddColumns();
			ReOrderColumns();
		}

		void AddColumns()
		{
			var columnStyles = grid.ColumnStyles;

			columnStyles.Add(CreateZTextBoxColumn(nameof(NctsHeader.LrnRegistrationNumber), Res.GetData("5C03B00C-89AB-41FA-B431-873C3119B54E", "LRN")));
			columnStyles.Add(CreateZDateEditColumn(nameof(NctsHeader.LrnRegistrationDate), Res.GetData("72F41AAB-4E70-447A-902F-B3D4AADDFAB9", "LRN Date")));

			ZArchitecture.ZTextBoxColumnStyleInfo CreateZTextBoxColumn(string columnName, ResourceStringData caption)
			{
				return new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = columnName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					CaptionResourceString = caption
				};
			}

			ZArchitecture.ZDateEditColumnStyleInfo CreateZDateEditColumn(string columnName, ResourceStringData caption)
			{
				return new ZArchitecture.ZDateEditColumnStyleInfo
				{
					ColumnName = columnName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					CaptionResourceString = caption
				};
			}
		}

		void ReOrderColumns()
		{
			Grid.ReOrderColumns(listColumns);
		}
		readonly string[] listColumns =
		{
			nameof(NctsHeader.LrnRegistrationNumber),
			nameof(NctsHeader.LrnRegistrationDate),
			nameof(NctsHeader.JobReferenceNumber),
			nameof(NctsHeader.MovementReferenceNumber),
			nameof(NctsHeader.LocalReferenceNumberForDisplay),
			nameof(NctsHeader.BH_HeaderType),
			nameof(NctsHeader.MovementHeader.BM_CustomsStatus),
			nameof(NctsHeader.MovementHeader.CustomsStatusDescription),
			nameof(NctsHeader.ArrivalMovementHeader.BM_CustomsStatus),
			nameof(NctsHeader.ArrivalMovementHeader.ArrivalStatusDescription),
			nameof(NctsHeader.CommonMovementHeader.BM_Phase),
			nameof(NctsHeader.CommonMovementHeader.PhaseStatusDescription),
			nameof(NctsHeader.DepartureCustomsOfficeCode),
			nameof(NctsHeader.DestinationCustomsOfficeCodeForDeparture),
			nameof(NctsHeader.TotalNumberOfItems),
			nameof(NctsHeader.TotalNumberOfPackages),
			nameof(NctsHeader.TotalGrossMassInKilograms),
			nameof(NctsHeader.CommonMovementHeader.BM_EntryDate),
			nameof(NctsHeader.ArrivalMovementHeader.BM_ArrivalDate),
			nameof(NctsHeader.MovementReferenceIssueDate),
			nameof(NctsHeader.MovementHeader.IsSimplifiedNctsProcedure),
			nameof(NctsHeader.BH_FTZMove),
			nameof(NctsHeader.CountryOfDispatch),
			nameof(NctsHeader.MovementHeader.BM_RL_NKDestinationPort),
			nameof(NctsHeader.MovementHeader.BM_InBondEntryType),
			nameof(NctsHeader.MovementHeader.BM_LocationOfGoodsCode),
			nameof(NctsHeader.MovementHeader.BM_LocationOfGoods),
			nameof(NctsHeader.MovementHeader.BM_TransportAtDeparture),
			nameof(NctsHeader.MovementHeader.PlaceOfLoading),
			nameof(NctsHeader.PlaceOfUnloading),
			nameof(NctsHeader.MovementHeader.BM_CustomsSubPlace),
			nameof(NctsHeader.MovementHeader.BM_InlandTransportMode),
			nameof(NctsHeader.MovementHeader.BM_ExportTransportMode),
			nameof(NctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry),
			nameof(NctsHeader.MovementHeader.BM_TOLCarrierID),
			nameof(NctsHeader.MovementHeader.BM_TOLCarrierCode),
			nameof(NctsHeader.MovementHeader.IsContainerised),
			nameof(NctsHeader.DeclarationPlace),
			nameof(NctsHeader.MovementHeader.BM_BTAIndicator),
			nameof(NctsHeader.MovementHeader.BM_MethodOfPayment),
			nameof(NctsHeader.MovementHeader.BM_AdditionalText),
			nameof(NctsHeader.MovementHeader.BM_ConveyanceNumber),
			nameof(NctsHeader.MovementHeader.BM_ExportDate),
			nameof(NctsHeader.MovementHeader.BM_GS_NKCusAgent),
			nameof(NctsHeader.EffectiveMessageStatus),
			nameof(NctsHeader.DestinationCustomsOfficeCodeForArrival),
			nameof(NctsHeader.Job.JH_Status),
			nameof(NctsHeader.BH_ApplicationCode),
			nameof(NctsHeader.Job.JH_HoldReason),
		};
	}
}
