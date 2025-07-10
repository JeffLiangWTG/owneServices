using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseCusStorageDocPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadByTablePrefix()
		{
			var pivot = GetNewBusinessObjectForDeleteTest(Factory);
			Factory.Save();

			var loadResult = new BusinessObjectFactory().Load(pivot.TablePrefix, pivot.PK);
			AssertType("BaseCusStorageDocPivot", GetExpectedBusinessObjectType(), loadResult);
		}

		public abstract void TestParent();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() =>
			GetNewBusinessObjectForDeleteTest(Factory);
	}
}
