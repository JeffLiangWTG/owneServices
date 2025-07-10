using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Agency.Module
{
	public class UpdateDetentionDaysActionMethod : OperationalActionMethod
	{
		public UpdateDetentionDaysActionMethod()
			: base(new ZGuid("0fea9796-177a-4525-9efb-3f037999f3a3")) { }

		public override string Name
		{
			get { return Res.GetString("0fea9796-177a-4525-9efb-3f037999f3a3", "Update Detention Days"); }
		}

		public override string Description
		{
			get { return Res.GetString("c443c168-e0a2-4345-aa63-906434a52b21", "Re-calculates the number of detention days for all selected movements."); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UpdateDetentionDaysApplicator();
		}
	}
}


