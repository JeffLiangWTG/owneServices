using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsInventoryHeldCodeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ID

		public override ControllerID ID
		{
			get { return ControllerIDs.WhsInventoryHeldCodes; }
		}

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsInventoryHeldCodes; }
		}

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsInventoryHeldCode); }
		}

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new WhsInventoryHeldCodeEntryForm((WhsInventoryHeldCode)businessEntity);
		}

		#endregion

		#region CheckPointForView

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WhsConfigInventoryHeldCodeView; }
		}

		#endregion

		#region CheckPointForNew

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WhsConfigInventoryHeldCodeNew; }
		}

		#endregion

		#region CheckPointForEdit

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WhsConfigInventoryHeldCodeEdit; }
		}

		#endregion

		#region CheckPointForDelete

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WhsConfigInventoryHeldCodeDelete; }
		}

		#endregion
	}
}
