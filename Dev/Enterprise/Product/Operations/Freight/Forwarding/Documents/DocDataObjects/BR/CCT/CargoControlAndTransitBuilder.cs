using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using EventConstants = CargoWise.EventReference.Constants;
using ExportAWBHeader = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader;
using ExportAWBRateLine = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	sealed class CargoControlAndTransitBuilder
	{
		public CargoControlAndTransitBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.awbHeader = shipment.AWBHeader;
			context = new CommonContext(shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;
		readonly ExportAWBHeader awbHeader;

		public CargoControlAndTransit Build()
		{
			var cct = new CargoControlAndTransit(
				nameof(ForwardingShipment),
				shipment.JS_UniqueConsignRef);

			if (awbHeader == null)
			{
				return cct;
			}

			awbHeader.PopulateIfNotOverridden();

			cct.Shipper = CreateShipper();
			cct.Consignee = CreateConsignee();
			cct.Issuer = CreateIssuer();
			cct.ImportAgent = CreateImportAgent();
			cct.ExportAgent = CreateExportAgent();

			cct.ReferenceNumber = awbHeader.EH_ConsolNumber;
			cct.OptionalShippingInformation = awbHeader.EH_OptionalShippingInformation;
			cct.OptionalShippingInformation2 = awbHeader.EH_OptionalShippingInformation2;

			PopulateHeader(cct);
			PopulateAirports(cct);
			PopulatePortOfFirstArrival(cct);
			PopulateChargesAndMonies(cct);
			PopulateSpecialHandling(cct);

			PopulateSignature(cct, awbHeader, GlbStaff.CurrentUser);
			PopulatePrepaidAndCollectValues(cct, awbHeader);
			PopulateRateLines(cct, awbHeader.AWBRateLines);
			PopulateCurrency(cct, awbHeader);

			PopulateRUCReferenceNumber(cct);

			cct.ValidateAllIncludingChildren();

			return cct;
		}

		CodeDescriptionPairList IataLocationsList
		{
			get
			{
				if (iataLocationsList == null)
				{
					iataLocationsList = new CodeDescriptionPairList();

					iataLocationsList.AddPair(awbHeader.EH_AWBOriginCode, awbHeader.EH_AirportOfDepartureAndRequestRouteText);
					iataLocationsList.AddPair(awbHeader.EH_AirportOfDestinationCode, awbHeader.EH_AirportOfDestinationText);
					iataLocationsList.AddPair(awbHeader.EH_To1st);
				}

				return iataLocationsList;
			}
		}
		CodeDescriptionPairList iataLocationsList;

		CodeDescriptionPairList TaxList
		{
			get
			{
				if (taxList == null)
				{
					taxList = new CodeDescriptionPairList();
					taxList.AddPair(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, (NoResString)"CNPJ Cadastro Nacional da Pessoa Jurídica"); // programmatic constant
					taxList.AddPair(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, (NoResString)"Individual Tax Payer Registration"); // programmatic constant
					taxList.AddPair(OrgCusCode.CodeTypes.PassportID, (NoResString)"Passport"); // programmatic constant
				}
				return taxList;
			}
		}
		CodeDescriptionPairList taxList;

		Address CreateShipper()
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

			shipper.AddPartyNameAndAddressValidation(Res.GetString("743DBE24-A7AE-4362-9421-5D4F66B19718", "Shipper"));
			shipper.AddressLine1Info.AddMessageError(() => shipper.AddressFormatted.IsEmpty, Res.GetString("1bf180cc-de91-488e-804c-4386c11b2c12", "Shipper Address is required for CCT messaging."));
			shipper.AddressLine1Info.AddWarning(() => (shipper.AddressLine1.Length + shipper.AddressLine2.Length) > 68, Res.GetString("41A42E75-3B6F-4BA0-9672-4F33ECD0945B", "Shipper's Address line 1 & 2 should not exceed 68 characters to comply with Cargo Control and Transit (CCT) system requirements.\r\nOnly the first 68 characters will be sent in the message to CCT."));

			shipper.AddValidationDependencies(shipper.AddressLine1Info, shipper.AddressLine2Info);

			shipper.Contact = shipment.ConsignorDocumentaryAddress.E2_Contact;
			shipper.Phone = shipment.ConsignorDocumentaryAddress.E2_Phone;
			shipper.Fax = shipment.ConsignorDocumentaryAddress.E2_Fax;
			shipper.Email = shipment.ConsignorDocumentaryAddress.E2_Email;

			if (shipment.Consignor != null)
			{
				var cnpj = GetCNPJ(shipment.Consignor);

				if (cnpj != null)
				{
					shipper.TaxNumber = cnpj.OK_CustomsRegNo;
					shipper.TaxNumberType = new CodeDescription(TaxList)
					{
						Code = cnpj.OK_CodeType
					};
				}
			}
			else
			{
				shipper.TaxNumberType = new CodeDescription(TaxList)
				{
					Code = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ
				};
			}

			return shipper;
		}

		Address CreateConsignee()
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

			consignee.AddPartyNameAndAddressValidation(Res.GetString("EF1DE026-FD02-4B3E-8950-23569A6FDD19", "Consignee"));
			consignee.AddressLine1Info.AddMessageError(() => consignee.AddressFormatted.IsEmpty, Res.GetString("AA06AD98-F52B-4352-83AE-029E25F80748", "Consignee Address is required for CCT messaging."));
			consignee.AddressLine1Info.AddWarning(() => (consignee.AddressLine1.Length + consignee.AddressLine2.Length) > 68, Res.GetString("4E9D86B3-82DC-4E76-8439-8904C0FD169F", "Consignee's Address line 1 & 2 should not exceed 68 characters to comply with Cargo Control and Transit (CCT) system requirements.\r\nOnly the first 68 characters will be sent in the message to CCT."));

			consignee.AddValidationDependencies(consignee.AddressLine1Info, consignee.AddressLine2Info);

			consignee.Contact = shipment.ConsigneeDocumentaryAddress.E2_Contact;
			consignee.Phone = shipment.ConsigneeDocumentaryAddress.E2_Phone;
			consignee.Fax = shipment.ConsigneeDocumentaryAddress.E2_Fax;
			consignee.Email = shipment.ConsigneeDocumentaryAddress.E2_Email;

			if (shipment.Consignee != null)
			{
				var cnpj = GetCNPJ(shipment.Consignee);

				if (cnpj != null)
				{
					consignee.TaxNumber = cnpj.OK_CustomsRegNo;
					consignee.TaxNumberType = new CodeDescription(TaxList)
					{
						Code = cnpj.OK_CodeType
					};
				}
			}
			else
			{
				consignee.TaxNumberType = new CodeDescription(TaxList)
				{
					Code = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ
				};
			}

			consignee.TaxNumberInfo.AddMessageErrorIfEmpty(Res.GetString("3CE4F26D-1233-43F0-A4BD-FC2786B2FBA7", "CNPJ is required for CCT messaging."));

			return consignee;
		}

		Address CreateIssuer()
		{
			var issuer = AddressBuilder.CreateForCurrentUser(context);
			issuer.CompanyName = GlbBranch.CurrentBranch.CompanyName;
			issuer.AddressLine1 = GlbBranch.CurrentBranch.Address1;
			issuer.AddressLine2 = GlbBranch.CurrentBranch.Address2;
			issuer.City = GlbBranch.CurrentBranch.City;
			issuer.State = GlbBranch.CurrentBranch.State;
			issuer.Postcode = GlbBranch.CurrentBranch.Postcode;

			return issuer;
		}

		Address CreateImportAgent()
		{
			var consol = shipment
				.Consols
				.OfType<ForwardingConsol>()
				.FirstOrDefault(c =>
					c.JK_TransportMode == Core.Constants.TransportModes.Air
					&& c.JK_RL_NKDischargePort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Brazil);

			Address importAgent = null;

			if (consol?.ReceivingForwarderAddress != null)
			{
				importAgent = AddressBuilder.Create(context, consol.ReceivingForwarderAddress);
				var receivingForwarder = consol.ReceivingForwarderAddress.OA_OH.IsValid
					? shipment.Factory.Load<OrgHeader>(consol.ReceivingForwarderAddress.OA_OH)
					: null;

				var cnpj = receivingForwarder != null
					? GetCNPJ(receivingForwarder)
					: null;

				if (cnpj != null)
				{
					importAgent.TaxNumber = cnpj.OK_CustomsRegNo;
					importAgent.TaxNumberType = new CodeDescription(TaxList)
					{
						Code = cnpj.OK_CodeType
					};
				}
			}
			else
			{
				importAgent = AddressBuilder.Create(context);
				importAgent.TaxNumberType = new CodeDescription(TaxList)
				{
					Code = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ
				};
			}

			importAgent.AddPartyNameAndAddressValidation(Res.GetString("74325B74-C319-4DA6-95A8-842A85BBB167", "Import Agent"));
			importAgent.AddressLine1Info.AddMessageError(() => importAgent.AddressFormatted.IsEmpty, Res.GetString("C66F41CD -4902-4C5C-AE49-9F2AC31595D6", "Import Agent Address is required for CCT messaging."));
			importAgent.AddressLine1Info.AddWarning(() => (importAgent.AddressLine1.Length + importAgent.AddressLine2.Length) > 68, Res.GetString("2931CB76-F5EB-4933-8D97-CC95E1C61EFB", "Import Agent's Address line 1 & 2 should not exceed 68 characters to comply with Cargo Control and Transit (CCT) system requirements.\r\nOnly the first 68 characters will be sent in the message to CCT."));

			importAgent.AddValidationDependencies(importAgent.AddressLine1Info, importAgent.AddressLine2Info);

			importAgent.TaxNumberInfo.AddMessageErrorIfEmpty(Res.GetString("02AAA875-2CB9-42D5-9588-0011642204EC", "CNPJ is required for CCT messaging."));

			return importAgent;
		}

		Address CreateExportAgent()
		{
			var consol = shipment.Consols.OfType<ForwardingConsol>()
				.FirstOrDefault(c => c.JK_TransportMode == Core.Constants.TransportModes.Air
									 && c.JK_RL_NKDischargePort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Brazil);

			var exportAgent = AddressBuilder.Create(context, consol?.SendingForwarderAddress);

			if (consol?.SendingForwarderWithContact?.OrgContact != null)
			{
				exportAgent.Contact = consol.SendingForwarderWithContact.OrgContact.OC_ContactName;
			}

			return exportAgent;
		}

		void PopulateHeader(CargoControlAndTransit cargoControlAndTransit)
		{
			cargoControlAndTransit.AirlinePrefix = awbHeader.EH_AirlinePrefix;
			cargoControlAndTransit.SerialNo = awbHeader.EH_AWBSerialNo;
			cargoControlAndTransit.AWBNumber = awbHeader.EH_BillNumber;

			cargoControlAndTransit.AirlinePrefixInfo.AddMessageErrorIfEmpty(Res.GetString("61FCD69D-624D-462F-B595-9823F1F7E6BE", "Airline Prefix is required for CCT messaging."));
			cargoControlAndTransit.SerialNoInfo.AddMessageErrorIfEmpty(Res.GetString("7E2B92FF-2CB3-408C-BA77-45A9BD6D73F8", "MAWB number is required."));
			cargoControlAndTransit.AWBNumberInfo.AddMessageErrorIfEmpty(Res.GetString("0F481DFF-BA62-4F69-B73B-45D8AC8CA673", "HAWB number is required."));
			cargoControlAndTransit.AWBNumberInfo.AddMessageError(() => cargoControlAndTransit.AWBNumber.Length > 11, Res.GetString("6b41f905-2cc9-4d1f-b469-f6c0eaa0b7dc", "HAWB should be less than or equal to 11 characters"));
		}

		void PopulateAirports(CargoControlAndTransit cct)
		{
			var airportOfDeparture = new CodeDescription(IataLocationsList)
			{
				Code = awbHeader.EH_AWBOriginCode
			};

			airportOfDeparture.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("FC4D3C8B-F209-4256-ABD4-2C87E7B2D911", "Airport Of Departure Code is required for CCT messaging."));
			airportOfDeparture.DescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("8ECBB15B-2464-4C97-9E78-0B7D0D86F0AB", "Airport Of Departure is required for CCT messaging."));

			cct.AirportOfDeparture = airportOfDeparture;

			cct.To1st = new CodeDescription(IataLocationsList)
			{
				Code = awbHeader.EH_To1st
			};

			var airportOfDestination = new CodeDescription(IataLocationsList)
			{
				Code = awbHeader.EH_AirportOfDestinationCode
			};

			airportOfDestination.DescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("4527DB4D-2D03-413E-8B5A-1D0178A5D4FE", "Airport Of Destination is required for CCT messaging."));

			cct.AirportOfDestination = airportOfDestination;
		}

		void PopulatePortOfFirstArrival(CargoControlAndTransit cct)
		{
			var airLegDischargingInBR = shipment.TransportsIncludingRelated
				.OfType<Freight.Business.Transport>()
				.OrderBy(t => t.JW_LegOrder)
				.FirstOrDefault(transport => transport.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.Brazil, StringComparison.OrdinalIgnoreCase) && transport.IsAir);

			if (airLegDischargingInBR == null)
			{
				cct.PortOfFirstArrival = Unloco.Create(context, shipment.Destination);
			}
			else
			{
				cct.PortOfFirstArrival = Unloco.Create(context, airLegDischargingInBR.DiscPort);
			}
		}

		void PopulateSpecialHandling(CargoControlAndTransit cct)
		{
			var specialHandlingList = new List<CargoControlAndTransitSpecialHandling>();

			var specialHandlingCodeList = new AWBSpecialHandlingCodeDescriptionPairList();

			specialHandlingCodeList.SortByDescription();

			var numberOfSpecialHandlingCodesAcceptedInTemplate = 9;

			for (var line = 0; line < numberOfSpecialHandlingCodesAcceptedInTemplate; line++)
			{
				var specialHandlingItems = new CodeDescription(specialHandlingCodeList);

				var specialHandling = new CargoControlAndTransitSpecialHandling(line)
				{
					CodeAndDescription = specialHandlingItems
				};

				specialHandling.CodeAndDescription.CodeInfo.AddMessageError(
					() => IsDuplicatedItemInSpecialHandlingCodes(specialHandling, specialHandlingList), Res.GetString("2E8A6569-B6AF-4D5A-8249-A5FCF30FAE5A", "Special Handling Codes cannot contain duplicated items"));

				specialHandling.CodeAndDescription.CodeInfo.AddMessageError(
					() => VerifySecurityStatusInSpecialHandlingCodes(specialHandling, specialHandlingList), Res.GetString("EF157D00-26C8-4FCD-8AFC-81B2A6BDE73A", "Special Handling Codes can contain one Security Status only"));

				specialHandlingList.Add(specialHandling);
			}

			cct.SpecialHandling = specialHandlingList;
		}

		void PopulateRateLines(CargoControlAndTransit cct, ExportAWBRateLineCollection awbRateLines)
		{
			var rateLines = new List<CargoControlAndTransitRateLine>();

			cct.TotalGrossWeight = new Measurement();

			if (awbRateLines.Count > 0)
			{
				foreach (var awbRateLine in awbRateLines.OfType<ExportAWBRateLine>())
				{
					var rateLine = new CargoControlAndTransitRateLine
					{
						RateClass = awbRateLine.ER_RateClass,
						CommodityItemNumber = awbRateLine.ER_CommodityItemNumber,
						GrossWeight = new Measurement
						{
							Value = awbRateLine.ER_GrossWeight,
							Unit = new CodeDescription(context.WeightUnits)
							{
								Code = awbRateLine.ER_WeightInLBsOrKGs,
							}
						},
						ChargeableWeight = new Measurement
						{
							Value = awbRateLine.ER_ChargeableWeight,
							Unit = new CodeDescription(context.WeightUnits)
							{
								Code = awbRateLine.ER_WeightInLBsOrKGs,
							}
						},
						RateChargeOrDiscount = awbRateLine.ER_RateChargeOrDiscount,
						NatureAndQtyOfGoods = awbRateLine.NatureAndQtyOfGoods?.Text ?? ZString.Empty,
						IsHSCodeLine = awbRateLine.IsHSCodeLine
					};

					rateLine.NatureAndQtyOfGoodsInfo.AddMessageError(() => !rateLine.NatureAndQtyOfGoods.IsWesternEuropeanOrEmpty,
						Res.GetString("4BA0243B-F751-4A22-9AC2-37D9F814D6BF", $"This text contains characters not supported by the Brazil Customs."));

					rateLine.Total = awbRateLine.ER_Total;

					cct.TotalGrossWeight.Value += awbRateLine.ER_GrossWeight;

					if (int.TryParse(awbRateLine.ER_NoOfPiecesOrRCP, out var noOfPieces))
					{
						rateLine.NoOfPieces = noOfPieces;
						cct.TotalNoOfPieces += noOfPieces;
					}

					rateLines.Add(rateLine);
				}

				var firstline = rateLines.FirstOrDefault();

				if (shipment.JS_GoodsDescription.IsEmpty && shipment.DetailedGoodsDescriptionNoteText.IsEmpty)
				{
					for (int i = rateLines.Count; i > 1; i--)
					{
						rateLines[i - 1].NatureAndQtyOfGoods = rateLines[i - 2].NatureAndQtyOfGoods;
						rateLines[i - 1].IsHSCodeLine = rateLines[i - 2].IsHSCodeLine;
					}
					firstline.NatureAndQtyOfGoods = ZString.Empty;
					firstline.NatureAndQtyOfGoodsInfo.AddMessageErrorIfEmpty(Res.GetString("06DC374B-AB6D-4139-AB82-871D4F3E1CFC", "Goods Des is mandatory for Brazil import"));
				}

				firstline.NatureAndQtyOfGoodsInfo.AddMessageError(() => rateLines.Sum(p => p.NatureAndQtyOfGoods.Length) > 600,
					Res.GetString("CCEF1A88-AC7F-42E9-BDC0-CDAF33E21248", "Exceeds 600 characters, not actually Goods Description"));
			}

			cct.TotalNoOfPiecesInfo.AddMessageError(() => cct.TotalNoOfPieces <= 0,
				Res.GetString("ECB137B3-1FC7-495C-90D2-3B498793AFFC", "Total No Of Pieces greater than zero is required for CCT messaging."));

			((Measurement)cct.TotalGrossWeight).ValueInfo.AddMessageError(() => cct.TotalGrossWeight.Value <= 0,
				Res.GetString("F23EA3A5-D4FE-4560-A026-36BE0BEFF969", "Total Gross Weight greater than zero is required for CCT messaging."));

			cct.RateLines = rateLines;
		}

		void PopulateSignature(CargoControlAndTransit cct, ExportAWBHeader appropriateAwbHeader, GlbStaff user)
		{
			cct.ShippersSignature = appropriateAwbHeader.Parent != null && !appropriateAwbHeader.IsAWBOverridden ? appropriateAwbHeader.GetShippersSignature(user) : appropriateAwbHeader.EH_ShippersSignature;
			cct.IssueDate = appropriateAwbHeader.EH_AWBIssueDate;
			cct.IssuePlace = appropriateAwbHeader.EH_AWBIssuePlace;
			cct.AgentsSignature = appropriateAwbHeader.EH_AWBAgentsSignature;
			cct.AgentApprovedExporterNumber = appropriateAwbHeader.EH_AgentApprovedExporterNumber;

			cct.ShippersSignatureInfo.AddMessageErrorIfEmpty(Res.GetString("BE529695-DDB6-452D-8E39-5D0068CF11CA", "Shippers Signature is required for CCT messaging."));
			cct.IssueDateInfo.AddMessageErrorIfEmpty(Res.GetString("39E4A248-E13E-45BA-90A5-28D09B91BA7E", "Issue Date is required for CCT messaging."));
			cct.IssueDateInfo.AddWarning(() => CargoControlAndTransitHelper.IsInvalidShipmentIssueDate(cct.IssueDate, shipment.JS_RL_NKDestination), Res.GetString("ED36FA0F-53EA-4296-A992-2335CF5EE6A8", "Future Issue Date detected for this Shipment which may result in the failure of the CCT House Manifest being sent at a later stage.\r\nPlease verify the date on the Shipment > Additional Details > View/Edit AWB > Executed on (date) OR Shipment > Basic Registration > Issue Date."));
			cct.IssuePlaceInfo.AddMessageErrorIfEmpty(Res.GetString("A3A60E59-C49D-449A-988F-28B952C4ACF4", "Issue Place is required for CCT messaging."));

			bool areBothEmpty() => cct.AgentsSignature.IsEmpty && cct.AgentApprovedExporterNumber.IsEmpty;
			cct.AgentsSignatureInfo.AddMessageError(areBothEmpty, Res.GetString("23B9CE4E-3CFD-4813-A2DF-7917A5A0EA2D", "Agents Signature or Agent Approved Exporter Number is required for CCT messaging."));
			cct.AgentApprovedExporterNumberInfo.AddMessageError(areBothEmpty, Res.GetString("84DD1385-F5C6-4DE5-8186-4E3382F50A4F", "Agents Signature or Agent Approved Exporter Number is required for CCT messaging."));
			cct.AddValidationDependencies(cct.AgentApprovedExporterNumberInfo, cct.AgentsSignatureInfo);

			cct.IsSignatureReadOnly = shipment.IsAir
				&& (shipment.Destination?.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Brazil
				&& CheckLatestLog(shipment);

			#region CheckLatestLog

			bool CheckLatestLog(IStmALogParent logParent)
			{
				return logParent.Logs
					.GetAllLogs()
					.Cast<StmALog>()
					.Any(log => log.SL_SE_NKEvent == Events.MessageSentCode && IsLogApplicable(log));

				#region IsLogApplicable

				bool IsLogApplicable(StmALog log)
				{
					var messageType = log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType);
					return string.Equals(messageType, "FHL", StringComparison.OrdinalIgnoreCase);
				}

				#endregion
			}

			#endregion
		}

		void PopulatePrepaidAndCollectValues(CargoControlAndTransit cct, ExportAWBHeader appropriateAwbHeader)
		{
			cct.TotalWeightPPD = appropriateAwbHeader.EH_TotalWeightPPD;
			cct.TotalWeightCOL = appropriateAwbHeader.EH_TotalWeightCOL;

			cct.ValuationPPD = appropriateAwbHeader.EH_ValuationPPD;
			cct.ValuationCOL = appropriateAwbHeader.EH_ValuationCOL;

			cct.TaxesPPD = appropriateAwbHeader.EH_TaxesPPD;
			cct.TaxesCOL = appropriateAwbHeader.EH_TaxesCOL;

			cct.OtherChargesDueAgentPPD = appropriateAwbHeader.EH_OtherChargesDueAgentPPD;
			cct.OtherChargesDueAgentCOL = appropriateAwbHeader.EH_OtherChargesDueAgentCOL;

			cct.OtherChargesDueCarrierPPD = appropriateAwbHeader.EH_OtherChargesDueCarrierPPD;
			cct.OtherChargesDueCarrierCOL = appropriateAwbHeader.EH_OtherChargesDueCarrierCOL;

			cct.WeightPrepaidCollect = new CodeDescription(appropriateAwbHeader.PrepaidCollectList)
			{
				Code = appropriateAwbHeader.EH_WeightPrepaidCollect,
			};

			cct.OtherPrepaidCollect = new CodeDescription(appropriateAwbHeader.PrepaidCollectList)
			{
				Code = appropriateAwbHeader.EH_OtherPrepaidCollect
			};

			cct.TotalPrepaid = cct.TotalWeightPPD + cct.ValuationPPD + cct.TaxesPPD + cct.OtherChargesDueAgentPPD + cct.OtherChargesDueCarrierPPD;
			cct.TotalCollect = cct.TotalWeightCOL + cct.ValuationCOL + cct.TaxesCOL + cct.OtherChargesDueAgentCOL + cct.OtherChargesDueCarrierCOL;
			bool areBothEmpty() => cct.TotalPrepaid == 0 && cct.TotalCollect == 0;
			var err = Res.GetString("CC696A16-7562-491D-B579-D5F578A1293D", "Total Charges are required for CCT Shipment messages.");
			cct.TotalPrepaidInfo.AddMessageError(areBothEmpty, err);
			cct.TotalCollectInfo.AddMessageError(areBothEmpty, err);
		}

		void PopulateCurrency(CargoControlAndTransit cct, ExportAWBHeader appropriateAwbHeader)
		{
			cct.Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
			{
				Code = appropriateAwbHeader.EH_Currency
			};
		}

		void PopulateChargesAndMonies(CargoControlAndTransit cct)
		{
			cct.Charges = new CodeDescription(awbHeader.ChargeCodesList)
			{
				Code = awbHeader.EH_ChargesCode,
			};

			cct.CarriageValue = new Money
			{
				Amount = awbHeader.EH_DeclaredValue,
				Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
				{
					Code = awbHeader.EH_HouseDeclaredValueCurrency
				}
			};

			cct.CustomsValue = new Money
			{
				Amount = awbHeader.EH_CustomsValue,
				Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
				{
					Code = awbHeader.EH_HouseCustomsValueCurrency
				}
			};

			cct.InsuranceValue = new Money
			{
				Amount = awbHeader.EH_InsuranceValue,
				Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
				{
					Code = awbHeader.EH_HouseInsuranceValueCurrency
				}
			};

			((Money)cct.InsuranceValue).AmountInfo.AddMessageError(() => cct.InsuranceValue.Amount < 0,
				Res.GetString("2DC7CDF9-9C7A-433E-AD44-177CBEDB69F2", "Insurance Value is required for CCT messaging."));
		}

		void PopulateRUCReferenceNumber(CargoControlAndTransit cct)
		{
			var numbers = (from CusEntryNumber n in shipment.Numbers where n.CE_EntryType == BrazilAdditionalReferenceNumberTypes.Codes.RUC select n).ToList();
			cct.RUCReferenceNumber = numbers.FirstOrDefault()?.CE_EntryNum ?? ZString.Empty;
			cct.RUCReferenceNumberInfo.AddWarning(() => numbers.Count > 1, Res.GetString("1DBE86AC-7B80-43FC-9D9D-9EFB74C98617", "CCT supports only one RUC and first available RUC will be sent."));
		}

		OrgCusCode GetCNPJ(OrgHeader org)
		{
			if (org != null)
			{
				return org
					.CustomsCodes
					.Cast<OrgCusCode>()
					.OrderBy(cusCode => cusCode.OK_CodeType)
					.FirstOrDefault(cusCode =>
						cusCode.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Brazil
						&& (cusCode.OK_CodeType == BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ ||
						cusCode.OK_CodeType == BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration ||
						cusCode.OK_CodeType == OrgCusCode.CodeTypes.PassportID));
			}
			else
			{
				return null;
			}
		}

		#region Validations

		bool VerifySecurityStatusInSpecialHandlingCodes(CargoControlAndTransitSpecialHandling specialHandling, List<CargoControlAndTransitSpecialHandling> specialHandlingList)
		{
			if (specialHandling.CodeAndDescription.Code == "NSC" || specialHandling.CodeAndDescription.Code == "SCO" ||
				specialHandling.CodeAndDescription.Code == "SPX" || specialHandling.CodeAndDescription.Code == "SHR")
			{
				return specialHandlingList.Count(itemList =>
						   itemList.CodeAndDescription.Code == "NSC" || itemList.CodeAndDescription.Code == "SCO" ||
						   itemList.CodeAndDescription.Code == "SPX" || itemList.CodeAndDescription.Code == "SHR") > 1;
			}

			return false;
		}

		bool IsDuplicatedItemInSpecialHandlingCodes(CargoControlAndTransitSpecialHandling specialHandling, List<CargoControlAndTransitSpecialHandling> specialHandlingList)
		{
			return specialHandlingList.Count(item =>
					   item.CodeAndDescription.Code != "" &&
					   item.CodeAndDescription.Code == specialHandling.CodeAndDescription.Code) > 1;
		}

		#endregion
	}
}
