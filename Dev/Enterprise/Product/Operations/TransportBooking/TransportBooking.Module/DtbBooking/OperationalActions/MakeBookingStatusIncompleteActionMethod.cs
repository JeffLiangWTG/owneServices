using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module
{
	public class MakeBookingStatusIncompleteActionMethod : OperationalActionMethod
	{
		public MakeBookingStatusIncompleteActionMethod()
			: base(new ZGuid("E61247B8-0F6F-4E48-BD74-13F3CC8B3108"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new MakeBookingStatusIncompleteActionMethodApplicator();
		}

		public override string Name
		{
			get { return Res.GetString("C4C445A8-EC51-433F-99AC-E3543903C1BD", "Change Status From Held to Incomplete"); }
		}

		public override string Description
		{
			get { return Res.GetString("C4C445A8-EC51-433F-99AC-E3543903C1BD", "Change Status From Held to Incomplete"); }
		}
	}
}
