using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public abstract class GteCommonController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GateManagement;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GateManagement;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GateManagement;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GateManagement;
	}
}
