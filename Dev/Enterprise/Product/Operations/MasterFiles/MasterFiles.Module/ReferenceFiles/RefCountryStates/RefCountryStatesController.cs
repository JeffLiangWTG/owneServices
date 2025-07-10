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
	public class RefCountryStatesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.RefCountryStates; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.RefCountryStates;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefCountryStates); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCountryStatesForm((RefCountryStates)businessEntity);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.StatesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.StatesModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.StatesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.StatesView; }
		}

		#endregion
	}
}
