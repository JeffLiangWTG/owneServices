using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentForACASWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public HVLVConsignmentForACASWrapper(HVLVConsignment consignment)
		{
			Argument.NotNull(consignment, nameof(consignment));
			Consignment = consignment;
		}

		public HVLVConsignment Consignment { get; }

		#region Properties

		public bool ConsigneeIsOrganisation => Consignment.ConsigneeIsOrganisation;
		public bool ShipperIsOrganisation => Consignment.ShipperIsOrganisation;
		public ZString WaybillNumber => Consignment.HVC_WaybillNumber;
		public ZPropertyInfo WaybillNumberInfo => GetZPropertyInfo(nameof(WaybillNumber));

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ConsigneeNameMaxLength)]
		public ZString ConsigneeName
		{
			get { return Consignment.HVC_ConsigneeName; }
			set
			{
				Consignment.HVC_ConsigneeName = value;
				if (!IsValidationSuspended)
				{
					ValidateConsigneeName();
				}
				ConsigneeNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeNameInfo => GetZPropertyInfo(nameof(ConsigneeName));

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ConsigneeAddress1MaxLength)]
		public ZString ConsigneeAddress1
		{
			get { return Consignment.HVC_ConsigneeAddress1; }
			set
			{
				Consignment.HVC_ConsigneeAddress1 = value;
				if (!IsValidationSuspended)
				{
					ValidateConsigneeAddress1();
				}
				ConsigneeAddress1Info.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeAddress1Info => GetZPropertyInfo(nameof(ConsigneeAddress1));

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ConsigneeAddress2MaxLength)]
		public ZString ConsigneeAddress2
		{
			get { return Consignment.HVC_ConsigneeAddress2; }
			set { Consignment.HVC_ConsigneeAddress2 = value; }
		}

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ConsigneeCityMaxLength)]
		public ZString ConsigneeCity
		{
			get { return Consignment.HVC_ConsigneeCity; }
			set
			{
				Consignment.HVC_ConsigneeCity = value;
				if (!IsValidationSuspended)
				{
					ValidateConsigneeCity();
				}
				ConsigneeCityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeCityInfo => GetZPropertyInfo(nameof(ConsigneeCity));

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ConsigneeStateMaxLength)]
		public ZString ConsigneeState
		{
			get { return Consignment.HVC_ConsigneeState; }
			set
			{
				Consignment.HVC_ConsigneeState = value;
				if (!IsValidationSuspended)
				{
					ValidateConsigneeState();
				}
				ConsigneeStateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeStateInfo => GetZPropertyInfo(nameof(ConsigneeState));

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ConsigneePostcodeMaxLength)]
		public ZString ConsigneePostcode
		{
			get { return Consignment.HVC_ConsigneePostcode; }
			set
			{
				Consignment.HVC_ConsigneePostcode = value;
				if (!IsValidationSuspended)
				{
					ValidateConsigneePostcode();
				}
				ConsigneePostcodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneePostcodeInfo => GetZPropertyInfo(nameof(ConsigneePostcode));

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_RN_NKConsigneeCountryCodeMaxLength)]
		public ZString ConsigneeCountryCode
		{
			get { return Consignment.HVC_RN_NKConsigneeCountryCode; }
			set
			{
				Consignment.HVC_RN_NKConsigneeCountryCode = value;
				if (!IsValidationSuspended)
				{
					ValidateConsigneeCountryCode();
				}
				ConsigneeCountryCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeCountryCodeInfo => GetZPropertyInfo(nameof(ConsigneeCountryCode));

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ConsigneeMobileMaxLength)]
		public ZString ConsigneeMobile
		{
			get { return Consignment.HVC_ConsigneeMobile; }
			set
			{
				Consignment.HVC_ConsigneeMobile = value;
				if (!IsValidationSuspended)
				{
					ValidateConsigneeMobile();
				}
				ConsigneeMobileInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeMobileInfo => GetZPropertyInfo(nameof(ConsigneeMobile));

		[ReadOnlyMember(nameof(ConsigneeIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ConsigneeEmailMaxLength)]
		public ZString ConsigneeEmail
		{
			get { return Consignment.HVC_ConsigneeEmail; }
			set
			{
				Consignment.HVC_ConsigneeEmail = value;
				if (!IsValidationSuspended)
				{
					ValidateConsigneeEmail();
				}
				ConsigneeEmailInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeEmailInfo => GetZPropertyInfo(nameof(ConsigneeEmail));

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ShipperNameMaxLength)]
		public ZString ShipperName
		{
			get { return Consignment.HVC_ShipperName; }
			set
			{
				Consignment.HVC_ShipperName = value;
				if (!IsValidationSuspended)
				{
					ValidateShipperName();
				}
				ShipperNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShipperNameInfo => GetZPropertyInfo(nameof(ShipperName));

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ShipperAddress1MaxLength)]
		public ZString ShipperAddress1
		{
			get { return Consignment.HVC_ShipperAddress1; }
			set
			{
				Consignment.HVC_ShipperAddress1 = value;
				if (!IsValidationSuspended)
				{
					ValidateShipperAddress1();
				}
				ShipperAddress1Info.RefreshBinding();
			}
		}

		public ZPropertyInfo ShipperAddress1Info => GetZPropertyInfo(nameof(ShipperAddress1));

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ShipperAddress2MaxLength)]
		public ZString ShipperAddress2
		{
			get { return Consignment.HVC_ShipperAddress2; }
			set { Consignment.HVC_ShipperAddress2 = value; }
		}

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ShipperCityMaxLength)]
		public ZString ShipperCity
		{
			get { return Consignment.HVC_ShipperCity; }
			set
			{
				Consignment.HVC_ShipperCity = value;
				if (!IsValidationSuspended)
				{
					ValidateShipperCity();
				}
				ShipperCityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShipperCityInfo => GetZPropertyInfo(nameof(ShipperCity));

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ShipperStateMaxLength)]
		public ZString ShipperState
		{
			get { return Consignment.HVC_ShipperState; }
			set
			{
				Consignment.HVC_ShipperState = value;
				if (!IsValidationSuspended)
				{
					ValidateShipperState();
				}
				ShipperStateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShipperStateInfo => GetZPropertyInfo(nameof(ShipperState));

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ShipperPostcodeMaxLength)]
		public ZString ShipperPostcode
		{
			get { return Consignment.HVC_ShipperPostcode; }
			set
			{
				Consignment.HVC_ShipperPostcode = value;
				if (!IsValidationSuspended)
				{
					ValidateShipperPostcode();
				}
				ShipperPostcodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShipperPostcodeInfo => GetZPropertyInfo(nameof(ShipperPostcode));

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_RN_NKShipperCountryCodeMaxLength)]
		public ZString ShipperCountryCode
		{
			get { return Consignment.HVC_RN_NKShipperCountryCode; }
			set
			{
				Consignment.HVC_RN_NKShipperCountryCode = value;
				if (!IsValidationSuspended)
				{
					ValidateShipperCountryCode();
				}
				ShipperCountryCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShipperCountryCodeInfo => GetZPropertyInfo(nameof(ShipperCountryCode));

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ShipperMobileMaxLength)]
		public ZString ShipperMobile
		{
			get { return Consignment.HVC_ShipperMobile; }
			set
			{
				Consignment.HVC_ShipperMobile = value;
				if (!IsValidationSuspended)
				{
					ValidateShipperMobile();
				}
				ShipperMobileInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShipperMobileInfo => GetZPropertyInfo(nameof(ShipperMobile));

		[ReadOnlyMember(nameof(ShipperIsOrganisation))]
		[MaxLength(AutoHVLVConsignment.Schema.HVC_ShipperEmailMaxLength)]
		public ZString ShipperEmail
		{
			get { return Consignment.HVC_ShipperEmail; }
			set
			{
				Consignment.HVC_ShipperEmail = value;
				if (!IsValidationSuspended)
				{
					ValidateShipperEmail();
				}
				ShipperEmailInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShipperEmailInfo => GetZPropertyInfo(nameof(ShipperEmail));

		[MaxLength(AutoHVLVConsignment.Schema.HVC_GoodsDescriptionMaxLength)]
		public ZString GoodsDescription
		{
			get { return Consignment.HVC_GoodsDescription; }
			set
			{
				Consignment.HVC_GoodsDescription = value;
				if (!IsValidationSuspended)
				{
					ValidateGoodsDescription();
				}
				GoodsDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		public ZShort ItemCount => Consignment.HVC_ItemCount;

		public ZDecimal ManifestedWeight => Consignment.HVC_ManifestedWeight;

		public ZDecimal ActualWeight => Consignment.HVC_ActualWeight;

		public ZString ACASStatus => Consignment.HVC_ACASStatus;

		public ZString ACASInterchangeStatus => Consignment.HVC_ACASInterchangeStatus;

		public ZString ACASMessageStatus => Consignment.HVC_ACASMessageStatus;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateConsigneeName();
			ValidateConsigneeAddress1();
			ValidateConsigneeCity();
			ValidateConsigneeState();
			ValidateConsigneePostcode();
			ValidateConsigneeCountryCode();
			ValidateConsigneeEmail();
			ValidateConsigneeMobile();
			ValidateShipperName();
			ValidateShipperAddress1();
			ValidateShipperCity();
			ValidateShipperState();
			ValidateShipperPostcode();
			ValidateShipperCountryCode();
			ValidateShipperEmail();
			ValidateShipperMobile();
			ValidateGoodsDescription();
		}

		void ValidateConsigneeName()
		{
			ConsigneeNameInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ConsigneeNameInfo);
		}

		void ValidateConsigneeAddress1()
		{
			ConsigneeAddress1Info.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ConsigneeAddress1Info);
		}

		void ValidateConsigneeCity()
		{
			ConsigneeCityInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ConsigneeCityInfo);
		}

		void ValidateConsigneeState()
		{
			ConsigneeStateInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ConsigneeStateInfo);
		}

		void ValidateConsigneePostcode()
		{
			ConsigneePostcodeInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ConsigneePostcodeInfo);
		}

		void ValidateConsigneeCountryCode()
		{
			ConsigneeCountryCodeInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ConsigneeCountryCodeInfo);
		}

		void ValidateConsigneeMobile()
		{
			ConsigneeMobileInfo.ClearAllNotifications();
			if (ConsigneeMobile.IsEmpty)
			{
				ConsigneeMobileInfo.AddMessageError(ACASUpdateEmailMobileErrorMessage);
			}
		}

		void ValidateConsigneeEmail()
		{
			ConsigneeEmailInfo.ClearAllNotifications();
			if (ConsigneeEmail.IsEmpty)
			{
				ConsigneeEmailInfo.AddMessageError(ACASUpdateEmailMobileErrorMessage);
			}
		}

		void ValidateShipperName()
		{
			ShipperNameInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ShipperNameInfo);
		}

		void ValidateShipperAddress1()
		{
			ShipperAddress1Info.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ShipperAddress1Info);
		}

		void ValidateShipperCity()
		{
			ShipperCityInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ShipperCityInfo);
		}

		void ValidateShipperState()
		{
			ShipperStateInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ShipperStateInfo);
		}

		void ValidateShipperPostcode()
		{
			ShipperPostcodeInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ShipperPostcodeInfo);
		}

		void ValidateShipperCountryCode()
		{
			ShipperCountryCodeInfo.ClearAllNotifications();
			MandatoryValidation.MessageErrorIfNotEntered(ShipperCountryCodeInfo);
		}

		void ValidateShipperMobile()
		{
			ShipperMobileInfo.ClearAllNotifications();
			if (ShipperMobile.IsEmpty)
			{
				ShipperMobileInfo.AddMessageError(ACASUpdateEmailMobileErrorMessage);
			}
		}
		void ValidateShipperEmail()
		{
			ShipperEmailInfo.ClearAllNotifications();
			if (ShipperEmail.IsEmpty)
			{
				ShipperEmailInfo.AddMessageError(ACASUpdateEmailMobileErrorMessage);
			}
		}

		void ValidateGoodsDescription()
		{
			GoodsDescriptionInfo.ClearAllNotifications();
			var goodsDescription = Consignment.ACASReportGoodsDescription;
			if (string.IsNullOrEmpty(goodsDescription))
			{
				GoodsDescriptionInfo.AddMessageError(Res.GetString("d6c282f7-3a5b-43ed-9c7c-321847c5926b", "Goods Description is required when sending ACAS report."));
			}
			else if (goodsDescription.Length > 490)
			{
				GoodsDescriptionInfo.AddMessageError(Res.GetString("dc368cfa-2bf5-48b6-afc3-21102d1d9f7b", "Goods Description has a limit of 490 characters when sending ACAS report."));
			}
			else if (!goodsDescription.IsEnglishOnlyOrEmpty)
			{
				GoodsDescriptionInfo.AddMessageError(Res.GetString("95b526bd-0767-4b18-a3d6-daf7599171fb", @"This text contains characters not supported by the United States Customs (CBP).
				Only characters shown directly on a keyboard with US layout are acceptable for this message, not typed or special characters."));
			}
		}

		#endregion

		string ACASUpdateEmailMobileErrorMessage => Res.GetString("54f75d5d-94ac-412f-aeb6-a645c1147ca8", "You have not entered a value. It is strongly suggested to send a value as per 21/08/24 CBP ACAS update.");
	}
}
