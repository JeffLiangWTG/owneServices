using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class UnmarkProductsAsBarcodedMethod : OperationalActionMethod
	{
		public UnmarkProductsAsBarcodedMethod()
			: base(new ZGuid("1ECD908E-AABD-4DBA-93ED-9FB2DADC09D2"))
		{
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new UnmarkProductsAsBarcodedMethodApplicator(Name);
		}

		public override string Name
		{
			get { return Res.GetString("8779f5e9-c583-4169-8480-dd30d72a6320", "Un-mark products as barcoded."); }
		}

		public override string Description
		{
			get { return Res.GetString("8779f5e9-c583-4169-8480-dd30d72a6320", "Un-mark products as barcoded."); }
		}
	}
}
