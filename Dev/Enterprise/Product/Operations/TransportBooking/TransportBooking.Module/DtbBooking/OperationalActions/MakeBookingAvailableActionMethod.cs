using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module
{
	public class MakeBookingAvailableActionMethod : OperationalActionMethod
	{
		public MakeBookingAvailableActionMethod()
			: base(new ZGuid("c0380bda-b02a-4c1a-b42f-9d7058ee5b59"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new MakeBookingAvailableChangeActionMethodApplicator();
		}

		public override string Name
		{
			get { return Res.GetString("05d766b6-2a24-4442-af70-9a112129d8d3", "Make Available"); }
		}

		public override string Description
		{
			get { return Res.GetString("05d766b6-2a24-4442-af70-9a112129d8d3", "Make Available"); }
		}
	}
}
