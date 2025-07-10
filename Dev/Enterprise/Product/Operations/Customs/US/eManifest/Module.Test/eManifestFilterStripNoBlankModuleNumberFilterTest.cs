using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestFilterStrip.NoBlankModuleNumberFilter))]
	sealed class eManifestFilterStripNoBlankModuleNumberFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new eManifestFilterStrip.NoBlankModuleNumberFilter(
				eManifestFilterStrip.Descriptions.JobReferenceFilterId,
				eManifestFilterStrip.Descriptions.JobReferenceMultilingualDescription,
				CusInBondHeaderSchema.BH_JobReference);
		}
	}
}
