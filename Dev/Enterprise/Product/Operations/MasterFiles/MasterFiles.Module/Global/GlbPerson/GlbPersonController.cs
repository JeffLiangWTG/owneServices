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
	public class GlbPersonController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.GlbPerson;

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbPerson;

		public override Type TypeOfTopLevelBusinessObject => typeof(GlbPerson);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.PersonIntelligenceView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.PersonIntelligenceNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.PersonIntelligenceEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.PersonIntelligenceDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbPersonForm(businessEntity as GlbPerson);
		}
	}
}
