using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingPendingTransactionsNotificationGroupRegistryItem))]
	sealed class EInvoicingPendingTransactionsNotificationGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<EInvoicingPendingTransactionsNotificationGroup>
	{
		public void TestOnAllValuesSavedAction()
		{
			AssertNull("OnAllValuesSavedAction", Item.OnAllValuesSavedAction);
		}

		protected override StronglyTypedRegistryItem<EInvoicingPendingTransactionsNotificationGroup, EInvoicingPendingTransactionsNotificationGroup> GetNewRegistryItem()
			=> new EInvoicingPendingTransactionsNotificationGroupRegistryItem("", null, null, null, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default,
				new EInvoicingPendingTransactionsNotificationGroup()
				{
					GroupPK = TestGroup.PK,
					DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.PostDate,
					Days = 1
				});

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();
			TestGroup = factory.NewWithValidTestData<GlbGroup>();

			factory.Save();
		}

		GlbGroup TestGroup;
	}
}
