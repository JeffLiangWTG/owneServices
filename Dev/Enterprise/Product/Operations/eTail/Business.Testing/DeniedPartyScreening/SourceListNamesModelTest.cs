using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	public class SourceListNamesModelTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			InitList("AAA");
			InitList("BBB");
			InitList("CCC", true);
			InitList("DDD", true);
			Factory.Save();

			var model = new SourceListNamesModel(Factory, new[] { "AAA", "BBB", "CCC" });

			AssertEquals("Included list contains 2 entry", 2, model.IncludedLists.Length);
			AssertContainsExactElementsInAnyOrder(new[] { ("AAA Name", "AAA Description"), ("BBB Name", "BBB Description") }, model.IncludedLists.Select(u => (u.Name.ToString(), u.Description.ToString())));

			AssertEquals("Excluded list contains 1 entry", 1, model.ExcludedLists.Length);
			AssertContainsExactElementsInAnyOrder(new[] { ("CCC Name", "CCC Description") }, model.ExcludedLists.Select(u => (u.Name.ToString(), u.Description.ToString())));
		}

		public void TestConstructor_ArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new SourceListNamesModel(Factory, null));
			AssertExceptionThrown<ArgumentNullException>(() => _ = new SourceListNamesModel(null, Array.Empty<string>()));
		}

		void InitList(string code, bool? isExcluded = null)
		{
			var complianceList = Factory.New<RefComplianceList>();
			complianceList.RCL_ListCode = code;
			complianceList.RCL_ListType = code + " Type";
			complianceList.RCL_ListName = code + " Name";
			complianceList.RCL_ListDescription = code + " Description";
			complianceList.RCL_ListPublisher = code + " Publisher";
			complianceList.RCL_IsActive = true;
			complianceList.RCL_LastUpdatedDate = new ZDate(2021, 6, 30);

			if (isExcluded.HasValue)
			{
				complianceList.RCL_IsExcluded = isExcluded.Value;
			}
		}
	}
}
