using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(LinerAndAgencyBOLColumnProvider))]
	sealed class LinerAndAgencyBOLColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.LinerAndAgencyBillOfLadings.PaymentTerms],
				TestProvider[WebTracker.Grids.LinerAndAgencyBillOfLadings.Weight],
				TestProvider[WebTracker.Grids.LinerAndAgencyBillOfLadings.Volume],
				TestProvider[WebTracker.Grids.LinerAndAgencyBillOfLadings.ShippersRef],
				TestProvider[WebTracker.Grids.LinerAndAgencyBillOfLadings.ShipmentNumber]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Shipment #", TrackingBillOfLading.Schema.JS_UniqueConsignRef)
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ShipmentNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.LinerAndAgencyBillOfLadingDetailsPage + "?Ref={0}", // Partial URL
				DataNavigateUrlFields = new string[] { "PK" }
			});
			AddDefaultsColumn(new ZTextEditColumn("Ocean Bill", TrackingBillOfLading.Schema.JS_HouseBill) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.OceanBill });
			AddColumn(new ZTextEditColumn("Booking Party", "BookingPartyDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.BookingParty });
			AddDefaultsColumn(new ZTextEditColumn(FreightDataRegistry.Instance.ConsignorShipperTerminology.Value, "ConsignorDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Shipper });
			AddDefaultsColumn(new ZTextEditColumn("Consignee", "ConsigneeDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Consignee });
			AddDefaultsColumn(new ZDropDownListColumn("Status", TrackingBillOfLading.Schema.JS_ShipmentStatus) { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Status });
			AddDefaultsColumn(new ZTextEditColumn("Vessel", "Sailing+JX_JV_NKVessel") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Vessel });
			AddDefaultsColumn(new ZTextEditColumn("Voyage No.", "Sailing+JX_JV_VoyageFlight") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Voyage });

			AddColumn(new ZCodeFindBoxColumn("Origin", TrackingBillOfLading.Schema.JS_RL_NKOrigin, "Lookups.Origins", typeof(TrackingBillOfLading))
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Origin,

				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZCodeFindBoxColumn("Load", TrackingBillOfLading.Schema.JS_NKLoadPort, "Lookups.Origins", typeof(TrackingBillOfLading))
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Load,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZCodeFindBoxColumn("Disch.", TrackingBillOfLading.Schema.JS_NKDischargePort, "Lookups.Destinations", typeof(TrackingBillOfLading))
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.DischargePort,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddColumn(new ZCodeFindBoxColumn("Dest.", TrackingBillOfLading.Schema.JS_RL_NKDestination, "Lookups.Destinations", typeof(TrackingBillOfLading))
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Destination,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});
			AddDefaultsColumn(new ZTextEditColumn("Cargo Desc.", TrackingBillOfLading.Schema.JS_GoodsDescription) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.CargoDescription });

			ZCalcEditColumn packsColumn = new ZCalcEditColumn("Packs", TrackingBillOfLading.Schema.JS_OuterPacks);
			ZTextEditColumn packTypeColumn = new ZTextEditColumn("Type", TrackingBillOfLading.Schema.JS_F3_NKPackType);
			AddColumn(new ZGroupColumn("Packs", packsColumn, packTypeColumn) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Packs });

			AddColumn(new ZTextEditColumn("Weight", TrackingBillOfLading.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Weight });
			AddColumn(new ZTextEditColumn("Volume", TrackingBillOfLading.Schema.VolumeWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Volume });
			AddColumn(new ZTextEditColumn("Cargo Type", TrackingBillOfLading.Schema.JS_PackingMode) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.CargoType });
			AddColumn(new ZDateTimeColumn("ETD", TrackingBillOfLading.Schema.JS_Calc_CurrentETD) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ETD });
			AddColumn(new ZDateTimeColumn("ETA", TrackingBillOfLading.Schema.JS_Calc_CurrentETA) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ETA });
			AddColumn(new ZTextEditColumn("Cnr. Contact", TrackingBillOfLading.Schema.ConsignorContact) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ConsignorContact });
			AddColumn(new ZTextEditColumn("Shipper's Ref#", TrackingBillOfLading.Schema.JS_BookingReference) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.ShippersRef });
			AddColumn(new ZTextEditColumn("Interim #", TrackingBillOfLading.Schema.JS_InterimReceipt) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.InterimNumber });
			AddColumn(new ZDateTimeColumn("Booked", TrackingBillOfLading.Schema.JS_A_BKD) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.Booked });
			AddColumn(new ZDateTimeColumn("CFS Recv. Start", "Sailing+JX_DepotReceivalCommences") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.LCLReceivalCommences });
			AddColumn(new ZDateTimeColumn("CFS Cut Off", "Sailing+JX_DepotCutOff") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.LCLCutOff });
			AddColumn(new ZDateTimeColumn("CTO Recv. Start", "Sailing+JX_JA_CTOReceivalCommences") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.FCLReceivalCommences });
			AddColumn(new ZDateTimeColumn("CTO Cut Off", "Sailing+JX_JA_CTOCutOff") { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.FCLCutOff });
			AddColumn(new ZDropDownListColumn("Payment Term", TrackingBillOfLading.Schema.JS_INCO) { ColumnKey = WebTracker.Grids.LinerAndAgencyBillOfLadings.PaymentTerms });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.LinerAndAgencyBillOfLadings.Packs
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new LinerAndAgencyBOLColumnProvider();
		}

		#endregion
	}
}
