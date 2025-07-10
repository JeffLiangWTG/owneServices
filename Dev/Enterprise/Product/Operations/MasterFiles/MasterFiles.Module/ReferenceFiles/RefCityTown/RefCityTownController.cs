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
	public class RefCityTownController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CityTownDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CityTownModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CityTownNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CityTownView; }
		}

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCityTownForm((RefCityTown)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefCityTown; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefCityTown; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefCityTown); }
		}
	}
}
