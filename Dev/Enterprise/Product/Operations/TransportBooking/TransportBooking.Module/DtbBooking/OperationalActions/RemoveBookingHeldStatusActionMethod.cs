using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module
{
	public class RemoveBookingHeldStatusActionMethod : OperationalActionMethod
	{
		public RemoveBookingHeldStatusActionMethod() : base(new ZGuid("5ad77f35-3a4d-4da9-b38a-fdf4b5a6eda2"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new RemoveBookingHeldStatusActionMethodApplicator();
		}

		public override string Name
		{
			get { return Res.GetString("c7550376-a37f-4e67-83ff-605ad177d92b", "Remove Held Status"); }
		}

		public override string Description
		{
			get { return Res.GetString("c7550376-a37f-4e67-83ff-605ad177d92b", "Remove Held Status"); }
		}
	}
}
