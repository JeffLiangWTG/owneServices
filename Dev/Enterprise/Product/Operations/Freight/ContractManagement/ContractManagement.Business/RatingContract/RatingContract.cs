using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	[CodeProperty(Schema.RCT_ContractNumber)]
	public class RatingContract : AutoRatingContract, IRatingContract, IAutoRateDateByChargeGroupConfiguration
	{
		public RatingContract(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related Business Object Collections

		IRatingContractAllocationLineCollection IRatingContract.Allocations => Allocations;

		public RatingContractAllocationLineCollection Allocations
		{
			get
			{
				if (allocations == null)
				{
					allocations = new RatingContractAllocationLineCollection(this);
				}
				return allocations;
			}
		}
		RatingContractAllocationLineCollection allocations;

		[ChildEditable(true)]
		public IRelatedNamedAccountsPivotCollection<IRatingContractNamedAccountPivot> NamedAccountPivots
		{
			get
			{
				if (namedAccountPivots == null)
				{
					namedAccountPivots = new RelatedNamedAccountsPivotCollection(this);
					RegisterEditableChildObject(namedAccountPivots);
				}

				return namedAccountPivots;
			}
		}
		RelatedNamedAccountsPivotCollection namedAccountPivots;

		IRatingContractContainerDetentionCollection IRatingContract.ContainerDetentions => ContainerDetentions;

		[ChildEditable(true)]
		public RatingContractContainerDetentionCollection ContainerDetentions
		{
			get
			{
				if (containerDetentions == null)
				{
					containerDetentions = new RatingContractContainerDetentionCollection(this);
					RegisterEditableChildObject(containerDetentions);
				}

				return containerDetentions;
			}
		}

		RatingContractContainerDetentionCollection containerDetentions;

		#endregion

		public IOrgHeader ServiceProvider => Factory.Load<IOrgHeader>(RCT_OH);

		public ICarrierContractQuantityUnitPair CarrierContractQuantities
		{
			get
			{
				if (carrierContractQuantities == null)
				{
					carrierContractQuantities = new CarrierContractQuantityUnitPair(
						ContractQuantityTU,
						ContractQuantityContainers);
				}

				return carrierContractQuantities;
			}
		}

		CarrierContractQuantityUnitPair carrierContractQuantities;

		public ICarrierContractQuantityUnitPair CarrierContractCapacityWithVariance
		{
			get
			{
				if (carrierContractCapacityWithVariance == null)
				{
					carrierContractCapacityWithVariance = new CarrierContractQuantityUnitPair(
						ViewRatingContractQuantity?.RCQ_CapacityWithVarianceTU ?? 0,
						ViewRatingContractQuantity?.RCQ_CapacityWithVarianceCN ?? 0);
				}

				return carrierContractCapacityWithVariance;
			}
		}

		CarrierContractQuantityUnitPair carrierContractCapacityWithVariance;

		public ICarrierContractQuantityUnitPair CurrentContractUtilisation
		{
			get
			{
				if (currentContractUtilisation == null)
				{
					currentContractUtilisation = new CarrierContractQuantityUnitPair(
						ViewRatingContractSummary?.RCV_TEUCount ?? 0,
						Convert.ToDecimal(ViewRatingContractSummary?.RCV_ContainerCount ?? 0));
				}

				return currentContractUtilisation;
			}
		}

		CarrierContractQuantityUnitPair currentContractUtilisation;

		public ICarrierContractQuantityUnitPair CurrentContractOutstandingCommitted
		{
			get
			{
				if (currentContractOutstandingCommitted == null)
				{
					currentContractOutstandingCommitted = new CarrierContractQuantityUnitPair(
						ContractQuantityTU - (ViewRatingContractSummary?.RCV_TEUCount ?? 0),
						ContractQuantityContainers - (ViewRatingContractSummary?.RCV_ContainerCount ?? 0));
				}

				return currentContractOutstandingCommitted;
			}
		}

		CarrierContractQuantityUnitPair currentContractOutstandingCommitted;

		public ICarrierContractQuantityUnitPair CurrentContractOutstandingWithVariance
		{
			get
			{
				if (currentContractOutstandingWithVariance == null)
				{
					currentContractOutstandingWithVariance = new CarrierContractQuantityUnitPair(
						(ViewRatingContractQuantity?.RCQ_CapacityWithVarianceTU ?? 0) - (ViewRatingContractSummary?.RCV_TEUCount ?? 0),
						(ViewRatingContractQuantity?.RCQ_CapacityWithVarianceCN ?? 0) - (ViewRatingContractSummary?.RCV_ContainerCount ?? 0));
				}

				return currentContractOutstandingWithVariance;
			}
		}

		CarrierContractQuantityUnitPair currentContractOutstandingWithVariance;

		#region Implementation

		internal ViewRatingContractQuantity ViewRatingContractQuantity => viewRatingContractQuantity
			?? (viewRatingContractQuantity = Factory.LoadTop1<ViewRatingContractQuantity>(new ZQuery(ViewRatingContractQuantitySchema.RCQ_RCT_RatingContract, PK)));

		ViewRatingContractQuantity viewRatingContractQuantity;

		internal ViewRatingContractSummary ViewRatingContractSummary => viewRatingContractSummary
			?? (viewRatingContractSummary = Factory.LoadTop1<ViewRatingContractSummary>(new ZQuery(ViewRatingContractSummarySchema.RCV_RCT_RatingContract, PK)));

		ViewRatingContractSummary viewRatingContractSummary;

		ZDecimal ContractQuantityTU => Convert.ToDecimal(ViewRatingContractQuantity?.RCQ_AllocatedQuantityTU ?? 0);

		ZDecimal ContractQuantityContainers => Convert.ToDecimal(ViewRatingContractQuantity?.RCQ_AllocatedQuantityCN ?? 0);

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("943ab3c2-d438-8383-47a4-24b7565a0071", "Contract {0}", RCT_ContractNumber);
			}
		}

		#region IAutoRateDateByChargeGroupConfiguration

		ZString IAutoRateDateByChargeGroupConfiguration.FilterType => RCT_AutoratingDateFiltering;

		IEnumerable<IAutoRateDate> IAutoRateDateByChargeGroupConfiguration.GetAutoRateDates(string chargeGroup)
		{
			var query = new ZQuery(RatingDateConfigSchema.RDT_ParentTableCode, RatingContractSchema.Constants.Prefix);
			query.AddToFilter(RatingDateConfigSchema.RDT_ParentID, PK);
			query.AddToFilter(RatingDateConfigSchema.RDT_ChargeGroup, chargeGroup);

			var ratingDateConfigs = Factory.Load<RatingDateConfig>(query);

			return ratingDateConfigs;
		}

		#endregion

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			RCT_ContractType = Core.Constants.RatingContractTypes.Client;
			RCT_StartDate = ZDate.Today;
			RCT_EndDate = ZDate.Today.AddDays(30);
			RCT_GS_NKContractOwner = "USR";
		}

#endif
	}
}
