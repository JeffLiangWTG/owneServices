using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module
{
	public class BookingController : AgencyShipmentController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AgencyBooking; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyBooking; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AgencyBooking); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AgencyBookingForm(businessEntity as AgencyBooking);
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var result = base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			if (result is AgencyBooking booking)
			{
				AgencyBooking.SetCurrentBookingInfo(Factory, booking.PK, booking.JS_ShipmentStatus);
			}

			return result;
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			if (OpenedFormCache.GetInstance().Contains(sourceEntityPK.ToGuid(), ControllerIDs.AgencyBillOfLading.ToString()))
			{
				return null;
			}

			return factory.Load(JobShipmentSchema.Constants.Prefix, sourceEntityPK) as AgencyBooking;
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return Res.GetString("b4ca9f9c-d138-8680-4c5f-736a43c5cf2f",
					"This Booking has been confirmed or is in the process of being converted to a Bill of Lading and is not currently accessible from the Bookings module.");
			}
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = base.GetNewBusinessEntityInLocalFactory();
			if (result is AgencyBooking booking)
			{
				AgencyBooking.SetCurrentBookingInfo(Factory, booking.PK, booking.JS_ShipmentStatus);
			}

			return result;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AgencyBooking; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AgencyBookingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AgencyBookingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AgencyBookingDelete; }
		}

		#endregion

		#region CRM Security

		readonly AgencyBookingCRMSecurityProvider SecurityProvider = new AgencyBookingCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as AgencyBooking, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as AgencyBooking, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as AgencyBooking, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
