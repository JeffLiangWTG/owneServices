using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class CashFlowCategoryBasedOnOrgGroupTest<T, S> : RegistryBusinessObjectTemplateTestCase where T : CashFlowCategoryBasedOnOrgGroup, new()
																										where S : CashFlowCategoryBasedOnOrgGroupCollection<T>, new()
	{
		public abstract void TestGetOrgGroupList();
		public abstract void TestGetOrgGroupDescriptionCore();

		protected override void SetUp()
		{
			base.SetUp();
			item = PopulateCashFlowCategoryBasedOnOrgGroupTestData();
			items = new S();
			items.Add(item);
		}

		protected T item;
		protected S items;

		public abstract T PopulateCashFlowCategoryBasedOnOrgGroupTestData();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			T item = PopulateCashFlowCategoryBasedOnOrgGroupTestData();
			return item;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected new T BizObj
		{
			get { return (T)base.BizObj; }
		}

		protected virtual S GetAuthorisationSettingsCollection()
		{
			return new S();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
