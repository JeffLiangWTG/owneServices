using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(ASNRefreshOptionsConfigRegistryItem))]
	sealed class ASNRefreshOptionsConfigRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<ASNRefreshOptionsConfigCollection>
	{
		public void TestCompanyLevelValue()
		{
			var refreshOptions = registryItem.Value;

			AssertEquals(1, refreshOptions.Count);

			AssertEquals(Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification, refreshOptions[0].FieldType);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);

			var config = collection.AddNew();
			config.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification;

			registryItem = new ASNRefreshOptionsConfigRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, collection);
		}

		protected override StronglyTypedRegistryItem<ASNRefreshOptionsConfigCollection, ASNRefreshOptionsConfigCollection> GetNewRegistryItem()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);

			return new ASNRefreshOptionsConfigRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, collection);
		}

		ASNRefreshOptionsConfigRegistryItem registryItem;

		#endregion
	}
}
