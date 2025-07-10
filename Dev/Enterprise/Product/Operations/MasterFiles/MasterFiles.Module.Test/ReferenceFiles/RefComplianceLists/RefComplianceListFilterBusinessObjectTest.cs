using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Module.RefComplianceListFilterBusinessObject;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefComplianceListFilterBusinessObject))]
	class RefComplianceListFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TextFilters

		public void TestListName()
		{
			var refComplianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			var refComplianceList2 = Factory.NewWithValidTestData<RefComplianceList>();

			refComplianceList1.RCL_ListName = "ABC Enterprise";
			refComplianceList2.RCL_ListName = "XYZ Enterprise";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Name"];
			AssertNotNull("Name filter", filter);

			filter.Property = "ABC Enterprise";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Equal 'ABC Enterprise': Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionNotContains("Equal 'ABC Enterprise': Expect collection does not contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = "Enterprise";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Contains 'Enterprise': Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionContains("Contains 'Enterprise': Expect collection to contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = "XY";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionNotContains("StartsWith 'XY': Expect collection does not contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionContains("StartsWith 'XY': Expect collection to contain refComplianceList2", refComplianceList2, collection);
			});
		}

		public void TestListCode()
		{
			var refComplianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			var refComplianceList2 = Factory.NewWithValidTestData<RefComplianceList>();

			refComplianceList1.RCL_ListCode = "ABC";
			refComplianceList2.RCL_ListCode = "XYZ";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Code"];
			AssertNotNull("Code filter", filter);

			filter.Property = "ABC";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Equal 'ABC': Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionNotContains("Equal 'ABC': Expect collection does not contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = "X";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionNotContains("StartsWith 'X': Expect collection does not contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionContains("StattsWith 'X': Expect collection to contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = "XYZ";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("NotEqual 'XYZ': Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionNotContains("NotEqual 'XYZ': Expect collection does not contain refComplianceList2", refComplianceList2, collection);
			});
		}

		public void TestListPublisher()
		{
			var refComplianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			var refComplianceList2 = Factory.NewWithValidTestData<RefComplianceList>();

			refComplianceList1.RCL_ListPublisher = "CCGOV1";
			refComplianceList2.RCL_ListPublisher = "CCGOV2";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Publisher"];
			AssertNotNull("Publisher filter", filter);

			filter.Property = "CCGOV1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Equal 'CCGOV1': Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionNotContains("Equal 'CCGOV1': Expect collection does not contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = "GOV";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Contains 'GOV': Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionContains("Contains 'GOV': Expect collection to contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = "GOV";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionNotContains("NotContains 'GOV': Expect collection does not contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionNotContains("NotContains 'GOV': Expect collection does not contain refComplianceList2", refComplianceList2, collection);
			});
		}

		public void TestListPublisherJurisdiction()
		{
			var refComplianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			var refComplianceList2 = Factory.NewWithValidTestData<RefComplianceList>();

			refComplianceList1.RCL_PublisherJurisdiction = "PUBLISHER JURISDICTION TEST 1";
			refComplianceList2.RCL_PublisherJurisdiction = "PUBLISHER JURISDICTION TEST 2";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Publisher Jurisdiction"];
			AssertNotNull("Publisher Jurisdiction filter", filter);

			filter.Property = "PUBLISHER JURISDICTION TEST 1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Equal 'PUBLISHER JURISDICTION TEST 1': Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionNotContains("Equal 'PUBLISHER JURISDICTION TEST 1': Expect collection does not contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = string.Empty;
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("IsNotBlank: Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionContains("IsNotBlank: Expect collection to contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = "PUBLISHER";
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionNotContains("DoesNotStartWith 'PUBLISHER': Expect collection does not contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionNotContains("DoesNotStartWith 'PUBLISHER': Expect collection does not contain refComplianceList2", refComplianceList2, collection);
			});
		}

		public void TestListType()
		{
			var refComplianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			var refComplianceList2 = Factory.NewWithValidTestData<RefComplianceList>();

			refComplianceList1.RCL_ListType = "TYPE TEST 1";
			refComplianceList2.RCL_ListType = "TYPE TEST 2";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Type"];
			AssertNotNull("Type filter", filter);

			filter.Property = "TYPE TEST 1";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Equal 'TYPE TEST 1': Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionNotContains("Equal 'TYPE TEST 1': Expect collection does not contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = "TEST";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Contains 'TEST': Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionContains("Contains 'TEST': Expect collection to contain refComplianceList2", refComplianceList2, collection);
			});

			filter.Property = "1";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.IsActive = true;
			collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionNotContains("NotContains '1': Expect collection does not contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionContains("NotContains '1': Expect collection to contain refComplianceList2", refComplianceList2, collection);
			});
		}

		#endregion

		#region TestExclusionStatus

		public void TestExclusionStatus_Excluded()
		{
			var refComplianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			var refComplianceList2 = Factory.NewWithValidTestData<RefComplianceList>();
			refComplianceList2.RCL_IsExcluded = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Exclusion Status"];
			AssertNotNull("Exclusion Status filter", filter);
			AssertEquals("Filter category", FilterCategories.StatusAndFlags, filter.Category);

			filter.Property = RefComplianceListExclusionStatusFilterCodes.Excluded;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionNotContains("Expect collection does not contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionContains("Expect collection to contain refComplianceList2", refComplianceList2, collection);
			});
		}

		public void TestExclusionStatus_Included()
		{
			var refComplianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			var refComplianceList2 = Factory.NewWithValidTestData<RefComplianceList>();
			refComplianceList2.RCL_IsExcluded = true;
			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Exclusion Status"];
			AssertNotNull("Exclusion Status filter", filter);
			AssertEquals("Filter category", FilterCategories.StatusAndFlags, filter.Category);

			filter.Property = RefComplianceListExclusionStatusFilterCodes.Included;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionNotContains("Expect collection does not contain refComplianceList2", refComplianceList2, collection);
			});
		}

		public void TestExclusionStatus_All()
		{
			var refComplianceList1 = Factory.NewWithValidTestData<RefComplianceList>();
			var refComplianceList2 = Factory.NewWithValidTestData<RefComplianceList>();
			refComplianceList2.RCL_IsExcluded = true;
			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Exclusion Status"];
			AssertNotNull("Exclusion Status filter", filter);
			AssertEquals("Filter category", FilterCategories.StatusAndFlags, filter.Category);

			filter.Property = RefComplianceListExclusionStatusFilterCodes.All;
			filter.IsActive = true;
			var collection = Factory.Load<RefComplianceList>(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertCollectionContains("Expect collection to contain refComplianceList1", refComplianceList1, collection);
				AssertCollectionContains("Expect collection to contain refComplianceList2", refComplianceList2, collection);
			});
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery("delete from dbo.RefComplianceList"); //to fix "...very large number for a test. Change the test so it uses fewer objects" error
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefComplianceListFilterBusinessObject();
		}

		protected virtual FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = GetNewFilterStripBusinessObject();
				}
				return fFilterStripBizO;
			}
		}

		FilterStripBusinessObject fFilterStripBizO;

		#endregion
	}
}
