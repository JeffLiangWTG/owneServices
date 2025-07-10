using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RelatedJobsComplianceRiskStatusUpdaterTest : TestCaseWithFactory
	{
		public void TestUpdateOrg()
		{
			var companyPK = Guid.NewGuid();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OLDCODE";
			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				updater.UpdateForOrg(orgHeader.PK.ToGuid(), companyPK, null);
				orgHeader.Reload();
				AssertEquals("OLDCODE", orgHeader.OH_Code);
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				updater.UpdateForOrg(orgHeader.PK.ToGuid(), companyPK, null);
				orgHeader.Reload();
				AssertEquals("NEWCODE0", orgHeader.OH_Code);
				AssertEquals(companyPK, dummyUpdater.CompanyPK);

				var useActionToExcuteCommand = false;
				updater.UpdateForOrg(orgHeader.PK.ToGuid(), companyPK, (cmd, connection) =>
				{
					useActionToExcuteCommand = true;
				});

				orgHeader.Reload();
				AssertEquals(true, useActionToExcuteCommand);
				AssertEquals("NEWCODE0", orgHeader.OH_Code);
			}
		}

		public void TestUpdateVessel()
		{
			var companyPK = Guid.NewGuid();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "OLDCODE";
			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				updater.UpdateForVessel(vessel.PK.ToGuid(), companyPK, null);
				vessel.Reload();
				AssertEquals("OLDCODE", vessel.RV_Code);
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				updater.UpdateForVessel(vessel.PK.ToGuid(), companyPK, null);
				vessel.Reload();
				AssertEquals("NEWCODE0", vessel.RV_Code);
				AssertEquals(companyPK, dummyUpdater.CompanyPK);

				var useActionToExcuteCommand = false;
				updater.UpdateForVessel(vessel.PK.ToGuid(), companyPK, (cmd, connection) =>
				{
					useActionToExcuteCommand = true;
				});

				vessel.Reload();
				AssertEquals(true, useActionToExcuteCommand);
				AssertEquals("NEWCODE0", vessel.RV_Code);
			}
		}

		public void TestUpdateCommand_HasAtSymbol()
		{
			var updater = new RelatedJobComplianceRiskStatusUpdaterForTest("A", "B");
			using (var command1 = updater.GetCommandForOrgUpdateExposed(Guid.NewGuid(), Guid.NewGuid()))
			using (var command2 = updater.GetCommandForVesselUpdateExposed(Guid.NewGuid(), Guid.NewGuid()))
			{
				var parameters = typeof(CargoWise.Data.DbCommand).GetProperty("InternalParameters", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				var dbParameterCollection1 = parameters.GetValue(command1) as DbParameterCollection;
				var dbParameterCollection2 = parameters.GetValue(command2) as DbParameterCollection;
				AssertNotNull(dbParameterCollection1);
				AssertNotNull(dbParameterCollection2);

				var dbParameterCollection = dbParameterCollection1.ToList<DbParameter>().Concat(dbParameterCollection2.ToList<DbParameter>()).ToList();
				Assert("Command parameter should always start with @", dbParameterCollection.All(u => u.ParameterName.StartsWith("@")));
			}
		}

		RelatedJobsComplianceRiskStatusUpdater updater;
		DummyJobComplianceRiskStatusUpdater dummyUpdater;

		protected override void SetUp()
		{
			base.SetUp();
			updater = new RelatedJobsComplianceRiskStatusUpdater();
			var updaters = typeof(RelatedJobsComplianceRiskStatusUpdater).GetProperty("RelatedJobComplianceRiskStatusUpdaters", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(updater) as List<RelatedJobComplianceRiskStatusUpdater>;
			AssertNotNull(updaters);
			dummyUpdater = new DummyJobComplianceRiskStatusUpdater();
			updaters.Clear();
			updaters.Add(dummyUpdater);
		}

		class RelatedJobComplianceRiskStatusUpdaterForTest : RelatedJobComplianceRiskStatusUpdater
		{
			public RelatedJobComplianceRiskStatusUpdaterForTest(string orgSpScriptName, string vesselSpScriptName) : base(orgSpScriptName, vesselSpScriptName)
			{
			}

			public CargoWise.Data.DbCommand GetCommandForOrgUpdateExposed(Guid orgPK, Guid companyPK)
			{
				return base.GetCommandForOrgUpdate(orgPK, companyPK);
			}

			public CargoWise.Data.DbCommand GetCommandForVesselUpdateExposed(Guid vesselPK, Guid companyPK)
			{
				return base.GetCommandForVesselUpdate(vesselPK, companyPK);
			}
		}
	}
}
