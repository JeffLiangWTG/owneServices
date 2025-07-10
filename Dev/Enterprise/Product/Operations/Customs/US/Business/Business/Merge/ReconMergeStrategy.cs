using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ReconMergeStrategy : EntryCreationStrategy
	{
		public ReconMergeStrategy(ReconDeclaration reconDeclaration, ReconMergeContext context = ReconMergeContext.None)
			: base(reconDeclaration.ReconWrappedJobDeclaration, CusEntryHeaderMessageTypeList.Codes.ReconEntry)
		{
			this.reconDeclaration = reconDeclaration;
			this.context = context;
		}

		protected readonly ReconDeclaration reconDeclaration;
		readonly ReconMergeContext context;

		public ZString GetMergedLineNumber(JobComInvoiceLine invoiceLine, ref int countOfSecondary, ref int countOfParentOrStandAlone)
		{
			if (IsParentLineOrStandAloneLine(invoiceLine))
			{
				countOfParentOrStandAlone++;
				countOfSecondary = 0;
			}
			else if (IsSecondaryLine(invoiceLine))
			{
				countOfSecondary++;
			}

			return GetMergedLineNumber(IsSecondaryLine(invoiceLine), IsParentLineOrStandAloneLine(invoiceLine), IsParentLine(invoiceLine), countOfSecondary, countOfParentOrStandAlone, invoiceLine.IsSetXLine);
		}

		protected virtual bool IsSecondaryLine(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.IsSecondaryTariffLine || !invoiceLine.HasEmptySupTariff || !invoiceLine.US_R_OrigSupTariff.IsEmpty;
		}

		protected virtual bool IsParentLine(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.IsParentLine && invoiceLine.HasEmptySupTariff && invoiceLine.US_R_OrigSupTariff.IsEmpty;
		}

		protected virtual bool IsParentLineOrStandAloneLine(JobComInvoiceLine invoiceLine)
		{
			return IsParentLine(invoiceLine) || !invoiceLine.IsParentLine && !invoiceLine.IsSecondaryTariffLine && invoiceLine.HasEmptySupTariff && invoiceLine.US_R_OrigSupTariff.IsEmpty;
		}

		protected ZString GetMergedLineNumber(bool isChild, bool isParentOrStandAlone, bool isParent, int countOfSecondaryLines, int countOfParentOrStandAlone, bool isXLine)
		{
			ZString result = ZString.Empty;

			if (isParentOrStandAlone)
			{
				result = countOfParentOrStandAlone.ToString();

				if (isParent && !isXLine)
				{
					result += "A";
				}
			}
			else if (isChild)
			{
				result = countOfParentOrStandAlone.ToString();

				switch (countOfSecondaryLines)
				{
					case 1:
						result += "B";
						break;
					case 2:
						result += "C";
						break;
					case 3:
						result += "D";
						break;
					case 4:
						result += "E";
						break;
					case 5:
						result += "F";
						break;
					case 6:
						result += "G";
						break;
					case 7:
						result += "H";
						break;
					case 8:
						result += "I";
						break;
				}
			}

			return result;
		}

		public void Initialise(ReconChangedLine mergedLine, JobComInvoiceLine invoiceLine)
		{
			mergedLine.US_ReconReason = reconDeclaration.US_IssueCode.Left(mergedLine.US_ReconReasonInfo.MaxLength);

			mergedLine.US_UC_NKCountryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;

			mergedLine.US_SPI = GetSPIToMerge(invoiceLine.US_SPI);

			mergedLine.US_OrigSPI = GetSPIToMerge(invoiceLine.US_R_OrigSPI);
			mergedLine.US_SecondarySPI = invoiceLine.US_SecondarySPI;
			mergedLine.US_HTSChangedDueToValueIndicator = invoiceLine.US_R_HTSChanged4ValueInd;

			ReconOriginalEntryHeader originalEntry = invoiceLine.InvoiceHeader != null ? invoiceLine.InvoiceHeader.ReconOriginalEntry : null;

			if (originalEntry != null)
			{
				if (originalEntry.US_ImportDate.IsValid)
				{
					mergedLine.US_Year = originalEntry.US_ImportDate.Year.ToString();
				}

				mergedLine.US_SchDEntry = originalEntry.US_SchDEntry.Left(mergedLine.US_SchDEntryInfo.MaxLength);
				mergedLine.US_CottonFeeMandatory = originalEntry.US_R_CottonFeeMandatory;
				mergedLine.US_NAFTAReconIndicator = originalEntry.US_NAFTAReconIndicator;
			}

			InitialiseCore(mergedLine, invoiceLine);

			if (context != ReconMergeContext.Messaging && originalEntry != null && !mergedLine.HasRateChanged)
			{
				if (ZShort.TryParse(invoiceLine.US_R_OrigEntryLineNo, out ZShort entryLineNo) && entryLineNo != ZShort.Zero)
				{
					var entryLineRow = FindeMatchedCusEntryLine(reconDeclaration, originalEntry, entryLineNo, mergedLine);

					if (entryLineRow != null)
					{
						var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(entryLineRow[CusEntryLineSchema.Constants.CL_AddInfo].ToString());
						if (addInfos.TryGetValue(AutoUSAddInfo.Schema.US_DutyRateDesc.Substring(3), out var desc))
						{
							mergedLine.US_OrigDutyRateDesc = desc;
						}
					}
				}
			}
		}

		IColumnIndexer FindeMatchedCusEntryLine(ReconDeclaration tempDeclaration, ReconOriginalEntryHeader originalEntry, ZShort entryLineNo, ReconChangedLine mergedLine)
		{
			var rowFactory = ((IBusinessObjectFactoryInternals)(tempDeclaration.Factory)).RowFactory;
			var loadOriginalEntryLineQuery = new ZQuery(CusEntryLineSchema.CL_CH, originalEntry.CH_CH_OriginalEntry);
			loadOriginalEntryLineQuery.AddToFilter(CusEntryLineSchema.CL_LineNumber, entryLineNo);
			loadOriginalEntryLineQuery.AddToFilter(CusEntryLineSchema.CL_AdValoremTariff, mergedLine.US_OrigTariff);
			var strDutyRateDesc = AutoUSAddInfo.Schema.US_DutyRateDesc.Substring(3);
			loadOriginalEntryLineQuery.AddToFilter(CusEntryLineSchema.CL_AddInfo, SQLComparisonOperator.Contains, strDutyRateDesc + BaseAddInfo.CodeValueSeparator);
			loadOriginalEntryLineQuery.AddToFilter(CusEntryLineSchema.CL_AddInfo, IsSupLine ? SQLComparisonOperator.Contains : SQLComparisonOperator.NotContains, AutoUSAddInfo.Schema.US_SupLine.Substring(3) + BaseAddInfo.CodeValueSeparator + YesNoList.Codes.Yes);
			return rowFactory.Load(CusEntryLineSchema.Constants.TableName, loadOriginalEntryLineQuery).Cast<IColumnIndexer>().FirstOrDefault();
		}

		protected virtual bool IsSupLine => false;

		protected virtual void InitialiseCore(ReconChangedLine mergedLine, JobComInvoiceLine invoiceLine)
		{
			InitialiseTariff(mergedLine, invoiceLine.JI_Tariff, invoiceLine.US_R_OrigTariff, invoiceLine.OriginalImportTariff);
		}

		protected void InitialiseTariff(ReconChangedLine mergedLine, ZString tariff, ZString origTariff, USCTariff originalImportTariff)
		{
			mergedLine.US_Tariff = tariff.Left(mergedLine.US_TariffInfo.MaxLength);
			mergedLine.US_OrigTariff = origTariff.Left(mergedLine.US_OrigTariffInfo.MaxLength);
			mergedLine.US_OriginalHTSEffectiveDate = originalImportTariff?.UE_DateFrom ?? ZDateTime.Empty;
		}

		public void MergeValues(ReconChangedLine mergedLine, JobComInvoiceLine invoiceLine)
		{
			if (mergedLine.US_ReconReasonText.IsEmpty && !invoiceLine.US_R_ReconReasonText.IsEmpty)
			{
				mergedLine.US_ReconReasonText = invoiceLine.US_R_ReconReasonText.Left(mergedLine.US_ReconReasonTextInfo.MaxLength);
			}
			ReconOriginalEntryHeader originalEntry = invoiceLine.InvoiceHeader != null ? invoiceLine.InvoiceHeader.ReconOriginalEntry : null;

			if (originalEntry != null)
			{
				if (!mergedLine.US_SchDEntry.IsEmpty && mergedLine.US_SchDEntry != "All" && mergedLine.US_SchDEntry != originalEntry.US_SchDEntry)
				{
					mergedLine.US_SchDEntry = "All";
				}
			}

			MergeValuesCore(mergedLine, invoiceLine);
		}

		protected virtual void MergeValuesCore(ReconChangedLine mergedLine, JobComInvoiceLine invoiceLine)
		{
			MergeFees(mergedLine, invoiceLine);

			mergedLine.US_FirstQty += invoiceLine.JI_CustomsQuantity;
			mergedLine.US_OrigFirstQty += invoiceLine.US_R_OrigFirstQty;
			mergedLine.US_CustomsValue += invoiceLine.JI_LinePrice;
			mergedLine.US_OrigCustomsValue += invoiceLine.US_R_OrigCV;
			mergedLine.US_Duty += invoiceLine.US_Duty;
			mergedLine.US_OrigDuty += invoiceLine.US_R_OrigDuty;
			mergedLine.AddOrigEntryLine(new ReconOrigEntryLineWrap(invoiceLine));
		}

		void CalculatePayableMPFChange(JobComInvoiceLine invoiceLine, ReconChangedLine mergedLine, ZDecimal originalFee, ZDecimal reconFee)
		{
			if (context == ReconMergeContext.Documents && invoiceLine.InvoiceHeader is JobComInvoiceHeader invoice && invoice.ReconOriginalEntry is ReconOriginalEntryHeader reconOriginalEntry)
			{
				var originalMPFOnEntryLevel = reconOriginalEntry.OriginalMPF;
				var reconMPFOnEntryLevel = reconOriginalEntry.ReconMPF;
				var invoiceLines = invoice.JobComInvoiceLines;
				var totalCalculatedOrigMPFAmount = invoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.US_R_OrigMPFAmount);
				var totalCalculatedReconMPFAmount = invoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.US_R_ReconMPFAmount);
				var apportionedPayableReconMPF = totalCalculatedReconMPFAmount == 0 ? ZDecimal.Zero : (ZDecimal)(reconFee / totalCalculatedReconMPFAmount * reconMPFOnEntryLevel);
				var apportionedPayableOrigMPF = totalCalculatedOrigMPFAmount == 0 ? ZDecimal.Zero : (ZDecimal)((originalFee / totalCalculatedOrigMPFAmount) * originalMPFOnEntryLevel);
				mergedLine.MPFChange += ((ZDecimal)(apportionedPayableReconMPF - apportionedPayableOrigMPF)).Round(2);
			}
		}

		void MergeFees(ReconChangedLine mergedLine, JobComInvoiceLine invoiceLine)
		{
			List<IReconciliationImportEntryFee> newFees = new List<IReconciliationImportEntryFee>();

			foreach (CodeDescriptionPair pair in CusFeeCodeConstants.GetAccountingClassFeeCodeList(reconDeclaration.Factory))
			{
				var feeClassCode = pair.Code;
				var originalFee = invoiceLine.ReconOriginalCharges.GetAmount(feeClassCode);
				var reconFee = invoiceLine.FeeCusCodes.GetValue(feeClassCode);
				if (originalFee > 0m || reconFee > 0m)
				{
					newFees.Add(new ReconEntryFeeIReconciliationImportEntryFee()
					{
						FeeType = feeClassCode,
						OriginalFee = originalFee,
						ReconFee = reconFee
					});

					if (feeClassCode == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
					{
						CalculatePayableMPFChange(invoiceLine, mergedLine, originalFee, reconFee);
					}
				}
			}

			foreach (ReconRefundedCharge refundedFee in invoiceLine.ReconRefundedFees)
			{
				newFees.Add(new ReconEntryFeeIReconciliationImportEntryFee()
				{
					FeeType = refundedFee.CY_Code,
					OriginalFee = 0m,
					ReconFee = 0m
				});
			}

			if (reconDeclaration.IsACE)
			{
				var originTariffRequiredCodes = invoiceLine.OriginalImportTariff.GetReconTariffRequiredFeeCodes(invoiceLine.US_R_OrigCottonFeeExempt == YesNoDefaultList.Codes.Yes, mergedLine.US_CottonFeeMandatory);
				var reconTariffRequiredCode = invoiceLine.ImportTariff.GetReconTariffRequiredFeeCodes(invoiceLine.IsCottonFeeExemptIndicated, mergedLine.US_CottonFeeMandatory);
				var tariffRequiredCodes = originTariffRequiredCodes.Concat(reconTariffRequiredCode).Distinct();

				foreach (var feeCode in tariffRequiredCodes)
				{
					if (!newFees.Any(x => x.FeeClass == feeCode))
					{
						newFees.Add(new ReconEntryFeeIReconciliationImportEntryFee()
						{
							FeeType = feeCode,
							OriginalFee = 0m,
							ReconFee = 0m
						});
					}
				}
			}

			if (newFees.Count > 0)
			{
				mergedLine.AddFees(newFees);
			}
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			return new MergeKey(0);//there is only one entry always for recon
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)baseInvoiceLine;

			MergeKey result = GetMergeKeyForOneLine(invoiceLine);

			if (invoiceLine.IsParentLine || invoiceLine.IsSecondaryTariffLine)
			{
				JobComInvoiceLine parentLine = invoiceLine.ParentTariffLine ?? invoiceLine;

				result += GetMergeKeyForOneLine(parentLine);

				List<JobComInvoiceLine> sortedInvoiceLine = new List<JobComInvoiceLine>(parentLine.SecondaryTariffLines);
				sortedInvoiceLine.Sort(new InvoiceLineComparer());//secondary tariff lines' line number

				foreach (JobComInvoiceLine secondaryLine in sortedInvoiceLine)
				{
					result += GetMergeKeyForOneLine(secondaryLine);
				}
			}

			return result;
		}

		protected virtual MergeKey GetMergeKeyForOneLine(JobComInvoiceLine invoiceLine)
		{
			MergeKey result = new MergeKey();

			result.Add(invoiceLine.US_R_OrigTariff);
			result.Add(GetSPIToMerge(invoiceLine.US_R_OrigSPI));
			result.Add(invoiceLine.US_R_OrigFirstUQ);
			result.Add(invoiceLine.US_R_OrigSecondUQ);
			result.Add(invoiceLine.US_R_OrigThirdUQ);

			result.Add(invoiceLine.JI_Tariff);
			result.Add(invoiceLine.US_UC_NKCountryOfOrigin);
			result.Add(GetSPIToMerge(invoiceLine.US_SPI));

			result.Add(invoiceLine.US_SupTariff);
			result.Add(invoiceLine.US_SupUQ1);
			result.Add(invoiceLine.US_SupUQ2);
			result.Add(invoiceLine.US_SupUQ3);

			result.Add(invoiceLine.US_R_OrigSupTariff);
			result.Add(invoiceLine.US_R_OrigSupUQ1);
			result.Add(invoiceLine.US_R_OrigSupUQ2);
			result.Add(invoiceLine.US_R_OrigSupUQ3);

			if (invoiceLine.Declaration.ReconDeclaration.IsACE)
			{
				var hTSChanged4ValueKey = invoiceLine.US_R_HTSChanged4ValueInd ? new ZString("HC") : ZString.Empty;
				result.Add(hTSChanged4ValueKey);
			}

			var hasDecreaseKey = invoiceLine.Declaration.ReconDeclaration != null &&
				invoiceLine.Declaration.ReconDeclaration.US_R_Waive && invoiceLine.HasDecrease ? new ZString("D") : ZString.Empty;
			result.Add(hasDecreaseKey);

			var originalEntry = invoiceLine.InvoiceHeader?.ReconOriginalEntry;
			if (originalEntry != null)
			{
				if (originalEntry.US_R_ReleaseDate.IsValid)
				{
					result.Add(ZString.Format(originalEntry.US_R_ReleaseDate.Year.ToString(CultureInfo.InvariantCulture)));
				}

				result.Add(originalEntry.US_R_CottonFeeMandatory);
				result.Add(originalEntry.US_NAFTAReconIndicator);
			}
			result.Add(GetOverridenOrRateTypeFeesInOrder(invoiceLine));

			return result;
		}

		ZString GetOverridenOrRateTypeFeesInOrder(JobComInvoiceLine invoiceLine)
		{
			var result = new ZStringBuilder();
			result.Append(GetOverridenOrRateTypeFeesInOrder(invoiceLine.ReconOriginalCharges.OfType<IFee>()));
			result.Append(GetOverridenOrRateTypeFeesInOrder(invoiceLine.FeeCusCodes.OfType<IFee>()));
			return result.ToString();
		}

		ZString GetOverridenOrRateTypeFeesInOrder(IEnumerable<IFee> feeCollection)
		{
			var result = new ZStringBuilder();
			var sortedFeesList = feeCollection.ToList();
			sortedFeesList.Sort(new System.Comparison<IFee>((IFee fee1, IFee fee2) => fee1.Code.CompareTo(fee2.Code)));
			foreach (IFee feeData in sortedFeesList)
			{
				if (feeData.IsOverridden || !feeData.SelectedRateType.IsEmpty)
				{
					result.Append(feeData.Code + feeData.IsOverridden + feeData.SelectedRateType);
				}
			}
			return result.ToString();
		}

		internal static ZString GetSPIToMerge(ZString lineSPI)
		{
			return lineSPI == SPICompleteList.MoreCodes.NotApplicable ? ZString.Empty : lineSPI;
		}

		protected override Customs.Business.CusEntryHeader GetExistingEntryHeader(BaseJobComInvoiceLine invoiceLine)
		{
			CusEntryHeader result = (CusEntryHeader)base.GetExistingEntryHeader(invoiceLine) ?? reconDeclaration.ReconEntry.GetEntry();

			return result;
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			yield return reconDeclaration.ReconEntry.GetEntry();
		}

		public override bool LineIsValidForMerge(BaseJobComInvoiceLine invoiceLine)
		{
			return !invoiceLine.JI_Tariff.IsEmpty;
		}
	}

	public class ReconSupMergeStrategy : ReconMergeStrategy
	{
		public ReconSupMergeStrategy(ReconDeclaration reconDeclaration)
			: base(reconDeclaration)
		{
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			MergeKey result = base.GetKeyForLine(baseInvoiceLine);

			result.Add(new ZString("SUP"));

			return result;
		}

		public override bool LineIsValidForMerge(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			return !invoiceLine.HasEmptySupTariff || !invoiceLine.US_R_OrigSupTariff.IsEmpty;
		}

		protected override bool IsSupLine => true;
		protected override void InitialiseCore(ReconChangedLine mergedLine, JobComInvoiceLine invoiceLine)
		{
			InitialiseTariff(mergedLine, invoiceLine.US_SupTariff, invoiceLine.US_R_OrigSupTariff, invoiceLine.OriginalImportSupTariff);
		}

		protected override void MergeValuesCore(ReconChangedLine mergedLine, JobComInvoiceLine invoiceLine)
		{
			mergedLine.US_FirstQty += invoiceLine.US_SupQty1;
			mergedLine.US_OrigFirstQty += invoiceLine.US_R_OrigSupQty1;
			mergedLine.US_CustomsValue += invoiceLine.US_98GoodsValue;
			mergedLine.US_OrigCustomsValue += invoiceLine.US_R_Orig98Value;
			mergedLine.US_Duty += invoiceLine.US_SupDuty;
			mergedLine.US_OrigDuty += invoiceLine.US_R_OrigSupDuty;
		}

		protected override bool IsSecondaryLine(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.IsSecondaryTariffLine;
		}

		protected override bool IsParentLine(JobComInvoiceLine invoiceLine)
		{
			return !IsSecondaryLine(invoiceLine);
		}

		protected override bool IsParentLineOrStandAloneLine(JobComInvoiceLine invoiceLine)
		{
			return IsParentLine(invoiceLine);
		}
	}
}
