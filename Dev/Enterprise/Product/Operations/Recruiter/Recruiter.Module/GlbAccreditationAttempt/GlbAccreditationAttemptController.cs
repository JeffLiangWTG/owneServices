using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationAttemptController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var attempt = businessEntity as GlbAccreditationAttempt;
			var person = attempt?.Person;
			var personForm = new GlbPersonForm(person);
			personForm.SetInitialAttempt(attempt);
			return personForm;
		}

		public override IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("32a8fe44-30fd-439e-a867-e0edcdf7423b", "You are not allowed to add new accreditation attempts manually"));
			return null;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			Globals.Message.Show(Res.GetString("55844107-d1b2-4f94-8d63-d0185b56afd6", "Multiple attempts can be deleted at once through this grid by highlighting multiple rows and clicking delete. Deleting a single attempt must be done through the Person form (Double-click the attempt and delete the highlighted record in the grid on the Person form)."));
			return null;
		}

		public override ControllerID ID => ControllerIDs.GlbAccreditationAttempt;
		public override ModuleIdentifier ModuleID => ModuleIDs.GlbAccreditationAttempt;

		public override Type TypeOfTopLevelBusinessObject => typeof(GlbAccreditationAttempt);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlbAccreditationAttemptView;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlbAccreditationAttemptEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlbAccreditationAttemptDelete;
	}
}
