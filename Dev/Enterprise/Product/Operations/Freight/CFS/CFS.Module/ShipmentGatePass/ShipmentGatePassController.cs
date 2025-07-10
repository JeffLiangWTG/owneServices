using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	/// <summary>
	/// Module Controller for GatePass.
	/// </summary>
	public class ShipmentGatePassController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public ShipmentGatePassController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.ShipmentGatePass;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ShipmentGatePass; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GatePassShipment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var gatePassShipment = (GatePassShipment)businessEntity;
			gatePassShipment.IsRoot = true;
			return new ShipmentGatePassForm(gatePassShipment);
		}

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("Gate Pass does not allow creation of new records");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CFSGatePass; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CFSGatePassModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CFSGatePassModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CFSGatePassModify; }
		}

		#region SetStrategyProvider

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			factory.SetFreightDomainContext(FreightDomainContext.CFS);
		}

		#endregion
	}
}
