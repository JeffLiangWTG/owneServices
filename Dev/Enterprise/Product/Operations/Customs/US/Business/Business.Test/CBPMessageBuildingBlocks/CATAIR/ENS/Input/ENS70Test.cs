using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS70Test : BIRDSecondaryLineUpdateTest
	{
		public void TestSetValueAtAlternativeTariff()
		{
			var messageText =
"AA7501SV93901B00161100                  20131203184818                          " +
"10A110154-10814150054-108141500                 8         SV9 7003389701856  MD " +
"20     FTZ0219               4501091213                          091213H872 001 " +
"30                                                  6                           " +
"40001JP00000050000000000120                    000000005058866                  " +
"50 82152000000000034000000000500000PCS                              JP120313N   " +
"60                                        JPACETAC288OSA                        " +
"62          50100000625                                                         " +
"62          49900001732                                                         " +
"708215993500           000000500000PCS                                          " +
"895010000000062549900000002500                                                  " +
"9000000034000           0                       0000000312500000005000          " +
"ZAEPA TEST IMPORTER                  100 MAIN STREET                            " +
"ZBGREENVILLE                    SC296001                                        " +
"ZI001USD00000500000100000000000000000 00000000000                               " +
"ZZ75011128                                                                      ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var notifications = new NotificationCollection();
			var declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(2, declaration.InvoiceLines.Count);

			var firstLine = declaration.InvoiceLines[0];
			AssertNotNull("PreCondition", firstLine.ImportTariff);
			AssertEquals("PreCondition", ComputationCodeList.Codes.Derived, firstLine.ImportTariff.UE_DutyComputationCode);

			AssertEquals("Line price should not have been set to the first line", 0m, firstLine.JI_LinePrice);

			var secondLine = declaration.InvoiceLines[1];
			AssertNotEquals("Line price should not have been set to the first line", 0m, secondLine.JI_LinePrice);
		}

		protected override IBIRDSecondaryLineRecord[] GetPopulatedSecondaryLineRecords()
		{
			ENS70 ens70 = new ENS70();

			ens70.TariffNumber2 = "0000000000";
			ens70.Duty = 89.88m;
			ens70.Quantity1 = 123456m;
			ens70.Unit1 = "KG";
			ens70.Quantity2 = 23456m;
			ens70.Unit2 = "L";
			ens70.Quantity3 = 234m;
			ens70.Unit3 = "BO";
			ens70.Value = 9999m;
			ens70.SpecialProgramsIndicatorPrimaryOrCountry = "E";
			ens70.SpecialProgramsIndicatorSecondary = "F";

			return new IBIRDSecondaryLineRecord[] { ens70 };
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine secondaryLine, IBIRDSecondaryLineRecord lineRecord)
		{
			base.PrepareData(declaration, invoice, secondaryLine, lineRecord);

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "L";
			tariff.UE_Unit3 = "BO";
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS70);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
