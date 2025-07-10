using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestFilterStrip.EqualModuleTextFilter))]
	sealed class eManifestFilterStripEqualModuleTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new eManifestFilterStrip.EqualModuleTextFilter(
				eManifestFilterStrip.Descriptions.TransitDirectionFilterId,
				eManifestFilterStrip.Descriptions.TransitDirectionMultilingualDescription,
				FilterCategories.StatusAndFlags,
				CusInBondHeaderSchema.BH_TransitDirection,
				() => new ArrayList(),
				false);
		}
	}
}
