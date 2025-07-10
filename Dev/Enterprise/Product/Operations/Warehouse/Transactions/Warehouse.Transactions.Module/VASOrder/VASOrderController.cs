using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class VASOrderController : WhsControllerBase
	{
		#region ID

		public override ControllerID ID
		{
			get { return ControllerIDs.WhsVASOrder; }
		}

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsVASOrder; }
		}

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(WhsVASOrder); }
		}

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness vasOrder, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			return new VASOrderEntryForm((WhsVASOrder)vasOrder, whsNotificationSubscriberGuiHelper);
		}

		#endregion

		#region SecurityCheckpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.WhsVASOrderView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.WhsVASOrderNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.WhsVASOrderEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.WhsVASOrderDelete; }
		}

		#endregion
	}
}
