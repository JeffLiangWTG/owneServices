using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupFilterBusinessObject))]
	sealed class AccTaxOverrideGroupFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccTaxOverrideGroupFilterBusinessObject();
		}

		ModuleTextFilter GetModuleTextFilter(ZString description)
		{
			return (ModuleTextFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		ModuleGuidFilter GetModuleGuidFilter(ZString description)
		{
			return (ModuleGuidFilter)(GetNewFilterStripBusinessObject()[description]);
		}

		public void TestCodeFilter()
		{
			AccTaxOverrideGroupCollection taxOverrideGroups = new AccTaxOverrideGroupCollection(Factory);
			taxOverrideGroups.Load();
			taxOverrideGroups.RemoveAndDeleteAll();

			AccTaxOverrideGroup taxOverrideGroup1 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup1.AX_Code = "AB";

			AccTaxOverrideGroup taxOverrideGroup2 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup2.AX_Code = "BA";

			ModuleTextFilter filter = GetModuleTextFilter("Code");
			filter.IsActive = true;

			taxOverrideGroups = new AccTaxOverrideGroupCollection(Factory);
			filter.Property = "A";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			taxOverrideGroups.Load(filter.Query);
			AssertEquals("taxOverrideGroups.Count", 2, taxOverrideGroups.Count);
			AssertEquals("Should contain taxOverrideGroup1", true, taxOverrideGroups.Contains(taxOverrideGroup1));
			AssertEquals("Should contain taxOverrideGroup2", true, taxOverrideGroups.Contains(taxOverrideGroup2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			taxOverrideGroups.Load(filter.Query);
			AssertEquals("taxOverrideGroups.Count", 1, taxOverrideGroups.Count);
			AssertEquals("Should contain taxOverrideGroup1", true, taxOverrideGroups.Contains(taxOverrideGroup1));
			AssertEquals("Should not contain taxOverrideGroup2", false, taxOverrideGroups.Contains(taxOverrideGroup2));
		}

		public void TestDescriptionFilter()
		{
			AccTaxOverrideGroupCollection taxOverrideGroups = new AccTaxOverrideGroupCollection(Factory);
			taxOverrideGroups.Load();
			taxOverrideGroups.RemoveAndDeleteAll();

			AccTaxOverrideGroup taxOverrideGroup1 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup1.AX_Description = "AB";

			AccTaxOverrideGroup taxOverrideGroup2 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup2.AX_Description = "BA";

			ModuleTextFilter filter = GetModuleTextFilter("Description");
			filter.IsActive = true;

			taxOverrideGroups = new AccTaxOverrideGroupCollection(Factory);
			filter.Property = "A";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			taxOverrideGroups.Load(filter.Query);
			AssertEquals("taxOverrideGroups.Count", 2, taxOverrideGroups.Count);
			AssertEquals("Should contain taxOverrideGroup1", true, taxOverrideGroups.Contains(taxOverrideGroup1));
			AssertEquals("Should contain taxOverrideGroup2", true, taxOverrideGroups.Contains(taxOverrideGroup2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			taxOverrideGroups.Load(filter.Query);
			AssertEquals("taxOverrideGroups.Count", 1, taxOverrideGroups.Count);
			AssertEquals("Should contain taxOverrideGroup1", true, taxOverrideGroups.Contains(taxOverrideGroup1));
			AssertEquals("Should not contain taxOverrideGroup2", false, taxOverrideGroups.Contains(taxOverrideGroup2));
		}

		public void TestLinkedChargeCodeFilter()
		{
			AccTaxOverrideGroup taxOverrideGroup1 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup1.AX_Description = "CST1 CST5";
			taxOverrideGroup1.AX_Code = "AB1";

			AccTaxOverrideGroup taxOverrideGroup2 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup2.AX_Description = "REV1";
			taxOverrideGroup2.AX_Code = "AB2";

			AccTaxOverrideGroup taxOverrideGroup3 = Factory.New<AccTaxOverrideGroup>();
			taxOverrideGroup3.AX_Description = "CST1";
			taxOverrideGroup3.AX_Code = "AB3";

			AccChargeCode testChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode1.AC_Code = "CST1";
			testChargeCode1.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			testChargeCode1.AC_AX_TaxOverrideGroup = taxOverrideGroup3.PK;

			AccChargeCode testChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode2.AC_Code = "CST5";
			testChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;

			AccChargeCode testChargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			testChargeCode3.AC_Code = "REV1";
			testChargeCode3.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			testChargeCode3.AC_AX_TaxOverrideGroup = taxOverrideGroup2.PK; //VAT Tax Override group

			//Setting two different charge code in one tax override group
			AccTaxOverrideGroupChargeCodePivot taxOverrideGroupChargeCodePivot1 = Factory.New<AccTaxOverrideGroupChargeCodePivot>();
			taxOverrideGroupChargeCodePivot1.ACP_AC_ChargeCode = testChargeCode1.PK;
			taxOverrideGroupChargeCodePivot1.ACP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;

			AccTaxOverrideGroupChargeCodePivot taxOverrideGroupChargeCodePivot2 = Factory.New<AccTaxOverrideGroupChargeCodePivot>();
			taxOverrideGroupChargeCodePivot2.ACP_AC_ChargeCode = testChargeCode2.PK;
			taxOverrideGroupChargeCodePivot2.ACP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;

			Factory.Save();

			var taxOverrideGroups = new AccTaxOverrideGroupCollection(Factory);

			ModuleGuidFilter filter = GetModuleGuidFilter("Linked Charge Code");
			filter.IsActive = true;
			filter.Property = testChargeCode1.PK;
			taxOverrideGroups.Load(filter.Query);
			AssertEquals("taxOverrideGroups.Count", 2, taxOverrideGroups.Count);
			AssertEquals("Should contain taxOverrideGroup1", true, taxOverrideGroups.Contains(taxOverrideGroup1));
			AssertEquals("Should contain taxOverrideGroup3", true, taxOverrideGroups.Contains(taxOverrideGroup3));

			filter.IsActive = true;
			filter.Property = Guid.NewGuid();
			taxOverrideGroups.Load(filter.Query);
			AssertEquals("taxOverrideGroups.Count", 0, taxOverrideGroups.Count);

			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = testChargeCode1.PK;
			taxOverrideGroups.Load(filter.Query);
			AssertEquals("taxOverrideGroups.Count", 1, taxOverrideGroups.Count);
			AssertEquals("Should contain taxOverrideGroup2", true, taxOverrideGroups.Contains(taxOverrideGroup2));
		}

		public void TestTaxOverrideGroupFilterConfiguration()
		{
			ModuleGuidFilter linkedChargeCodeFilter = GetModuleGuidFilter("Linked Charge Code");
			Assert("Blank operators not supported for Linked Charge Code", !linkedChargeCodeFilter.SupportsBlankComparisonOperators);
			Assert("Filter Match Comparison operator supported for Linked Charge Code", linkedChargeCodeFilter.SupportsFiltersMatchComparisonOperator);
		}

		#endregion
	}
}
