using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotCollection<BaseCusClassPartPivot>))]
	class CusClassPartPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewElement()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			BaseCusClassPartPivot pivot = part.PivotsForBinding.AddNew();

			var childPivots = new CusClassPartPivotCollection<BaseCusClassPartPivot>(pivot, GlbCompany.CurrentCompany.Country.RN_Code);

			BaseCusClassPartPivot childPivot = childPivots.AddNew();
			AssertEquals(part.PK, childPivot.CI_OP);
			AssertEquals((byte)1, childPivot.CI_ChildListOrder);

			childPivot = childPivots.AddNew();
			AssertEquals(part.PK, childPivot.CI_OP);
			AssertEquals((byte)2, childPivot.CI_ChildListOrder);
			AssertEquals(GlbCompany.CurrentCompany.Country.RN_Code, childPivot.CI_RN_NKCountry);
		}

		public void TestNoDbHitsForChildren()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var childPivots = new CusClassPartPivotCollection<BaseCusClassPartPivot>(pivot, GlbCompany.CurrentCompany.Country.RN_Code);
			childPivots.AddNew();
			AssertEquals("(pre-condition)", 1, childPivots.Count);

			Factory.Save();

			var otherFactory = NewFactory();
			var part2 = otherFactory.Load<OrgSupplierPart>(part.PK);
			var pivot2 = part2.PivotsForBinding.Single(p => p.PK == pivot.PK);

			using (AssertDbHitsForAllFactories(ignoreUnspecified: false, expectedHitCounts: new Dictionary<string, int>
			{
				{ CusClassPartPivotSchema.Constants.TableName, 0 }
			}))
			{
				var childPivots2 = new CusClassPartPivotCollection<BaseCusClassPartPivot>(pivot2, GlbCompany.CurrentCompany.Country.RN_Code);
				childPivots2.Load();
				AssertEquals("Children.Count", 1, childPivots2.Count);
			}
		}

		public void TestSetDefaultsForNewElement_PartConstructor()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			BaseCusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			AssertEquals(GlbCompany.CurrentCompany.Country.RN_Code, pivot.CI_RN_NKCountry);
		}

		public void TestGetAnotherHTIPivotWithAttributeSetupFor()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.Attributes1.AddNew();

			AssertEquals(pivot1, part.PivotsForBinding.GetAnotherHTIPivotWithAttributeSetupFor(CusAttributeFilter.AttributeFilterName.AT1, pivot2));
			AssertNull(part.PivotsForBinding.GetAnotherHTIPivotWithAttributeSetupFor(CusAttributeFilter.AttributeFilterName.AT2, pivot2));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var part = Factory.New<OrgSupplierPart>();
			return new CusClassPartPivotCollection<BaseCusClassPartPivot>(part, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<BaseCusClassPartPivot>();
		}
	}
}
