using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public class WhsCartonGroupController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ID

		public override ControllerID ID
		{
			get { return ControllerIDs.WhsCartonGroup; }
		}

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsCartonGroup; }
		}

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsCartonGroup); }
		}

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WhsCartonGroupEntryForm((WhsCartonGroup)businessEntity);
		}

		#endregion

		#region CheckPointForView

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WhsConfigCartonGroupView; }
		}

		#endregion

		#region CheckPointForNew

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WhsConfigCartonGroupNew; }
		}

		#endregion

		#region CheckPointForEdit

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WhsConfigCartonGroupEdit; }
		}

		#endregion

		#region CheckPointForDelete

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WhsConfigCartonGroupDelete; }
		}

		#endregion
	}
}
