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
	public class UNDGCommonDataController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Standard Module Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.UNDGCommonData; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.UNDGCommonData; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(UNDGCommonData); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new UNDGCommonDataForm((UNDGCommonData)businessEntity);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.UNDGSubstance; }
		}

		#endregion
	}
}
