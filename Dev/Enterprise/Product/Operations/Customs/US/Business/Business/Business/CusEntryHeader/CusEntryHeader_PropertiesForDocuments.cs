using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	partial class CusEntryHeader
	{
		#region Delivery Order Document fields

		public StmNoteCollection DeclarationSpecialInstructions
		{
			get
			{
				StmNoteCollection declarationSpecialInstructions = new StmNoteCollection(Declaration, Factory);
				Declaration.RegisterEditableChildObject(declarationSpecialInstructions);
				ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.SpecialInstructions.Description);
				declarationSpecialInstructions.LoadWithMoreFiltering(filter);
				return declarationSpecialInstructions;
			}
		}

		public ZString Location
		{
			get
			{
				string result = ZString.Empty;

				if (Declaration.DepotDocAddress != null)
				{
					result = Declaration.DepotDocAddress.E2_Address1;
				}

				if (Declaration.JE_ContainerMode == Enterprise.Core.Constants.ContainerModes.Containerised)
				{
					foreach (CusContainer container in Containers)
					{
						if (container.CO_FCL_LCL_AIR == Enterprise.Core.Constants.ContainerModes.FCL)
						{
							if (Declaration.ContainerTerminalOperatorDocAddress != null)
							{
								result = Declaration.ContainerTerminalOperatorDocAddress.E2_Address1;
							}
							break;
						}
					}
				}
				return result;
			}
		}

		public ZString SupplierContactPerson
		{
			get
			{
				string result = ZString.Empty;
				if ((Declaration.Supplier != null) && (Declaration.Supplier.Contacts.Count > 0))
				{
					result = Declaration.Supplier.Contacts[0].OC_ContactName;
				}
				return result;
			}
		}

		public ZString ImporterDeliveryContactPerson
		{
			get
			{
				string result = ZString.Empty;
				if ((Declaration.ImporterDeliveryAddress.Organisation != null) && (Declaration.ImporterDeliveryAddress.Organisation.Contacts.Count > 0))
				{
					result = Declaration.ImporterDeliveryAddress.Organisation.Contacts[0].OC_ContactName;
				}
				return result;
			}
		}

		#endregion

		#region 7501 Entry Summary DocData

		#region DutyPercentAsString - DocPrintingDetails

		public void CreateDocPrintingDetails(ZGuid outgoingMsgPK)
		{
			ZGuid invoicePK = ZGuid.Empty;
			ZInt linesThisInvoice = 0;
			ZInt currentLineThisInvoice = 0;
			bool printInvoiceHeading = false;
			bool printInvoiceDetails = false;
			bool taxDeferred = Declaration.TaxDeferred;

			foreach (CusEntryLine entryLine in EntryLines)
			{
				US7501DocPrinting docPrinting = US7501DocPrintingData.AddNew();
				if (entryLine.RandomLine.JI_JZ != invoicePK)
				{
					invoicePK = entryLine.RandomLine.JI_JZ;
					currentLineThisInvoice = 0;
					linesThisInvoice = LinesThisInvoice(invoicePK);
				}

				if (!entryLine.IsSecondaryTariffLine)
				{
					currentLineThisInvoice++;
					printInvoiceHeading = currentLineThisInvoice == 1;
					printInvoiceDetails = currentLineThisInvoice == linesThisInvoice;
				}

				fAdValoremConversionCalculation = null;
				bool hasSecondaryWatchLine = DoesLineHaveSecondaryWatchLine(entryLine);
				bool isUSComponentsAssembledAbroad = IsTariffUSComponentsAssembledAbroad(entryLine);
				bool requiresAdValoremConversionCalculation = AdValoremConversionCalculation(entryLine, hasSecondaryWatchLine);
				bool isWatchLineAssembledAbroad = entryLine.IsParentLine && isUSComponentsAssembledAbroad && hasSecondaryWatchLine;

				docPrinting.US_MsgPK = outgoingMsgPK;
				docPrinting.US_LineNo = entryLine.CL_LineNumber;
				docPrinting.US_DutyDate = entryLine.DateForDutyCalculation;
				if (entryLine.Declaration.US_EntryType == EntryTypeList.Codes.TemporaryImportationBond)
				{
					AppendixFDutyCalculator calculator = new AppendixFDutyCalculator(entryLine, Factory);
					docPrinting.US_RateAsString = calculator.DutyResult.RateString;
				}

				if (docPrinting.US_RateAsString.IsEmpty)
				{
					docPrinting.US_RateAsString = entryLine.CL_DutyPercentAsString.Left(US7501DocPrintingAddInfo.Schema.US_RateAsStringMaxLength);
				}

				SetADDCVDIndicators(entryLine, docPrinting);
				SetFeeAndTaxRatePercentageStrings(entryLine, docPrinting, taxDeferred);

				if (entryLine.RandomLine.InvoiceHeader != null)
				{
					docPrinting.US_InvoicePK = entryLine.RandomLine.InvoiceHeader.PK;
					docPrinting.US_InvoiceSeq = entryLine.RandomLine.InvoiceHeader.JZ_InvoiceDisplaySequence;
				}
				docPrinting.US_InvoiceLinePK = entryLine.RandomLine.PK;

				if (requiresAdValoremConversionCalculation)
				{
					SetAVConversionDetails(docPrinting);
				}

				if (isWatchLineAssembledAbroad && entryLine.IsRandomLineSPINotCAAndS)
				{
					SetProRatedDetails(entryLine, docPrinting);
				}

				ExtractSecondaryLineDetails(entryLine, docPrinting, isWatchLineAssembledAbroad, isUSComponentsAssembledAbroad, requiresAdValoremConversionCalculation, taxDeferred, outgoingMsgPK);
			}
		}

		#region ADDCVD

		void SetADDCVDIndicators(CusEntryLine entryLine, US7501DocPrinting docPrinting)
		{
			var adCaseNo = entryLine.ADDNo.IsEmpty ? entryLine.RandomLine.US_ADDCaseNo : entryLine.ADDNo.KeepAlphanumericCharacters();
			if (!adCaseNo.IsEmpty)
			{
				if (entryLine.US_SupLine)
				{
					docPrinting.US_ADDOnParentOrChild = entryLine.RandomLine.AntidumpingDutyCase.MatchesTariff(entryLine.ChildLines[0].CL_AdValoremTariff) ? ADDCVDOnParentOrChild.Codes.Child : ADDCVDOnParentOrChild.Codes.Parent;
				}
				else
				{
					docPrinting.US_ADDOnParentOrChild = entryLine.RandomLine.US_ADDCaseNo.IsEmpty ? ADDCVDOnParentOrChild.Codes.Child : ADDCVDOnParentOrChild.Codes.Parent;
				}
			}

			var cvCaseNo = entryLine.CVDNo.IsEmpty ? entryLine.RandomLine.US_CVDCaseNo : entryLine.CVDNo.KeepAlphanumericCharacters();
			if (!cvCaseNo.IsEmpty)
			{
				if (entryLine.US_SupLine)
				{
					docPrinting.US_CVDOnParentOrChild = entryLine.RandomLine.CountervailingDutyCase.MatchesTariff(entryLine.ChildLines[0].CL_AdValoremTariff) ? ADDCVDOnParentOrChild.Codes.Child : ADDCVDOnParentOrChild.Codes.Parent;
				}
				else
				{
					docPrinting.US_CVDOnParentOrChild = entryLine.RandomLine.US_CVDCaseNo.IsEmpty ? ADDCVDOnParentOrChild.Codes.Child : ADDCVDOnParentOrChild.Codes.Parent;
				}
			}
		}

		#endregion

		#region DocData Details

		int LinesThisInvoice(ZGuid invoicePK)
		{
			int result = 0;

			foreach (CusEntryLine entryLine in EntryLines)
			{
				if (entryLine.RandomLine.JI_JZ == invoicePK && !entryLine.IsSecondaryTariffLine)
				{
					result++;
				}
			}

			return result;
		}

		void SetFeeAndTaxRatePercentageStrings(CusEntryLine entryLine, US7501DocPrinting docPrinting, bool taxDeferred)
		{
			var feeList = CusFeeCodeConstants.GetAccountingClassFeeCodeList(entryLine.Factory);
			foreach (CusEntryLineFee fee in entryLine.Fees)
			{
				if (fee.CF_ChargeType != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing &&
					fee.CF_ChargeType != Core.Constants.USCustoms.FeeCodes.HMF &&
					feeList.ContainsCode(fee.CF_ChargeType))
				{
					docPrinting.US_FEECode = fee.CF_ChargeType.SubstringSafe(0, AutoUS7501DocPrintingAddInfo.Schema.US_FEECodeMaxLength);

					docPrinting.US_FEEPercentAsString = entryLine.GetTaxOrFeeRate(docPrinting.US_FEECode, taxDeferred && CusFeeCodeConstants.IsExciseTax(docPrinting.US_FEECode)).Left(US7501DocPrintingAddInfo.Schema.US_FEEPercentAsStringMaxLength);

					docPrinting.US_SpecificRate = ((IFeeCalculationDataProvider)entryLine).TaxRateType;

					break;
				}
			}
		}

		#endregion

		#region SecondaryLineDetails

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ExtractSecondaryLineDetails(CusEntryLine entryLine, US7501DocPrinting docPrinting, bool isWatchLineAssembledAbroad, bool isUSComponentsAssembledAbroad, bool requiresAdValoremConversionCalculation, bool taxDeferred, ZGuid outgoingMsgPK)
		{
			ZInt secondaryTariffLinesCount = 0;
			CusEntryLine secondaryTariffLine1 = null;
			CusEntryLine secondaryTariffLine2 = null;
			CusEntryLine secondaryTariffLine3 = null;
			CusEntryLine secondaryTariffLine4 = null;
			CusEntryLine secondaryTariffLine5 = null;
			CusEntryLine secondaryTariffLine6 = null;
			CusEntryLine secondaryTariffLine7 = null;

			var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(entryLine) as IEntryLineOrInvoiceLineDutyData;
			if (Chapter98Helper.HaveTIB9813TariffLine(dutyData))
			{
				var dutyResult = Chapter98Helper.CalculateTIBDutyForPrint(entryLine);
				if (dutyResult != null)
				{
					docPrinting.US_DutyAmount = dutyResult.TotalAmount.Amount;
					docPrinting.US_RateAsString = dutyResult.RateString;
				}
			}
			else if (CalculateDutyForDocument.ShouldCalculateDutyOnlyForDocumnet(dutyData))
			{
				var dutyResult = CalculateDutyForDocument.PrinterDuty(dutyData);
				if (dutyResult != null)
				{
					docPrinting.US_DutyAmount = dutyResult.TotalAmount.Amount;
					docPrinting.US_RateAsString = dutyResult.RateString;
					docPrinting.US_IsAdditionalTotalPrintingDuty = true;
				}
			}

			var childSecondaryEntryLines = entryLine.IsCombinedLine() ? entryLine.ChildSecondaryEntryLines.Where(x => !((IDutyData)x).Tariff.IsEmpty) : entryLine.ChildSecondaryEntryLines;
			foreach (CusEntryLine secondaryTariffLine in childSecondaryEntryLines)
			{
				var secondaryDutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(secondaryTariffLine);
				US7501DocPrinting docPrintingSecondary = US7501DocPrintingData.AddNew();
				secondaryTariffLinesCount++;
				docPrintingSecondary.US_MsgPK = outgoingMsgPK;
				docPrintingSecondary.US_LineNo = entryLine.CL_LineNumber;
				docPrintingSecondary.US_SecondaryLineNo = secondaryTariffLinesCount;

				if (isWatchLineAssembledAbroad)
				{
					if (secondaryTariffLinesCount == 1)
					{
						secondaryTariffLine1 = secondaryTariffLine;
						ZDecimal customsValue = entryLine.CustomsValue.Amount + secondaryTariffLine.CustomsValue.Amount;
						docPrinting.US_SecondaryLine1DutyAmount = ChildLineCalculator(secondaryTariffLine, customsValue).DutyResult.TotalAmount.Amount.Round(2);
					}
					else if (secondaryTariffLinesCount == 2)
					{
						secondaryTariffLine2 = secondaryTariffLine;
					}
					else if (secondaryTariffLinesCount == 3)
					{
						secondaryTariffLine3 = secondaryTariffLine;
						ZDecimal customsValue = secondaryTariffLine2.CustomsValue.Amount + secondaryTariffLine3.CustomsValue.Amount;
						docPrinting.US_SecondaryLine3DutyAmount = ChildLineCalculator(secondaryTariffLine3, customsValue).DutyResult.TotalAmount.Amount.Round(2);
					}
					else if (secondaryTariffLinesCount == 4)
					{
						secondaryTariffLine4 = secondaryTariffLine;
					}
					else if (secondaryTariffLinesCount == 5)
					{
						secondaryTariffLine5 = secondaryTariffLine;
						ZDecimal customsValue = secondaryTariffLine4.CustomsValue.Amount + secondaryTariffLine5.CustomsValue.Amount;
						docPrinting.US_SecondaryLine5DutyAmount = ChildLineCalculator(secondaryTariffLine5, customsValue).DutyResult.TotalAmount.Amount.Round(2);
					}
					else if (secondaryTariffLinesCount == 6)
					{
						secondaryTariffLine6 = secondaryTariffLine;
					}
					else if (secondaryTariffLinesCount == 7)
					{
						secondaryTariffLine7 = secondaryTariffLine;
						ZDecimal customsValue = secondaryTariffLine6.CustomsValue.Amount + secondaryTariffLine7.CustomsValue.Amount;
						docPrinting.US_SecondaryLine7DutyAmount = ChildLineCalculator(secondaryTariffLine7, customsValue).DutyResult.TotalAmount.Amount.Round(2);
					}
				}

				if (secondaryTariffLinesCount == 2 || secondaryTariffLinesCount == 4 || secondaryTariffLinesCount == 6)
				{
					docPrintingSecondary.US_RateAsString = secondaryTariffLine.CL_DutyPercentAsString.Left(US7501DocPrintingAddInfo.Schema.US_RateAsStringMaxLength);
				}
				else
				{
					docPrintingSecondary.US_RateAsString = SecondaryLineDutyPercentAsString(entryLine, secondaryTariffLine, isUSComponentsAssembledAbroad, requiresAdValoremConversionCalculation).Left(docPrintingSecondary.US_RateAsStringInfo.MaxLength);
				}

				if (entryLine.Declaration.US_EntryType == EntryTypeList.Codes.TemporaryImportationBond)
				{
					if (secondaryTariffLine.IsSetVLine)
					{
						docPrintingSecondary.US_RateAsString = ZString.Empty;
						docPrintingSecondary.US_SecondaryLineTIBCalculatedDutyAmount = 0;
					}
					else
					{
						if (secondaryTariffLine.IsCombinedLine())
						{
							var dutyResult = Chapter98Helper.CalculateTIBDutyForPrint(secondaryTariffLine);
							if (dutyResult != null)
							{
								docPrintingSecondary.US_SecondaryLineTIBCalculatedDutyAmount = dutyResult.TotalAmount.Amount;
								docPrintingSecondary.US_RateAsString = dutyResult.RateString;
							}
						}
						else
						{
							var calculator = AppendixFDutyCalculator.NewWithCombinedCustomsValue(secondaryTariffLine, entryLine);
							docPrintingSecondary.US_RateAsString = calculator.DutyResult.RateString;
							docPrintingSecondary.US_SecondaryLineTIBCalculatedDutyAmount = calculator.DutyResult.TotalAmount.Amount;
						}
					}
				}
				else if (CalculateDutyForDocument.ShouldCalculateDutyOnlyForDocumnet(secondaryDutyData))
				{
					var dutyResult = CalculateDutyForDocument.PrinterDuty(secondaryDutyData);
					if (dutyResult != null)
					{
						docPrintingSecondary.US_DutyAmount = dutyResult.TotalAmount.Amount;
						docPrintingSecondary.US_RateAsString = dutyResult.RateString;
						docPrintingSecondary.US_IsAdditionalTotalPrintingDuty = true;
					}
				}

				SetFeeAndTaxRatePercentageStrings(secondaryTariffLine, docPrintingSecondary, taxDeferred);
			}
		}

		bool DoesLineHaveSecondaryWatchLine(CusEntryLine line)
		{
			// Chapter 91 - Clocks and watches and parts thereof
			bool result = false;

			foreach (CusEntryLine childline in line.ChildLines)
			{
				if (childline.IsNormalTariffLine())
				{
					result = childline.CL_AdValoremTariff.StartsWith("91");
					break;
				}
			}

			return result;
		}

		bool IsTariffUSComponentsAssembledAbroad(CusEntryLine line)
		{
			return line.ImportTariff != null && line.ImportTariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, line.DateForDutyCalculation);
		}

		ZBool AdValoremConversionCalculation(CusEntryLine line, bool hasSecondaryWatchLine)
		{
			if (!fAdValoremConversionCalculation.HasValue)
			{
				fAdValoremConversionCalculation = false;

				if (hasSecondaryWatchLine)
				{
					bool applicableSPICountry = false;

					USCCountry countryOfOrigin = line.Factory.LoadTop1<USCCountry>(new ZQuery(USCCountrySchema.UC_Code, ((ICusEntryLine)line).CountryOfOrigin));
					if (countryOfOrigin != null)
					{
						applicableSPICountry = countryOfOrigin.IsValidForSPI(((ICusEntryLine)line).SpecialProgramsIndicatorCountry, line.DateForDutyCalculation);
					}

					if (!applicableSPICountry && line.ImportTariff is USCTariff importTariff && importTariff.Applies(TariffRuleList.Codes.RepairTariffs, line.DateForDutyCalculation))
					{
						fAdValoremConversionCalculation = true;
						ExtractAdValoremDetails(line, importTariff.UE_Tariff);
					}
				}
			}

			return fAdValoremConversionCalculation.Value;
		}
		ZBool? fAdValoremConversionCalculation;

		ZString SecondaryLineDutyPercentAsString(CusEntryLine line, CusEntryLine secondaryLine, bool isUSComponentsAssembledAbroad, bool requiresAdValoremConversionCalculation)
		{
			ZString dutyPercentString = ZString.Empty;

			var isDutyFreeOnSecondaryLine = secondaryLine.CL_DutyPercentAsString.ToUpper() == "FREE";
			if (isUSComponentsAssembledAbroad && line.IsParentLine && !isDutyFreeOnSecondaryLine)
			{
				var dutyData = new DutyDataProxy(secondaryLine);
				dutyData.CustomsValue = line.CustomsValue.Amount;
				dutyPercentString = line.USComponentsAssembledAbroadDutyRateForPrint(dutyData);
			}
			else if (requiresAdValoremConversionCalculation && isDutyFreeOnSecondaryLine)
			{
				dutyPercentString = fAVRateString;
			}
			else
			{
				dutyPercentString = secondaryLine.CL_DutyPercentAsString;
			}

			return dutyPercentString;
		}

		#endregion

		#region AdValoremDetails

		void SetAVConversionDetails(US7501DocPrinting docPrinting)
		{
			docPrinting.US_AVWatches = fAVWatches;
			docPrinting.US_AVCases = fAVCases;
			docPrinting.US_AVBracelets = fAVBracelets;
			docPrinting.US_AVBatteries = fAVBatteries;
			docPrinting.US_AVLine2 = fAVLine2;
			docPrinting.US_AVRateString = fAVRateString;
			docPrinting.US_AVWatchesDuty = fAVWatchesDuty;
			docPrinting.US_AVCasesDuty = fAVCasesDuty;
			docPrinting.US_AVBraceletsDuty = fAVBraceletsDuty;
			docPrinting.US_AVBatteriesDuty = fAVBatteriesDuty;
			docPrinting.US_AVTotalDuty = fAVTotalDuty;
		}

		void ExtractAdValoremDetails(CusEntryLine line, ZString repairTariff)
		{
			fAVWatches = ZString.Empty;
			fAVCases = ZString.Empty;
			fAVBracelets = ZString.Empty;
			fAVBatteries = ZString.Empty;
			fAVLine2 = ZString.Empty;
			fAVRateString = ZString.Empty;
			fAVWatchesDuty = 0;
			fAVCasesDuty = 0;
			fAVBraceletsDuty = 0;
			fAVBatteriesDuty = 0;
			fAVTotalDuty = 0;

			if (line.IsParentLine)
			{
				var childLineNo = ZInt.Zero;
				var customsValue = ZDecimal.Zero;
				var repairCustomsValue = line.CL_CustomsValue;
				var totalEnteredValueForLine = line.CL_CustomsValue;

				foreach (var childLine in line.ChildLines)
				{
					if (childLine.CL_AdValoremTariff == repairTariff || childLine.IsNormalTariffLine())
					{
						totalEnteredValueForLine += childLine.CL_CustomsValue;

						if (childLine.CL_AdValoremTariff != repairTariff)
						{
							customsValue = repairCustomsValue + childLine.CL_CustomsValue;
						}
						else
						{
							repairCustomsValue = childLine.CL_CustomsValue;
						}
					}

					if (childLine.IsNormalTariffLine())
					{
						childLineNo++;

						if (childLineNo == 1)
						{
							AppendixFDutyCalculator childLineCalculator = ChildLineCalculator(childLine, customsValue);
							fAVWatches = childLine.CustomsQuantity.ToString(childLine.CustomsQuantity.DecimalPlaces) + " x $" + childLineCalculator.DutyResult.PerUnitAmount.Amount.ToString(childLineCalculator.DutyResult.PerUnitAmount.Amount.DecimalPlaces) + " " + childLineCalculator.DutyResult.PerUnitUQ;
							fAVWatchesDuty = childLineCalculator.DutyResult.TotalAmount.Amount;
							fAVTotalDuty += fAVWatchesDuty;
						}
						else if (childLineNo == 2)
						{
							AppendixFDutyCalculator childLineCalculator = ChildLineCalculator(childLine, customsValue);
							fAVCases = "$" + customsValue.ToString(customsValue.DecimalPlaces) + " x " + childLineCalculator.DutyResult.PercentOfValue.ToString(childLineCalculator.DutyResult.PercentOfValue.DecimalPlaces) + "%";
							fAVCasesDuty = childLineCalculator.DutyResult.TotalAmount.Amount;
							fAVTotalDuty += fAVCasesDuty;
						}
						else if (childLineNo == 3)
						{
							AppendixFDutyCalculator childLineCalculator = ChildLineCalculator(childLine, customsValue);
							fAVBracelets = "$" + customsValue.ToString(customsValue.DecimalPlaces) + " x " + childLineCalculator.DutyResult.PercentOfValue.ToString(childLineCalculator.DutyResult.PercentOfValue.DecimalPlaces) + "%";
							fAVBraceletsDuty = childLineCalculator.DutyResult.TotalAmount.Amount;
							fAVTotalDuty += fAVBraceletsDuty;
						}
						else if (childLineNo == 4)
						{
							AppendixFDutyCalculator childLineCalculator = ChildLineCalculator(childLine, customsValue);
							fAVBatteries = "$" + customsValue.ToString(customsValue.DecimalPlaces) + " x " + childLineCalculator.DutyResult.PercentOfValue.ToString(childLineCalculator.DutyResult.PercentOfValue.DecimalPlaces) + "%";
							fAVBatteriesDuty = childLineCalculator.DutyResult.TotalAmount.Amount;
							fAVTotalDuty += fAVBatteriesDuty;
						}
					}
				}

				ZDecimal fAVRate = totalEnteredValueForLine > 0 ? (fAVTotalDuty / totalEnteredValueForLine) * 100 : 0;
				fAVRateString = fAVRate.Truncate(3).ToString() + "%";

				fAVLine2 = "$" + fAVTotalDuty + "/$" + totalEnteredValueForLine.ToString(2) + " (Total Entered Value)" + " = " + fAVRateString;
			}
		}

		public ZString AVWatches { get { return fAVWatches; } }
		public ZDecimal AVWatchesDuty { get { return fAVWatchesDuty; } }
		public ZString AVCases { get { return fAVCases; } }
		public ZDecimal AVCasesDuty { get { return fAVCasesDuty; } }
		public ZString AVBracelets { get { return fAVBracelets; } }
		public ZDecimal AVBraceletsDuty { get { return fAVBraceletsDuty; } }
		public ZString AVBatteries { get { return fAVBatteries; } }
		public ZDecimal AVBatteriesDuty { get { return fAVBatteriesDuty; } }
		public ZDecimal AVTotalDuty { get { return fAVTotalDuty; } }
		public ZString AVLine2 { get { return fAVLine2; } }

		ZString fAVWatches;
		ZDecimal fAVWatchesDuty;
		ZString fAVCases;
		ZDecimal fAVCasesDuty;
		ZString fAVBracelets;
		ZDecimal fAVBraceletsDuty;
		ZString fAVBatteries;
		ZDecimal fAVBatteriesDuty;
		ZDecimal fAVTotalDuty;
		ZString fAVLine2;
		ZString fAVRateString;

		AppendixFDutyCalculator ChildLineCalculator(CusEntryLine childLine, ZDecimal customsValue)
		{
			var dutyData = new DutyDataProxy(childLine);
			dutyData.CustomsValue = customsValue;
			return new AppendixFDutyCalculator(dutyData, childLine.Factory);
		}

		#endregion

		#region ProRated Calculations

		void SetProRatedDetails(CusEntryLine entryLine, US7501DocPrinting docPrinting)
		{
			ZDecimal totalValueForLine = TotalValueForLine(entryLine);
			ZDecimal proRatedPercent = ProRatedPercent(entryLine, totalValueForLine);
			ZDecimal proRatedDuty = ProRatedDuty(entryLine, proRatedPercent).Round(2);
			docPrinting.US_ProRatedLine1 = entryLine.FormattedTariff + " (Free) " + ComponentValue(entryLine) + " / " + totalValueForLine + " (Total Value) = " + proRatedPercent + "%";
			docPrinting.US_ProRatedLine2 = proRatedPercent + "% x $" + TotalComponentDuty(entryLine) + " (Total Duty Column 34) = $" + proRatedDuty;
			docPrinting.US_ProRatedLine3 = "$" + TotalComponentDuty(entryLine) + " - $" + proRatedDuty + " = $" + (TotalComponentDuty(entryLine) - proRatedDuty).ToString() + " (Total Duty Due)";
		}

		ZDecimal TotalValueForLine(CusEntryLine line)
		{
			ZDecimal result = line.RoundedCustomsValue;

			foreach (CusEntryLine childLine in line.ChildLines)
			{
				result = result + childLine.RoundedCustomsValue;
			}

			return result.Round(0);
		}

		ZDecimal ComponentValue(CusEntryLine line)
		{
			var result = line.CustomsValueRounded;
			int childLineNo = 0;

			foreach (CusEntryLine childLine in line.ChildLines)
			{
				childLineNo++;
				if (childLineNo == 2 || childLineNo == 4 || childLineNo == 6)
				{
					result = result + childLine.CustomsValueRounded;
				}
			}

			return result;
		}

		ZDecimal ProRatedPercent(CusEntryLine line, ZDecimal totalValueForLine)
		{
			ZDecimal result = ZDecimal.Zero;

			if (totalValueForLine > 0)
			{
				result = (ComponentValue(line) / totalValueForLine) * 100;
			}

			return result.Round(3);
		}

		ZDecimal TotalComponentDuty(CusEntryLine line)
		{
			ZDecimal result = ZDecimal.Zero;

			if (line.IsParentLine)
			{
				int childLineNo = 0;
				ZDecimal customsValue = 0;

				foreach (CusEntryLine childLine in line.ChildLines)
				{
					childLineNo++;
					if (childLineNo == 1)
					{
						customsValue = line.CustomsValue.Amount + childLine.CustomsValue.Amount;
					}
					else if (childLineNo == 2 || childLineNo == 4 || childLineNo == 6)
					{
						customsValue = childLine.CustomsValue.Amount;
					}
					else if (childLineNo == 3 || childLineNo == 5 || childLineNo == 7)
					{
						customsValue += childLine.CustomsValue.Amount;
					}

					if (childLineNo == 1 || childLineNo == 3 || childLineNo == 5 || childLineNo == 7)
					{
						var dutyData = new DutyDataProxy(childLine);
						dutyData.CustomsValue = customsValue;
						var childLineCalculator = new AppendixFDutyCalculator(dutyData, line.Factory);
						result += childLineCalculator.DutyResult.TotalAmount.Amount.Round(2);
					}
				}
			}

			return result;
		}

		ZDecimal ProRatedDuty(CusEntryLine line, ZDecimal proRatedPercent)
		{
			return proRatedPercent * TotalComponentDuty(line) / 100;
		}

		#endregion

		#endregion

		#region 7501 Message to print

		public StmNoteCollection MessageToPrintOn7501
		{
			get
			{
				IStmNoteParent messageToPrintParent;
				if (Declaration.Shipment != null)
				{
					messageToPrintParent = Declaration.Shipment;
				}
				else
				{
					messageToPrintParent = Declaration;
				}

				StmNoteCollection customsMessageToPrintOn7501 = new StmNoteCollection(messageToPrintParent, Factory);
				Declaration.RegisterEditableChildObject(customsMessageToPrintOn7501);
				ZQuery filter = new ZQuery(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.CustomsMessageToPrintOn7501.Description);
				customsMessageToPrintOn7501.LoadWithMoreFiltering(filter);
				return customsMessageToPrintOn7501;
			}
		}

		#endregion

		#endregion

		#region 3461 Fields

		public ZDateTime Box1_ArrivalDate
		{
			get
			{
				var result = ZDateTime.Empty;
				var declaration = Declaration;
				if (declaration != null)
				{
					if (declaration.JE_PrimaryITNumber.IsEmpty)
					{
						result = declaration.JE_DateOfArrival;
					}
					else
					{
						result = declaration.US_EntryDate;
					}
				}
				return result;
			}
		}

		public ZString Box8_ConsigneeNo
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.HasMultipleConsignees ? (ZString)USConstants.MultipleValueIndicator : declaration.UltimateConsigneeCustomsClientNumberForDocument : ZString.Empty;
			}
		}

		public ZString Box10_ConsigneeName
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;
				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;

				if (declaration != null && !declaration.HasMultipleConsignees && ultConsigneeAddressDetails != null)
				{
					result = ultConsigneeAddressDetails.CompanyName;
				}

				return result;
			}
		}

		public ZString Box10_ConsigneeAddressLine1
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;
				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;

				if (declaration != null && !declaration.HasMultipleConsignees && ultConsigneeAddressDetails != null)
				{
					result = ultConsigneeAddressDetails.AddressLine1;
				}

				return result;
			}
		}

		public ZString Box10_ConsigneeAddressLine2
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;
				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;

				if (declaration != null && !declaration.HasMultipleConsignees && ultConsigneeAddressDetails != null)
				{
					result = ultConsigneeAddressDetails.AddressLine2.IsEmpty ? EUCCityStatePostCodeCountry : ultConsigneeAddressDetails.AddressLine2;
				}

				return result;
			}
		}

		public ZString Box10_ConsigneeCityStatePostCodeCountry
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;
				var ultConsigneeAddressDetails = EffectiveUltimateConsigneeWrapperAddressDetails;

				if (declaration != null && !declaration.HasMultipleConsignees && ultConsigneeAddressDetails != null && !ultConsigneeAddressDetails.AddressLine2.IsEmpty)
				{
					result = EUCCityStatePostCodeCountry;
				}

				return result;
			}
		}

		public ZBool ShouldPrintBrokerSignature
		{
			get { return BrokerProvider.ShouldPrintBrokerSignature; }
		}

		public Image BrokerSignature3461
		{
			get { return BrokerProvider.BrokerSignature; }
		}

		public Image BrokerSignature3461ElectronicRelease
		{
			get { return BrokerProvider.BrokerSignatureForElectronicRelease; }
		}

		internal GlbStaff Signatory
		{
			get { return BrokerProvider.Signatory; }
		}

		PrintBrokerSignatureProvider BrokerProvider
		{
			get { return fBrokerProvider ?? (fBrokerProvider = new PrintBrokerSignatureProvider(this)); }
		}
		PrintBrokerSignatureProvider fBrokerProvider;

		#endregion

		#region 3461 ACE Cargo Release Fields

		ZString ImporterNumberType
		{
			get { return ACECargoReleaseData.GetCustomsNumberType(ImporterOfRecordNumber); }
		}

		public ZString Box3IsIRS
		{
			get { return ImporterNumberType == EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber ? "X" : ""; }
		}

		public ZString Box3IsSSN
		{
			get { return (Declaration?.PrintSocialSecurityNumberOnDocument ?? false) ? ImporterNumberType == EntityIdentifierQualifierList.Codes.SocialSecurityNumber ? "X" : "" : ""; }
		}

		public ZString Box3IsCBP
		{
			get { return ImporterNumberType == EntityIdentifierQualifierList.Codes.CBPAssignedNumber ? "X" : ""; }
		}

		public ZString IsSingleTransBond
		{
			get { return Declaration != null ? Declaration.US_BondType == BondTypeList.Codes.SingleTransactionBond ? "X" : "" : ""; }
		}

		public ZString IsContinuousBond
		{
			get { return Declaration != null ? Declaration.US_BondType == BondTypeList.Codes.ContinuousBond ? "X" : "" : ""; }
		}

		public ZString IsNoBondRequired
		{
			get { return Declaration != null ? Declaration.US_BondType == ZString.Empty ? "X" : "" : ""; }
		}

		public ZString Box14LocationOfGoods
		{
			get { return ((ICusEntryHeader)this).LocationOfGoods; }
		}

		public BusinessObjectCollectionWrapper<PartyDocWrapper> Parties_3461
		{
			get
			{
				var parties = new List<PartyDocWrapper>();
				foreach (var entity in Entities_3461)
				{
					parties.Add(new PartyDocWrapper(entity, Declaration?.PrintSocialSecurityNumberOnDocument ?? false));
				}
				return new BusinessObjectCollectionWrapper<PartyDocWrapper>(parties);
			}
		}

		public IEnumerable<ISimplifiedEntryOrganisationDetails> Entities_3461
		{
			get
			{
				var result = new List<ISimplifiedEntryOrganisationDetails>();

				var firstEntity = Declaration.JE_OA_ManufacturerAddress;
				var isTheSamePartyForAllInvoices = InvoiceHeaders.SelectMany(x => x.InvoiceLines).Cast<JobComInvoiceLine>().All(x => x.JI_OA_ManufacturerAddress == firstEntity);
				if (isTheSamePartyForAllInvoices)
				{
					ACECargoReleaseData.AddEntity(result, Declaration.JE_OA_ManufacturerAddressInfo, EntityCodeList.Codes.ManufacturerSupplier);
				}

				firstEntity = Declaration.JE_OA_ConsigneeAddress;
				isTheSamePartyForAllInvoices = InvoiceHeaders.SelectMany(x => x.InvoiceLines).Cast<JobComInvoiceLine>().All(x => x.JI_OA_ConsigneeAddress == firstEntity);
				if (isTheSamePartyForAllInvoices && Declaration.ConsigneeAddress != null)
				{
					ACECargoReleaseData.AddEntity(result, Declaration.JE_OA_ConsigneeAddressInfo, EntityCodeList.Codes.Consignee);
				}

				firstEntity = Declaration.JE_OA_SoldToPartyAddress;
				isTheSamePartyForAllInvoices = InvoiceHeaders.SelectMany(x => x.InvoiceLines).Cast<JobComInvoiceLine>().All(x => x.JI_OA_SoldToPartyAddress == firstEntity);
				if (isTheSamePartyForAllInvoices && Declaration.SoldToPartyAddress != null)
				{
					ACECargoReleaseData.AddEntity(result, Declaration.JE_OA_SoldToPartyAddressInfo, EntityCodeList.Codes.BuyingParty);
				}

				firstEntity = Declaration.JE_OA_SellerAddress;
				isTheSamePartyForAllInvoices = InvoiceHeaders.SelectMany(x => x.InvoiceLines).Cast<JobComInvoiceLine>().All(x => x.JI_OA_Seller == firstEntity);
				if (isTheSamePartyForAllInvoices && Declaration.SellerAddress != null)
				{
					ACECargoReleaseData.AddEntity(result, Declaration.JE_OA_SellerAddressInfo, EntityCodeList.Codes.SellingParty);
				}

				return result;
			}
		}

		public ZString IsAir
		{
			get { return Declaration != null ? Declaration.IsAir ? "X" : "" : ""; }
		}

		public ZString IsSea
		{
			get { return Declaration != null ? Declaration.IsSea ? "X" : "" : ""; }
		}

		public ZString IsRail
		{
			get { return Declaration != null ? Declaration.IsRail ? "X" : "" : ""; }
		}

		public ZString IsTruck
		{
			get { return Declaration != null ? Declaration.IsTruck ? "X" : "" : ""; }
		}

		public ZString IsHandCarry
		{
			get { return Declaration != null ? Declaration.IsHandCarry ? "X" : "" : ""; }
		}

		public ZString IsPipeline
		{
			get { return Declaration != null ? Declaration.IsFixedTransportInstallations ? "X" : "" : ""; }
		}

		public ZString IsOther
		{
			get
			{
				var result = ZString.Empty;
				if (Declaration != null && !Declaration.IsAir && !Declaration.IsSea && !Declaration.IsRail && !Declaration.IsTruck && !Declaration.IsHandCarry && !Declaration.IsFixedTransportInstallations)
				{
					result = "X";
				}
				return result;
			}
		}

		public ZString Voyage_3461
		{
			get { return ((ICusEntryHeader)this).VoyageNumber; }
		}

		public ZString Conveyance_3461
		{
			get
			{
				if (Declaration != null && Declaration.IsSea)
				{
					return Declaration.VesselName;
				}
				else if (Declaration != null && Declaration.IsConsumptionFTZ)
				{
					return "FTZ" + Declaration.JE_MasterBill;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public BusinessObjectCollectionWrapper<NonABI3461Page> NonABI3461Pages
		{
			get
			{
				if (nonABI3461Pages == null)
				{
					var pages = new List<NonABI3461Page>();
					ZInt pageCount;
					ZInt pageCountForEntryLine = EntryLines_3461.Count / NonABI3461Page.EntryLineCountMax;
					if (EntryLines_3461.Count % NonABI3461Page.EntryLineCountMax != 0)
					{
						pageCountForEntryLine += 1;
					}
					ZInt pageCountForBill = Bills_3461.Count / NonABI3461Page.BillCountMax;
					if (Bills_3461.Count % NonABI3461Page.BillCountMax != 0)
					{
						pageCountForBill += 1;
					}
					if (pageCountForEntryLine > pageCountForBill)
					{
						pageCount = pageCountForEntryLine;
					}
					else
					{
						pageCount = pageCountForBill;
					}

					if (pageCount > 0)
					{
						for (int i = 0; i < pageCount; i++)
						{
							var newPage = new NonABI3461Page();
							pages.Add(newPage);
							for (int j = 0; j < NonABI3461Page.EntryLineCountMax; j++)
							{
								var index = i * NonABI3461Page.EntryLineCountMax + j;
								if (index < EntryLines_3461.Count)
								{
									newPage.AddNewEntryLine(EntryLines_3461[index]);
								}
								else
								{
									break;
								}
							}

							for (int k = 0; k < NonABI3461Page.BillCountMax; k++)
							{
								var index = i * NonABI3461Page.BillCountMax + k;
								if (index < Bills_3461.Count)
								{
									newPage.AddNewBill(Bills_3461[index]);
								}
								else
								{
									break;
								}
							}
						}
					}
					nonABI3461Pages = new BusinessObjectCollectionWrapper<NonABI3461Page>(pages);
				}
				return nonABI3461Pages;
			}
		}
		BusinessObjectCollectionWrapper<NonABI3461Page> nonABI3461Pages;

		public void ResetDocumentDataCache()
		{
			bills_3461 = null;
			entryLines_3461 = null;
			nonABI3461Pages = null;
		}

#if DEBUG
		internal
#endif
		BusinessObjectCollectionWrapper<BillDocWrapper> Bills_3461
		{
			get
			{
				if (bills_3461 == null || (Declaration != null && bills_3461.Count != Declaration.Bills.Count))
				{
					var list = new List<BillDocWrapper>();
					foreach (Bill bill in Declaration.FilteredBills)
					{
						if (bill.IsMasterBill)
						{
							if (bill.ChildBills.Count > 0)
							{
								list.Add(new BillDocWrapper(bill, BillDocWrapper.MasterBill));
							}
							else
							{
								list.Add(new BillDocWrapper(bill, BillDocWrapper.RegularBill));
							}
						}
						if (bill.IsHouseBill)
						{
							list.Add(new BillDocWrapper(bill, BillDocWrapper.HouseBill));
						}
						if (bill.ITNumber != ZString.Empty)
						{
							list.Add(new BillDocWrapper(bill, BillDocWrapper.InBondBill));
						}
					}
					bills_3461 = new BusinessObjectCollectionWrapper<BillDocWrapper>(list);
				}
				return bills_3461;
			}
		}

		BusinessObjectCollectionWrapper<BillDocWrapper> bills_3461;

#if DEBUG
		internal
#endif
		BusinessObjectCollectionWrapper<EntryLineDocWrapper> EntryLines_3461
		{
			get
			{
				if (entryLines_3461 == null || entryLines_3461.Count != EntryLines.Count())
				{
					var list = new List<EntryLineDocWrapper>();
					foreach (var entryLine in EntryLines)
					{
						list.Add(new EntryLineDocWrapper(entryLine));
					}
					entryLines_3461 = new BusinessObjectCollectionWrapper<EntryLineDocWrapper>(list);
				}
				return entryLines_3461;
			}
		}
		BusinessObjectCollectionWrapper<EntryLineDocWrapper> entryLines_3461;

		#endregion
	}
}
