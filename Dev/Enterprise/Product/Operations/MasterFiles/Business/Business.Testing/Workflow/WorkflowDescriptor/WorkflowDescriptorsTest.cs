using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowDescriptorsTest : TransactionedTestCase
	{
		public void TestWorkflowDescriptors_ForProductivityWise_ListsMatch()
		{
			foreach (var code in WorkflowDescriptors.AllowedWorkflowDescriptorCodesForProductivityWise())
			{
				AssertEquals("These two methods should always call one another or rely on a common list", true, WorkflowDescriptors.IsAllowedForProductivityWise(code));
			}

			AssertContainsExactElementsInAnyOrder(ProductivityWiseWorkflowTypes.Concat(new[] { WorkflowDescriptors.StandAloneTaskWorkflowDescriptor }), WorkflowDescriptors.AllowedWorkflowDescriptorCodesForProductivityWise());
		}

		public void TestTryGetValue()
		{
			var list = ObjectFactory.Get<IWorkflowDescriptorList>();
			Assert("Providers exist in the list", list.Count > 10);
			foreach (CodeDescriptionPair providerCodePair in list)
			{
				WorkflowDescriptor provider = null;
				WorkflowDescriptors.TryGetValue(providerCodePair.Code, out provider);
				AssertNotNull(provider);
			}
		}

		public void TestValues()
		{
			AssertEquals("Providers exist in Values collection", true, new List<WorkflowDescriptor>(WorkflowDescriptors.Values).Count > 10);
			foreach (WorkflowDescriptor provider in WorkflowDescriptors.Values)
			{
				AssertNotNull(provider);
			}
		}

		public void TestValues_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			AssertContainsExactElementsInAnyOrder(ProductivityWiseWorkflowTypes, WorkflowDescriptors.Values.Select(w => w.Code));
		}

		public void TestValues_WhenNotInUserInteractiveMode_ShouldNotFilterList()
		{
			var completeList = (Hashtable)ObjectFactory.Get("WorkflowDescriptors");

			AssertEquals("Pre-condition: Globals.IsUserInteractive", true, Globals.IsUserInteractive);
			AssertEquals("When Globals.IsUserInteractive is true and ProductivityWiseModeEnabled is false, the list should be filtered so that ProductivityWise-only workflow types are not shown to users", completeList.Count, WorkflowDescriptors.Values.Count());

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertEquals("When Globals.IsUserInteractive is true and ProductivityWiseModeEnabled is true, the list should be filtered so that only ProductivityWise-enabled workflow types are shown to users", ProductivityWiseWorkflowTypes.Length, WorkflowDescriptors.Values.Count());

			Globals.IsUserInteractive = false;
			AssertEquals("Regardless of the value of ProductivityWiseModeEnabled, when Globals.IsUserInteractive is false, the list of valid workflow types should be un-filtered. This is so that Log Walker can continue to process events from any job type considering the available job types could differ by session depending on the startup exe argument.",
				completeList.Count, WorkflowDescriptors.Values.Count());

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			AssertEquals("Regardless of the value of ProductivityWiseModeEnabled, when Globals.IsUserInteractive is false, the list of valid workflow types should be un-filtered. This is so that Log Walker can continue to process events from any job type considering the available job types could differ by session depending on the startup exe argument.",
				completeList.Count, WorkflowDescriptors.Values.Count());
		}

		public static string[] ProductivityWiseWorkflowTypes => new[]
		{
			WorkflowDescriptors.AccPayableOrderHeaderCode,
			WorkflowDescriptors.APInvoiceCode,
			WorkflowDescriptors.ARInvoiceCode,
			WorkflowDescriptors.CampaignWorkflowDescriptorCode,
			WorkflowDescriptors.CollectionBatchCode,
			WorkflowDescriptors.CollectionOrderCode,
			WorkflowDescriptors.CommunicationWorkflowDescriptorCode,
			WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode,
			WorkflowDescriptors.GlbAccreditationAttemptWorkflowDescriptorCode,
			WorkflowDescriptors.GlbGroupWorkflowDescriptorCode,
			WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode,
			WorkflowDescriptors.GlbStaffDescriptorCode,
			WorkflowDescriptors.GlbStaffHolidayDescriptorCode,
			WorkflowDescriptors.HRCampaignWorkflowDescriptorCode,
			WorkflowDescriptors.HRHiringRequestDescriptorCode,
			WorkflowDescriptors.HRJobApplicationWorkflowDescriptorCode,
			WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode,
			WorkflowDescriptors.HRRecruitmentJobCampaignWorkflowDescriptorCode,
			WorkflowDescriptors.OpportunityWorkflowDescriptorCode,
			WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode,
			WorkflowDescriptors.ProjectWorkflowDescriptorCode,
			WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode,
			WorkflowDescriptors.WorkItemWorkflowDescriptorCode,
		};

		public void TestValuesSorted()
		{
			var rangedDescriptors = new Dictionary<string, int>();
			WorkflowDescriptors.Values.Aggregate(1, (i, descriptor) =>
			{
				rangedDescriptors.Add(descriptor.Code, i);
				return ++i;
			});

			int bookingIndex = rangedDescriptors[WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode];
			int billOfLadingIndex = rangedDescriptors[WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode];
			int orderIndex = rangedDescriptors[WorkflowDescriptors.OrderWorkflowDescriptorCode];

			Assert(bookingIndex < billOfLadingIndex);
			Assert(billOfLadingIndex < orderIndex);
		}

		[ExpectNoExceptions]
		public void TestDuplicateDictKey()
		{
			recurseAndNull(5, WorkflowDescriptors);
			AssertEquals("DuplicateWorkflowCodeAdded", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		WorkflowDescriptor recurseAndNull(int n, WorkflowDescriptors instance)
		{
			if (n > 0)
			{
				return instance.GetWorkflowDescriptorFromAnyCreator("SUP", () => { return recurseAndNull(n - 1, instance); });
			}
			return null;
		}

		public void TestTryGetValueSafe()
		{
			string wrongCode = ")(_*#*(";
			AssertNull(WorkflowDescriptors.TryGetValueSafe(wrongCode));
			AssertNotNull(WorkflowDescriptors.TryGetValueSafe(WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode));
		}

		WorkflowDescriptors WorkflowDescriptors
		{
			get { return WorkflowDescriptors.Instance; }
		}
	}
}
