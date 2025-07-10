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
	class EDICodeMappingController : ZController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Messaging.EDICodeMapping;
		public override ControllerID ID => ControllerIDs.Messaging.EDICodeMapping;
		public override Type TypeOfTopLevelBusinessObject => typeof(OrgPatternMatchOverride);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDICodeMappingForm(businessEntity as OrgPatternMatchOverride);
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.EDICodeMappingView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EDICodeMappingEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.EDICodeMappingNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.EDICodeMappingDelete;

		#endregion
	}
}
