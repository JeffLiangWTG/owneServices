using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public abstract class NumberModule : ZPopupModule
	{
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
