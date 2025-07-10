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
	public class RefCountryController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefCountry; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefCountry; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefCountry); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCountryForm((RefCountry)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CountriesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CountriesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CountriesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CountriesView; }
		}
	}
}
