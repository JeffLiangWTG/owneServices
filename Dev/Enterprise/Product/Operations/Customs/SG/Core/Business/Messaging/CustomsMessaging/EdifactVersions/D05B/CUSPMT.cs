using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D05B.Elements;
using Enterprise.Edifact.D05B.Messages.CUSPMT;
using Enterprise.Edifact.D05B.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	public class CUSPMT : CUSPMTMessage, IPrintPermitTN4
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup1 gR1 in Group1)
				{
					foreach (RFFSegment rff in gR1.RFF)
					{
						if (rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.CustomsDeclarationNumber)
						{
							result = rff.Reference.ReferenceIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString UniqueRef
		{
			get { return BGM[0].DocumentMessageIdentification.DocumentIdentifier; }
		}

		public ZString MessageType
		{
			get
			{
				ZString newValue;
				CommonAccessReferenceCodeList messageList = new CommonAccessReferenceCodeList();
				newValue = UNH[0].CommonAccessReference.Substring(0, 3) + "PMT";
				return messageList.GetDescriptionFromCode(newValue).ToUpper();
			}
		}

		public ZString DeclarationType
		{
			get
			{
				var declarationType = new DeclarationTypeCodeList();
				var typeCode = BGM[0].DocumentMessageName.DocumentName;
				var result = ZString.Empty;

				if (!string.IsNullOrWhiteSpace(typeCode) && typeCode != "BKT")
				{
					result = declarationType.GetDescriptionFromCode(typeCode).ToUpper();
				}
				else
				{
					var reference = UNH[0].CommonAccessReference.Substring(0, 3);

					switch (reference)
					{
						case MessageTypeCodeList.Codes.INP:
							{
								result = declarationType.GetDescriptionFromCode(DeclarationTypeCodeList.Codes.BKN).ToUpper();
								break;
							}

						case MessageTypeCodeList.Codes.OUT:
							{
								result = declarationType.GetDescriptionFromCode(DeclarationTypeCodeList.Codes.BKO).ToUpper();
								break;
							}

						case MessageTypeCodeList.Codes.IPT:
							{
								result = declarationType.GetDescriptionFromCode(DeclarationTypeCodeList.Codes.BKP).ToUpper();
								break;
							}
					}
				}
				return result;
			}
		}

		public ZString Importer
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					foreach (NADSegment nad in gR6.NAD)
					{
						if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Importer)
						{
							result = nad.PartyName.PartyName1 + nad.PartyName.PartyName2 + nad.PartyName.PartyName3 + nad.PartyName.PartyName4 + "\n" + nad.PartyIdentificationDetails.PartyIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString Exporter
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					foreach (NADSegment nad in gR6.NAD)
					{
						if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Exporter)
						{
							result = nad.PartyName.PartyName1 + nad.PartyName.PartyName2 + nad.PartyName.PartyName3 + nad.PartyName.PartyName4 + "\n" + nad.PartyIdentificationDetails.PartyIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString HandlingAgent
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					foreach (NADSegment nad in gR6.NAD)
					{
						if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.TransitPrincipalsAgentRepresentative)
						{
							result = nad.PartyName.PartyName1 + nad.PartyName.PartyName2 + nad.PartyName.PartyName3 + nad.PartyName.PartyName4 + "\n" + nad.PartyIdentificationDetails.PartyIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString PortOfLoading
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PlacePortOfLoading)
					{
						if (loc.LocationIdentification.LocationNameCode != ZString.Empty)
						{
							var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, loc.LocationIdentification.LocationNameCode, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
							if (cusCode != null)
							{
								result = cusCode.ZZD_Description.ToUpper();
							}
						}
						else
						{
							result = ZString.Empty;
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
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.NextPortOfCall)
					{
						if (loc.LocationIdentification.LocationNameCode != ZString.Empty)
						{
							var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, loc.LocationIdentification.LocationNameCode, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
							if (cusCode != null)
							{
								result = cusCode.ZZD_Description.ToUpper();
							}
						}
						else
						{
							result = ZString.Empty;
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
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PortOfDischarge)
					{
						if (loc.LocationIdentification.LocationNameCode != ZString.Empty)
						{
							var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, loc.LocationIdentification.LocationNameCode, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
							if (cusCode != null)
							{
								result = cusCode.ZZD_Description.ToUpper();
							}
						}
						else
						{
							result = ZString.Empty;
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
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PlaceOfUltimateDestinationOfConveyance)
					{
						if (loc.LocationIdentification.LocationNameCode != ZString.Empty)
						{
							var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, loc.LocationIdentification.LocationNameCode, Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
							if (cusCode != null)
							{
								result = cusCode.ZZD_Description.ToUpper();
							}
						}
						else
						{
							result = ZString.Empty;
						}
					}
				}
				return result;
			}
		}

		public ZString CountryOfFinalDest
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.CountryOfUltimateDestination)
					{
						if (loc.LocationIdentification.LocationNameCode != ZString.Empty)
						{
							RefCountry contry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, loc.LocationIdentification.LocationNameCode));
							result = contry.RN_DescMultilingual.GetUnresolvedString().ToUpper();
						}
						else
						{
							result = ZString.Empty;
						}
					}
				}
				return result;
			}
		}

		public ZString InwardCarrierAgent
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					foreach (NADSegment nad in gR6.NAD)
					{
						if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.CarriersAgent)
						{
							result = nad.PartyName.PartyName1 + nad.PartyName.PartyName2 + nad.PartyName.PartyName3;
						}
					}
				}
				return result;
			}
		}

		public ZString OutwardCarrierAgent
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					foreach (NADSegment nad in gR6.NAD)
					{
						if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Carrier)
						{
							result = nad.PartyName.PartyName1 + nad.PartyName.PartyName2 + nad.PartyName.PartyName3;
						}
					}
				}
				return result;
			}
		}

		public ZString PlaceOfRelease
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PlacePortOfDischarge)
					{
						result = SingaporePlace(loc);
					}
				}

				return result;
			}
		}

		public ZDate ValidityPeriodFrom
		{
			get
			{
				foreach (SegmentGroup1 group1 in Group1)
				{
					foreach (RFFSegment rFF in group1.RFF)
					{
						if (rFF.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.CustomsDeclarationNumber)
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
						if (rFF.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.CustomsDeclarationNumber)
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
				ZString result = ZString.Empty;
				foreach (MEASegment mea in MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.DimensionsTotalWeight)
					{
						result = mea.ValueRange.Measure + "/" + mea.ValueRange.MeasurementUnitCode;
					}
				}
				return result;
			}
		}

		public ZString TotalOuterPack
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (MEASegment mea in MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.ExternalDimension)
					{
						result = mea.ValueRange.Measure + "/" + mea.ValueRange.MeasurementUnitCode;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalCustomsDUTPayable
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup49 gR49 in Group49)
				{
					if (gR49.TAX[0].DutyOrTaxOrFeeFunctionCodeQualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty)
					{
						foreach (MOASegment moa in gR49.MOA)
						{
							if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.DutyAmount)
							{
								result = moa.MonetaryAmount.MonetaryAmount;
							}
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZDecimal TotalOtherTaxPayable
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal TotalExciseDUTPayable
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup49 gR49 in Group49)
				{
					if (gR49.TAX[0].DutyOrTaxOrFeeFunctionCodeQualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.CustomsDuty)
					{
						foreach (MOASegment moa in gR49.MOA)
						{
							if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount)
							{
								result = moa.MonetaryAmount.MonetaryAmount;
							}
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZDecimal TotalGstAmount
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup49 gR49 in Group49)
				{
					if (gR49.TAX[0].DutyOrTaxOrFeeFunctionCodeQualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.Tax)
					{
						foreach (MOASegment moa in gR49.MOA)
						{
							if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.TaxAmount)
							{
								result = moa.MonetaryAmount.MonetaryAmount;
							}
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZDecimal TotalAmountPayable
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup49 gR49 in Group49)
				{
					if (gR49.TAX[0].DutyOrTaxOrFeeFunctionCodeQualifier == DutyOrTaxOrFeeFunctionCodeQualifierList.TotalOfAllDutiesTaxesAndFeesCustomsItem)
					{
						foreach (MOASegment moa in gR49.MOA)
						{
							if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.AmountDueAmountPayable)
							{
								result = moa.MonetaryAmount.MonetaryAmount;
							}
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZString CargoPackingType
		{
			get
			{
				ZString cargoPackingDescription;

				if (CST[0].CustomsIdentityCodes1.CustomsGoodsIdentifier == CargoPackingTypeCodeList.Codes.PackingType4)
				{
					cargoPackingDescription = "PACKED TO BULK";
				}
				else
				{
					CargoPackingTypeCodeList cargoList = new CargoPackingTypeCodeList();
					cargoPackingDescription = cargoList.GetDescriptionFromCode(CST[0].CustomsIdentityCodes1.CustomsGoodsIdentifier).ToUpper();
				}
				return cargoPackingDescription;
			}
		}

		public ZString InVesName
		{
			get
			{
				ZString result = ZString.Empty;
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup4 gR4 in Group4)
				{
					foreach (TDTSegment tdt in gR4.TDT)
					{
						if (tdt.TransportStageCodeQualifier == TransportStageCodeQualifierList.AtArrival)
						{
							result = tdt.TransportIdentification.TransportMeansIdentificationNameIdentifier;
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup5 gR5 in Group5)
				{
					foreach (DOCSegment doc in gR5.DOC)
					{
						if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterBillOfLading)
						{
							result = doc.DocumentMessageDetails.DocumentIdentifier;
						}
					}
				}
				if (result == ZString.Empty)
				{
					foreach (SegmentGroup30 gR30 in Group30)
					{
						foreach (SegmentGroup37 gR37 in gR30.Group37)
						{
							foreach (DOCSegment doc in gR37.DOC)
							{
								if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterAirWaybill)
								{
									result = doc.DocumentMessageDetails.DocumentIdentifier;
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
				ZString result = ZString.Empty;
				foreach (DTMSegment dtm in DTM)
				{
					if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansArrivalDateTimeActual)
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

		public ZString OutVesName
		{
			get
			{
				ZString result = ZString.Empty;
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

		public ZString OutVesLocation
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.ScheduledBerth)
					{
						result = loc.LocationIdentification.LocationNameCode;
					}
				}
				return result;
			}
		}

		public ZString OutVoyageFlightNumber
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup4 gR4 in Group4)
				{
					foreach (TDTSegment tdt in gR4.TDT)
					{
						if (tdt.TransportStageCodeQualifier == TransportStageCodeQualifierList.AtDeparture)
						{
							result = tdt.TransportIdentification.TransportMeansIdentificationNameIdentifier;
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup4 gR4 in Group4)
				{
					foreach (TDTSegment tdt in gR4.TDT)
					{
						if (tdt.TransportStageCodeQualifier == TransportStageCodeQualifierList.InlandWaterwayTransport)
						{
							result = tdt.TransportIdentification.TransportMeansIdentificationName;
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup5 gR5 in Group5)
				{
					foreach (DOCSegment doc in gR5.DOC)
					{
						if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterAirWaybill)
						{
							result = doc.DocumentMessageDetails.DocumentIdentifier;
						}
					}
				}
				if (result == ZString.Empty)
				{
					foreach (SegmentGroup30 gR30 in Group30)
					{
						foreach (SegmentGroup37 gR37 in gR30.Group37)
						{
							foreach (DOCSegment doc in gR37.DOC)
							{
								if (doc.DocumentMessageName.DocumentNameCode == DocumentNameCodeList.MasterAirWaybill)
								{
									result = doc.DocumentMessageDetails.DocumentIdentifier;
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
				ZString result = ZString.Empty;
				foreach (DTMSegment dtm in DTM)
				{
					if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.TransportMeansDepartureDateTime)
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup1 gR1 in Group1)
				{
					foreach (RFFSegment rff in gR1.RFF)
					{
						if (rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.DocumentNumber)
						{
							result = rff.Reference.ReferenceIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString CertificateNo
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup1 gR1 in Group1)
				{
					foreach (RFFSegment rff in gR1.RFF)
					{
						if (rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.OriginalCertificateNumber)
						{
							result = rff.Reference.ReferenceIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString PlaceOfReceipt
		{
			get
			{
				ZString result = "";
				foreach (LOCSegment loc in LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.PlaceOfReceipt)
					{
						result = SingaporePlace(loc);
					}
				}

				return result;
			}
		}

		public ZString CustomsProcedureCodes
		{
			get { return ZString.Empty; }
		}

		public IPrintPermitConsignment[] ConsignmentDetails
		{
			get
			{
				CUSPMTPrintPermitConsignment[] result = new CUSPMTPrintPermitConsignment[Group30.Count];
				for (int i = 0; i < Group30.Count; i++)
				{
					result.SetValue(new CUSPMTPrintPermitConsignment(Group30[i]), i);
				}
				return result;
			}
		}

		public ZString ManufacturerName
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup10 gR10 in Group10)
				{
					foreach (SegmentGroup14 gR14 in gR10.Group14)
					{
						foreach (NADSegment nad in gR14.NAD)
						{
							if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Supplier)
							{
								result = nad.PartyName.PartyName1 + " " + nad.PartyName.PartyName2 + " " + nad.PartyName.PartyName3 + "\n";
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
				IPrintPermitContainers[] result = new IPrintPermitContainers[(EQD.Count + 1) / 2];
				CUSPMTPrintPermitContainers currentContainer = null;

				for (int i = 0; i < EQD.Count; i++)
				{
					if ((i % 2) == 0)
					{
						currentContainer = new CUSPMTPrintPermitContainers(i + 1, GetContainerIdentifier(EQD[i], i));
						result[(i / 2)] = currentContainer;
					}
					else
					{
						currentContainer.SequenceNumber2 = i + 1;
						currentContainer.ContainerIdentifier2 = GetContainerIdentifier(EQD[i], i);
					}
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
			string weight = containerDetails.SubstringSafe(5, 3);

			return eqd.EquipmentIdentification.EquipmentIdentifier + " " + type + " " + size + " " + weight + " " + sealNumber;
		}

		public ZString NameOfCompany
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					foreach (NADSegment nad in gR6.NAD)
					{
						if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.DeclarantsAgentRepresentative)
						{
							result = nad.PartyName.PartyName1 + nad.PartyName.PartyName2 + nad.PartyName.PartyName3;
						}
					}
				}
				return result;
			}
		}

		public ZString EntityIdentOfCompany
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					foreach (NADSegment nad in gR6.NAD)
					{
						if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.DeclarantsAgentRepresentative)
						{
							result = nad.PartyIdentificationDetails.PartyIdentifier;
						}
					}
				}
				return result;
			}
		}

		public ZString DeclarantName
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					foreach (NADSegment nad in gR6.NAD)
					{
						if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Declarant)
						{
							result = nad.NameAndAddress.NameAndAddressDescription1 + nad.NameAndAddress.NameAndAddressDescription2 + nad.NameAndAddress.NameAndAddressDescription3;
						}
					}
				}
				return result;
			}
		}

		public ZString DeclarantCode
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					foreach (NADSegment nad in gR6.NAD)
					{
						if (nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Declarant)
						{
							foreach (CTASegment cta in gR6.CTA)
							{
								if (cta.ContactFunctionCode == ContactFunctionCodeList.InformationContact)
								{
									result = cta.DepartmentOrEmployeeDetails.DepartmentOrEmployeeName;
								}
							}
						}
					}
				}
				return result;
			}
		}

		public ZString TelNb
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup6 gR6 in Group6)
				{
					if (gR6.NAD[0].PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Declarant)
					{
						foreach (COMSegment com in gR6.COM)
						{
							if (com.CommunicationContact.CommunicationAddressCodeQualifier == CommunicationAddressCodeQualifierList.Telephone)
							{
								result = com.CommunicationContact.CommunicationAddressIdentifier;
							}
						}
					}
				}
				return result;
			}
		}

		public ICConditions[] CAConditions
		{
			get
			{
				int total = 0;
				CUSPMTPrintPermitConditions[] result = null;
				foreach (SegmentGroup1 gR1 in Group1)
				{
					foreach (FTXSegment ftx in gR1.FTX)
					{
						if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.RegulatoryInformation)
						{
							total++;
						}
					}
				}
				if (total > 0)
				{
					result = new CUSPMTPrintPermitConditions[total];
					foreach (SegmentGroup1 gR1 in Group1)
					{
						for (int i = 0; i < gR1.FTX.Count; i++)
						{
							if (gR1.FTX[i].TextSubjectCodeQualifier == TextSubjectCodeQualifierList.RegulatoryInformation)
							{
								result.SetValue(new CUSPMTPrintPermitConditions(gR1.FTX[i]), i);
							}
						}
					}
				}
				return result;
			}
		}

		public ICConditions[] CustomsConditions
		{
			get
			{
				int total = 0;
				CUSPMTPrintPermitConditions[] result = null;
				foreach (SegmentGroup1 gR1 in Group1)
				{
					foreach (FTXSegment ftx in gR1.FTX)
					{
						if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.CustomsClearanceInstructions)
						{
							total++;
						}
					}
				}
				if (total > 0)
				{
					result = new CUSPMTPrintPermitConditions[total];
					foreach (SegmentGroup1 gR1 in Group1)
					{
						for (int i = 0; i < gR1.FTX.Count; i++)
						{
							if (gR1.FTX[i].TextSubjectCodeQualifier == TextSubjectCodeQualifierList.CustomsClearanceInstructions)
							{
								result.SetValue(new CUSPMTPrintPermitConditions(gR1.FTX[i]), i);
							}
						}
					}
				}
				return result;
			}
		}

		public ZDate AmendDate
		{
			get
			{
				if (AmendFields.Length != 0)
				{
					foreach (SegmentGroup1 gR1 in Group1)
					{
						foreach (DTMSegment dtm in gR1.DTM)
						{
							if (dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.AuthorizationDate)
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
				foreach (SegmentGroup1 gR1 in Group1)
				{
					foreach (FTXSegment ftx in gR1.FTX)
					{
						if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.CustomsDeclarationInformation)
						{
							resultLine = new UpdateSummaryCode().GetFieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText1);
							result.Add(resultLine);
							if (ftx.TextLiteral.FreeText2.Length > 0)
							{
								resultLine = new UpdateSummaryCode().GetFieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText2);
								result.Add(resultLine);
								if (ftx.TextLiteral.FreeText3.Length > 0)
								{
									resultLine = new UpdateSummaryCode().GetFieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText3);
									result.Add(resultLine);
									if (ftx.TextLiteral.FreeText4.Length > 0)
									{
										resultLine = new UpdateSummaryCode().GetFieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText4);
										result.Add(resultLine);
										if (ftx.TextLiteral.FreeText5.Length > 0)
										{
											resultLine = new UpdateSummaryCode().GetFieldDescriptionFromSummaryCode(ftx.TextLiteral.FreeText5);
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

		public ZBool HideMawbLine
		{
			get { return false; }
		}

		public ZBool HideHawbLine
		{
			get { return false; }
		}

		public ZBool HideCifFobValue
		{
			get { return false; }
		}

		public ZBool HideLspValue
		{
			get { return false; }
		}

		public ZBool HideGstValue
		{
			get { return false; }
		}

		public ZBool HideDutQtyWtVolValue
		{
			get { return false; }
		}

		public ZBool HideUnitPriceValue
		{
			get { return false; }
		}

		public ZBool HideExciseValue
		{
			get { return false; }
		}

		public ZBool HideDutyValue
		{
			get { return false; }
		}

		public ZBool HideOtherTaxValue
		{
			get { return false; }
		}

		public ZBool HideManufacturerName
		{
			get { return false; }
		}

		#endregion

		string SingaporePlace(LOCSegment lOC)
		{
			string result;
			var location = SGPlacesRefCusCodeList.GetCurrentOrMatchingPlace(Factory, lOC.LocationIdentification.LocationNameCode);
			if (location.IsNonSystemNonLicenced())
			{
				result = lOC.LocationIdentification.LocationName + "\n" + lOC.LocationIdentification.LocationNameCode;
			}
			else
			{
				if (location != null && location.ZZD_IsSystem)
				{
					result = location.ZZD_Description.SubstringSafe(0, 105) + "\n" + lOC.LocationIdentification.LocationNameCode;
				}
				else
				{
					result = lOC.LocationIdentification.LocationName + "\n" + lOC.LocationIdentification.LocationNameCode;
				}
			}
			return result;
		}
	}

	#region CUSPMTPrintPermitConsignment

	public class CUSPMTPrintPermitConsignment : IPrintPermitConsignment
	{
		public CUSPMTPrintPermitConsignment(SegmentGroup30 gr30)
		{
			gR30 = gr30;
		}
		readonly SegmentGroup30 gR30;

		#region IPrintPermitConsignment Members

		public ZString SerialNb
		{
			get { return gR30.CST[0].GoodsItemNumber; }
		}

		public ZString HSCode
		{
			get { return gR30.CST[0].CustomsIdentityCodes1.CustomsGoodsIdentifier; }
		}

		public ZString BrandName
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (FTXSegment ftx in gR30.FTX)
				{
					if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.ProductInformation)
					{
						result = ftx.TextLiteral.FreeText1;
					}
				}
				return result;
			}
		}

		public ZString ManufacturerName => ZString.Empty;

		public ZString HSQuantity
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (MEASegment mea in gR30.MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.CustomsLineItemMeasurement)
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
				ZString result = ZString.Empty;
				foreach (MEASegment mea in gR30.MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.CustomsLineItemMeasurement)
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup31 gR31 in gR30.Group31)
				{
					foreach (SegmentGroup32 gR32 in gR31.Group32)
					{
						if (gR32.PCI[0].MarkingInstructionsCode == MarkingInstructionsCodeList.LegalRequirements)
						{
							result = gR32.PCI[0].MarksLabels.ShippingMarksDescription1;
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
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in gR30.LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.CountryOfOrigin)
					{
						result = loc.LocationIdentification.LocationNameCode;
					}
				}
				return result;
			}
		}

		public ZString Model
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (FTXSegment ftx in gR30.FTX)
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
				ZString result = ZString.Empty;
				foreach (MEASegment mea in gR30.MEA)
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
				ZString result = ZString.Empty;
				foreach (MEASegment mea in gR30.MEA)
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup37 gR37 in gR30.Group37)
				{
					foreach (DOCSegment doc in gR37.DOC)
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup37 gR37 in gR30.Group37)
				{
					foreach (DOCSegment doc in gR37.DOC)
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup37 gR37 in gR30.Group37)
				{
					foreach (DOCSegment doc in gR37.DOC)
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
				ZString result = ZString.Empty;
				foreach (SegmentGroup37 gR37 in gR30.Group37)
				{
					foreach (DOCSegment doc in gR37.DOC)
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
				ZString result = ZString.Empty;
				foreach (FTXSegment ftx in gR30.FTX)
				{
					if (ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.GoodsDescription)
					{
						result = ftx.TextLiteral.FreeText1 + " " + ftx.TextLiteral.FreeText2 + " " + ftx.TextLiteral.FreeText3 + " " + ftx.TextLiteral.FreeText4 + " " + ftx.TextLiteral.FreeText5;
					}
				}
				return result;
			}
		}

		public ZDecimal UnitPrice
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 gR33 in gR30.Group33)
				{
					foreach (MOASegment moa in gR33.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.UnitPrice)
						{
							result = moa.MonetaryAmount.MonetaryAmount;
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZString UnitPriceCurrency
		{
			get { return Core.Constants.CurrencyCodes.Singapore; }  // This property is not used here, required for TN4.1 printing
		}

		public ZDecimal CustomsDutyPayable
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup41 gR41 in gR30.Group41)
				{
					foreach (MOASegment moa in gR41.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.DutyAmount)
						{
							result = moa.MonetaryAmount.MonetaryAmount;
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZDecimal ExciseDutyPayable
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup41 gR41 in gR30.Group41)
				{
					foreach (MOASegment moa in gR41.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.DutyTaxOrFeeAmount)
						{
							result = moa.MonetaryAmount.MonetaryAmount;
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZDecimal OtherTaxPayable
		{
			get { return ZDecimal.Zero; } // new field for TN4.1 - not required here
		}

		public ZString CurrentLotNb
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (LOCSegment loc in gR30.LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.Warehouse)
					{
						result = loc.LocationIdentification.LocationNameCode;
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
				foreach (LOCSegment loc in gR30.LOC)
				{
					if (loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.CargoFacilityLocation)
					{
						result = loc.LocationIdentification.LocationNameCode;
					}
				}
				return result;
			}
		}

		public ZDecimal CifFobLspValue
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 gR33 in gR30.Group33)
				{
					foreach (MOASegment moa in gR33.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.FobValue)
						{
							result = moa.MonetaryAmount.MonetaryAmount;
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZDecimal LspAmount
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup33 gR33 in gR30.Group33)
				{
					foreach (MOASegment moa in gR33.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.AssignedCustomsValue)
						{
							result = moa.MonetaryAmount.MonetaryAmount;
						}
					}
				}

				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZDecimal GstAmount
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup41 gR41 in gR30.Group41)
				{
					foreach (MOASegment moa in gR41.MOA)
					{
						if (moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == MonetaryAmountTypeCodeQualifierList.TaxAmount)
						{
							result = moa.MonetaryAmount.MonetaryAmount;
						}
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZString CASCProductCode
		{
			get
			{
				ZString result = ZString.Empty;
				var productCount = ZInt.Zero;
				foreach (SegmentGroup35 gR35 in gR30.Group35)
				{
					foreach (RFFSegment rff in gR35.RFF)
					{
						if (rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.GovernmentAgencyReferenceNumber)
						{
							productCount++;
							result += productCount.ToString().PadRight(10, ' ') + rff.Reference.ReferenceIdentifier.PadRight(50, ' ') + CASCQtyAndUQForThisProduct(productCount) + "\r\n";
						}
					}
				}
				return result;
			}
		}

		ZString CASCQtyAndUQForThisProduct(int sequence)
		{
			var result = ZString.Empty;
			var meaCount = ZInt.Zero;
			foreach (MEASegment mea in gR30.MEA)
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

		public ZDecimal CASCProductQty
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (MEASegment mea in gR30.MEA)
				{
					if (mea.MeasurementPurposeCodeQualifier == MeasurementPurposeCodeQualifierList.UnitOfMeasureUsedForOrderedQuantities)
					{
						result = mea.ValueRange.Measure;
					}
				}
				if (result != "")
				{
					return Convert.ToDecimal(result);
				}
				else
				{
					return 0;
				}
			}
		}

		public ZString CASCProductUQ
		{
			get { return ZString.Empty; }
		}

		public ZString EngineNbChassisNb
		{
			get
			{
				var result = ZString.Empty;
				var sequence = ZInt.Zero;
				bool isVehicle = HSCode.StartsWith("87");
				foreach (SegmentGroup35 gR35 in gR30.Group35)
				{
					if (isVehicle)
					{
						foreach (GINSegment gin in gR35.GIN)
						{
							if (gin.ObjectIdentificationCodeQualifier == ObjectIdentificationCodeQualifierList.ValueListSubset)
							{
								sequence++;
								var paddingGap = sequence < 10 ? "         " : "        ";
								result += sequence.ToString().PadLeft(3, ' ') + paddingGap + gin.IdentityNumberRange1.ObjectIdentifier1 + " / " + gin.IdentityNumberRange2.ObjectIdentifier1 + "\r\n";
							}
						}
					}
				}

				return result;
			}
		}

		#region OuterPack

		public ZInt OuterPackQty
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (SegmentGroup31 g31 in gR30.Group31)
				{
					foreach (PACSegment pac in g31.PAC)
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
				foreach (SegmentGroup31 g31 in gR30.Group31)
				{
					foreach (PACSegment pac in g31.PAC)
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
				foreach (SegmentGroup31 g31 in gR30.Group31)
				{
					foreach (PACSegment pac in g31.PAC)
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
				foreach (SegmentGroup31 g31 in gR30.Group31)
				{
					foreach (PACSegment pac in g31.PAC)
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
				foreach (SegmentGroup31 g31 in gR30.Group31)
				{
					foreach (PACSegment pac in g31.PAC)
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
				foreach (SegmentGroup31 g31 in gR30.Group31)
				{
					foreach (PACSegment pac in g31.PAC)
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
				foreach (SegmentGroup31 g31 in gR30.Group31)
				{
					foreach (PACSegment pac in g31.PAC)
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
				foreach (SegmentGroup31 g31 in gR30.Group31)
				{
					foreach (PACSegment pac in g31.PAC)
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

		#region For TradeNet 4.1 Not used here
		public ZString LineValue1
		{
			get { return ZString.Empty; }
		}

		public ZString LineUnit1
		{
			get { return ZString.Empty; }
		}

		public ZString LineValue2
		{
			get { return ZString.Empty; }
		}

		public ZString LineUnit2
		{
			get { return ZString.Empty; }
		}

		public ZString LineValue3
		{
			get { return ZString.Empty; }
		}

		public ZString LineUnit3
		{
			get { return ZString.Empty; }
		}

		public ZString LineValue4
		{
			get { return ZString.Empty; }
		}

		public ZString LineUnit4
		{
			get { return ZString.Empty; }
		}

		public ZString LineValue5
		{
			get { return ZString.Empty; }
		}

		public ZString LineUnit5
		{
			get { return ZString.Empty; }
		}

		public ZString LineValue6
		{
			get { return ZString.Empty; }
		}

		public ZString LineValue7
		{
			get { return ZString.Empty; }
		}

		public ZString LineValue8
		{
			get { return ZString.Empty; }
		}

		public ICASCProductCode[] CASCProductCodes => Array.Empty<ICASCProductCode>();

		public IEngineOrChassisNumber[] EngineOrChassisNumbers => Array.Empty<IEngineOrChassisNumber>();

		#endregion

		#endregion
	}

	#endregion

	#region CUSPMTPrintPermitConditions

	public class CUSPMTPrintPermitConditions : ICConditions
	{
		public CUSPMTPrintPermitConditions(FTXSegment message)
		{
			msg = message;
		}

		readonly FTXSegment msg;

		#region ICConditions Members

		public ZString Code
		{
			get { return new ZString(msg.TextLiteral.FreeText1).Left(3); }
		}

		public ZString Message
		{
			get { return msg.TextLiteral.FreeText1 + " " + msg.TextLiteral.FreeText2 + " " + msg.TextLiteral.FreeText3 + " " + msg.TextLiteral.FreeText4; }
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
			get { return SequenceNumber1 > 0 ? SequenceNumber1.ToString("00") + ")" : ""; }
		}

		ZString IPrintPermitContainers.ContainerIdentifier1
		{
			get { return ContainerIdentifier1; }
		}

		ZString IPrintPermitContainers.ContainerSequenceNumber2
		{
			get { return SequenceNumber2 > 0 ? SequenceNumber2.ToString("00") + ")" : ""; }
		}

		ZString IPrintPermitContainers.ContainerIdentifier2
		{
			get { return ContainerIdentifier2; }
		}

		#endregion
	}

	#endregion
}
