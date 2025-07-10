using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(DpsStatusUpdateSettingRegistryItem))]
	public class DpsStatusUpdateSettingRegistryItemTest : StronglyTypedRegistryItemTestCase<DpsStatusUpdateSetting>
	{
		public void TestShipmentDPSChangeDependsOnPhaseMatching()
		{
			var phaseSecurity = GetPhaseSecurity();
			var updateSetting = new DpsStatusUpdateSetting();
			updateSetting.Option = DpsStatusUpdateOptions.Codes.PHS;
			var phaseSetting = updateSetting.JobUpdateSettings.AddNew();
			phaseSetting.Code = "PH1";
			phaseSetting.ShouldUpdate = true;
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			using (ForwardingConfigurationRegistry.Instance.ShipmentDpsStatusUpdateSetting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, updateSetting))
			{
				var factory = new BusinessObjectFactory();
				var job = factory.New<IForwardingShipment>();
				var shipment = ((IScreeningPartyProvider)job);

				job.JS_Phase = "PH1";

				var header = factory.NewWithValidTestData<OrgHeader>();
				var jobDocAddress = factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress.E2_OA_Address = header.Addresses[0].PK;
				jobDocAddress.E2_ParentID = job.PK;
				jobDocAddress.E2_ParentTableCode = "JS";
				jobDocAddress.E2_AddressOverride = false;

				shipment.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

				factory.Save();
				var result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)shipment).Reload();

				AssertEquals("Phase matches, shipment DPS changed to Unknown", ScreeningStatusesList.Codes.Unknown, shipment.ScreeningStatus);

				job.JS_Phase = "PH8";

				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				factory.Save();
				result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)shipment).Reload();

				AssertEquals("Phase not match, shipment DPS still Unknown", ScreeningStatusesList.Codes.Unknown, shipment.ScreeningStatus);
			}
		}

		public void TestConsolDPSChangeDependsOnPhaseMatching()
		{
			var phaseSecurity = GetPhaseSecurity();
			var updateSetting = new DpsStatusUpdateSetting();
			updateSetting.Option = DpsStatusUpdateOptions.Codes.PHS;
			var phaseSetting = updateSetting.JobUpdateSettings.AddNew();
			phaseSetting.Code = "PH1";
			phaseSetting.ShouldUpdate = true;
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			using (ForwardingConfigurationRegistry.Instance.ConsolDpsStatusUpdateSetting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, updateSetting))
			{
				var factory = new BusinessObjectFactory();
				var job = factory.New<IForwardingConsol>();
				var consol = ((IScreeningPartyProvider)job);

				job.JK_Phase = "PH1";

				var header = factory.NewWithValidTestData<OrgHeader>();
				var jobDocAddress = factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress.E2_OA_Address = header.Addresses[0].PK;
				jobDocAddress.E2_ParentID = job.PK;
				jobDocAddress.E2_ParentTableCode = "JK";
				jobDocAddress.E2_AddressOverride = false;

				consol.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

				factory.Save();
				var result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)consol).Reload();

				AssertEquals("Phase matches, consol DPS changed to Unknown", ScreeningStatusesList.Codes.Unknown, consol.ScreeningStatus);

				job.JK_Phase = "PH8";

				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				factory.Save();
				result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)consol).Reload();

				AssertEquals("Phase not match, consol DPS still Unknown", ScreeningStatusesList.Codes.Unknown, consol.ScreeningStatus);
			}
		}

		public void TestShipmentDPSChangeDependsOnDABWhenPhaseIsALL()
		{
			var phaseSecurity = GetPhaseSecurity();
			var updateSetting = new DpsStatusUpdateSetting();
			updateSetting.Option = DpsStatusUpdateOptions.Codes.PHS;
			var phaseSetting = updateSetting.JobUpdateSettings.AddNew();
			phaseSetting.Code = "PH1";
			phaseSetting.ShouldUpdate = true;
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			using (ForwardingConfigurationRegistry.Instance.ShipmentDpsStatusUpdateSetting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, updateSetting))
			{
				var factory = new BusinessObjectFactory();
				var job = factory.New<IForwardingShipment>();
				var shipment = ((IScreeningPartyProvider)job);

				job.JS_E_ARV = ZDateTime.Now;
				job.JS_Phase = "ALL"; //now we only try to match by date

				var header = factory.NewWithValidTestData<OrgHeader>();
				var jobDocAddress = factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress.E2_OA_Address = header.Addresses[0].PK;
				jobDocAddress.E2_ParentID = job.PK;
				jobDocAddress.E2_ParentTableCode = "JS";
				jobDocAddress.E2_AddressOverride = false;

				shipment.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

				factory.Save();
				var result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)shipment).Reload();

				AssertEquals("Date matches, shipment DPS changed to Unknown", ScreeningStatusesList.Codes.Unknown, shipment.ScreeningStatus);

				job.JS_E_ARV = ZDateTime.Now.AddMonths(-1);
				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				factory.Save();

				result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)shipment).Reload();

				AssertEquals("Date not match, shipment DPS still Unknown", ScreeningStatusesList.Codes.Unknown, shipment.ScreeningStatus);
			}
		}

		public void TestConsolDPSChangeDependsOnDABWhenPhaseIsALL()
		{
			var phaseSecurity = GetPhaseSecurity();
			var updateSetting = new DpsStatusUpdateSetting();
			updateSetting.Option = DpsStatusUpdateOptions.Codes.PHS;
			var phaseSetting = updateSetting.JobUpdateSettings.AddNew();
			phaseSetting.Code = "PH1";
			phaseSetting.ShouldUpdate = true;
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			using (ForwardingConfigurationRegistry.Instance.ConsolDpsStatusUpdateSetting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, updateSetting))
			{
				var factory = new BusinessObjectFactory();
				var job = factory.New<IForwardingConsol>();
				var consol = ((IScreeningPartyProvider)job);

				var jobHeader = factory.New<JobHeader>();
				jobHeader.JH_ParentID = job.PK;
				jobHeader.JH_Status = "WRK";

				job.JK_Phase = "ALL";//now we only try to match by Job Status

				var header = factory.NewWithValidTestData<OrgHeader>();
				var jobDocAddress = factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress.E2_OA_Address = header.Addresses[0].PK;
				jobDocAddress.E2_ParentID = job.PK;
				jobDocAddress.E2_ParentTableCode = "JK";
				jobDocAddress.E2_AddressOverride = false;

				consol.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

				factory.Save();
				var result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)consol).Reload();

				AssertEquals("Date matches, consol DPS changed to Unknown", ScreeningStatusesList.Codes.Unknown, consol.ScreeningStatus);

				jobHeader.JH_Status = "CMP";
				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				factory.Save();
				result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)consol).Reload();

				AssertEquals("Date not match, consol DPS still Unknown", ScreeningStatusesList.Codes.Unknown, consol.ScreeningStatus);
			}
			ErrorReporter.Clear();
		}

		public void TestShipmentDpsUpdateCombinedOption()
		{
			var phaseSecurity = GetPhaseSecurity();
			var updateSetting = new DpsStatusUpdateSetting();
			updateSetting.Option = DpsStatusUpdateOptions.Codes.COM;
			var phaseSetting = updateSetting.JobUpdateSettings.AddNew();
			phaseSetting.Code = "PH1";
			phaseSetting.ShouldUpdate = true;
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			using (ForwardingConfigurationRegistry.Instance.ShipmentDpsStatusUpdateSetting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, updateSetting))
			{
				var factory = new BusinessObjectFactory();
				var header = factory.NewWithValidTestData<OrgHeader>();

				var job1 = factory.New<IForwardingShipment>();
				var shipment1 = ((IScreeningPartyProvider)job1);
				job1.JS_Phase = "PH1";
				var jobDocAddress1 = factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress1.E2_OA_Address = header.Addresses[0].PK;
				jobDocAddress1.E2_ParentID = job1.PK;
				jobDocAddress1.E2_ParentTableCode = "JS";
				jobDocAddress1.E2_AddressOverride = false;

				var job2 = factory.New<IForwardingShipment>();
				var shipment2 = ((IScreeningPartyProvider)job2);
				job2.JS_Phase = "ALL";
				job2.JS_E_ARV = ZDateTime.Now;
				var jobDocAddress2 = factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress2.E2_OA_Address = header.Addresses[0].PK;
				jobDocAddress2.E2_ParentID = job2.PK;
				jobDocAddress2.E2_ParentTableCode = "JS";
				jobDocAddress2.E2_AddressOverride = false;

				shipment1.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				shipment2.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

				factory.Save();
				var result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)shipment1).Reload();
				((BusinessObject)shipment2).Reload();

				AssertEquals("Phase matches, shipment1 DPS changed to Unknown", ScreeningStatusesList.Codes.Unknown, shipment1.ScreeningStatus);
				AssertEquals("Date matches, shipment2 DPS changed to Unknown", ScreeningStatusesList.Codes.Unknown, shipment2.ScreeningStatus);

				job2.JS_E_ARV = ZDateTime.MinSmallDateTimeValue;

				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				factory.Save();
				result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)shipment1).Reload();

				AssertEquals("Date not match neither does phase, shipment2 DPS still Unknown", ScreeningStatusesList.Codes.Unknown, shipment2.ScreeningStatus);
			}
		}

		public void TestConsolDpsUpdateCombinedOption()
		{
			var phaseSecurity = GetPhaseSecurity();
			var updateSetting = new DpsStatusUpdateSetting();
			updateSetting.Option = DpsStatusUpdateOptions.Codes.COM;
			var phaseSetting = updateSetting.JobUpdateSettings.AddNew();
			phaseSetting.Code = "PH1";
			phaseSetting.ShouldUpdate = true;
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false)))
			using (ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			using (ForwardingConfigurationRegistry.Instance.ConsolDpsStatusUpdateSetting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, updateSetting))
			{
				var factory = new BusinessObjectFactory();
				var header = factory.NewWithValidTestData<OrgHeader>();

				var job1 = factory.New<IForwardingConsol>();
				var consol1 = ((IScreeningPartyProvider)job1);
				job1.JK_Phase = "PH1";
				var jobDocAddress1 = factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress1.E2_OA_Address = header.Addresses[0].PK;
				jobDocAddress1.E2_ParentID = job1.PK;
				jobDocAddress1.E2_ParentTableCode = "JK";
				jobDocAddress1.E2_AddressOverride = false;

				var job2 = factory.New<IForwardingConsol>();
				var consol2 = ((IScreeningPartyProvider)job2);
				job2.JK_Phase = "ALL";
				job2.Transports_AddNew().JW_ETA = ZDateTime.Now;

				var transport1 = job2.Transports_Get(0);
				(transport1 as INeedRow).Row[JobConsolTransportSchema.Constants.JW_TransportMode] = TransportModes.Sea;
				transport1.JW_Vessel = "TestCode";
				transport1.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				var transport2 = job2.Transports_Get(1);
				(transport2 as INeedRow).Row[JobConsolTransportSchema.Constants.JW_TransportMode] = TransportModes.Sea;
				transport2.JW_Vessel = "TestCode";
				transport2.JW_VesselScreeningStatus = ScreeningStatusesList.Codes.Unknown;

				var jobDocAddress2 = factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress2.E2_OA_Address = header.Addresses[0].PK;
				jobDocAddress2.E2_ParentID = job2.PK;
				jobDocAddress2.E2_ParentTableCode = "JK";
				jobDocAddress2.E2_AddressOverride = false;

				consol1.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				consol2.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

				factory.Save();
				var result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)consol1).Reload();
				((BusinessObject)consol2).Reload();

				AssertEquals("Phase matches, consol 1 DPS changed to Unknown", ScreeningStatusesList.Codes.Unknown, consol1.ScreeningStatus);
				AssertEquals("Date matches, consol 2 DPS changed to Unknown", ScreeningStatusesList.Codes.Unknown, consol2.ScreeningStatus);

				job2.Transports_Get(0).JW_ETA = ZDateTime.BrettsBirthday;

				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

				factory.Save();
				result = ScreeningStatusUpdater.UpdateRelatedJobs(header);

				((BusinessObject)consol1).Reload();

				AssertEquals("Date not match neither does phase, shipment2 DPS still Unknown", ScreeningStatusesList.Codes.Unknown, consol2.ScreeningStatus);
			}
		}

		internal static PhaseSecurity GetPhaseSecurity()
		{
			var phaseSecurity = new PhaseSecurity();
			phaseSecurity.RuleLocations.AddPair("Bedroom", "There is a bed");
			phaseSecurity.RuleLocations.AddPair("Restroom", "There is a toilet");

			var phase1 = phaseSecurity.Phases.AddNew();
			phase1.Code = "PH1";
			phase1.Description = (NoResString)"Phase 1";
			phase1.Rules.AddNew().Location = "Bedroom";

			var phase2 = phaseSecurity.Phases.AddNew();
			phase2.Code = "PH2";
			phase2.Description = (NoResString)"Phase 2";
			phase2.Rules.AddNew().Location = "Restroom";

			return phaseSecurity;
		}

		protected override StronglyTypedRegistryItem<DpsStatusUpdateSetting, DpsStatusUpdateSetting> GetNewRegistryItem()
		{
			return new DpsStatusUpdateSettingRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, null);
		}
	}
}
