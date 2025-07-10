using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class CreditControlledDocumentDeliveryGUIManagerTest : TestCaseWithFactory
	{
		public void TestInitializeComplianceWorkflowPopupIfNeeded()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var manager = new DocumentDeliveryRestrictionGUIManager())
			{
				manager.Initialise(shipment);
				AssertNull(((ICompliancePartyRiskStatusProvider)shipment).InitializeComplianceWorkflowPopupIfNeeded);
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var manager = new DocumentDeliveryRestrictionGUIManager())
			{
				manager.Initialise(shipment);
				AssertNotNull(((ICompliancePartyRiskStatusProvider)shipment).InitializeComplianceWorkflowPopupIfNeeded);
			}

			AssertNull(((ICompliancePartyRiskStatusProvider)shipment).InitializeComplianceWorkflowPopupIfNeeded);
		}
	}
}
