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
	internal class ExternalRequestTypesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.ExternalRequestTypes;

		public override Type TypeOfTopLevelBusinessObject => typeof(ExternalRequestType);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ExternalRequestTypeForm((ExternalRequestType)businessEntity);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.ExternalRequestTypes;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ExternalRequestTypesView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ExternalRequestTypesEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ExternalRequestTypesNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ExternalRequestTypesDeactivate;

		#endregion
	}
}
