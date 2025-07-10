using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgMatchApproval.Loader))]
	sealed class OrgMatchApprovalNewLoaderTestCase : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new OrgMatchApproval.Loader(Factory);
		}
	}
}
