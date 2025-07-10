using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.Business
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override void SortMergedLineInvoiceLines(Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> mergedLines)
		{
			foreach (CusEntryLine mergedLine in mergedLines)
			{
				mergedLine.SortInvoiceLines();
			}
		}
		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			var result = new Customs.Business.EntryCreationStrategy[]
			{
					new FTZMergeStrategy(Declaration),

					new SupplementaryTariffLineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone, delegate { return new FTZLineKeyGenerator(); } ),

					//Keep the order of strategies, ENS > CRL > INB
					//If applicable, ENS entries should be created first as CH_CH_PrimeEntry is the link from CRL entries to ENS entries

					new ENSMergeStrategy(Declaration),

					new SupplementaryTariffLineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, delegate { return new ENSLineKeyGenerator(); } ),

					new SupplementaryAdditionalTariff1LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, delegate { return new ENSLineKeyGenerator(); } ),

					new SupplementaryAdditionalTariff2LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, delegate { return new ENSLineKeyGenerator(); } ),

					new SupplementaryAdditionalTariff3LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, delegate { return new ENSLineKeyGenerator(); } ),

					new SupplementaryAdditionalTariff4LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, delegate { return new ENSLineKeyGenerator(); } ),

					new SupplementaryAdditionalTariff5LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.EntrySummary, delegate { return new ENSLineKeyGenerator(); } ),

					new BCRMergeStrategy(Declaration),

					new SupplementaryTariffLineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease, delegate { return new CargoReleaseLineMergeKeyGenerator(); }),

					new CRLMergeStrategy(Declaration),

					new SupplementaryTariffLineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.CargoRelease, delegate { return new CargoReleaseLineMergeKeyGenerator(); }),

					new SimplifiedEntryStrategy(Declaration),

					new SupplementaryTariffLineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, delegate { return new SimplifiedEntryLineMergeKeyGenerator(); }),

					new SupplementaryAdditionalTariff1LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, delegate { return new SimplifiedEntryLineMergeKeyGenerator(); }),

					new SupplementaryAdditionalTariff2LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, delegate { return new SimplifiedEntryLineMergeKeyGenerator(); }),

					new SupplementaryAdditionalTariff3LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, delegate { return new SimplifiedEntryLineMergeKeyGenerator(); }),

					new SupplementaryAdditionalTariff4LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, delegate { return new SimplifiedEntryLineMergeKeyGenerator(); }),

					new SupplementaryAdditionalTariff5LineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, delegate { return new SimplifiedEntryLineMergeKeyGenerator(); }),

					new INBMergeStrategy(Declaration),

					new SupplementaryTariffLineMergeStrategy(Declaration, CusEntryHeaderMessageTypeList.Codes.InBond, delegate { return new INBLineKeyGenerator(); }),

					new AESMergeStrategy(Declaration)
			};

			return result;
		}

		protected override void OnMerging()
		{
			base.OnMerging();
			var declaration = this.Declaration;
			if (declaration.IsImport)
			{
				var data = (USLinkedToDeclarationData)declaration.GetNewLinkedToDeclarationData();
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					invoiceLine.UpdateOGAPGADetailsOnMessaging(data);
				}
			}

			declaration.InvoiceLines.ClearCalculateException();
		}

		protected override void OnMerged()
		{
			base.OnMerged();

			//inbond only and all bills are short-formed and invoice lines have not been entered
			if (Declaration.IsInBondOnly && Declaration.ActiveEntryHeaders.Count == 0)
			{
				CusEntryHeader inbondEntry = Declaration.ActiveEntryHeaders.AddNew();
				inbondEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			}

			if (Declaration.IsImport)
			{
				foreach (JobComInvoiceHeader invoice in Declaration.Invoices)
				{
					invoice.JobComInvoiceLines.RefreshFullLineNumberRangeList();

					foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
					{
						foreach (FeeCusCodeData feeData in invoiceLine.FeeCusCodes.ToArray())
						{
							if (feeData.CY_FeeAmount == ZDecimal.Zero && !feeData.CY_IsOverridden && !feeData.IsMandatory)
							{
								feeData.Delete();
							}
							if (!feeData.IsDeleted)
							{
								feeData.Validation.ValidateCY_FeeAmount();
							}
						}
					}
				}

				foreach (Bill bill in Declaration.Bills)
				{
					bill.RefreshAfterMerge();
				}

				if (Declaration.US_EnableENS)//Entry Summary Entries have been created
				{
					Declaration.DefaultBondAmountForSingleTransactionBond();
					Declaration.CalculateTotalEnteredValue();
				}
			}

			CalculateFinalDestinationFromLineWithMaxValue();//Customs value should have been set

			if (Declaration.IsExport)
			{
				if (string.IsNullOrEmpty(Declaration.JobNumber))
				{
					Declaration.AssignAESShipmentNumberOnSaving = true;
				}
				else
				{
					UpdateShipmentNoAndStatus(Declaration);
				}
			}
		}

		void CalculateFinalDestinationFromLineWithMaxValue()
		{
			if (Declaration.IsImport)
			{
				CusEntryHeader entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

				if (entry != null)
				{
					ZString result = Declaration.US_DestinationState;

					ZDecimal maxValue = 0m;

					foreach (CusEntryLine entryLine in entry.MergedLines)
					{
						if (!entryLine.IsSecondaryTariffLine)
						{
							if (maxValue == 0m || maxValue < entryLine.CL_CustomsValue)
							{
								maxValue = entryLine.CL_CustomsValue;
								result = entryLine.DestinationState;
							}
						}
					}

					entry.US_DestinationState = result;
				}
			}
		}

		protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();

			if (Declaration.IsImport)
			{
				new US_CL_ParentLineManager().Manage(Declaration);

				new DDPDisbursementChargeCalculator(Declaration).Calculate();

				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					if (entry.IsFormalEntry || entry.IsCargoRelease || entry.IsBorderCargoRelease || entry.IsFTZAdmission || entry.IsACECargoRelease)
					{
						new USCustomsValueRoundingTool().Round(entry);
					}
				}
			}
		}

		protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();
			if (Declaration.IsFormalImport || Declaration.IsFTZAdmission)
			{
				new DDPDisbursementChargeCalculator(Declaration).ReCalculateDDD();
			}
		}

		protected override void CalculateDuties()
		{
			new LineCustomsValues().Calculate(Declaration);

			new OGAInvValueConverter().Convert(Declaration);
			var entryDutyCalculator = new EntryDutyCalculator(Declaration);
			entryDutyCalculator.CalculateActualDuty();
			entryDutyCalculator.CalculateDutyReportingDuty();
		}

		protected override Customs.Business.ILineNumberAssigner GetLineNumberAssigner(Customs.Business.CusEntryHeader entryHeader)
		{
			if (Declaration.IsFTZAdmission)
			{
				return new FTZLineNumberAssigner(Declaration.ActiveEntryHeaders.FTZEntry);
			}
			else
			{
				return new LineNumberAssigner(entryHeader);
			}
		}

		internal static void UpdateShipmentNoAndStatus(JobDeclaration declaration)
		{
			var bgmReferences = new Dictionary<ZGuid, ZString>();
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				if (!entry.CH_BGMReference.IsEmpty)
				{
					bgmReferences.Add(entry.PK, entry.CH_BGMReference);
				}
			}

			var bgmRefLastUsed = declaration.US_AESBGMRefLastUsed;

			var helper = new BGMReferenceCalculateHelper();
			helper.BGMRefPrefixStringLastUsed = BGMPrefix;
			helper.IsBGMRefPrefixStringLastUsedFromJobNumber = false;
			helper.BGMRefSuffixCodeLastUsed = -1;

			if (bgmRefLastUsed.StartsWith(declaration.JobNumber)
				|| (bgmRefLastUsed.IsEmpty && declaration.JobNumber.Length > 0 && declaration.JobNumber.Length <= CusEntryHeader.Schema.AESTIRCH_BGMReferenceMaxLength))
			{
				helper.BGMRefPrefixStringLastUsed = declaration.JobNumber;
				helper.IsBGMRefPrefixStringLastUsedFromJobNumber = true;

				if (!bgmRefLastUsed.IsEmpty)
				{
					helper.BGMRefSuffixCodeLastUsed = ConvertSuffixStringToSuffixCode(bgmRefLastUsed.Right(bgmRefLastUsed.Length - declaration.JobNumber.Length));
				}
			}

			helper.BGMRefSuffixStringLength = Math.Min(2, CusEntryHeader.Schema.AESTIRCH_BGMReferenceMaxLength - helper.BGMRefPrefixStringLastUsed.Length);

			foreach (var entry in declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Where(e => e.IsExport))
			{
				if (entry.CH_BGMReference.IsEmpty || entry.IsCurrentlyWithdrawn)
				{
					entry.CH_BGMReference = GetNewBGMReference(entry, helper, bgmReferences);
					bgmRefLastUsed = entry.CH_BGMReference;
				}

				if (entry.US_XTN.IsEmpty)
				{
					UpdateXTN(entry);
				}

				SEDMessageManager manager = new SEDMessageManager(entry);
				entry.US_ShouldBeReportToCustoms = manager.CanSendOriginal;
				if (manager.RequiresAmendment() && !entry.IsCurrentlyWithdrawn)
				{
					entry.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired;
					entry.US_ShouldBeReportToCustoms = true;
				}
			}

			declaration.US_AESBGMRefLastUsed = bgmRefLastUsed.Left(CusEntryHeader.Schema.AESTIRCH_BGMReferenceMaxLength);
		}

		static ZString GetNewBGMReference(CusEntryHeader entry, BGMReferenceCalculateHelper helper, Dictionary<ZGuid, ZString> bgmReferences)
		{
			if (entry.IsCurrentlyWithdrawn) // replace current reference in dictionary so a new reference will be generated. Cannot re-send entry with same reference after it has been withdrawn
			{
				bgmReferences.Remove(entry.PK);
				bgmReferences.Add(ZGuid.NewZGuid(), entry.CH_BGMReference);
			}

			var newReference = ZString.Empty;
			do
			{
				helper.BGMRefSuffixCodeLastUsed += 1;
				if (helper.BGMRefSuffixCodeLastUsed >= helper.BGMRefSuffixCodeUpper)
				{
					var tempBGMReferencePrefix = helper.BGMRefPrefixStringLastUsed;
					while (helper.BGMRefPrefixStringLastUsed == tempBGMReferencePrefix)
					{
						helper.BGMRefPrefixStringLastUsed = BGMPrefix;
					}
					helper.IsBGMRefPrefixStringLastUsedFromJobNumber = false;
					helper.BGMRefSuffixCodeLastUsed = 0;
					helper.BGMRefSuffixStringLength = 2;
				}

				if (helper.IsBGMRefPrefixStringLastUsedFromJobNumber && helper.BGMRefSuffixCodeLastUsed == 0)
				{
					newReference = helper.BGMRefPrefixStringLastUsed;
				}
				else
				{
					newReference = helper.BGMRefPrefixStringLastUsed + ConvertSuffixCodeToSuffixString(helper.BGMRefSuffixCodeLastUsed, helper.BGMRefSuffixStringLength);
				}
			}
			while (bgmReferences.ContainsValue(newReference));

			bgmReferences.Add(entry.PK, newReference);
			return newReference;
		}

		#region Conversion of Number Systems

		static int ConvertSuffixStringToSuffixCode(ZString suffixString)
		{
			var number = 0;

			foreach (var ch in suffixString)
			{
				if ('0' <= ch && ch <= '9')
				{
					number = number * 36 + ch - '0';
				}
				else
				{
					number = number * 36 + ch - 'A' + 10;
				}
			}

			return number;
		}

		static ZString ConvertSuffixCodeToSuffixString(int suffixCode, int width = 0)
		{
			List<char> suffixChars = new List<char>();

			while (suffixCode > 0)
			{
				var remaining = suffixCode % 36;

				if (remaining < 10)
				{
					suffixChars.Add(Convert.ToChar(remaining + '0'));
				}
				else
				{
					suffixChars.Add(Convert.ToChar(remaining - 10 + 'A'));
				}

				suffixCode /= 36;
			}

			suffixChars.Reverse();

			return string.Concat(suffixChars).PadLeft(width, '0');
		}

		#endregion

		static ZString BGMPrefix
		{
			get { return Env.CurrentUser.Initials.PadRight(3, '0') + ZDateTime.Now.ToString("yyMMddHHmmss"); }
		}

		static void UpdateXTN(CusEntryHeader entry)
		{
			var entryFilerID = USCustomsDataRegistry.Instance.ExportEntryFilerID.GetFallBackValueAtAllLevels(entry.RegistryCompanyPK, entry.RegistryBranchPK, Guid.Empty).EntryFilerID.KeepAlphanumericCharacters();
			if (!entry.CH_BGMReference.IsEmpty && !entryFilerID.IsEmpty)
			{
				entry.US_XTN = entryFilerID + "-" + entry.CH_BGMReference;
			}
		}

		class BGMReferenceCalculateHelper
		{
			public BGMReferenceCalculateHelper()
			{
			}

			public ZString BGMRefPrefixStringLastUsed { get; set; }

			public bool IsBGMRefPrefixStringLastUsedFromJobNumber { get; set; }

			public int BGMRefSuffixCodeLastUsed { get; set; }

			public int BGMRefSuffixStringLength { get; set; }

			public int BGMRefSuffixCodeUpper => Convert.ToInt32(Math.Pow(36, BGMRefSuffixStringLength));
		}
	}
}
