using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	class ConsolDashboardContractAllocationFormActions : IRatingContractSimulationFormActions
	{
		public ConsolDashboardContractAllocationFormActions(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		readonly ForwardingConsol consol;

		bool IRatingContractSimulationFormActions.IsEnabledAllocationToContract => true;

		bool IRatingContractSimulationFormActions.IsEnabledAllocationToRoute => true;

		// This is an action driven by the Contract and Allocation Simulation GUI when initiated from the Consol Dashboard.
		bool IRatingContractSimulationFormActions.TryAllocateToAllocationRoute(IRatingContractAllocationLine allocationRoute)
		{
			var allocationConfirmationMessage = Res.GetString(
				"54584e2a-74e0-139a-465c-9e2dc3608f26",
				"This operation will attempt to allocate {0} to {1} and save. Continue?",
				consol.HumanReadableName,
				allocationRoute.HumanReadableName);
			if (Globals.Message.Show(
				allocationConfirmationMessage,
				allocationRoute.HumanReadableName,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question) == DialogResult.No)
			{
				return false;
			}

			var factoryForDashboardAllocation = new BusinessObjectFactory()
			{
				NameForDebugging = "Consol Dashboard's Factory for Allocation Route allocation."
			};

			var loadedConsolidation = factoryForDashboardAllocation.Load<ForwardingConsol>(consol.PK);
			var parentContractCarrier = allocationRoute.Contract?.ServiceProvider?.MainAddress?.PK ?? ZGuid.Empty;
			if (loadedConsolidation != null
				&& !loadedConsolidation.JK_OA_ShippingLineAddress.IsEmpty
				&& loadedConsolidation.JK_OA_ShippingLineAddress != parentContractCarrier
				&& ZDialogResult.No == ConsolCarrierOverridePromptUtil.PromptConfirmationForCarrierOverrideWithAllocationRoute(allocationRoute, consol))
			{
				return false;
			}

			loadedConsolidation.JK_OA_ShippingLineAddress = parentContractCarrier;
			loadedConsolidation.JK_CarrierContractNumber = allocationRoute.Contract?.RCT_ContractNumber ?? ZString.Empty;
			loadedConsolidation.JK_RCA_AllocationLine = allocationRoute.PK;
			loadedConsolidation.RunPreSaveValidation();

			if (loadedConsolidation.HasErrors)
			{
				Globals.Message.Show(Res.GetString("861ca774-5978-3e82-48ee-77889eb13d9c", "{0} could not be saved because of validation errors.", loadedConsolidation.HumanReadableName));
				return false;
			}

			ZExceptionReporting.ProcessWithSaveExceptionHandling(factoryForDashboardAllocation.Save, null);
			Globals.Message.Show(Res.GetString("45d56097-54cd-619f-44c7-f3672f707394", "{0} has been successfully allocated to {1} and saved.", loadedConsolidation.HumanReadableName, allocationRoute.HumanReadableName));

			return true;
		}

		// This is an action driven by the Contract and Allocation Simulation GUI when initiated from the Consol Dashboard.
		bool IRatingContractSimulationFormActions.TryAllocateToContract(IRatingContract contract)
		{
			var allocationConfirmationMessage = Res.GetString(
				"3e0af124-898e-d2ac-4571-9d502b0d6f5e",
				@"This operation will attempt to allocate {0} to {1} and save. Continue?",
				consol.HumanReadableName,
				contract.HumanReadableName);

			if (Globals.Message.Show(
				allocationConfirmationMessage,
				contract.HumanReadableName,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question) == DialogResult.No)
			{
				return false;
			}

			var factoryForDashboardContractAllocation = new BusinessObjectFactory()
			{
				NameForDebugging = "Consol Dashboard's Factory for Contracts allocation."
			};

			var loadedConsolidation = factoryForDashboardContractAllocation.Load<ForwardingConsol>(consol.PK);

			var contractCarrier = contract.ServiceProvider?.MainAddress?.PK ?? ZGuid.Empty;
			if (loadedConsolidation != null
				&& !loadedConsolidation.JK_OA_ShippingLineAddress.IsEmpty
				&& loadedConsolidation.JK_OA_ShippingLineAddress != contractCarrier
				&& ZDialogResult.No == ConsolCarrierOverridePromptUtil.PromptConfirmationForCarrierOverrideWithContract(contract, consol))
			{
				return false;
			}

			loadedConsolidation.JK_OA_ShippingLineAddress = contractCarrier;
			loadedConsolidation.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			loadedConsolidation.RunPreSaveValidation();

			if (loadedConsolidation.HasErrors)
			{
				Globals.Message.Show(Res.GetString("acdb9baf-b678-5e97-44a8-a874cfaa7163", "{0} could not be saved because of validation errors.", loadedConsolidation.HumanReadableName));
				return false;
			}

			ZExceptionReporting.ProcessWithSaveExceptionHandling(factoryForDashboardContractAllocation.Save, null);
			Globals.Message.Show(Res.GetString("79405491-2704-7990-4351-2b71ee932c66", "{0} has been successfully allocated to {1} and saved.", loadedConsolidation.HumanReadableName, contract.HumanReadableName));

			return true;
		}
	}
}
