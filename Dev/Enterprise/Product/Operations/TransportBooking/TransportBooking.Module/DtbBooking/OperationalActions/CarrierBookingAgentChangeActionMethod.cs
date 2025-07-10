using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module
{
	public class CarrierBookingAgentChangeActionMethod : OperationalActionMethod
	{
		public CarrierBookingAgentChangeActionMethod()
			: base(new ZGuid("983C6C0F-50FD-476D-B4DD-247E1E1133B8"))
		{
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl()
		{
			return new CarrierBookingAgentChangeControl();
		}

		public override string Description
		{
			get { return Res.GetString("CarrierBookingAgentChangeActionMethod|Description", "Mass Assign Carrier Booking Agent"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CarrierBookingAgentChangeActionMethodApplicator(Name, factory);
		}

		public override string Name
		{
			get { return Res.GetString("CarrierBookingAgentChangeActionMethod|Name", "Assign Carrier Booking Agent"); }
		}
	}
}
