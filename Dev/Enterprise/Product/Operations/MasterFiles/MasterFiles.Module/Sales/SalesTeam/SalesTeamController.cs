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
	public class SalesTeamController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public SalesTeamController()
		{
		}

		#region Standard Controller Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SalesTeam; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.SalesTeam; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(SalesTeam); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new SalesTeamForm((SalesTeam)businessEntity);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.SalesTeamsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SalesTeamsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SalesTeamsNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SalesTeamsView; }
		}

		#endregion
	}
}
