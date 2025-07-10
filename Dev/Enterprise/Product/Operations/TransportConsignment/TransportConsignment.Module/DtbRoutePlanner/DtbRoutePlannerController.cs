using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbRoutePlannerController : ZPopupController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DtbRoutePlannerForm(DtbRoutePlannerCollection.New(Factory));
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DtbRoutePlanner; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return null; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		#region New Form

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DtbRoutePlanner; }
		}

		#endregion
	}
}
