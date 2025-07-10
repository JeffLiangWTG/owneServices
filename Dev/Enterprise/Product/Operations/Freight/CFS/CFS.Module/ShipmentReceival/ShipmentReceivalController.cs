using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	/// <summary>
	/// Module Controller for ShipmentReceival.
	/// </summary>
	public class ShipmentReceivalController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public ShipmentReceivalController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.ShipmentReceival;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ShipmentReceival; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CFSShipment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var cfsShipment = (CFSShipment)businessEntity;
			cfsShipment.IsRoot = true;
			return new ShipmentReceivalForm(cfsShipment);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CFSShipment; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CFSShipmentModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CFSShipmentModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CFSShipmentModify; }
		}

		#region SetStrategyProvider

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Shipment);
			factory.SetFreightDomainContext(FreightDomainContext.CFS);
		}

		#endregion
	}
}
