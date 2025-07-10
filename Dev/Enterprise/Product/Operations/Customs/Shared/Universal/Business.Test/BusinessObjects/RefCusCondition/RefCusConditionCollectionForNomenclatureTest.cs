using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusConditionCollectionForNomenclature))]
	public class RefCusConditionCollectionForNomenclatureTest : ActiveBusinessObjectCollectionTestCase<RefCusConditionCollectionForNomenclature>
	{
		protected override RefCusConditionCollectionForNomenclature GetCollectionToTest()
		{
			return new RefCusConditionCollectionForNomenclature(Factory.New<RefCusNomenclatureGroup>());
		}
	}
}
