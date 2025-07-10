using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class ManifestController : ASYCUDA.Module.ASYCUDAManifestController
	{
		public ManifestController()
		{
			CountryCode = Core.Constants.CountryCodes.Singapore;
			CreateVOC = false;
		}

		public override ControllerID ID => ControllerIDs.Customs.ASYCUDA.SGAccess.Manifest;
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest;
		public override Type TypeOfTopLevelBusinessObject => typeof(AsycudaManifestHeader);

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			Provider = ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, Core.Constants.CountryCodes.Singapore).First();
			return base.GetNewBusinessEntityInLocalFactory();
		}
	}
}
