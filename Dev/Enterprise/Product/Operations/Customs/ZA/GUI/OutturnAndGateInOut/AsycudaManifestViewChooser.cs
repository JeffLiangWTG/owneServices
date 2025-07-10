using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.GUI
{
	class AsycudaManifestViewChooser : ZRecordChooser<AsycudaManifestHeader>
	{
		public AsycudaManifestViewChooser(IBusinessObjectCollection collection)
			: base(ModuleIDs.Customs.ASYCUDA.Manifest, collection)
		{
		}

		protected override ZRecordChooserModuleDecisionProvider GetDefaultModuleDecisionProvider()
		{
			return new AsycudaManifestViewChooserModuleDecisionProvider(this);
		}

		#region AsycudaManifestViewChooserModuleDecisionProvider

		public class AsycudaManifestViewChooserModuleDecisionProvider : ZRecordChooserModuleDecisionProvider
		{
			public AsycudaManifestViewChooserModuleDecisionProvider(ZRecordChooser<AsycudaManifestHeader> recordChooser)
				: base(recordChooser)
			{
			}

			public override bool AllowMultiSelect => false;
		}

		#endregion
	}
}
