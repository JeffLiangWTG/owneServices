using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseClassificationCollection<BaseCusClassification>))]
	sealed class BaseClassificationCollectionBaseOnlyTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new BaseClassificationCollection<BaseCusClassification>(Factory);

		public void TestCountryFilter()
		{
			var classificationMadeInThisCountry = Factory.New<BaseCusClassification>();
			classificationMadeInThisCountry.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classificationMadeInThisCountry.CC_LookupCode = "THIS COUNRTY LOOKUPXZX";

			var otherCountry = Factory.New<RefCountry>();

			var classificationMadeInOtherCountry = Factory.New<BaseCusClassification>();
			classificationMadeInOtherCountry.CC_RN_NKCountryCode = otherCountry.Code;
			classificationMadeInOtherCountry.CC_LookupCode = "OTHER COUNTRY LOOOKUPXZX";

			var testCollection = new BaseClassificationCollection<BaseCusClassification>(Factory, new ZQuery(CusClassificationSchema.CC_LookupCode, SQLComparisonOperator.EndsWith, "XZX"));
			testCollection.Load();

			AssertEquals("There is only one classification lookup made in this country", 1, testCollection.Count);
		}

		public void TestListProviderReturnsCorrectCountryOnly()
		{
			GlbCompany.CurrentCompany.SetCountry("FJ");
			var collection = new BaseClassificationCollection<BaseCusClassification>.ListProvider(new BaseClassificationCollection<BaseCusClassification>(Factory));

			var bizObj = collection.GetBusinessObjectFromCode("XYZ123");
			AssertNull(bizObj);
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Fiji;
			classification.CC_LookupCode = "XYZ123";
			AssertEquals(classification, collection.GetBusinessObjectFromCode("XYZ123"));
			classification.CC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Brazil;
			bizObj = collection.GetBusinessObjectFromCode("XYZ123");
			AssertNull(bizObj);
		}

		public void TestUSCusClassificationFindBoxIsApplicableToPR()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			var cusClassification = Factory.New<BaseCusClassification>();
			cusClassification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			cusClassification.CC_LookupCode = "PRTEST";
			cusClassification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;

			var usDecType = ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>();
			var dec = Factory.New(usDecType) as BaseJobDeclaration;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine["US_TariffType"] = "HTS";
			var findBoxListProvider = invoiceLine.Lookups.ClassificationList as IFindBoxListProvider;
			AssertEquals("list contains an us cusClassification", cusClassification.CC_LookupCode, findBoxListProvider.CodeFromPrimaryKey(cusClassification.PK));
		}
	}
}
