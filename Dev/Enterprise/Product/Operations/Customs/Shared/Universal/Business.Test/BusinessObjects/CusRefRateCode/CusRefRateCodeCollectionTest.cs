using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefRateCodeCollection))]
	public class CusRefRateCodeCollectionTest : ActiveBusinessObjectCollectionTestCase<CusRefRateCodeCollection>
	{
		protected override CusRefRateCodeCollection GetCollectionToTest()
		{
			return new CusRefRateCodeCollection(Factory);
		}
	}
}
