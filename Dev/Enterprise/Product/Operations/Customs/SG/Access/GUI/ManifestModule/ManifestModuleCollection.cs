using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class ManifestModuleCollection : ASYCUDA.Module.AsycudaManifestModuleCollection
	{
		public ManifestModuleCollection(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.Singapore)
		{
		}
	}
}
