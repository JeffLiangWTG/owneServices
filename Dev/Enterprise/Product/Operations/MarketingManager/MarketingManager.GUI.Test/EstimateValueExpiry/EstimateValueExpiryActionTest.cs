using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(EstimateValueExpiryAction))]
	public class EstimateValueExpiryActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetExpiry()
		{
			var detail = PrepareTradeDetailForTest();

			detail.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 2, 22);
			AssertEquals("Pre-condition", ZDate.Empty, detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("Pre-condition", ZString.Empty, detail.ProspectDetail.PAP_ExpiryReason);

			var action = new EstimateValueExpiryAction(new OrgTradeDetail[] { detail }, EstimateValueExpiryAction.ActionType.SetExpiry)
			{
				ExpiryDate = new ZDate(2018, 6, 22),
				ExpiryReason = "MAN"
			};

			AssertEquals("No change yet", ZDate.Empty, detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("No change yet", ZString.Empty, detail.ProspectDetail.PAP_ExpiryReason);

			action.Apply();

			AssertEquals("Expiry info has been updated", new ZDate(2018, 6, 22), detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("Expiry info has been updated", "MAN", detail.ProspectDetail.PAP_ExpiryReason);
		}

		public void TestUndoExpiry()
		{
			var detail = PrepareTradeDetailForTest();
			detail.ProspectDetail.PAP_ExpiryDate = new ZDate(2018, 6, 22);
			detail.ProspectDetail.PAP_ExpiryReason = OrgTradeProspectExpiryReasonList.Codes.Lost;

			AssertEquals("Expiry undo is not allowed", false, detail.ProspectDetail.AllowManualExpiry);
			AssertEquals("Pre-condition", new ZDate(2018, 6, 22), detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("Pre-condition", OrgTradeProspectExpiryReasonList.Codes.Lost, detail.ProspectDetail.PAP_ExpiryReason);

			var action = new EstimateValueExpiryAction(new OrgTradeDetail[] { detail }, EstimateValueExpiryAction.ActionType.UndoExpiry);

			AssertEquals("No change yet", new ZDate(2018, 6, 22), detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("No change yet", OrgTradeProspectExpiryReasonList.Codes.Lost, detail.ProspectDetail.PAP_ExpiryReason);

			action.Apply();

			AssertEquals("No change as system expiry does not allow undo", new ZDate(2018, 6, 22), detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("No change as system expiry does not allow undo", OrgTradeProspectExpiryReasonList.Codes.Lost, detail.ProspectDetail.PAP_ExpiryReason);

			detail.ProspectDetail.PAP_ExpiryReason = "MAN";
			AssertEquals("Expiry undo is allowed", true, detail.ProspectDetail.AllowManualExpiry);
			action.Apply();
			AssertEquals("Expiry info has been updated", ZDate.Empty, detail.ProspectDetail.PAP_ExpiryDate);
			AssertEquals("Expiry info has been updated", ZString.Empty, detail.ProspectDetail.PAP_ExpiryReason);
		}

		OrgTradeDetail PrepareTradeDetailForTest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			sales.OW_IsTraded = false;
			var detail = sales.TradeDetails.AddNew();

			detail.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._12Months;
			detail.ProspectPeriodStart = new ZDate(2018, 3, 1);
			detail.ProspectPeriodEnd = new ZDate(2019, 2, 1);

			detail.PA_Status = OpportunityTradeStatus.Codes.Successful;

			Factory.Save();

			return detail;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var detail = PrepareTradeDetailForTest();
			return new EstimateValueExpiryAction(new OrgTradeDetail[] { detail }, EstimateValueExpiryAction.ActionType.SetExpiry);
		}
	}
}
