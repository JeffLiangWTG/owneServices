using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Environment.Module
{
	public class AssignProductToPickFaceActionMethod : OperationalActionMethod
	{
		public AssignProductToPickFaceActionMethod()
			: base(new ZGuid("9a28e38e-9794-42ab-b96a-0557625023f7"))
		{
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl()
		{
			return new AssignProductToPickFaceControl();
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new AssignProductToPickFaceActionMethodApplicator(factory);
		}

		public override bool IsRunAgainDisabled
		{
			get { return true; }
		}

		public override string Name
		{
			get { return Res.GetString("a9a6721c-ca2a-4860-8b46-fcff784f007c", "Assign Product to Pick Face"); }
		}

		public override string Description
		{
			get { return Res.GetString("a9a6721c-ca2a-4860-8b46-fcff784f007c", "Assign Product to Pick Face"); }
		}
	}
}
