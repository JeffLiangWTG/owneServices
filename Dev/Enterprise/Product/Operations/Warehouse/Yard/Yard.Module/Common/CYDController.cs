using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public abstract class CYDController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ContainerYard; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ContainerYard; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ContainerYard; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ContainerYard; }
		}

		#endregion
	}
}
