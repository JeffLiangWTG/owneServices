using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingBookingValidation : ZValidation
	{
		public TrackingBookingValidation(TrackingBooking parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override void ValidateAll()
		{
			ValidateOrigin();
			ValidateDestination();
			ValidateMode();
			ValidateIsDomesticFreight();
			ValidateServiceLevel();

			ValidateThirdPartyAddressPK();

			ValidateCustomsEntryNumber();
			ValidateUniqueConsignRef();
			ValidateBookingReference();
			ValidateGoodsDescription();
			ValidateOuterPacks();
			ValidateOuterPacksPackType();
			ValidateActualWeight();
			ValidateUnitOfWeight();
			ValidateActualVolume();
			ValidateUnitOfVolume();
			ValidateGoodsValue();
			ValidateGoodsValueCurr();
			ValidateInsuranceValue();
			ValidateInsuranceCurrency();
			ValidateShipperCODAmount();
			ValidateShipperCODPayMethod();
			ValidateA_RCV();
			ValidateINCO();
			ValidateMarksAndNumbers();
			ValidateIsCancelled();

			ValidateOrderItemsAsString();
			ValidateEstimatedPickup();
			ValidatePickupRequiredBy();
			ValidateFCLPickupEquipmentNeeded();
			ValidateEstimatedDelivery();
			ValidateDeliveryRequiredBy();
			ValidateFCLDeliveryEquipmentNeeded();

			ValidateConsignorOrganisationPK();
			ValidateConsigneeOrganisationPK();
		}

		void ValidateThirdPartyAddressPK()
		{
			ValidateCalculatedProperty(Parent.ThirdPartyAddressPKInfo);
		}

		protected virtual void CheckThirdPartyAddressPK()
		{
			if (Parent.IsDomesticFreight && Parent.INCO == Core.Constants.DomesticPaymentTerms.CollectThirdParty)
			{
				MandatoryValidation.CheckEntered(Parent.ThirdPartyAddressPKInfo);
			}
		}

		#region Origin

		public void ValidateOrigin()
		{
			ValidateCalculatedProperty(Parent.OriginInfo);
		}

		protected virtual void CheckOrigin()
		{
			ProxyValidation(Parent.OriginInfo, Parent.QuotedBooking.Booking.JS_RL_NKOriginInfo);
		}

		#endregion

		#region Destination

		public void ValidateDestination()
		{
			ValidateCalculatedProperty(Parent.DestinationInfo);
		}

		protected virtual void CheckDestination()
		{
			ProxyValidation(Parent.DestinationInfo, Parent.QuotedBooking.Booking.JS_RL_NKDestinationInfo);
		}

		#endregion

		#region Mode

		public void ValidateMode()
		{
			ValidateCalculatedProperty(Parent.ModeInfo);
		}

		protected virtual void CheckMode()
		{
			ProxyValidation(Parent.ModeInfo, Parent.QuotedBooking.ModeInfo);
		}

		#endregion

		#region IsDomesticFreight

		public void ValidateIsDomesticFreight()
		{
			ValidateCalculatedProperty(Parent.IsDomesticFreightInfo);
		}

		protected virtual void CheckIsDomesticFreight()
		{
			ProxyValidation(Parent.IsDomesticFreightInfo, Parent.QuotedBooking.IsDomesticFreightInfo);
		}

		#endregion

		#region ServiceLevel

		public void ValidateServiceLevel()
		{
			ValidateCalculatedProperty(Parent.ServiceLevelInfo);
		}

		protected virtual void CheckServiceLevel()
		{
			ProxyValidation(Parent.ServiceLevelInfo, Parent.QuotedBooking.ServiceLevelInfo);
		}

		#endregion

		#region CustomsEntryNumber

		public void ValidateCustomsEntryNumber()
		{
			ValidateCalculatedProperty(Parent.CustomsEntryNumberInfo);
		}

		protected virtual void CheckCustomsEntryNumber()
		{
			ProxyValidation(Parent.CustomsEntryNumberInfo, Parent.QuotedBooking.Booking.CustomsEntryNumberInfo);
		}

		#endregion

		#region UniqueConsignRef

		public void ValidateUniqueConsignRef()
		{
			ValidateCalculatedProperty(Parent.UniqueConsignRefInfo);
		}

		protected virtual void CheckUniqueConsignRef()
		{
			ProxyValidation(Parent.UniqueConsignRefInfo, Parent.QuotedBooking.Booking.JS_UniqueConsignRefInfo);
		}

		#endregion

		#region BookingReference

		public void ValidateBookingReference()
		{
			ValidateCalculatedProperty(Parent.BookingReferenceInfo);
		}

		protected virtual void CheckBookingReference()
		{
			ProxyValidation(Parent.BookingReferenceInfo, Parent.QuotedBooking.Booking.JS_BookingReferenceInfo);
		}

		#endregion

		#region GoodsDescription

		public void ValidateGoodsDescription()
		{
			ValidateCalculatedProperty(Parent.GoodsDescriptionInfo);
		}

		protected virtual void CheckGoodsDescription()
		{
			ProxyValidation(Parent.GoodsDescriptionInfo, Parent.QuotedBooking.Booking.JS_GoodsDescriptionInfo);
		}

		#endregion

		#region OuterPacks

		public void ValidateOuterPacks()
		{
			ValidateCalculatedProperty(Parent.OuterPacksInfo);
		}

		protected virtual void CheckOuterPacks()
		{
			ProxyValidation(Parent.OuterPacksInfo, Parent.QuotedBooking.Booking.JS_OuterPacksInfo);
		}

		#endregion

		#region OuterPacksPackType

		public void ValidateOuterPacksPackType()
		{
			ValidateCalculatedProperty(Parent.OuterPacksPackTypeInfo);
		}

		protected virtual void CheckOuterPacksPackType()
		{
			ProxyValidation(Parent.OuterPacksPackTypeInfo, Parent.QuotedBooking.Booking.JS_F3_NKPackTypeInfo);
		}

		#endregion

		#region ActualWeight

		public void ValidateActualWeight()
		{
			ValidateCalculatedProperty(Parent.ActualWeightInfo);
		}

		protected virtual void CheckActualWeight()
		{
			ProxyValidation(Parent.ActualWeightInfo, Parent.QuotedBooking.Booking.JS_ActualWeightInfo);
		}

		#endregion

		#region UnitOfWeight

		public void ValidateUnitOfWeight()
		{
			ValidateCalculatedProperty(Parent.UnitOfWeightInfo);
		}

		protected virtual void CheckUnitOfWeight()
		{
			ProxyValidation(Parent.UnitOfWeightInfo, Parent.QuotedBooking.Booking.JS_UnitOfWeightInfo);
		}

		#endregion

		#region ActualVolume

		public void ValidateActualVolume()
		{
			ValidateCalculatedProperty(Parent.ActualVolumeInfo);
		}

		protected virtual void CheckActualVolume()
		{
			ProxyValidation(Parent.ActualVolumeInfo, Parent.QuotedBooking.Booking.JS_ActualVolumeInfo);
		}

		#endregion

		#region UnitOfVolume

		public void ValidateUnitOfVolume()
		{
			ValidateCalculatedProperty(Parent.UnitOfVolumeInfo);
		}

		protected virtual void CheckUnitOfVolume()
		{
			ProxyValidation(Parent.UnitOfVolumeInfo, Parent.QuotedBooking.Booking.JS_UnitOfVolumeInfo);
		}

		#endregion

		#region GoodsValue

		public void ValidateGoodsValue()
		{
			ValidateCalculatedProperty(Parent.GoodsValueInfo);
		}

		protected virtual void CheckGoodsValue()
		{
			ProxyValidation(Parent.GoodsValueInfo, Parent.QuotedBooking.Booking.JS_GoodsValueInfo);
		}

		#endregion

		#region GoodsValueCurr

		public void ValidateGoodsValueCurr()
		{
			ValidateCalculatedProperty(Parent.GoodsValueCurrInfo);
		}

		protected virtual void CheckGoodsValueCurr()
		{
			ProxyValidation(Parent.GoodsValueCurrInfo, Parent.QuotedBooking.Booking.JS_RX_NKGoodsValueCurrInfo);
		}

		#endregion

		#region InsuranceValue

		public void ValidateInsuranceValue()
		{
			ValidateCalculatedProperty(Parent.InsuranceValueInfo);
		}

		protected virtual void CheckInsuranceValue()
		{
			ProxyValidation(Parent.InsuranceValueInfo, Parent.QuotedBooking.Booking.JS_InsuranceValueInfo);
		}

		#endregion

		#region InsuranceCurrency

		public void ValidateInsuranceCurrency()
		{
			ValidateCalculatedProperty(Parent.InsuranceCurrencyInfo);
		}

		protected virtual void CheckInsuranceCurrency()
		{
			ProxyValidation(Parent.InsuranceCurrencyInfo, Parent.QuotedBooking.Booking.JS_RX_NKInsuranceCurrencyInfo);
		}

		#endregion

		#region ShipperCODAmount

		public void ValidateShipperCODAmount()
		{
			ValidateCalculatedProperty(Parent.ShipperCODAmountInfo);
		}

		protected virtual void CheckShipperCODAmount()
		{
			ProxyValidation(Parent.ShipperCODAmountInfo, Parent.QuotedBooking.Booking.JS_ShipperCODAmountInfo);
		}

		#endregion

		#region ShipperCODPayMethod

		public void ValidateShipperCODPayMethod()
		{
			ValidateCalculatedProperty(Parent.ShipperCODPayMethodInfo);
		}

		protected virtual void CheckShipperCODPayMethod()
		{
			ProxyValidation(Parent.ShipperCODPayMethodInfo, Parent.QuotedBooking.Booking.JS_ShipperCODPayMethodInfo);
		}

		#endregion

		#region A_RCV

		public void ValidateA_RCV()
		{
			ValidateCalculatedProperty(Parent.A_RCVInfo);
		}

		protected virtual void CheckA_RCV()
		{
			ProxyValidation(Parent.A_RCVInfo, Parent.QuotedBooking.Booking.JS_A_RCVInfo);
		}

		#endregion

		#region INCO

		public void ValidateINCO()
		{
			ValidateCalculatedProperty(Parent.INCOInfo);
		}

		protected virtual void CheckINCO()
		{
			ProxyValidation(Parent.INCOInfo, Parent.QuotedBooking.Booking.JS_INCOInfo);
		}

		#endregion

		#region MarksAndNumbers

		public void ValidateMarksAndNumbers()
		{
			ValidateCalculatedProperty(Parent.MarksAndNumbersInfo);
		}

		protected virtual void CheckMarksAndNumbers()
		{
			ProxyValidation(Parent.MarksAndNumbersInfo, Parent.QuotedBooking.Booking.JS_MarksAndNumbersInfo);
		}

		#endregion

		#region IsCancelled

		public void ValidateIsCancelled()
		{
			ValidateCalculatedProperty(Parent.IsCancelledInfo);
		}

		protected virtual void CheckIsCancelled()
		{
			ProxyValidation(Parent.IsCancelledInfo, Parent.QuotedBooking.Booking.JS_IsCancelledInfo);
		}

		#endregion

		#region OrderItemsAsString

		public void ValidateOrderItemsAsString()
		{
			ValidateCalculatedProperty(Parent.OrderItemsAsStringInfo);
		}

		protected virtual void CheckOrderItemsAsString()
		{
			ProxyValidation(Parent.OrderItemsAsStringInfo, Parent.QuotedBooking.Booking.DocsAndCartage.JP_OrderItemsAsStringInfo);
		}

		#endregion

		#region EstimatedPickup

		public void ValidateEstimatedPickup()
		{
			ValidateCalculatedProperty(Parent.EstimatedPickupInfo);
		}

		protected virtual void CheckEstimatedPickup()
		{
			ProxyValidation(Parent.EstimatedPickupInfo, Parent.QuotedBooking.Booking.DocsAndCartage.JP_EstimatedPickupInfo);
		}

		#endregion

		#region PickupRequiredBy

		public void ValidatePickupRequiredBy()
		{
			ValidateCalculatedProperty(Parent.PickupRequiredByInfo);
		}

		protected virtual void CheckPickupRequiredBy()
		{
			ProxyValidation(Parent.PickupRequiredByInfo, Parent.QuotedBooking.Booking.DocsAndCartage.JP_PickupRequiredByInfo);
		}

		#endregion

		#region FCLPickupEquipmentNeeded

		public void ValidateFCLPickupEquipmentNeeded()
		{
			ValidateCalculatedProperty(Parent.FCLPickupEquipmentNeededInfo);
		}

		protected virtual void CheckFCLPickupEquipmentNeeded()
		{
			ProxyValidation(Parent.FCLPickupEquipmentNeededInfo, Parent.QuotedBooking.Booking.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo);
		}

		#endregion

		#region EstimatedDelivery

		public void ValidateEstimatedDelivery()
		{
			ValidateCalculatedProperty(Parent.EstimatedDeliveryInfo);
		}

		protected virtual void CheckEstimatedDelivery()
		{
			ProxyValidation(Parent.EstimatedDeliveryInfo, Parent.QuotedBooking.Booking.DocsAndCartage.JP_EstimatedDeliveryInfo);
		}

		#endregion

		#region DeliveryRequiredBy

		public void ValidateDeliveryRequiredBy()
		{
			ValidateCalculatedProperty(Parent.DeliveryRequiredByInfo);
		}

		protected virtual void CheckDeliveryRequiredBy()
		{
			ProxyValidation(Parent.DeliveryRequiredByInfo, Parent.QuotedBooking.Booking.DocsAndCartage.JP_DeliveryRequiredByInfo);
		}

		#endregion

		#region FCLDeliveryEquipmentNeeded

		public void ValidateFCLDeliveryEquipmentNeeded()
		{
			ValidateCalculatedProperty(Parent.FCLDeliveryEquipmentNeededInfo);
		}

		protected virtual void CheckFCLDeliveryEquipmentNeeded()
		{
			ProxyValidation(Parent.FCLDeliveryEquipmentNeededInfo, Parent.QuotedBooking.Booking.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo);
		}

		#endregion

		public void ValidateConsignorOrganisationPK()
		{
			Parent.ConsignorPickupAddress.Validation.ValidateAll();
			ValidateCalculatedProperty(Parent.ConsignorOrganisationPKInfo);
		}

		protected virtual void CheckConsignorOrganisationPK()
		{
			ProxyValidation(Parent.ConsignorOrganisationPKInfo, Parent.QuotedBooking.Booking.ConsignorPickupAddress.OrganisationPKInfo);
		}

		public void ValidateConsigneeOrganisationPK()
		{
			Parent.ConsigneeDeliveryAddress.Validation.ValidateAll();
			ValidateCalculatedProperty(Parent.ConsigneeOrganisationPKInfo);
		}

		protected virtual void CheckConsigneeOrganisationPK()
		{
			ProxyValidation(Parent.ConsigneeOrganisationPKInfo, Parent.QuotedBooking.Booking.ConsigneeDeliveryAddress.OrganisationPKInfo);
		}

		public override Type AutoValidationType
		{
			get { return typeof(TrackingBooking); }
		}

		#region Implementation

		#region ProxyValidation

		bool ProxyValidation(ZPropertyInfo toInfo, ZPropertyInfo fromInfo)
		{
			bool success = false;

			if (!fromInfo.BizObj.IsValidationSuspended)
			{
				((IBusinessObjectInternals)fromInfo.BizObj).Validate(fromInfo);
				toInfo.AddAllNotificationsFrom(fromInfo);
				success = true;
			}

			return success;
		}

		#endregion

		readonly TrackingBooking Parent;

		#endregion
	}

	public class TrackingPickupDeliveryAddressValidation : JobDocAddressValidation
	{
		public TrackingPickupDeliveryAddressValidation(JobDocAddress parent)
			: base(parent)
		{
		}

		#region OrganisationPK

		protected override void CheckOrganisationPK()
		{
			if (!Parent.OrganisationPKInfo.HasErrors() &&
				!Parent.IsValidAddress)
			{
				if (Parent.E2_AddressType == MasterFiles.Integration.AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress)
				{
					Parent.OrganisationPKInfo.AddError(Res.GetString("019ddb92-51ed-4d12-b6d0-009e1ca050a5", "Pickup address is not entered"));
				}
				else if (Parent.E2_AddressType == MasterFiles.Integration.AutoDocAddressTypes.Codes.ConsigneePickupDeliveryAddress)
				{
					Parent.OrganisationPKInfo.AddError(Res.GetString("6ec9acc7-108d-40c0-8717-c45507c06cf2", "Delivery address is not entered"));
				}
			}
		}

		#endregion
	}
}
