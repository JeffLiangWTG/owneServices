using System.Windows.Forms;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.SailingDataVendor.GUI
{
	internal class VesselRoutingVoyageImportGuiHelper : NotificationSubscriberGuiHelper
	{
		public override void QueryUser(IQueryUserEventArgs e)
		{
			QueryUserSelectVesselFromLloydsNumber vesselSelect = e as QueryUserSelectVesselFromLloydsNumber;
			if (vesselSelect != null)
			{
				using (VesselSelectForm form = new VesselSelectForm(vesselSelect))
				{
					FormShowDialogWithoutDispose(form);
				}
			}
			else
			{
				base.QueryUser(e);
			}
		}

		protected virtual DialogResult FormShowDialogWithoutDispose(VesselSelectForm form)
		{
			return ZFormModaliser.ShowDialogWithoutDispose(form);
		}
	}
}
