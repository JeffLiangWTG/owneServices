using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartage))]
	sealed class CommonCartageRelatableActivityTest : RelatableActivityTestCase<CommonCartage>
	{
		protected override CommonCartage GetNewActivity()
		{
			return Factory.NewWithValidTestData<CommonCartage>();
		}
	}
}
