using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ReconImportEntryRetriever
	{
		public ReconImportEntryRetriever(ReconDeclaration reconDeclaration)
		{
			this.reconDeclaration = reconDeclaration;
		}
		readonly ReconDeclaration reconDeclaration;

		public void ImportLines()
		{
			ImportLines(reconDeclaration.OriginalEntries.ToArray<ReconOriginalEntryHeader>());
		}

		public void ImportLines(IEnumerable<ReconOriginalEntryHeader> originalEntries)
		{
			using (reconDeclaration.GetValidationSuspender())
			{
				foreach (var entry in originalEntries)
				{
					var originalDec = entry.OriginalDeclaration;

					if (originalDec != null)
					{
						ImportLinesFromImportEntry(entry);
					}
				}
			}
			reconDeclaration.ReleaseReadFactory();
		}

		public void ImportEntryDetails(ReconOriginalEntryHeader reconEntry)
		{
			var importEntry = reconDeclaration.ReadFactory.Load<CusEntryHeader>(reconEntry.CH_CH_OriginalEntry);

			if (importEntry == null)
			{
				return;
			}

			CopyDetailsFromImportEntry(reconEntry, importEntry);
		}

		#region Implementation

		internal void ImportLinesFromImportEntry(ReconOriginalEntryHeader entry)
		{
			RetrieveImportLinesIfNecessary(entry);

			var importEntry = GetImportEntry(entry.CH_CH_OriginalEntry);
			if (importEntry != null)
			{
				RetrieveEntryDetailsFromENSEntryIfNecessary(entry, importEntry);
			}

			entry.RefreshBinding();
		}

		void RetrieveImportLinesIfNecessary(ReconOriginalEntryHeader entry)
		{
			if (entry.Invoice.JobComInvoiceLines.Count == 0 && !entry.US_R_NoLineDetails)
			{
				using (entry.ReconDeclaration.ReconWrappedJobDeclaration.SuspendMarkApportionmentDirty())
				{
					try
					{
						RetrieveImportLineDetails(entry);
					}
					finally
					{
						entry.ReconDeclaration.ReconWrappedJobDeclaration.ResumeApportionment();
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		void RetrieveEntryDetailsFromENSEntryIfNecessary(ReconOriginalEntryHeader entry, CusEntryHeader importEntry)
		{
			if (entry.IsACE && importEntry.Messages.Count > 0)
			{
				var lastAcceptedENSMessage = (EDIMessage)PGADispositionProviderExtensionMethods.GetLastENSClearedMessage(importEntry.Declaration);
				if (lastAcceptedENSMessage != null)
				{
					AENS40 ens40Block = null;
					foreach (var block in lastAcceptedENSMessage.MessageBlock.MessageBlocks)
					{
						if (block is AENS40)
						{
							ens40Block = (AENS40)block;
						}
						else if (ens40Block != null && block is AENS62 ens62Block)
						{
							if (ens40Block.FeeExemptionCode != "1" && ens62Block.AccountingClassCode == Core.Constants.USCustoms.FeeCodes.Cotton && ens62Block.UserFeeAmount.IsEmpty)
							{
								entry.US_R_CottonFeeMandatory = true;
							}
						}
					}
				}
			}
		}

		void RetrieveImportLineDetails(ReconOriginalEntryHeader entry)
		{
			//First ZGuid: Original Invoice Line PK
			//Second ZGuid: Cloned Invoice Line PK
			var importEntry = reconDeclaration.ReadFactory.Load<CusEntryHeader>(entry.CH_CH_OriginalEntry);

			if (importEntry == null)
			{
				return;
			}

			Dictionary<ZGuid, ZGuid> dictionaryForParentIDReference = new Dictionary<ZGuid, ZGuid>();

			List<CusEntryLine> sorted = new List<CusEntryLine>(new TypedEnumerable<CusEntryLine>(importEntry.MergedLines));
			sorted.Sort(new CusEntryLineComparer());

			foreach (CusEntryLine entryLine in sorted)
			{
				if (!entryLine.US_SupLine)
				{
					JobComInvoiceLine importInvoiceLine = entryLine.RandomLine;

					JobComInvoiceHeader reconInvoice = entry.Invoice;
					JobComInvoiceLine reconLine = importInvoiceLine.Clone(new BusinessObjectCloneArgs(entry.Factory, new string[] { JobComInvoiceLine.Schema.JI_JZ, JobComInvoiceLine.Schema.JI_MatchingKey }, typeof(JobComInvoiceLine), true));

					using (reconLine.GetValidationSuspender())
					{
						reconLine.JI_JZ = reconInvoice.PK;
						reconDeclaration.InvoiceLines.Add(reconLine);

						reconLine.FeeCusCodes.RemoveAndDeleteAll();
						AddChargesToReconLine(reconLine, entryLine);

						ZGuid parentID = entryLine.US_CL_ParentLine.IsValid ? entryLine.US_CL_ParentLine : entryLine.PK;

						if (!dictionaryForParentIDReference.ContainsKey(parentID))
						{
							dictionaryForParentIDReference.Add(parentID, reconLine.PK);
						}

						if (entryLine.IsChildLine)
						{
							ZGuid result;
							dictionaryForParentIDReference.TryGetValue(entryLine.US_CL_ParentLine, out result);

							if (result != reconLine.PK)
							{
								reconLine.JI_ParentID = result;
							}
						}

						if (reconLine.JI_OP != ZGuid.Empty)
						{
							var part = reconLine.Factory.Load<OrgSupplierPart>(reconLine.JI_OP);
							if (part != null && part.OP_IsActive == ZBool.False)
							{
								reconLine.JI_OP = ZGuid.Empty;
							}
						}

						reconLine.US_R_OrigEntryLineNo = entryLine.CL_LineNumber.ToString();
						reconLine.JI_InvoiceQuantity = entryLine.InvoiceQuantity;
						reconLine.JI_InvoiceUQ = entryLine.InvoiceUQ;

						reconLine.JI_CustomsQuantity = entryLine.CustomsQuantity;
						reconLine.JI_CustomsSecondQuantity = entryLine.SecondCustomsQuantity;
						reconLine.JI_CustomsThirdQuantity = entryLine.ThirdCustomsQuantity;

						reconLine.US_R_OrigTariff = importInvoiceLine.JI_Tariff.Left(reconLine.US_R_OrigTariffInfo.MaxLength);
						reconLine.JI_LinePrice = CustomsValueDeciderForInvoiceLine.GetCustomsValue(entryLine);

						reconLine.US_R_OrigFirstUQ = importInvoiceLine.JI_CustomsUnitQty.Left(3);
						reconLine.US_R_OrigFirstQty = reconLine.JI_CustomsQuantity;
						reconLine.US_R_OrigSecondUQ = importInvoiceLine.JI_CustomsSecondUnitQty.Left(reconLine.US_R_OrigSecondUQInfo.MaxLength);
						reconLine.US_R_OrigSecondQty = reconLine.JI_CustomsSecondQuantity;
						reconLine.US_R_OrigThirdUQ = importInvoiceLine.JI_CustomsThirdUnitQty.Left(reconLine.US_R_OrigSecondUQInfo.MaxLength);
						reconLine.US_R_OrigThirdQty = reconLine.JI_CustomsThirdQuantity;
						reconLine.US_R_OrigSPI = importInvoiceLine.US_SPI;
						reconLine.US_R_OrigRateType = importInvoiceLine.US_SelectedRateType;
						reconLine.US_R_OrigCV = CustomsValueDeciderForInvoiceLine.GetCustomsValue(entryLine);
						reconLine.US_R_OrigOverrideDuty = importInvoiceLine.US_OverrideDuty;
						reconLine.US_R_OrigDuty = entryLine.DutyAmount;

						reconLine.US_CottonFeeExempt = reconLine.US_CottonFeeExempt;
						if (importInvoiceLine.HasCottonCertificate)
						{
							reconLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
						}

						reconLine.US_R_OrigCottonFeeExempt = reconLine.US_CottonFeeExempt;
						if (importInvoiceLine.HasCottonCertificate)
						{
							reconLine.US_R_OrigCottonFeeExempt = YesNoDefaultList.Codes.Yes;
						}

						reconLine.US_R_Textile = entryLine.HasTextileCategoryNo;

						reconLine.US_R_OrigTaxApply = reconLine.US_TaxApply;
						reconLine.US_R_OrigTaxCode = reconLine.US_TaxCode;
						reconLine.US_R_OrigTaxRateS = reconLine.US_TaxRateS;
						reconLine.US_R_OrigTaxRate = reconLine.US_TaxRate;
						reconLine.US_R_OrigTaxRateT = reconLine.US_TaxRateT;
						reconLine.US_R_OrigTaxQty = reconLine.US_TaxQty;

						var isACE = entry.IsACE;
						reconLine.US_SecondarySPI = isACE ? importInvoiceLine.US_SetInd :
							((importInvoiceLine.IsSetVLine || importInvoiceLine.IsSetXLine) ? importInvoiceLine.US_SecondarySPI : ZString.Empty);

						reconLine.FDAs.RemoveAndDeleteAll();
						reconLine.DOTs.RemoveAndDeleteAll();
						reconLine.FCCs.RemoveAndDeleteAll();
						reconLine.LaceyActLines.RemoveAndDeleteAll();
						reconLine.AIILines.RemoveAndDeleteAll();
						reconLine.LineGroupingRanges.RemoveAndDeleteAll();
						reconLine.FSISLines.RemoveAndDeleteAll();
						reconLine.VehicleLines.RemoveAndDeleteAll();
						reconLine.PSTLines.RemoveAndDeleteAll();
						reconLine.OMCHeaders.RemoveAndDeleteAll();
						reconLine.ACE_FDALines.RemoveAndDeleteAll();
						reconLine.NHTSALines.RemoveAndDeleteAll();
						reconLine.CPSCHeaders.RemoveAndDeleteAll();
						reconLine.DEAHeaders.RemoveAndDeleteAll();

						var parentLine = entryLine.RandomLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);

						var supLine = parentLine != null && (parentLine.US_SupLine || parentLine.CL_AdValoremTariff.StartsWith("98") || parentLine.CL_AdValoremTariff.StartsWith("99")) ? parentLine : null;

						var hasMPF = entryLine.US_HasMPF;
						if (supLine != null)
						{
							// For recon, no need to differentiate two values, 98 goods value & 98 Value InvCurr
							reconLine.US_98GoodsValue = CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(supLine.CL_AdValoremTariff) ? supLine.CL_CustomsValue : ZDecimal.Zero;
							reconLine.US_98ValueInvCurr = ZDecimal.Zero;

							reconLine.US_SupTariff = supLine.CL_AdValoremTariff.Left(AddInfo.Schema.US_SupTariffMaxLength);
							reconLine.US_SupUQ1 = supLine.CustomsUnitQty.Left(3);
							reconLine.US_SupUQ2 = supLine.SecondCustomsUnitQty;
							reconLine.US_SupUQ3 = supLine.ThirdCustomsUnitQty;

							reconLine.US_SupQty1 = supLine.CustomsQuantity;
							reconLine.US_SupQty2 = supLine.SecondCustomsQuantity;
							reconLine.US_SupQty3 = supLine.ThirdCustomsQuantity;

							reconLine.US_R_OrigSupTariff = reconLine.US_SupTariff;
							reconLine.US_R_OrigSupUQ1 = reconLine.US_SupUQ1;
							reconLine.US_R_OrigSupQty1 = reconLine.US_SupQty1;
							reconLine.US_R_OrigSupUQ2 = reconLine.US_SupUQ2;
							reconLine.US_R_OrigSupQty2 = reconLine.US_SupQty2;
							reconLine.US_R_OrigSupUQ3 = reconLine.US_SupUQ3;
							reconLine.US_R_OrigSupQty3 = reconLine.US_SupQty3;
							reconLine.US_R_Orig98Value = reconLine.US_98GoodsValue;

							reconLine.US_R_OrigOverrideSupDuty = reconLine.US_OverrideSupDuty;
							reconLine.US_R_OrigSupDuty = supLine.DutyAmount;
							hasMPF |= supLine.US_HasMPF;

							AddChargesToReconLine(reconLine, supLine);
						}

						reconLine.US_HasMPF = hasMPF;
						reconLine.US_R_OrigHasMPF = hasMPF;

						reconLine.US_UC_NKCountryOfOrigin = (entryLine.ParentLine ?? entryLine).RandomLine.US_UC_NKCountryOfOrigin;
					}
				}
			}
		}

		static void AddChargesToReconLine(JobComInvoiceLine reconLine, CusEntryLine importEntryLine)
		{
			var feeAndChargeList = CusFeeCodeConstants.GetAccountingClassFeeCodeList(reconLine.Factory);
			foreach (CusEntryLineFee fee in importEntryLine.Fees)
			{
				if (feeAndChargeList.ContainsCode(fee.CF_ChargeType))
				{
					var existingReconAmount = reconLine.FeeCusCodes.GetFeeOrChargeAmount(fee.CF_ChargeType);
					reconLine.FeeCusCodes.UpdateOrAddCharge(fee.CF_ChargeType, existingReconAmount + fee.CF_ChargeAmount);

					var existingOrigAmount = reconLine.ReconOriginalCharges.GetFeeOrChargeAmount(fee.CF_ChargeType);
					reconLine.ReconOriginalCharges.SetAmount(fee.CF_ChargeType, existingOrigAmount + fee.CF_ChargeAmount);
				}
			}
		}

		CusEntryHeader GetImportEntry(ZGuid origianlEntryPK) => reconDeclaration.ReadFactory.Load<CusEntryHeader>(origianlEntryPK);

		void CopyDetailsFromImportEntry(ReconOriginalEntryHeader entry, CusEntryHeader importEntry)
		{
			using (entry.GetValidationSuspender())
			{
				var impDeclaration = importEntry.Declaration;
				if (impDeclaration != null)
				{
					entry.US_ImportDate = impDeclaration.JE_DateOfArrival;
					entry.US_R_ReleaseDate = impDeclaration.JE_EntryAuthorisationDate.IsEmpty ? impDeclaration.US_PresentationDate : impDeclaration.JE_EntryAuthorisationDate;
					entry.US_PaymentDate = impDeclaration.US_PaymentDate.IsEmpty ? impDeclaration.US_PaymentDueDate : impDeclaration.US_PaymentDate;

					entry.US_R_DateForMPFCalc = impDeclaration.DateForMPFCalculation;
					entry.US_PriorDisclosure = impDeclaration.US_PriorDisclosure;
					entry.US_NAFTAClaimStat = impDeclaration.US_NAFTAClaimStat;
					entry.US_ProtestStat = impDeclaration.US_ProtestStat;
					entry.US_R_MonthlyFiling = impDeclaration.US_MonthlyFiling;

					entry.US_SchDEntry = impDeclaration.US_SchDEntry;
					entry.US_R_GoodsDescription = impDeclaration.JE_GoodsDescription;
					entry.US_R_OwnerRef = impDeclaration.JE_OwnerRef;

					//Do this after entry type and transport mode are set
					entry.US_R_IsHMFApplicable = impDeclaration.US_IsHMFApplicable;
					entry.US_R_MsgMode = impDeclaration.JE_ApplicationCode;

					DefaultIssueCodeAndSuretyCodeIfRequired(impDeclaration);
					RetrieveEntryDetailsFromENSEntryIfNecessary(entry, importEntry);
				}

				entry.US_R_DutyRateDate = importEntry.US_DutyCalcDate;
				CopyDutiesAndFees(entry, importEntry);
			}
		}

		void CopyDutiesAndFees(ReconOriginalEntryHeader entry, CusEntryHeader importEntry)
		{
			foreach (CusEntryHeaderCharges charge in importEntry.Charges)
			{
				entry.OriginalCharges.UpdateOrAddCharge(charge.C1_ChargeType, charge.C1_ChargeAmount);

				entry.AddAggregateFeesIfNecessary();
			}

			entry.OriginalCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.Duty, importEntry.TotalDutyAmount);

			foreach (ReconEntryOriginalCharge charge in entry.OriginalCharges)
			{
				entry.ReconCharges.UpdateOrAddCharge(charge.CY_Code, charge.CY_Amount);
			}
		}

		void DefaultIssueCodeAndSuretyCodeIfRequired(JobDeclaration impDeclaration)
		{
			if (reconDeclaration.IOROrgPK.IsEmpty)
			{
				reconDeclaration.IOROrgPK = impDeclaration.IOROrgPK;
			}

			if (reconDeclaration.US_IssueCode.IsEmpty || (reconDeclaration.US_IssueCode == ReconIssueCodeList.Codes.NotApplicable && !impDeclaration.US_OtherReconIndicator.IsEmpty))
			{
				reconDeclaration.US_IssueCode = impDeclaration.US_OtherReconIndicator;
			}

			if (reconDeclaration.US_IssueCode.IsEmpty && impDeclaration.US_NAFTAReconIndicator)
			{
				reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			}

			if (reconDeclaration.US_SuretyCode.IsEmpty)
			{
				reconDeclaration.US_SuretyCode = impDeclaration.US_SuretyCode;
			}

			reconDeclaration.RefreshBinding();
		}

		#endregion
	}
}
