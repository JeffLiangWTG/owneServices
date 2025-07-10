using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ComplianceRiskShipmentFormTest : ZFormBindingContextTester
	{
		[RequiresSTA]
		public void TestSynchronizeEnforceOnFormLoad_WhenComplianceRiskNotExists_NoDeveloperNotificationExceptionThrown()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			AssertNoExceptionThrown(() =>
			{
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(true)))
				using (TestingState.SuspendIsRunningTests())
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
				}
			});
		}

		[RequiresSTA]
		public void TestSynchronizeEnforceOnFormLoad_WhenComplianceRiskExists_NoDeveloperNotificationExceptionThrown()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = refDataHelper.CreateNewOrGetExistingTariffType(Codes.WorldCustomsOrganisationWCO, TariffTypes.HarmonizedSystem);

			Factory.Save();

			var tariff = refDataHelper.CreateTariff(Codes.WorldCustomsOrganisationWCO, tariffType.PK, "110723", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var eventLog = Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = "CRI";
			eventLog.SCE_EventSubType = "CAI";
			eventLog.SCE_ParentID = shipment.PK;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			AssertNoExceptionThrown(() =>
			{
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(true)))
				using (TestingState.SuspendIsRunningTests())
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					CombineAssertions(() =>
					{
						AssertEquals("Commodity Risk status is Incomplete", ComplianceRiskStatusCodeList.Codes.Incomplete, complianceRiskStatus.COR_CommodityRisk);
						AssertEquals("Overall Risk status still Held", ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
						AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
					});
				}
			});
		}

		[RequiresSTA]
		public void TestCompliancePotentialRiskMessageBannerWhenValidRegisrtySecurityShowVisibleON()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = shipment.TablePrefix;
			complianceRisk.COR_ParentID = shipment.PK;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				complianceRisk.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
				Application.DoEvents();

				var riskBanner = form.MainStatusBar.FindSingle<ZLabel>(x => x.Name == "CompliancePotentialRiskMessageBanner");
				AssertEquals("Job Compliance status is not Clear. View the Compliance Risk tab.", riskBanner.Text);
				AssertEquals(true, riskBanner.Visible);
			}
		}

		public void TestCompliancePotentialRiskMessageBannerWhenInvalidRegisrtySecurityShowVisibleOFf()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentTableCode = shipment.TablePrefix;
			complianceRisk.COR_ParentID = shipment.PK;
			complianceRisk.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;

			Factory.Save();

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (OrganisationsDataRegistry.Instance.EnableComplianceWarningMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				Application.DoEvents();

				var riskBanner = form.MainStatusBar.FindSingleOrDefault<ZLabel>(x => x.Name == "CompliancePotentialRiskMessageBanner");
				AssertNull(riskBanner);
			}
		}

		protected override ZForm GetBoundForm()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			return new ShipmentForm(Factory.New<ForwardingShipment>());
		}
	}
}
