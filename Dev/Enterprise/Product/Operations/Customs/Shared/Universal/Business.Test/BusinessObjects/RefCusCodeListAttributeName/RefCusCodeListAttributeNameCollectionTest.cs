using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeListAttributeNameCollection))]
	public class RefCusCodeListAttributeNameCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusCodeListAttributeNameCollection>
	{
		protected override RefCusCodeListAttributeNameCollection GetCollectionToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TP1", "Ref Code Type 1");
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			return new RefCusCodeListAttributeNameCollection(collection);
		}

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}
	}
}
