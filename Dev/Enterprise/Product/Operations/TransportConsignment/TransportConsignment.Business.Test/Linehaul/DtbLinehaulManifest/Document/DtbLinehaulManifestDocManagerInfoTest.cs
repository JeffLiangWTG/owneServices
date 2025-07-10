using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbLinehaulManifestDocManagerInfo))]
	sealed class DtbLinehaulManifestDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<DtbLinehaulManifest>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.NewWithValidTestData<DtbLinehaulManifest>();
		}
	}
}
