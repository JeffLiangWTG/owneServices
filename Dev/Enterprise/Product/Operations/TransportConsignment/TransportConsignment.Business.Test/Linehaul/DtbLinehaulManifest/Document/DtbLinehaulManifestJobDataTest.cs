using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbLinehaulManifestJobDataTest : AssemblyDataTest
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(DtbLinehaulManifest), new DtbLinehaulManifestJobData().BusinessObjectType);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertEquals(typeof(DtbLinehaulManifestCollection), new DtbLinehaulManifestJobData().GetBusinessObjectCollection(Factory).GetType());
		}

		public void TestModuleID()
		{
			AssertEquals(null, new DtbLinehaulManifestJobData().ModuleID);
		}

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, new DtbLinehaulManifestJobData().ReferenceType);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Linehaul Manifest", new DtbLinehaulManifestJobData().HumanReadableName.ToString());
		}

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, new DtbLinehaulManifestJobData().IsAllowedForUnallocatedeDocs);
		}
	}
}
