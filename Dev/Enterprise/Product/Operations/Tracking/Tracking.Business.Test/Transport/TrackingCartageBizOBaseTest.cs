using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCartage))]
	sealed class TrackingCartageBizOBaseTest : EnterpriseBusinessObjectTestCase
	{
		#region Milestones

		public void TestMilestones()
		{
			var testCartage = Factory.NewWithValidTestData<TrackingCartage>();
			AssertNotNull(testCartage.Milestones);
			AssertEquals(0, testCartage.Milestones.Count);

			var milestone1 = ((IWorkflowProvider)testCartage).WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = ((IWorkflowProvider)testCartage).WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testCartage.Factory.Save();
			AssertEquals(0, testCartage.Milestones.Count);

			testCartage.ReloadMilestones();
			AssertEquals(2, testCartage.Milestones.Count);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<TrackingCartage>();
		}

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonCartage);
			}
		}
	}
}
