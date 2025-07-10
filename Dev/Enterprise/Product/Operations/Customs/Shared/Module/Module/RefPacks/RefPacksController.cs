using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Shared.Module
{
	public class RefPacksController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.RefPacks; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.RefPacks; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusRefPacks); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new MasterFiles.GUI.RefPacksForm((CusRefPacks)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.RefPacksView; }
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.RefPacksModify; }
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.RefPacksNew; }
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.RefPacksDelete; }
		}
	}
}
