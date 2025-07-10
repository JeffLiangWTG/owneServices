using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class TransferController : WhsControllerBase
	{
		public TransferController()
		{
		}

		public override ControllerID ID => ControllerIDs.WhsTransfer;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsTransfer;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsTransfer);

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			return new TransferEntryForm((WhsTransfer)businessEntity, whsNotificationSubscriberGuiHelper);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsTransferView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsTransferNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsTransferEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsTransferDelete;

		public override IZForm ShowNewForm()
		{
			var form = (TransferEntryForm)base.ShowNewForm();
			if (form != null) // form will be null if security denied
			{
				form.SetupInventoryFilterStripUserControl();
			}
			return form;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var form = (TransferEntryForm)base.ShowEditForm(sourceEntity);
			var docket = ((WhsDocket)sourceEntity);
			var isFinalisedOrPutawayTransfer = (docket.IsFinalised || docket.WD_IsPutawayTransfer);
			if (form != null && !isFinalisedOrPutawayTransfer)
			{
				form.SetupInventoryFilterStripUserControl();
			}
			return form;
		}
	}
}
