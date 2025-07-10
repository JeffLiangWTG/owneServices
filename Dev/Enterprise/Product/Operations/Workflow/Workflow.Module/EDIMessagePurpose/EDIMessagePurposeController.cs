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
	class EDIMessagePurposeController : ZController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Messaging.EDIMessagePurpose;
		public override ControllerID ID => ControllerIDs.Messaging.EDIMessagePurpose;
		public override Type TypeOfTopLevelBusinessObject => typeof(EDIMessagePurpose);
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is EDIMessagePurpose purpose)
			{
				return new EDIMessagePurposeForm(purpose);
			}
			else
			{
				throw new InvalidOperationException("Unknown type.");
			}
		}
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.EDIMessagePurposeView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EDIMessagePurposeEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.EDIMessagePurposeNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.EDIMessagePurposeDelete;

		#endregion
	}
}
