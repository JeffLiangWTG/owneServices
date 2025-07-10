using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public interface ISailingChooserParent
	{
		BusinessObjectFactory Factory { get; }

		ZDateTime BookedDate { get; }
		event EventHandler BookedDateChanged;

		ZDateTime ETD { get; }

		ZDateTime RequestedByDate { get; }
		event EventHandler RequestedByDateChanged;

		ZString AWBServiceLevel { get; set; }
		event EventHandler AWBServiceLevelChanged;

		ZBool IsDirect { get; set; }
		event EventHandler IsDirectChanged;

		ZBool IsDirectEnabled { get; }

		ZBool IsNeutralMaster { get; set; }
		event EventHandler IsNeutralMasterChanged;

		ZString MawbNumber { get; set; }
		event EventHandler MawbNumberChanged;

		ZGuid Carrier { get; set; }
		event EventHandler CarrierChanged;

		ZString TransportMode { get; }
		event EventHandler TransportModeChanged;

		ZString ContainerMode { get; }
		event EventHandler ContainerModeChanged;

		ZString ReservedMasterBill { get; set; }
		ZString Origin { get; set; }
		ZString Destination { get; set; }

		ZString LoadPort { get; }
		event EventHandler LoadPortChanged;

		ZString DischargePort { get; }
		event EventHandler DischargePortChanged;

		ZGuid SailingJX { get; set; }
		event EventHandler SailingJXChanged;

		ScheduleChooser ScheduleChooser { get; }

		ForwardingShipment Booking { get; }

		void SetIsNeutralMasterReadOnly(bool value);
	}
}
