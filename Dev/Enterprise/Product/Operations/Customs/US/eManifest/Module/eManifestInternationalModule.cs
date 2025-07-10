using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.eManifest.Module
{
	public class eManifestInternationalModule : eManifestModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.US.eManifestIntl; }
		}
	}
}
