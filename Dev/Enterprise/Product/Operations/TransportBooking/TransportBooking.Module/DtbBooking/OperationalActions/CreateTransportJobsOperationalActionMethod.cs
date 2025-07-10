using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module.OperationalActions
{
	public class CreateTransportJobsOperationalActionMethod : OperationalActionMethod
	{
		public CreateTransportJobsOperationalActionMethod()
			: base(new ZGuid("A21297B6-EFE9-4656-8A5D-131E56810CAE"))
		{
		}

		public override string Description
		{
			get { return Res.GetString("CreateTransportJobsOperationalActionMethod|Description", "Create Default Transport Jobs"); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new CreateTransportJobsOperationalActionMethodApplicator(Description, factory);
		}

		public override string Name
		{
			get { return Res.GetString("CreateTransportJobsOperationalActionMethod|Name", "Create Default Transport Jobs"); }
		}
	}
}
