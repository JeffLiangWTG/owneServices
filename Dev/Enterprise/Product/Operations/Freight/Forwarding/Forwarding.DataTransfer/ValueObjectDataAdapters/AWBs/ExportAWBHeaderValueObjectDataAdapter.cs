using System;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	/// <summary>
	/// This valueobjectdataAdapter is currently for internal use only. It exports airwaybill of the shipment/ consol
	/// </summary>
	public class ExportAWBHeaderValueObjectDataAdapter : ValueObjectDataAdapter<ExportAWBHeader, Xsd.AWBHeader>
	{
		public ExportAWBHeaderValueObjectDataAdapter(IAWBParent exportAWBParent)
			: base()
		{
			AWBParent = exportAWBParent;
			exportAWBParent.PopulateAWB();
		}

		readonly IAWBParent AWBParent;

		#region Override

		public override XmlSchema Schema
		{
			get { return FreightXmlSchemaDefinitions.Instance.SingleAWBHeaderSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return FreightXmlSchemaDefinitions.Instance.AWBHeadersSchema; }
		}

		public override string RootCollectionElementName
		{
			get { return "AWBHeaders"; }
		}

		public override string RootElementName
		{
			get { return "AWBHeader"; }
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(ExportAWBHeader bizObj, Xsd.AWBHeader value, IValueObjectImportContext context)
		{
			throw new NotSupportedException(Res.GetString("BF6E4553-6B9A-4f92-AF86-729CC8118F5F", "Import functionality is currently not available for AWB "));
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(ExportAWBHeader awbHeader, Xsd.AWBHeader awbValueObject, IValueObjectExportContext context)
		{
			awbValueObject.AWBIdentifier.JobNumber = CodePropertyAttribute.CodeFromBusinessObject(((BusinessObject)AWBParent));
			awbValueObject.AWBIdentifier.Type = AWBIdentifierType;
			awbValueObject.MAWB = AWBParent.MAWB;
			awbValueObject.HAWB = AWBParent.HAWB;

			awbValueObject.ReferenceNumber = awbHeader.EH_ConsolNumber;
			awbValueObject.OriginCode = awbHeader.EH_AWBOriginCode;
			awbValueObject.IssueDate = awbHeader.EH_AWBIssueDate;
			awbValueObject.IssuePlace = awbHeader.EH_AWBIssuePlace;
			awbValueObject.FinalizationDate = awbHeader.EH_FinalizationDate;
			awbValueObject.AirportOfDepartureAndRouting = awbHeader.EH_AirportOfDepartureAndRequestRouteText;
			awbValueObject.AirportOfDestination.Code = awbHeader.EH_AirportOfDestinationCode;
			awbValueObject.AirportOfDestination.Text = awbHeader.EH_AirportOfDestinationText;
			awbValueObject.Currency = awbHeader.EH_Currency;
			awbValueObject.ChargesCode = awbHeader.EH_ChargesCode;
			awbValueObject.WTVALPrepaidOrCollect = awbHeader.EH_WeightPrepaidCollect;
			awbValueObject.OtherPrepaidOrCollect = awbHeader.EH_OtherPrepaidCollect;
			awbValueObject.SCI = awbHeader.EH_SpecialHandlingCode;
			awbValueObject.TotalNoOfPieces = awbHeader.EH_TotalNoOfPieces;
			awbValueObject.TotalNoOfPiecesSpecified = true;
			awbValueObject.TotalGrossWeight = awbHeader.EH_TotalGrossWeight;
			awbValueObject.TotalGrossWeightSpecified = true;

			if (awbHeader.EH_ShippingLoadAndCount != 0)
			{
				awbValueObject.ShippersLoadAndCount = awbHeader.EH_ShippingLoadAndCount;
				awbValueObject.ShippersLoadAndCountSpecified = true;
			}

			awbValueObject.ReplaceRateWithAsAgreedOn1stSet = awbHeader.EH_AsAgreed1st;
			awbValueObject.ReplaceRateWithAsAgreedOn1stSetSpecified = true;
			awbValueObject.ReplaceRateWithAsAgreedOn2ndSet = awbHeader.EH_AsAgreed2nd;
			awbValueObject.ReplaceRateWithAsAgreedOn2ndSetSpecified = true;

			awbValueObject.DeclaredValueForCarriage = Xsd.FinancialValue.FromAmountAndCurrencyCode(awbHeader.EH_DeclaredValue, awbHeader.EH_HouseDeclaredValueCurrency);
			awbValueObject.DeclaredValueForCustoms = Xsd.FinancialValue.FromAmountAndCurrencyCode(awbHeader.EH_CustomsValue, awbHeader.EH_HouseCustomsValueCurrency);
			awbValueObject.InsuranceAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(awbHeader.EH_InsuranceValue, awbHeader.EH_HouseInsuranceValueCurrency);
			awbValueObject.NetRate = awbHeader.EH_NetRateCode;
			awbValueObject.NatureAndQuantityOfGoods = awbHeader.NatureAndQtyOfGoods.Trim();
			awbValueObject.ExtraCarrierInfo = awbHeader.EH_ExtraCarrierInfoLine2;

			ExportFlightInfos(awbHeader, awbValueObject);
			ExportRoutingAndDestination(awbHeader, awbValueObject.RoutingAndDestination);
			ExportOrgAddresses(awbHeader, awbValueObject);
			ExportAccountingInfos(awbHeader.AWBAccountingInformations, awbValueObject.AccountingInfos);
			ExportAWBRateLines(awbHeader.AWBRateLines, awbValueObject.RateLines);
			ExportOtherCharges(awbHeader.AWBOtherCharges, awbValueObject.OtherCharges);
			ExportPrepaidOrCollectCharges(awbHeader, awbValueObject);
		}

		Xsd.AWBHeaderIdentifierType AWBIdentifierType
		{
			get
			{
				Xsd.AWBHeaderIdentifierType identifiertype = new Xsd.AWBHeaderIdentifierType();
				if (AWBParent is ForwardingShipment)
				{
					identifiertype = Xsd.AWBHeaderIdentifierType.Shipment;
				}
				else if (AWBParent is ForwardingConsol)
				{
					identifiertype = Xsd.AWBHeaderIdentifierType.Consol;
				}

				return identifiertype;
			}
		}

		void ExportPrepaidOrCollectCharges(ExportAWBHeader awbHeader, Xsd.AWBHeader awbValueObject)
		{
			awbValueObject.PrepaidCharges.Weight = awbHeader.EH_TotalWeightPPD;
			awbValueObject.PrepaidCharges.Valuation = awbHeader.EH_ValuationPPD;
			awbValueObject.PrepaidCharges.Tax = awbHeader.EH_TaxesPPD;
			awbValueObject.PrepaidCharges.OtherChargeDueAgent = awbHeader.EH_OtherChargesDueAgentPPD;
			awbValueObject.PrepaidCharges.OtherChargeDueCarrier = awbHeader.EH_OtherChargesDueCarrierPPD;
			awbValueObject.PrepaidCharges.TotalChargesAmount = awbHeader.EH_TotalPPD;

			awbValueObject.CollectCharges.Weight = awbHeader.EH_TotalWeightCOL;
			awbValueObject.CollectCharges.Valuation = awbHeader.EH_ValuationCOL;
			awbValueObject.CollectCharges.Tax = awbHeader.EH_TaxesCOL;
			awbValueObject.CollectCharges.OtherChargeDueAgent = awbHeader.EH_OtherChargesDueAgentCOL;
			awbValueObject.CollectCharges.OtherChargeDueCarrier = awbHeader.EH_OtherChargesDueCarrierCOL;
			awbValueObject.CollectCharges.TotalChargesAmount = awbHeader.EH_TotalCOL;

			awbValueObject.PrepaidCharges.WeightSpecified = awbValueObject.PrepaidCharges.Weight != 0;
			awbValueObject.PrepaidCharges.ValuationSpecified = awbValueObject.PrepaidCharges.Valuation != 0;
			awbValueObject.PrepaidCharges.TaxSpecified = awbValueObject.PrepaidCharges.Tax != 0;
			awbValueObject.PrepaidCharges.OtherChargeDueAgentSpecified = awbValueObject.PrepaidCharges.OtherChargeDueAgent != 0;
			awbValueObject.PrepaidCharges.OtherChargeDueCarrierSpecified = awbValueObject.PrepaidCharges.OtherChargeDueCarrier != 0;
			awbValueObject.PrepaidCharges.TotalChargesAmountSpecified = awbValueObject.PrepaidCharges.TotalChargesAmount != 0;

			awbValueObject.CollectCharges.WeightSpecified = awbValueObject.CollectCharges.Weight != 0;
			awbValueObject.CollectCharges.ValuationSpecified = awbValueObject.CollectCharges.Valuation != 0;
			awbValueObject.CollectCharges.TaxSpecified = awbValueObject.CollectCharges.Tax != 0;
			awbValueObject.CollectCharges.OtherChargeDueAgentSpecified = awbValueObject.CollectCharges.OtherChargeDueAgent != 0;
			awbValueObject.CollectCharges.OtherChargeDueCarrierSpecified = awbValueObject.CollectCharges.OtherChargeDueCarrier != 0;
			awbValueObject.CollectCharges.TotalChargesAmountSpecified = awbValueObject.CollectCharges.TotalChargesAmount != 0;
		}

		void ExportOtherCharges(ExportAWBOtherChargesCollection exportAWBOtherCharges, Xsd.AWBOtherChargeCollection otherChargesValueObject)
		{
			foreach (ExportAWBOtherCharges current in exportAWBOtherCharges)
			{
				Xsd.AWBOtherCharge otherChargeValueObject = otherChargesValueObject.AddNew();
				otherChargeValueObject.ChargeCode = current.EO_ChargeCode;
				otherChargeValueObject.Description = current.EO_ChargeDescription;
				otherChargeValueObject.Entitlement = current.EO_EntitlementCode;

				if (current.EO_Amount != 0)
				{
					otherChargeValueObject.ChargeAmount = current.EO_Amount;
					otherChargeValueObject.ChargeAmountSpecified = true;
				}
				otherChargeValueObject.PrepaidOrCollect = current.EO_PPDCLT;
			}
		}

		void ExportAWBRateLines(ExportAWBRateLineCollection rateLines, Xsd.AWBRateLineCollection rateLinesValueObject)
		{
			foreach (ExportAWBRateLine current in rateLines)
			{
				Xsd.AWBRateLine rateLineValueObject = new Xsd.AWBRateLine();
				if (current.ER_NoOfPiecesOrRCPAsInt != 0 || current.ER_NoOfPiecesOrRCP.ExcludeChars("0") != ZString.Empty)
				{
					rateLineValueObject.NoOfPiecesOrRCP = current.ER_NoOfPiecesOrRCP;
				}

				if (current.ER_GrossWeight != 0)
				{
					rateLineValueObject.GrossWeight.Value = current.ER_GrossWeight;
					rateLineValueObject.GrossWeight.DimensionType = current.ER_WeightInLBsOrKGs;
				}
				rateLineValueObject.RateClass = current.ER_RateClass;
				rateLineValueObject.CommodityItem = current.ER_CommodityItemNumber;

				if (current.ER_ChargeableWeight != 0)
				{
					rateLineValueObject.CharageableWeight.Value = current.ER_ChargeableWeight;
				}

				if (current.ER_RateChargeOrDiscount != 0)
				{
					rateLineValueObject.RateOrCharge = current.ER_RateChargeOrDiscount.Round(3);
					rateLineValueObject.RateOrChargeSpecified = true;
				}

				if (current.ER_Total != 0)
				{
					rateLineValueObject.Total = current.ER_Total;
					rateLineValueObject.TotalSpecified = true;
				}

				if (current.ER_LineCount != 0)
				{
					rateLineValueObject.LineCount = current.ER_LineCount;
					rateLineValueObject.LineCountSpecified = true;
				}

				if (rateLineValueObject.IsSpecified)
				{
					rateLinesValueObject.Add(rateLineValueObject);
				}
			}
		}

		void ExportRoutingAndDestination(ExportAWBHeader awbHeader, Xsd.AWBHeaderRoutingAndDestination routingValueObject)
		{
			routingValueObject.Route1.CarrierCode = awbHeader.EH_By1st;
			routingValueObject.Route1.Destination = awbHeader.EH_To1st;
			routingValueObject.Route2.CarrierCode = awbHeader.EH_By2nd;
			routingValueObject.Route2.Destination = awbHeader.EH_To2nd;
			routingValueObject.Route3.CarrierCode = awbHeader.EH_By3rd;
			routingValueObject.Route3.Destination = awbHeader.EH_To3rd;
		}

		void ExportFlightInfos(ExportAWBHeader awbHeader, Xsd.AWBHeader awbValueObject)
		{
			MapRequestedFlightInfo(awbValueObject.RequestedFlight.Flight1, awbHeader.EH_Booking1stCarrier, awbHeader.EH_Booking1stFlight, awbHeader.EH_Booking1stFlightDate);
			MapRequestedFlightInfo(awbValueObject.RequestedFlight.Flight2, awbHeader.EH_Booking2ndCarrier, awbHeader.EH_Booking2ndFlight, awbHeader.EH_Booking2ndFlightDate);
		}

		void MapRequestedFlightInfo(Xsd.AWBRequestedFlightInfo requestedFlightValueObject, ZString carrierCode, ZString flightNo, ZString flightDate)
		{
			requestedFlightValueObject.Carrier = carrierCode;
			requestedFlightValueObject.FlightNo = flightNo;
			requestedFlightValueObject.FlightDate = flightDate;
		}

		void ExportAccountingInfos(ExportAWBAccountingInformationCollection accountingInfos, Xsd.AWBAccountingInfoCollection awbAcctInfosValueObject)
		{
			foreach (ExportAWBAccountingInformation current in accountingInfos)
			{
				Xsd.AWBAccountingInfo acctInfoValueObject = awbAcctInfosValueObject.AddNew();
				acctInfoValueObject.Code = current.EA_InformationID;
				acctInfoValueObject.Information = current.EA_Information;
			}
		}

		void ExportOrgAddresses(ExportAWBHeader awbHeader, Xsd.AWBHeader awbValueObject)
		{
			//shipper
			if (awbHeader.EH_IsShipperOverriden)
			{
				MapOrgAddressOverride(awbValueObject.Shipper, awbHeader.EH_ShipperOverride1,
					awbHeader.EH_ShipperOverride2, awbHeader.EH_ShipperOverride3, awbHeader.EH_ShipperOverride4, awbHeader.EH_ShipperOverride5);
			}
			else
			{
				AWBAddressRecordForExport record = new AWBAddressRecordForExport(awbHeader.EH_ShipperAccount, awbHeader.EH_ShipperName,
					awbHeader.EH_ShipperAddress, awbHeader.EH_ShipperAddress2, awbHeader.EH_ShipperCountryCode, awbHeader.EH_ShipperPlace,
				 awbHeader.EH_ShipperState, awbHeader.EH_ShipperPostCode, awbHeader.EH_ShipperContactCode, awbHeader.EH_ShipperContactDetail);

				MapOrgDocAddress(record, awbValueObject.Shipper);
			}

			awbValueObject.Shipper.ExtraInfos.Line1 = awbHeader.EH_ExtraShipperInfoLine1;
			awbValueObject.Shipper.ExtraInfos.Line2 = awbHeader.EH_ExtraShipperInfoLine2;
			awbValueObject.Shipper.AWBSignature = awbHeader.EH_ShippersSignature;

			//consignee 
			if (awbHeader.EH_IsConsigneeOverriden)
			{
				MapOrgAddressOverride(awbValueObject.Consignee, awbHeader.EH_ConsigneeOverride1,
					awbHeader.EH_ConsigneeOverride2, awbHeader.EH_ConsigneeOverride3, awbHeader.EH_ConsigneeOverride4, awbHeader.EH_ConsigneeOverride5);
			}
			else
			{
				AWBAddressRecordForExport record = new AWBAddressRecordForExport(awbHeader.EH_ConsigneeAccount, awbHeader.EH_ConsigneeName,
					awbHeader.EH_ConsigneeAddress, awbHeader.EH_ConsigneeAddress2, awbHeader.EH_ConsigneeCountryCode, awbHeader.EH_ConsigneePlace,
				 awbHeader.EH_ConsigneeState, awbHeader.EH_ConsigneePostCode, awbHeader.EH_ConsigneeContactCode, awbHeader.EH_ConsigneeContactDetail);

				MapOrgDocAddress(record, awbValueObject.Consignee);
			}

			//notify party
			if (awbHeader.EH_IsNotifyOverriden)
			{
				MapOrgAddressOverride(awbValueObject.AlsoNotify, awbHeader.EH_NotifyOverride1,
					awbHeader.EH_NotifyOverride2, awbHeader.EH_NotifyOverride3, awbHeader.EH_NotifyOverride4, awbHeader.EH_NotifyOverride5);
			}
			else
			{
				AWBAddressRecordForExport record = new AWBAddressRecordForExport(ZString.Empty, awbHeader.EH_AlsoNotifyName,
					awbHeader.EH_AlsoNotifyAddress, awbHeader.EH_AlsoNotifyAddress2, awbHeader.EH_AlsoNotifyCountryCode, awbHeader.EH_AlsoNotifyPlace,
				 awbHeader.EH_AlsoNotifyState, awbHeader.EH_AlsoNotifyPostCode, awbHeader.EH_AlsoNotifyContactCode, awbHeader.EH_AlsoNotifyContactDetail);

				MapOrgDocAddress(record, awbValueObject.AlsoNotify);
			}

			//agent
			if (awbHeader.DeparturePortRelatedBranch != null)
			{
				using (awbHeader.DeparturePortRelatedBranch.SetAsTemporaryContext())
				{
					awbValueObject.Agent.AccountNo = awbHeader.EH_AgentAccountNo;
					awbValueObject.Agent.Name = awbHeader.EH_AgentName;
					awbValueObject.Agent.City = awbHeader.EH_AgentPlace;
					awbValueObject.Agent.IATACode = awbHeader.EH_AgentIATACodeFormatted;
				}
			}
			else
			{
				awbValueObject.Agent.AccountNo = awbHeader.EH_AgentAccountNo;
				awbValueObject.Agent.Name = awbHeader.EH_AgentName;
				awbValueObject.Agent.City = awbHeader.EH_AgentPlace;
				awbValueObject.Agent.IATACode = awbHeader.EH_AgentIATACodeFormatted;
			}

			awbValueObject.Agent.AWBSignature = awbHeader.EH_AWBAgentsSignature;

			awbValueObject.IssuedBy.CompanyName = awbHeader.EH_IssuingAgentName;
			awbValueObject.IssuedBy.AddressLine1 = awbHeader.EH_IssuingAgentAddress1;
			awbValueObject.IssuedBy.AddressLine2 = awbHeader.EH_IssuingAgentAddress2;

			awbValueObject.OptionalShippingInfo.Text1 = awbHeader.EH_OptionalShippingInformation;
			awbValueObject.OptionalShippingInfo.Text2 = awbHeader.EH_OptionalShippingInformation2;

			awbValueObject.HandlingInformation = awbHeader.EH_HandlingInformation;
		}

		void MapOrgAddressOverride(Xsd.AWBOrganisationAddress addressValueObject, ZString addrLine1, ZString addrLine2, ZString addrLine3, ZString addrLine4, ZString addrLine5)
		{
			Xsd.AWBOrgAddressOverride addressOverride = new Xsd.AWBOrgAddressOverride();
			addressOverride.AddressLine1 = addrLine1;
			addressOverride.AddressLine2 = addrLine2;
			addressOverride.AddressLine3 = addrLine3;
			addressOverride.AddressLine4 = addrLine4;
			addressOverride.AddressLine5 = addrLine5;

			addressValueObject.Item = addressOverride;
		}

		void MapOrgDocAddress(AWBAddressRecordForExport record, Xsd.AWBOrganisationAddress addressValueObject)
		{
			Xsd.AWBOrgDocAddress docAddress = new Xsd.AWBOrgDocAddress();
			docAddress.AccountCode = record.AccountCode;
			docAddress.CompanyName = record.CompanyName;
			docAddress.AddressLine1 = record.AddrLine1;
			docAddress.AddressLine2 = record.AddrLine2;
			docAddress.CountryCode = record.CountryCode;
			docAddress.CityOrSuburb = record.City;
			docAddress.StateOrProvince = record.State;
			docAddress.PostCode = record.PostCode;
			docAddress.ContactInfo.ContactInfoType = record.ContactType;
			docAddress.ContactInfo.ContactDetail = record.ContactDetail;

			addressValueObject.Item = docAddress;
		}

		protected override void AddExportEvent(Xsd.AWBHeader valueObject, ExportAWBHeader bizObj, IValueObjectExportContext context, ZString reference)
		{
			//don't need to add DEX event
			//This dataAdapter is for internal use only
		}

		#endregion
	}
}

