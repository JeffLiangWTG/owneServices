using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class UpdateRatesController : ZSingletonController
	{
		public UpdateRatesController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GRIUpdateForm();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ClientRatesUpdate; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ClientRateUpdates; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ClientRate); }
		}
	}
}

