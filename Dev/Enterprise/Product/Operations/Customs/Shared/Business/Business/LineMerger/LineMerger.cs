using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// This is a generic class to merge invoice lines into customs lines.
	/// For merging criteria for invoices, please refer to HeaderItems.
	/// For merging criteria for invoice lines, please refer to LineItems.
	/// </summary>
	public class LineMerger
	{
		public LineMerger(BaseJobDeclaration declaration)
		{
			this.Declaration = declaration;
			SupportsAmendments = true;
		}

		public bool SupportsAmendments;

		public void DoMerge()
		{
			OnMerging();
			try
			{
				ResetMergedEntryTotalsAndCachedValues();

				Declaration.IsMergeInProgress = true;

				Declaration.Bills.SetMergingInProgress(true);
				Declaration.Invoices.SetMergingInProgress(true);

				MergeRecyclingEntriesWherePossible();

				RefreshCollectionsForPostMergeBizObjs();
				PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();

				foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
				{
					GetLineNumberAssigner(entryHeader).Execute();
					var mergedLines = entryHeader.MergedLines;
					mergedLines.CustomSort();
					SortMergedLineInvoiceLines(mergedLines);
				}
				UpdateProvisionalPayments();
				CalculateDuties();
				UpdateLandedCostOnlyFlagIfNeeded();
				PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();
				PopulateTotalAmountPayableForCusEntryHeaders();
			}
			finally
			{
				Declaration.Invoices.SetMergingInProgress(false);
				Declaration.Bills.SetMergingInProgress(false);
				Declaration.IsMergeInProgress = false;
			}
			OnMerged();
		}

		public readonly BaseJobDeclaration Declaration;

		protected bool NeedsToSetEntryFeeOnDeclaration => !ConsolidatedDeclaration.IsConsolidated(Declaration) || Declaration.IsLeadDeclarationOfConsolidatedDeclarations;

		#region Implementation

		protected virtual void SortMergedLineInvoiceLines(ICusEntryLineCollection<CusEntryLine> mergedLines)
		{
		}

		void MergeRecyclingEntriesWherePossible()
		{
			var strategies = GetEntryCreationStrategies();

			var lines = new List<BaseJobComInvoiceLine>(new TypedEnumerable<BaseJobComInvoiceLine>(Declaration.InvoiceLines));
			var cleanFactory = new ReadOnlyBusinessObjectFactory();
			//needs to go through each strategy even though it is not enabled in order to deactivate or delete entries or entry lines
			//that were created through a now-inactive strategy.
			foreach (var strategy in strategies)
			{
				if (strategy.IsActive)
				{
					strategy.Initiate();
					lines.Sort(strategy.GetInvoiceLineComparerForMerge(cleanFactory));

					for (int i = 0; i < lines.Count; i++) // Changed to make debugging easier - can see progress
					{
						BaseJobComInvoiceLine line = lines[i];
						if (strategy.LineIsValidForMerge(line))
						{
							CusEntryLine entryLine = strategy.GetOrCreateEntryLine(line);
							if (entryLine != null)
							{
								entryLine.MergeInvoiceLine(line);
							}
						}
						else
						{
							strategy.ClearReferenceToEntryLineWhenLineIsNotValidForMerge(line);
						}
					}
				}

				strategy.DiscardUnusedObjects();
			}

			ActiveBusinessObjectCollection<BaseJobComInvoiceLine>.RefreshAll(Declaration.Factory);
		}

		protected void ResetMergedEntryTotalsAndCachedValues()
		{
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders.ToArray())
			{
				entryHeader.ResetTotalsAndCachedValues();
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					entryLine.ResetTotalsAndCachedValues();
				}
			}
		}

		protected virtual void OnMerging()
		{
			if (Declaration.ApportionmentDirty)
			{
				Declaration.ResumeApportionment();
			}
		}

		/// <summary>
		/// Override if you DON'T want to add "Entry lines merged" into the logs
		/// </summary>
		protected virtual bool ShouldLogMerge => true;

		protected virtual void OnMerged()
		{
			if (ShouldLogMerge)
			{
				LogMerge();
			}
		}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		void LogMerge() => Declaration.Logs.AddNew(Events.EditedARecord, "Entry lines merged");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

		protected virtual EntryCreationStrategy[] GetEntryCreationStrategies() => new[] { new EntryCreationStrategy(Declaration) };

		protected void RefreshCollectionsForPostMergeBizObjs()
		{
			Declaration.IsMergeInProgress = false;

			Declaration.ActiveEntryHeaders.Rebuild();

			foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
			{
				entryHeader.AllEntryLines.Load();
				entryHeader.MergedLines.Rebuild();
				entryHeader.PendingDeletionEntryLines.Rebuild();
				entryHeader.DeletedEntryLines.Rebuild();
				entryHeader.RefreshRandomHeader();

				foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
				{
					entryLine.RefreshInvoiceLines();
					entryLine.SetValuesAfterInvoiceLinesAreRebuilt();
				}

				entryHeader.Bills.PopulateBills();
			}
		}

		protected virtual ILineNumberAssigner GetLineNumberAssigner(CusEntryHeader entryHeader)
		{
			return new LineNumberAssigner(entryHeader);
		}

		protected virtual void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
		}

		protected virtual void CalculateInvoiceAmount()
		{
			var declaration = Declaration;
			foreach (CusEntryHeader header in declaration.ActiveEntryHeaders)
			{
				foreach (var line in header.MergedLines)
				{
					line.CL_InvoiceAmount = line.GetInvoicedDocumentaryAmount();
					line.CL_RX_NKInvoiceAmountCurrency = line.GetInvoicedDocumentaryAmountCurrency();
				}
			}
		}

		protected virtual void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
		{
		}

		void UpdateLandedCostOnlyFlagIfNeeded()
		{
			var landedCostOnlyConfigurationProvider = GetLandedCostOnlyConfigurationProvider();
			foreach (CusEntryHeader header in Declaration.ActiveEntryHeaders)
			{
				foreach (CusEntryHeaderCharges charge in header.Charges)
				{
					charge.C1_IsLandedCostOnly = header.IsLandedCostingOnly(charge.C1_ChargeType, landedCostOnlyConfigurationProvider);
				}

				foreach (CusEntryLine line in header.MergedLines)
				{
					foreach (CusEntryLineFee fee in line.Fees)
					{
						fee.CF_IsLandedCostOnly = line.IsLandedCostingOnly(fee.CF_ChargeType, landedCostOnlyConfigurationProvider);
					}
				}
			}
		}

		protected virtual void UpdateProvisionalPayments()
		{
		}

		protected virtual void CalculateDuties()
		{
			GetNewDutyCalculatorStrategy()?.CalculateDuties();
		}

		protected virtual IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);

		protected virtual ILandedCostOnlyConfigurationProvider GetLandedCostOnlyConfigurationProvider() => new LandedCostOnlyConfigurationProviderBase();

		public void PopulateTotalAmountPayableForCusEntryHeaders()
		{
			foreach (CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
			{
				entryHeader.CH_TotalPaid = entryHeader.TotalAmountPayable;
			}
		}
		#endregion
	}
}
