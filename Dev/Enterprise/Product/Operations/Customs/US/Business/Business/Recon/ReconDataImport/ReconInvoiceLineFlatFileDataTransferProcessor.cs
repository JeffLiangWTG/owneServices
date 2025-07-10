using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconInvoiceLineFlatFileDataTransferProcessor : ReconFlatFileDataTransferProcessor
	{
		#region Export

		#region ExportInvoiceLinesSortOrder

		class ExportInvoiceLinesSortOrder : IComparer<JobComInvoiceLine>
		{
			#region IComparer<JobComInvoiceLine> Members

			int IComparer<JobComInvoiceLine>.Compare(JobComInvoiceLine x, JobComInvoiceLine y)
			{
				ZString xEntryReference = x.InvoiceHeader.ReconOriginalEntry != null ? x.InvoiceHeader.ReconOriginalEntry.CH_OrigEntryReference : ZString.Empty;
				ZString yEntryReference = x.InvoiceHeader.ReconOriginalEntry != null ? x.InvoiceHeader.ReconOriginalEntry.CH_OrigEntryReference : ZString.Empty;

				if (!xEntryReference.EqualsIgnoringCase(yEntryReference))
				{
					return xEntryReference.CompareTo(yEntryReference);
				}
				else if (!x.InvoiceHeader.JZ_InvoiceNumber.EqualsIgnoringCase(y.InvoiceHeader.JZ_InvoiceNumber))
				{
					return x.InvoiceHeader.JZ_InvoiceNumber.CompareTo(y.InvoiceHeader.JZ_InvoiceNumber);
				}
				else
				{
					JobComInvoiceLine line1 = x.ParentTariffLine ?? x;
					JobComInvoiceLine line2 = y.ParentTariffLine ?? y;

					if (line1 == line2)
					{
						return x.JI_LineNo.CompareTo(y.JI_LineNo);
					}
					else
					{
						return line1.JI_LineNo.CompareTo(line2.JI_LineNo);
					}
				}
			}

			#endregion
		}

		#endregion

		public override void ExportReconDataToCollection(ReconFlattenedDataLineCollection collection, ReconDeclaration reconDeclaration)
		{
			int processed = 0;

			List<JobComInvoiceLine> invoiceLines = new List<JobComInvoiceLine>(new TypedEnumerable<JobComInvoiceLine>(reconDeclaration.InvoiceLines));

			invoiceLines.Sort(new ExportInvoiceLinesSortOrder());

			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				if (!OnProgressChanged(processed * 100 / reconDeclaration.InvoiceLines.Count, string.Format("Processing ({0} of {1}) ...", processed, reconDeclaration.InvoiceLines.Count)))
				{
					break;
				}
				processed++;
				ExportOriginalDetailsToOneDataLine(collection.AddNew(), invoiceLine);
			}
		}

		void ExportOriginalDetailsToOneDataLine(ReconFlattenedDataLine reconDataLine, JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.InvoiceHeader != null)
			{
				ReconOriginalEntryHeader originalEntry = invoiceLine.InvoiceHeader.ReconOriginalEntry;

				if (originalEntry != null)
				{
					ExportOriginalEntryDetails(reconDataLine, originalEntry);
				}
			}

			ExportOriginalInvoiceLineDetailsToOneDataLine(reconDataLine, invoiceLine);
		}

		void ExportOriginalEntryDetails(ReconFlattenedDataLine dataLine, ReconOriginalEntryHeader entry)
		{
			dataLine.EntryNumber = entry.CH_OrigEntryReference.Left(ReconFlattenedDataLine.Schema.EntryNumberMaxLength);
			dataLine.PaymentDate = entry.US_PaymentDate;
			dataLine.EntryDate = entry.US_R_ReleaseDate;
			dataLine.EntryPort = entry.US_SchDEntry.Left(ReconFlattenedDataLine.Schema.EntryPortMaxLength);
			dataLine.ImportationDate = entry.US_ImportDate;
			dataLine.OwnerReferenceNumber = entry.US_R_OwnerRef;

			var impDeclaration = entry.OriginalDeclaration;

			if (impDeclaration != null)
			{
				dataLine.JobNumber = impDeclaration.JE_DeclarationReference;
			}
			dataLine.MessageMode = entry.US_R_MsgMode.Left(ReconFlattenedDataLine.Schema.MessageModeMaxLength);
		}

		void ExportOriginalInvoiceLineDetailsToOneDataLine(ReconFlattenedDataLine dataLine, JobComInvoiceLine invoiceLine)
		{
			using (dataLine.GetValidationSuspender())
			{
				dataLine.LineNumber = invoiceLine.JI_LineNo;
				dataLine.OrgLineNumber = invoiceLine.US_R_OrigEntryLineNo;
				dataLine.IsChildLine = invoiceLine.IsChildLine;
				dataLine.ProductNumber = invoiceLine.JI_PartNo;
				CusClassification classification = invoiceLine.Classification;
				dataLine.Lookup = classification == null ? ZString.Empty : classification.CC_LookupCode;

				dataLine.CountryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin;

				dataLine.OriginalTariff = invoiceLine.US_R_OrigTariff.Left(ReconFlattenedDataLine.Schema.OriginalTariffMaxLength);
				dataLine.OriginalSupTariff = invoiceLine.US_R_OrigSupTariff.Left(ReconFlattenedDataLine.Schema.OriginalSupTariffMaxLength);
				dataLine.ReconTariff = invoiceLine.JI_Tariff.Left(ReconFlattenedDataLine.Schema.ReconTariffMaxLength);
				dataLine.ReconSupTariff = invoiceLine.US_SupTariff.Left(ReconFlattenedDataLine.Schema.ReconSupTariffMaxLength);

				dataLine.GoodsDescription = invoiceLine.JI_Description.Left(ReconFlattenedDataLine.Schema.GoodsDescriptionMaxLength);

				dataLine.OriginalSPI = invoiceLine.US_R_OrigSPI;
				dataLine.ReconSPI = invoiceLine.US_SPI;
				dataLine.ReconSecondarySPI = invoiceLine.US_SecondarySPI;

				dataLine.OriginalFirstUQ = invoiceLine.US_R_OrigFirstUQ;
				dataLine.OriginalSecondUQ = invoiceLine.US_R_OrigSecondUQ;
				dataLine.OriginalThirdUQ = invoiceLine.US_R_OrigThirdUQ;
				dataLine.OriginalFirstQty = invoiceLine.US_R_OrigFirstQty;
				dataLine.OriginalSecondQty = invoiceLine.US_R_OrigSecondQty;
				dataLine.OriginalThirdQty = invoiceLine.US_R_OrigThirdQty;

				dataLine.OriginalSupFirstUQ = invoiceLine.US_R_OrigSupUQ1;
				dataLine.OriginalSupSecondUQ = invoiceLine.US_R_OrigSupUQ2;
				dataLine.OriginalSupThirdUQ = invoiceLine.US_R_OrigSupUQ3;
				dataLine.OriginalSupFirstQty = invoiceLine.US_R_OrigSupQty1;
				dataLine.OriginalSupSecondQty = invoiceLine.US_R_OrigSupQty2;
				dataLine.OriginalSupThirdQty = invoiceLine.US_R_OrigSupQty3;

				dataLine.ReconFirstQty = invoiceLine.JI_CustomsQuantity;
				dataLine.ReconSecondQty = invoiceLine.JI_CustomsSecondQuantity;
				dataLine.ReconThirdQty = invoiceLine.JI_CustomsThirdQuantity;

				dataLine.ReconSupFirstQty = invoiceLine.US_SupQty1;
				dataLine.ReconSupSecondQty = invoiceLine.US_SupQty2;
				dataLine.ReconSupThirdQty = invoiceLine.US_SupQty3;

				dataLine.OriginalCustomsValue = invoiceLine.US_R_OrigCV;
				dataLine.Original98Value = invoiceLine.US_R_Orig98Value;

				dataLine.ReconCustomsValue = invoiceLine.JI_LinePrice;
				dataLine.Recon98Value = invoiceLine.US_98GoodsValue;

				dataLine.OriginalDutyOverride = invoiceLine.US_R_OrigOverrideDuty;
				dataLine.OriginalDuty = invoiceLine.US_R_OrigDuty;
				dataLine.OriginalSupDutyOverride = invoiceLine.US_R_OrigOverrideSupDuty;
				dataLine.OriginalSupDuty = invoiceLine.US_R_OrigSupDuty;

				dataLine.ReconDutyOverride = invoiceLine.US_OverrideDuty;
				dataLine.ReconDuty = invoiceLine.US_Duty;
				dataLine.ReconSupDutyOverride = invoiceLine.US_OverrideSupDuty;
				dataLine.ReconSupDuty = invoiceLine.US_SupDuty;

				dataLine.OriginalTaxApply = invoiceLine.US_R_OrigTaxApply;
				dataLine.OriginalTaxRateType = invoiceLine.US_R_OrigTaxRateT;
				dataLine.OriginalTaxCode = invoiceLine.US_R_OrigTaxCode;
				dataLine.OriginalTaxAmount = invoiceLine.US_R_OrigTaxAmount;
				dataLine.OriginalTaxRateS = invoiceLine.US_R_OrigTaxRateS;
				dataLine.OriginalTaxRate = invoiceLine.US_R_OrigTaxRate;
				dataLine.OriginalTaxRateQuantity = invoiceLine.US_R_OrigTaxQty;

				dataLine.ReconTaxApply = invoiceLine.US_TaxApply;
				dataLine.ReconTaxRateType = invoiceLine.US_TaxRateT;
				dataLine.ReconTaxCode = invoiceLine.US_TaxCode;
				dataLine.ReconTaxRateS = invoiceLine.US_TaxRateS;
				dataLine.ReconTaxRate = invoiceLine.US_TaxRate;
				dataLine.ReconTaxRateQuantity = invoiceLine.US_TaxQty;

				dataLine.OverrideOriginalMPF = invoiceLine.US_R_OverrideOriginMPF;
				dataLine.OriginalMPF = invoiceLine.US_R_OrigMPFAmount;
				dataLine.OverrideOriginalHMF = invoiceLine.US_R_OverrideOriginHMF;
				dataLine.OriginalHMF = invoiceLine.US_R_OrigHMFAmount;
				dataLine.OriginalCottonFeeExempt = invoiceLine.US_R_OrigCottonFeeExempt;
				dataLine.OverrideReconMPF = invoiceLine.US_R_OverrideReconMPF;
				dataLine.ReconMPF = invoiceLine.US_R_ReconMPFAmount;
				dataLine.OverrideReconHMF = invoiceLine.US_R_OverrideReconHMF;
				dataLine.ReconHMF = invoiceLine.US_R_ReconHMFAmount;
				dataLine.OverrideReconOtherFeeAmount = invoiceLine.US_R_OverrideReconOtherFeeAmount;
				dataLine.ReconOtherFeeCode = invoiceLine.US_R_ReconOtherFeeCode;
				dataLine.ReconOtherFee = invoiceLine.US_R_ReconOtherFeeAmount;
				dataLine.ReconCottonFeeExempt = invoiceLine.US_CottonFeeExempt;
				dataLine.HTSChangedDueToValue = invoiceLine.US_R_HTSChanged4ValueInd;
				dataLine.IsTextile = invoiceLine.US_R_Textile;
				dataLine.OriginalRateType = invoiceLine.US_R_OrigRateType;
				dataLine.ReconRateType = invoiceLine.US_SelectedRateType;
				dataLine.ReconReason = invoiceLine.US_R_ReconReasonText;

				ZString? uniqueFeeOtherThan499_501 = null;
				ZBool? overrideUniqueFeeOtherThan499_501 = null;

				foreach (ReconEntryOriginalCharge originalCharge in invoiceLine.ReconOriginalCharges)
				{
					if (originalCharge.CY_Code != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing &&
						originalCharge.CY_Code != Core.Constants.USCustoms.FeeCodes.HMF &&
						!CusFeeCodeConstants.IsExciseTax(originalCharge.CY_Code) &&
						originalCharge.CY_Amount > 0m)
					{
						if (!uniqueFeeOtherThan499_501.HasValue)
						{
							uniqueFeeOtherThan499_501 = originalCharge.CY_Code;
							overrideUniqueFeeOtherThan499_501 = originalCharge.CY_IsOverridden;
						}
						else if (uniqueFeeOtherThan499_501.Value != originalCharge.CY_Code)
						{
							uniqueFeeOtherThan499_501 = ZString.Empty;
							overrideUniqueFeeOtherThan499_501 = false;
						}

						dataLine.OriginalOtherFee += originalCharge.CY_Amount;
					}
				}

				if (uniqueFeeOtherThan499_501.HasValue)
				{
					dataLine.OriginalOtherFeeCode = uniqueFeeOtherThan499_501.Value;
					dataLine.OverrideOriginalOtherFeeAmount = overrideUniqueFeeOtherThan499_501.Value;
				}
			}
		}

		#endregion

		#region Import

		public override void ImportReconDataFromCollection(ReconFlattenedDataLineCollection collection, ReconDeclaration reconDeclaration)
		{
			collection.Factory.ActivateStringInterning(); // leave activated for the life of the Factory (ie: until the Form is closed)
			entryPKs = new List<ZGuid>();
			JobComInvoiceLine previousParentLine = null;
			using (reconDeclaration.ReconWrappedJobDeclaration.SuspendDefaultingSecondaryTariffLines())
			using (reconDeclaration.FilteredInvoiceLines.SuspendListChanged())
			using (reconDeclaration.OriginalEntries.SuspendListChanged())
			using (reconDeclaration.InvoiceLines.SuspendListChanged())
			using (reconDeclaration.ActiveGroupHeader.SuspendListChanged())
			{
				try
				{
					using (reconDeclaration.GetValidationSuspender())
					{
						reconDeclaration.Factory.RefreshEnabled = false;

						for (int r = 0; r < collection.Count; r++)
						{
							if (!OnProgressChanged(r * 100 / collection.Count, string.Format("Processing ({0} of {1}) ...", r, collection.Count)))
							{
								break;
							}

							JobComInvoiceLine line = ImportFromOneDataLine(collection[r], reconDeclaration, previousParentLine);
							if (!line.IsChildLine)
							{
								previousParentLine = line;
							}
						}
					}
				}
				finally
				{
					reconDeclaration.Factory.RefreshEnabled = true;
				}
			}
		}
		List<ZGuid> entryPKs;

		JobComInvoiceLine ImportFromOneDataLine(ReconFlattenedDataLine dataLine, ReconDeclaration reconDeclaration, JobComInvoiceLine previousParentLine)
		{
			ReconOriginalEntryHeader originalEntry = ImportOriginalEntryDetails(dataLine, reconDeclaration);
			return ImportInvoiceLineDetails(dataLine, originalEntry, previousParentLine);
		}

		ReconOriginalEntryHeader ImportOriginalEntryDetails(ReconFlattenedDataLine dataLine, ReconDeclaration reconDeclaration)
		{
			ReconOriginalEntryHeader result = LocateEntry(dataLine, reconDeclaration);

			if (result != null)
			{
				if (!entryPKs.Contains(result.CH_PK))
				{
					entryPKs.Add(result.CH_PK);
					result.Invoice.Delete();
					if (!result.US_R_ChangedLinesOnly)
					{
						result.OriginalCharges.RemoveAndDeleteAll();
					}
				}

				using (result.GetValidationSuspender())
				{
					SetCommonEntryLevelInfo(dataLine, result);
					if (!dataLine.GoodsDescription.IsEmpty && result.US_R_GoodsDescription.IsEmpty)
					{
						result.US_R_GoodsDescription = dataLine.GoodsDescription;
					}
				}
			}

			return result;
		}

		JobComInvoiceLine ImportInvoiceLineDetails(ReconFlattenedDataLine dataLine, ReconOriginalEntryHeader originalEntry, JobComInvoiceLine previousParentLine)
		{
			JobComInvoiceLine result = null;
			if (originalEntry != null)
			{
				JobComInvoiceHeader invoice = originalEntry.Invoice;
				var shouldImportOrigDuty = !originalEntry.US_R_CalcOrigDuty;

				result = invoice.InvoiceLines.AddNew();
				using (result.GetValidationSuspender())
				{
					using (invoice.GetLineNumberRenumberingSuspender())
					{
						result.JI_LineNo = dataLine.LineNumber;
					}

					result.US_R_OrigEntryLineNo = dataLine.OrgLineNumber;

					if (dataLine.IsChildLine)
					{
						result.JI_ParentID = previousParentLine != null && previousParentLine.JI_JZ == result.JI_JZ ? previousParentLine.PK : ZGuid.Empty;
					}
					result.JI_PartNo = dataLine.ProductNumber;
					CusClassification classification = new CusClassification.Loader(result.Factory).Load(dataLine.Lookup, CusClassification.ClassificationType.IMP);
					if (classification != null)
					{
						result.JI_CC = classification.PK;
					}

					if (classification == null || classification.CC_TariffNum.IsEmpty)
					{
						result.JI_Tariff = dataLine.ReconTariff.IsEmpty ? dataLine.OriginalTariff : dataLine.ReconTariff;
					}

					result.US_UC_NKCountryOfOrigin = dataLine.CountryOfOrigin;
					result.US_R_OrigTariff = dataLine.OriginalTariff;
					result.JI_Description = dataLine.GoodsDescription;
					result.US_SPI = dataLine.ReconSPI;
					result.US_R_OrigSPI = dataLine.OriginalSPI;
					result.US_SecondarySPI = dataLine.ReconSecondarySPI;
					result.JI_CustomsQuantity = dataLine.ReconFirstQty;
					result.JI_CustomsSecondQuantity = dataLine.ReconSecondQty;
					result.JI_CustomsThirdQuantity = dataLine.ReconThirdQty;
					result.US_R_OrigFirstQty = dataLine.OriginalFirstQty;
					result.US_R_OrigSecondQty = dataLine.OriginalSecondQty;
					result.US_R_OrigThirdQty = dataLine.OriginalThirdQty;
					result.US_R_OrigCV = dataLine.OriginalCustomsValue;
					result.JI_LinePrice = dataLine.ReconCustomsValue;
					result.US_R_OrigCottonFeeExempt = dataLine.OriginalCottonFeeExempt;
					result.US_CottonFeeExempt = dataLine.ReconCottonFeeExempt;

					result.US_R_OrigSupTariff = dataLine.OriginalSupTariff;
					result.US_R_OrigSupQty1 = dataLine.OriginalSupFirstQty;
					result.US_R_OrigSupQty2 = dataLine.OriginalSupSecondQty;
					result.US_R_OrigSupQty3 = dataLine.OriginalSupThirdQty;
					result.US_R_Orig98Value = dataLine.Original98Value;

					result.US_SupTariff = dataLine.ReconSupTariff;
					result.US_SupQty1 = dataLine.ReconSupFirstQty;
					result.US_SupQty2 = dataLine.ReconSupSecondQty;
					result.US_SupQty3 = dataLine.ReconSupThirdQty;
					result.US_98GoodsValue = dataLine.Recon98Value;

					result.US_R_OrigOverrideDuty = dataLine.OriginalDutyOverride;
					if (dataLine.OriginalDutyOverride || shouldImportOrigDuty)
					{
						result.US_R_OrigDuty = dataLine.OriginalDuty;
					}

					result.US_R_OrigOverrideSupDuty = dataLine.OriginalSupDutyOverride;
					if (dataLine.OriginalSupDutyOverride || shouldImportOrigDuty)
					{
						result.US_R_OrigSupDuty = dataLine.OriginalSupDuty;
					}

					result.US_OverrideDuty = dataLine.ReconDutyOverride;
					if (dataLine.ReconDutyOverride)
					{
						result.US_Duty = dataLine.ReconDuty;
					}

					result.US_OverrideSupDuty = dataLine.ReconSupDutyOverride;
					if (dataLine.ReconSupDutyOverride)
					{
						result.US_SupDuty = dataLine.ReconSupDuty;
					}

					result.US_R_OrigTaxApply = dataLine.OriginalTaxApply;
					result.US_R_OrigTaxRateT = dataLine.OriginalTaxRateType;

					if (!dataLine.OriginalTaxCode.IsEmpty)
					{
						result.US_R_OrigTaxCode = dataLine.OriginalTaxCode.Length > 0 && dataLine.OriginalTaxCode.Length < 3 ?
													dataLine.OriginalTaxCode.PadLeft(3, '0') : dataLine.OriginalTaxCode;
					}
					if (!dataLine.OriginalTaxAmount.IsEmpty)
					{
						result.US_R_OrigTaxAmount = dataLine.OriginalTaxAmount;
					}
					if (!result.OrigTaxRateS_ReadOnly)
					{
						result.US_R_OrigTaxRateS = dataLine.OriginalTaxRateS;
					}
					if (!result.US_R_OrigTaxRate_ReadOnly)
					{
						result.US_R_OrigTaxRate = dataLine.OriginalTaxRate;
					}
					if (!result.US_R_OrigTaxQty_ReadOnly)
					{
						result.US_R_OrigTaxQty = dataLine.OriginalTaxRateQuantity;
					}

					result.US_TaxApply = dataLine.ReconTaxApply;
					result.US_TaxRateT = dataLine.ReconTaxRateType;

					if (!dataLine.ReconTaxCode.IsEmpty)
					{
						result.US_TaxCode = dataLine.ReconTaxCode.Length > 0 && dataLine.ReconTaxCode.Length < 3 ?
													dataLine.ReconTaxCode.PadLeft(3, '0') : dataLine.ReconTaxCode;
					}

					if (!result.TaxRateS_ReadOnly)
					{
						result.US_TaxRateS = dataLine.ReconTaxRateS;
					}
					if (!result.TaxRate_ReadOnly)
					{
						result.US_TaxRate = dataLine.ReconTaxRate;
					}
					if (!result.TaxQty_ReadOnly)
					{
						result.US_TaxQty = dataLine.ReconTaxRateQuantity;
					}

					if (dataLine.OverrideOriginalMPF)
					{
						result.US_R_OverrideOriginMPF = dataLine.OverrideOriginalMPF;
					}
					if (!dataLine.OriginalMPF.IsEmpty)
					{
						result.US_R_OrigMPFAmount = dataLine.OriginalMPF;
						result.US_R_OrigHasMPF = dataLine.OriginalMPF > 0;
					}
					if (dataLine.OverrideOriginalHMF)
					{
						result.US_R_OverrideOriginHMF = dataLine.OverrideOriginalHMF;
					}
					if (!dataLine.OriginalHMF.IsEmpty)
					{
						result.US_R_OrigHMFAmount = dataLine.OriginalHMF;
					}
					if (dataLine.OverrideOriginalOtherFeeAmount)
					{
						result.US_R_OverrideOrigOtherFeeAmount = dataLine.OverrideOriginalOtherFeeAmount;
					}
					if (!dataLine.OriginalOtherFeeCode.IsEmpty)
					{
						result.US_R_OrigOtherFeeCode = dataLine.OriginalOtherFeeCode.Length > 0 && dataLine.OriginalOtherFeeCode.Length < 3 ?
												dataLine.OriginalOtherFeeCode.PadLeft(3, '0') : dataLine.OriginalOtherFeeCode;
					}
					if (!dataLine.OriginalOtherFee.IsEmpty)
					{
						result.US_R_OrigOtherFeeAmount = dataLine.OriginalOtherFee;
					}
					if (dataLine.OverrideReconMPF)
					{
						result.US_R_OverrideReconMPF = dataLine.OverrideReconMPF;
					}
					if (!dataLine.ReconMPF.IsEmpty)
					{
						result.US_R_ReconMPFAmount = dataLine.ReconMPF;
					}
					if (dataLine.OverrideReconHMF)
					{
						result.US_R_OverrideReconHMF = dataLine.OverrideReconHMF;
					}
					if (!dataLine.ReconHMF.IsEmpty)
					{
						result.US_R_ReconHMFAmount = dataLine.ReconHMF;
					}
					if (dataLine.OverrideReconOtherFeeAmount)
					{
						result.US_R_OverrideReconOtherFeeAmount = dataLine.OverrideReconOtherFeeAmount;
					}
					if (!dataLine.ReconOtherFeeCode.IsEmpty)
					{
						result.US_R_ReconOtherFeeCode = dataLine.ReconOtherFeeCode.Length > 0 && dataLine.ReconOtherFeeCode.Length < 3 ?
												dataLine.ReconOtherFeeCode.PadLeft(3, '0') : dataLine.ReconOtherFeeCode;
					}
					if (!dataLine.ReconOtherFee.IsEmpty)
					{
						result.US_R_ReconOtherFeeAmount = dataLine.ReconOtherFee;
					}
					result.US_R_HTSChanged4ValueInd = dataLine.HTSChangedDueToValue;
					result.US_R_Textile = dataLine.IsTextile;
					result.US_R_OrigRateType = dataLine.OriginalRateType;
					result.US_SelectedRateType = dataLine.ReconRateType;
					result.US_R_ReconReasonText = dataLine.ReconReason;
				}
			}

			return result;
		}

		#endregion
	}
}
