using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.GUI
{
	public static class CommissionReversalConfirmationHelper
	{
		public static void ConfirmReversal(CommonShipment shipment, ZPropertyInfo info, ZPropertyInfo[] allInfos)
		{
			if (shipment.IsInDatabase && !info.Value.Equals(info.OriginalValue))
			{
				if (shipment.Job != null && shipment.Job.HasNonReversedCommissionHeaders())
				{
					Globals.Message.Show(
						JobHeader.GetCommissionReversalWarningMessage(allInfos),
						JobHeader.CommissionReversalWarningMessageHeader,
						MessageBoxButtons.OK, MessageBoxIcon.Question);
				}
			}
		}
	}
}
