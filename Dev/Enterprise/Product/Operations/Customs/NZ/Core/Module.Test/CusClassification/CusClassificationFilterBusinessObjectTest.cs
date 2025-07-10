using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(CusClassificationFilterBusinessObject))]
	sealed class CusClassificationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CusClassificationFilterBusinessObject();
		}

		public void TestTariffNumberQuery()
		{
			class1.CC_TariffNum = "0000.00.00.00A";
			class2.CC_TariffNum = "1111.00.00.00A";
			Factory.Save();
			ModuleTextFilter tariffFilter = (ModuleTextFilter)filterBO["Tariff No"];
			tariffFilter.Property = "0000.00.00.00A";
			tariffFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			tariffFilter.Property = "0000000000A";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			tariffFilter.Property = "2222.00.00.00A";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
		}

		public void TestPermitCodeQuery()
		{
			class1.PermitCodes.AddNew("CUD", "1111");
			class1.PermitCodes.AddNew("DOC", "2222");
			class2.PermitCodes.AddNew("DOC", "2222");
			Factory.Save();
			ModuleTextFilter permitCodeFilter = (ModuleTextFilter)filterBO["Permit Code"];
			permitCodeFilter.Property = "CUD";
			permitCodeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			permitCodeFilter.Property = "DOC";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Two records", 2, filterCollection.Count);
			permitCodeFilter.Property = "CCC";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
		}

		public void TestProhibitedCodeQuery()
		{
			class1.ProhibitedCodes.AddNew("ANT", "");
			class1.ProhibitedCodes.AddNew("APC", "");
			class2.ProhibitedCodes.AddNew("APC", "");
			Factory.Save();
			ModuleTextFilter prohibitedCodeFilter = (ModuleTextFilter)filterBO["Prohibited Code"];
			prohibitedCodeFilter.Property = "ANT";
			prohibitedCodeFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			prohibitedCodeFilter.Property = "APC";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Two records", 2, filterCollection.Count);
			prohibitedCodeFilter.Property = "CCC";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
		}

		public void TestOtherInfoQuery()
		{
			class1.OtherInfos.AddNew("AWC", "");
			class1.OtherInfos.AddNew("BUN", "");
			class2.OtherInfos.AddNew("BUN", "");
			Factory.Save();
			ModuleTextFilter otherInfoFilter = (ModuleTextFilter)filterBO["Other Info"];
			otherInfoFilter.Property = "AWC";
			otherInfoFilter.IsActive = true;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			otherInfoFilter.Property = "BUN";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Two records", 2, filterCollection.Count);
			otherInfoFilter.Property = "CCC";
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
		}

		public void TestConcessionCodeQuery()
		{
			NZCConcession concession1 = Factory.NewWithValidTestData<NZCConcession>();
			concession1.U2_Code = "c1";
			NZCConcession concession2 = Factory.NewWithValidTestData<NZCConcession>();
			concession2.U2_Code = "c2";
			NZCConcession concession3 = Factory.NewWithValidTestData<NZCConcession>();
			concession3.U2_Code = "c3";
			class1.CC_ConcessionCode = concession1.U2_Code;
			class2.CC_ConcessionCode = concession2.U2_Code;
			class3.CC_ConcessionCode = concession2.U2_Code;
			Factory.Save();
			var concessionCodeFilter = (ModuleNkFilter)filterBO["Concession Code"];
			concessionCodeFilter.IsActive = true;
			concessionCodeFilter.Property = concession1.U2_Code;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("One record", 1, filterCollection.Count);
			AssertCollectionContains(class1, filterCollection);
			concessionCodeFilter.Property = concession2.U2_Code;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Two records", 2, filterCollection.Count);
			AssertCollectionContains(class2, filterCollection);
			AssertCollectionContains(class3, filterCollection);
			concessionCodeFilter.Property = concession3.U2_Code;
			filterCollection.Load(filterBO.Filter);
			AssertEquals("No record", 0, filterCollection.Count);
		}

		CusClassificationFilterBusinessObject filterBO;
		Customs.Business.BaseClassificationCollection<CusClassification> filterCollection;
		CusClassification class1;
		CusClassification class2;
		CusClassification class3;
		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (CusClassificationFilterBusinessObject)GetNewFilterStripBusinessObject();
			filterCollection = new Customs.Business.BaseClassificationCollection<CusClassification>(Factory);
			class1 = Factory.New<CusClassification>();
			class1.CC_LookupCode = "1";
			class2 = Factory.New<CusClassification>();
			class2.CC_LookupCode = "2";
			class3 = Factory.New<CusClassification>();
			class3.CC_LookupCode = "3";
		}
	}
}
