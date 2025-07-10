using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	sealed class AWBHeaderDataObjectWriter : DataObjectWriter<ExportAWBHeader, AWBHeader>
	{
		public AWBHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override AWBHeader PopulateDataObject(ExportAWBHeader awbHeader)
		{
			var awbHeaderParent = awbHeader.Parent as BusinessObject;
			if (awbHeaderParent != null
				&& awbHeaderParent.IsInDatabase
				&& !awbHeaderParent.HasChanges
				&& !awbHeader.HasChanges)
			{
				var readonlyFactory = awbHeader.Factory.GetCachedReadOnlyFactory();
				var awbHeaderParentInReadonlyFactory = readonlyFactory.Load(awbHeaderParent.GetType(), awbHeaderParent.PK) as IAWBParent;
				var awbHeaderInReadonlyFactory = awbHeaderParentInReadonlyFactory?.AWBHeader;

				if (awbHeaderInReadonlyFactory != null)
				{
					return PopulateDataObjectCore(awbHeaderInReadonlyFactory);
				}
			}

			return PopulateDataObjectCore(awbHeader);
		}

		AWBHeader PopulateDataObjectCore(ExportAWBHeader awbHeader)
		{
			using (awbHeader.SetIsExportingData())
			{
				awbHeader.Parent?.PopulateAWB();
			}

			var awbHeaderDataObject = new AWBHeader(writeManager.WriterStrategy);
			var listCache = BindToLists.GetCachedLists(awbHeader.Factory);

			awbHeaderDataObject.AWBType = ListHelper.GetWithDescription<CodeDescriptionPair>(awbHeader.EH_AWBType, awbHeader.Lookups.AWBTypeList);
			awbHeaderDataObject.AWBNumber = awbHeader.EH_AWBSerialNo;
			awbHeaderDataObject.ForwardingAgentReference = awbHeader.EH_ReferenceNumber;
			awbHeaderDataObject.OriginCode = awbHeader.EH_AWBOriginCode;

			SetShipper(awbHeader, awbHeaderDataObject, listCache);
			SetConsignee(awbHeader, awbHeaderDataObject, listCache);
			SetAlsoNotify(awbHeader, awbHeaderDataObject, listCache);
			SetAsAgreed(awbHeader, awbHeaderDataObject);

			awbHeaderDataObject.AgentName = awbHeader.EH_AgentName;
			awbHeaderDataObject.AgentPlace = awbHeader.EH_AgentPlace;
			awbHeaderDataObject.AgentIATACode = awbHeader.EH_AgentIATACodeFormatted;
			awbHeaderDataObject.AgentAccountNo = awbHeader.EH_AgentAccountNo;

			awbHeaderDataObject.IssuedByName = awbHeader.EH_IssuingAgentName;
			awbHeaderDataObject.IssuedByAddress1 = awbHeader.EH_IssuingAgentAddress1;
			awbHeaderDataObject.IssuedByAddress2 = awbHeader.EH_IssuingAgentAddress2;

			awbHeaderDataObject.SetAccountingInfoCollection(() => ProcessCollection(awbHeader.AWBAccountingInformations, new AWBAccountingInfoDataObjectWriter(writeManager)));

			if (awbHeader.AWBSpecialHandlingItems.Count > 0)
			{
				awbHeaderDataObject.SetSpecialHandlingCollection(() =>
				{
					var list = new List<CodeDescriptionPair>();

					foreach (ExportAWBSpecialHandling specialHandling in awbHeader.AWBSpecialHandlingItems)
					{
						list.Add(ListHelper.GetWithDescription<CodeDescriptionPair>(specialHandling.EP_SpecialHandling, specialHandling.Lookups.SpecialHandlingCodeDescriptionList));
					}
					return list;
				});
			}

			awbHeaderDataObject.AirportOfDepartureAndRequestRouteText = awbHeader.EH_AirportOfDepartureAndRequestRouteText;

			awbHeaderDataObject.Routing1stTo = awbHeader.EH_To1st;
			awbHeaderDataObject.Routing1stBy = awbHeader.EH_By1st;
			awbHeaderDataObject.Routing2ndTo = awbHeader.EH_To2nd;
			awbHeaderDataObject.Routing2ndBy = awbHeader.EH_By2nd;
			awbHeaderDataObject.Routing3rdTo = awbHeader.EH_To3rd;
			awbHeaderDataObject.Routing3rdBy = awbHeader.EH_By3rd;

			awbHeaderDataObject.AirportOfDestinationCode = awbHeader.EH_AirportOfDestinationCode;
			awbHeaderDataObject.AirportOfDestinationText = awbHeader.EH_AirportOfDestinationText;

			awbHeaderDataObject.Requested1stCarrier = awbHeader.EH_Booking1stCarrier;
			awbHeaderDataObject.Requested1stFlight = awbHeader.EH_Booking1stFlight;
			awbHeaderDataObject.Requested1stFlightDate = awbHeader.EH_Booking1stFlightDate;
			awbHeaderDataObject.Requested2ndCarrier = awbHeader.EH_Booking2ndCarrier;
			awbHeaderDataObject.Requested2ndFlight = awbHeader.EH_Booking2ndFlight;
			awbHeaderDataObject.Requested2ndFlightDate = awbHeader.EH_Booking2ndFlightDate;

			awbHeaderDataObject.HandlingInformation = awbHeader.EH_HandlingInformation;
			awbHeaderDataObject.SpecialHandlingCode = awbHeader.EH_SpecialHandlingCode;
			awbHeaderDataObject.ShippersLoadAndCount = awbHeader.EH_ShippingLoadAndCount;

			awbHeaderDataObject.ManifestDescriptionOfGoods = awbHeader.EH_ManifestDescriptionOfGoods;
			awbHeaderDataObject.NetRateCode = awbHeader.EH_NetRateCode;

			awbHeaderDataObject.OptionalShippingInformation1 = awbHeader.EH_OptionalShippingInformation;
			awbHeaderDataObject.OptionalShippingInformation2 = awbHeader.EH_OptionalShippingInformation2;

			awbHeaderDataObject.Currency = ListHelper.GetWithDescription<Currency>(awbHeader.EH_Currency, listCache.RefCurrency_List);

			awbHeaderDataObject.ChargesPayment = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(awbHeader.EH_ChargesCode, awbHeader.ChargeCodesList);
			awbHeaderDataObject.WeightChargesPayment = ListHelper.GetWithDescription<CodeDescriptionPair>(awbHeader.EH_WeightPrepaidCollect, awbHeader.PrepaidCollectList);
			awbHeaderDataObject.OtherChargesPayment = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(awbHeader.EH_OtherPrepaidCollect, awbHeader.PrepaidCollectList);
			awbHeaderDataObject.ValueForCarriage = awbHeader.EH_DeclaredValue;
			awbHeaderDataObject.ValueForCustoms = awbHeader.EH_CustomsValue;
			awbHeaderDataObject.AmountOfInsurance = awbHeader.EH_InsuranceValue;

			awbHeaderDataObject.SetRateLineCollection(() => ProcessCollection(awbHeader.AWBRateLines.Cast<ExportAWBRateLine>().Where(r => !r.IsEmpty), new AWBRateLineDataObjectWriter(writeManager)));
			awbHeaderDataObject.SetOtherChargesCollection(() => ProcessCollection(awbHeader.AWBOtherCharges, new AWBOtherChargesDataObjectWriter(writeManager)));

			awbHeaderDataObject.TotalValuationPrepaid = awbHeader.EH_ValuationPPD;
			awbHeaderDataObject.TotalValuationCollect = awbHeader.EH_ValuationCOL;
			awbHeaderDataObject.TotalTaxesPrepaid = awbHeader.EH_TaxesPPD;
			awbHeaderDataObject.TotalTaxesCollect = awbHeader.EH_TaxesCOL;

			awbHeaderDataObject.ShipperExtraInfoLine1 = awbHeader.EH_ExtraShipperInfoLine1;
			awbHeaderDataObject.ShipperExtraInfoLine2 = awbHeader.EH_ExtraShipperInfoLine2;
			awbHeaderDataObject.ShippersSignature = awbHeader.EH_ShippersSignature;

			awbHeaderDataObject.AWBIssuerExtraInfo = awbHeader.EH_ExtraCarrierInfoLine2;
			awbHeaderDataObject.AWBIssueDate = awbHeader.EH_AWBIssueDate;
			awbHeaderDataObject.AWBIssuePlace = awbHeader.EH_AWBIssuePlace;
			awbHeaderDataObject.AWBIssuerSignature = awbHeader.EH_AWBAgentsSignature;
			awbHeaderDataObject.AWBIssuerApprovedExporterNumber = awbHeader.EH_AgentApprovedExporterNumber;

			awbHeaderDataObject.CargoSecurityDeclaration = GetCargoSecurityDeclaration(awbHeader, listCache);

			return awbHeaderDataObject;
		}

		CargoSecurityDeclaration GetCargoSecurityDeclaration(ExportAWBHeader awbHeader, BindToLists listCache)
		{
			var securityDeclaration = new CargoSecurityDeclaration(writeManager.WriterStrategy)
			{
				AgentApprovalCategory = new CodeDescriptionPair
				{
					Code = awbHeader.EH_AgentApprovalCategory,
					Description = awbHeader.Lookups.AgentApprovalCategoryList.GetDescriptionFromCode(awbHeader.EH_AgentApprovalCategory)
				},
				AgentApprovalNumber = awbHeader.EH_AgentApprovalNumber,
				AgentApprovalExpiryDate = awbHeader.EH_AgentApprovalExpiryDate,
				AgentApprovalCountry = ListHelper.GetWithName<Country>(awbHeader.EH_RN_NKAgentApprovalCountryCode, listCache.RefCountry_List),

				SecurityStatus = GetSecurityStatus(awbHeader),
				SecurityStatusIssueDate = awbHeader.EH_SecurityStatusIssueDate,
				SecurityStatusIssuedBy = awbHeader.EH_SecurityStatusIssuedBy,

				AdditionalScreeningMethods = awbHeader.EH_AdditionalScreeningMethods,
				AdditionalSecurityInformation = awbHeader.EH_AdditionalSecurityInformation,
				AdditionalSecurityInformationStatement = awbHeader.EH_AdditionalSecurityInformationStatement,

				TSASecurityStatement = awbHeader.TSASecurityStatement
			};

			securityDeclaration.SetScreeningMethodCollection(() => GetScreeningMethods(awbHeader));
			securityDeclaration.SetReceivedFromShipperCollection(() => GetReceivedFrom(awbHeader));
			securityDeclaration.SetGroundsForExemptionCollection(() => GetExemptionCodes(awbHeader));

			return securityDeclaration;
		}

		CodeDescriptionPair GetSecurityStatus(ExportAWBHeader awbHeader)
		{
			var description = string.Empty;

			if (awbHeader.EH_SecurityStatus.IsEmpty)
			{
				return null;
			}

			if (awbHeader.EH_SecurityStatus ==
				AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft)
			{
				description =
					AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			}
			else
			{
				var handlingItem = awbHeader
					.AWBSpecialHandlingItems
					.Cast<ExportAWBSpecialHandling>()
					.FirstOrDefault(handling => handling.IsSecurityStatus);

				description = handlingItem != null
					? handlingItem.Lookups.SpecialHandlingCodeDescriptionList.GetDescriptionFromCode(handlingItem.EP_SpecialHandling)
					: string.Empty;
			}

			return new CodeDescriptionPair
			{
				Code = awbHeader.EH_SecurityStatus,
				Description = description
			};
		}

		List<ReceivedFromShipper> GetReceivedFrom(ExportAWBHeader awbHeader)
		{
			var securitySchemeMembershipList = new AviationSecuritySchemeMembership();

			var receivedFrom = awbHeader
				.CargoSecurityKnownShippers
				.Cast<ExportAWBSecurityStatusLine>()
				.Select(line => new ReceivedFromShipper
				{
					Code = line.EAS_ApprovalCategory,
					Description = securitySchemeMembershipList.GetDescriptionFromCode(line.EAS_ApprovalCategory),
					ExpiryDate = line.EAS_ApprovalExpiryDate,
					Number = line.EAS_ApprovalNumber
				})
				.ToList();

			return receivedFrom.Any()
				? receivedFrom
				: null;
		}

		List<CodeDescriptionPair> GetScreeningMethods(ExportAWBHeader awbHeader)
		{
			var screeningMethodsList = ShipmentInspectionTypeLists.GetCountrySpecificScreeningMethods(GlbCompany.CurrentCompany.Country.Code);

			var screeningMethods = awbHeader
				.CargoSecurityScreeningMethods
				.Cast<ExportAWBSecurityStatusLine>()
				.Select(line => new CodeDescriptionPair
				{
					Code = line.EAS_ScreeningMethod,
					Description = screeningMethodsList.GetDescriptionFromCode(line.EAS_ScreeningMethod)
				})
				.ToList();

			return screeningMethods.Any()
				? screeningMethods
				: null;
		}

		List<CodeDescriptionPair4Char> GetExemptionCodes(ExportAWBHeader awbHeader)
		{
			var exemptionCodesList = ShipmentInspectionTypeLists.GetCountrySpecificExemptionCodes(GlbCompany.CurrentCompany.Country.Code);

			var exemptionCodes = awbHeader
				.CargoSecurityExemptionGrounds
				.Cast<ExportAWBSecurityStatusLine>()
				.Select(line => new CodeDescriptionPair4Char
				{
					Code = line.EAS_ExemptionGround,
					Description = exemptionCodesList.GetDescriptionFromCode(line.EAS_ExemptionGround)
				})
				.ToList();

			return exemptionCodes.Any()
				? exemptionCodes
				: null;
		}

		void SetShipper(ExportAWBHeader awbHeader, AWBHeader awbHeaderDataObject, BindToLists listCache)
		{
			var awbPartyDataObject = new AWBParty();

			awbPartyDataObject.AccountCode = awbHeader.EH_ShipperAccount;
			awbPartyDataObject.Name = awbHeader.EH_ShipperName;
			awbPartyDataObject.AddressLine1 = awbHeader.EH_ShipperAddress;
			awbPartyDataObject.AddressLine2 = awbHeader.EH_ShipperAddress2;
			awbPartyDataObject.City = awbHeader.EH_ShipperPlace;
			awbPartyDataObject.State = awbHeader.EH_ShipperState;
			awbPartyDataObject.PostCode = awbHeader.EH_ShipperPostCode;
			awbPartyDataObject.Country = ListHelper.GetWithName<Country>(awbHeader.EH_ShipperCountryCode, listCache.RefCountry_List);
			awbPartyDataObject.ContactType = ListHelper.GetWithDescription<CodeDescriptionPair>(awbHeader.EH_ShipperContactCode, awbHeader.ContactCodesList);
			awbPartyDataObject.ContactDetail = awbHeader.EH_ShipperContactDetail;
			awbPartyDataObject.ContactName = awbHeader.EH_ShipperContactName;
			awbPartyDataObject.CompanyIDCode = awbHeader.EH_ShipperTraderNoType;
			awbPartyDataObject.CompanyID = awbHeader.EH_ShipperTraderNo;

			awbPartyDataObject.IsAddressOverriddenForPaperWaybill = awbHeader.EH_IsShipperOverriden;
			awbPartyDataObject.PaperOverrideLine1 = awbHeader.EH_ShipperOverride1;
			awbPartyDataObject.PaperOverrideLine2 = awbHeader.EH_ShipperOverride2;
			awbPartyDataObject.PaperOverrideLine3 = awbHeader.EH_ShipperOverride3;
			awbPartyDataObject.PaperOverrideLine4 = awbHeader.EH_ShipperOverride4;
			awbPartyDataObject.PaperOverrideLine5 = awbHeader.EH_ShipperOverride5;

			awbHeaderDataObject.Shipper = awbPartyDataObject;
		}

		void SetConsignee(ExportAWBHeader awbHeader, AWBHeader awbHeaderDataObject, BindToLists listCache)
		{
			var awbPartyDataObject = new AWBParty();

			awbPartyDataObject.AccountCode = awbHeader.EH_ConsigneeAccount;
			awbPartyDataObject.Name = awbHeader.EH_ConsigneeName;
			awbPartyDataObject.AddressLine1 = awbHeader.EH_ConsigneeAddress;
			awbPartyDataObject.AddressLine2 = awbHeader.EH_ConsigneeAddress2;
			awbPartyDataObject.City = awbHeader.EH_ConsigneePlace;
			awbPartyDataObject.State = awbHeader.EH_ConsigneeState;
			awbPartyDataObject.PostCode = awbHeader.EH_ConsigneePostCode;
			awbPartyDataObject.Country = ListHelper.GetWithName<Country>(awbHeader.EH_ConsigneeCountryCode, listCache.RefCountry_List);
			awbPartyDataObject.ContactType = ListHelper.GetWithDescription<CodeDescriptionPair>(awbHeader.EH_ConsigneeContactCode, awbHeader.ContactCodesList);
			awbPartyDataObject.ContactDetail = awbHeader.EH_ConsigneeContactDetail;
			awbPartyDataObject.ContactName = awbHeader.EH_ConsigneeContactName;
			awbPartyDataObject.CompanyIDCode = awbHeader.EH_ConsigneeTraderNoType;
			awbPartyDataObject.CompanyID = awbHeader.EH_ConsigneeTraderNo;

			awbPartyDataObject.IsAddressOverriddenForPaperWaybill = awbHeader.EH_IsConsigneeOverriden;
			awbPartyDataObject.PaperOverrideLine1 = awbHeader.EH_ConsigneeOverride1;
			awbPartyDataObject.PaperOverrideLine2 = awbHeader.EH_ConsigneeOverride2;
			awbPartyDataObject.PaperOverrideLine3 = awbHeader.EH_ConsigneeOverride3;
			awbPartyDataObject.PaperOverrideLine4 = awbHeader.EH_ConsigneeOverride4;
			awbPartyDataObject.PaperOverrideLine5 = awbHeader.EH_ConsigneeOverride5;

			awbHeaderDataObject.Consignee = awbPartyDataObject;
		}

		void SetAlsoNotify(ExportAWBHeader awbHeader, AWBHeader awbHeaderDataObject, BindToLists listCache)
		{
			var awbPartyDataObject = new AWBParty();

			awbPartyDataObject.Name = awbHeader.EH_AlsoNotifyName;
			awbPartyDataObject.AddressLine1 = awbHeader.EH_AlsoNotifyAddress;
			awbPartyDataObject.AddressLine2 = awbHeader.EH_AlsoNotifyAddress2;
			awbPartyDataObject.City = awbHeader.EH_AlsoNotifyPlace;
			awbPartyDataObject.State = awbHeader.EH_AlsoNotifyState;
			awbPartyDataObject.PostCode = awbHeader.EH_AlsoNotifyPostCode;
			awbPartyDataObject.Country = ListHelper.GetWithName<Country>(awbHeader.EH_AlsoNotifyCountryCode, listCache.RefCountry_List);
			awbPartyDataObject.ContactType = ListHelper.GetWithDescription<CodeDescriptionPair>(awbHeader.EH_AlsoNotifyContactCode, awbHeader.ContactCodesList);
			awbPartyDataObject.ContactDetail = awbHeader.EH_AlsoNotifyContactDetail;
			awbPartyDataObject.ContactName = awbHeader.EH_AlsoNotifyContactName;
			awbPartyDataObject.CompanyIDCode = awbHeader.EH_AlsoNotifyTraderNoType;
			awbPartyDataObject.CompanyID = awbHeader.EH_AlsoNotifyTraderNo;

			awbPartyDataObject.IsAddressOverriddenForPaperWaybill = awbHeader.EH_IsNotifyOverriden;
			awbPartyDataObject.PaperOverrideLine1 = awbHeader.EH_NotifyOverride1;
			awbPartyDataObject.PaperOverrideLine2 = awbHeader.EH_NotifyOverride2;
			awbPartyDataObject.PaperOverrideLine3 = awbHeader.EH_NotifyOverride3;
			awbPartyDataObject.PaperOverrideLine4 = awbHeader.EH_NotifyOverride4;
			awbPartyDataObject.PaperOverrideLine5 = awbHeader.EH_NotifyOverride5;

			awbHeaderDataObject.AlsoNotify = awbPartyDataObject;
		}

		static void SetAsAgreed(ExportAWBHeader awbHeader, AWBHeader awbHeaderDataObject)
		{
			awbHeaderDataObject.AsAgreedOn1stAWBSet = awbHeader.EH_AsAgreed1st == Core.Constants.AWB.AsAgreedTypes.Codes.All;

			awbHeaderDataObject.AsAgreedOn2ndAWBSet = awbHeader.EH_AsAgreed2nd == Core.Constants.AWB.AsAgreedTypes.Codes.All;

			awbHeaderDataObject.AsAgreedTypeOn1stAWBSet = awbHeader.EH_AsAgreed1st;
			awbHeaderDataObject.AsAgreedTypeOn2ndAWBSet = awbHeader.EH_AsAgreed2nd;
		}
	}
}
