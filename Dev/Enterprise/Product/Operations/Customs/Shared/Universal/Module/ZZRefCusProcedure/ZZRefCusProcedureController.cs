using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusProcedureController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.Universal.ZZRefCusProcedure; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.Universal.ZZRefCusProcedure; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefCusProcedure); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCusProcedureForm((RefCusProcedure)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GlobalCodesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GlobalCodesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GlobalCodesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GlobalCodesView; }
		}
	}
}
