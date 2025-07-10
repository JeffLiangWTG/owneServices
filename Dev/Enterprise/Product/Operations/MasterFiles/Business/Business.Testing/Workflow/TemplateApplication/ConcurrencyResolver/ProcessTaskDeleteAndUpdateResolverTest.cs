using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.ConcurrencyResolver;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing.Workflow.TemplateApplication.ConcurrencyResolver
{
	public class ProcessTaskDeleteAndUpdateResolverTest : TestCaseWithFactory
	{
		public void TestShouldReturnOriginalIfDuplicateDoesNotHaveActualDate()
		{
			var original = Factory.NewWithValidTestData<ProcessTask>();
			var duplicate = Factory.NewWithValidTestData<ProcessTask>();
			((IMilestoneDateDefaultable)duplicate).ActualDate = ZDateTimeOffset.Empty;
			((IMilestoneDateDefaultable)original).ActualDate = ZDateTimeOffset.Now;

			var result = new ProcessTaskDeleteAndUpdateResolver(new Mock<ProcessTaskDeleteAndUpdateResolver.ProcessTaskConcurrencyResolverUpdate>().Object).DeleteDuplicateAndUpdateOriginalWhenRequired(original, duplicate);

			AssertEquals(1, result.RecordsMarkedForDeletion.Count());
			Assert(duplicate.IsDeleted);
			AssertEquals(original, result.ResultingRecord);
			Assert(result.Resolved);
		}

		public void TestShouldDeleteDuplicateIfBothDoNotHaveActualDates()
		{
			var original = Factory.NewWithValidTestData<ProcessTask>();
			var duplicate = Factory.NewWithValidTestData<ProcessTask>();

			var result = new ProcessTaskDeleteAndUpdateResolver(new Mock<ProcessTaskDeleteAndUpdateResolver.ProcessTaskConcurrencyResolverUpdate>().Object).DeleteDuplicateAndUpdateOriginalWhenRequired(original, duplicate);

			AssertEquals(1, result.RecordsMarkedForDeletion.Count());
			Assert(duplicate.IsDeleted);
			AssertEquals(original, result.ResultingRecord);
			Assert(result.Resolved);
		}

		public void TestShouldDeleteDuplicateAndUpdateOriginalIfDuplicateHasActualDateAndOriginalDoesnt()
		{
			var original = Factory.NewWithValidTestData<ProcessTask>();
			var duplicate = Factory.NewWithValidTestData<ProcessTask>();
			var actualDate = ZDateTimeOffset.Now;
			duplicate.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			((IMilestoneDateDefaultable)duplicate).ActualDate = actualDate;

			var mockUpdateFunction = new Mock<ProcessTaskDeleteAndUpdateResolver.ProcessTaskConcurrencyResolverUpdate>();
			var result = new ProcessTaskDeleteAndUpdateResolver(mockUpdateFunction.Object).DeleteDuplicateAndUpdateOriginalWhenRequired(original, duplicate);

			AssertEquals(1, result.RecordsMarkedForDeletion.Count());
			Assert(duplicate.IsDeleted);
			AssertEquals(original, result.ResultingRecord);
			Assert(result.Resolved);

			mockUpdateFunction.Verify(func => func(It.IsAny<ProcessTask>(), It.IsAny<ProcessTask>()), Times.Once);
		}

		public void TestShouldDeleteDuplicateAndKeepOriginalIfBothHaveActualDates()
		{
			var original = Factory.NewWithValidTestData<ProcessTask>();
			var duplicate = Factory.NewWithValidTestData<ProcessTask>();
			((IMilestoneDateDefaultable)duplicate).ActualDate = ZDateTimeOffset.Now;
			var targetDate = ZDateTimeOffset.Now.AddDays(1);
			((IMilestoneDateDefaultable)original).ActualDate = targetDate;

			var result = new ProcessTaskDeleteAndUpdateResolver(new Mock<ProcessTaskDeleteAndUpdateResolver.ProcessTaskConcurrencyResolverUpdate>().Object).DeleteDuplicateAndUpdateOriginalWhenRequired(original, duplicate);

			AssertEquals(1, result.RecordsMarkedForDeletion.Count());
			Assert(duplicate.IsDeleted);
			AssertEquals(original, result.ResultingRecord);
			AssertEquals(targetDate.ToZDateTime(), original.P9_ActualDate);
			Assert(result.Resolved);
		}

		public void TestResolverShouldThrowExceptionForNullInputs_DuplicateIsNull()
		{
			var original = Factory.NewWithValidTestData<ProcessTask>();
			AssertExceptionThrown<ArgumentNullException>(() =>
				new ProcessTaskDeleteAndUpdateResolver(new Mock<ProcessTaskDeleteAndUpdateResolver.ProcessTaskConcurrencyResolverUpdate>().Object).DeleteDuplicateAndUpdateOriginalWhenRequired(original, null));
		}

		public void TestResolverShouldThrowExceptionForNullInputs_OriginalIsNull()
		{
			var duplicate = Factory.NewWithValidTestData<ProcessTask>();
			AssertExceptionThrown<ArgumentNullException>(() =>
				new ProcessTaskDeleteAndUpdateResolver(new Mock<ProcessTaskDeleteAndUpdateResolver.ProcessTaskConcurrencyResolverUpdate>().Object).DeleteDuplicateAndUpdateOriginalWhenRequired(null, duplicate));
		}
	}
}
