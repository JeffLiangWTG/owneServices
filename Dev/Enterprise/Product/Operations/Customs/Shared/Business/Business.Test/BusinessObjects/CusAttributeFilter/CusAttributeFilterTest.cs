using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAttributeFilter))]
	class CusAttributeFilterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCloneNotCopy_BG_CI()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			var attrib = pivot.Attributes1.AddNew();
			attrib.BG_AttributeValue1 = "V1";

			var attribClone = (CusAttributeFilter)attrib.Clone();

			AssertEquals("V1", attribClone.BG_AttributeValue1);
			AssertEquals(ZGuid.Empty, attribClone.BG_CI);
		}

		public void TestFilterName()
		{
			var attribute = Factory.New<CusAttributeFilter>();
			AssertEquals(CusAttributeFilter.AttributeFilterName.Unknown, attribute.FilterName);
			foreach (var filterName in new CusAttributeFilter.AttributeFilterName[] { CusAttributeFilter.AttributeFilterName.AT1, CusAttributeFilter.AttributeFilterName.AT2, CusAttributeFilter.AttributeFilterName.AT3 })
			{
				attribute.BG_AttributeName = filterName.ToString();
				AssertEquals(filterName, attribute.FilterName);
			}
		}

		public void TestIsAttribute1()
		{
			var attribute = Factory.New<CusAttributeFilter>();
			AssertEquals(false, attribute.IsAttribute1);
			attribute.BG_AttributeName = nameof(CusAttributeFilter.AttributeFilterName.AT1);
			AssertEquals(true, attribute.IsAttribute1);
			foreach (var filterName in new CusAttributeFilter.AttributeFilterName[] { CusAttributeFilter.AttributeFilterName.AT2, CusAttributeFilter.AttributeFilterName.AT3 })
			{
				attribute.BG_AttributeName = filterName.ToString();
				AssertEquals(false, attribute.IsAttribute1);
			}
		}

		public void TestIsAttribute2()
		{
			var attribute = Factory.New<CusAttributeFilter>();
			AssertEquals(false, attribute.IsAttribute2);
			attribute.BG_AttributeName = nameof(CusAttributeFilter.AttributeFilterName.AT2);
			AssertEquals(true, attribute.IsAttribute2);
			foreach (var filterName in new CusAttributeFilter.AttributeFilterName[] { CusAttributeFilter.AttributeFilterName.AT1, CusAttributeFilter.AttributeFilterName.AT3 })
			{
				attribute.BG_AttributeName = filterName.ToString();
				AssertEquals(false, attribute.IsAttribute2);
			}
		}

		public void TestIsAttribute3()
		{
			var attribute = Factory.New<CusAttributeFilter>();
			AssertEquals(false, attribute.IsAttribute3);
			attribute.BG_AttributeName = nameof(CusAttributeFilter.AttributeFilterName.AT3);
			AssertEquals(true, attribute.IsAttribute3);
			foreach (var filterName in new CusAttributeFilter.AttributeFilterName[] { CusAttributeFilter.AttributeFilterName.AT2, CusAttributeFilter.AttributeFilterName.AT1 })
			{
				attribute.BG_AttributeName = filterName.ToString();
				AssertEquals(false, attribute.IsAttribute3);
			}
		}

		public void TestPart()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var attributeFilter = pivot.Attributes1.AddNew();
			AssertEquals(part, attributeFilter.Part);
			attributeFilter.BG_CI = ZGuid.Empty;
			AssertNull(attributeFilter.Part);
		}

		public void TestITypeDeciderContext()
		{
			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusAttributeFilter>() as ITypeDeciderContext).Country);

				var cusClassification = Factory.New<BaseCusClassification>();
				cusClassification.CC_RN_NKCountryCode = "CA";
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_RN_NKCountry = "CA";
				pivot.CI_CC = cusClassification.PK;

				var attributeFilter = pivot.Attributes1.AddNew();
				AssertEquals("From Pivot", "CA", (attributeFilter as ITypeDeciderContext).Country);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var part = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var attributeFilter = pivot.Attributes1.AddNew();
			return attributeFilter;
		}
	}
}
