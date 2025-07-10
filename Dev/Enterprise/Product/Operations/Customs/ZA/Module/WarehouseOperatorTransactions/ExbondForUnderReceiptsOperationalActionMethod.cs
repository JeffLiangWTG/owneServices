using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Module
{
	internal class ExbondForUnderReceiptsOperationalActionMethod : OperationalActionMethod
	{
		public ExbondForUnderReceiptsOperationalActionMethod() : base(new ZGuid("3FDA9B20-F813-487D-8CD1-E625AA6A046E"))
		{
		}

		public override string Name => "Generate Ex-bond for Under Receipts";

		public override string Description => "Generate Ex-bond for Under Receipts";

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ExbondForUnderReceiptsApplicator(factory);
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl()
		{
			return new ExportOperationalActionUserControl();
		}
	}
}
