using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusPackage))]
	sealed class CusPackageDeferrableTriggerTest : DeferrableTriggerTestCase<CusPackage>
	{
	}
}
