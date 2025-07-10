using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public partial class TWBillItemsUserControl : ZUserControl, IAdditionalTabPage
	{
		public TWBillItemsUserControl()
		{
			InitializeComponent();
			if (BillItemsGrid.GetColumnStyle(nameof(AsycudaPackedItem.API_FormattedTariff)) is Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo)
			{
				tariffColumnStyleInfo.GetCountryCode = () => Core.Constants.CountryCodes.Taiwan;
				tariffColumnStyleInfo.GetDataGrouping = () => Core.Constants.CountryCodes.Taiwan;
				tariffColumnStyleInfo.GetTariffType = () => Universal.Constants.TariffTypes.HarmonizedSystem;
				tariffColumnStyleInfo.GetEffectiveDate = () => ZDateTime.Today;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			ReOrderBillItemsGridColumns();
			base.OnLoad(e);
		}

		string[] BillItemsGridColumnNamesInSortOrder => billItemsGridColumnNamesInSortOrder ??= new []
		{
			AsycudaPackedItem.Schema.API_LineNo,
			AsycudaPackedItem.Schema.API_CustomsQty,
			AsycudaPackedItem.Schema.API_CustomsUQ,
			AsycudaPackedItem.Schema.API_UnitPrice,
			AsycudaPackedItem.Schema.API_GoodsValue,
			AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency,
			AsycudaPackedItem.Schema.API_CustomsValue,
			AsycudaPackedItem.Schema.API_NetWeight,
			AsycudaPackedItem.Schema.API_NetWeightUQ,
			AsycudaPackedItem.Schema.API_GoodsDescription,
			AsycudaPackedItem.Schema.API_FormattedTariff,
			AsycudaPackedItem.Schema.API_CustomsQty2,
			AsycudaPackedItem.Schema.API_CustomsUQ2,
			nameof(AsycudaPackedItem.ModeOfStatisticsOrDutyTreatment),
			AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin,
			AsycudaPackedItem.Schema.API_Preference,
			nameof(AsycudaPackedItem.FormattedAdValoremDutyRate),
			nameof(AsycudaPackedItem.FormattedSpecificDutyRate),
			AsycudaPackedItem.Schema.API_CustomsBuyerPartNo,
			AsycudaPackedItem.Schema.API_CustomsSupplierPartNo,
			AsycudaPackedItem.Schema.API_PreviousEntryNo,
			AsycudaPackedItem.Schema.API_PreviousEntryLineNo,
			AsycudaPackedItem.Schema.API_Brand,
			AsycudaPackedItem.Schema.API_Model,
			AsycudaPackedItem.Schema.API_Remarks,
		};
		string[] billItemsGridColumnNamesInSortOrder;

		#region IAdditionalTabPage
		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("2C2149F7-F47C-4F10-86E2-54307A4C1C14", "Items");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 0;
		#endregion

		bool IsSea => DataSource is AsycudaManifestHeader header && header.IsSea;

		bool IsImport => DataSource is AsycudaManifestHeader header && header.IsImport;

		bool IsExport => DataSource is AsycudaManifestHeader header && header.IsExport;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is AsycudaManifestHeader header)
			{
				header.AMA_NatureInfo.ValueChanged -= AMA_NatureInfo_ValueChanged;
				header.AMA_NatureInfo.ValueChanged += AMA_NatureInfo_ValueChanged;
				header.AMA_TransportModeInfo.ValueChanged -= AMA_TransportModeInfo_ValueChanged;
				header.AMA_TransportModeInfo.ValueChanged += AMA_TransportModeInfo_ValueChanged;
				AMA_NatureInfo_ValueChanged(null, null);
				AMA_TransportModeInfo_ValueChanged(null, null);
			}
		}

		void AMA_NatureInfo_ValueChanged(object sender, EventArgs e)
		{
			var isImport = IsImport;
			TaxesGroupBox.Visible = isImport;
			BillItemsGrid.SetColumnCaption(nameof(AsycudaPackedItem.API_CustomsValue), isImport ? Res.GetString("5A369198-D0B8-404F-AC13-3D784FDF9EEA", "Customs Value") : Res.GetString("2861FAD1-1558-4B6D-A8C2-BA39DAC7AE2E", "FOB Value"));
			BillItemsGrid.SetColumnCaption(nameof(AsycudaPackedItem.ModeOfStatisticsOrDutyTreatment), isImport ? Res.GetString("B612B8EF-45A5-432D-A00B-79EEBDA768ED", "Duty Treatment") : IsExport ? Res.GetString("A447A1EC-1DCE-4C44-B8F2-4B0300B7FC77", "Mode of Statistics") : Res.GetString("0474E36D-0378-49F1-98C4-BF81466C055E", "Procedure"));
			BillItemsGrid.SetAvailability(isImport, new string[] { nameof(AsycudaPackedItem.API_Preference), nameof(AsycudaPackedItem.FormattedAdValoremDutyRate), nameof(AsycudaPackedItem.FormattedSpecificDutyRate) });
			SetBillItemsGridColumnAvailability();
			ReOrderBillItemsGridColumns();
		}

		void AMA_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetBillItemsGridColumnAvailability();
			ReOrderBillItemsGridColumns();
		}

		void SetBillItemsGridColumnAvailability()
		{
			BillItemsGrid.SetAvailability(IsExport && IsSea, new string[] { nameof(AsycudaPackedItem.API_CustomsBuyerPartNo), nameof(AsycudaPackedItem.API_CustomsSupplierPartNo), nameof(AsycudaPackedItem.API_PreviousEntryNo), nameof(AsycudaPackedItem.API_PreviousEntryLineNo) });
		}

		void ReOrderBillItemsGridColumns()
		{
			using (BillItemsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				BillItemsGrid.ReOrderColumns(BillItemsGridColumnNamesInSortOrder);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			if (DataSource is AsycudaManifestHeader header)
			{
				header.AMA_NatureInfo.ValueChanged -= AMA_NatureInfo_ValueChanged;
				header.AMA_TransportModeInfo.ValueChanged -= AMA_TransportModeInfo_ValueChanged;
			}
			base.Dispose(disposing);
		}
	}
}
