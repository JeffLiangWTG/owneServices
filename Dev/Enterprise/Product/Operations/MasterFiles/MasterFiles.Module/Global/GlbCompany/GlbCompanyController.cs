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
	public class GlbCompanyController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbCompany; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbCompany; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbCompany); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbCompanyForm((GlbCompany)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CompaniesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CompaniesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CompaniesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CompaniesView; }
		}

		#endregion
	}
}
