using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ImportClassificationCollection))]
	sealed class ImportClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var collection = new ImportClassificationCollection(Factory);
			CusClassification classification = collection.AddNew();
			AssertNotNull("classification", classification);
			AssertEquals("CC_ClassificationType", CusClassification.ClassificationType.IMP, classification.CC_ClassificationType);
		}

		public void TestLoadWithRelationshipfilter()
		{
			CusClassification importClassInThisCountry = Factory.New<CusClassification>();
			importClassInThisCountry.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			importClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CusClassification importClassInOtherCountry = Factory.New<CusClassification>();
			importClassInOtherCountry.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			importClassInOtherCountry.CC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;
			CusClassification exportClassInThisCountry = Factory.New<CusClassification>();
			exportClassInThisCountry.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			exportClassInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var importClasses = new ImportClassificationCollection(Factory);
			ZQuery filter = new ZQuery();
			filter.FetchOnlyFromLocalCache = true;
			importClasses.Load(filter);
			AssertEquals("Should load records made in this country, IMP type", 1, importClasses.Count);
			AssertEquals("Should load records made in this country, IMP type", CusClassification.ClassificationType.IMP, importClasses[0].CC_ClassificationType);
			AssertEquals("Should load records made in this country, IMP type", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, importClasses[0].CC_RN_NKCountryCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new ImportClassificationCollection(Factory);
	}
}
