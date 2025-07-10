using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefPreferenceCollection))]
	public class CusRefPreferenceCollectionTest : ActiveBusinessObjectCollectionTestCase<CusRefPreferenceCollection>
	{
		protected override CusRefPreferenceCollection GetCollectionToTest()
		{
			return new CusRefPreferenceCollection(Factory);
		}
	}
}
