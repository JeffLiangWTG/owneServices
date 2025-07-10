using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.GlobalCommercialInvoice.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	public class GlobalCommercialInvoiceController : ZController
	{
		public override ResourceStringData PluginTabPageCaption => Constants.PluginTabPageCaption;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new GlobalCommercialInvoicePlugin(businessEntity);

		public override ControllerID ID => ControllerIDs.GlobalCommercialInvoicePlugin;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("Not supported");

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("This controller does not have GUI.");

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject) => InvoiceSecurityProvider.GetCheckPointForView(bizObject);

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject) => InvoiceSecurityProvider.GetCheckPointForEdit(bizObject);
	}
}
