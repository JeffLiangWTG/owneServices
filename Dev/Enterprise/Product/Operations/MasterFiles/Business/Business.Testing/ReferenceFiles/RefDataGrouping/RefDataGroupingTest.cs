using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefDataGrouping))]
	sealed class RefDataGroupingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetParentDataGrouping()
		{
			var dataGrouping1 = Factory.New<RefDataGrouping>();
			dataGrouping1.ZZZ_DataGrouping = "DG1";
			dataGrouping1.ZZZ_Description = "DG1";
			var dataGrouping2 = Factory.New<RefDataGrouping>();
			dataGrouping2.ZZZ_DataGrouping = "DG2";
			dataGrouping2.ZZZ_Description = "DG2";
			dataGrouping2.ZZZ_ZZZ_Grouping = dataGrouping1.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNull("Parent", RefDataGrouping.GetParentDataGrouping(Factory, "DG1"));
				AssertEquals("Child", "DG1", RefDataGrouping.GetParentDataGrouping(Factory, "DG2").ZZZ_DataGrouping);
			});
		}

		public void TestGetParentDataGroupingCode()
		{
			var dataGrouping1 = Factory.New<RefDataGrouping>();
			dataGrouping1.ZZZ_DataGrouping = "DG1";
			dataGrouping1.ZZZ_Description = "DG1";
			var dataGrouping2 = Factory.New<RefDataGrouping>();
			dataGrouping2.ZZZ_DataGrouping = "DG2";
			dataGrouping2.ZZZ_Description = "DG2";
			dataGrouping2.ZZZ_ZZZ_Grouping = dataGrouping1.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Parent", "", RefDataGrouping.GetParentDataGroupingCode(Factory, "DG1"));
				AssertEquals("Child", "DG1", RefDataGrouping.GetParentDataGroupingCode(Factory, "DG2"));
			});
		}

		public void TestDataGroupingMembers()
		{
			var dataGrouping1 = Factory.New<RefDataGrouping>();
			dataGrouping1.ZZZ_DataGrouping = "DG1";
			dataGrouping1.ZZZ_Description = "DG1 DESC";
			var dataGrouping2 = Factory.New<RefDataGrouping>();
			dataGrouping2.ZZZ_DataGrouping = "DG2";
			dataGrouping2.ZZZ_Description = "DG2 DESC";
			dataGrouping2.ZZZ_ZZZ_Grouping = dataGrouping1.PK;
			var dataGrouping3 = Factory.New<RefDataGrouping>();
			dataGrouping3.ZZZ_DataGrouping = "DG3";
			dataGrouping3.ZZZ_Description = "DG3 DESC";
			var dataGrouping4 = Factory.New<RefDataGrouping>();
			dataGrouping4.ZZZ_DataGrouping = "DG4";
			dataGrouping4.ZZZ_Description = "DG4 DESC";
			dataGrouping4.ZZZ_ZZZ_Grouping = dataGrouping3.PK;
			var dataGrouping5 = Factory.New<RefDataGrouping>();
			dataGrouping5.ZZZ_DataGrouping = "DG5";
			dataGrouping5.ZZZ_Description = "DG5 DESC";
			dataGrouping5.ZZZ_ZZZ_Grouping = dataGrouping3.PK;
			var members = dataGrouping1.DataGroupingMembers;
			AssertContainsExactElementsInAnyOrder(dataGrouping1.DataGroupingMembers, new[] { dataGrouping2 });
			AssertEquals(0, dataGrouping2.DataGroupingMembers.Count);
			AssertContainsExactElementsInAnyOrder(dataGrouping3.DataGroupingMembers, new[] { dataGrouping4, dataGrouping5 });
			AssertEquals(0, dataGrouping4.DataGroupingMembers.Count);
			AssertEquals(0, dataGrouping5.DataGroupingMembers.Count);
		}

		public void TestApplicableDataqGroupingList()
		{
			var dataGrouping1 = Factory.New<RefDataGrouping>();
			dataGrouping1.ZZZ_DataGrouping = "DG1";
			dataGrouping1.ZZZ_Description = "DG1 DESC";
			var dataGrouping2 = Factory.New<RefDataGrouping>();
			dataGrouping2.ZZZ_DataGrouping = "DG2";
			dataGrouping2.ZZZ_Description = "DG2 DESC";
			dataGrouping2.ZZZ_ZZZ_Grouping = dataGrouping1.PK;
			var dataGrouping3 = Factory.New<RefDataGrouping>();
			dataGrouping3.ZZZ_DataGrouping = "DG3";
			dataGrouping3.ZZZ_Description = "DG3 DESC";
			var dataGrouping4 = Factory.New<RefDataGrouping>();
			dataGrouping4.ZZZ_DataGrouping = "DG5";
			dataGrouping4.ZZZ_Description = "DG4 DESC";
			dataGrouping4.ZZZ_ZZZ_Grouping = dataGrouping3.PK;
			var dataGrouping5 = Factory.New<RefDataGrouping>();
			dataGrouping5.ZZZ_DataGrouping = "DG4";
			dataGrouping5.ZZZ_Description = "DG5 DESC";
			dataGrouping5.ZZZ_ZZZ_Grouping = dataGrouping3.PK;
			var list = dataGrouping1.ApplicableDataGroupingList;
			AssertSame(list, dataGrouping1.ApplicableDataGroupingList);
			AssertEquals(2, list.Count);
			AssertEquals(dataGrouping1, list[0]);
			AssertEquals(dataGrouping2, list[1]);

			list = dataGrouping2.ApplicableDataGroupingList;
			AssertEquals(1, list.Count);
			AssertEquals(dataGrouping2, list[0]);

			list = dataGrouping3.ApplicableDataGroupingList;
			AssertEquals(3, list.Count);
			AssertEquals(dataGrouping3, list[0]);
			AssertEquals(dataGrouping5, list[1]);
			AssertEquals(dataGrouping4, list[2]);

			list = dataGrouping4.ApplicableDataGroupingList;
			AssertEquals(1, list.Count);
			AssertEquals(dataGrouping4, list[0]);

			list = dataGrouping5.ApplicableDataGroupingList;
			AssertEquals(1, list.Count);
			AssertEquals(dataGrouping5, list[0]);
		}

		public void TestGetQueryIncludeParentDataGrouping()
		{
			var dataGrouping1 = Factory.New<RefDataGrouping>();
			dataGrouping1.ZZZ_DataGrouping = "DG1";
			dataGrouping1.ZZZ_Description = "DG1";
			var dataGrouping2 = Factory.New<RefDataGrouping>();
			dataGrouping2.ZZZ_DataGrouping = "DG2";
			dataGrouping2.ZZZ_Description = "DG2";
			dataGrouping2.ZZZ_ZZZ_Grouping = dataGrouping1.PK;

			var vessel1 = Factory.New<RefVesselZZ>();
			vessel1.ZZO_Code = "vessel1";
			vessel1.ZZO_ZZZ_NKDataGrouping = "DG1";

			var vessel2 = Factory.New<RefVesselZZ>();
			vessel2.ZZO_Code = "vessel2";
			vessel2.ZZO_ZZZ_NKDataGrouping = "DG2";

			Factory.Save();

			var vessels = Factory.Load<RefVesselZZ>(RefDataGrouping.GetQueryIncludeParentDataGrouping(Factory, RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, "DG2"));
			AssertEquals("Include the record belongs to parent data grouping", 2, vessels.Length);
		}

		public void TestGetDataGroupingIncludingParent()
		{
			var dataGrouping1 = Factory.New<RefDataGrouping>();
			dataGrouping1.ZZZ_DataGrouping = "DG1";
			dataGrouping1.ZZZ_Description = "DG1";
			var dataGrouping2 = Factory.New<RefDataGrouping>();
			dataGrouping2.ZZZ_DataGrouping = "DG2";
			dataGrouping2.ZZZ_Description = "DG2";
			dataGrouping2.ZZZ_ZZZ_Grouping = dataGrouping1.PK;

			Factory.Save();

			var dataGroupings = RefDataGrouping.GetDataGroupingIncludingParent(Factory, dataGrouping1.ZZZ_DataGrouping);
			AssertEquals("Data grouping without parent", 1, dataGroupings.Length);

			dataGroupings = RefDataGrouping.GetDataGroupingIncludingParent(Factory, dataGrouping2.ZZZ_DataGrouping);
			AssertEquals("Data grouping with parent", 2, dataGroupings.Length);

			dataGroupings = RefDataGrouping.GetDataGroupingIncludingParent(Factory, ZString.Empty);
			AssertEquals("Empty data grouping", 1, dataGroupings.Length);
		}
	}
}
