using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ExportClassificationCollection))]
	sealed class ExportClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var collection = new ExportClassificationCollection(Factory);
			CusClassification classification = collection.AddNew();
			AssertNotNull("classification", classification);
			AssertEquals("CC_ClassificationType", CusClassification.ClassificationType.EXP, classification.CC_ClassificationType);
		}

		public void TestLoadWithRelationshipfilter()
		{
			CusClassification importClassInThisCountry = Factory.New<CusClassification>();
			importClassInThisCountry.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			importClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CusClassification exportClassInOtherCountry = Factory.New<CusClassification>();
			exportClassInOtherCountry.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			exportClassInOtherCountry.CC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;
			CusClassification exportClassInThisCountry = Factory.New<CusClassification>();
			exportClassInThisCountry.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			exportClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var exportClasses = new ExportClassificationCollection(Factory);
			ZQuery filter = new ZQuery();
			filter.FetchOnlyFromLocalCache = true;
			exportClasses.Load(filter);
			AssertEquals("Should load records made in this country, EXP type", 1, exportClasses.Count);
			AssertEquals("Should load records made in this country, EXP type", CusClassification.ClassificationType.EXP, exportClasses[0].CC_ClassificationType);
			AssertEquals("Should load records made in this country, EXP type", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, exportClasses[0].CC_RN_NKCountryCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new ExportClassificationCollection(Factory);
	}
}
