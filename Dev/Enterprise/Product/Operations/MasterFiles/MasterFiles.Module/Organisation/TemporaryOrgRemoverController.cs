using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.TemporaryOrgRemover;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class TemporaryOrgRemoverController : ZSingletonController
	{
		public TemporaryOrgRemoverController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TemporaryOrgRemoverForm(new Remover());
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationDelete; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.TemporaryOrgRemover; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgHeader); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
