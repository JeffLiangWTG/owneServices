using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class ConsolCarrierOverridePromptUtil
	{
		public static ZDialogResult PromptConfirmationForCarrierOverrideWithAllocationRoute(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			var caption = Res.GetString("00804d04-aaef-08ac-48b7-63f1f20b8402", "Override the Carrier on the Consol");
			var message = Res.GetString(
				"91c5031e-1203-53b2-4fd1-558950d2fa28",
				"Service Provider {0} of Contract {1} and Allocation Route {2} is different from the Carrier {3} on Consol.\r\n\r\nDo you want to override the Carrier on the Consol with {4}?",
				allocationRoute.Contract.ServiceProvider?.OH_Code,
				allocationRoute.Contract.RCT_ContractNumber,
				allocationRoute.RCA_AllocationLineID,
				consol.ShippingLine?.OH_Code,
				allocationRoute.Contract.ServiceProvider?.OH_Code);

			var dialogResult = Globals.Message.Show(
				message,
				caption,
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Warning);

			return dialogResult;
		}

		public static ZDialogResult PromptConfirmationForCarrierOverrideWithContract(IRatingContract contract, ForwardingConsol consol)
		{
			var caption = Res.GetString("0ad9b2b8-13a7-15a1-4be9-0df0a62438a7", "Override the Carrier on the Consol");
			var message = Res.GetString("57d7b4da-713f-fe90-4442-4167a3c5809a",
				"Service Provider {0} of Contract {1} to be allocated is different from the Carrier {2} on Consol.\r\n\r\nDo you want to override the Carrier on the Consol with {3}?",
				contract.ServiceProvider?.OH_Code,
				contract.RCT_ContractNumber,
				consol.ShippingLine?.OH_Code,
				contract.ServiceProvider?.OH_Code);

			var dialogResult = Globals.Message.Show(
				message,
				caption,
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Warning);

			return dialogResult;
		}
	}
}
