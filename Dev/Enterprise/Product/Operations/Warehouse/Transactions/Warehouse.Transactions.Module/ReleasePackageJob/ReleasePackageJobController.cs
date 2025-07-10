using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReleasePackageJobController : WhsControllerBase
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public ReleasePackageJobController()
		{
		}

		public override ControllerID ID => ControllerIDs.WhsReleasePackageJob;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsReleasePackageJob;

		public override Type TypeOfTopLevelBusinessObject => typeof(PkgPackageJob);

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			var order = (WhsOrder)((PkgPackageJob)businessEntity).ParentJob;
			return new ReleaseEntryForm(order.Pick, whsNotificationSubscriberGuiHelper);
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			var form = (ReleaseEntryForm)base.ShowLoadedForm(sourceEntity, action);
			var orderPK = ((PkgPackageJob)sourceEntity).KJ_ParentID;
			form.SetInitialOrderToSelectInGrid(orderPK);
			form.SelectPackageTabPage();
			return form;
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsReleaseView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsReleaseEdit;

		protected override SecurityCheckpoint CheckPointForNew
			=> throw new ControllerShowNewFormNotSupportedException("ShowNewForm is not supported for this controller.");

		protected override SecurityCheckpoint CheckPointForDelete
			=> throw new ControllerShowDeleteFormNotSupportedException("ShowDeleteForm is not supported for this controller.");
	}
}
