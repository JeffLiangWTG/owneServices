using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAttributeFilterCollection))]
	sealed class CusAttributeFilterCollectionTest : ActiveBusinessObjectCollectionTestCase<CusAttributeFilterCollection>
	{
		public void TestHasValue1()
		{
			BaseCusClassPartPivot pivot = Factory.New<BaseCusClassPartPivot>();
			CusAttributeFilterCollection collection = new CusAttributeFilterCollection(pivot, nameof(CusAttributeFilter.AttributeFilterName.AT1));
			var attrib1 = collection.AddNew();
			attrib1.BG_AttributeValue1 = "A";
			var attrib2 = collection.AddNew();
			attrib2.BG_AttributeValue1 = "B";
			var attrib3 = collection.AddNew();
			attrib3.BG_AttributeValue1 = "C";
			AssertEquals(true, collection.HasValue1("a"));
			AssertEquals(true, collection.HasValue1("b"));
			AssertEquals(true, collection.HasValue1("C"));
			AssertEquals(false, collection.HasValue1(""));
			AssertEquals(false, collection.HasValue1("d"));
		}

		public void TestHasSameAttribute1()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			var attrib1 = pivot1.Attributes1.AddNew();
			attrib1.BG_AttributeValue1 = "A";
			var attrib2 = pivot1.Attributes1.AddNew();
			attrib2.BG_AttributeValue1 = "B";
			var pivot2 = part.PivotsForBinding.AddNew();
			var attrib3 = pivot2.Attributes1.AddNew();
			attrib3.BG_AttributeValue1 = "C";
			var attrib4 = pivot2.Attributes1.AddNew();
			attrib4.BG_AttributeValue1 = "a";
			AssertEquals(true, pivot1.Attributes1.HasSameValue1(attrib4));
			AssertEquals(false, pivot2.Attributes1.HasSameValue1(attrib4));
			attrib4.BG_AttributeValue1 = "c";
			AssertEquals(false, pivot1.Attributes1.HasSameValue1(attrib4));
			AssertEquals(true, pivot2.Attributes1.HasSameValue1(attrib4));
			attrib4.BG_AttributeValue1 = "d";
			AssertEquals(false, pivot1.Attributes1.HasSameValue1(attrib4));
			AssertEquals(false, pivot2.Attributes1.HasSameValue1(attrib4));
		}

		protected override CusAttributeFilterCollection GetCollectionToTest()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			return new CusAttributeFilterCollection(pivot, nameof(CusAttributeFilter.AttributeFilterName.AT1));
		}
	}
}
