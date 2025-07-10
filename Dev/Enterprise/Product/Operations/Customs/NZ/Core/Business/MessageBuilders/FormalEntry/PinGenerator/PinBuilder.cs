using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.Types;
using Enterprise.Edifact.D96BNZ.Elements;
using Enterprise.Edifact.D96BNZ.Messages.CUSDEC;
using Enterprise.Edifact.D96BNZ.Segments;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.PinGenerator
{
	public class PinBuilder
	{
		static class NZCusSign64bitDll
		{
			public static class NativeMethods
			{
				[DllImport(@"NZCUSSIGN-X64.DLL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NZSign", ExactSpelling = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
				[return: MarshalAs(UnmanagedType.U1)]
				public static extern bool NZSign([MarshalAs(UnmanagedType.LPStr)] string PBUF, [MarshalAs(UnmanagedType.LPStr)] string Pin1, [MarshalAs(UnmanagedType.LPStr)] string Pin2, [MarshalAs(UnmanagedType.LPStr)] StringBuilder Result, [MarshalAs(UnmanagedType.LPStr)] string DebugString);
			}
		}

		public static bool NZSign(string pBUF, string pin1, string pin2, StringBuilder result, string debugString)
		{
			return NZCusSign64bitDll.NativeMethods.NZSign(pBUF, pin1, pin2, result, debugString);
		}

		public PinBuilder(string pinCode)
		{
			this.pinCode = pinCode;
			headerData = new DataBlock("A", "Header Data");
			remarks = new DataBlock("B", "Remarks");
			transportAndPermitDetails = new DataBlock("D", "Transport and Permit Details");
			partyIdentification = new DataBlock("E", "Party Identification");
			totals = new DataBlock("H", "Totals");
		}

		#region GetPBUF
		public string GetPBUF()
		{
			StringBuilder pBUF = new StringBuilder();

			pBUF.Append(headerData.GetPinGenerationBlock());
			pBUF.Append(remarks.GetPinGenerationBlock());
			foreach (DataBlock packaging in packagingDataBlocks)
			{
				pBUF.Append(packaging.GetPinGenerationBlock());
			}
			pBUF.Append(transportAndPermitDetails.GetPinGenerationBlock());
			pBUF.Append(partyIdentification.GetPinGenerationBlock());
			foreach (DataBlock invoiceHeader in invoiceHeaderDataBlocks)
			{
				pBUF.Append(invoiceHeader.GetPinGenerationBlock());
			}
			foreach (DataBlock invoiceLine in invoiceLineDataBlocks.Values)
			{
				pBUF.Append(invoiceLine.GetPinGenerationBlock());
			}
			pBUF.Append(totals.GetPinGenerationBlock());

			return pBUF.ToString();
		}
		#endregion

		#region GetMAC
		public string GetMAC()
		{
			string pBUF = GetPBUF();
			if (string.IsNullOrEmpty(pBUF))
			{
				return "PF.NO.PBUF";
			}
			else
			{
				StringBuilder result = new StringBuilder(16);
				NZSign(pBUF, pinCode, pinCode, result, null);
				return result.ToString();
			}
		}
		#endregion

		#region GenerateBlocksFromEDIFACTMessageText
		public void GenerateBlocksFromEDIFACTMessageText(string eDIFACTMessageText)
		{
			messageMAC = "";
			message = new CUSDECMessage();
			try
			{
				message.Parse(new Edifact.UNOACharacterSet(), eDIFACTMessageText);
				PopulateHeaderData();
				PopulateRemarks();
				PopulatePackaging();
				PopulateTransportAndPermitDetails();
				PopulatePartyIdentification();
				PopulateInvoiceHeaders();
				PopulateInvoiceLines();
				PopulateTotals();
			}
			catch (Edifact.InvalidFormatException)
			{
			}
		}
		#endregion

		#region GetPINDebugText
		public string GetPINDebugText()
		{
			StringBuilder pINDebugText = new StringBuilder();

			pINDebugText.Append(headerData.GetHumanReadableBlock());
			pINDebugText.Append(remarks.GetHumanReadableBlock());
			for (int index = 0; index < packagingDataBlocks.Count; index++)
			{
				DataBlock packaging = (DataBlock)packagingDataBlocks[index];
				pINDebugText.Append("block_" + packaging.BlockCode.ToLower() + " number =" + index.ToString() + "\r\n");
				pINDebugText.Append(packaging.GetHumanReadableBlock());
			}
			pINDebugText.Append(transportAndPermitDetails.GetHumanReadableBlock());
			pINDebugText.Append(partyIdentification.GetHumanReadableBlock());
			foreach (DataBlock invoiceHeader in invoiceHeaderDataBlocks)
			{
				pINDebugText.Append(invoiceHeader.GetHumanReadableBlock());
			}
			foreach (DataBlock invoiceLine in invoiceLineDataBlocks.Values)
			{
				pINDebugText.Append(invoiceLine.GetHumanReadableBlock());
			}
			pINDebugText.Append(totals.GetHumanReadableBlock());

			string pBUF = GetPBUF();
			string mAC = GetMAC();
			pINDebugText.Append("pbuf=[" + pBUF + "]\r\n");
			pINDebugText.Append("length of pbuf = " + pBUF.Length.ToString() + "\r\n");
			pINDebugText.Append("Looking up [" + declarantCode + "] in database\r\n");
			pINDebugText.Append("aut_result=    [" + mAC + "]\r\n");
			pINDebugText.Append("mes_aut_result=[" + messageMAC + "]\r\n");

			return pINDebugText.ToString();
		}

		#endregion

		#region Implementation

		protected CUSDECMessage message;

		#region Message Parser

		protected void PopulateHeaderData()
		{
			if (message.BGM.Count > 0)
			{
				BGMSegment bGM = message.BGM[0];
				headerData.AddDataField("class entry    ", bGM.DocumentMessageName.DocumentMessageNameCoded.ToString());
				headerData.AddDataField("client_ref     ", bGM.DocumentMessageIdentification.DocumentMessageNumber);
				headerData.AddDataField("tran type      ", bGM.MessageFunctionCoded.ToString());
			}
			if (message.CST.Count > 0)
			{
				CSTSegment cST = message.CST[0];
				headerData.AddDataField("entry type     ", cST.CustomsIdentityCodes1.CustomsCodeIdentification);
			}

			AddHeaderDataFromLOC("port loading   ", PlaceLocationQualifierList.PlacePortOfLoading);
			AddHeaderDataFromLOC("port discharge ", PlaceLocationQualifierList.PlacePortOfDischarge);
			AddHeaderDataFromLOC("customs control", PlaceLocationQualifierList.Warehouse);
			AddHeaderDataFromLOC("processing port", PlaceLocationQualifierList.CustomsOfficeOfEntry);
			AddHeaderDataFromLOC("country of dest", PlaceLocationQualifierList.CountryOfDestinationOfGoods);

			AddHeaderDataFromDTM("entry period   ", DateTimePeriodQualifierList.ProcessingDatePeriod);
			AddHeaderDataFromDTM("date import    ", DateTimePeriodQualifierList.ImportationDate);
			AddHeaderDataFromDTM("date export    ", DateTimePeriodQualifierList.ExportationDate);

			AddHeaderDataFromGIS("override ind   ", CodeListQualifierList.CustomsProcedure);
			AddHeaderDataFromGIS("sold_ind       ", CodeListQualifierList.StatisticalNatureOfTransaction);

			int otherInfoCount = 0;
			foreach (GISSegment gIS in message.GIS)
			{
				if (gIS.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsSpecialCodes)
				{
					otherInfoCount++;
					headerData.AddDataField("other info " + otherInfoCount.ToString().PadLeft(2) + "  ", gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString());
					headerData.AddDataField("other data " + otherInfoCount.ToString().PadLeft(2) + "  ", gIS.ProcessingIndicator.ProcessTypeIdentification.ToString());
					if (otherInfoCount == 10)
					{
						break;
					}
				}
			}
			otherInfoCount++;
			for (; otherInfoCount <= 10; otherInfoCount++)
			{
				headerData.AddDataField("other info " + otherInfoCount.ToString().PadLeft(2) + "  ", "");
				headerData.AddDataField("other data " + otherInfoCount.ToString().PadLeft(2) + "  ", "");
			}

			if (message.MEA.Count > 0)
			{
				MEASegment mEA = message.MEA[0];
				headerData.AddDataField("total weight   ", mEA.ValueRange.MeasurementValue);
			}
		}

		protected void PopulateRemarks()
		{
			if (message.FTX.Count > 0)
			{
				FTXSegment fTX = message.FTX[0];
				remarks.AddDataField("remarks (1)   ", fTX.TextLiteral.FreeText1);
				remarks.AddDataField("remarks (2)   ", fTX.TextLiteral.FreeText2);
				remarks.AddDataField("remarks (3)   ", fTX.TextLiteral.FreeText3);
				remarks.AddDataField("remarks (4)   ", fTX.TextLiteral.FreeText4);
				remarks.AddDataField("remarks (5)   ", fTX.TextLiteral.FreeText5);
			}
		}

		protected void PopulatePackaging()
		{
			Hashtable containerTypes = new Hashtable();
			Hashtable containerSealNos = new Hashtable();
			foreach (SegmentGroup99 group99 in message.Group99)
			{
				EQDSegment eQD = group99.EQD[0];
				if (eQD.EquipmentQualifier == EquipmentQualifierList.Container)
				{
					if (!containerTypes.Contains(eQD.EquipmentIdentification.EquipmentIdentificationNumber))
					{
						containerTypes.Add(eQD.EquipmentIdentification.EquipmentIdentificationNumber, eQD.FullEmptyIndicatorCoded.ToString());
					}
					ZString sealString = "";
					foreach (SELSegment sEL in group99.SEL)
					{
						sealString += sEL.SealNumber;
					}
					if (!sealString.IsEmpty)
					{
						containerSealNos.Add(eQD.EquipmentIdentification.EquipmentIdentificationNumber, sealString);
					}
				}
			}

			ZString referenceType = "";
			ZString referenceNumber = "";
			ZString containerType = "";
			ZString containerNumber = "";
			ZString containerStatus = "";
			ZString containerSealNo = "";

			foreach (SegmentGroup1 group1 in message.Group1)
			{
				RFFSegment rFF = group1.RFF[0]; // Is always 1 RFF as it's a trigger segment
				if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.MasterBillOfLadingNumber)
				{
					AddPackagingData(rFF.Reference.ReferenceQualifier.ToString(), rFF.Reference.ReferenceNumber, "", "", "", "", "", "");
				}
				else
				{
					if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.ArticleNumber
						|| rFF.Reference.ReferenceQualifier == ReferenceQualifierList.BillOfLadingNumber
						|| rFF.Reference.ReferenceQualifier == ReferenceQualifierList.HouseWaybillNumber)
					{
						referenceType = rFF.Reference.ReferenceQualifier.ToString();
						referenceNumber = rFF.Reference.ReferenceNumber;
						containerType = "";
						containerNumber = "";
						containerStatus = "";
						containerSealNo = "";
					}
					else if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.ShippingUnitIdentification
						|| rFF.Reference.ReferenceQualifier == ReferenceQualifierList.UnitLoadDeviceEGContainerIdentificationNumber)
					{
						containerNumber = rFF.Reference.ReferenceNumber;
						containerType = rFF.Reference.ReferenceQualifier.ToString();
						if (!containerNumber.IsEmpty)
						{
							if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.UnitLoadDeviceEGContainerIdentificationNumber)
							{
								containerStatus = containerTypes[rFF.Reference.ReferenceNumber] == null ? "" : containerTypes[rFF.Reference.ReferenceNumber].ToString();
								containerSealNo = containerSealNos[rFF.Reference.ReferenceNumber] == null ? "" : containerSealNos[rFF.Reference.ReferenceNumber].ToString();
							}
						}
					}
					else
					{
						referenceType = "";
						referenceNumber = "";
						containerType = "";
						containerNumber = "";
						containerStatus = "";
						containerSealNo = "";
					}

					if (referenceType != "" && referenceNumber != "")
					{
						bool firstPACSegment = true;
						foreach (SegmentGroup2 group2 in group1.Group2)
						{
							PACSegment pAC = group2.PAC[0];
							AddPackagingData(referenceType, referenceNumber, containerType, containerNumber, containerStatus, pAC.NumberOfPackages, pAC.PackageType.TypeOfPackagesIdentification, (firstPACSegment ? containerSealNo : ZString.Empty));
							firstPACSegment = false;
						}
						if (group1.Group2.Count == 0 && containerStatus == "4") // Empty Containers do not require a PAC Segment
						{
							AddPackagingData(referenceType, referenceNumber, containerType, containerNumber, containerStatus, "", "", containerSealNo);
						}
					}
				}
			}
		}

		protected void PopulateTransportAndPermitDetails()
		{
			foreach (SegmentGroup4 group4 in message.Group4)
			{
				TDTSegment tDT = group4.TDT[0];
				transportAndPermitDetails.AddDataField("voyage_number      ", tDT.ConveyanceReferenceNumber);
				transportAndPermitDetails.AddDataField("transport mode     ", tDT.ModeOfTransport.ModeOfTransportCoded);
				transportAndPermitDetails.AddDataField("craft flight no    ", tDT.TransportIdentification.IdOfTheMeansOfTransport);
			}

			int permitCount = 0;
			foreach (SegmentGroup5 group5 in message.Group5)
			{
				permitCount++;
				DOCSegment dOC = group5.DOC[0];
				transportAndPermitDetails.AddDataField("permit auth code" + permitCount.ToString().PadLeft(2) + " ", dOC.DocumentMessageName.DocumentMessageNameCoded.ToString());
				transportAndPermitDetails.AddDataField("permit auth no" + permitCount.ToString().PadLeft(2) + "   ", dOC.DocumentMessageDetails.DocumentMessageNumber);
				if (permitCount == 10)
				{
					break;
				}
			}
			permitCount++;
			for (; permitCount <= 10; permitCount++)
			{
				transportAndPermitDetails.AddDataField("permit auth code" + permitCount.ToString().PadLeft(2) + " ", "");
				transportAndPermitDetails.AddDataField("permit auth no" + permitCount.ToString().PadLeft(2) + "   ", "");
			}
		}

		protected void PopulatePartyIdentification()
		{
			string clientCode = "";
			string clientName = "";
			string brokerCode = "";
			string delAuthCode = "";

			foreach (SegmentGroup6 group6 in message.Group6)
			{
				NADSegment nAD = group6.NAD[0];
				if (nAD.PartyQualifier == PartyQualifierList.Principal)
				{
					clientCode = nAD.PartyIdentificationDetails.PartyIdIdentification;
					clientName = nAD.NameAndAddress.NameAndAddressLine1;
				}
				else if (nAD.PartyQualifier == PartyQualifierList.CustomsBroker)
				{
					brokerCode = nAD.PartyIdentificationDetails.PartyIdIdentification;
				}
				else if (nAD.PartyQualifier == PartyQualifierList.DeliveryParty)
				{
					delAuthCode = nAD.PartyIdentificationDetails.PartyIdIdentification;
				}
			}

			partyIdentification.AddDataField("client code  ", clientCode);
			partyIdentification.AddDataField("client name  ", clientName);
			partyIdentification.AddDataField("broker code  ", brokerCode);
			partyIdentification.AddDataField("del auth code", delAuthCode);
		}

		protected void PopulateInvoiceHeaders()
		{
			foreach (SegmentGroup10 group10 in message.Group10)
			{
				DMSSegment dMS = group10.DMS[0];
				AddNewInvoiceHeader();
				CurrentInvoiceHeader.AddDataField("invoice number ", dMS.DocumentMessageNumber);
				foreach (SegmentGroup13 group13 in group10.Group13)
				{
					TODSegment tOD = group13.TOD[0];
					CurrentInvoiceHeader.AddDataField("invoice terms  ", tOD.TermsOfDeliveryOrTransport.TermsOfDeliveryOrTransportCoded);
				}
			}
		}

		protected void PopulateInvoiceLines()
		{
			foreach (SegmentGroup30 group30 in message.Group30)
			{
				CSTSegment cST = group30.CST[0];
				AddNewInvoiceLine(int.Parse(cST.GoodsItemNumber));
				CurrentInvoiceLine.AddDataField("line_number         ", cST.GoodsItemNumber);
				CurrentInvoiceLine.AddDataField("tariff item         ", cST.CustomsIdentityCodes1.CustomsCodeIdentification);
				CurrentInvoiceLine.AddDataField("cons code           ", cST.CustomsIdentityCodes2.CustomsCodeIdentification);

				string goodsDescription = "";
				if (group30.FTX.Count > 0)
				{
					FTXSegment fTX = group30.FTX[0];
					goodsDescription = fTX.TextLiteral.FreeText1 + fTX.TextLiteral.FreeText2 + fTX.TextLiteral.FreeText3 + fTX.TextLiteral.FreeText4 + fTX.TextLiteral.FreeText5;
				}
				CurrentInvoiceLine.AddDataField("goods descr         ", goodsDescription);

				string countryOfOrigin = "";
				string countryOfExport = "";
				foreach (LOCSegment lOC in group30.LOC)
				{
					if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.CountryOfOrigin)
					{
						countryOfOrigin = lOC.LocationIdentification.PlaceLocationIdentification;
					}
					else if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.CountryOfExportationDespatch)
					{
						countryOfExport = lOC.LocationIdentification.PlaceLocationIdentification;
					}
				}
				CurrentInvoiceLine.AddDataField("country origin      ", countryOfOrigin);
				CurrentInvoiceLine.AddDataField("country export      ", countryOfExport);

				string statUnit = "";
				string statQty = "";
				string suppUnit = "";
				string suppQty = "";
				foreach (MEASegment mEA in group30.MEA)
				{
					if (mEA.MeasurementApplicationQualifier == MeasurementApplicationQualifierList._1stSpecifiedTariffQuantity)
					{
						statUnit = mEA.ValueRange.MeasureUnitQualifier;
						statQty = mEA.ValueRange.MeasurementValue;
					}
					else if (mEA.MeasurementApplicationQualifier == MeasurementApplicationQualifierList._2ndSpecifiedTariffQuantity)
					{
						suppUnit = mEA.ValueRange.MeasureUnitQualifier;
						suppQty = mEA.ValueRange.MeasurementValue;
					}
				}
				CurrentInvoiceLine.AddDataField("stat unit           ", statUnit);
				CurrentInvoiceLine.AddDataField("stat qty            ", statQty);
				CurrentInvoiceLine.AddDataField("supp unit           ", suppUnit);
				CurrentInvoiceLine.AddDataField("supp qty            ", suppQty);

				string supplierCode = "";
				string supplierName = "";
				foreach (NADSegment nAD in group30.NAD)
				{
					if (nAD.PartyQualifier == PartyQualifierList.Supplier)
					{
						supplierCode = nAD.PartyIdentificationDetails.PartyIdIdentification;
						supplierName = nAD.NameAndAddress.NameAndAddressLine1;
					}
				}
				CurrentInvoiceLine.AddDataField("supplier code       ", supplierCode);
				CurrentInvoiceLine.AddDataField("supplier name       ", supplierName);

				string valueInCurr = "";
				string currCode = "";
				string valueInLocal = "";
				string freight = "";
				string insurance = "";
				string exchRate = "";
				string exchInd = "";
				foreach (SegmentGroup33 group33 in group30.Group33)
				{
					MOASegment mOA = group33.MOA[0];
					if (mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.AmountTargetCurrency)
					{
						valueInCurr = mOA.MonetaryAmount.MonetaryAmount;
						currCode = mOA.MonetaryAmount.CurrencyCoded;
						foreach (SegmentGroup34 group34 in group33.Group34)
						{
							CUXSegment cUX = group34.CUX[0];
							exchRate = cUX.RateOfExchange;
							exchInd = cUX.CurrencyMarketExchangeCoded.ToString();
						}
					}
					else if (mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.CustomsValue)
					{
						valueInLocal = mOA.MonetaryAmount.MonetaryAmount;
					}
					else if (mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.FreightCharge)
					{
						freight = mOA.MonetaryAmount.MonetaryAmount;
					}
					else if (mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.InsuranceChargesCustoms)
					{
						insurance = mOA.MonetaryAmount.MonetaryAmount;
					}
				}
				CurrentInvoiceLine.AddDataField("value in curr       ", valueInCurr);
				CurrentInvoiceLine.AddDataField("curr code           ", currCode);
				CurrentInvoiceLine.AddDataField("value in nz         ", valueInLocal);
				CurrentInvoiceLine.AddDataField("freight             ", freight);
				CurrentInvoiceLine.AddDataField("insurance           ", insurance);
				CurrentInvoiceLine.AddDataField("exch rate           ", exchRate);
				CurrentInvoiceLine.AddDataField("exch ind            ", exchInd);

				int permitCount = 0;
				foreach (SegmentGroup37 group37 in group30.Group37)
				{
					DOCSegment dOC = group37.DOC[0];
					if (dOC.DocumentMessageName.CodeListQualifier == CodeListQualifierList.DocumentRequestedByCustoms)
					{
						permitCount++;
						CurrentInvoiceLine.AddDataField("permit auth code  " + permitCount.ToString() + " ", dOC.DocumentMessageName.DocumentMessageNameCoded.ToString());
						CurrentInvoiceLine.AddDataField("permit auth no    " + permitCount.ToString() + " ", dOC.DocumentMessageDetails.DocumentMessageNumber);
						if (permitCount == 5)
						{
							break;
						}
					}
				}
				permitCount++;
				for (; permitCount <= 5; permitCount++)
				{
					CurrentInvoiceLine.AddDataField("permit auth code  " + permitCount.ToString() + " ", "");
					CurrentInvoiceLine.AddDataField("permit auth no    " + permitCount.ToString() + " ", "");
				}

				string relationInd = "";
				foreach (SegmentGroup40 group40 in group30.Group40)
				{
					GISSegment gIS = group40.GIS[0];
					if (gIS.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsIndicator)
					{
						relationInd = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
						break;
					}
				}
				CurrentInvoiceLine.AddDataField("relation ind        ", relationInd);

				int prohibitedCodeCount = 0;
				foreach (SegmentGroup40 group40 in group30.Group40)
				{
					GISSegment gIS = group40.GIS[0];
					if (gIS.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.GovernmentAgencyProcedure)
					{
						prohibitedCodeCount++;
						CurrentInvoiceLine.AddDataField("proh code " + prohibitedCodeCount.ToString() + "         ", gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString());
						if (prohibitedCodeCount == 3)
						{
							break;
						}
					}
				}
				prohibitedCodeCount++;
				for (; prohibitedCodeCount <= 3; prohibitedCodeCount++)
				{
					CurrentInvoiceLine.AddDataField("proh code " + prohibitedCodeCount.ToString() + "         ", "");
				}

				int otherInfoCount = 0;
				foreach (SegmentGroup40 group40 in group30.Group40)
				{
					GISSegment gIS = group40.GIS[0];
					if (gIS.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsSpecialCodes)
					{
						otherInfoCount++;
						CurrentInvoiceLine.AddDataField("other info " + otherInfoCount.ToString().PadLeft(2) + "       ", gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString());
						CurrentInvoiceLine.AddDataField("other data " + otherInfoCount.ToString().PadLeft(2) + "       ", gIS.ProcessingIndicator.ProcessTypeIdentification.ToString());
						if (otherInfoCount == 5)
						{
							break;
						}
					}
				}
				otherInfoCount++;
				for (; otherInfoCount <= 5; otherInfoCount++)
				{
					CurrentInvoiceLine.AddDataField("other info " + otherInfoCount.ToString().PadLeft(2) + "       ", "");
					CurrentInvoiceLine.AddDataField("other data " + otherInfoCount.ToString().PadLeft(2) + "       ", "");
				}

				string aLACLevy = "";
				string aCCLevy = "";
				string hERALevy = "";
				string pFMLLevy = "";
				string sGGLevy = "";
				string antiDump = "";
				string countervailing = "";
				string importDuty = "";
				string dutyCredit = "";
				string gST = "";
				string prefInd = "";

				foreach (SegmentGroup41 group41 in group30.Group41)
				{
					TAXSegment tAX = group41.TAX[0];
					foreach (MOASegment mOA in group41.MOA)
					{
						if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.CommoditySpecificTax)
						{
							if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.AlacAlcoholLevy)
							{
								aLACLevy = mOA.MonetaryAmount.MonetaryAmount;
							}
							else if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.HeraSteelLevy)
							{
								hERALevy = mOA.MonetaryAmount.MonetaryAmount;
							}
							else if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.AccFuelLevy)
							{
								aCCLevy = mOA.MonetaryAmount.MonetaryAmount;
							}
							else if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.PfmlFuelLevy)
							{
								pFMLLevy = mOA.MonetaryAmount.MonetaryAmount;
							}
							else if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.SggSyntheticGreenhouseGasesLevy)
							{
								sGGLevy = mOA.MonetaryAmount.MonetaryAmount;
							}
						}
						else if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.AntiDumpingDuty)
						{
							antiDump = mOA.MonetaryAmount.MonetaryAmount;
						}
						else if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.CountervailingDuty)
						{
							countervailing = mOA.MonetaryAmount.MonetaryAmount;
						}
						else if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.CustomsDuty)
						{
							if (mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.NonTaxableAmount)
							{
								dutyCredit = mOA.MonetaryAmount.MonetaryAmount;
							}
							else
							{
								importDuty = mOA.MonetaryAmount.MonetaryAmount;
							}
						}
						else if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.GoodsAndServicesTax)
						{
							gST = mOA.MonetaryAmount.MonetaryAmount;
						}
					}
					if (group41.GIS.Count > 0)
					{
						GISSegment gIS = group41.GIS[0];
						prefInd = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
					}
				}
				CurrentInvoiceLine.AddDataField("alac levy           ", aLACLevy);
				CurrentInvoiceLine.AddDataField("hera levy           ", hERALevy);
				CurrentInvoiceLine.AddDataField("acc levy            ", aCCLevy);
				CurrentInvoiceLine.AddDataField("pfml levy           ", pFMLLevy);
				CurrentInvoiceLine.AddDataField("sgg levy            ", sGGLevy);
				CurrentInvoiceLine.AddDataField("anti dump           ", antiDump);
				CurrentInvoiceLine.AddDataField("countervailing      ", countervailing);
				CurrentInvoiceLine.AddDataField("tariff duty         ", ""); // Don't know why this is in the dump we get from Customs, but it is....
				CurrentInvoiceLine.AddDataField("import duty         ", importDuty);
				CurrentInvoiceLine.AddDataField("duty credit         ", dutyCredit);
				CurrentInvoiceLine.AddDataField("gst                 ", gST);
				CurrentInvoiceLine.AddDataField("pref ind            ", prefInd);
			}
		}

		protected void PopulateTotals()
		{
			string totalInvoices = "";
			string totalLines = "";
			string totalPackages = "";
			foreach (CNTSegment cNT in message.CNT)
			{
				if (cNT.Control.ControlQualifier == ControlQualifierList.NumberOfInvoiceLines)
				{
					totalInvoices = cNT.Control.ControlValue;
				}
				else if (cNT.Control.ControlQualifier == ControlQualifierList.NumberOfCustomsItemDetailLines)
				{
					totalLines = cNT.Control.ControlValue;
				}
				else if (cNT.Control.ControlQualifier == ControlQualifierList.TotalNumberOfPackages)
				{
					totalPackages = cNT.Control.ControlValue;
				}
			}
			totals.AddDataField("total_invoices      ", totalInvoices);
			totals.AddDataField("total_lines         ", totalLines);
			totals.AddDataField("total_packages      ", totalPackages);

			string totalALAC = "";
			string totalHERA = "";
			string totalACC = "";
			string totalPFML = "";
			string totalSGG = "";
			string totalAntiDump = "";
			string totalCountervailing = "";
			string totalTariffDuty = "";
			string totalValueInNZ = "";
			string totalGST = "";
			string totalAmountPayable = "";
			string totalDutyCreditsOrDepositRefunded = "";
			string methodOfPayment = "";

			foreach (SegmentGroup49 group49 in message.Group49)
			{
				TAXSegment tAX = group49.TAX[0];
				if (tAX.DutyTaxFeeFunctionQualifier == DutyTaxFeeFunctionQualifierList.TotalOfEachDutyTaxOrFeeTypeCustomsDeclaration
					&& tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.CustomsDuty)
				{
					totalValueInNZ = tAX.DutyTaxFeeAssessmentBasis;
				}
				foreach (MOASegment mOA in group49.MOA)
				{
					if (tAX.DutyTaxFeeFunctionQualifier == DutyTaxFeeFunctionQualifierList.TotalOfEachDutyTaxOrFeeTypeCustomsDeclaration)
					{
						if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.CommoditySpecificTax)
						{
							if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.AlacAlcoholLevy)
							{
								totalALAC = mOA.MonetaryAmount.MonetaryAmount;
							}
							else if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.HeraSteelLevy)
							{
								totalHERA = mOA.MonetaryAmount.MonetaryAmount;
							}
							else if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.AccFuelLevy)
							{
								totalACC = mOA.MonetaryAmount.MonetaryAmount;
							}
							else if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.PfmlFuelLevy)
							{
								totalPFML = mOA.MonetaryAmount.MonetaryAmount;
							}
							else if (tAX.DutyTaxFeeAccountDetail.DutyTaxFeeAccountIdentification == CustomsLevyTypeList.Codes.SggSyntheticGreenhouseGasesLevy)
							{
								totalSGG = mOA.MonetaryAmount.MonetaryAmount;
							}
						}
						else if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.AntiDumpingDuty)
						{
							totalAntiDump = mOA.MonetaryAmount.MonetaryAmount;
						}
						else if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.CountervailingDuty)
						{
							totalCountervailing = mOA.MonetaryAmount.MonetaryAmount;
						}
						else if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.CustomsDuty)
						{
							totalTariffDuty = mOA.MonetaryAmount.MonetaryAmount;
						}
						else if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.GoodsAndServicesTax)
						{
							totalGST = mOA.MonetaryAmount.MonetaryAmount;
						}
					}
					else if (tAX.DutyTaxFeeFunctionQualifier == DutyTaxFeeFunctionQualifierList.TotalOfAllDutiesTaxesAndFeeTypesCustomsDeclaration)
					{
						if (tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.Total)
						{
							if (mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.DutyTaxOrFeeAmount)
							{
								totalAmountPayable = mOA.MonetaryAmount.MonetaryAmount;
							}
							else if (mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.DepositRefund
								|| mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.NonTaxableAmount)
							{
								totalDutyCreditsOrDepositRefunded = mOA.MonetaryAmount.MonetaryAmount;
							}
						}
					}
				}
				if (tAX.DutyTaxFeeFunctionQualifier == DutyTaxFeeFunctionQualifierList.TotalOfAllDutiesTaxesAndFeeTypesCustomsDeclaration
					&& tAX.DutyTaxFeeType.DutyTaxFeeTypeCoded == DutyTaxFeeTypeCodedList.Total
					&& group49.GIS.Count > 0)
				{
					GISSegment gIS = group49.GIS[0];
					methodOfPayment = gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
				}
			}

			totals.AddDataField("total_alac          ", totalALAC);
			totals.AddDataField("total_hera          ", totalHERA);
			totals.AddDataField("total_acc           ", totalACC);
			totals.AddDataField("total_pfml          ", totalPFML);
			totals.AddDataField("total_sgg           ", totalSGG);
			totals.AddDataField("total_anti dump     ", totalAntiDump);
			totals.AddDataField("total_countervailing", totalCountervailing);
			totals.AddDataField("total_tariff duty   ", totalTariffDuty);
			totals.AddDataField("total_value in nz   ", totalValueInNZ);
			totals.AddDataField("total_gst           ", totalGST);
			totals.AddDataField("total_amount        ", totalAmountPayable);
			totals.AddDataField("total_duty credits  ", totalDutyCreditsOrDepositRefunded);
			totals.AddDataField("method of payment   ", methodOfPayment);

			declarantCode = "";
			foreach (SegmentGroup50 group50 in message.Group50)
			{
				AUTSegment aUT = group50.AUT[0];
				declarantCode = aUT.ValidationKeyIdentification;
				messageMAC = aUT.ValidationResult;
			}
			totals.AddDataField("declarant code      ", declarantCode);
		}

		#endregion

		#region Message Parser Implementation

		protected void AddHeaderDataFromLOC(string tagName, PlaceLocationQualifierList lOCType)
		{
			foreach (LOCSegment lOC in message.LOC)
			{
				if (lOC.PlaceLocationQualifier == lOCType)
				{
					headerData.AddDataField(tagName, lOC.LocationIdentification.PlaceLocationIdentification);
					break;
				}
			}
		}

		protected void AddHeaderDataFromDTM(string tagName, DateTimePeriodQualifierList dTMType)
		{
			foreach (DTMSegment dTM in message.DTM)
			{
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == dTMType)
				{
					headerData.AddDataField(tagName, dTM.DateTimePeriod.DateTimePeriod);
					break;
				}
			}
		}

		protected void AddHeaderDataFromGIS(string tagName, CodeListQualifierList gISType)
		{
			foreach (GISSegment gIS in message.GIS)
			{
				if (gIS.ProcessingIndicator.CodeListQualifier == gISType)
				{
					headerData.AddDataField(tagName, gIS.ProcessingIndicator.ProcessingIndicatorCoded.ToString());
					break;
				}
			}
		}

		protected void AddPackagingData(string referenceType, string referenceNumber, string containerType, string containerNumber, string containerStatus, string numberPackages, string typePackages, string sealNumber)
		{
			AddNewPackaging();
			CurrentPackaging.AddDataField("reference type q   ", referenceType);
			CurrentPackaging.AddDataField("reference number   ", referenceNumber);
			CurrentPackaging.AddDataField("container type q   ", containerType);
			CurrentPackaging.AddDataField("container number   ", containerNumber);
			CurrentPackaging.AddDataField("container status   ", containerStatus);
			CurrentPackaging.AddDataField("number packages    ", numberPackages);
			CurrentPackaging.AddDataField("type packages      ", typePackages);
			CurrentPackaging.AddDataField("seal numbers       ", sealNumber);
		}

		#endregion

		#region DataBlocks
		protected string pinCode;
		protected string declarantCode;
		protected string messageMAC;
		protected DataBlock headerData;
		protected DataBlock remarks;
		protected DataBlock transportAndPermitDetails;
		protected DataBlock partyIdentification;
		protected DataBlock totals;

		protected DataBlock CurrentPackaging
		{
			get
			{
				return fCurrentPackaging;
			}
		}

		protected void AddNewPackaging()
		{
			fCurrentPackaging = new DataBlock("C", "Packaging");
			packagingDataBlocks.Add(fCurrentPackaging);
		}

		protected DataBlock CurrentInvoiceHeader
		{
			get
			{
				return fCurrentInvoiceHeader;
			}
		}

		protected void AddNewInvoiceHeader()
		{
			fCurrentInvoiceHeader = new DataBlock("F", "Invoice Details");
			invoiceHeaderDataBlocks.Add(fCurrentInvoiceHeader);
		}

		protected DataBlock CurrentInvoiceLine
		{
			get
			{
				return fCurrentInvoiceLine;
			}
		}

		protected void AddNewInvoiceLine(int lineNumber)
		{
			fCurrentInvoiceLine = new DataBlock("G", "Line Item");

			invoiceLineDataBlocks.Add(lineNumber, fCurrentInvoiceLine);
		}

		protected DataBlock fCurrentPackaging;
		protected ArrayList packagingDataBlocks = new ArrayList();

		protected DataBlock fCurrentInvoiceHeader;
		protected ArrayList invoiceHeaderDataBlocks = new ArrayList();

		protected DataBlock fCurrentInvoiceLine;
		protected SortedList<int, DataBlock> invoiceLineDataBlocks = new SortedList<int, DataBlock>();
		#endregion

		#endregion
	}
}
