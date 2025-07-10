using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Confirmations.Module
{
	public class ConfirmationsPluginController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.ConfirmationsPlugin; }
		}

		#region Security checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PickupDeliveryConfirmationsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PickupDeliveryConfirmationsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PickupDeliveryConfirmationsNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PickupDeliveryConfirmations; }
		}

		#endregion

		#region Not Supported

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not supported");
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#endregion

		#region Plugin

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			if (businessEntity == null)
			{
				throw new ArgumentException("BusinessEntity is null, ensure tab control is added to the form before adding the plugin");
			}

			IConfirmationsHost confirmationsParent = businessEntity as IConfirmationsHost ?? throw new NotSupportedException("BusinessEntity is does not implement IConfirmationsHost");

			return new ConfirmationsPlugin(confirmationsParent);
		}

		#endregion
	}
}
