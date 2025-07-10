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
	public class RefUNLOCOController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.UNLOCODelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.UNLOCOModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.UNLOCONew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.UNLOCOView; }
		}

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefUNLOCOForm((RefUNLOCO)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefUNLOCO; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefUNLOCO; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefUNLOCO); }
		}
	}
}
