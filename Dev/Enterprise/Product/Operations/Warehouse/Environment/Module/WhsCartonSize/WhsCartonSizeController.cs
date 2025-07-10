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
	public class WhsCartonSizeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ID

		public override ControllerID ID
		{
			get { return ControllerIDs.WhsCartonSize; }
		}

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsCartonSize; }
		}

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsCartonSize); }
		}

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WhsCartonSizeEntryForm((WhsCartonSize)businessEntity);
		}

		#endregion

		#region CheckPointForView

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WhsConfigCartonSizeView; }
		}

		#endregion

		#region CheckPointForNew

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WhsConfigCartonSizeNew; }
		}

		#endregion

		#region CheckPointForEdit

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WhsConfigCartonSizeEdit; }
		}

		#endregion

		#region CheckPointForDelete

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WhsConfigCartonSizeDelete; }
		}

		#endregion
	}
}
