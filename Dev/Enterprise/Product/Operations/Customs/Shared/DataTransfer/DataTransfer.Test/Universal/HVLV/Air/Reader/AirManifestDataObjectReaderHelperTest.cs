using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	sealed class AirManifestDataObjectReaderHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestHandlingOfUnprocessedBills()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var mawb1 = (CusMAWB)Factory.BOFactory.New<Integration.Customs.AU.ICusMAWB>();
				var hawb1 = mawb1.ChildBills.AddNew();
				hawb1.CS_HAWB = "HB1";
				var hawb2 = mawb1.ChildBills.AddNew();
				hawb2.CS_HAWB = "HB2";
				var hawb3 = mawb1.ChildBills.AddNew();
				hawb3.CS_HAWB = "HB3";
				var hawb4 = mawb1.ChildBills.AddNew();
				hawb4.CS_HAWB = "HB4";
				hawb4.CS_MsgStatus = Common.AU.CMR.CMRBaseStatuses.Codes.OriginalAccepted;
				Assert("PreCondition", !hawb4.CanDelete);

				var mawb2 = (CusMAWB)Factory.BOFactory.New<Integration.Customs.AU.ICusMAWB>();
				var hawb5 = mawb2.ChildBills.AddNew();
				hawb5.CS_HAWB = "HB5";
				var hawb6 = mawb2.ChildBills.AddNew();
				hawb6.CS_HAWB = "HB6";

				var mawb3 = (CusMAWB)Factory.BOFactory.New<Integration.Customs.AU.ICusMAWB>();
				var hawb7 = mawb3.ChildBills.AddNew();
				hawb7.CS_HAWB = "HB7";

				var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
				helper.MarkUnprocessedExistingBillsFor(mawb1);
				helper.MarkUnprocessedExistingBillsFor(mawb2);
				helper.MarkProcessed(hawb2);
				helper.MarkProcessed(hawb3);
				helper.MarkProcessed(hawb6);
				helper.MarkProcessed(hawb7);
				foreach (var hawb in new[] { hawb1, hawb2, hawb3, hawb4, hawb5, hawb6, hawb7 })
				{
					AssertEquals("IsDeleted", false, hawb.IsDeleted);
				}

				var logger = new TestErrorLogger();
				helper.DeleteUnprocessedBillsFor(mawb3, logger);
				foreach (var hawb in new[] { hawb1, hawb2, hawb3, hawb4, hawb5, hawb6, hawb7 })
				{
					AssertEquals("IsDeleted", false, hawb.IsDeleted);
				}
				AssertEquals("logger.Logs", "", logger.Logs);

				helper.DeleteUnprocessedBillsFor(mawb1, logger);
				AssertEquals("hawb1.IsDeleted", true, hawb1.IsDeleted);
				foreach (var hawb in new[] { hawb2, hawb3, hawb4, hawb5, hawb6, hawb7 })
				{
					AssertEquals("IsDeleted", false, hawb.IsDeleted);
				}

				ZString reason = hawb4.ReasonForNotAbleToDelete;
				AssertContains(reason.Left(1).ToLower() + reason.SubstringSafe(1), logger.Logs);

				logger.ClearLogs();
				helper.DeleteUnprocessedBillsFor(mawb2, logger);
				AssertEquals("hawb5.IsDeleted", true, hawb5.IsDeleted);
				foreach (var hawb in new[] { hawb2, hawb3, hawb6, hawb7 })
				{
					AssertEquals("IsDeleted", false, hawb.IsDeleted);
				}
				AssertEquals("logger.Logs", "Information - Deleted Air Cargo House (HAWB: HB5) from UniversalShipment.", logger.Logs);
			}
		}
	}
}
