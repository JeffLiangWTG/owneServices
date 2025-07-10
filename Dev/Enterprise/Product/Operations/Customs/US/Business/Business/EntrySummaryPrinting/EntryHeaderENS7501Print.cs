using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class EntryHeaderENS7501Print<TLine> : EntrySummary7501Print
		where TLine : EntryHeaderENS7501Line
	{
		public EntryHeaderENS7501Print(CusEntryHeader entry, Func<CusEntryLine, bool, bool, TLine> createNew7501Line)
			: base(entry)
		{
			DetermineAdditionalLineSectionPrintingFlags();
			this.createNew7501Line = createNew7501Line;
		}

		readonly Func<CusEntryLine, bool, bool, TLine> createNew7501Line;

		#region Overriden Document Properties

		void DetermineAdditionalLineSectionPrintingFlags()
		{
			foreach (CusEntryLine entryLine in entry.MergedLines)
			{
				if (entryLine.IsRandomLineSPINotCAAndS &&
					HasSecondaryWatchLine(entryLine) && entryLine.ImportTariff != null &&
					entryLine.ImportTariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, entryLine.DateForDutyCalculation))
				{
					entryHasProRatedCalculation = true;
				}

				if (HasSecondaryWatchLine(entryLine))
				{
					bool applicableSPICountry = false;

					var countryOfOrigin = entryLine.Factory.LoadTop1<USCCountry>(new ZQuery(USCCountrySchema.UC_Code, ((ICusEntryLine)entryLine).CountryOfOrigin));
					if (countryOfOrigin != null)
					{
						applicableSPICountry = countryOfOrigin.IsValidForSPI(((ICusEntryLine)entryLine).SpecialProgramsIndicatorCountry, entryLine.DateForDutyCalculation);
					}

					if (!applicableSPICountry && entryLine.ImportTariff != null && entryLine.ImportTariff.Applies(TariffRuleList.Codes.RepairTariffs, entryLine.DateForDutyCalculation))
					{
						entryHasAdValoremConversionCalculation = true;
					}
				}

				if (!entryLine.ADDNo.IsEmpty || !entryLine.CVDNo.IsEmpty || !entryLine.SecondCustomsQuantity.IsEmpty)
				{
					entryHasADDCVDLines = true;
				}

				if (entryLine.IsSecondaryTariffLine)
				{
					entryHasSecondaryTariffLines = true;
				}

				if (entryLine.ChildLines.Count > 1)
				{
					entryHasAdditionalTariffLines = true;
				}
			}
		}

		bool HasSecondaryWatchLine(CusEntryLine line)
		{
			bool result = false;

			foreach (var childline in line.ChildLines)
			{
				if (childline.IsNormalTariffLine())
				{
					result = childline.CL_AdValoremTariff.StartsWith("91");
					break;
				}
			}

			return result;
		}

		protected override bool IsACE
		{
			get { return entry.Declaration.IsACE; }
		}

		public override ZString FormattedEntryNumber
		{
			get { return EntryFilerCode + "-" + entry.EntryNumber.SubstringSafe(0, 7) + "-" + entry.EntryNumber.SubstringSafe(7, 1); }
		}

		public override ZString EntryType
		{
			get { return entry.EntryType; }
		}

		public override ZString EntryTypeCode
		{
			get { return entry.Declaration.US_LiveEntryIndicator == YesNoDefaultList.Codes.Yes ? "LIVE" : ""; }
		}

		public override ZString USTeamNo
		{
			get { return Declaration.US_TeamNo; }
		}

		public override ZString SummaryStatus
		{
			get { return ZString.Empty; }
		}

		public override ZBool DecFinalWithdrawal
		{
			get { return Declaration != null ? Declaration.US_IsFinalWHS : ZBool.False; }
		}

		public override ZBool TaxToBeDeferred
		{
			get { return Declaration.TaxToBeDeferred; }
		}

		public override ZBool DeferredTaxToBePaidByEFT
		{
			get { return Declaration.DeferredTaxToBePaidByEFT; }
		}

		public override ZString SuretyCode
		{
			get { return Declaration.US_SuretyCode; }
		}

		public override ZString BondType
		{
			get { return Declaration.US_BondType; }
		}

		public override ZString SchDEntry
		{
			get { return Declaration.US_SchDEntry; }
		}

		protected override bool IsSeaTransportMode
		{
			get { return Declaration.IsSea; }
		}

		protected override bool IsAirOrHandCarry
		{
			get { return Declaration.IsAir || Declaration.IsHandCarry; }
		}

		protected override ZString VesselName
		{
			get { return Declaration.JE_VesselName; }
		}

		protected override ZString PipelineName
		{
			get { return Declaration.US_PipelineName; }
		}

		protected override ZString CarrierCode
		{
			get { return entry.CarrierCode; }
		}

		protected override ZString FTZNumber
		{
			get { return entry.ImportFTZNumber; }
		}

		public override ZString USTransportMode
		{
			get { return IsConsumptionFTZ ? ZString.Empty : Declaration.JE_Calc_USTransportMode; }
		}

		public override ZString UniqueCountryOfOrigin
		{
			get { return entry.UniqueCountryOfOrigin; }
		}

		protected override bool HasMultiExportDates
		{
			get { return entry.HasMultiExportDates; }
		}

		protected override ZDate ExportDateFromFirstLine
		{
			get { return entry.MergedLines.Count > 0 ? entry.MergedLines[0].ExportDate : ZDate.Empty; }
		}

		public override ZString SCACAndMBillNumber
		{
			get { return Declaration != null ? (Declaration.IsAir ? Declaration.JE_MasterBill : Declaration.JE_MasterBillIssuerSCAC + Declaration.JE_MasterBill) : ""; }
		}

		public override ZString ManufacturerID
		{
			get
			{
				if (!manufacturerID.HasValue)
				{
					manufacturerID = UniqueValueCalculator.GetUniqueValue(entry.MergedLines.Cast<ICusEntryLine>(), x => x.ManufacturerSupplierCode, (ZString)USConstants.MultipleValueIndicator);
				}
				return manufacturerID.Value;
			}
		}
		ZString? manufacturerID;

		public override ZString UniqueCountryOfExport
		{
			get
			{
				if (!uniqueCountryOfExportCached.HasValue)
				{
					uniqueCountryOfExportCached = ZString.Empty;
					if (IsConsumptionFTZ)
					{
						var sortedLines = entry.MergedLines.OfType<CusEntryLine>().ToList();
						sortedLines.Sort((x, y) => x.CL_CustomsValue.CompareTo(y.CL_CustomsValue));

						var entryLineWithHighestValue = sortedLines.LastOrDefault();

						uniqueCountryOfExportCached = entryLineWithHighestValue != null ? entryLineWithHighestValue.RandomLine.US_UC_NKCountryOfExport : ZString.Empty;
					}
					else
					{
						uniqueCountryOfExportCached = entry.UniqueCountryOfExport;
					}
				}
				return uniqueCountryOfExportCached.Value;
			}
		}
		ZString? uniqueCountryOfExportCached;

		public override ZString FirstBillITNO
		{
			get
			{
				var result = ZString.Empty;

				if (!IsConsumptionFTZ && Declaration.ITNumbersFromBills.Count > 0)
				{
					result = Declaration.ITNumbersFromBills[0];
				}
				return result;
			}
		}

		public override ZString MissingDoc1
		{
			get { return IsConsumptionFTZ ? ZString.Empty : Declaration.US_MissingDocument1.IsEmpty ? ZString.Empty : Declaration.US_MissingDocument1; }
		}

		public override ZString MissingDoc2
		{
			get { return IsConsumptionFTZ ? ZString.Empty : Declaration.US_MissingDocument2.IsEmpty ? ZString.Empty : Declaration.US_MissingDocument2; }
		}

		public override ZString UniquePortOfLading
		{
			get { return !IsConsumptionFTZ && Declaration.IsSea ? entry.UniquePortOfLading : ZString.Empty; }
		}

		public override ZString SchDArrival
		{
			get { return IsConsumptionFTZ ? ZString.Empty : Declaration.US_SchDArrival; }
		}

		protected override ZString LocationOfGoodsAndNameCore
		{
			get
			{
				return CachedValueHelper.GetValue(
					ref cachedLocationOfGoodsAndName,
					() =>
					{
						var result = base.LocationOfGoodsAndNameCore;

						if (result.IsEmpty)
						{
							result = ((ICusEntryHeader)entry).LocationOfGoods;
							var firms = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, result, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);
							result += firms != null ? ("/" + firms.ZZD_Description) : string.Empty;
						}

						if (result.IsEmpty && Declaration.IsAir)
						{
							result = Declaration.JE_VoyageFlightNo;
						}

						return result;
					});
			}
		}

		CachedValue<ZString> cachedLocationOfGoodsAndName;

		protected override ZString EffectiveUltimateConsigneeCustomsRegNoCore
		{
			get
			{
				var result = ZString.Empty;

				if (entry.ImportUltimateConsignee != null && entry.ImporterOfRecord != null)
				{
					if (!IsConsumptionFTZ && entry.ImportUltimateConsignee.OH_Code == entry.ImporterOfRecord.OH_Code)
					{
						result = USConstants.Same;
					}
					else
					{
						result = Declaration.UltimateConsigneeCustomsClientNumber;
					}
				}

				return result;
			}
		}

		protected override ZString ImporterOfRecordCustomsRegNoCore
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(entry.ImporterOfRecord, OrgMatchedCustomsRegNoType.EIN); }
		}

		protected override ZString Block24ReferenceNumberCore
		{
			get { return Declaration.CBPF4811ReferenceNumber; }
		}

		bool ConsigneeIsImporter
		{
			get
			{
				if (!fConsigneeIsImporter.HasValue)
				{
					fConsigneeIsImporter = (EffectiveUltimateConsigneeCustomsRegNo == USConstants.Same);
				}

				return fConsigneeIsImporter.Value;
			}
		}
		ZBool? fConsigneeIsImporter;

		public override ZString EffectiveUltimateConsigneeCompanyName
		{
			get
			{
				return ConsigneeIsImporter ?
						  ZString.Empty :
						  (entry.EffectiveUltimateConsigneeWrapperAddressDetails != null ?
								  entry.EffectiveUltimateConsigneeWrapperAddressDetails.CompanyName :
								  ZString.Empty);
			}
		}

		public override ZString EffectiveUltimateConsigneeAddressLine1
		{
			get { return ConsigneeIsImporter ? ZString.Empty : entry.EffectiveUltimateConsigneeWrapperAddressDetails != null ? entry.EffectiveUltimateConsigneeWrapperAddressDetails.AddressLine1 : ZString.Empty; }
		}

		public override ZString EffectiveUltimateConsigneeAddressLine2
		{
			get { return ConsigneeIsImporter ? ZString.Empty : entry.EffectiveUltimateConsigneeWrapperAddressDetails != null ? entry.EffectiveUltimateConsigneeWrapperAddressDetails.AddressLine2 : ZString.Empty; }
		}

		public override ZString EffectiveUltimateConsigneeCity
		{
			get { return ConsigneeIsImporter ? ZString.Empty : entry.EffectiveUltimateConsigneeWrapperAddressDetails != null ? entry.EffectiveUltimateConsigneeWrapperAddressDetails.City : ZString.Empty; }
		}

		public override ZString EffectiveUltimateConsigneeState
		{
			get
			{
				return ConsigneeIsImporter ? entry.US_DestinationState : entry.EffectiveUltimateConsigneeWrapperAddressDetails != null ? entry.EffectiveUltimateConsigneeWrapperAddressDetails.State : ZString.Empty;
			}
		}

		public override ZString EffectiveUltimateConsigneePostCode
		{
			get { return ConsigneeIsImporter ? ZString.Empty : entry.EffectiveUltimateConsigneeWrapperAddressDetails != null ? entry.EffectiveUltimateConsigneeWrapperAddressDetails.PostCode : ZString.Empty; }
		}

		public override ZString UltimateState
		{
			get
			{
				ZString result = ZString.Empty;
				if (entry.US_DestinationState != EffectiveUltimateConsigneeState)
				{
					result = entry.US_DestinationState;
				}

				return result;
			}
		}

		protected override ZString WarehouseEntryNoForWarehouseEntryWithdrawal
		{
			get
			{
				ZString result = ZString.Empty;
				ZString warehouseNo = entry.Declaration.US_WHSEntryNumber;

				if (entry.IsExWarehouseEntryType && !warehouseNo.IsEmpty)
				{
					result = entry.Declaration.US_WHSEntryFilerCode + "-" + warehouseNo.SubstringSafe(0, 7) + "-" + warehouseNo.SubstringSafe(7, 1);
				}

				return result;
			}
		}

		public override ZString ImporterCompanyName
		{
			get { return entry.ImporterWrapperAddressDetails != null ? entry.ImporterWrapperAddressDetails.CompanyName : ZString.Empty; }
		}

		public override ZString ImporterAddressLine1
		{
			get { return entry.ImporterWrapperAddressDetails != null ? entry.ImporterWrapperAddressDetails.AddressLine1 : ZString.Empty; }
		}

		public override ZString ImporterAddressLine2
		{
			get { return entry.ImporterWrapperAddressDetails != null ? entry.ImporterWrapperAddressDetails.AddressLine2 : ZString.Empty; }
		}

		public override ZString ImporterCity
		{
			get { return entry.ImporterWrapperAddressDetails != null ? entry.ImporterWrapperAddressDetails.City : ZString.Empty; }
		}

		public override ZString ImporterState
		{
			get { return entry.ImporterWrapperAddressDetails != null ? entry.ImporterWrapperAddressDetails.State : ZString.Empty; }
		}

		public override ZString ImporterPostCode
		{
			get { return entry.ImporterWrapperAddressDetails != null ? entry.ImporterWrapperAddressDetails.PostCode : ZString.Empty; }
		}

		public override ZString EntryFilerCode
		{
			get { return Declaration != null ? Declaration.US_EntryFilerCode : ZString.Empty; }
		}

		public override ZString EntryNo
		{
			get { return entry.EntryNumber; }
		}

		public override List<IFee> Fees
		{
			get
			{
				if (fees == null)
				{
					fees = new List<IFee>();
					if (HMFDeMinimis)
					{
						AddFee(Core.Constants.USCustoms.FeeCodes.HMF, 0m);
					}

					if (TotalAntidumpingDutyAmountPayable > 0)
					{
						AddFee("012", TotalAntidumpingDutyAmountPayable);
					}

					if (TotalCountervailingDutyPayable > 0)
					{
						AddFee("013", TotalCountervailingDutyPayable);
					}

					foreach (CusEntryHeaderCharges charge in NonExcisableCharges)
					{
						AddFee(charge.C1_ChargeType, charge.C1_ChargeAmount);
					}

					fees.Sort((fee1, fee2) => string.Compare(fee1.Code, fee2.Code));
				}

				return fees;
			}
		}

		public override ZString SummaryBlockOverflow
		{
			get
			{
				if (!summaryBlockOverflowCached.HasValue)
				{
					int specialChargeCategories = 0;
					if (TotalAntidumpingDutyAmountPayable > 0)
					{ specialChargeCategories++; }
					if (TotalCountervailingDutyPayable > 0)
					{ specialChargeCategories++; }

					summaryBlockOverflowCached = (NonExcisableCharges.Count + specialChargeCategories) > 4 ? "+" : "";
				}

				return summaryBlockOverflowCached.Value;
			}
		}
		ZString? summaryBlockOverflowCached;

		public override ZDateTime EntrySummaryFiledDate
		{
			get { return ZDateTime.Empty; }
		}

		public override ZDateTime EstimatedEntryDate
		{
			get { return Declaration != null ? Declaration.US_EstimatedEntryDate.Date : ZDateTime.Empty; }
		}

		public override ZDateTime FirstBillITDate
		{
			get { return IsConsumptionFTZ ? ZDateTime.Empty : Declaration.US_ITDate.Date; }
		}

		public override ZDateTime DateOfFirstArrival
		{
			get { return Declaration.JE_DateOfArrival; }
		}

		public override ZDecimal TotalEnteredValue
		{
			get { return entry.TotalEnteredValue; }
		}

		protected override ZDecimal TotalLineLevelHMFs
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					result += entryLine.HMFAmount;
				}

				return result;
			}
		}

		protected override ZDecimal EntryHMF
		{
			get { return entry.HMFAmountForEntry; }
		}

		public override ZDecimal TotalOther
		{
			get
			{
				ZDecimal otherFeeSummaryTotal = 0;

				if (!EntryTypeList.IsNothingPayable(EntryType))
				{
					if (EntryTypeList.IsOnlyHMFPayable(EntryType) || EntryTypeList.IsTIB(EntryType)) // CS00113346 - For Warehouse entry, only HMF is payable on entry & should be included in box 39.
					{
						otherFeeSummaryTotal += entry.HMFAmountForEntry;
					}
					else
					{
						foreach (CusEntryHeaderCharges hdrCharges in NonExcisableCharges)
						{
							otherFeeSummaryTotal += hdrCharges.C1_ChargeAmount;
						}

						otherFeeSummaryTotal += TotalAntidumpingDutyAmountPayable;
						otherFeeSummaryTotal += TotalCountervailingDutyPayable;
					}
				}

				return otherFeeSummaryTotal.Round(2);
			}
		}

		public override ZDecimal TotalAntidumpingDutyAmountPayable
		{
			get
			{
				if (!totalAntidumpingDutyAmountPayable.HasValue)
				{
					totalAntidumpingDutyAmountPayable = 0m;
					foreach (ICusEntryLine line in entry.MergedLines)
					{
						CusEntryLine cusline = (CusEntryLine)line;
						if (!cusline.RandomLine.US_ADDCaseNo.IsEmpty && (!cusline.IsCombinedLine() || cusline.IsNormalTariffLine()))
						{
							if (!line.BondedAntidumpingDuty)
							{
								totalAntidumpingDutyAmountPayable += line.AntidumpingDuty;
							}
						}
					}
				}
				return totalAntidumpingDutyAmountPayable.Value;
			}
		}
		ZDecimal? totalAntidumpingDutyAmountPayable;

		public override ZDecimal TotalCountervailingDutyPayable
		{
			get
			{
				if (!totalCountervailingDutyPayable.HasValue)
				{
					totalCountervailingDutyPayable = 0m;
					foreach (ICusEntryLine line in entry.MergedLines)
					{
						CusEntryLine cusline = (CusEntryLine)line;
						if (!cusline.RandomLine.US_CVDCaseNo.IsEmpty && (!cusline.IsCombinedLine() || cusline.IsNormalTariffLine()))
						{
							if (!line.BondedCountervailingDuty)
							{
								totalCountervailingDutyPayable += line.CountervailingDuty;
							}
						}
					}
				}
				return totalCountervailingDutyPayable.Value;
			}
		}
		ZDecimal? totalCountervailingDutyPayable;

		// CS00113346 - For Warehouse entry, Duty is not payable on entry. It should be shown on entry lines but not included in the box 37 totals.
		public override ZDecimal TotalDutyAmt
		{
			get { return EntryTypeList.IsOnlyHMFPayable(EntryType) || IsTIBEntry || EntryTypeList.IsNothingPayable(EntryType) ? ZDecimal.Zero : (ZDecimal)(entry.TotalDutyAmount + AdditionalTotalPrintingDuty); }
		}

		public override ZDecimal AdditionalTotalPrintingDuty
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (CusEntryLine line in entry.MergedLines)
				{
					var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(line);
					if (line.DutyAmount.IsEmpty && CalculateDutyForDocument.ShouldCalculateDutyOnlyForDocumnet(dutyData))
					{
						result += CalculateDutyForDocument.PrinterDuty(dutyData).TotalAmount.Amount;
					}
				}
				return result;
			}
		}

		public override ZDecimal TotalEstTax
		{
			get { return entry.TotalEstimatedTax; }
		}

		protected override ZString DeferredTaxIndicator
		{
			get { return ZString.Empty; }
		}

		public override EntrySummary7501BillCollection EntryPrintBills
		{
			get
			{
				if (entryPrintBills == null)
				{
					entryPrintBills = new EntrySummary7501BillCollection(Factory);

					foreach (Bill bill in entry.Bills)
					{
						if (bill.NoITNumbersExist)
						{
							var ensBill = new EntryHeaderENS7501Bill(entry.PK, bill);
							entryPrintBills.Add(ensBill);
						}
						else
						{
							foreach (ITAndSplitDetails itNo in bill.ITAndSplitDetails)
							{
								var ensBill = new EntryHeaderENS7501Bill(entry.PK, itNo);
								entryPrintBills.Add(ensBill);
							}
						}
					}
				}

				return entryPrintBills;
			}
		}
		EntrySummary7501BillCollection entryPrintBills;

		public override EntrySummary7501LineCollection EntryPrintLines
		{
			get
			{
				if (entryPrintLines == null)
				{
					entryPrintLines = new EntrySummary7501LineCollection(Factory);

					ZGuid invoicePK = ZGuid.Empty;
					int linesThisInvoice = 0;
					int currentLineThisInvoice = 0;

					foreach (CusEntryLine line in entry.EntryLines)
					{
						if (line.RandomLine.JI_JZ != invoicePK)
						{
							invoicePK = line.RandomLine.JI_JZ;
							currentLineThisInvoice = 0;
							linesThisInvoice = LinesThisInvoice(entry, invoicePK);
						}

						if (!line.IsSecondaryTariffLine)
						{
							currentLineThisInvoice++;

							bool printInvoiceHeading = currentLineThisInvoice == 1;
							bool printInvoiceDetails = currentLineThisInvoice == linesThisInvoice;

							var iesLine = createNew7501Line(line, printInvoiceHeading, printInvoiceDetails);
							entryPrintLines.Add(iesLine);
						}
					}
				}

				return entryPrintLines;
			}
		}
		EntrySummary7501LineCollection entryPrintLines;

		int LinesThisInvoice(CusEntryHeader entry, ZGuid invoicePK)
		{
			int result = 0;

			foreach (CusEntryLine entryLine in entry.EntryLines)
			{
				if (entryLine.RandomLine.JI_JZ == invoicePK && !entryLine.IsSecondaryTariffLine)
				{
					result++;
				}
			}

			return result;
		}

		protected override bool IsNAFTAReconIndicator
		{
			get { return entry.Declaration.US_NAFTAReconIndicator; }
		}

		protected override ZString ENSOtherIssueCode
		{
			get { return ReconIssueCodeList.ConvertToENSOtherIssueCode(entry.Declaration.US_OtherReconIndicator); }
		}

		protected override ZString ENSOtherIssueCodeDescription
		{
			get { return Factory.GetCachedValue<ReconIssueCodeList>().GetDescriptionFromCode(entry.Declaration.US_OtherReconIndicator); }
		}

		#endregion
	}
}
