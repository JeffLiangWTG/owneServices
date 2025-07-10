using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileAttributeCollection))]
	public class RefCusProfileAttributeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusProfileAttributeCollection>
	{
		protected override RefCusProfileAttributeCollection GetCollectionToTest()
		{
			return new RefCusProfileAttributeCollection(Factory.New<RefCusProfile>());
		}
	}
}
