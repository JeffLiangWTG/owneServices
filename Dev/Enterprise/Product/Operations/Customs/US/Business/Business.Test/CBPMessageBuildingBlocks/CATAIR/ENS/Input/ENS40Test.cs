using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS40Test : BIRDLineUpdateTest
	{
		public void TestMultiplePortOfLadingsOnLineLevel()
		{
			var messageText =
"AA7501                                  201305021139570100                      " +
"10A1502995201-0067156-219873100                 8042913   OHL 0115724106036  NC " +
"20     FTZ0219               1502042913                          042913L844     " +
"30                                                  6                           " +
"40001HN0000000228000000002900000000000000000000000000000352000N         INV001  " +
"50P6101200010          000000000400DOZ000000002900KG                HN      Y   " +
"51                                             308276124                        " +
"60                                        HNGILACT6VIL                          " +
"62          49900000000                                                         " +
"40002HN0000000084000000001300000000000000000000000000000162001N         INV001  " +
"50P6103431520          000000000300DOZ000000001300KG                HN      Y   " +
"51                                             308276124                        " +
"60                                        HNGILACT6VIL                          " +
"62          49900000000                                                         " +
"40003BD0000001001000000006400000000000000000000000000001335601N         INV001  " +
"50 6109100012          000000003000DOZ000000006400KG                BD      N   " +
"60                                        BDRIPKNIGAZ                           " +
"62          49900000347                                                         " +
"40004VN0000000726000000006900000000000000000000000000000952002N         INV001  " +
"50 6307909882          000000043200NO 000000006900KG                VN      N   " +
"60                                        VNPHOPHU09HOC                         " +
"62          49900000251                                                         " +
"40005VN0000000726000123456700000000000000000000000000000952002N         INV001  " +
"50 6307909567          000000043200NO 000123456700KG                VN      N   " +
"40006VN0000000726000099999900000000000000000000000000000952002N         INV001  " +
"50 6307909999          000000043200NO 000099999900KG                VN      N   " +
"89499                                                                           " +
"9000000626830           0 00000000000000000000000000000733200000872923          " +
"ZAGildan Activewear SRL              Newton Industrial Park                     " +
"ZBChrist Church                 NCBB17047                                       " +
"ZI001USD00000626830100000000000000000 00000000000                               " +
"ZZ7501000000432                                                                 ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var notifications = new NotificationCollection();
			var declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(6, declaration.InvoiceLines.Count);
			var line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6101200010");
			AssertEquals("52000", line.US_SchDLoading);

			line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6103431520");
			AssertEquals("62001", line.US_SchDLoading);

			line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6109100012");
			AssertEquals("35601", line.US_SchDLoading);

			line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6307909882");
			AssertEquals("52002", line.US_SchDLoading);

			line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6307909567");
			AssertEquals(1234.567m, line.JI_Weight);
			AssertEquals(Core.Constants.Weight.Tonnes, line.JI_WeightUQ);

			line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Tariff == "6307909999");
			AssertEquals(999999m, line.JI_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, line.JI_WeightUQ);
		}

		public void TestIBIRDLineIDRecord()
		{
			var ens40 = new ENS40();
			ens40.InvoiceDelimiter = "INV002";
			ens40.LineItemNumber = 3;

			AssertEquals(3, ((IBIRDLineIDRecord)ens40).LineNumber);
			AssertEquals(2, ((IBIRDLineIDRecord)ens40).DelimiterInvSequence);

			ens40.InvoiceDelimiter = "";
			AssertEquals(0, ((IBIRDLineIDRecord)ens40).DelimiterInvSequence);
		}

		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			ENS40 ens40 = new ENS40();
			ens40.LineItemNumber = 1;
			ens40.CountryOfOrigin = "AU";
			ens40.Value = 20000m;
			ens40.GrossWeight = 30m;
			ens40.ADDSpecificDepositValue = 5000m;
			ens40.CVDSpecificDepositValue = 6000m;
			ens40.Charges = 50m;
			ens40.PortOfLading = "61010";
			ens40.ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			ens40.PrivilegedStatusFilingDate = new ZDate(2009, 1, 3);
			ens40.NAFTANetCostIndicator = "Y";

			return new IBIRDLineRecord[] { ens40 };
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
			base.PrepareData(declaration, invoice, invoiceLine, lineRecord);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine.US_ADDCaseNo = "A";
			invoiceLine.US_CVDCaseNo = "C";
		}

		protected override System.Type GetTypeOfMessageBlock() => typeof(ENS40);

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"SoftwoodLumber",//This is not populated and the permit number along with other details are sent in a different record
				"InvoiceDelimiter",//This is tested in an end-to-end test.
			};
		}

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
