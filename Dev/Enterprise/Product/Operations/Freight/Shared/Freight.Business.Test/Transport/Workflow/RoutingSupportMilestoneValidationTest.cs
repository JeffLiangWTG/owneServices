using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RoutingSupportMilestoneValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePortPair()
		{
			var milestone = (RoutingSupportProcessTask)ConsolWorkflowProvider.WorkflowItems.Milestones.AddNew();
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			milestone.PortPair = "AUSYD->MYPKG";
			AssertNoErrors("No errors for a valid port pair that has a corresponding transport", milestone.ReferenceCodeInfo);

			milestone.PortPair = "UNKNO->NPAIR";
			AssertEquals("UNKNO->NPAIR", milestone.PortPair);
			milestone.Validation.ValidateReferenceCode();
			AssertHasErrors("Error on a port pair that doesn't have a corresponding transport", milestone.ReferenceCodeInfo);
		}

		public void TestValidateReferenceCode()
		{
			var milestone = (RoutingSupportProcessTask)ConsolWorkflowProvider.WorkflowItems.Milestones.AddNew();
			Consol.JK_RL_NKLoadPort = "GBLHR";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			AssertNoWarnings("No warnings for milestone with valid Load / Discharge", milestone.ReferenceCodeInfo);

			milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone.Validation.ValidateReferenceCode();
			AssertEquals((ZString)"GBLHR->AUSYD", milestone.ReferenceCode);
			AssertNoWarnings("No warning for Arrival milestone with valid ReferenceCode", milestone.ReferenceCodeInfo);

			milestone.ReferenceCode = "";
			milestone.Validation.ValidateReferenceCode();
			AssertHasWarningContaining(milestone.ReferenceCodeInfo, "If leg is empty then the arrival milestone will not be met.");

			milestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone.ReferenceCode = "";
			milestone.Validation.ValidateReferenceCode();
			AssertHasWarningContaining(milestone.ReferenceCodeInfo, "If leg is empty then the departure milestone will not be met.");

			milestone.ReferenceCode = "GBLGW->AUBNE";
			milestone.Validation.ValidateReferenceCode();
			AssertNoWarnings("No warning for Departure milestone with valid ReferenceCode", milestone.ReferenceCodeInfo);
		}

		public void TestValidateReferenceCode_ConsolDetails()
		{
			var consolMilestone = (RoutingSupportProcessTask)ConsolWorkflowProvider.WorkflowItems.Milestones.AddNew();
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "MDKIV";

			var shipmentMilestone = (RoutingSupportProcessTask)ShipmentWorkflowProvider.WorkflowItems.Milestones.AddNew();

			Action<RoutingSupportProcessTask, string> assertInvalidatedMilestone = (milestone, expectedWarning) =>
			{
				milestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
				milestone.ReferenceCode = "";
				milestone.Validation.ValidateReferenceCode();
				AssertHasWarning(milestone.ReferenceCodeInfo, expectedWarning);
			};

			assertInvalidatedMilestone(consolMilestone, "If leg is empty then the departure milestone will not be met. This may happen if the Consol first load port is different from the load ports of all transport legs.");
			assertInvalidatedMilestone(shipmentMilestone, "If leg is empty then the departure milestone will not be met.");
		}

		#region Implementation

		IWorkflowProvider ConsolWorkflowProvider
		{
			get { return (IWorkflowProvider)Consol; }
		}

		CommonConsol Consol
		{
			get { return consol ?? (consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>()); }
		}
		CommonConsol consol;

		IWorkflowProvider ShipmentWorkflowProvider
		{
			get { return (IWorkflowProvider)Shipment; }
		}

		CommonShipment Shipment
		{
			get { return shipment ?? (shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>()); }
		}
		CommonShipment shipment;

		#endregion
	}
}
