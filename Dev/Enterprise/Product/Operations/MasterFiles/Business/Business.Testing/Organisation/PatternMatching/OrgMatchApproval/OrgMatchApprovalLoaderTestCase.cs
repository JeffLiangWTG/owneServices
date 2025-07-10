using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgMatchApprovalLoader))]
	sealed class OrgMatchApprovalLoaderTestCase : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new OrgMatchApprovalLoader(Factory);
		}
	}
}
