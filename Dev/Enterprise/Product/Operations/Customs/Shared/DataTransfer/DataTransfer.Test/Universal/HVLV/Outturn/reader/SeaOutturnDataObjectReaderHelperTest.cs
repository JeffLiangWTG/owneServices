using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	sealed class SeaOutturnDataObjectReaderHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestHandlingOfUnprocessedBills()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var outturnHeader1 = Factory.BOFactory.New<CusOutturnHeader>();
				var outturn1 = Factory.BOFactory.New<CusOutturn>();
				outturn1.C5_HouseBill = "HB1";
				var outturn2 = Factory.BOFactory.New<CusOutturn>();
				outturn2.C5_HouseBill = "HB2";
				var outturn3 = Factory.BOFactory.New<CusOutturn>();
				outturn3.C5_HouseBill = "HB3";
				var outturn4 = Factory.BOFactory.New<CusOutturn>();
				outturn4.C5_HouseBill = "HB4";
				outturnHeader1.Outturns.AddRange(outturn1, outturn2, outturn3, outturn4);

				var outturnHeader2 = Factory.BOFactory.New<CusOutturnHeader>();
				var outturn5 = Factory.BOFactory.New<CusOutturn>();
				outturn5.C5_HouseBill = "HB5";
				var outturn6 = Factory.BOFactory.New<CusOutturn>();
				outturn6.C5_HouseBill = "HB6";
				outturnHeader2.Outturns.AddRange(outturn5, outturn6);

				var outturnHeader3 = Factory.BOFactory.New<CusOutturnHeader>();
				var outturn7 = Factory.BOFactory.New<CusOutturn>();
				outturn7.C5_HouseBill = "HB7";
				outturnHeader3.Outturns.Add(outturn7);

				var helper = new SeaOutturnDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
				helper.MarkUnprocessedExistingBillsFor(outturnHeader1, null);
				helper.MarkUnprocessedExistingBillsFor(outturnHeader2, null);
				helper.MarkProcessed(outturn2);
				helper.MarkProcessed(outturn3);
				helper.MarkProcessed(outturn6);
				helper.MarkProcessed(outturn7);
				foreach (var outturn in new[] { outturn1, outturn2, outturn3, outturn4, outturn5, outturn6, outturn7 })
				{
					AssertEquals("IsDeleted", false, outturn.IsDeleted);
				}

				var logger = new TestErrorLogger();
				helper.DeleteUnprocessedBillsFor(outturnHeader3, logger);
				foreach (var outturn in new[] { outturn1, outturn2, outturn3, outturn4, outturn5, outturn6, outturn7 })
				{
					AssertEquals("IsDeleted", false, outturn.IsDeleted);
				}
				AssertEquals("logger.Logs", "", logger.Logs);

				helper.DeleteUnprocessedBillsFor(outturnHeader1, logger);
				AssertEquals("outturn1.IsDeleted", true, outturn1.IsDeleted);
				AssertEquals("outturn4.IsDeleted", true, outturn4.IsDeleted);
				foreach (var outturn in new[] { outturn2, outturn3, outturn5, outturn6, outturn7 })
				{
					AssertEquals("IsDeleted", false, outturn.IsDeleted);
				}
				AssertMultilineASCIIEquals("logger.Logs", @"Information - Deleted Outturn Bill HB1 from UniversalShipment.
Information - Deleted Outturn Bill HB4 from UniversalShipment.", logger.Logs);

				logger.ClearLogs();
				helper.DeleteUnprocessedBillsFor(outturnHeader2, logger);
				AssertEquals("outturn5.IsDeleted", true, outturn5.IsDeleted);
				foreach (var outturn in new[] { outturn2, outturn3, outturn6, outturn7 })
				{
					AssertEquals("IsDeleted", false, outturn.IsDeleted);
				}
				AssertEquals("logger.Logs", "Information - Deleted Outturn Bill HB5 from UniversalShipment.", logger.Logs);
			}
		}

		public void TestHandlingOfUnprocessedBillsWhenOutturnHasParentForwardingShipment()
		{
			var forwardingShipment1 = Factory.New<ForwardingShipment>();
			forwardingShipment1.JS_UniqueConsignRef = "S800052057";

			var forwardingShipment2 = Factory.New<ForwardingShipment>();
			forwardingShipment2.JS_UniqueConsignRef = "S800052068";

			var outturnHeader = Factory.BOFactory.New<CusOutturnHeader>();
			var outturn1 = Factory.BOFactory.New<CusOutturn>();
			outturn1.C5_HouseBill = "HB1";
			outturn1.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			outturn1.C5_ParentID = forwardingShipment1.PK;

			var outturn2 = Factory.BOFactory.New<CusOutturn>();
			outturn2.C5_HouseBill = "HB2";

			var outturn3 = Factory.BOFactory.New<CusOutturn>();
			outturn3.C5_HouseBill = "HB3";
			outturn3.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			outturn3.C5_ParentID = forwardingShipment2.PK;

			outturnHeader.Outturns.AddRange(outturn1, outturn2, outturn3);

			var logger = new TestErrorLogger();
			var helper = new SeaOutturnDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			helper.MarkUnprocessedExistingBillsFor(outturnHeader, forwardingShipment1.PK);
			helper.DeleteUnprocessedBillsFor(outturnHeader, logger);

			CombineAssertions(() =>
			{
				Assert("outturn1.IsDeleted", outturn1.IsDeleted);
				Assert("outturn2.IsDeleted", outturn2.IsDeleted);
				Assert("!outturn3.IsDeleted", !outturn3.IsDeleted);
			});
		}
	}
}
