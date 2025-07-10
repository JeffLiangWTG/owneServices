using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CommonPickupDeliveryConfirmDetailsGridColumnProvider))]
	sealed class CommonPickupDeliveryConfirmDetailsGridColumnProviderTest : GridColumnProviderTest
	{
		protected override bool SupportsOldLayoutFix
		{
			get
			{
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			ZBindToChecker.CheckBindTo((ZDateTime)((CommonPickupDeliveryConfirm)null).EU_PickupDeliveryTime);
			AddDefaultsColumn(new ZDateTimeColumn("Dispatched At", CommonPickupDeliveryConfirm.Schema.EU_PickupDeliveryTime) { ColumnKey = WebTracker.Grids.DeliveryInformation.Dispatched });

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).EU_DriversName);
			AddDefaultsColumn(new ZTextEditColumn("Driver", CommonPickupDeliveryConfirm.Schema.EU_DriversName) { ColumnKey = WebTracker.Grids.DeliveryInformation.Driver });

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).EU_TransportCoName);
			AddDefaultsColumn(new ZTextEditColumn("Transport Co Name", CommonPickupDeliveryConfirm.Schema.EU_TransportCoName) { ColumnKey = WebTracker.Grids.DeliveryInformation.TransportCompanyName });

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).EU_VehicleRegistration);
			AddDefaultsColumn(new ZTextEditColumn("Vehicle Registration", CommonPickupDeliveryConfirm.Schema.EU_VehicleRegistration) { ColumnKey = WebTracker.Grids.DeliveryInformation.VehicleRegistration });

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).FullGatePass);
			AddDefaultsColumn(new ZTextEditColumn("Gate Pass ID", CommonPickupDeliveryConfirm.Schema.FullGatePass) { ColumnKey = WebTracker.Grids.DeliveryInformation.GatePassID });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CommonPickupDeliveryConfirmDetailsGridColumnProvider();
		}
	}
}
