using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackage))]
	class CusPackageDeferrableTriggerTest : DeferrableTriggerTestCase<CusPackage>
	{
	}
}
