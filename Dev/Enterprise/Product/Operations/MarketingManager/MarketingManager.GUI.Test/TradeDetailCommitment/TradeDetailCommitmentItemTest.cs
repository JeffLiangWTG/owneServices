using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailCommitmentItem))]
	public class TradeDetailCommitmentItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInitAndConfirmChanges()
		{
			var oppStatusCollection = new OpportunityStatusCollection
			{
				{ "AAA", (NoResString)"Desc A", false, true, true, OpportunityTradeStatus.Codes.Active },
				{ "BBB", (NoResString)"Desc B", false, true, true, OpportunityTradeStatus.Codes.Unsuccessful },
				{ "CCC", (NoResString)"Desc C", false, true, true, OpportunityTradeStatus.Codes.Successful }
			};
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oppStatusCollection);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tradeLane = org.SalesCollection.AddNew();
			var tradeDetail = tradeLane.TradeDetails.AddNew();
			tradeDetail.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 3, 21);
			tradeDetail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			Factory.Save();

			var item = new TradeDetailCommitmentItem(tradeDetail, "CCC");
			AssertEquals(OpportunityTradeStatus.Codes.Successful, item.TradeDetailStatus);
			AssertEquals(new ZDate(2018, 3, 21), item.ExpectedTradeStartDate);
			AssertEquals(OrgTradeProspectPeriodEndTypeList.Codes._12Months, item.ProspectPeriodEndType);
			AssertEquals(new ZDate(2018, 3, 1), item.ProspectPeriodStart);
			AssertEquals(new ZDate(2019, 2, 1), item.ProspectPeriodEnd);
			AssertEquals(OrgTradeProspectForecastTypeList.Descriptions.Static, item.ForecastTypeDescription);

			item.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes._3Months;
			AssertEquals(new ZDate(2018, 3, 1), item.ProspectPeriodStart);
			AssertEquals(new ZDate(2018, 5, 1), item.ProspectPeriodEnd);

			item.ConfirmChange();
			Factory.Save();

			var loadedTradeDetail = new BusinessObjectFactory().Load<OrgTradeDetail>(tradeDetail.PK);

			var item2 = new TradeDetailCommitmentItem(loadedTradeDetail, "CCC");
			AssertEquals(OpportunityTradeStatus.Codes.Successful, item2.TradeDetailStatus);
			AssertEquals(new ZDate(2018, 3, 21), item2.ExpectedTradeStartDate);
			AssertEquals(OrgTradeProspectPeriodEndTypeList.Codes._3Months, item2.ProspectPeriodEndType);
			AssertEquals(new ZDate(2018, 3, 1), item2.ProspectPeriodStart);
			AssertEquals(new ZDate(2018, 5, 1), item2.ProspectPeriodEnd);
			AssertEquals(OrgTradeProspectForecastTypeList.Descriptions.Static, item2.ForecastTypeDescription);
		}

		public void TestInit_UnsuccessfulTradeDetail()
		{
			var oppStatusCollection = new OpportunityStatusCollection
			{
				{ "AAA", (NoResString)"Desc A", false, true, true, OpportunityTradeStatus.Codes.Active },
				{ "BBB", (NoResString)"Desc B", false, true, true, OpportunityTradeStatus.Codes.Unsuccessful },
				{ "CCC", (NoResString)"Desc C", false, true, true, OpportunityTradeStatus.Codes.Successful }
			};
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oppStatusCollection);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tradeLane = org.SalesCollection.AddNew();
			var tradeDetail = tradeLane.TradeDetails.AddNew();
			tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
			tradeDetail.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 4, 26);
			tradeDetail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			Factory.Save();

			var item = new TradeDetailCommitmentItem(tradeDetail, "CCC");
			AssertEquals(OpportunityTradeStatus.Codes.Unsuccessful, item.TradeDetailStatus);
			AssertEquals(new ZDate(2018, 4, 26), item.ExpectedTradeStartDate);
			AssertEquals(ZString.Empty, item.ProspectPeriodEndType);
			AssertEquals(ZDate.Empty, item.ProspectPeriodStart);
			AssertEquals(ZDate.Empty, item.ProspectPeriodEnd);
			AssertEquals(ZString.Empty, item.ForecastTypeDescription);
		}

		public void TestTradeDetailStatus()
		{
			var oppStatusCollection = new OpportunityStatusCollection
			{
				{ "AAA", (NoResString)"Desc A", false, true, true, OpportunityTradeStatus.Codes.Active },
				{ "BBB", (NoResString)"Desc B", false, true, true, OpportunityTradeStatus.Codes.Unsuccessful },
				{ "CCC", (NoResString)"Desc C", false, true, true, OpportunityTradeStatus.Codes.Successful }
			};
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oppStatusCollection);

			var tradeDetail = Factory.New<OrgTradeDetail>();
			tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;

			var item = new TradeDetailCommitmentItem(tradeDetail, "BBB");
			AssertEquals(OpportunityTradeStatus.Codes.Unsuccessful, item.TradeDetailStatus);
			AssertEquals(ZDate.Empty, item.ExpectedTradeStartDate);

			item.TradeDetailStatus = ZString.Empty;
			AssertHasErrors(item.TradeDetailStatusInfo);

			item = new TradeDetailCommitmentItem(tradeDetail, "AAA");
			item.TradeDetailStatus = OpportunityTradeStatus.Codes.Successful;
			AssertHasErrors(item.TradeDetailStatusInfo);

			item.TradeDetailStatus = OpportunityTradeStatus.Codes.Active;
			AssertNoErrors(item.TradeDetailStatusInfo);

			item.ConfirmChange();
			AssertEquals(OpportunityTradeStatus.Codes.Active, tradeDetail.PA_Status);

			tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			var prospectPeriod = tradeDetail.ProspectPeriods.AddNew();
			prospectPeriod.PAS_Period = new ZDate(2018, 3, 1);
			prospectPeriod.PAS_IsSuperseded = true;

			item = new TradeDetailCommitmentItem(tradeDetail, "CCC");
			AssertEquals(OpportunityTradeStatus.Codes.Successful, item.TradeDetailStatus);
			AssertEquals("Successful (Superseded)", item.TradeDetailStatusDescription);
			AssertEquals(true, item.TradeDetailStatusDescription_ReadOnly);
		}

		public void TestProspectEndDateValidation()
		{
			var oppStatusCollection = new OpportunityStatusCollection
			{
				{ "CCC", (NoResString)"Desc C", false, true, true, OpportunityTradeStatus.Codes.Successful }
			};
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oppStatusCollection);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tradeLane = org.SalesCollection.AddNew();
			var tradeDetail = tradeLane.TradeDetails.AddNew();
			tradeDetail.ProspectDetail.PAP_ExpectedTradeStartDate = new ZDate(2018, 3, 21);
			tradeDetail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Successful;
			Factory.Save();

			var item = new TradeDetailCommitmentItem(tradeDetail, "CCC");
			item.ProspectPeriodEndType = OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter;
			item.ProspectPeriodStart = ZDate.Empty;
			item.ProspectPeriodEnd = ZDate.Empty;

			AssertHasError("Start Period Enter Validation", item.ProspectPeriodStartInfo, "Please enter a Start Period.");
			AssertHasError("End Period Enter Validation", item.ProspectPeriodEndInfo, "Please enter an End Period.");

			item.ProspectPeriodStart = new ZDate(2018, 3, 3);
			item.ProspectPeriodEnd = new ZDate(2018, 3, 2);

			AssertNoErrors("Valid Start Date", item.ProspectPeriodStartInfo);
			AssertHasError("Invalid End Date", item.ProspectPeriodEndInfo, "The End Period must be later than the Start Period");

			item.ProspectPeriodStart = new ZDate(2018, 3, 4);
			AssertHasError("Invalid Start Date", item.ProspectPeriodStartInfo, "The Start Period must be earlier than the End Period");

			item.ProspectPeriodEnd = new ZDate(2018, 3, 5);
			item.RunPreSaveValidation();

			AssertNoErrors("Valid Start Date", item.ProspectPeriodStartInfo);
			AssertNoErrors("Valid End Date", item.ProspectPeriodEndInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var tradeDetail = Factory.New<OrgTradeDetail>();
			return new TradeDetailCommitmentItem(tradeDetail, "WON");
		}
	}
}
