using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public abstract class USCFilterGridModule : ZFilterGridModule
	{
		public override ZBool HasActions => true;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowView => false;

		public override bool AllowDelete => false;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;
	}
}
