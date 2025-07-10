using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Removes charges where there is another rate line that overrides this one (ie, is better).
	/// </summary>
	public class OverriddenRateLineRemover : SimpleOverriddenRateLinesRemover
	{
		public OverriddenRateLineRemover(RatingCriteria criteria, NotApplicableRateLineRemover.FilterOptions options)
			: this(criteria, options.IsCosting)
		{
			ignoreIncoterms = options.DisablePaymentTermsFilter;
		}

		public OverriddenRateLineRemover(RatingCriteria criteria, bool isCosting)
			: this(criteria, isCosting, false)
		{
		}

		public OverriddenRateLineRemover(RatingCriteria criteria, bool isCosting, bool ignoreMeasures, bool includeContractNumberComparer = false)
			: base(criteria)
		{
			this.isCosting = isCosting;
			this.ignoreMeasures = ignoreMeasures;
			this.includeContractNumberComparer = includeContractNumberComparer;
		}

		#region Comparers List

		internal override LinkedList<BaseRateLineComparer> GetComparers()
		{
			var result = base.GetComparers();
			result.AddFirst(new AgencyCalculatorComparer());
			if (isCosting)
			{
				result.AddAfter(result.Find(new GatewayAgentOrderComparer(Criteria)), new CostsProviderComparer(Criteria));
			}
			else if (!ignoreIncoterms)
			{
				// incoterms prioritization is skipped for scenarios like
				// fmc tariff id discovery.
				result.AddBefore(result.Find(new RateTypeComparer(Criteria)), new IncoTermComparer());
			}

			if (isCosting || !Criteria.IsMultipleClientContractNumberSupported || includeContractNumberComparer)
			{
				result.AddLast(new ContractNumberComparer(Criteria, isCosting));
			}

			result.AddAfter(result.Find(new TransportProviderComparer()), new TransportProviderConsortiumComparer());
			result.AddAfter(result.Find(new ColumnComparer(RateEntrySchema.TI_OH_Supplier)), new ForwarderGroupComparer());
			result.AddAfter(result.Find(new ColumnComparer(RateEntrySchema.TI_OH_Supplier)), new SupplierConsortiumComparer());
			result.AddLast(new NamedAccountComparer(Criteria.NamedAccount));
			result.AddLast(new ContainerQualityComparer(Criteria));

			if (isCosting && Criteria.JobDatesProvider is IJobDatesProviderForCarriers jobDatesProviderForCarriers && jobDatesProviderForCarriers.CarrierTransitTimes.Count > 0)
			{
				result.AddLast(new TransitTimeForCarrierComparer(Criteria));
			}
			else
			{
				result.AddLast(new TransitTimeComparer(Criteria));
			}

			if (Criteria.IsSCNFreight || (Criteria.IsBCNFreight && RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.Value))
			{
				result.AddLast(new BCNOrSCNComparer());
			}

			return result;
		}

		internal override bool IsComparable(FastLine line1, FastLine line2)
		{
			var entry1 = line1.ParentRateEntry;
			var entry2 = line2.ParentRateEntry;

			return base.IsComparable(line1, line2) && entry1.TI_RateCategory == entry2.TI_RateCategory &&
				Criteria.GetFreightLeg(entry1) == Criteria.GetFreightLeg(entry2) &&
				(ignoreMeasures || Criteria.JobMeasures.EqualMeasures(line1, line2));
		}

		readonly bool isCosting;
		readonly bool ignoreMeasures;
		readonly bool includeContractNumberComparer;
		readonly bool ignoreIncoterms;

		#endregion
	}
}
