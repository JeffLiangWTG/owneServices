using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public class BillOfLadingController : AgencyShipmentController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyBillOfLading; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AgencyBillOfLading; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BillOfLading); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new BillOfLadingForm(businessEntity as BillOfLading);
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			if (OpenedFormCache.GetInstance().Contains(sourceEntityPK.ToGuid(), ControllerIDs.AgencyBooking.ToString()))
			{
				return null;
			}

			var billOfLading = base.LoadBusinessEntity(factory, sourceEntityPK) as BillOfLading;
			if (billOfLading?.IsBillOfLadingStage != true)
			{
				return null;
			}

			return billOfLading;
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return Res.GetString(
					"4dc12fee-dc17-4d82-46f8-774cea1a8a22",
					"This is a Booking that has not been confirmed to Bill of Lading and is not accessible from the Bill of Lading module. Please access from the Booking module.");
			}
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AgencyBillOfLading; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AgencyBillOfLadingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AgencyBillOfLadingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AgencyBillOfLadingDelete; }
		}

		#endregion

		#region CRM Security

		readonly BillOfLadingCRMSecurityProvider SecurityProvider = new BillOfLadingCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as BillOfLading, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as BillOfLading, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as BillOfLading, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}




