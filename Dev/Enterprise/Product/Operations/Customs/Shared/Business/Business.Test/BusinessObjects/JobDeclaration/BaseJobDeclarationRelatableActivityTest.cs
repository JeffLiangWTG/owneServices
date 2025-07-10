using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobDeclaration))]
	sealed class BaseJobDeclarationRelatableActivityTest : RelatableActivityTestCase<BaseJobDeclaration>
	{
		protected override BaseJobDeclaration GetNewActivity() => Factory.NewWithValidTestData<BaseJobDeclaration>();
	}
}
