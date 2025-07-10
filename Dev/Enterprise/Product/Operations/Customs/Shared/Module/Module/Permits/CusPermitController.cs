using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CusPermitController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public CusPermitController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.Permits; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.Permits; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BaseCusPermitHeader); }
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PermitsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PermitsView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PermitsNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PermitsDelete; }
		}

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusPermitForm((BaseCusPermitHeader)businessEntity);
		}
	}
}
