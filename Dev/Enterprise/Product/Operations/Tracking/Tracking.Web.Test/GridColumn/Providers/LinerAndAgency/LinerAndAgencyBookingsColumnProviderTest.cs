using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(LinerAndAgencyBookingsColumnProvider))]
	sealed class LinerAndAgencyBookingsColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.LinerAndAgencyBookings.JobStatus],
				TestProvider[WebTracker.Grids.LinerAndAgencyBookings.Packs],
				TestProvider[WebTracker.Grids.LinerAndAgencyBookings.Weight],
				TestProvider[WebTracker.Grids.LinerAndAgencyBookings.Status],
				TestProvider[WebTracker.Grids.LinerAndAgencyBookings.ShipmentNumber]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Shipment #", TrackingLinerAndAgencyBooking.Schema.JS_UniqueConsignRef)
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ShipmentNumber,
				DataNavigateUrlFormatString = TrackingConstants.RelativePath.LinerAndAgencyBookingDetailsPage + "?Ref={0}", // Partial URL
				DataNavigateUrlFields = new string[] { "PK" }
			});
			AddDefaultsColumn(new ZTextEditColumn("Ocean Bill", TrackingLinerAndAgencyBooking.Schema.JS_HouseBill) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.OceanBill });
			AddColumn(new ZTextEditColumn("Consignor", "ConsignorDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Consignor });
			AddColumn(new ZTextEditColumn("Consignee", "ConsigneeDocumentaryAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Consignee });
			AddDefaultsColumn(new ZDropDownListColumn("Status", TrackingLinerAndAgencyBooking.Schema.JS_ShipmentStatus) { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Status });
			AddDefaultsColumn(new ZTextEditColumn("Vessel", "Sailing+JX_JV_NKVessel") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Vessel });
			AddDefaultsColumn(new ZTextEditColumn("Voyage No.", "Sailing+JX_JV_VoyageFlight") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Voyage });

			AddColumn(new ZCodeFindBoxColumn("Origin", TrackingLinerAndAgencyBooking.Schema.JS_RL_NKOrigin, "Lookups.Origins", typeof(TrackingLinerAndAgencyBooking))
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Origin,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZCodeFindBoxColumn("Load", TrackingLinerAndAgencyBooking.Schema.JS_NKLoadPort, "Lookups.Origins", typeof(TrackingLinerAndAgencyBooking))
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Load,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZCodeFindBoxColumn("Disch.", TrackingLinerAndAgencyBooking.Schema.JS_NKDischargePort, "Lookups.Destinations", typeof(TrackingLinerAndAgencyBooking))
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.DischargePort,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZCodeFindBoxColumn("Dest.", TrackingLinerAndAgencyBooking.Schema.JS_RL_NKDestination, "Lookups.Destinations", typeof(TrackingLinerAndAgencyBooking))
			{
				ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Destination,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZTextEditColumn("Cargo Desc.", TrackingLinerAndAgencyBooking.Schema.JS_GoodsDescription) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.CargoDescription });

			ZCalcEditColumn packsColumn = new ZCalcEditColumn("Packs", TrackingLinerAndAgencyBooking.Schema.JS_OuterPacks) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Packs };
			ZTextEditColumn packTypeColumn = new ZTextEditColumn("Type", TrackingLinerAndAgencyBooking.Schema.JS_F3_NKPackType) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Type };
			AddColumn(new ZGroupColumn("Packs", packsColumn, packTypeColumn) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Packs });

			AddColumn(new ZTextEditColumn("Weight", TrackingLinerAndAgencyBooking.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Weight });
			AddColumn(new ZTextEditColumn("Volume", TrackingLinerAndAgencyBooking.Schema.VolumeWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Volume });
			AddColumn(new ZTextEditColumn("Cargo Type", TrackingLinerAndAgencyBooking.Schema.JS_PackingMode) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.CargoType });
			AddColumn(new ZDateTimeColumn("ETD", "Sailing+JX_JA_E_DEP") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ETD });
			AddColumn(new ZDateTimeColumn("ETA", "Sailing+JX_JB_E_ARV") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ETA });
			AddColumn(new ZTextEditColumn("Cnr. Contact", TrackingLinerAndAgencyBooking.Schema.ConsignorContact) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ConsignorContact });
			AddColumn(new ZTextEditColumn("Shipper's Ref#", TrackingLinerAndAgencyBooking.Schema.JS_BookingReference) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.ShippersRef });
			AddColumn(new ZDateTimeColumn("Booked", TrackingLinerAndAgencyBooking.Schema.JS_A_BKD) { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.Booked });
			AddColumn(new ZDateTimeColumn("CFS Recv.", "Sailing+JX_DepotReceivalCommences") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.LCLReceivalCommences });
			AddColumn(new ZDateTimeColumn("CFS Cut", "Sailing+JX_DepotCutOff") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.LCLCutOff });
			AddColumn(new ZDateTimeColumn("CTO Recv.", "Sailing+JX_JA_CTOReceivalCommences") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.FCLReceivalCommences });
			AddColumn(new ZDateTimeColumn("CTO Cut", "Sailing+JX_JA_CTOCutOff") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.FCLCutOff });
			AddColumn(new ZTextEditColumn("Job Status", "Job+JH_Status") { ColumnKey = WebTracker.Grids.LinerAndAgencyBookings.JobStatus });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.LinerAndAgencyBookings.Packs
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new LinerAndAgencyBookingsColumnProvider();
		}

		#endregion
	}
}
