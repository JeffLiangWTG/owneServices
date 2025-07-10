using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationRegistryDataType))]
	sealed class CusGoodsLocationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CusGoodsLocationRegistryDataType>
	{
		protected override CusGoodsLocationRegistryDataType GetNewDataType() => new CusGoodsLocationRegistryDataType();
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new CusGoodsLocationCollection();
			var element1 = collection1.AddNew();
			element1.MessageType = "IMP";
			element1.GoodsLocation = "ANP0060D";
			element1.CustomsOffice = "CC";
			var collection2 = new CusGoodsLocationCollection();
			var element2 = collection2.AddNew();
			element2.MessageType = "EXP";
			element2.GoodsLocation = "ANP0061D";
			element2.CustomsOffice = "DD";
			return new[] { new ValidSampleAndBinaryValueInDB(collection1, new CusGoodsLocationRegistryDataType().Serialise(collection1)), new ValidSampleAndBinaryValueInDB(collection2, new CusGoodsLocationRegistryDataType().Serialise(collection2)) };
		}

		protected override string ExpectedEditorName => "CusGoodsLocationRegistryItemEditor";
		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(new BusinessObjectFactory()).CreateRegistryItemCusGoodsLocation();
		}
	}
}
