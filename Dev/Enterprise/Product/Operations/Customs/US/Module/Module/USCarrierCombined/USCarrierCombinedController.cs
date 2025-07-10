using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCarrierCombinedController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.US.USCarrierCombined;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.Carrier;

		public override Type TypeOfTopLevelBusinessObject => typeof(USCarrierCombined);

		protected override IZForm GetForm(IBusiness businessEntity) => new USCarrierCombinedForm((USCarrierCombined)businessEntity);

		protected override Security.SecurityCheckpoint CheckPointForDelete => Env.Security.USCustomsCarrierDelete;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Env.Security.USCustomsCarrierEdit;

		protected override Security.SecurityCheckpoint CheckPointForNew => Env.Security.USCustomsCarrierNew;

		protected override Security.SecurityCheckpoint CheckPointForView => Env.Security.USCustomsCarrierView;

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
			=> base.AlreadyDeletedOrIrreversiblyChangedMessageCore + "\r\nIf you have just created a user-editable copy of a system-generated record, please re-run your search query and try again.";
	}
}
