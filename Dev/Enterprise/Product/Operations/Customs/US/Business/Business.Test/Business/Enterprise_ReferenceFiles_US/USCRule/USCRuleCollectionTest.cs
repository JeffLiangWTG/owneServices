using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCRuleCollection))]
	sealed class USCRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<USCRuleCollection>
	{
	}
}
