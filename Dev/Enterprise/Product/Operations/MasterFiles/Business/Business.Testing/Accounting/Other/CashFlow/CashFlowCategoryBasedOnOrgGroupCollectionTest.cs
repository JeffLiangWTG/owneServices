using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class CashFlowCategoryBasedOnOrgGroupCollectionTest<T, CollectionT> : RegistryBusinessObjectCollectionTemplateTestCase<CollectionT> where T : CashFlowCategoryBasedOnOrgGroup, new()
		where CollectionT : CashFlowCategoryBasedOnOrgGroupCollection<T>, new()
	{
		#region Implementation

		protected override CollectionT GetCollectionToTest()
		{
			return new CollectionT();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new T();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new CollectionT Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
