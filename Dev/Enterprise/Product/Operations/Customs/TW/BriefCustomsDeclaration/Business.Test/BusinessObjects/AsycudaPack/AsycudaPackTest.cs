using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPackedItemType()
		{
			AssertType<AsycudaPackedItem>(packedItem);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew().Packs.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			pack = (AsycudaPack)GetNewBusinessObject();
			pack.Bill.Header.AMA_JobReference = "C4321";
			packedItem = pack.PackedItem;
		}

		AsycudaPack pack;
		AsycudaPackedItem packedItem;
	}
}
