//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVConsignmentValidation
//
//    This class should be used for overriding validation in AutoHVLVConsignmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eTail.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.ZArchitecture.Schema;
	using MasterFiles.Business;

	public class HVLVConsignmentValidation : AutoHVLVConsignmentValidation
	{
		public HVLVConsignmentValidation(AutoHVLVConsignment parent)
			: base(parent)
		{ }

		readonly AddressValidation AddressValidationHelper = new AddressValidation();

		protected new HVLVConsignment Parent
		{
			get { return (HVLVConsignment)base.Parent; }
		}

		public override void ValidateAll()
		{
			if (Parent.HasChanges)
			{
				base.ValidateAll();
			}
		}

		protected override void CheckHVC_ConsignmentId()
		{
			if (Parent.IsInDatabase)
			{
				CheckEnteredForCountryAndDirection(Parent.HVC_ConsignmentIdInfo);
			}

			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ConsignmentIdInfo);
		}

		protected override void CheckHVC_WaybillNumber()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_WaybillNumberInfo);
			if (validatingForUSCustomsJob)
			{
				HVLVConsignmentUSCustomsValidation.MessageErrorIfWaybillTooLong(Parent.HVC_WaybillNumberInfo);
			}
		}

		protected override void CheckHVC_ConsigneeName()
		{
			CheckIsEnteredConsideringSurplusAtDestinationCase(Parent.HVC_ConsigneeNameInfo);
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ConsigneeNameInfo);
		}

		protected override void CheckHVC_ConsigneeAddress1()
		{
			CheckIsEnteredConsideringSurplusAtDestinationCase(Parent.HVC_ConsigneeAddress1Info);
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ConsigneeAddress1Info);
		}

		protected override void CheckHVC_ConsigneeAddress2()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ConsigneeAddress2Info);
		}

		protected override void CheckHVC_ConsigneeCity()
		{
			CheckIsEnteredConsideringSurplusAtDestinationCase(Parent.HVC_ConsigneeCityInfo, RawDataRegistry.Instance.JobAddressValidation_CityMandatory.Value);
			AddressValidationHelper.CheckPostcodeViaCity(Parent.HVC_ConsigneeCityInfo, Parent.HVC_ConsigneePostcodeInfo, Parent.ConsigneeCountryCode);
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ConsigneeCityInfo);
		}

		protected override void CheckHVC_ConsigneeState()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_UseStateRules.Value)
			{
				AddressValidationHelper.CheckState(Parent.HVC_ConsigneeStateInfo, Parent.ConsigneeCountryCode, Parent.ValidationSection);
			}
		}

		protected override void CheckHVC_ConsigneePostcode()
		{
			CheckIsEnteredConsideringSurplusAtDestinationCase(Parent.HVC_ConsigneePostcodeInfo);
		}

		protected override void CheckHVC_RN_NKConsigneeCountryCode()
		{
			if (!Parent.HVC_RN_NKConsigneeCountryCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.HVC_RN_NKConsigneeCountryCodeInfo);
			}

			CheckIsEnteredConsideringSurplusAtDestinationCase(Parent.HVC_RN_NKConsigneeCountryCodeInfo, RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.Value);
		}

		protected override void CheckHVC_ConsigneeEmail()
		{
			AddressValidationHelper.CheckEmail(Parent.HVC_ConsigneeEmailInfo);
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ConsigneeEmailInfo);
		}

		protected override void CheckHVC_ConsigneeInstructions()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ConsigneeInstructionsInfo);
		}

		protected override void CheckHVC_ConsigneeContact()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ConsigneeContactInfo);
		}

		void CheckIsEnteredConsideringSurplusAtDestinationCase(ZPropertyInfo propertyInfo, bool additionalCondition = true)
		{
			if (!Parent.IsSurplusAtDestination && additionalCondition)
			{
				CheckEnteredForCountryAndDirection(propertyInfo);
			}
			else
			{
				MessageErrorIfNotEnteredForCountryAndDirection(propertyInfo);
			}
		}

		protected override void CheckHVC_ShipperName()
		{
			if (Parent.ShipperPropertyShouldPerformMandatoryValidation(Parent.HVC_ShipperNameInfo))
			{
				CheckEnteredForCountryAndDirection(Parent.HVC_ShipperNameInfo);
			}

			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ShipperNameInfo);
		}

		protected override void CheckHVC_ShipperAddress1()
		{
			if (Parent.ShipperPropertyShouldPerformMandatoryValidation(Parent.HVC_ShipperAddress1Info))
			{
				CheckEnteredForCountryAndDirection(Parent.HVC_ShipperAddress1Info);
			}

			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ShipperAddress1Info);
		}

		protected override void CheckHVC_ShipperAddress2()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ShipperAddress2Info);
		}

		protected override void CheckHVC_ShipperCity()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_CityMandatory.Value
				&& Parent.ShipperPropertyShouldPerformMandatoryValidation(Parent.HVC_ShipperCityInfo))
			{
				CheckEnteredForCountryAndDirection(Parent.HVC_ShipperCityInfo);
			}

			AddressValidationHelper.CheckPostcodeViaCity(Parent.HVC_ShipperCityInfo, Parent.HVC_ShipperPostcodeInfo, Parent.ShipperCountryCode);
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ShipperCityInfo);
		}

		protected override void CheckHVC_ShipperState()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_UseStateRules.Value)
			{
				AddressValidationHelper.CheckState(Parent.HVC_ShipperStateInfo, Parent.ShipperCountryCode, Parent.ValidationSection);
			}
		}

		protected override void CheckHVC_ShipperPostcode()
		{
			if (RawDataRegistry.Instance.JobAddressValidation_UsePostcodeRules.Value)
			{
				AddressValidationHelper.CheckPostCode(Parent.HVC_ShipperPostcodeInfo, Parent.ShipperCountryCode, Parent.ValidationSection);
			}

			AddressValidationHelper.CheckCityViaPostcode(Parent.HVC_ShipperPostcodeInfo, Parent.HVC_ShipperCityInfo, Parent.ShipperCountryCode);
		}

		protected override void CheckHVC_RN_NKShipperCountryCode()
		{
			if (!Parent.HVC_RN_NKShipperCountryCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.HVC_RN_NKShipperCountryCodeInfo);
			}

			if (RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.Value
				&& Parent.ShipperPropertyShouldPerformMandatoryValidation(Parent.HVC_RN_NKShipperCountryCodeInfo))
			{
				CheckEnteredForCountryAndDirection(Parent.HVC_RN_NKShipperCountryCodeInfo);
			}
		}

		protected override void CheckHVC_ShipperEmail()
		{
			AddressValidationHelper.CheckEmail(Parent.HVC_ShipperEmailInfo);
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ShipperEmailInfo);
		}

		protected override void CheckHVC_ShipperContact()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ShipperContactInfo);
		}

		protected override void CheckHVC_ShipperReference()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_ShipperReferenceInfo);
		}

		protected override void CheckHVC_RX_NKGoodsValueCurrency()
		{
			ListValidation.ErrorIfInvalidCode(Parent.HVC_RX_NKGoodsValueCurrencyInfo);
			MandatoryValidation.CheckUnitEntered(Parent.HVC_RX_NKGoodsValueCurrencyInfo, Parent.HVC_GoodsValueInfo);
			MandatoryValidation.CheckUnitEntered(Parent.HVC_RX_NKGoodsValueCurrencyInfo, Parent.HVC_TransportValueInfo);
			MandatoryValidation.CheckUnitEntered(Parent.HVC_RX_NKGoodsValueCurrencyInfo, Parent.HVC_InsuranceValueInfo);
		}

		protected override void CheckHVC_GoodsValue()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVC_GoodsValueInfo);

			if (Parent.Items.OfType<HVLVItem>().Any(items => items.Lines.OfType<HVLVItemLine>().Any()))
			{
				var totalCustomsValue = Parent.Items.OfType<HVLVItem>().Sum(items => items.Lines.OfType<HVLVItemLine>().Sum(line => line.HVS_CustomsValue));

				if (totalCustomsValue != Parent.HVC_GoodsValue)
				{
					Parent.HVC_GoodsValueInfo.AddWarning(Res.GetString("9989aaee-db97-4fe7-bda4-192b593c57c6", "Goods Value does not match Item Line Values."));
				}
			}
		}

		protected override void CheckHVC_TransportValue()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVC_TransportValueInfo);
		}

		protected override void CheckHVC_InsuranceValue()
		{
			CompareValidation.CheckNumberNotNegative(Parent.HVC_InsuranceValueInfo);
		}

		protected override void CheckHVC_WeightUQ()
		{
			CheckEnteredForCountryAndDirection(Parent.HVC_WeightUQInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HVC_WeightUQInfo);
		}

		protected override void CheckHVC_VolumeUQ()
		{
			CheckEnteredForCountryAndDirection(Parent.HVC_VolumeUQInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HVC_VolumeUQInfo);
		}

		protected override void CheckHVC_PL_NKLastMileCarrierServiceLevel()
		{
			ListValidation.ErrorIfInvalidCode(Parent.HVC_PL_NKLastMileCarrierServiceLevelInfo, Parent.Lookups.LastMileCarrierServiceLevels);
		}

		protected override void CheckHVC_RS_NKServiceLevel()
		{
			ListValidation.ErrorIfInvalidCode(Parent.HVC_RS_NKServiceLevelInfo, Parent.Lookups.ServiceLevels);
		}

		protected override void CheckHVC_UndgClass()
		{
			ListValidation.ErrorIfInvalidCode(Parent.HVC_UndgClassInfo, Parent.Lookups.HVC_UndgClass_List);
		}

		protected override void CheckHVC_GoodsDescription()
		{
			var originCountryCode = Parent.ManifestedOnShipment?.Origin?.RL_RN_NKCountryCode ?? default;
			if (originCountryCode == Core.Constants.CountryCodes.Australia && Parent.ManifestedOnShipment?.IsExport() == true)
			{
				CheckEnteredForCountryAndDirection(Parent.HVC_GoodsDescriptionInfo);
			}
			else
			{
				WarnIfNotEnteredForCountryAndDirection(Parent.HVC_GoodsDescriptionInfo);
			}

			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_GoodsDescriptionInfo);
		}

		protected override void CheckHVC_ItemCount()
		{
			if (Parent.HVC_IsActive)
			{
				CheckGreaterThanZeroForCountryAndDirection(Parent.HVC_ItemCountInfo, Res.GetString("f5b3a6c0-e638-49b4-9f80-3db17a36fc62", "Consignment needs to have at least one item."));
			}
		}

		protected override void CheckHVC_INCO()
		{
			ListValidation.ErrorIfInvalidCode(Parent.HVC_INCOInfo, Parent.Lookups.INCOTermsList);
		}

		protected override void CheckHVC_PreScreeningStatus()
		{
			CheckEnteredForCountryAndDirection(Parent.HVC_PreScreeningStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HVC_PreScreeningStatusInfo, Parent.Lookups.HVC_PreScreeningStatus_List);

			if (Parent.HVC_PreScreeningStatus == HVLVConsignmentPreScreeningStatusCodes.Codes.Failed)
			{
				if (!Parent.PreScreeningErrorDetails.IsEmpty)
				{
					Parent.HVC_PreScreeningStatusInfo.AddError(Parent.PreScreeningErrorDetails.ToString());
				}

				if (!Parent.PreScreeningWarningDetails.IsEmpty)
				{
					Parent.HVC_PreScreeningStatusInfo.AddWarning(Parent.PreScreeningWarningDetails.ToString());
				}
			}

			if (!Parent.PreScreeningNotifyOnlyWarningDetails.IsEmpty)
			{
				Parent.HVC_PreScreeningStatusInfo.AddWarning(Parent.PreScreeningNotifyOnlyWarningDetails.ToString());
			}
		}

		protected override void CheckHVC_VendorIdentifier()
		{
			DiacriticsValidation.WarnIfContainsAnyDiacritics(Parent.HVC_VendorIdentifierInfo);
		}

		protected override void CheckHVC_IsActive()
		{
			if (!Parent.HVC_IsActive && Parent.HasCustomsStatus)
			{
				Parent.HVC_IsActiveInfo.AddError(Parent.CannotDeactivateConsignmentsHavingCustomsStatusMessage);
			}

			if (!Parent.HVC_IsActive && Parent.HasItemsNotOnManagingShipment)
			{
				Parent.HVC_IsActiveInfo.AddError(Res.GetString("8e40d685-e8f4-4947-99ba-ff8172a0e800", "Cannot deactivate Consignment as it has Items that are not attached to this Shipment."));
			}
		}

		static IReadOnlyCollection<string> mandatoryPropertiesForAUExport => new HashSet<string>
		{
			HVLVConsignmentSchema.Constants.HVC_ItemCount,
			HVLVConsignmentSchema.Constants.HVC_RN_NKConsigneeCountryCode,
			HVLVConsignmentSchema.Constants.HVC_ShipperName,
			HVLVConsignmentSchema.Constants.HVC_GoodsDescription
		};

		void MessageErrorIfNotEnteredForCountryAndDirection(ZPropertyInfo propertyInfo)
		{
			CheckEnteredForCountryAndDirectionCore(propertyInfo, propInfo => MandatoryValidation.MessageErrorIfNotEntered(propertyInfo));
		}

		void CheckEnteredForCountryAndDirection(ZPropertyInfo propertyInfo)
		{
			CheckEnteredForCountryAndDirectionCore(propertyInfo, propInfo => MandatoryValidation.CheckEntered(propertyInfo));
		}

		void WarnIfNotEnteredForCountryAndDirection(ZPropertyInfo propertyInfo)
		{
			CheckEnteredForCountryAndDirectionCore(propertyInfo, propInfo => MandatoryValidation.WarnIfNotEntered(propertyInfo));
		}

		void CheckGreaterThanZeroForCountryAndDirection(ZPropertyInfo propertyInfo, string errorMessage)
		{
			CheckEnteredForCountryAndDirectionCore(propertyInfo, propInfo =>
			{
				if (new ZDecimal(propInfo.Value) <= 0M)
				{
					propInfo.AddError(errorMessage);
				}
			});
		}

		void CheckEnteredForCountryAndDirectionCore(ZPropertyInfo propertyInfo, Action<ZPropertyInfo> mandatoryValidation)
		{
			if (Parent.ManifestedOnShipment is ForwardingShipment manifestedOnShipment
				&& manifestedOnShipment.JS_RL_NKOrigin.Left(2) == Core.Constants.CountryCodes.Australia
				&& manifestedOnShipment.IsExport())
			{
				if (mandatoryPropertiesForAUExport.Contains(propertyInfo.Name))
				{
					mandatoryValidation.Invoke(propertyInfo);
				}
			}
			else
			{
				mandatoryValidation.Invoke(propertyInfo);
			}
		}

		public void ValidateHVC_WaybillNumberForUSCustomsJob()
		{
			try
			{
				validatingForUSCustomsJob = true;
				ValidateHVC_WaybillNumber();
			}
			finally
			{
				validatingForUSCustomsJob = false;
			}
		}

		bool validatingForUSCustomsJob;
	}
}
