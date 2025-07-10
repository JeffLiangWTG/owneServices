using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFHeaderCollection))]
	sealed class CusISFHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusISFHeaderCollection>
	{
	}
}
