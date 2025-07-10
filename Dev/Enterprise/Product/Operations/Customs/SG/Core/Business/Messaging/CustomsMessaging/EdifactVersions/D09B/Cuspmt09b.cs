using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D09B.Elements;
using Enterprise.Edifact.D09B.Messages.CUSPMT;
using Enterprise.Edifact.D09B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B
{
	public class Cuspmt09b : CUSPMTMessage, IPrintPermitTN41
	{
		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#region IPrintPermit Members

		public ZString TradeNetVersion
		{
			get { return UNH[0].MessageIdentifier.AssociationAssignedCode; }
		}

		public ZString PermitNumber
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup1 sg1 in Group1)
				{
					if (sg1.RFF[0].Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.GoodsDeclarationDocumentIdentifierCustoms)
					{
						result = sg1.RFF[0].Reference.ReferenceIdentifier;
					}
				}
				return result;
			}
		}

		public ZString UniqueRef
		{
			get
			{
				ZString reference = BGM[0].DocumentMessageIdentification.DocumentIdentifier;
				return reference.SubstringSafe(0, 17).Trim() + " " + reference.SubstringSafe(17, 8).Trim() + " " + reference.SubstringSafe(25, 4).PadRight(4, '0');
			}
		}

		public ZString MessageType
		{
			get
			{
				var messageList = new CommonAccessReferenceCodeList();
				return GetDescriptionInUpperCase(messageList, UNH[0].CommonAccessReference);
			}
		}

		public ZString DeclarationType
		{
			get
			{
				var result = ZString.Empty;
				var declarationTypeList = new DeclarationTypeCodeList();
				var decType = BGM[0].DocumentMessageName.DocumentName;
				if (decType != "BKT")
				{
					result = GetDescriptionInUpperCase(declarationTypeList, decType);
				}
				else
				{
					var messageType = UNH[0].CommonAccessReference.Substring(0, 3);
					if (messageType == MessageTypeCodeList.Codes.INP)
					{
						result = DeclarationTypeCodeList.Descriptions.BKN.ToString().ToUpper();
					}
					else if (messageType == MessageTypeCodeList.Codes.OUT)
					{
						result = DeclarationTypeCodeList.Descriptions.BKO.ToString().ToUpper();
					}
					else if (messageType == MessageTypeCodeList.Codes.IPT)
					{
						result = DeclarationTypeCodeList.Descriptions.BKP.ToString().ToUpper();
					}
				}

				return result;
			}
		}

		#region Importer

		public ZString Importer
		{
			get { return ZString.Empty; } // see below
		}

		public NADSegment ImporterSegment
		{
			get
			{
				NADSegment result = null;
				foreach (SegmentGroup6 sg6 in Group6)
				{
					var nad = sg6.NAD[0];
					if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Importer)
					{
						result = nad;
						break;
					}
				}

				return result;
			}
		}

		public ZString ImporterNameLine1
		{
			get
			{
				return ImporterSegment == null ? ZString.Empty : new ZString(ImporterSegment.PartyName.PartyName1);
			}
		}

		public ZString ImporterNameLine2
		{
			get
			{
				return ImporterSegment == null ? ZString.Empty : new ZString(ImporterSegment.PartyName.PartyName2);
			}
		}

		public ZString ImporterUEN
		{
			get
			{
				var result = ZString.Empty;
				return ImporterSegment == null ? ZString.Empty : new ZString(ImporterSegment.PartyIdentificationDetails.PartyIdentifier);
			}
		}

		#endregion

		#region Exporter

		public ZString Exporter
		{
			get { return ZString.Empty; } // see below
		}

		public NADSegment ExporterSegment
		{
			get
			{
				NADSegment result = null;
				foreach (SegmentGroup6 sg6 in Group6)
				{
					var nad = sg6.NAD[0];
					if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Exporter)
					{
						result = nad;
						break;
					}
				}
				return result;
			}
		}

		public ZString ExporterNameLine1
		{
			get
			{
				return ExporterSegment == null ? ZString.Empty : new ZString(ExporterSegment.PartyName.PartyName1);
			}
		}

		public ZString ExporterNameLine2
		{
			get
			{
				return ExporterSegment == null ? ZString.Empty : new ZString(ExporterSegment.PartyName.PartyName2);
			}
		}

		public ZString ExporterUEN
		{
			get
			{
				var result = ZString.Empty;
				return ExporterSegment == null ? ZString.Empty : new ZString(ExporterSegment.PartyIdentificationDetails.PartyIdentifier);
			}
		}

		#endregion

		#region HandlingAgent

		public ZString HandlingAgent
		{
			get { return ZString.Empty; } // see below
		}

		public NADSegment HandlingAgentSegment
		{
			get
			{
				NADSegment result = null;
				foreach (SegmentGroup6 sg6 in Group6)
				{
					var nad = sg6.NAD[0];
					if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.TransitPrincipalsAgentRepresentative)
					{
						result = nad;
						break;
					}
				}
				return result;
			}
		}

		public ZString HandlingAgentNameLine1
		{
			get
			{
				return HandlingAgentSegment == null ? ZString.Empty : new ZString(HandlingAgentSegment.PartyName.PartyName1);
			}
		}

		public ZString HandlingAgentNameLine2
		{
			get
			{
				return HandlingAgentSegment == null ? ZString.Empty : new ZString(HandlingAgentSegment.PartyName.PartyName2);
			}
		}

		public ZString HandlingAgentNameLine3
		{
			get
			{
				var result = ZString.Empty;
				return HandlingAgentSegment == null ? ZString.Empty : new ZString(HandlingAgentSegment.PartyName.PartyName1 + HandlingAgentSegment.PartyName.PartyName2).SubstringSafe(70, 30);
			}
		}

		public ZString HandlingAgentUEN
		{
			get
			{
				var result = ZString.Empty;
				return HandlingAgentSegment == null ? ZString.Empty : new ZString(HandlingAgentSegment.PartyIdentificationDetails.PartyIdentifier);
			}
		}

		#endregion

		public ZString PortOfLoading
		{
			get
			{
				var result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PlaceOfLoading)
					{
						if (!string.IsNullOrEmpty(loc.LocationIdentification.LocationIdentifier))
						{
							var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, loc.LocationIdentification.LocationIdentifier, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
							if (cusCode != null)
							{
								result = cusCode.ZZD_Description.ToUpper();
							}
						}
					}
				}
				return result;
			}
		}

		public ZString NextPortOfCall
		{
			get
			{
				var result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.NextPortOfCall)
					{
						if (!string.IsNullOrEmpty(loc.LocationIdentification.LocationIdentifier))
						{
							var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, loc.LocationIdentification.LocationIdentifier, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
							if (cusCode != null)
							{
								result = cusCode.ZZD_Description.ToUpper();
							}
						}
					}
				}
				return result;
			}
		}

		public ZString PortOfDischarge
		{
			get
			{
				var result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PortOfDischarge)
					{
						if (loc.LocationIdentification.LocationIdentifier != ZString.Empty)
						{
							var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, loc.LocationIdentification.LocationIdentifier, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
							if (cusCode != null)
							{
								result = cusCode.ZZD_Description.ToUpper();
							}
						}
					}
				}
				return result;
			}
		}

		public ZString FinalPortOfCall
		{
			get
			{
				var result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PlaceOfUltimateDestinationOfConveyance)
					{
						if (loc.LocationIdentification.LocationIdentifier != ZString.Empty)
						{
							var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, loc.LocationIdentification.LocationIdentifier, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
							if (cusCode != null)
							{
								result = cusCode.ZZD_Description.ToUpper();
							}
						}
					}
				}
				return result;
			}
		}

		public ZString OutVesLocation
		{
			get { return ZString.Empty; } // not used in TNV4.1
		}

		public ZString CountryOfFinalDest
		{
			get
			{
				var result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.CountryOfUltimateDestination)
					{
						if (!string.IsNullOrEmpty(loc.LocationIdentification.LocationIdentifier))
						{
							var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, loc.LocationIdentification.LocationIdentifier));
							if (country != null)
							{
								result = country.RN_DescMultilingual.GetUnresolvedString().ToUpper();
							}
						}
					}
				}
				return result;
			}
		}

		#region InwardCarrierAgent

		public ZString InwardCarrierAgent
		{
			get { return ZString.Empty; } // see below
		}

		public NADSegment InwardCarrierAgentSegment
		{
			get
			{
				NADSegment result = null;
				foreach (SegmentGroup6 sg6 in Group6)
				{
					var nad = sg6.NAD[0];
					if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.CarriersAgent)
					{
						result = nad;
						break;
					}
				}

				return result;
			}
		}

		public ZString InwardCarrierAgentNameLine1
		{
			get
			{
				return InwardCarrierAgentSegment == null ? ZString.Empty : new ZString(InwardCarrierAgentSegment.PartyName.PartyName1 + InwardCarrierAgentSegment.PartyName.PartyName2).SubstringSafe(0, 35);
			}
		}

		public ZString InwardCarrierAgentNameLine2
		{
			get
			{
				return InwardCarrierAgentSegment == null ? ZString.Empty : new ZString(InwardCarrierAgentSegment.PartyName.PartyName1 + InwardCarrierAgentSegment.PartyName.PartyName2).SubstringSafe(35, 35);
			}
		}

		public ZString InwardCarrierAgentNameLine3
		{
			get
			{
				return InwardCarrierAgentSegment == null ? ZString.Empty : new ZString(InwardCarrierAgentSegment.PartyName.PartyName1 + InwardCarrierAgentSegment.PartyName.PartyName2).SubstringSafe(70, 30);
			}
		}

		#endregion

		#region OutwardCarrierAgent

		public ZString OutwardCarrierAgent
		{
			get { return ZString.Empty; } // see below
		}

		public NADSegment OutwardCarrierAgentSegment
		{
			get
			{
				NADSegment result = null;
				foreach (SegmentGroup6 sg6 in Group6)
				{
					var nad = sg6.NAD[0];
					if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Carrier)
					{
						result = nad;
						break;
					}
				}

				return result;
			}
		}

		public ZString OutwardCarrierAgentNameLine1
		{
			get
			{
				return OutwardCarrierAgentSegment == null ? ZString.Empty : new ZString(OutwardCarrierAgentSegment.PartyName.PartyName1 + OutwardCarrierAgentSegment.PartyName.PartyName2).SubstringSafe(0, 35);
			}
		}

		public ZString OutwardCarrierAgentNameLine2
		{
			get
			{
				return OutwardCarrierAgentSegment == null ? ZString.Empty : new ZString(OutwardCarrierAgentSegment.PartyName.PartyName1 + OutwardCarrierAgentSegment.PartyName.PartyName2).SubstringSafe(35, 35);
			}
		}

		public ZString OutwardCarrierAgentNameLine3
		{
			get
			{
				return OutwardCarrierAgentSegment == null ? ZString.Empty : new ZString(OutwardCarrierAgentSegment.PartyName.PartyName1 + OutwardCarrierAgentSegment.PartyName.PartyName2).SubstringSafe(70, 30);
			}
		}

		#endregion

		public ZString PlaceOfReleaseName => GetSingaporePlaceName(LocationFunctionCodeQualifierList.PlaceOfDischarge);

		public ZString PlaceOfReleaseCode => GetSingaporePlaceCode(LocationFunctionCodeQualifierList.PlaceOfDischarge);

		public ZString PlaceOfReceiptName => GetSingaporePlaceName(LocationFunctionCodeQualifierList.PlaceOfReceipt);

		public ZString PlaceOfReceiptCode => GetSingaporePlaceCode(LocationFunctionCodeQualifierList.PlaceOfReceipt);

		public ZString PlaceOfReceipt => string.Empty;

		public ZString PlaceOfRelease => string.Empty;

		public ZDate ValidityPeriodFrom
		{
			get
			{
				foreach (SegmentGroup1 group1 in Group1)
				{
					foreach (RFFSegment rFF in group1.RFF)
					{
						if (rFF.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.GoodsDeclarationDocumentIdentifierCustoms)
						{
							foreach (DTMSegment dTM in group1.DTM)
							{
								if (dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.ValidityPeriod)
								{
									ZString validityPeriod = dTM.DateTimePeriod.DateOrTimeOrPeriodText;
									validityPeriod = validityPeriod.SubstringSafe(0, 8);
									return new ZDate(Convert.ToInt32(validityPeriod.Substring(0, 4)), Convert.ToInt32(validityPeriod.Substring(4, 2)), Convert.ToInt32(validityPeriod.Substring(6, 2)));
								}
							}
						}
					}
				}

				return new ZDate();
			}
		}

		public ZDate ValidityPeriodTo
		{
			get
			{
				foreach (SegmentGroup1 group1 in Group1)
				{
					foreach (RFFSegment rFF in group1.RFF)
					{
						if (rFF.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.GoodsDeclarationDocumentIdentifierCustoms)
						{
							foreach (DTMSegment dTM in group1.DTM)
							{
								if (dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.ValidityPeriod)
								{
									ZString validityPeriod = dTM.DateTimePeriod.DateOrTimeOrPeriodText;
									validityPeriod = validityPeriod.SubstringSafe(8, 8);
									return new ZDate(Convert.ToInt32(validityPeriod.Substring(0, 4)), Convert.ToInt32(validityPeriod.Substring(4, 2)), Convert.ToInt32(validityPeriod.Substring(6, 2)));
								}
							}
						}
					}
				}

				return new ZDate();
			}
		}

		public ZString TotalGrossWt
		{
			get
			{
				var result = ZString.Empty;
				foreach (MEASegment mea in MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.DimensionsTotalWeight)
					{
						result = mea.ValueRange.Measure.PadLeft(15, ' ') + "/" + mea.ValueRange.MeasurementUnitCode;
					}
				}
				return result;
			}
		}

		public ZString TotalOuterPack
		{
			get
			{
				var result = ZString.Empty;
				foreach (MEASegment mea in MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.ExternalDimension)
					{
						result = mea.ValueRange.Measure.PadLeft(8, ' ') + "/" + mea.ValueRange.MeasurementUnitCode;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalCustomsDUTPayable
		{
			get { return GetTotalAmountFromSegment(DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, MonetaryAmountTypeCodeQualifierList.DutyAmount); }
		}

		public ZDecimal TotalOtherTaxPayable
		{
			get { return GetTotalAmountFromSegment(DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, MonetaryAmountTypeCodeQualifierList.TaxAmount); }
		}

		public ZDecimal TotalExciseDUTPayable
		{
			get { return GetTotalAmountFromSegment(DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty, MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount); }
		}

		public ZDecimal TotalGstAmount
		{
			get { return GetTotalAmountFromSegment(DutyOrTaxOrFeeFunctionCodeQualifierList.Tax, MonetaryAmountTypeCodeQualifierList.GoodsAndServicesTax); }
		}

		public ZDecimal TotalAmountPayable
		{
			get { return GetTotalAmountFromSegment(DutyOrTaxOrFeeFunctionCodeQualifierList.TotalOfAllDutiesTaxesAndFeesCustomsItem, MonetaryAmountTypeCodeQualifierList.AmountDueAmountPayable); }
		}

		public ZString CargoPackingType
		{
			get
			{
				var cargoList = new CargoPackingCodeList();
				return GetDescriptionInUpperCase(cargoList, CST[0].CustomsIdentityCodes1.CustomsGoodsIdentifier);
			}
		}

		public ZString InVesName
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup4 gR4 in Group4)
				{
					foreach (TDTSegment tdt in gR4.TDT)
					{
						if (tdt.TransportStageCodeQualifier == TransportStageCodeQualifierList.AtArrival)
						{
							result = tdt.TransportIdentification.TransportMeansIdentificationName;
						}
					}
				}

				return result;
			}
		}

		public ZString InVoyageFlightNumber
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup4 gR4 in Group4)
				{
					foreach (TDTSegment tdt in gR4.TDT)
					{
						if (tdt.TransportStageCodeQualifier == TransportStageCodeQualifierList.AtArrival)
						{
							result = tdt.MeansOfTransportJourneyIdentifier;
						}
					}
				}

				return result;
			}
		}

		public ZString InOBLMawbNb
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup5 gR5 in Group5)
				{
					foreach (DOCSegment doc in gR5.DOC)
					{
						if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterBillOfLading) // MasterBillOfLading is the code value used for Inward OBL / MAWB
						{
							result = doc.DocumentMessageDetails.DocumentIdentifier;
							break;
						}
					}
				}

				var decType = BGM[0].DocumentMessageName.DocumentName;
				if (result.IsEmpty && (decType != DeclarationTypeCodeList.Codes.TTI && decType != DeclarationTypeCodeList.Codes.TTF))
				{
					foreach (SegmentGroup32 sg32 in Group32)
					{
						foreach (SegmentGroup39 sg39 in sg32.Group39)
						{
							foreach (DOCSegment doc in sg39.DOC)
							{
								if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterBillOfLading)
								{
									result = doc.DocumentMessageDetails.DocumentIdentifier;
									break;
								}
							}
						}
					}
				}

				return result;
			}
		}

		public ZDate ArrivalDate
		{
			get
			{
				var result = new ZDate();
				foreach (DTMSegment dtm in DTM)
				{
					if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeActual)
					{
						var arrivalDate = dtm.DateTimePeriod.DateOrTimeOrPeriodText;
						if (!string.IsNullOrEmpty(arrivalDate))
						{
							result = new ZDate(Convert.ToInt32(arrivalDate.Substring(0, 4)), Convert.ToInt32(arrivalDate.Substring(4, 2)), Convert.ToInt32(arrivalDate.Substring(6, 2)));
						}
					}
				}

				return result;
			}
		}

		public ZString OutVesName
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup4 gR4 in Group4)
				{
					foreach (TDTSegment tdt in gR4.TDT)
					{
						if (tdt.TransportStageCodeQualifier == TransportStageCodeQualifierList.AtDeparture)
						{
							result = tdt.TransportIdentification.TransportMeansIdentificationName;
						}
					}
				}

				return result;
			}
		}

		public ZString OutVoyageFlightNumber
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup4 gR4 in Group4)
				{
					foreach (TDTSegment tdt in gR4.TDT)
					{
						if (tdt.TransportStageCodeQualifier == TransportStageCodeQualifierList.AtDeparture)
						{
							result = tdt.MeansOfTransportJourneyIdentifier;
						}
					}
				}

				return result;
			}
		}

		public ZString TowingVesselName
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup4 gR4 in Group4)
				{
					foreach (TDTSegment tdt in gR4.TDT)
					{
						if (tdt.TransportStageCodeQualifier == TransportStageCodeQualifierList.InlandWaterwayTransport)
						{
							result = tdt.MeansOfTransportJourneyIdentifier;
						}
					}
				}

				return result;
			}
		}

		public ZString OutOBLMawbNb
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup5 gR5 in Group5)
				{
					foreach (DOCSegment doc in gR5.DOC)
					{
						if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterAirWaybill) // MasterAirWaybill is the code value used for Outward OBL / MAWB
						{
							result = doc.DocumentMessageDetails.DocumentIdentifier;
							break;
						}
					}
				}

				var decType = BGM[0].DocumentMessageName.DocumentName;
				if (result.IsEmpty && (decType != DeclarationTypeCodeList.Codes.TTI && decType != DeclarationTypeCodeList.Codes.TTF))
				{
					foreach (SegmentGroup32 sg32 in Group32)
					{
						foreach (SegmentGroup39 sg37 in sg32.Group39)
						{
							foreach (DOCSegment doc in sg37.DOC)
							{
								if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterAirWaybill)
								{
									result = doc.DocumentMessageDetails.DocumentIdentifier;
									break;
								}
							}
						}
					}
				}

				return result;
			}
		}

		public ZDate DepartureDate
		{
			get
			{
				var result = ZString.Empty;
				foreach (DTMSegment dtm in DTM)
				{
					if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTimeActual_136)
					{
						result = dtm.DateTimePeriod.DateOrTimeOrPeriodText;
					}
				}
				if (result != "")
				{
					return new ZDate(Convert.ToInt32(result.Substring(0, 4)), Convert.ToInt32(result.Substring(4, 2)), Convert.ToInt32(result.Substring(6, 2)));
				}
				else
				{
					return new ZDate();
				}
			}
		}

		public ZString LicenceNo
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup1 sg1 in Group1)
				{
					if (sg1.RFF[0].Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.DocumentIdentifier)
					{
						result += sg1.RFF[0].Reference.ReferenceIdentifier + "\r\n";
					}
				}
				return result.TrimEnd();
			}
		}

		public ZString CertificateNo
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup1 sg1 in Group1)
				{
					if (sg1.RFF[0].Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.OriginalCertificateNumber)
					{
						result = sg1.RFF[0].Reference.ReferenceIdentifier;
					}
				}
				return result;
			}
		}

		public ZString CustomsProcedureCodes
		{
			get
			{
				var result = ZString.Empty;

				foreach (SegmentGroup1 sg1 in Group1)
				{
					if (sg1.RFF[0].Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.DeclarantsReferenceNumber)
					{
						ZString cpc = sg1.RFF[0].Reference.ReferenceIdentifier;
						var procedureCode = cpc.SubstringSafe(0, 3);
						var concession = cpc.SubstringSafe(3, 4);
						var refCusProcedure = new RefCusProcedure.Loader(Factory).LoadFromProcedureAndPreviousProcedureAndConcession(procedureCode, ZString.Empty, concession, ZString.Empty, Core.Constants.CountryCodes.Singapore, ZDateTime.Today);
						if (refCusProcedure != null)
						{
							result += refCusProcedure.ZZ6_Description.Split('(').FirstOrDefault().Trim();
						}
					}
				}

				return result.TrimEnd();
			}
		}

		public IPrintPermitConsignment[] ConsignmentDetails
		{
			get
			{
				CUSPMTPrintPermitConsignment[] result = new CUSPMTPrintPermitConsignment[Group32.Count];
				for (int i = 0; i < Group32.Count; i++)
				{
					result.SetValue(new CUSPMTPrintPermitConsignment(Group32[i], Group11, BondedIntoOrReleasedFromBond), i);
				}
				return result;
			}
		}

		protected bool BondedIntoOrReleasedFromBond
		{
			get
			{
				bool result = false;
				var location = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, PlaceOfReleaseCode);
				if (location.IsLicencedPremise())
				{
					result = true;
				}

				if (!result && !PlaceOfStorageCode.IsEmpty)
				{
					location = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, PlaceOfStorageCode);
					if (location.IsLicencedPremise())
					{
						result = true;
					}
				}

				return result;
			}
		}

		protected ZString PlaceOfStorageCode
		{
			get
			{
				var result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.GoodsItemStorageLocation)
					{
						result = loc.LocationIdentification.LocationIdentifier;
					}
				}

				return result;
			}
		}

		public ZString ManufacturerName
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup11 sg11 in Group11)
				{
					foreach (SegmentGroup15 sg15 in sg11.Group15)
					{
						foreach (NADSegment nad in sg15.NAD)
						{
							if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Supplier)
							{
								result = nad.PartyName.PartyName1 + nad.PartyName.PartyName2;
							}
						}
					}
				}
				return result;
			}
		}

		public ZString[] TradersRemark
		{
			get
			{
				List<ZString> result = new List<ZString>();
				foreach (FTXSegment ftx in FTX)
				{
					if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.GeneralInformation)
					{
						result.Add(ftx.TextLiteral.FreeText1);
						if (!ftx.TextLiteral.FreeText2.IsNullOrEmpty())
						{
							result.Add(ftx.TextLiteral.FreeText2);
							if (!ftx.TextLiteral.FreeText3.IsNullOrEmpty())
							{
								result.Add(ftx.TextLiteral.FreeText3);
								if (!ftx.TextLiteral.FreeText4.IsNullOrEmpty())
								{
									result.Add(ftx.TextLiteral.FreeText4);
									if (!ftx.TextLiteral.FreeText5.IsNullOrEmpty())
									{
										result.Add(ftx.TextLiteral.FreeText5);
									}
								}
							}
						}
					}
				}
				return result.ToArray();
			}
		}

		public IPrintPermitContainers[] ContainerIdentifiers
		{
			get
			{
				IPrintPermitContainers[] result = new IPrintPermitContainers[(EQD.Count)];
				CUSPMTPrintPermitContainers currentContainer = null;

				for (int i = 0; i < EQD.Count; i++)
				{
					currentContainer = new CUSPMTPrintPermitContainers((i + 1), GetContainerIdentifier(EQD[i], i));
					result[i] = currentContainer;
				}

				return result;
			}
		}

		string GetContainerIdentifier(EQDSegment eqd, int sealIndex)
		{
			string sealNumber = SEL.Count > sealIndex ? SEL[sealIndex].TransportUnitSealIdentifier : "";
			ZString containerDetails = eqd.EquipmentSizeAndType.EquipmentSizeAndTypeDescription;
			string type = containerDetails.Left(3);
			string size = containerDetails.SubstringSafe(3, 2);
			string weight = eqd.EquipmentSizeAndType.EquipmentSizeAndTypeDescriptionCode;

			return eqd.EquipmentIdentification.EquipmentIdentifier.PadRight(13, ' ') + " " + type + " " + size + " " + weight.PadLeft(3, '0') + " " + sealNumber;
		}

		public ZString NameOfCompany
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup6 sg6 in Group6)
				{
					var nad = sg6.NAD[0];
					if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.DeclarantsAgentRepresentative)
					{
						result = nad.PartyName.PartyName1 + nad.PartyName.PartyName2;
					}
				}

				if (result.IsEmpty)
				{
					result = ImporterNameLine1.IsEmpty ? new ZString(ExporterNameLine1 + " " + ExporterNameLine2) : new ZString(ImporterNameLine1 + " " + ImporterNameLine2);
				}

				return result;
			}
		}

		public ZString EntityIdentOfCompany
		{
			get { return ZString.Empty; } // not required for TN4.1
		}

		public ZString DeclarantName
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup6 sg6 in Group6)
				{
					if (sg6.NAD[0].PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Declarant)
					{
						result = sg6.CTA[0].ContactDetails.ContactName;
						break;
					}
				}
				return result;
			}
		}

		public ZString DeclarantCode
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup6 sg6 in Group6)
				{
					if (sg6.NAD[0].PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Declarant)
					{
						result = sg6.CTA[0].ContactDetails.ContactIdentifier;
						result = result.Right(5).PadLeft(result.Length, 'X');
						break;
					}
				}
				return result;
			}
		}

		public ZString TelNb
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup6 sg6 in Group6)
				{
					if (sg6.NAD[0].PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Declarant)
					{
						foreach (COMSegment com in sg6.COM)
						{
							if (com.CommunicationContact.CommunicationMeansTypeCode == CommunicationMediumTypeCodeList.Telephone)
							{
								result = com.CommunicationContact.CommunicationAddressIdentifier;
							}
						}
					}
				}
				return result;
			}
		}

		public ITN41PermitConditions[] CAPermitConditions
		{
			get { return PermitConditions(TextSubjectCodeQualifierList.RegulatoryInformation); }
		}

		public ITN41PermitConditions[] CustomsPermitConditions
		{
			get { return PermitConditions(TextSubjectCodeQualifierList.CustomsClearanceInstructions); }
		}

		public ITN41PermitConditions[] PermitConditions(TextSubjectCodeQualifierList qualifier)
		{
			List<ITN41PermitConditions> conditions = new List<ITN41PermitConditions>();

			foreach (SegmentGroup1 sg1 in Group1)
			{
				int pos = 0;
				for (int i = 0; i < sg1.FTX.Count; i++)
				{
					if (sg1.FTX[i].TextSubjectCodeQualifier == qualifier)
					{
						ZString condition = sg1.FTX[i].TextLiteral.FreeText1.Replace("\\", "/");

						var conditionx = new CuspmtPrintPermitConditions("<b><ExpandToFit>" + sg1.FTX[i].TextReference.FreeTextDescriptionCode.PadRight(4, ' ') + "</b>" + " - " + condition.SubstringSafe(0, 73));
						conditions.Add(conditionx);
						pos = 73;

						while (pos < condition.Length)
						{
							conditions.Add(new CuspmtPrintPermitConditions(condition.SubstringSafe(pos, 80)));
							pos += 80;
						}
					}
				}
			}

			return conditions.ToArray();
		}

		public ZDate AmendDate
		{
			get
			{
				if (AmendFields.Length != 0)
				{
					foreach (SegmentGroup1 sg1 in Group1)
					{
						foreach (DTMSegment dtm in sg1.DTM)
						{
							if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.GoodsDeclarationDocumentAcceptanceDateTime)
							{
								ZString amendmentDate = dtm.DateTimePeriod.DateOrTimeOrPeriodText;
								return new ZDate(Convert.ToInt32(amendmentDate.Substring(0, 4)), Convert.ToInt32(amendmentDate.Substring(4, 2)), Convert.ToInt32(amendmentDate.Substring(6, 2)));
							}
						}
					}
				}
				return new ZDate();
			}
		}

		public ZString[] AmendFields
		{
			get
			{
				List<ZString> result = new List<ZString>();
				ZString resultLine;
				foreach (SegmentGroup1 sg1 in Group1)
				{
					foreach (FTXSegment ftx in sg1.FTX)
					{
						if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.CustomsDeclarationInformation)
						{
							resultLine = new UpdateSummaryCode().GetTN41FieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText1);
							result.Add(resultLine);
							if (ftx.TextLiteral.FreeText2.Length > 0)
							{
								resultLine = new UpdateSummaryCode().GetTN41FieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText2);
								result.Add(resultLine);
								if (ftx.TextLiteral.FreeText3.Length > 0)
								{
									resultLine = new UpdateSummaryCode().GetTN41FieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText3);
									result.Add(resultLine);
									if (ftx.TextLiteral.FreeText4.Length > 0)
									{
										resultLine = new UpdateSummaryCode().GetTN41FieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText4);
										result.Add(resultLine);
										if (ftx.TextLiteral.FreeText5.Length > 0)
										{
											resultLine = new UpdateSummaryCode().GetTN41FieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText5);
											result.Add(resultLine);
										}
									}
								}
							}
						}
					}
				}
				return result.ToArray();
			}
		}

		#region Skip Printing Heading Indicators (for empty line details)

		public ZBool HideMawbLine
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = (printConsignment.InwardMawbObl.IsEmpty && printConsignment.OutwardMawbObl.IsEmpty);
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZBool HideHawbLine
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = (printConsignment.InwardHawbHbl.IsEmpty && printConsignment.OutwardHawbHbl.IsEmpty);
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZBool HideCifFobValue
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = printConsignment.CifFobLspValue.IsEmpty;
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZBool HideLspValue
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = printConsignment.LspAmount.IsEmpty;
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZBool HideGstValue
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = printConsignment.GstAmount.IsEmpty;
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZBool HideDutQtyWtVolValue
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = printConsignment.DutQuantity.IsEmpty;
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZBool HideUnitPriceValue
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = printConsignment.UnitPrice.IsEmpty;
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZBool HideExciseValue
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = printConsignment.ExciseDutyPayable.IsEmpty;
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZBool HideDutyValue
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = printConsignment.CustomsDutyPayable.IsEmpty;
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZBool HideOtherTaxValue
		{
			get
			{
				var result = ZBool.True;

				foreach (IPrintPermitConsignment printConsignment in ConsignmentDetails)
				{
					result = printConsignment.OtherTaxPayable.IsEmpty;
					if (!result)
					{
						break;
					}
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Segment Methods

		ZString GetSingaporePlaceName(LocationFunctionCodeQualifierList functionCodeQualifier)
		{
			var result = ZString.Empty;

			foreach (LOCSegment loc in LOC)
			{
				if (loc.LocationFunctionCodeQualifier == functionCodeQualifier)
				{
					var locationIdentification = loc.LocationIdentification;

					if (locationIdentification != null)
					{
						var place = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, locationIdentification.LocationIdentifier);
						result = place != null && place.IsLocationAddressToBePrinted() ? locationIdentification.LocationName : string.Empty;
					}
				}
			}

			return result;
		}

		ZString GetSingaporePlaceCode(LocationFunctionCodeQualifierList functionCodeQualifier)
		{
			var result = ZString.Empty;

			foreach (LOCSegment loc in LOC)
			{
				if (loc.LocationFunctionCodeQualifier == functionCodeQualifier)
				{
					result = loc.LocationIdentification?.LocationIdentifier;
				}
			}

			return result;
		}

		ZDecimal GetTotalAmountFromSegment(DutyOrTaxOrFeeFunctionCodeQualifierList dutyOrTaxQualifier, MonetaryAmountTypeCodeQualifierList monetaryQualifier)
		{
			var result = ZDecimal.Zero;
			foreach (SegmentGroup51 sg51 in Group51)
			{
				if (sg51.TAX[0].DutyOrTaxOrFeeFunctionCodeQualifier == dutyOrTaxQualifier)
				{
					foreach (MOASegment moa in sg51.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == monetaryQualifier)
						{
							var amount = moa.MonetaryAmount.MonetaryAmount;
							if (!string.IsNullOrEmpty(amount))
							{
								result = Convert.ToDecimal(amount);
							}
						}
					}
				}
			}

			return result;
		}

		#endregion

		protected string GetDescriptionInUpperCase(CodeDescriptionPairList list, string code)
		{
			var result = list.GetDescriptionFromCode(code);
			return result == null ? string.Empty : result.ToUpper(CultureInfo.CurrentCulture);
		}
	}

	#region CUSPMTPrintPermitConsignment

	public class CUSPMTPrintPermitConsignment : IPrintPermitConsignment
	{
		public CUSPMTPrintPermitConsignment(SegmentGroup32 sg32, SegmentGroup11MessageSection invoices, bool bondedIntoOrReleasedFromBond)
		{
			sG32 = sg32;

			this.invoices = invoices;
			this.bondedIntoOrReleasedFromBond = bondedIntoOrReleasedFromBond;
		}
		readonly SegmentGroup32 sG32;
		readonly SegmentGroup11MessageSection invoices;
		readonly bool bondedIntoOrReleasedFromBond;

		#region IPrintPermitConsignment Members

		public ZString SerialNb
		{
			get { return sG32.CST[0].GoodsItemNumber.PadLeft(2, '0').PadLeft(5, ' '); }
		}

		public ZString HSCode
		{
			get { return sG32.CST[0].CustomsIdentityCodes1.CustomsGoodsIdentifier; }
		}

		public ZString BrandName
		{
			get
			{
				var result = ZString.Empty;
				foreach (FTXSegment ftx in sG32.FTX)
				{
					if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.ProductInformation)
					{
						result = ftx.TextLiteral.FreeText1;
					}
				}
				return result;
			}
		}

		public ZString ItemInvoiceNumber
		{
			get
			{
				var result = string.Empty;

				foreach (SegmentGroup37 sg37 in sG32.Group37)
				{
					foreach (RFFSegment rff in sg37.RFF)
					{
						if (rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.InvoiceDocumentIdentifier)
						{
							result = rff.Reference.ReferenceIdentifier;
						}
					}
				}

				return result;
			}
		}

		public ZString ManufacturerName => manufacturerName ?? (manufacturerName = GetManufacturerName());
		string manufacturerName;

		public ZString HSQuantity
		{
			get
			{
				var result = ZString.Empty;
				foreach (MEASegment mea in sG32.MEA)
				{
					// SG QA Nov-2011 #10672: "HS Qty & Unit field in CCP: For dutiable items, and non-dutiable items bonded into or released from bonded warehouse, specify total dutaable/non-dutiable quantity/weight/volume and unit of measurement"
					if (CustomsDutyPayable > 0 || ExciseDutyPayable > 0 || bondedIntoOrReleasedFromBond)
					{
						if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.Measurement)
						{
							result = mea.ValueRange.Measure;
						}
					}
					else if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.CustomsLineItemMeasurement)
					{
						result = mea.ValueRange.Measure;
					}
				}
				return result;
			}
		}

		public ZString HSQuantityUnit
		{
			get
			{
				var result = ZString.Empty;
				foreach (MEASegment mea in sG32.MEA)
				{
					if (CustomsDutyPayable > 0 || ExciseDutyPayable > 0 || bondedIntoOrReleasedFromBond)
					{
						if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.Measurement)
						{
							result = mea.ValueRange.MeasurementUnitCode;
						}
					}
					else if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.CustomsLineItemMeasurement)
					{
						result = mea.ValueRange.MeasurementUnitCode;
					}
				}
				return result;
			}
		}

		public ZString Marking
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup33 sg33 in sG32.Group33)
				{
					foreach (SegmentGroup34 sg34 in sg33.Group34)
					{
						if (sg34.PCI[0].MarkingInstructionsCode == MarkingInstructionsCodeList.LegalRequirements)
						{
							result = sg34.PCI[0].MarksLabels.ShippingMarksDescription1;
						}
					}
				}

				return result;
			}
		}

		public ZString CityOfOrigin
		{
			get
			{
				var result = ZString.Empty;
				foreach (LOCSegment loc in sG32.LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.CountryOfOrigin)
					{
						result = loc.LocationIdentification.LocationIdentifier;
					}
				}
				return result;
			}
		}

		public ZString Model
		{
			get
			{
				var result = ZString.Empty;
				foreach (FTXSegment ftx in sG32.FTX)
				{
					if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.ProductInformation)
					{
						result = ftx.TextLiteral.FreeText2;
					}
				}
				return result;
			}
		}

		public ZString DutQuantity
		{
			get
			{
				var result = ZString.Empty;
				foreach (MEASegment mea in sG32.MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.ItemWeight)
					{
						result = mea.ValueRange.Measure;
					}
				}
				return result;
			}
		}

		public ZString DutQuantityUnit
		{
			get
			{
				var result = ZString.Empty;
				foreach (MEASegment mea in sG32.MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.ItemWeight)
					{
						result = mea.ValueRange.MeasurementUnitCode;
					}
				}
				return result;
			}
		}

		public ZString InwardMawbObl
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup39 sg39 in sG32.Group39)
				{
					foreach (DOCSegment doc in sg39.DOC)
					{
						if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterBillOfLading)
						{
							result = doc.DocumentMessageDetails.DocumentIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString InwardHawbHbl
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup39 sg39 in sG32.Group39)
				{
					foreach (DOCSegment doc in sg39.DOC)
					{
						if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.HouseWaybill)
						{
							result = doc.DocumentMessageDetails.DocumentIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString OutwardMawbObl
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup39 sg39 in sG32.Group39)
				{
					foreach (DOCSegment doc in sg39.DOC)
					{
						if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterAirWaybill)
						{
							result = doc.DocumentMessageDetails.DocumentIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString OutwardHawbHbl
		{
			get
			{
				var result = ZString.Empty;
				foreach (SegmentGroup39 sg39 in sG32.Group39)
				{
					foreach (DOCSegment doc in sg39.DOC)
					{
						if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.HouseBillOfLading)
						{
							result = doc.DocumentMessageDetails.DocumentIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				var result = ZString.Empty;
				foreach (FTXSegment ftx in sG32.FTX)
				{
					if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.GoodsItemDescription)
					{
						result = ftx.TextLiteral.FreeText1;
					}
				}
				return result;
			}
		}

		public ZDecimal UnitPrice
		{
			get { return GetConsignmentAmountFromSegment(SegmentGroup.sg35, MonetaryAmountTypeCodeQualifierList.UnitPrice); }
		}

		public ZString UnitPriceCurrency
		{
			get { return GetConsignmentCurrencyFromSegment(SegmentGroup.sg35, MonetaryAmountTypeCodeQualifierList.UnitPrice); }
		}

		public ZDecimal CustomsDutyPayable
		{
			get { return GetConsignmentAmountFromSegment(SegmentGroup.sg43, MonetaryAmountTypeCodeQualifierList.DutyAmount); }
		}

		public ZDecimal ExciseDutyPayable
		{
			get { return GetConsignmentAmountFromSegment(SegmentGroup.sg43, MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount); }
		}

		public ZDecimal OtherTaxPayable
		{
			get { return GetConsignmentAmountFromSegment(SegmentGroup.sg43, MonetaryAmountTypeCodeQualifierList.TaxAmount); }
		}

		public ZString CurrentLotNb
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in sG32.LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.Warehouse)
					{
						result = loc.LocationIdentification.LocationIdentifier;
					}
				}
				return result;
			}
		}

		public ZString PreviousLotNb
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in sG32.LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.BondedWarehouse)
					{
						result = loc.LocationIdentification.LocationIdentifier;
					}
				}
				return result;
			}
		}

		public ZDecimal CifFobLspValue
		{
			get { return GetConsignmentAmountFromSegment(SegmentGroup.sg35, MonetaryAmountTypeCodeQualifierList.FobValue); }
		}

		public ZDecimal LspAmount
		{
			get { return GetConsignmentAmountFromSegment(SegmentGroup.sg35, MonetaryAmountTypeCodeQualifierList.AssignedCustomsValue); }
		}

		public ZDecimal GstAmount
		{
			get { return GetConsignmentAmountFromSegment(SegmentGroup.sg43, MonetaryAmountTypeCodeQualifierList.GoodsAndServicesTax); }
		}

		public ZString CASCProductCode => string.Empty;

		public ZDecimal CASCProductQty
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (MEASegment mea in sG32.MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.UnitOfMeasureUsedForOrderedQuantities)
					{
						var amount = mea.ValueRange.Measure;
						if (!string.IsNullOrEmpty(amount))
						{
							result = Convert.ToDecimal(amount);
							CASCProductUQ = mea.ValueRange.MeasurementUnitCode;
						}
					}
				}

				return result;
			}
		}

		public ZString CASCProductUQ
		{
			get { return cascProductUQ; }
			set { cascProductUQ = value; }
		}
		ZString cascProductUQ;

		public ZString EngineNbChassisNb => string.Empty;

		#region OuterPack

		public ZInt OuterPackQty
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 sg33 in sG32.Group33)
				{
					foreach (PACSegment pac in sg33.PAC)
					{
						if (pac.PackagingDetails.PackagingLevelCode == PackagingLevelCodeList.Outer)
						{
							result = pac.PackageQuantity;
						}
					}
				}

				if (result != "")
				{
					return Convert.ToInt32(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZString OuterPackUQ
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 sg33 in sG32.Group33)
				{
					foreach (PACSegment pac in sg33.PAC)
					{
						if (pac.PackagingDetails.PackagingLevelCode == PackagingLevelCodeList.Outer)
						{
							result = pac.PackageType.PackageTypeDescriptionCode;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region InPack

		public ZInt InPackQty
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 sg33 in sG32.Group33)
				{
					foreach (PACSegment pac in sg33.PAC)
					{
						if (pac.PackagingDetails.PackagingLevelCode == PackagingLevelCodeList.Intermediate)
						{
							result = pac.PackageQuantity;
						}
					}
				}

				if (result != "")
				{
					return Convert.ToInt32(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZString InPackUQ
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 sg33 in sG32.Group33)
				{
					foreach (PACSegment pac in sg33.PAC)
					{
						if (pac.PackagingDetails.PackagingLevelCode == PackagingLevelCodeList.Intermediate)
						{
							result = pac.PackageType.PackageTypeDescriptionCode;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region InnerPack

		public ZInt InnerPackQty
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 sg33 in sG32.Group33)
				{
					foreach (PACSegment pac in sg33.PAC)
					{
						if (pac.PackagingDetails.PackagingLevelCode == PackagingLevelCodeList.Inner)
						{
							result = pac.PackageQuantity;
						}
					}
				}

				if (result != "")
				{
					return Convert.ToInt32(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZString InnerPackUQ
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 sg33 in sG32.Group33)
				{
					foreach (PACSegment pac in sg33.PAC)
					{
						if (pac.PackagingDetails.PackagingLevelCode == PackagingLevelCodeList.Inner)
						{
							result = pac.PackageType.PackageTypeDescriptionCode;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region InmostPack

		public ZInt InmostPackQty
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 sg33 in sG32.Group33)
				{
					foreach (PACSegment pac in sg33.PAC)
					{
						if (pac.PackagingDetails.PackagingLevelCode == PackagingLevelCodeList.ShipmentLevel)
						{
							result = pac.PackageQuantity;
						}
					}
				}

				if (result != "")
				{
					return Convert.ToInt32(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZString InmostPackUQ
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 sg33 in sG32.Group33)
				{
					foreach (PACSegment pac in sg33.PAC)
					{
						if (pac.PackagingDetails.PackagingLevelCode == PackagingLevelCodeList.ShipmentLevel)
						{
							result = pac.PackageType.PackageTypeDescriptionCode;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region ConsignmentLineValues

		public ZString[] ValueFields
		{
			get
			{
				if (fValueFields == null)
				{
					List<ZString> result = new List<ZString>();

					LineUnit1 = ZString.Empty;
					LineUnit2 = ZString.Empty;
					LineUnit3 = ZString.Empty;
					LineUnit4 = ZString.Empty;
					LineUnit5 = ZString.Empty;

					if (CifFobLspValue > 0)
					{
						result.Add(CifFobLspValue.ToString(2));
					}

					if (LspAmount > 0)
					{
						result.Add(LspAmount.ToString(2));
					}

					if (GstAmount > 0)
					{
						result.Add(GstAmount.ToString(2));
					}

					if (!DutQuantity.IsEmpty)
					{
						result.Add(DutQuantity);
						SetLineUnitValue(result.Count, DutQuantityUnit);
					}

					if (UnitPrice > 0)
					{
						result.Add(UnitPrice.ToString(4));
						SetLineUnitValue(result.Count, UnitPriceCurrency);
					}

					if (ExciseDutyPayable > 0)
					{
						result.Add(ExciseDutyPayable.ToString(2));
					}

					if (CustomsDutyPayable > 0)
					{
						result.Add(CustomsDutyPayable.ToString(2));
					}

					if (OtherTaxPayable > 0)
					{
						result.Add(OtherTaxPayable.ToString(2));
					}

					fValueFields = result;
				}

				return fValueFields.ToArray();
			}
		}
		List<ZString> fValueFields;

		protected void SetLineUnitValue(int valueCount, string unitValue)
		{
			if (valueCount == 1)
			{
				LineUnit1 = unitValue;
			}
			else if (valueCount == 2)
			{
				LineUnit2 = unitValue;
			}
			else if (valueCount == 3)
			{
				LineUnit3 = unitValue;
			}
			else if (valueCount == 4)
			{
				LineUnit4 = unitValue;
			}
			else if (valueCount == 5)
			{
				LineUnit5 = unitValue;
			}
		}

		public ZString LineValue1
		{
			get
			{
				return ValueFields.Length > 0 ? ValueFields[0] : ZString.Empty;
			}
		}

		public ZString LineUnit1
		{
			get { return fLineUnit1; }
			set { fLineUnit1 = value; }
		}
		ZString fLineUnit1;

		public ZString LineValue2
		{
			get
			{
				return ValueFields.Length > 1 ? ValueFields[1] : ZString.Empty;
			}
		}

		public ZString LineUnit2
		{
			get { return fLineUnit2; }
			set { fLineUnit2 = value; }
		}
		ZString fLineUnit2;

		public ZString LineValue3
		{
			get
			{
				return ValueFields.Length > 2 ? ValueFields[2] : ZString.Empty;
			}
		}

		public ZString LineUnit3
		{
			get { return fLineUnit3; }
			set { fLineUnit3 = value; }
		}
		ZString fLineUnit3;

		public ZString LineValue4
		{
			get
			{
				return ValueFields.Length > 3 ? ValueFields[3] : ZString.Empty;
			}
		}

		public ZString LineUnit4
		{
			get { return fLineUnit4; }
			set { fLineUnit4 = value; }
		}
		ZString fLineUnit4;

		public ZString LineValue5
		{
			get
			{
				return ValueFields.Length > 4 ? ValueFields[4] : ZString.Empty;
			}
		}

		public ZString LineUnit5
		{
			get { return fLineUnit5; }
			set { fLineUnit5 = value; }
		}
		ZString fLineUnit5;

		public ZString LineValue6
		{
			get
			{
				return ValueFields.Length > 5 ? ValueFields[5] : ZString.Empty;
			}
		}

		public ZString LineValue7
		{
			get
			{
				return ValueFields.Length > 6 ? ValueFields[6] : ZString.Empty;
			}
		}

		public ZString LineValue8
		{
			get
			{
				return ValueFields.Length > 7 ? ValueFields[7] : ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region Segment Methods

		ZString GetManufacturerName()
		{
			var itemInvoiceNumber = ItemInvoiceNumber;

			if (!itemInvoiceNumber.IsEmpty)
			{
				foreach (SegmentGroup11 sg11 in invoices)
				{
					foreach (SegmentGroup15 sg15 in sg11.Group15)
					{
						foreach (SegmentGroup16 sg16 in sg15.Group16)
						{
							foreach (DOCSegment doc in sg16.DOC)
							{
								if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.CommercialInvoice
									&& doc.DocumentMessageDetails.DocumentIdentifier.Equals(itemInvoiceNumber, StringComparison.OrdinalIgnoreCase))
								{
									foreach (NADSegment nad in sg15.NAD)
									{
										if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Supplier)
										{
											return nad.PartyName.PartyName1 + nad.PartyName.PartyName2;
										}
									}
								}
							}
						}
					}
				}
			}

			return ZString.Empty;
		}

		enum SegmentGroup { sg35, sg43 }

		ZDecimal GetConsignmentAmountFromSegment(SegmentGroup group, MonetaryAmountTypeCodeQualifierList monetaryQualifier)
		{
			var result = ZDecimal.Zero;
			MOASegment relevantMOA = null;

			if (group == SegmentGroup.sg35)
			{
				foreach (SegmentGroup35 sg35 in sG32.Group35)
				{
					foreach (MOASegment moa in sg35.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == monetaryQualifier)
						{
							relevantMOA = moa;
							break;
						}
					}
				}
			}
			else if (group == SegmentGroup.sg43)
			{
				foreach (SegmentGroup43 sg43 in sG32.Group43)
				{
					foreach (MOASegment moa in sg43.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == monetaryQualifier)
						{
							relevantMOA = moa;
							break;
						}
					}
				}
			}

			if (relevantMOA != null)
			{
				var amount = relevantMOA.MonetaryAmount.MonetaryAmount;
				if (!string.IsNullOrEmpty(amount))
				{
					result = Convert.ToDecimal(amount);
				}
			}

			return result;
		}

		ZString GetConsignmentCurrencyFromSegment(SegmentGroup group, MonetaryAmountTypeCodeQualifierList monetaryQualifier)
		{
			var result = ZString.Empty;

			if (group == SegmentGroup.sg35)
			{
				foreach (SegmentGroup35 sg35 in sG32.Group35)
				{
					foreach (MOASegment moa in sg35.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == monetaryQualifier)
						{
							if (!string.IsNullOrEmpty(moa.MonetaryAmount.CurrencyIdentificationCode))
							{
								result = moa.MonetaryAmount.CurrencyIdentificationCode;
							}
							else
							{
								result = Core.Constants.CurrencyCodes.Singapore;
							}

							break;
						}
					}
				}
			}
			else if (group == SegmentGroup.sg43)
			{
				foreach (SegmentGroup43 sg43 in sG32.Group43)
				{
					foreach (MOASegment moa in sg43.MOA)
					{
						if (!string.IsNullOrEmpty(moa.MonetaryAmount.CurrencyIdentificationCode))
						{
							if (!string.IsNullOrEmpty(moa.MonetaryAmount.CurrencyIdentificationCode))
							{
								result = moa.MonetaryAmount.CurrencyIdentificationCode;
							}
							else
							{
								result = Core.Constants.CurrencyCodes.Singapore;
							}

							break;
						}
					}
				}
			}

			return result;
		}

		public ICASCProductCode[] CASCProductCodes
		{
			get
			{
				if (productCodes == null)
				{
					var result = new List<CUSPMTProductCode>();

					var productCount = ZInt.Zero;

					foreach (SegmentGroup37 sg37 in sG32.Group37)
					{
						foreach (RFFSegment rff in sg37.RFF)
						{
							if (rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber)
							{
								productCount++;
								result.Add(new CUSPMTProductCode
								{
									SequenceNumber = productCount.ToString("00").PadLeft(5, ' '),
									ProductCode = rff.Reference.ReferenceIdentifier,
									ProductQuantity = CASCQtyAndUQForThisProduct(productCount)
								});
							}
						}
					}

					productCodes = result.ToArray();
				}

				return productCodes;
			}
		}
		ICASCProductCode[] productCodes;

		ZString CASCQtyAndUQForThisProduct(int sequence)
		{
			var result = ZString.Empty;
			var meaCount = ZInt.Zero;
			foreach (MEASegment mea in sG32.MEA)
			{
				if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.UnitOfMeasureUsedForOrderedQuantities)
				{
					meaCount++;
					if (meaCount == sequence)
					{
						result = mea.ValueRange.Measure + "  " + mea.ValueRange.MeasurementUnitCode;
						break;
					}
				}
			}

			return result;
		}

		public IEngineOrChassisNumber[] EngineOrChassisNumbers
		{
			get
			{
				if (engineOrChassisNumbers == null)
				{
					var result = new List<CUSPMTEngineOrChassisNumber>();

					if (HSCode.StartsWith("87"))
					{
						var sequence = ZInt.Zero;

						foreach (SegmentGroup37 sg37 in sG32.Group37)
						{
							foreach (GINSegment gin in sg37.GIN)
							{
								if (gin.ObjectIdentificationCodeQualifier == ObjectIdentificationCodeQualifierList.ValueListSubset)
								{
									sequence++;
									result.Add(new CUSPMTEngineOrChassisNumber
									{
										SequenceNumber = sequence.ToString("00").PadLeft(5, ' '),
										Number = gin.IdentityNumberRange1.ObjectIdentifier1 + " / " + gin.IdentityNumberRange2.ObjectIdentifier1
									});
								}
							}
						}
					}

					engineOrChassisNumbers = result.ToArray();
				}

				return engineOrChassisNumbers;
			}
		}
		IEngineOrChassisNumber[] engineOrChassisNumbers;

		#endregion
	}

	#endregion

	#region CUSPMTProductCode

	public class CUSPMTProductCode : ICASCProductCode
	{
		public ZString SequenceNumber { get; set; }

		public ZString ProductCode { get; set; }

		public ZString ProductQuantity { get; set; }
	}

	#endregion

	#region CUSPMTEngineOrChassisNumber

	public class CUSPMTEngineOrChassisNumber : IEngineOrChassisNumber
	{
		public ZString SequenceNumber { get; set; }

		public ZString Number { get; set; }
	}

	#endregion

	#region CUSPMTPrintPermitConditions

	public class CuspmtPrintPermitConditions : ITN41PermitConditions
	{
		public CuspmtPrintPermitConditions(ZString condition)
		{
			this.condition = condition;
		}
		readonly ZString condition;

		#region ITN41PermitConditions Members

		public ZString Condition
		{
			get { return condition; }
		}

		#endregion
	}

	#endregion

	#region CUSPMTPrintPermitContainers

	public class CUSPMTPrintPermitContainers : IPrintPermitContainers
	{
		public CUSPMTPrintPermitContainers(int sequenceNumber1, ZString containerIdentifier1)
		{
			this.SequenceNumber1 = sequenceNumber1;
			this.ContainerIdentifier1 = containerIdentifier1;
		}

		public int SequenceNumber1;
		public ZString ContainerIdentifier1;

		public int SequenceNumber2;
		public ZString ContainerIdentifier2;

		#region IPrintPermitContainers Members

		ZString IPrintPermitContainers.ContainerSequenceNumber1
		{
			get { return SequenceNumber1 > 0 ? SequenceNumber1.ToString("00").PadLeft(5, ' ') : ""; }
		}

		ZString IPrintPermitContainers.ContainerIdentifier1
		{
			get { return ContainerIdentifier1; }
		}

		ZString IPrintPermitContainers.ContainerSequenceNumber2
		{
			get { return SequenceNumber2 > 0 ? SequenceNumber2.ToString("00") : ""; }
		}

		ZString IPrintPermitContainers.ContainerIdentifier2
		{
			get { return ContainerIdentifier2; }
		}

		#endregion
	}

	#endregion
}
