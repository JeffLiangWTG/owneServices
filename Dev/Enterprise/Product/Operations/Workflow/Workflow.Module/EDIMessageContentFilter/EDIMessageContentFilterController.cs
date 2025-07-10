using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Workflow.Module
{
	class EDIMessageContentFilterController : ZController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Messaging.EDIMessageContentFilter;
		public override ControllerID ID => ControllerIDs.Messaging.EDIMessageContentFilter;
		public override Type TypeOfTopLevelBusinessObject => typeof(EDIMessageContentFilter);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is EDIMessageContentFilter filter)
			{
				return new EDIMessageContentFilterForm(filter);
			}
			else
			{
				throw new InvalidOperationException("Unknown type");
			}
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.EDIMessageContentFilterView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EDIMessageContentFilterEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.EDIMessageContentFilterNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.EDIMessageContentFilterDelete;

		#endregion
	}
}
