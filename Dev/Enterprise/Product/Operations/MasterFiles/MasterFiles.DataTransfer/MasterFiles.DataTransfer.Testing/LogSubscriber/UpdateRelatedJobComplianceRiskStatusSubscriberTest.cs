using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.DataTransfer.Testing
{
	[TestedType(typeof(UpdateRelatedJobSubscriberComplianceRiskTest))]
	public class UpdateRelatedJobComplianceRiskStatusSubscriberTest : LogSubscriberTest<UpdateRelatedJobSubscriberComplianceRiskTest>
	{
		public void TestProcessLogQueueItems_UpdateOrgRelatedJobsComplianceRiskStatus()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var consol = Factory.New<IForwardingConsol>();
				consol.JK_OA_CreditorAddress = org.MainAddress.PK;
				var log = org.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP=MAN";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				org.OH_Code = "OLDCODE";
				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				org.Reload();
				((BusinessObject)consol).Reload();
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);
				AssertEquals("Precondition: ", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

				RunLogWalkerCycleForTest();

				org.Reload();
				AssertEquals("NEWCODE", org.OH_Code);

				((BusinessObject)consol).Reload();
				AssertNotEquals("Should ignore update related jobs' DPS status when CPW is enabled.", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			}
		}

		public void TestProcessLogQueueItems_UpdateVesselRelatedJobsComplianceRiskStatus()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				var consol = Factory.New<IForwardingConsol>();
				var transport = consol.Transports_AddNew();
				transport.JW_Vessel = vessel.RV_Code;
				vessel.RV_RN_NKCountryOfReg = "AU";
				var log = vessel.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP=MAN";
					log.SL_EventTime = DateTime.Now;
					log.SL_IsEstimate = false;
				}

				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();
				vessel.Reload();
				((BusinessObject)consol).Reload();
				AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);

				RunLogWalkerCycleForTest();

				vessel.Reload();
				AssertEquals("US", vessel.RV_RN_NKCountryOfReg);

				((BusinessObject)consol).Reload();
				AssertNotEquals("Should ignore update related jobs' DPS status when CPW is enabled.", ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
			}
		}

		public void TestProcessLogQueueItems_NotUpdateVesselOrgRelatedJobsComplianceRiskStatusWhenRegistryOff()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningEnableLogWalkerServiceTaskToUpdateRelatedJobs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				var org = Factory.NewWithValidTestData<OrgHeader>();

				var log1 = vessel.Logs.AddNew();

				using (log1.LockForUpdatingKeyFieldsForTesting())
				{
					log1.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log1.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP=MAN";
					log1.SL_EventTime = DateTime.Now;
					log1.SL_IsEstimate = false;
				}

				var log2 = org.Logs.AddNew();

				using (log2.LockForUpdatingKeyFieldsForTesting())
				{
					log2.SL_SE_NKEvent = AutoEvents.DeniedPartyStatusUpdated.Code;
					log2.SL_Reference = $"|CMP={Environment.Env.CurrentCompanyPK}|NEW=MAT|OLD=CLR|TYP=MAN";
					log2.SL_EventTime = DateTime.Now;
					log2.SL_IsEstimate = false;
				}

				vessel.RV_RN_NKCountryOfReg = "AU";
				vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				org.OH_Code = "OLDCODE";
				org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();
				org.Reload();
				vessel.Reload();
				AssertEquals(ScreeningStatusesList.Codes.Matched, vessel.RV_ScreeningStatus);
				AssertEquals(ScreeningStatusesList.Codes.Matched, org.OH_ScreeningStatus);

				RunLogWalkerCycleForTest();

				vessel.Reload();
				org.Reload();
				AssertEquals("AU", vessel.RV_RN_NKCountryOfReg);
				AssertEquals("OLDCODE", org.OH_Code);
			}
		}

		public new void TestRegisteredInSystemOrClientSpecificLogSubscribersList()
		{
			Assert(true);
		}
	}

	[Serializable]
	public class UpdateRelatedJobSubscriberComplianceRiskTest : UpdateRelatedJobSubscriber
	{
		protected override RelatedJobsComplianceRiskStatusUpdater GetComplianceRiskStatusUpdater()
		{
			var complianceRiskStatusUpdater = new RelatedJobsComplianceRiskStatusUpdater();
			var updaters = typeof(RelatedJobsComplianceRiskStatusUpdater).GetProperty("RelatedJobComplianceRiskStatusUpdaters", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(complianceRiskStatusUpdater) as List<RelatedJobComplianceRiskStatusUpdater>;
			updaters.Add(new DummyJobComplianceRiskStatusUpdater());

			return complianceRiskStatusUpdater;
		}
	}

	class DummyJobComplianceRiskStatusUpdater : RelatedJobComplianceRiskStatusUpdater
	{
		protected override DbCommand GetCommandForOrgUpdate(Guid orgPK, Guid companyPK)
		{
			var cmd = Db.Connection.Command("Update dbo.OrgHeader set OH_Code = 'NEWCODE' where OH_PK = @OrgPk AND EXISTS (SELECT NULL from dbo.GlbCompany where GC_PK = @CompanyPk)");
			cmd.AddParameter("@OrgPk", System.Data.SqlDbType.UniqueIdentifier, orgPK);
			cmd.AddParameter("@CompanyPk", System.Data.SqlDbType.UniqueIdentifier, companyPK);

			return cmd;
		}

		protected override DbCommand GetCommandForVesselUpdate(Guid vesselPK, Guid companyPK)
		{
			var cmd = Db.Connection.Command("Update dbo.RefVessel set RV_RN_NKCountryOfReg = 'US' where RV_PK = @VesselPk AND EXISTS (SELECT NULL from dbo.GlbCompany where GC_PK = @CompanyPk)");
			cmd.AddParameter("@VesselPk", System.Data.SqlDbType.UniqueIdentifier, vesselPK);
			cmd.AddParameter("@CompanyPk", System.Data.SqlDbType.UniqueIdentifier, companyPK);

			return cmd;
		}
	}
}
