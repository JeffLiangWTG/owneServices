using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using BaseEntryCreationStrategy = Enterprise.Customs.Business.EntryCreationStrategy;

namespace Enterprise.Customs.ZA.Business
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		#region Overrides

		protected override BaseEntryCreationStrategy[] GetEntryCreationStrategies() => new BaseEntryCreationStrategy[] { new EntryCreationStrategy(Declaration) };

		protected override void UpdateProvisionalPayments()
		{
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				var firstEntryLine = entryHeader.MergedLines.Cast<CusEntryLine>().OrderBy(x => x.CL_LineNumber).FirstOrDefault();

				if (firstEntryLine != null)
				{
					if (!firstEntryLine.ProvisionalPayments.OfType<ProvisionalPaymentAmountCodeData>().Any(x => x.CY_Code == LineLevelProvisionalPayments.Codes.PPE))
					{
						var provisionalPaymentPayInfo = entryHeader.EntryInstruction?.ProvisionalPaymentPayInfos.FirstOrDefault(x => x.C9_TransactionType == LineLevelProvisionalPayments.Codes.PPE);
						if (provisionalPaymentPayInfo != null && !provisionalPaymentPayInfo.C9_RemAdvReceived)
						{
							var payment = firstEntryLine.ProvisionalPayments.AddNew();
							payment.CY_Code = LineLevelProvisionalPayments.Codes.PPE;
							payment.CY_Value = provisionalPaymentPayInfo.C9_PaymentAmount;
							payment.ReadOnly = true;
						}
					}
				}
			}
		}

		protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration, valaValueRebateCalculator, dutyRebateCalculator);

		protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
			valaValueRebateCalculator = new ValaValueRebateCalculator(Declaration);
			dutyRebateCalculator = new DutyRebateCalculator(Declaration);

			Declaration.RoundCustomsValueForAllMergedLines();
			if (Declaration.IsImportByExternalBroker)
			{
				foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
				{
					entryHeader.EntryNumber = entryHeader.CH_BGMReference = entryHeader.EntryInstruction?.CEI_PreviousMRN ?? ZString.Empty;
				}
			}

			AddFetchHintForCusEntryPayInfo();
		}

		void AddFetchHintForCusEntryPayInfo()
		{
			var factory = Declaration.Factory;
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				var query = ProvisionalPaymentEntryPayInfoCollection.GetFetchHintQueryForProvisionalPaymentPayInfos(factory, entryHeader);
				factory.AddFetchHint(CusEntryPayInfoSchema.Instance, query);
			}
		}

		protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();

			var dutyCalculatorStrategy = new DutyCalculatorStrategy(Declaration, valaValueRebateCalculator, dutyRebateCalculator);
			dutyCalculatorStrategy.CalculateBND();
			SetActualPriceAddInfo();

			AdditionalInformationCalculator.ResetRCCCertificateValues(Declaration);
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				DefaultPaymentMethod(entryHeader);
				DefaultCH_PackagesIfNeeded(entryHeader);
				DefaultCH_TotalEntries(entryHeader);
				DefaultCH_EntryNumber(entryHeader);

				CusEntryLine entryLine = null;
				foreach (CusEntryLine currentEntryLine in entryHeader.MergedLines)
				{
					var invoiceLine = currentEntryLine.RandomLine;
					if (invoiceLine != null)
					{
						var additionalDesc = " " + (currentEntryLine.RebateDescription + " " + invoiceLine.AdditionalDescriptionForTariffDescription).Trim();
						currentEntryLine.CL_Description = currentEntryLine.CL_Description.Left(350 - additionalDesc.Length) + additionalDesc;
					}
					AdditionalInformationCalculator.Calculate(currentEntryLine);
					if (entryLine == null)
					{
						entryLine = currentEntryLine;
					}
				}

				if (entryLine != null)
				{
					entryHeader.UniqueConsignmentReference = entryLine.EntryInstruction?.CEI_UCROverride ?? ZString.Empty;
				}
				entryHeader.CIFInLocalCurrencyRoundedInfo.RefreshBinding();
				entryHeader.CustomsValueInfo.RefreshBinding();
				entryHeader.CustomsDutyExcluding12BAfterInfo.RefreshBinding();
				entryHeader.S1P2BDutyAfterInfo.RefreshBinding();
				entryHeader.ValueAddedTaxInfo.RefreshBinding();
				entryHeader.ProvisionalPaymentAmountAfterInfo.RefreshBinding();
				entryHeader.PenaltyAmountAfterInfo.RefreshBinding();
			}

			SubmissionHelper.CalculateDeferralAccountAndPaymentMethod();
			valaValueRebateCalculator = null;
			dutyRebateCalculator = null;
		}

		void SetActualPriceAddInfo()
		{
			foreach (JobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
			{
				Declaration.Factory.AddFetchHint(JobComInvHeaderChargeSchema.J7_ParentID, invoiceLine.PK);
				invoiceLine.JI_ActualPrice = invoiceLine.JI_Calc_ActualPrice;
			}
		}

		protected override void OnMerged()
		{
			var allowAutomaticSplitEntriesByBondAmount = ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.Value;
			var percentageOfBondAmountUsedBeforeSplit = ZACustomsRegistry.Instance.PercentageOfBondAmountUsedBeforeSplit.Value;

			if (!Declaration.HasSplitByBondAmount && DoesNeedFurtherSplittingDueToBNDValuesExceedingBondGuaranteeValues(allowAutomaticSplitEntriesByBondAmount, percentageOfBondAmountUsedBeforeSplit))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Declaration.Logs.AddNew(Events.EditedARecord, "Entry BND values exceed the bond guarantee value of the holder - attempting to split entries");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var splitByBondAmounts = new Dictionary<ZGuid, SplitByBondAmountObject>();
				var bondGuaranteeValues = new Dictionary<ZGuid, ZDecimal>();
				foreach (var instruction in Declaration.CustomsEntryInstructions)
				{
					bondGuaranteeValues.Add(instruction.PK, instruction.BondGuaranteeValue);
				}

				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					var firstLine = true;
					foreach (var entryLine in entry.MergedLines)
					{
						CusEntryInstruction currentEntryInstructionBeingUsed = null;
						var currentEntryLineInstruction = entryLine.EntryInstruction;
						var bnd = entryLine.BND;
						var hasSplitByBondAmount = splitByBondAmounts.ContainsKey(entryLine.EntryInstruction.PK);

						if (hasSplitByBondAmount)
						{
							splitByBondAmounts[entryLine.EntryInstruction.PK].TotalGuaranteeValue += bnd;
						}
						else
						{
							splitByBondAmounts.Add(entryLine.EntryInstruction.PK, new SplitByBondAmountObject
							{
								OriginalEntryInstruction = currentEntryLineInstruction,
								TotalGuaranteeValue = bnd
							});
						}

						var splitObject = splitByBondAmounts[entryLine.EntryInstruction.PK];
						currentEntryInstructionBeingUsed = splitObject.CopiedInstructions.LastOrDefault() ?? splitObject.OriginalEntryInstruction;

						if (!firstLine && splitObject.TotalGuaranteeValue > (currentEntryInstructionBeingUsed.BondGuaranteeValue * percentageOfBondAmountUsedBeforeSplit / 100))
						{
							var clonedInstruction = currentEntryInstructionBeingUsed.Clone() as CusEntryInstruction;
							Declaration.CustomsEntryInstructions.Add(clonedInstruction);
							clonedInstruction.CEI_JE = Declaration.PK;
							entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x =>
							{
								using (x.SuspendAutomaticSplitByBondAmountCEISetter())
								{
									x.JI_CEI = clonedInstruction.PK;
								}
							});
							clonedInstruction.BHValid = true;
							clonedInstruction.CEI_Description = ZString.Format("Copy {0} of {1}", splitObject.CopiedInstructions.Count + 1, splitObject.OriginalEntryInstruction.CEI_Description);
							clonedInstruction.OH_SubContractor = currentEntryInstructionBeingUsed.OH_SubContractor;
							clonedInstruction.CEI_RemoverEDI = currentEntryInstructionBeingUsed.CEI_RemoverEDI;
							splitObject.CopiedInstructions.Add(clonedInstruction);
							splitObject.TotalGuaranteeValue = bnd;
						}
						else
						{
							firstLine = false;
							if (!entryLine.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.JI_CEI == currentEntryInstructionBeingUsed.PK))
							{
								entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.JI_CEI = currentEntryInstructionBeingUsed.PK);
							}
						}
					}
				}

				Declaration.HasSplitByBondAmount = ZBool.True;
				Declaration.DoMerge();
			}
			else
			{
				base.OnMerged();

				SplitByMaxEntryLines();

				Declaration.HasSplitByBondAmount = ZBool.False;
				Declaration.HasSplitByMaxEntryLines = ZBool.False;
				Declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.RefreshBinding();
			}
		}

		void SplitByMaxEntryLines()
		{
			var maxEntryLines = ZACustomsRegistry.Instance.ExbondMaxNumberEntryLines.Value;

			if (!Declaration.HasSplitByMaxEntryLines && DoesNeedtoSplitBasedOnMaxEntryLines(maxEntryLines))
			{
				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					if (entry.MergedLines.Count > maxEntryLines)
					{
						int index = 1;
						SplitEntryLinesByMaxEntryLines(entry, entry.MergedLines, maxEntryLines, index);
					}
				}

				Declaration.HasSplitByMaxEntryLines = ZBool.True;
				Declaration.DoMerge();
			}
		}

		void SplitEntryLinesByMaxEntryLines(CusEntryHeader entry, IEnumerable<CusEntryLine> entryLines, int maxEntryLines, int index)
		{
			var entryLinesLeft = entryLines.Skip(maxEntryLines);
			var currentBatch = entryLinesLeft.Take(maxEntryLines);
			var clonedInstruction = entry.EntryInstruction.Clone() as CusEntryInstruction;
			clonedInstruction.CEI_Description = ZString.Format("Copy {0} of {1}", index, entry.EntryInstruction.CEI_Description);
			Declaration.CustomsEntryInstructions.Add(clonedInstruction);
			clonedInstruction.CEI_JE = Declaration.PK;
			currentBatch.ForEach(x => x.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(y =>
			{
				using (y.SuspendAutomaticSplitByBondAmountCEISetter())
				{
					y.JI_CEI = clonedInstruction.PK;
				}
			}));

			if (entryLinesLeft.Count() > maxEntryLines)
			{
				SplitEntryLinesByMaxEntryLines(entry, entryLinesLeft, maxEntryLines, ++index);
			}
		}

		bool DoesNeedtoSplitBasedOnMaxEntryLines(int maxEntryLines) => maxEntryLines > 0 && Declaration.IsExWarehouse && Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.MergedLines.Count > maxEntryLines);

		bool DoesNeedFurtherSplittingDueToBNDValuesExceedingBondGuaranteeValues(bool automaticSplitting, int percentageOfBondAmountToBeUsed)
		{
			var needsSplitting = false;

			if (automaticSplitting)
			{
				needsSplitting = Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.MergedLines.Sum(s => s.BND) > x.EntryInstruction.BondGuaranteeValue * percentageOfBondAmountToBeUsed / 100);
			}

			return needsSplitting;
		}

		#endregion

		#region Implementation

		#region PerformCountrySpecificOperationAfterMergeAfterCalculateDuty

		#region Default UZ_EntryNumber

		void DefaultCH_EntryNumber(CusEntryHeader entryHeader)
		{
			var result = ZInt.Zero;
			var index = Declaration?.ClearanceParts?.IndexOf(entryHeader);
			if (index.HasValue)
			{
				result = index.Value + 1;
			}
			entryHeader.CH_EntryNumber = result;
		}

		#endregion

		#region Default UZ_TotalEntries

		void DefaultCH_TotalEntries(CusEntryHeader entryHeader)
		{
			entryHeader.CH_TotalEntries = Declaration.ClearanceParts.Count;
		}

		#endregion

		#region Default UZ_Packages

		void DefaultCH_PackagesIfNeeded(CusEntryHeader entryHeader)
		{
			if (Declaration.ActiveEntryHeaders.Count == 1)
			{
				ZInt result = ZInt.Zero;
				var containers = entryHeader.Containers;
				var hasFCLOnly = containers.Length > 0 && containers.OfType<CusContainer>().All(x => x.CO_FCL_LCL_AIR == Core.Constants.ContainerModes.FCL);

				if (hasFCLOnly)
				{
					result = containers.Length;
				}
				else
				{
					result = Declaration.JE_TotalNoOfPacks;
				}
				entryHeader.CH_Packages = result;
			}
		}

		#endregion

		void DefaultPaymentMethod(CusEntryHeader entryHeader)
		{
			var totalAmountDueDifference = entryHeader.AmountDueDifference;
			var dutyDifference = entryHeader.CustomsDutyExcluding12BDifference + entryHeader.S1P2BDutyDifference;
			var taxDifference = entryHeader.ValueAddedTax;
			var ppDifference = entryHeader.ProvisionalPaymentAmountDifference + entryHeader.PenaltyAmountDifference;

			if (totalAmountDueDifference.IsEmpty || totalAmountDueDifference <= 0)
			{
				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Free;
			}
			else if (dutyDifference == 0 && taxDifference == 0 && ppDifference != 0)
			{
				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			}
			else if (SubmissionHelper.CanCalculateDeferralAccountAndPaymentMethod)
			{
				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
			}
			else
			{
				var accountMapping = entryHeader.GetFinancialAccountMapping();
				if (accountMapping != null)
				{
					entryHeader.CH_PaymentMethod = accountMapping.Cash ? PaymentMethodCodeList.Codes.Cash : PaymentMethodCodeList.Codes.Defer;
				}
				else
				{
					entryHeader.CH_PaymentMethod = ZString.Empty;
				}
			}
		}

		DeferredSubmissionHelper SubmissionHelper => fSubmissionHelper ?? (fSubmissionHelper = new DeferredSubmissionHelper(Declaration));
		DeferredSubmissionHelper fSubmissionHelper;

		#endregion

		#endregion

		protected override ILineNumberAssigner GetLineNumberAssigner(Customs.Business.CusEntryHeader entryHeader)
		{
			var header = entryHeader as CusEntryHeader;
			if (header.IsImportByExternalBroker)
			{
				return new LineNumberAssignerForIMX(header);
			}
			else if (!(header.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty).IsEmpty &&
				entryHeader.InvoiceLines.OfType<JobComInvoiceLine>().Any(x => !x.JI_TargetEntryLineNumber.IsEmpty))
			{
				return new LineNumberAssigner(header);
			}
			else
			{
				return base.GetLineNumberAssigner(entryHeader);
			}
		}

		ValaValueRebateCalculator valaValueRebateCalculator;
		DutyRebateCalculator dutyRebateCalculator;

		class SplitByBondAmountObject
		{
			public CusEntryInstruction OriginalEntryInstruction { get; set; }
			public List<CusEntryInstruction> CopiedInstructions = new List<CusEntryInstruction>();
			public ZDecimal TotalGuaranteeValue = 0.0;
		}
	}

	public static class Extension
	{
		public static IEnumerable<ITariffDetail> WhereRateTypeIn(this IEnumerable<ITariffDetail> cusLineTariffDetails, IList<ZString> rateTypes)
		{
			return cusLineTariffDetails.Where(x =>
			{
				var result = false;
				if (rateTypes.Count > 0 && x.UniversalTariffType != null)
				{
					result = x.UniversalTariff?.Rates?.Any(y => rateTypes.Contains(y.ZZ2_ZZR_RateTypeCode)) ?? false;
				}
				return result;
			});
		}

		public static IEnumerable<ITariffDetail> WhereRateType(this IEnumerable<ITariffDetail> cusLineTariffDetails, ZString rateType)
		{
			return cusLineTariffDetails.Where(x => x.UniversalTariff?.Rates.Any(y => y.ZZ2_ZZR_RateTypeCode == rateType) ?? false);
		}

		public static IEnumerable<ITariffDetail> WhereRateTypeNot(this IEnumerable<ITariffDetail> cusLineTariffDetails, ZString rateType)
		{
			return cusLineTariffDetails.Where(x => x.UniversalTariff?.Rates.All(y => y.ZZ2_ZZR_RateTypeCode != rateType) ?? false);
		}
	}
}
