using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USExportClassificationFilterBusinessObject))]
	sealed class USExportClassificationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestTariffFilter()
		{
			var filterCollection = new Customs.Business.BaseClassificationCollection<CusClassification>(Factory);
			var filterBO = new USExportClassificationFilterBusinessObject();
			var class1 = Factory.New<CusClassification>();
			class1.CC_LookupCode = "TestLookup";
			class1.CC_TariffNum = "1234.56.7890";
			class1.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			Factory.Save();
			var tariffFilter = (ModuleTextFilter)filterBO["Schedule B"];
			tariffFilter.IsActive = true;
			tariffFilter.Property = "1234.56.7890";
			AssertEquals("1234.56.7890", tariffFilter.Property);
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			AssertCollectionContains(class1, filterCollection);
			tariffFilter.Property = "1234567890";
			AssertEquals("1234.56.7890", tariffFilter.Property);
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			AssertCollectionContains(class1, filterCollection);
			tariffFilter.Property = "9876.54.32 10";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
			tariffFilter.Property = "1234";
			tariffFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			AssertCollectionContains(class1, filterCollection);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USExportClassificationFilterBusinessObject();
	}
}
