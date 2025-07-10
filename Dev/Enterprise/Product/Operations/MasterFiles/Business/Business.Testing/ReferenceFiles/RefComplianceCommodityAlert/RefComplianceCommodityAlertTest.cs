using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefComplianceCommodityAlert))]
	sealed class RefComplianceCommodityAlertTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			alert.RCR_AlertType = "NOM";
			alert.RCR_AlertCode = "DUMMYCODE";
			alert.RCR_AlertName = "DUMMY NAME";
			alert.RCR_AlertDescription = "DUMMY DESCRIPTION";
			alert.RCR_CountryRegion = "EU";
			alert.RCR_PublishYear = 2025;
			alert.RCR_TradeDirection = "IMP";
			alert.RCR_CommodityRiskStatus = "PRS";

			return alert;
		}

		public void TestRefComplianceCommodityAlertUniqueCode()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			alert.RCR_CountryRegion = "EU";
			alert.RCR_TradeDirection = "IMP";

			AssertEquals("EU-IMP", alert.RefComplianceCommodityAlertCode);
		}

		public void TestAlertType()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			alert.RCR_AlertType = "NOM";
			AssertEquals("Nomenclature Alert", alert.AlertType);
		}

		public void TestPreventCancel()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			AssertEquals(RefComplianceList.PreventionMessage, alert.CanCancel());
		}

		public void TestPreventReactivate()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			AssertEquals(RefComplianceList.PreventionMessage, alert.CanReactivate());
		}

		public void TestCountryRegionDetails()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			alert.RCR_CountryRegion = "EU";
			AssertEquals("EU - European Union", alert.CountryRegionDetails);
		}

		public void TestLastEditedTime()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			alert.RCR_SystemLastEditTimeUtc = new ZDateTime(2021, 1, 1, 0, 0, 0);
			AssertEquals(new ZDateTime(2021, 1, 1, 0, 0, 0).ToLocalBranchTime(), alert.LastEditedTime);
		}

		public void TestCreatedTime()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			alert.RCR_SystemCreateTimeUtc = new ZDateTime(2021, 1, 1, 0, 0, 0);
			AssertEquals(new ZDateTime(2021, 1, 1, 0, 0, 0).ToLocalBranchTime(), alert.CreatedTime);
		}

		public void TestSourceURL()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			alert.RCR_SourceURL = "/?tariff=040490&c=CA&impexp=E&tab=exportControl2022";
			AssertEquals("https://app.borderwise.com/?tariff=040490&c=CA&impexp=E&tab=exportControl2022", alert.SourceURL);
		}

		public void TestReadOnlySecurity()
		{
			var alert = Factory.New<RefComplianceCommodityAlert>();
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);
			security.RefComplianceCommodityAlertEdit.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				AssertEquals(true, alert.RCR_CommodityRiskStatusInfo.ReadOnly);
			}
			security.RefComplianceCommodityAlertEdit.IsAllowed = true;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				AssertEquals(false, alert.RCR_CommodityRiskStatusInfo.ReadOnly);
			}
		}

		public void TestNoAuditLog()
		{
			var refComplianceCommodityAlert = (RefComplianceCommodityAlert)GetNewBusinessObject();
			Factory.Save();

			refComplianceCommodityAlert.RCR_AlertDescription = "Test Update";
			Factory.Save();

			CombineAssertions("No audit log for Ref Compliance Commodity Alert", () =>
			{
				Assert("No ADD log when created", !refComplianceCommodityAlert.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystemCode));
				Assert("No EDT log when edited", !refComplianceCommodityAlert.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode));
			});
		}
	}
}
