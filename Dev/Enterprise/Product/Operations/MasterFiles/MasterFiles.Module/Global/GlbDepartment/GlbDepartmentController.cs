using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbDepartmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbDepartment; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbDepartment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbDepartment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbDepartmentForm((GlbDepartment)businessEntity);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			if (!sourceEntity.CanDelete)
			{
				Globals.Message.Show(sourceEntity.ReasonForNotAbleToDelete);
			}
			else
			{
				result = base.ShowDeleteForm(sourceEntity);
			}
			return result;
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DepartmentsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DepartmentsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DepartmentsNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DepartmentsView; }
		}
		#endregion
	}
}
