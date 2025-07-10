using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Workflow.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskIterationLinkViewModelCollection))]
	internal class ProcessTaskIterationLinkViewModelCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProcessTaskIterationLinkViewModelCollection>
	{
		protected override ProcessTaskIterationLinkViewModelCollection GetCollectionToTest()
		{
			return new ProcessTaskIterationLinkViewModelCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProcessTaskIterationLinkViewModel(Factory.New<IProcessTaskIterationLink>());
		}

		public void TestLoad_ShouldFilterByQITLinkType()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "S01";
			staff1.GS_LoginName = "S01";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "S02";
			staff2.GS_LoginName = "S02";

			var task = Factory.New<ProcessTask>();

			var link1 = Factory.New<IProcessTaskIterationLink>();
			link1.P9I_P9_ContainmentBarrierTask = task.PK;
			link1.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link1.P9I_GS_NKResourceUnderReview = "S01";

			var link2 = Factory.New<IProcessTaskIterationLink>();
			link2.P9I_P9_ContainmentBarrierTask = task.PK;
			link2.P9I_LinkType = IterationLinkTypeList.Codes.PassedContainmentBarrier;
			link2.P9I_GS_NKResourceUnderReview = "S02";

			Factory.Save();

			task.IterationLinksViewModel.Build();

			AssertArrayEqualsByElements(
				"GIVEN QIT (Quality Iteration Task) and PCB (Passed Containtment Barrier) links WHEN load IterationLinksViewModel THEN should only load QIT link",
				new[] { link1.P9I_GS_NKResourceUnderReview },
				task.IterationLinksViewModel.Cast<ProcessTaskIterationLinkViewModel>().Select(link => link.ResourceUnderReviewNK).ToArray());
		}

		[TestDate(2017, 1, 1, 9, 0, 0)]
		public void TestLoad_SortByDate_Descending()
		{
			var task = Factory.New<ProcessTask>();

			var link1 = Factory.New<IProcessTaskIterationLink>();
			link1.P9I_P9_ContainmentBarrierTask = task.PK;
			link1.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link1.P9I_SystemCreateTimeUtc = ZDateTime.UtcNow;

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			var link2 = Factory.New<IProcessTaskIterationLink>();
			link2.P9I_P9_ContainmentBarrierTask = task.PK;
			link2.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link2.P9I_SystemCreateTimeUtc = ZDateTime.UtcNow;

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(2);

			var link3 = Factory.New<IProcessTaskIterationLink>();
			link3.P9I_P9_ContainmentBarrierTask = task.PK;
			link3.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link3.P9I_SystemCreateTimeUtc = ZDateTime.UtcNow;

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(3);
			link2.P9I_SystemLastEditTimeUtc = ZDateTime.UtcNow;

			var processTaskIterationLinkViewModelCollection = new ProcessTaskIterationLinkViewModelCollection(task);
			processTaskIterationLinkViewModelCollection.Build();

			AssertEquals("Should contain all 3 links i.e. link1, link2, link3", 3, processTaskIterationLinkViewModelCollection.Count);

			AssertEquals("Last updated", link2.P9I_GS_NKResourceUnderReview, processTaskIterationLinkViewModelCollection[0].ResourceUnderReviewNK);
			AssertEquals("Last created", link3.P9I_GS_NKResourceUnderReview, processTaskIterationLinkViewModelCollection[1].ResourceUnderReviewNK);
			AssertEquals("First created", link1.P9I_GS_NKResourceUnderReview, processTaskIterationLinkViewModelCollection[2].ResourceUnderReviewNK);
		}

		public void TestCollectionDoesNotAllowNew()
		{
			var task = Factory.New<ProcessTask>();
			var collection = new ProcessTaskIterationLinkViewModelCollection(task);
			AssertEquals("Collection should not allow new", false, collection.AllowNew);
		}

		public void TestCollectionDoesNotAllowRemove()
		{
			var task = Factory.New<ProcessTask>();
			var collection = new ProcessTaskIterationLinkViewModelCollection(task);
			AssertEquals("Collection should not allow remove", false, collection.AllowRemove);
		}
	}
}
