using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module.OperationalActions
{
	public class CreatePortTransportJobsOperationalActionMethod : OperationalActionMethod
	{
		public CreatePortTransportJobsOperationalActionMethod()
			: base(new ZGuid("B91940EB-0616-44DE-8901-BDC4B969C959"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("CreatePortTransportJobsOperationalActionMethod|Description", "Create Port Transport Jobs"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CreatePortTransportJobsOperationalActionMethodApplicator(Description, factory);
		}

		public override string Name
		{
			get { return Res.GetString("CreatePortTransportJobsOperationalActionMethod|Name", "Create Port Transport Jobs"); }
		}
	}
}
