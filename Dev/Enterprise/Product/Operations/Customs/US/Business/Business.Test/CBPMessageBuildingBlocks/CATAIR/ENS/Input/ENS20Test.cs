using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS20Test : BIRDHeaderUpdateTest
	{
		public void TestSetReconIssueToNotApplicable()
		{
			var ens20 = new ENS20();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var notifications = new NotificationCollection();

			((IBIRDHeaderRecord)ens20).Update(declaration, notifications);

			AssertEquals(ReconIssueCodeList.Codes.NotApplicable, declaration.US_OtherReconIndicator);
		}

		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			ENS20 sea20 = new ENS20();
			sea20.ImportingVesselName = "SOUTHERN STAR";
			sea20.ModeOfTransportationMOTCode = TransportModeCodes.Codes.VesselContainer;
			sea20.DistrictPortOfUnlading = "3901";
			sea20.DateOfImportation = new CargoWise.Types.ZDate(2009, 6, 1);
			sea20.BrokerReferenceNumber = "";
			sea20.ClientBranchDesignation = "01";
			sea20.VoyageFlightTripManifestNumber = "56";
			sea20.EstimatedDateOfArrival = new CargoWise.Types.ZDate(2009, 6, 10);
			sea20.LocationOfGoods = "A001";
			sea20.TradeAgreementReconciliationIndicator = "1";
			sea20.OtherReconciliationIndicator = "001";

			ENS20 air20 = new ENS20();
			air20.ModeOfTransportationMOTCode = TransportModeCodes.Codes.AirContainer;
			air20.DistrictPortOfUnlading = "3901";
			air20.DateOfImportation = new CargoWise.Types.ZDate(2009, 6, 1);
			air20.BrokerReferenceNumber = "";
			air20.ClientBranchDesignation = "01";
			air20.VoyageFlightTripManifestNumber = "345";
			air20.EstimatedDateOfArrival = new CargoWise.Types.ZDate(2009, 6, 10);
			air20.LocationOfGoods = "A001";
			air20.TradeAgreementReconciliationIndicator = "1";
			air20.OtherReconciliationIndicator = "002";

			ENS20 ftz20 = new ENS20();
			ftz20.ImportingVesselName = "FTZ001";
			ftz20.DateOfImportation = new CargoWise.Types.ZDate(2009, 6, 1);
			ftz20.BrokerReferenceNumber = "";
			ftz20.ClientBranchDesignation = "01";
			ftz20.EstimatedDateOfArrival = new CargoWise.Types.ZDate(2009, 6, 10);
			ftz20.LocationOfGoods = "A001";
			ftz20.TradeAgreementReconciliationIndicator = "1";
			ftz20.OtherReconciliationIndicator = "004";

			return new IBIRDHeaderRecord[] { sea20, air20, ftz20 };
		}

		protected override void PrepareDeclaration(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			base.PrepareDeclaration(declaration, headerRecord);

			ENS20 ens20 = (ENS20)headerRecord;

			if (ens20.ImportingVesselName.StartsWith("FTZ"))
			{
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			}
		}

		protected override System.Type GetTypeOfMessageBlock() => typeof(ENS20);

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"ImportingVesselCode",//Currently not populated in EntrySummaryMessageBuilder
				"BrokerReferenceNumber",//Entry number is an enough reference number and we decided not to store this value for outport jobs.
			};
		}

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
