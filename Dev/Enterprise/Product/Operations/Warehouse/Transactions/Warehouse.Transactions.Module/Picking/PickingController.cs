using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickingController : WhsControllerBase
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public PickingController()
			: this(PickType.Codes.Order)
		{
		}

		public PickingController(string pickType)
		{
			this.pickType = pickType;
		}

		readonly string pickType;

		public override ControllerID ID => ControllerIDs.WhsPicking;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsPicking;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsPick);

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			return new PickEntryForm((WhsPick)businessEntity, whsNotificationSubscriberGuiHelper);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsPickingView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsPickingNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsPickingEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var pick = (WhsPick)base.GetNewBusinessEntityInLocalFactory();
			pick.WP_PickType = pickType;
			return pick;
		}
	}
}
