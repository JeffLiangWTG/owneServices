using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(QueryMessageFilterStripBusinessObject))]
	sealed class QueryMessageFilterStripBusinessObjectTest : MQEDIMessageCommonFilterStripBusinessObjectTest
	{
		public void TestGetMessageStatusQuery()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentCompanyBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch1.GB_GC = currentCompanyPK;
			var message11 = GetNewMessageWithTypeToTest(currentCompanyBranch1, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, "ABC123");
			var message12 = GetNewMessageWithTypeToTest(currentCompanyBranch1, EDIMessage.Direction.Receive, EDIMessage.Status.Received, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse, "ABC123");
			var message21 = GetNewMessageWithTypeToTest(currentCompanyBranch1, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, "ABC124");
			Factory.Save();
			var filter = new QueryMessageFilterStripBusinessObject();
			Assert("SNT message11 matches filter", message11.MatchesFilter(filter.Filter));
			Assert("SNT message21 does not match filter", message21.MatchesFilter(filter.Filter));
			Assert("RCV message21 matches filter", message21.MatchesFilter(filter.Filter));
			Assert("RCV message21 has not response message", !message21.HasRelatedMessage);
		}

		public void TestMessageType()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, false))
			{
				var filter = new QueryMessageFilterStripBusinessObject();
				var result = filter.GetQueryMessageTypeList();
				AssertEquals("Currently 15 message types", 15, result.Count);
				AssertEquals("contains QueryQuota", true, result.ContainsCode(ApplicationIdentifierCodeList.Codes.QueryQuota));
				AssertEquals("contains QueryErrorStatistics", true, result.ContainsCode(ApplicationIdentifierCodeList.Codes.QueryErrorStatistics));
				AssertEquals("contains QueryEntrySummary", true, result.ContainsCode(ApplicationIdentifierCodeList.Codes.QueryEntrySummary));
				AssertEquals("contains ACE QueryEntrySummary", true, result.ContainsCode(ApplicationIdentifierCodeList.Codes.ACEEntrySummaryQuery));
				AssertEquals("does not contains new ACE QueryEntrySummary", false, result.ContainsCode(ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery));
				AssertEquals("contains HarmonizedTariffScheduleQuery", true, result.ContainsCode(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery));
				AssertEquals("contains AntidumpingCountervailingDutyQuery", true, result.ContainsCode(ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQuery));
				AssertEquals("contains ManufacturerNameandAddressQuery", true, result.ContainsCode(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQuery));
				AssertEquals("contains ImporterBond Query", true, result.ContainsCode(ACEApplicationIdentifierCodeList.Codes.QueryImporterBond));
				AssertEquals("contains Establishment Identifier (FDA) Query", true, result.ContainsCode(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifier));
				AssertEquals("contains User Statistics", true, result.ContainsCode(ApplicationIdentifierCodeList.Codes.UserStatistics));
				AssertEquals("contains ACE Census Warning Query", true, result.ContainsCode(ACEApplicationIdentifierCodeList.Codes.CensusWarningQuery));
				AssertEquals("contains ACE Cargo/Manifest Query", true, result.ContainsCode(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery));
				AssertEquals("contains ACE Quota Query", true, result.ContainsCode(ACEApplicationIdentifierCodeList.Codes.QuotaQuery));
				AssertEquals("contains ACE Manufacturer Name And Address Query", true, result.ContainsCode(ACEApplicationIdentifierCodeList.Codes.ManufacturerNameAndAddressQuery));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NewEntrySummaryQuery, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Now, true))
			{
				var filter = new QueryMessageFilterStripBusinessObject();
				var result = filter.GetQueryMessageTypeList();
				AssertEquals("Currently 15 message types", 15, result.Count);
				AssertEquals("does not contains ACE QueryEntrySummary", false, result.ContainsCode(ApplicationIdentifierCodeList.Codes.ACEEntrySummaryQuery));
				AssertEquals("contains new ACE QueryEntrySummary", true, result.ContainsCode(ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery));
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new QueryMessageFilterStripBusinessObject();

		protected override MQEDIMessage GetNewMessageWithSpecificBranchToTest(GlbBranch branch)
		{
			var result = Factory.New<MQEDIMessage>();
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_GB = branch.PK;
			return result;
		}

		MQEDIMessage GetNewMessageWithTypeToTest(GlbBranch branch, string transmit, string status, string messageType, string messageNumber)
		{
			var result = Factory.New<MQEDIMessage>();
			result.EM_ReceiveTransmit = transmit;
			result.EM_MessageType = messageType;
			result.EM_Status = status;
			result.EM_GB = branch.PK;
			result.EM_MessageNum = messageNumber;
			return result;
		}
	}
}
