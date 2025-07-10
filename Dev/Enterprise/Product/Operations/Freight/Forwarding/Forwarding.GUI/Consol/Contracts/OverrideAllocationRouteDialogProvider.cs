using System;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class OverrideAllocationRouteDialogProvider : IOverrideAllocationRouteDialogProvider
	{
		public static void Register(ZForm form)
		{
			if (form != null && form.BusinessEntity != null)
			{
				form.BusinessEntity.Factory.SetValue<IOverrideAllocationRouteDialogProvider, OverrideAllocationRouteDialogProvider>();
			}
		}

		bool IOverrideAllocationRouteDialogProvider.PromptUserForConfirmingOverride(IRatingContractAllocationLine route, IAllocationRouteAssignable routeAssignable)
		{
			var caption = Res.GetString("9d3cdc43-ccae-4459-9f57-d49fa5c34580", "Override Carrier Contract and Allocation Route?");

			ZString message;
			if (!routeAssignable.ServiceProvider.IsEmpty && route.Contract.ServiceProvider != null
				&& (routeAssignable.AllocationRouteID.IsEmpty || routeAssignable.AllocationRouteID == route.RCA_AllocationLineID)
				&& (routeAssignable.CarrierContractNumber.IsEmpty || string.Equals(routeAssignable.CarrierContractNumber, route.Contract.RCT_ContractNumber, StringComparison.OrdinalIgnoreCase)))
			{
				message = Res.GetString(
					"4c0d6ea7-0c14-74a3-4518-a825e18b781f",
					"The {0} has Carrier Organization {1} which does not align with the Carrier Contract of the Allocation Route selected.\r\nDo you wish to override it with the Carrier {2} from the Allocation Route?",
					routeAssignable.HumanReadableName, routeAssignable.ServiceProvider, route.Contract.ServiceProvider.HumanReadableName);
			}
			else if (routeAssignable.AllocationRouteID.IsEmpty)
			{
				message = Res.GetString(
					"9df6ac48-6b27-477e-bf68-2bfedd24a0e3",
					"The {0} is already assigned Carrier Contract {1}.\r\nDo you wish to override it with the Allocation Route of the Sailing Schedule (Carrier {2} and {3})?",
					routeAssignable.HumanReadableName, routeAssignable.CarrierContractNumber, route.Contract.HumanReadableName, route.HumanReadableName);
			}
			else
			{
				message = Res.GetString(
					"87c1c27c-bfba-498c-bcd6-e498398e1294",
					"The {0} is already assigned Carrier Contract {1} and Allocation Route {2}.\r\nDo you wish to override it with the Allocation Route of the Sailing Schedule (Carrier {3} and {4})?",
					routeAssignable.HumanReadableName, routeAssignable.CarrierContractNumber, routeAssignable.AllocationRouteID, route.Contract.HumanReadableName, route.HumanReadableName);
			}

			var dialogContext = new DialogDefaultContext(
				new ZGuid("f16b1014-f491-485a-88eb-95c656d3feac"),
				caption,
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Question,
				null,
				showCheckboxOnly: true);

			var dialogResult = Globals.Message.ShowOrDefault(dialogContext, message);
			return dialogResult == ZDialogResult.Yes;
		}
	}
}
