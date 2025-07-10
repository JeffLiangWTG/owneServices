using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module.OperationalActions
{
	public class CreateLandTransportConsignmentsOperationalActionMethod : OperationalActionMethod
	{
		public CreateLandTransportConsignmentsOperationalActionMethod()
			: base(new ZGuid("CB44D76B-0689-494C-A1A3-3B20C0CB3000"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("CreateLandTransportConsignmentsOperationalActionMethod|Description", "Create Land Transport Consignments"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CreateLandTransportConsignmentsOperationalActionMethodApplicator(Description, factory);
		}

		public override string Name
		{
			get { return Res.GetString("CreateLandTransportConsignmentsOperationalActionMethod|Name", "Create Land Transport Consignments"); }
		}
	}
}
