using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used by later implementation.")]
	sealed class HouseAirwayBillBuilder
	{
		public HouseAirwayBillBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			awbHeader = shipment.AWBHeader;
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;
		readonly ExportAWBHeader awbHeader;

		public HouseAirwayBill Build()
		{
			var hawb = new HouseAirwayBill(nameof(ForwardingShipment), shipment.JS_UniqueConsignRef);

			if (awbHeader == null)
			{
				return hawb;
			}

			PopulateMesssageHeaderDocument(hawb);
			PopulateBusinessHeaderDocument(hawb);
			PopulateMasterConsignment(hawb);
			PopulateIncludedHouseConsignment(hawb);
			PopulateShipper(hawb);
			PopulateConsignee(hawb);
			PopulateExportAgent(hawb);
			PopulateSendingParty(hawb);
			PopulateRateLines(hawb);
			PopulateSpecialHandlingAndValidations(hawb);

			AddValidationsMesssageHeaderDocument(hawb);
			AddValidationsExportAgentCode(hawb);
			AddValidationsBusinessHeaderDocument(hawb);
			AddValidationsMasterConsignment(hawb);
			AddValidationsIncludedHouseConsignment(hawb);
			AddValidationsRateLines(hawb);

			hawb.ValidateAllIncludingChildren();

			return hawb;
		}

		void PopulateMesssageHeaderDocument(HouseAirwayBill hawb)
		{
			hawb.SendingPartyCode = GetSendingPartyCodeFromBrancheOrCompany(OrgCusCode.CodeTypes.PortSystemNumber, Constants.CountryCodes.Mexico);
		}

		void AddValidationsMesssageHeaderDocument(HouseAirwayBill hawb)
		{
			hawb.SendingPartyCode?.ValueInfo.AddMessageError(()
				=> hawb.SendingPartyCode.Value.IsEmpty,
				Res.GetString("4348E67A-8899-4187-8022-7D87A81CDE04", "Sender's ID is mandatory. Please maintain it in branch or company organization. proxy Organization > Config > Registration Numbers/Codes - type PSN."));

			hawb.ErrorPlaceHolderInfo.AddMessageError(()
				=> hawb.SendingPartyCode.Value.IsEmpty,
				Res.GetString("BA669B34-3321-499F-9F45-65CB9AE6970D", "Sender's ID is mandatory. Please maintain it in branch or company organization. proxy Organization > Config > Registration Numbers/Codes - type PSN."));

			hawb.SendingPartyCode?.ValueInfo.AddMessageError(()
				=> !hawb.SendingPartyCode.Value.IsEmpty && hawb.SendingPartyCode.Value.Length > 4,
				Res.GetString("669A0815-94E8-47CB-91C5-14F83E87C8BE", "Incorrect format - Sender's ID must be maximum 4 characters long."));

			hawb.ErrorPlaceHolderInfo.AddMessageError(()
				=> !hawb.SendingPartyCode.Value.IsEmpty && hawb.SendingPartyCode.Value.Length > 4,
				Res.GetString("C6288271-6B65-4B01-8F79-5DC8E0E754AE", "Incorrect format - Sender's ID must be maximum 4 characters long."));
		}

		void PopulateSendingParty(HouseAirwayBill hawb)
		{
			var sendingParty = AddressBuilder.Create(context, GlbCompany.CurrentCompany.OrgProxy?.MainAddress);

			sendingParty.AddPartyNameAndAddressValidation(Res.GetString("A23DA0A9-47EB-4303-8959-6487303627B4", "Sending Party"));
			sendingParty.AddressFormattedInfo.AddMessageErrorIfEmpty(Res.GetString("6A60091E-A8DB-4854-98E8-387C330AB65C", "Sending Party Address is required for VUCEM messaging."));

			hawb.SendingParty = sendingParty;
		}

		void PopulateBusinessHeaderDocument(HouseAirwayBill hawb)
		{
			hawb.AWBNumber = shipment.JS_HouseBill;
			hawb.ShippersSignature = awbHeader.EH_ShippersSignature.Substring(0, 20);
			hawb.IssueDate = awbHeader.EH_AWBIssueDate;
			hawb.AgentsSignature = awbHeader.EH_AWBAgentsSignature.Substring(0, 20);
			hawb.IssuePlace = awbHeader.EH_AWBIssuePlace;
		}

		void AddValidationsBusinessHeaderDocument(HouseAirwayBill hawb)
		{
			hawb.AWBNumberInfo.AddMessageErrorIfEmpty(
				Res.GetString("99c3a0b4-e8b7-43cf-a819-8ebc78c92f7a", "AWB Number is a mandatory field."));
			hawb.AWBNumberInfo.AddMessageError(()
				=> !hawb.AWBNumber.IsEmpty && hawb.AWBNumber.Length > 25,
				Res.GetString("f06bc013-ea8a-4fb2-ba10-c8f431b82d11", "Incorrect format - AWB Number must be maximum 25 characters long."));

			hawb.ShippersSignatureInfo.AddMessageErrorIfEmpty(
				Res.GetString("b9b0daca-7216-4ea7-99a4-c57469af891c", "Shippers Signature is a mandatory field."));
			hawb.ShippersSignatureInfo.AddWarning(()
				=> !hawb.ShippersSignature.IsEmpty && awbHeader.EH_ShippersSignature.Length > 20,
				Res.GetString("3bca73f1-159a-4a7e-86e6-676611cd60d5", "Incorrect format - Shippers Signature must be maximum 20 characters long."));

			hawb.IssueDateInfo.AddMessageErrorIfEmpty(
				Res.GetString("2833a307-3799-4057-b26c-9f13c39a76be", "Issue Date is a mandatory field."));

			hawb.AgentsSignatureInfo.AddMessageErrorIfEmpty(
				Res.GetString("9e206258-7e93-4ab4-bc97-9663d1161e22", "Agents Signature is a mandatory field."));
			hawb.AgentsSignatureInfo.AddWarning(()
				=> !hawb.AgentsSignature.IsEmpty && awbHeader.EH_AWBAgentsSignature.Length > 20,
				Res.GetString("6bad2733-24c4-409f-92e6-283a0310cd72", "Incorrect format - Agents Signature must be maximum 20 characters long."));

			hawb.IssuePlaceInfo.AddMessageErrorIfEmpty(
				Res.GetString("cecf964f-07f8-4f77-be34-cdda4eee5353", "Issue Place is a mandatory field."));
			hawb.IssuePlaceInfo.AddMessageError(()
				=> !hawb.IssuePlace.IsEmpty && hawb.IssuePlace.Length > 17,
				Res.GetString("1eb842f9-5f9c-4db8-8df4-ff77c209ef43", "Incorrect format - Issue Place must be maximum 17 characters long."));
		}

		void PopulateMasterConsignment(HouseAirwayBill hawb)
		{
			hawb.AirlinePrefix = awbHeader.EH_AirlinePrefix;
			hawb.SerialNo = awbHeader.EH_AWBSerialNo;

			if (shipment.Origin == null)
			{
				hawb.AirportOfDeparture = new Unloco(context.Factory, context.Unlocos, context.Countries);
			}
			else
			{
				hawb.AirportOfDeparture = Unloco.Create(context, shipment.Origin);
			}
			if (shipment.Destination == null)
			{
				hawb.AirportOfDestination = new Unloco(context.Factory, context.Unlocos, context.Countries);
			}
			else
			{
				hawb.AirportOfDestination = Unloco.Create(context, shipment.Destination);
			}
		}

		void AddValidationsMasterConsignment(HouseAirwayBill hawb)
		{
			hawb.AirlinePrefixInfo.AddMessageErrorIfEmpty(
				Res.GetString("29677203-1695-4792-aa3b-93acbef90edc", "Airline Prefix is a mandatory field."));
			hawb.SerialNoInfo.AddMessageErrorIfEmpty(
				Res.GetString("230f4b6e-4506-4757-a15a-03a769a1c8a7", "Serial No is a mandatory field."));

			((Unloco)hawb.AirportOfDeparture)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("443c841d-c4ad-4178-bb54-19af10315641", "Airport of Departure is a mandatory field."));
			((Unloco)hawb.AirportOfDeparture)?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("443c841d-c4ad-4178-bb54-19af10315641", "Airport of Departure is a mandatory field."));
			((Unloco)hawb.AirportOfDestination)?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("80a3cef3-4485-4fc8-8d36-f2544d0fb4ef", "Airport of Destination is a mandatory field."));
			((Unloco)hawb.AirportOfDestination)?.NameInfo.AddMessageErrorIfEmpty(Res.GetString("80a3cef3-4485-4fc8-8d36-f2544d0fb4ef", "Airport of Destination is a mandatory field."));
		}

		void PopulateIncludedHouseConsignment(HouseAirwayBill hawb)
		{
			hawb.CarriageValue = new Money
			{
				Amount = awbHeader.EH_DeclaredValue,
				Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
				{
					Code = awbHeader.EH_HouseDeclaredValueCurrency
				}
			};
			hawb.CustomsValue = new Money
			{
				Amount = awbHeader.EH_CustomsValue,
				Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
				{
					Code = awbHeader.EH_HouseCustomsValueCurrency
				}
			};
			hawb.InsuranceValue = new Money
			{
				Amount = awbHeader.EH_InsuranceValue,
				Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
				{
					Code = awbHeader.EH_HouseInsuranceValueCurrency
				}
			};

			var totalGrossWeight = 0m;
			foreach (ExportAWBRateLine awbRateLine in awbHeader.AWBRateLines)
			{
				switch (awbHeader.EH_WeightPrepaidCollect)
				{
					case "P":
						hawb.TotalWeightPPD += awbRateLine.ER_Total;
						break;
					case "C":
						hawb.TotalWeightCOL += awbRateLine.ER_Total;
						break;
					default:
						hawb.TotalWeightPPD += awbRateLine.ER_Total;
						hawb.TotalWeightCOL += awbRateLine.ER_Total;
						break;
				}

				totalGrossWeight += awbRateLine.ER_GrossWeight;
				if (int.TryParse(awbRateLine.ER_NoOfPiecesOrRCP, out var noOfPieces))
				{
					hawb.TotalNoOfPieces += noOfPieces;
				}
			}
			hawb.TotalGrossWeight = new Measurement
			{
				Value = totalGrossWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = awbHeader.AWBRateLine1.ER_WeightInLBsOrKGs
				}
			};

			hawb.Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
			{
				Code = awbHeader.EH_Currency
			};
			hawb.ValuationPPD = awbHeader.EH_ValuationPPD;
			hawb.ValuationCOL = awbHeader.EH_ValuationCOL;
			hawb.TaxesPPD = awbHeader.EH_TaxesPPD;
			hawb.TaxesCOL = awbHeader.EH_TaxesCOL;

			hawb.OtherChargesDueAgentPPD = awbHeader.EH_OtherChargesDueAgentPPD;
			hawb.OtherChargesDueAgentCOL = awbHeader.EH_OtherChargesDueAgentCOL;
			hawb.OtherChargesDueCarrierPPD = awbHeader.EH_OtherChargesDueCarrierPPD;
			hawb.OtherChargesDueCarrierCOL = awbHeader.EH_OtherChargesDueCarrierCOL;

			hawb.TotalPrepaid = hawb.TotalWeightPPD + hawb.ValuationPPD + hawb.TaxesPPD + hawb.OtherChargesDueAgentPPD + hawb.OtherChargesDueCarrierPPD;
			hawb.TotalCollect = hawb.TotalWeightCOL + hawb.ValuationCOL + hawb.TaxesCOL + hawb.OtherChargesDueAgentCOL + hawb.OtherChargesDueCarrierCOL;

			hawb.WeightPrepaidCollect = new CodeDescription(awbHeader.PrepaidCollectList)
			{
				Code = awbHeader.EH_WeightVPPDCOL
			};

			hawb.OtherPrepaidCollect = new CodeDescription(awbHeader.PrepaidCollectList)
			{
				Code = awbHeader.EH_OtherPPDCOL
			};
		}

		void AddValidationsIncludedHouseConsignment(HouseAirwayBill hawb)
		{
			((Measurement)hawb.TotalGrossWeight)?.ValueInfo.AddMessageError(() => hawb.TotalGrossWeight.Value <= 0,
				Res.GetString("2a21444f-660c-47cc-bc9b-a8dbec816647", "Total Gross Weight greater than zero is required for VUCEM messaging."));

			hawb.TotalNoOfPiecesInfo.AddMessageError(() => hawb.TotalNoOfPieces <= 0,
				Res.GetString("324c015a-496b-413d-a5e6-df42cbe7ea73", "Total No Of Pieces greater than zero is required for VUCEM messaging."));
		}

		void PopulateShipper(HouseAirwayBill hawb)
		{
			var shipper = AddressBuilder.Create(context);

			shipper.CompanyName = awbHeader.EH_ShipperName;
			shipper.AddressLine1 = awbHeader.EH_ShipperAddress;
			shipper.AddressLine2 = awbHeader.EH_ShipperAddress2;
			shipper.City = awbHeader.EH_ShipperPlace;
			shipper.State = awbHeader.EH_ShipperState;
			shipper.Postcode = awbHeader.EH_ShipperPostCode;
			shipper.Country = new Country(context.Factory, context.Countries)
			{
				Code = awbHeader.EH_ShipperCountryCode
			};
			shipper.Contact = awbHeader.EH_ShipperContactName;
			shipper.Phone = shipment.ConsignorDocumentaryAddress.E2_Phone;
			shipper.Fax = shipment.ConsignorDocumentaryAddress.E2_Fax;
			shipper.Email = shipment.ConsignorDocumentaryAddress.E2_Email;

			shipper.AddPartyNameAndAddressValidation(Res.GetString("732F1947-A855-4B57-8456-08D3112BEA7B", "Shipper"));
			shipper.AddressFormattedInfo.AddMessageErrorIfEmpty(Res.GetString("B870EBD7-6CFD-4470-9870-807B8D0FC803", "Shipper Address is required for VUCEM messaging."));

			hawb.Shipper = shipper;
		}

		void PopulateConsignee(HouseAirwayBill hawb)
		{
			var consignee = AddressBuilder.Create(context);

			consignee.CompanyName = awbHeader.EH_ConsigneeName;
			consignee.AddressLine1 = awbHeader.EH_ConsigneeAddress;
			consignee.AddressLine2 = awbHeader.EH_ConsigneeAddress2;
			consignee.City = awbHeader.EH_ConsigneePlace;
			consignee.State = awbHeader.EH_ConsigneeState;
			consignee.Postcode = awbHeader.EH_ConsigneePostCode;
			consignee.Country = new Country(context.Factory, context.Countries)
			{
				Code = awbHeader.EH_ConsigneeCountryCode
			};
			consignee.Contact = awbHeader.EH_ConsigneeContactName;
			consignee.Phone = shipment.ConsigneeDocumentaryAddress.E2_Phone;
			consignee.Fax = shipment.ConsigneeDocumentaryAddress.E2_Fax;
			consignee.Email = shipment.ConsigneeDocumentaryAddress.E2_Email;

			consignee.AddPartyNameAndAddressValidation(Res.GetString("0D75F9D3-3AAB-4741-9EF4-F822A563DA79", "Consignee"));
			consignee.AddressFormattedInfo.AddMessageErrorIfEmpty(Res.GetString("1EB336AC-6998-4B28-99B6-B7D3AB89E053", "Consignee Address is required for VUCEM messaging."));

			hawb.Consignee = consignee;
		}

		void PopulateExportAgent(HouseAirwayBill hawb)
		{
			var exportAgent = AddressBuilder.Create(context, shipment.ArrivalConsol?.SendingForwarderAddress);

			exportAgent.AddPartyNameAndAddressValidation(Res.GetString("20E70A17-AE6B-4E38-AC5C-934C98B5A6F5", "Export Agent"));
			exportAgent.AddressFormattedInfo.AddMessageErrorIfEmpty(Res.GetString("B664F580-B51D-4A2E-8B7B-1EE9A65B8F92", "Export Agent Address is required for VUCEM messaging."));

			hawb.ExportAgent = exportAgent;

			var registrationNumber = new RegistrationNumber();
			registrationNumber.Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Constants.CountryCodes.Mexico))
			{
				Code = OrgCusCode.CodeTypes.PortSystemNumber
			};
			registrationNumber.CountryOfIssue = new Country(context.Factory, context.Countries)
			{
				Code = Constants.CountryCodes.Mexico
			};
			if (shipment.ArrivalConsol?.SendingForwarderAddress != null)
			{
				registrationNumber.Value = (ZString)(shipment.ArrivalConsol?.SendingForwarderAddress.GetRegistrationNumberWithFallbackToOrgHeader(OrgCusCode.CodeTypes.PortSystemNumber, Constants.CountryCodes.Mexico));
			}
			hawb.ExportAgentCode = registrationNumber;
		}

		void AddValidationsExportAgentCode(HouseAirwayBill hawb)
		{
			hawb.ExportAgentCode?.ValueInfo.AddMessageError(()
				=> hawb.ExportAgentCode.Value.IsEmpty,
				Res.GetString("F89425FF-F313-47DC-94A0-93944B2D76C9", "Export Agent's ID is mandatory. Please maintain it in Sending Freight Forwarder organization. proxy Organization > Config > Registration Numbers/Codes - type PSN."));

			hawb.ExportAgentCode?.ValueInfo.AddMessageError(()
				=> !hawb.ExportAgentCode.Value.IsEmpty && hawb.ExportAgentCode.Value.Length > 4,
				Res.GetString("964C4013-3F4E-41C8-AF91-C1601E1CFF56", "Incorrect format - Export Agent's ID must be maximum 4 characters long."));
		}

		void PopulateRateLines(HouseAirwayBill hawb)
		{
			var rateLines = new List<HouseAirwayBillRateLine>();

			if (awbHeader.AWBRateLines.Count > 0)
			{
				foreach (ExportAWBRateLine awbRateLine in awbHeader.AWBRateLines)
				{
					var rateLine = new HouseAirwayBillRateLine
					{
						GrossWeight = new Measurement
						{
							Value = awbRateLine.ER_GrossWeight,
							Unit = new CodeDescription(context.WeightUnits)
							{
								Code = awbRateLine.ER_WeightInLBsOrKGs
							}
						},
						NatureAndQtyOfGoods = awbRateLine.NatureAndQtyOfGoods?.Text ?? ZString.Empty,
						RateClass = awbRateLine.ER_RateClass,
						CommodityItemNumber = awbRateLine.ER_CommodityItemNumber,
						ChargeableWeight = new Measurement
						{
							Value = awbRateLine.ER_ChargeableWeight,
							Unit = new CodeDescription(context.WeightUnits)
							{
								Code = awbRateLine.ER_WeightInLBsOrKGs
							}
						},
						RateChargeOrDiscount = awbRateLine.ER_RateChargeOrDiscount,
						Total = awbRateLine.ER_ChargeableWeight * awbRateLine.ER_RateChargeOrDiscount
					};

					if (int.TryParse(awbRateLine.ER_NoOfPiecesOrRCP, out var noOfPieces))
					{
						rateLine.NoOfPieces = noOfPieces;
					}

					rateLines.Add(rateLine);
				}
			}

			hawb.RateLines = rateLines;
		}

		void AddValidationsRateLines(HouseAirwayBill hawb)
		{
			foreach (HouseAirwayBillRateLine rateline in hawb.RateLines)
			{
				rateline.NatureAndQtyOfGoodsInfo.AddMessageError(() => rateline.NatureAndQtyOfGoods.Length > 256,
					Res.GetString("1de58a78-6f05-455e-ac1e-77646f0bd0bc", "Incorrect format - Rateline item description must be maximum 256 characters long."));
			}
		}

		void PopulateSpecialHandlingAndValidations(HouseAirwayBill hawb)
		{
			var specialHandlingList = new List<HouseAirwayBillSpecialHandling>();
			var specialHandlingCodeList = new AWB.Business.AWBSpecialHandlingCodeDescriptionPairList();
			specialHandlingCodeList.SortByDescription();
			var numberOfSpecialHandlingCodesAcceptedInForm = 9;

			for (var i = 0; i < numberOfSpecialHandlingCodesAcceptedInForm; i++)
			{
				var specialHandlingItems = new CodeDescription(specialHandlingCodeList);
				var specialHandling = new HouseAirwayBillSpecialHandling(i)
				{
					CodeAndDescription = specialHandlingItems
				};

				specialHandling.CodeAndDescription.CodeInfo.AddMessageError(
					() => IsDuplicatedItemInSpecialHandlingCodes(specialHandling, specialHandlingList), Res.GetString("d960af22-4d1b-4338-a1e5-2d4c2ce2855f", "Special Handling Codes cannot contain duplicated items."));

				specialHandling.CodeAndDescription.CodeInfo.AddMessageError(
					() => VerifySecurityStatusInSpecialHandlingCodes(specialHandling, specialHandlingList), Res.GetString("2e44a1ed-19b7-4f1c-a09a-58fc53646c3a", "Special Handling Codes can contain one Security Status only."));

				specialHandlingList.Add(specialHandling);
			}

			hawb.SpecialHandling = specialHandlingList;
		}

		RegistrationNumber GetSendingPartyCodeFromBrancheOrCompany(string codeType, ZString country)
		{
			var registrationNumber = new RegistrationNumber();
			registrationNumber.Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Constants.CountryCodes.Mexico))
			{
				Code = codeType
			};
			registrationNumber.CountryOfIssue = new Country(context.Factory, context.Countries)
			{
				Code = Constants.CountryCodes.Mexico
			};

			if (GlbBranch.CurrentBranch.OrgProxy?.MainAddress != null)
			{
				registrationNumber.Value = GlbBranch.CurrentBranch.OrgProxy.MainAddress.GetRegistrationNumberWithFallbackToOrgHeader(codeType, country);
			}

			if (registrationNumber.Value.IsEmpty && GlbCompany.CurrentCompany.OrgProxy?.MainAddress != null)
			{
				registrationNumber.Value = GlbCompany.CurrentCompany.OrgProxy.MainAddress.GetRegistrationNumberWithFallbackToOrgHeader(codeType, country);
			}

			return registrationNumber;
		}

		bool IsDuplicatedItemInSpecialHandlingCodes(HouseAirwayBillSpecialHandling specialHandling, List<HouseAirwayBillSpecialHandling> specialHandlingList)
		{
			return specialHandlingList.Count(item =>
					item.CodeAndDescription.Code != "" &&
					item.CodeAndDescription.Code == specialHandling.CodeAndDescription.Code) > 1;
		}

		bool VerifySecurityStatusInSpecialHandlingCodes(HouseAirwayBillSpecialHandling specialHandling, List<HouseAirwayBillSpecialHandling> specialHandlingList)
		{
			if (specialHandling.CodeAndDescription.Code == AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft ||
				specialHandling.CodeAndDescription.Code == AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly ||
				specialHandling.CodeAndDescription.Code == AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft ||
				specialHandling.CodeAndDescription.Code == AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements)
			{
				return specialHandlingList.Count(itemList =>
						itemList.CodeAndDescription.Code == AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft ||
						itemList.CodeAndDescription.Code == AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly ||
						itemList.CodeAndDescription.Code == AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft ||
						itemList.CodeAndDescription.Code == AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements)
					> 1;
			}

			return false;
		}
	}
}
