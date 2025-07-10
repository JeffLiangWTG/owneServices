using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS89Test : BIRDHeaderUpdateTest
	{
		[TestDate(2011, 09, 14)]
		public void TestEntryLevelMPFPersistOK()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888813-26220360013-262203600                 8         XJ5 6001874001891  DC 20     ADMIRALENGRACHT     102809010409B00003289            56   010409    1    22            YOUIER8907                          00000015PK         APLU       30                                  0               1                   APLU808 40001AU00000100000000000000                    000000005060267                  50 44219097200000033000000001500000GR                               AU010409N   51                                                                              60                                        XYBEREQU6LON                          62          49900002100                                                         8949900000002500                                                                9000000033000           0                       0000000250000000010000          ZZ7501000000010                                                                 ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(10000m, declaration.InvoiceLines[0].JI_LinePrice);
			AssertEquals(21m, declaration.InvoiceLines[0].FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(25m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);
		}

		protected override IBIRDHeaderRecord[] GetPopulatedHeaderRecords()
		{
			var ens89 = new ENS89();
			ens89.ClassCode = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			ens89.TotalAmount = 25m;

			ens89.ClassCode1 = Core.Constants.USCustoms.FeeCodes.Avocado;
			ens89.TotalAmount1 = 10.10m;

			ens89.ClassCode2 = Core.Constants.USCustoms.FeeCodes.Beef;
			ens89.TotalAmount2 = 20.20m;

			ens89.ClassCode3 = Core.Constants.USCustoms.FeeCodes.Blueberry;
			ens89.TotalAmount3 = 40.44m;

			ens89.ClassCode4 = Core.Constants.USCustoms.FeeCodes.Cotton;
			ens89.TotalAmount4 = 55.55m;

			return new IBIRDHeaderRecord[] { ens89 };
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS89);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration, IBIRDHeaderRecord headerRecord)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
