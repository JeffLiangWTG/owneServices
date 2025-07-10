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
	class RefOrgPartCategoryController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RefOrgPartCategoryController()
		{ }

		public override ControllerID ID
		{
			get { return ControllerIDs.RefOrgPartCategory; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.RefOrgPartCategoryModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.RefOrgPartCategoryModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.RefOrgPartCategoryModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.RefOrgPartCategory; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefOrgPartCategoryForm((OrgPartCategory)businessEntity);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgPartCategory); }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.RefOrgPartCategory;
			}
		}
	}
}
