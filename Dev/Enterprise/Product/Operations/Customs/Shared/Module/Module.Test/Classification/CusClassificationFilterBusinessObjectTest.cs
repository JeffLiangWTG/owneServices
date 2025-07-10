using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusClassificationFilterBusinessObject))]
	public class CusClassificationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLookupCodeQuery()
		{
			class1.CC_LookupCode = "XXX";
			class2.CC_LookupCode = "YYY";
			Factory.Save();
			var lookupCodeFilter = (ModuleTextFilter)filterBO["Lookup Code"];
			lookupCodeFilter.Property = "XXX";
			lookupCodeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			lookupCodeFilter.Property = "ZZZ";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
		}

		public void TestDescriptionQuery()
		{
			class1.CC_Description = "XXX";
			class2.CC_Description = "YYY";
			Factory.Save();
			var descriptionFilter = (ModuleTextFilter)filterBO["Description"];
			descriptionFilter.Property = "XXX";
			descriptionFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			descriptionFilter.Property = "ZZZ";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
		}

		public void TestTariffNumQuery()
		{
			class1.CC_TariffNum = "111";
			class2.CC_TariffNum = "222";
			Factory.Save();
			var tariffFilter = (ModuleTextFilter)filterBO["Tariff No"];
			tariffFilter.Property = "111";
			tariffFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			tariffFilter.Property = "333";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
		}

		protected CusClassificationFilterBusinessObject filterBO;
		protected BaseClassificationCollection<BaseCusClassification> filterCollection;
		protected BaseCusClassification class1;
		protected BaseCusClassification class2;
		protected BaseCusClassification class3;
		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new CusClassificationFilterBusinessObject();
			filterCollection = new BaseClassificationCollection<BaseCusClassification>(Factory);
			class1 = Factory.New<BaseCusClassification>();
			class1.CC_LookupCode = "1";
			class1.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			class2 = Factory.New<BaseCusClassification>();
			class2.CC_LookupCode = "2";
			class2.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			class3 = Factory.New<BaseCusClassification>();
			class3.CC_LookupCode = "3";
			class3.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CusClassificationFilterBusinessObject();
		}
	}
}
