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
	public class EDIMessageDeliveryContextController : ZController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Messaging.EDIMessageDeliveryContext;
		public override ControllerID ID => ControllerIDs.Messaging.EDIMessageDeliveryContext;
		public override Type TypeOfTopLevelBusinessObject => typeof(EDIMessageDeliveryContextSelector);
		protected override IZForm GetForm(IBusiness businessEntity) => new EDIMessageDeliveryContextForm((EDIMessageDeliveryContextSelector)businessEntity);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.EDIMessageDeliveryContextView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EDIMessageDeliveryContextEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.EDIMessageDeliveryContextNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.EDIMessageDeliveryContextDelete;

		#endregion
	}
}
