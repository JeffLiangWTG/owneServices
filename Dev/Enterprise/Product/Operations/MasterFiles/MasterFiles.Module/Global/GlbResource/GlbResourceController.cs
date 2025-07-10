using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbResourceController : GlbStaffController
	{
		public GlbResourceController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbResource; }
		}

		#region Form

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbResourceForm((GlbStaff)businessEntity);
		}

		public override IZForm ShowNewForm()
		{
			GlbResourceForm form = (GlbResourceForm)base.ShowNewForm();
			if (form != null)
			{
				using (form.Resource.SuspendSettingHasChanges())
				{
					form.Resource.GS_IsResource = true;
				}
			}
			return form;
		}

		#endregion

		#region Security CheckPoints

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ResourceEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ResourceEdit; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ResourceEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Staff; }
		}

		#endregion
	}
}
