using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	class WhsSalesChannelController : ZController
	{
		public override ControllerID ID => ControllerIDs.WhsSalesChannel;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsSalesChannel;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsSalesChannel);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigSalesChannelView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsConfigSalesChannelNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigSalesChannelEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsConfigSalesChannelDelete;

		protected override IZForm GetForm(IBusiness businessEntity) => new WhsSalesChannelEntryForm((WhsSalesChannel)businessEntity);
	}
}
