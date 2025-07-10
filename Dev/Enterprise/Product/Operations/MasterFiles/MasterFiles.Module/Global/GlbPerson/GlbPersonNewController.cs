using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Global.GlbPerson;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	class GlbPersonNewController : GlbPersonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.GlbPersonNew; }
		}

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbPersonNewForm(businessEntity as GlbPerson);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var editForm = (GlbPersonNewForm)base.ShowEditForm(sourceEntity);
			editForm.SetEditMode();
			return editForm;
		}

		#endregion
	}
}
