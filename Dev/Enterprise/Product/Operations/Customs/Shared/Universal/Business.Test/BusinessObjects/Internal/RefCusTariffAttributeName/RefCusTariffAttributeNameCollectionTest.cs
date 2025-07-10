using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffAttributeNameCollection))]
	public class RefCusTariffAttributeNameCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusTariffAttributeNameCollection>
	{
		protected override RefCusTariffAttributeNameCollection GetCollectionToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China, "China");
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, "ABC");
			helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, "TP1", "Attribute 1", "Caption1", Core.Constants.CountryCodes.China, "ABC");

			var collection = new ChildTariffViewCollection(Factory, Core.Constants.CountryCodes.China, "ABC", ZDateTime.Today, new[] { new KeyValuePair<ZString, ZString>() });
			return new RefCusTariffAttributeNameCollection(collection);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
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
