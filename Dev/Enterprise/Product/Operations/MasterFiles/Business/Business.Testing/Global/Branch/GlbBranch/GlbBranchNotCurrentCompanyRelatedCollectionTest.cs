using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchNotCurrentCompanyRelatedCollection))]
	sealed class GlbBranchNotCurrentCompanyRelatedCollectionTest : GlbBranchCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlbBranchNotCurrentCompanyRelatedCollection(Factory);
		}
	}
}
