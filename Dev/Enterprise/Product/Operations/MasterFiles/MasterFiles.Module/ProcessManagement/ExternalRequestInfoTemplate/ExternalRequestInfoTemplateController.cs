using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	internal class ExternalRequestInfoTemplateController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.ExternalRequestInfoTemplate;

		public override Type TypeOfTopLevelBusinessObject => typeof(ExternalRequestInfoTemplate);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ExternalRequestInfoTemplateForm((ExternalRequestInfoTemplate)businessEntity);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.ExternalRequestInfoTemplate;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ExternalRequestInfoTemplateView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ExternalRequestInfoTemplateEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ExternalRequestInfoTemplateNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ExternalRequestInfoTemplateDeactivate;

		#endregion
	}
}
