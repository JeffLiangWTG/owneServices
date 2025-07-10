using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class HREmailsController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.MailItem);
			var internals = controller as ZControllerInternals;
			return internals.GetForm(businessEntity);
		}

		public override ControllerID ID => ControllerIDs.HREmails;

		public override ModuleIdentifier ModuleID => ModuleIDs.HREmails;

		public override Type TypeOfTopLevelBusinessObject => typeof(HREmails);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.HREmailsView;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.HREmailsDelete;
	}
}
