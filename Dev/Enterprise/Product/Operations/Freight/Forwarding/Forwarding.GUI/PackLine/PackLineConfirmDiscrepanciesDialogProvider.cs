using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class PackLineConfirmDiscrepanciesDialogProvider
	{
		public PackLineConfirmDiscrepancyAction ConditionallyShowDialogAndGetResult(PackLine packLine)
		{
			var originTransitWarehouse = packLine.JL_Calc_OriginTransitWarehouse;
			var isAtOrigin = originTransitWarehouse != null && originTransitWarehouse == packLine.LastKnownTransitWarehouseAddress;
			if (isAtOrigin && packLine.JL_LastKnownTransitWarehouseStatus == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received)
			{
				return PackLineConfirmDiscrepancyAction.UpdatePacklineAndConfirm;
			}
			return ShowDialogAndGetResult(packLine);
		}

		protected virtual PackLineConfirmDiscrepancyAction ShowDialogAndGetResult(PackLine packLine)
		{
			using (var form = new PackLineConfirmDiscrepanciesDialog(packLine))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				return form.result;
			}
		}
	}
}
