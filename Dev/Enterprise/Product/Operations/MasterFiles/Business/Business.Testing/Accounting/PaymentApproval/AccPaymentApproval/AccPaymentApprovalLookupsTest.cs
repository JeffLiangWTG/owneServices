using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccPaymentApprovalLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHeaders()
		{
			var approval = Factory.New<AccPaymentApproval>();

			AssertNotNull("OrgHeaders should not be null", approval.Lookups.Headers);
			AssertEquals("OrgHeaders Type", typeof(CreditorCollection), approval.Lookups.Headers.GetType());
		}

		public void TestAV_LedgersList()
		{
			var approval = Factory.New<AccPaymentApproval>();

			AssertNotNull("LedgerList should not be null", approval.Lookups.AV_LedgerList);
			AssertEquals("LedgerList.Count", 2, approval.Lookups.AV_LedgerList.Count);

			AssertEquals("1st Element (Code)", LedgerTypes.AccountsPayable, approval.Lookups.AV_LedgerList[0].Code);
			AssertEquals("1st Element (Description)", AccPaymentApprovalLookups.AccountsPayableDescription, approval.Lookups.AV_LedgerList[0].Description);

			AssertEquals("2nd Element (Code)", LedgerTypes.AccountsReceivable, approval.Lookups.AV_LedgerList[1].Code);
			AssertEquals("2nd Element (Description)", AccPaymentApprovalLookups.AccountsReceivableDescription, approval.Lookups.AV_LedgerList[1].Description);
		}

		public void TestStatusList()
		{
			var approval = Factory.New<AccPaymentApproval>();

			AssertNotNull("StatusList should not be null", approval.Lookups.AV_StatusList);
			var statusListOrigin = AccPaymentApprovalLookups.GetStatusList();
			AssertArrayEqualsByElements(statusListOrigin.ToArray(), approval.Lookups.AV_StatusList.ToArray());
			AssertEquals("StatusList.Count", 6, approval.Lookups.AV_StatusList.Count);

			AssertEquals("1st Element (Code)", PaymentApprovalStatus.AwaitingApproval, approval.Lookups.AV_StatusList[0].Code);
			AssertEquals("1st Element (Description)", AccPaymentApprovalLookups.AwaitingApprovalDescription, approval.Lookups.AV_StatusList[0].Description);

			AssertEquals("2nd Element (Code)", PaymentApprovalStatus.FullyApproved, approval.Lookups.AV_StatusList[1].Code);
			AssertEquals("2nd Element (Description)", AccPaymentApprovalLookups.FullyApprovedDescription, approval.Lookups.AV_StatusList[1].Description);

			AssertEquals("3rd Element (Code)", PaymentApprovalStatus.Posted, approval.Lookups.AV_StatusList[2].Code);
			AssertEquals("3rd Element (Description)", AccPaymentApprovalLookups.PostedDescription, approval.Lookups.AV_StatusList[2].Description);

			AssertEquals("4th Element (Code)", PaymentApprovalStatus.Rejected, approval.Lookups.AV_StatusList[3].Code);
			AssertEquals("4th Element (Description)", AccPaymentApprovalLookups.RejectedDescription, approval.Lookups.AV_StatusList[3].Description);

			AssertEquals("5th Element (Code)", PaymentApprovalStatus.Cancelled, approval.Lookups.AV_StatusList[4].Code);
			AssertEquals("5th Element (Description)", AccPaymentApprovalLookups.CancelledDescription, approval.Lookups.AV_StatusList[4].Description);

			AssertEquals("6th Element (Code)", PaymentApprovalStatus.Draft, approval.Lookups.AV_StatusList[5].Code);
			AssertEquals("6th Element (Description)", AccPaymentApprovalLookups.DraftDescription, approval.Lookups.AV_StatusList[5].Description);
		}
	}
}
