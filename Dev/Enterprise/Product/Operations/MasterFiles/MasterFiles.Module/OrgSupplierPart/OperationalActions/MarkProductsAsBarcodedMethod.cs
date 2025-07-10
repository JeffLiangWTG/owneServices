using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class MarkProductsAsBarcodedMethod : OperationalActionMethod
	{
		public MarkProductsAsBarcodedMethod()
			: base(new ZGuid("232A7283-F6F3-4388-A66A-0E13B0336CE3"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new MarkProductsAsBarcodedMethodApplicator(Name);
		}

		public override string Name
		{
			get { return Res.GetString("6378d7dc-df97-4250-90a6-1f212ef7af0a", "Mark products as barcoded."); }
		}

		public override string Description
		{
			get { return Res.GetString("6378d7dc-df97-4250-90a6-1f212ef7af0a", "Mark products as barcoded."); }
		}
	}
}
