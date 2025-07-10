using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class USOrgBuyerSupplierLinkAdditionalCustomsDetailsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.US.OrgBuyerSupplierLinkAdditionalCustomsDetails;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierBuyerLink);

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|OrgBuyerSupplierLinkAdditionalCustomsDetails", "Additional Customs Defaults");

		protected override SecurityCheckpoint CheckPointForView => Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("This function is not supported.");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new USOrgAdditionalCustomsDefaultsPlugIn(businessEntity);
	}
}
