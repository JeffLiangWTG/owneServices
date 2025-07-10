using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class AWBHeaderDataObjectReader : DataObjectReader<AWBHeader, ExportAWBHeader>
	{
		public AWBHeaderDataObjectReader(AWBHeader awbHeaderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IAWBParent awbParentBO)
			: base(awbHeaderDataObject, logger, factory)
		{
			this.awbParentBO = awbParentBO;
		}

		readonly IAWBParent awbParentBO;

		protected override ExportAWBHeader GetExistingBusinessObject()
		{
			return awbParentBO.LoadOrCreateAWB();
		}

		protected override void PopulateBusinessObject(ExportAWBHeader awbHeaderBO)
		{
			awbHeaderBO.Parent.IsAWBValuesOverriddenProperty = ZBool.True;

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AWBType, dataObject.AWBType);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AWBOriginCode, dataObject.OriginCode);

			SetShipper(awbHeaderBO, dataObject.Shipper);
			SetConsignee(awbHeaderBO, dataObject.Consignee);
			SetAlsoNotify(awbHeaderBO, dataObject.AlsoNotify);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_IssuingAgentName, dataObject.IssuedByName);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_IssuingAgentAddress1, dataObject.IssuedByAddress1);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_IssuingAgentAddress2, dataObject.IssuedByAddress2);

			if (dataObject.AccountingInfoCollection != null)
			{
				awbHeaderBO.AWBAccountingInformations.RemoveAndDeleteAll();
				foreach (var accountingInfo in dataObject.AccountingInfoCollection)
				{
					awbHeaderBO.AWBAccountingInformations.Add(new AWBAccountingInfoDataObjectReader(accountingInfo, logger, factory).ReadIntoBusinessObject());
				}
			}

			if (dataObject.SpecialHandlingCollection != null)
			{
				awbHeaderBO.AWBSpecialHandlingItems.RemoveAndDeleteAll();
				foreach (var specialHandling in dataObject.SpecialHandlingCollection)
				{
					var handlingBO = awbHeaderBO.AWBSpecialHandlingItems.AddNew();
					handlingBO.EP_SpecialHandling = specialHandling.GetCodeAsUpperCase();
				}
			}

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AirportOfDepartureAndRequestRouteText, dataObject.AirportOfDepartureAndRequestRouteText);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_To1st, dataObject.Routing1stTo);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_By1st, dataObject.Routing1stBy);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_To2nd, dataObject.Routing2ndTo);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_By2nd, dataObject.Routing2ndBy);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_To3rd, dataObject.Routing3rdTo);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_By3rd, dataObject.Routing3rdBy);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AirportOfDestinationCode, dataObject.AirportOfDestinationCode);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AirportOfDestinationText, dataObject.AirportOfDestinationText);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_Booking1stCarrier, dataObject.Requested1stCarrier);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_Booking1stFlight, dataObject.Requested1stFlight);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_Booking1stFlightDate, dataObject.Requested1stFlightDate);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_Booking2ndCarrier, dataObject.Requested2ndCarrier);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_Booking2ndFlight, dataObject.Requested2ndFlight);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_Booking2ndFlightDate, dataObject.Requested2ndFlightDate);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_HandlingInformation, dataObject.HandlingInformation);
			SetAsAgreed(awbHeaderBO);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_SpecialHandlingCode, dataObject.SpecialHandlingCode);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShippingLoadAndCount, dataObject.ShippersLoadAndCount);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_NetRateCode, dataObject.NetRateCode);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_OptionalShippingInformation, dataObject.OptionalShippingInformation1);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_OptionalShippingInformation2, dataObject.OptionalShippingInformation2);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_Currency, dataObject.Currency);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ChargesCode, dataObject.ChargesPayment);

			var weightChargesPaymentCode = dataObject.WeightChargesPayment.GetCodeAsUpperCase();
			if (weightChargesPaymentCode.Length > awbHeaderBO.EH_WeightPrepaidCollectInfo.MaxLength)
			{
				var errorMessage = Res.GetString("81391c8e-23a9-417b-9ba6-5300239bd2f6", "Attempted to import Weight Charges Payment with an invalid value: '{0}'. Maximum of {1} digit is allowed.", weightChargesPaymentCode, awbHeaderBO.EH_WeightPrepaidCollectInfo.MaxLength);
				throw new DataObjectReadFailureException(errorMessage);
			}
			else
			{
				awbHeaderBO.EH_WeightPrepaidCollect = weightChargesPaymentCode;
			}

			awbHeaderBO.EH_OtherPrepaidCollect = dataObject.OtherChargesPayment.GetCodeAsUpperCase();
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_DeclaredValue, dataObject.ValueForCarriage);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_CustomsValue, dataObject.ValueForCustoms);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_InsuranceValue, dataObject.AmountOfInsurance);

			if (dataObject.RateLineCollection != null)
			{
				foreach (AWBRateLine rateLine in dataObject.RateLineCollection)
				{
					new AWBRateLineDataObjectReader(rateLine, logger, factory, awbHeaderBO).ReadIntoBusinessObject();
				}
			}

			if (dataObject.OtherChargesCollection != null)
			{
				awbHeaderBO.AWBOtherCharges.RemoveAndDeleteAll();
				foreach (var charge in dataObject.OtherChargesCollection)
				{
					awbHeaderBO.AWBOtherCharges.Add(new AWBOtherChargesDataObjectReader(charge, logger, factory).ReadIntoBusinessObject());
				}
			}

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ValuationPPD, dataObject.TotalValuationPrepaid);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ValuationCOL, dataObject.TotalValuationCollect);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_TaxesPPD, dataObject.TotalTaxesPrepaid);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_TaxesCOL, dataObject.TotalTaxesCollect);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ExtraShipperInfoLine1, dataObject.ShipperExtraInfoLine1);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ExtraShipperInfoLine2, dataObject.ShipperExtraInfoLine2);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShippersSignature, dataObject.ShippersSignature);

			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ExtraCarrierInfoLine2, dataObject.AWBIssuerExtraInfo);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AWBIssueDate, dataObject.AWBIssueDate);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AWBIssuePlace, dataObject.AWBIssuePlace);
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AWBAgentsSignature, dataObject.AWBIssuerSignature);
		}

		void SetShipper(ExportAWBHeader awbHeaderBO, AWBParty shipper)
		{
			if (shipper != null)
			{
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperAccount, shipper.AccountCode);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperName, shipper.Name);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperAddress, shipper.AddressLine1);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperAddress2, shipper.AddressLine2);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperPlace, shipper.City);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperState, shipper.State);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperPostCode, shipper.PostCode);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperCountryCode, shipper.Country);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperContactCode, shipper.ContactType);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperContactDetail, shipper.ContactDetail);

				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_IsShipperOverriden, shipper.IsAddressOverriddenForPaperWaybill);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperOverride1, shipper.PaperOverrideLine1);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperOverride2, shipper.PaperOverrideLine2);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperOverride3, shipper.PaperOverrideLine3);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperOverride4, shipper.PaperOverrideLine4);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ShipperOverride5, shipper.PaperOverrideLine5);
			}
		}

		void SetConsignee(ExportAWBHeader awbHeaderBO, AWBParty consignee)
		{
			if (consignee != null)
			{
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeAccount, consignee.AccountCode);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeName, consignee.Name);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeAddress, consignee.AddressLine1);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeAddress2, consignee.AddressLine2);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneePlace, consignee.City);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeState, consignee.State);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneePostCode, consignee.PostCode);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeCountryCode, consignee.Country);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeContactCode, consignee.ContactType);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeContactDetail, consignee.ContactDetail);

				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_IsConsigneeOverriden, consignee.IsAddressOverriddenForPaperWaybill);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeOverride1, consignee.PaperOverrideLine1);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeOverride2, consignee.PaperOverrideLine2);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeOverride3, consignee.PaperOverrideLine3);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeOverride4, consignee.PaperOverrideLine4);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_ConsigneeOverride5, consignee.PaperOverrideLine5);
			}
		}

		void SetAlsoNotify(ExportAWBHeader awbHeaderBO, AWBParty alsoNotify)
		{
			if (alsoNotify != null)
			{
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AlsoNotifyName, alsoNotify.Name);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AlsoNotifyAddress, alsoNotify.AddressLine1);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AlsoNotifyAddress2, alsoNotify.AddressLine2);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AlsoNotifyPlace, alsoNotify.City);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AlsoNotifyState, alsoNotify.State);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AlsoNotifyPostCode, alsoNotify.PostCode);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AlsoNotifyCountryCode, alsoNotify.Country);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AlsoNotifyContactCode, alsoNotify.ContactType);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AlsoNotifyContactDetail, alsoNotify.ContactDetail);

				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_IsNotifyOverriden, alsoNotify.IsAddressOverriddenForPaperWaybill);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_NotifyOverride1, alsoNotify.PaperOverrideLine1);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_NotifyOverride2, alsoNotify.PaperOverrideLine2);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_NotifyOverride3, alsoNotify.PaperOverrideLine3);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_NotifyOverride4, alsoNotify.PaperOverrideLine4);
				SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_NotifyOverride5, alsoNotify.PaperOverrideLine5);
			}
		}

		void SetAsAgreed(ExportAWBHeader awbHeaderBO)
		{
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AsAgreed1st, GetAsAgreedType(dataObject.AsAgreedTypeOn1stAWBSet, dataObject.AsAgreedOn1stAWBSet));
			SetValue(awbHeaderBO, ExportAWBHeaderSchema.EH_AsAgreed2nd, GetAsAgreedType(dataObject.AsAgreedTypeOn2ndAWBSet, dataObject.AsAgreedOn2ndAWBSet));
		}

		static ZString? GetAsAgreedType(ZString? asAgreedType, ZBool? asAgreedBool)
		{
			if (asAgreedType.HasValue && !asAgreedType.Value.IsEmpty)
			{
				return asAgreedType;
			}

			if (asAgreedBool.HasValue && asAgreedBool.Value)
			{
				return Core.Constants.AWB.AsAgreedTypes.Codes.All;
			}

			return Core.Constants.AWB.AsAgreedTypes.Codes.None;
		}
	}
}

