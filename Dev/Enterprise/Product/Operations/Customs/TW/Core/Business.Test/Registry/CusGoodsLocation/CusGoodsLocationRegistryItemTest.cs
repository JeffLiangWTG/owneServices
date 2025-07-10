using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationRegistryItem))]
	sealed class CusGoodsLocationRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CusGoodsLocationCollection>
	{
		protected override StronglyTypedRegistryItem<CusGoodsLocationCollection, CusGoodsLocationCollection> GetNewRegistryItem() => new CusGoodsLocationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		protected override CusGoodsLocationCollection ValidValue
		{
			get
			{
				var collection = new CusGoodsLocationCollection();
				var element1 = collection.AddNew();
				element1.MessageType = "IMP";
				element1.GoodsLocation = "ANP0060D";
				element1.CustomsOffice = "CC";
				element1 = collection.AddNew();
				element1.MessageType = "EXP";
				element1.GoodsLocation = "ANP0061D";
				element1.CustomsOffice = "DD";
				return collection;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
		}
	}
}
