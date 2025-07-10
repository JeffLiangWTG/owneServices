using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class ReconDutyFeeCalculationManager
	{
		public ReconDutyFeeCalculationManager(ReconDeclaration reconDeclaration)
		{
			this.reconDeclaration = reconDeclaration;
			hasChangesHunters = new Dictionary<ReconOriginalEntryHeader, HasChangesHunter>();
		}

		readonly ReconDeclaration reconDeclaration;
		readonly Dictionary<ReconOriginalEntryHeader, HasChangesHunter> hasChangesHunters;

		public void CalculateAll()
		{
			if (!reconDeclaration.IsNoChangeAggregate)
			{
				Calculate(new TypedEnumerable<ReconOriginalEntryHeader>(reconDeclaration.OriginalEntries));
			}
		}

		public void CalculateOnlyChangedEntries()
		{
			if (!reconDeclaration.IsNoChangeAggregate)
			{
				var changedEntries = new List<ReconOriginalEntryHeader>();

				foreach (ReconOriginalEntryHeader entry in reconDeclaration.OriginalEntries)
				{
					HasChangesHunter hunter;
					if (!hasChangesHunters.TryGetValue(entry, out hunter))
					{
						hunter = new HasChangesHunter(entry, GetTypesWhichDoNotEffectMerge().ToArray());
						hasChangesHunters.Add(entry, hunter);
					}

					if (hunter.HasChangesSinceLastMark)
					{
						changedEntries.Add(entry);
					}
				}

				Calculate(changedEntries);
			}
		}

		void Calculate(IEnumerable<ReconOriginalEntryHeader> entries)
		{
			OnCalculating(entries);

			DutyFeeCalculationManager manager = new DutyFeeCalculationManager(reconDeclaration);
			foreach (ReconOriginalEntryHeader entry in entries)
			{
				if (entry.ShouldDutiesFeesBeCalculated)
				{
					var dataLineHeader = new IDutyDataLineHeader[] { new ReconOriginalDutyDataLineHeader(entry), new ReconCurrentDutyDataLineHeader(entry) };
					manager.Calculate(dataLineHeader);
				}
			}

			OnCalculated(entries);
		}

		void OnCalculating(IEnumerable<ReconOriginalEntryHeader> entries)
		{
			foreach (ReconOriginalEntryHeader entry in entries)
			{
				entry.ResetReconChargesIfNecessary();
			}
		}

		void OnCalculated(IEnumerable<ReconOriginalEntryHeader> entries)
		{
			List<IReconInterestDataProvider> dataProviders = new List<IReconInterestDataProvider>();

			bool shouldIncludeAllEntries =
					reconDeclaration.US_IsAggregate && reconDeclaration.HasChanges ||
					!reconDeclaration.US_IsAggregate && reconDeclaration.HasAddInfoChangesSinceLastSaving(USAddInfoSchema.US_PreliminaryStatementPrintDate);

			foreach (ReconOriginalEntryHeader originalEntry in reconDeclaration.OriginalEntries)
			{
				if (shouldIncludeAllEntries || entries.Contains(originalEntry))
				{
					dataProviders.Add(new ReconInterestDataProviderReconOriginalEntry(originalEntry));
				}

				RefreshEntrySummaryValues(originalEntry);
			}

			new ReconInterestCalculator(new ReconInterestDataProviderReconDec(reconDeclaration)).Execute(dataProviders);

			new ReconEntryPayableAmountCalculator().Calculate(reconDeclaration);

			RefreshReconSummaryValues();
		}

		void RefreshEntrySummaryValues(ReconOriginalEntryHeader originalEntry)
		{
			originalEntry.OriginalDutyInfo.RefreshBinding();
			originalEntry.ReconDutyInfo.RefreshBinding();
			originalEntry.OriginalFeeInfo.RefreshBinding();
			originalEntry.ReconFeeInfo.RefreshBinding();
			originalEntry.OriginalTaxInfo.RefreshBinding();
			originalEntry.ReconTaxInfo.RefreshBinding();
			originalEntry.ReconInterestInfo.RefreshBinding();
		}

		void RefreshReconSummaryValues()
		{
			reconDeclaration.TotalOriginalDutyInfo.RefreshBinding();
			reconDeclaration.TotalReconDutyInfo.RefreshBinding();
			reconDeclaration.TotalDutyDifferenceInfo.RefreshBinding();
			reconDeclaration.TotalOriginalFeeInfo.RefreshBinding();
			reconDeclaration.TotalReconFeeInfo.RefreshBinding();
			reconDeclaration.TotalFeeDifferenceInfo.RefreshBinding();
			reconDeclaration.TotalOriginalTaxInfo.RefreshBinding();
			reconDeclaration.TotalReconTaxInfo.RefreshBinding();
			reconDeclaration.TotalTaxDifferenceInfo.RefreshBinding();
			reconDeclaration.InterestPaymentAmountInfo.RefreshBinding();
		}

		protected virtual List<HasChangesHunterExclusionDetails> GetTypesWhichDoNotEffectMerge()
		{
			List<HasChangesHunterExclusionDetails> result = new List<HasChangesHunterExclusionDetails>();
			result.Add(new HasChangesHunterExclusionDetails(typeof(StmALog)));
			return result;
		}
	}
}
