using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	[TestedType(typeof(QuotedBookingSupporter))]
	public class QuotedBookingSupporterTest : OperationalActionSupporterTest<QuotedBookingSupporter>
	{
		#region TestInvoicingCheckpoint

		public void TestInvoicingCheckpoint()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.QuickBookingJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}

		#endregion

		#region TestActionMethodGroups

		public void TestActionMethodGroups()
		{
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods",
				p => p.Name,
				new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Accounting },
				Supporter.Methods.GetAllIds());
		}

		#endregion

		protected override ModuleIdentifier ModuleID => ModuleIDs.QuotedBookings;

		public override bool ShouldSupportDocuments => true;
		public override void TestModuleCorrectlySupportsOperationalActions()
		{
			// Override the base assertion: Supporter.RootType is Module.GridCollection.TypeOfElements
			// QuotedBookingSupporter does not implement IDocumentSupportable
			AssertEquals("RootType RuntimeType", nameof(QuotedBooking), Supporter.RootType.Name);
			AssertEquals("GridCollection items RuntimeType", nameof(ViewQuotedBooking), Module.GridCollection.TypeOfElements.Name);
		}

		#region Implementation

		public override BusinessObject NewTarget()
		{
			return QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
		}

		#endregion
	}
}
