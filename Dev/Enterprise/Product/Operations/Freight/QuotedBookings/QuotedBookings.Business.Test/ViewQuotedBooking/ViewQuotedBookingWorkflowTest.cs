using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ViewQuotedBooking))]
	public class ViewQuotedBookingWorkflowTest : WorkflowProviderTest<ViewQuotedBooking, QuotedBookingProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode; }
		}

		protected override ViewQuotedBooking GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return CreateQuotedBooking(factory);
		}

		public new void TestCreateTaskAndSave()
		{
			Assert("It's a view!", true);
		}

		protected override Type ParentProxyType => typeof(QuotedBooking);

		protected override BusinessObject GetParent(ViewQuotedBooking bizo)
		{
			return bizo.QuotedBooking;
		}

		protected override string GetPropertyNameForReleaseGroupRulesTest(BusinessObject job) => nameof(QuotedBooking.Origin);

		public new void TestProcessTasksCascadeDeleted()
		{
			Assert("Deletion of the ViewQuotedBooking is not supported", true);
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			Assert("It's a view!", true);
		}

		protected override IWorkflowProvider ReloadWorkflowProvider(BusinessObjectFactory factory, ViewQuotedBooking workFlowProvider)
		{
			ViewQuotedBooking viewQuotedBooking = factory.New<ViewQuotedBooking>();
			viewQuotedBooking.VB_JS = workFlowProvider.VB_JS;
			viewQuotedBooking.VB_TH = workFlowProvider.VB_TH;
			return viewQuotedBooking;
		}

		internal static ViewQuotedBooking CreateQuotedBooking(BusinessObjectFactory factory)
		{
			var viewQuotedBooking = factory.New<ViewQuotedBooking>();
			var quote = QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(factory);

			viewQuotedBooking.VB_JS = booking.PK;
			viewQuotedBooking.VB_TH = quote.PK;

			return viewQuotedBooking;
		}
	}

	[TestedType(typeof(ViewQuotedBooking))]
	public class ViewQuotedBookingWorkflowTest2 : WorkflowProviderTest<ViewQuotedBooking, ProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode; }
		}

		protected override BusinessObject GetParent(ViewQuotedBooking bizo) => bizo.QuotedBooking;

		protected override Type ParentProxyType => typeof(QuotedBooking);

		protected override string GetPropertyNameForReleaseGroupRulesTest(BusinessObject job) => nameof(QuotedBooking.Origin);

		public new void TestCreateTaskAndSave()
		{
			Assert("It's a view!", true);
		}

		public new void TestProcessTasksCascadeDeleted()
		{
			Assert("Deletion of the ViewQuotedBooking is not supported", true);
		}

		public override void TestProcessTasksCreatedOnSave()
		{
			Assert("It's a view!", true);
		}

		public override void TestProcessTasksAreSavedThenReloaded()
		{
			Assert("It's a view!", true);
		}

		protected override ViewQuotedBooking GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return ViewQuotedBookingWorkflowTest.CreateQuotedBooking(factory);
		}
	}
}
