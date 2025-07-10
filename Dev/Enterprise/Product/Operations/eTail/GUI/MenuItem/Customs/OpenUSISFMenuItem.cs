using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.eTail.GUI
{
	public class OpenUSISFMenuItem : BaseHVLVMenuItem
	{
		public OpenUSISFMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("570a7bf7-d185-4ab7-b007-a489dce99771", "Open US Importer Security Filing"), shipment)
		{
		}

		protected override Action MenuAction => () =>
		{
			if (shipment.HasTransferredLog(CustomsModuleCodes.Codes.ImporterSecurityFiling))
			{
				var form = ObjectFactory.Get<IHVLVISFMetaHeaderForm>(nameof(IHVLVISFMetaHeaderForm), shipment.Factory, shipment.PK) as ZForm;
				ZFormModaliser.ShowDialogAndDispose(form, this.Form);
			}
		};

		public override void UpdateVisibilityAndCaption()
		{
			Visible = HVLVMenuItemHelper.IsSeaShipmentWithUSDestination(shipment)
				&& (Header.GenPivotCollection.CustomsJobs.Any(job => typeof(ICusISFHeader).IsAssignableFrom(job.GetType())) || shipment.HasTransferredLog(CustomsModuleCodes.Codes.ImporterSecurityFiling));
		}
	}
}
