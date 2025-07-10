using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS34Test : BIRDHeaderUpdateTest
	{
		public void TestWithInformalFee()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001767611891  DC 20     APL EMERALD         102809                           V123W               22            OBL66                               00000001PK         AAAA       30                                  0 P             1                   AAAA    3431100000200                                                                   40001HK00000030000000009000                    000000005060267                  50 74199930000000009000            KG                               AU120908    51                                                                              521                                                                             60                                        KNBEREQU6LON                          8931100000000200                                                                9000000009000           0                       0000000020000000003000          ZZ7501000000010                                                                 ";
			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("Informal fee is persisted OK", 2m, declaration.ActiveEntryHeaders.EntrySummaryEntry.InformalFee);
		}

		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			ENS34 ens34 = new ENS34();
			ens34.ClassCode = Core.Constants.USCustoms.FeeCodes.MerchandiseInformal;
			ens34.Amount = 5m;

			ens34.ClassCode1 = Core.Constants.USCustoms.FeeCodes.DutiableMail;
			ens34.Amount1 = 6m;

			ens34.ClassCode2 = Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge;
			ens34.Amount2 = 5.99m;

			return new IBIRDHeaderRecord[] { ens34 };
		}

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"ClassCode3",//There are only three types of header fees and this field would never be populated
				"Amount3",//There are only three types of header fees and this field would never be populated

				"ClassCode4",//There are only three types of header fees and this field would never be populated
				"Amount4",//There are only three types of header fees and this field would never be populated

				"ClassCode5",//There are only three types of header fees and this field would never be populated
				"Amount5",//There are only three types of header fees and this field would never be populated
			};
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS34);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
