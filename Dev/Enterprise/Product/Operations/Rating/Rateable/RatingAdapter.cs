using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Rateable
{
	public abstract class RatingAdapter : IAutoRating, IAutoRatingAccountingInfo, IRatingAdapter
	{
		#region IAutoRating

		public virtual IJobInvoicingSupporter InvoicingSupporter
		{
			get { return null; }
		}

		public virtual IJobDatesProvider JobDatesProvider
		{
			get { return null; }
		}

		public virtual AutoRatingStatusInfo StatusInformation
		{
			get { return new AutoRatingStatusInfo(true); }
		}

		public virtual ChargeCodeGroupCollection ChargeCodeGroups
		{
			get { return new ChargeCodeGroupCollection(); }
		}

		public virtual JobServicesCollection JobServices
		{
			get { return new JobServicesCollection(); }
		}

		public virtual ZBool IsServicesOnly => false;

		public virtual IContractNumberConfiguration GetContractNumberConfiguration(CostSell costOrSell) => new DefaultContractNumberConfiguration();

		protected class DefaultContractNumberConfiguration : IContractNumberConfiguration
		{
			public bool ShouldAddContractNumberQueryFilter => true;

			public bool ShouldApplySpecificAdapterContractNumberFilter => false;

			public bool ShouldIgnoreJobClientContractNumbers => false;

			public bool ShouldIgnoreJobCarrierContractNumbers => false;

			public bool ShouldMatchJobBlankContractNumber => false;

			public bool ShouldUseCarrierContractDateFilter => false;
		}

		public virtual IEnumerable<ZString> CarrierContractNumbers => Enumerable.Empty<ZString>();

		public virtual IEnumerable<ZString> ClientContractNumbers => Enumerable.Empty<ZString>();

		public virtual ZString HBLDeliveryMode => ZString.Empty;

		public virtual ZString NamedAccount => string.Empty;

		public virtual RateType RateTypeToUse
		{
			get { return new RateType(); }
		}

		public virtual JobInvoicingConsumerType ConsumerType
		{
			get { return null; }
		}

		public virtual MergeChargeOptions MergeCharges
		{
			get { return new MergeChargeOptions(); }
		}

		public virtual Collection<IBusiness> AutoRatedFor
		{
			get { return new Collection<IBusiness>(); }
		}

		public virtual ZInt RouteSetNumber
		{
			get { return 0; }
		}

		public OrgHeader Consignor
		{
			get
			{
				return DebtorOrgs[RatingDebtorOrgTypes.CNR];
			}
		}

		public OrgHeader Consignee
		{
			get
			{
				return DebtorOrgs[RatingDebtorOrgTypes.CNE];
			}
		}

		public OrgHeader BillTo
		{
			get
			{
				return DebtorOrgs[RatingDebtorOrgTypes.LC];
			}
		}

		public OrgHeader Agent
		{
			get
			{
				return DebtorOrgs[RatingDebtorOrgTypes.AG];
			}
		}

		public virtual IJobExRateCurrencyConverter CurrencyConverter =>
			InvoicingSupporter?.Job?.CurrencyConverter.ToExRateCurrencyConverter()
			?? NonOrgSpecificExRateCurrencyConverter.Default(new BusinessObjectFactory());

		public virtual AdapterType AdapterType
		{
			get { return new AdapterType(); }
		}

		public virtual ZString OperationalJobCode
		{
			get { return ZString.Empty; }
		}

		public abstract ZString JobID { get; }

		public virtual ZString PaymentTermOverride
		{
			get => ZString.Empty;
		}

		/// <summary>
		///		Per Job charges correspond to charges which don't calculate per specific measure type and part.
		///		Such charges are applicable to the whole job rather than to a concrete measure part of the job.
		///		Usually, it is charges with Flat or Percentage calculators which don't depend on a specific unit.
		///
		///		In some scenarios we may have multiple adapters for the same job autorating specific charges or specific parts of the job.
		///		But, they all will autorate charges per Job which will lead to duplicated charges created.
		///		This flag allows controlling this, i.e. we will allow only 1 adapter to autorate such kind of charges.
		///
		///		Please note that this solution is a bit of hack. A normal solution would be:
		///		1. Create a specific measure type (MeasureType.Job?) for such kind of charges. Unidentified - is a bit confusing name.
		///		2. Those adapters who want to AutoRate such charges (almost all adapters) will have to explicitly add this measure type
		///			with some dummy JobLevelPart
		///		3. Update the LineMeasureMatcher logic that if a part is null then filter out the line at all. If it is MeasureType.Job
		///			then calculate it as per job, otherwise calculate it as Part.
		///
		///		But, too much changes required. Perhaps we can do it later as a refactoring.
		/// </summary>
		[XmlIgnore]
		public virtual ZBool ShouldDiscardPerJobCharges => false;

		/// <summary>
		/// We remove the charge by default unless adapters tell us not to.
		/// </summary>
		public virtual bool ShouldRemoveChargeWhenMissingServiceOrChargeableUnit(AccChargeCode chargeCode) => true;

		/// <summary>
		/// Override this property to return a list of attribute codes which will be skipped when comparing the infos.
		/// Attributes codes are defined in JobChargeAttribTypeList.Codes.
		/// See JobChargeAttrib.IsComparableType for the default attributes that are used for comparing.
		/// </summary>
		public virtual IEnumerable<ZString> ExcludedAttributesWhenMergingRateInfos => Enumerable.Empty<ZString>();

		#endregion

		#region IAutoRatingLocations

		public virtual ILocation Origin
		{
			get { return null; }
		}

		public virtual ILocation Destination
		{
			get { return null; }
		}

		public virtual ILocation GetVia(CostSell costSell) => null;

		public virtual ILocation PlannedLoad(CostSell costSell) => null;

		public virtual ILocation PlannedDischarge(CostSell costSell) => null;

		public virtual ILocation RateOrigin
		{
			get { return null; }
		}

		public virtual ILocation RateDestination
		{
			get { return null; }
		}

		public virtual ILocation GetFirstLoad(CostSell costSell) => null;
		public virtual ILocation GetLastDischarge(CostSell costSell) => null;
		public virtual ILocation GetFirstRouteSetLoad(CostSell costSell) => null;
		public virtual ILocation GetLastRouteSetDischarge(CostSell costSell) => null;

		#endregion

		#region IAutoRatingOrganisations

		public virtual ZString DeliveryCartageEquipment
		{
			get { return null; }
		}

		public virtual IDocAddress DeliveryAddress
		{
			get { return null; }
		}

		public virtual IDocAddress ConsigneeDocumentaryAddress
		{
			get { return null; }
		}

		public virtual ZString PickupCartageEquipment
		{
			get { return null; }
		}

		public virtual IDocAddress PickupAddress
		{
			get { return null; }
		}

		public virtual IDocAddress ConsignorDocumentaryAddress
		{
			get { return null; }
		}

		public virtual OrgHeader Carrier
		{
			get { return null; }
		}

		public virtual OrgHeader ImportBroker
		{
			get { return null; }
		}

		public virtual OrgHeader ExportBroker
		{
			get { return null; }
		}

		public virtual DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection();

				if (InvoicingSupporter != null && InvoicingSupporter.Job != null)
				{
					if (InvoicingSupporter.Job.LocalCharges != null)
					{
						result[RatingDebtorOrgTypes.LC] = InvoicingSupporter.Job.LocalCharges;
					}

					if (InvoicingSupporter.Job.AgentCollect != null)
					{
						result[RatingDebtorOrgTypes.AG] = InvoicingSupporter.Job.AgentCollect;
					}
				}

				return result;
			}
		}

		public virtual void OnAutoRated(IEnumerable<IAutoRatedCharge> charges)
		{
		}

		public virtual Creditors Creditors => null;
		public virtual IEnumerable<OrgHeader> PossibleServiceProviders => null;
		public virtual IEnumerable<OrgHeader> PossibleCarriers => null;

		#endregion

		#region IAutoRatingFreightInfo

		public virtual FreightMode FreightMode
		{
			get { return FreightMode.UKN; }
		}

		public virtual ZString ContainerMode
		{
			get { return Core.Constants.ContainerModes.Empty; }
		}

		public virtual ServiceLevelRatingInformation ServiceLevel
		{
			get { return new ServiceLevelRatingInformation(); }
		}

		public virtual MoneyType MonetaryValues
		{
			get { return new MoneyType(); }
		}

		/// <summary>
		/// RateableMeasures.
		/// A new instance should be created each call if the business object is being edited.
		/// For example, the user can run autorating, make some edits, and run autorating again.
		/// New measures are needed the second time.
		/// AutoRating will create a new AutoRatingProxy each time to wrap this IAutoRating instance.
		/// That will only call RateableMeasures once and cache the result.
		/// Note, by contrast RatingAdapter instances are usually just created once and cached in the business object.
		/// </summary>
		public virtual IRateableMeasureSet RateableMeasures => new RateableMeasureSet();

		public virtual ZString HousebillReleaseType
		{
			get { return null; }
		}

		public virtual OrgAddress WharfCTOAddress
		{
			get { return null; }
		}

		public virtual PaymentTermInfos PaymentTerm { get; set; }

		public virtual bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return false;
		}

		public virtual Directions JobDirection
		{
			get { return Directions.Unknown; }
		}

		public virtual ZString AircraftType
		{
			get { return ZString.Empty; }
		}

		public virtual ZString FMCTariffID => ZString.Empty;

		public virtual IEnumerable<RefCommodityCode> OverriddenCommodity => Enumerable.Empty<RefCommodityCode>();

		public virtual bool SkipFreightCharge => false;

		#endregion

		protected IEnumerable<JobServiceInfo> GetServiceInfosFromJobServices(JobServiceDependentCollection jobServices, string chargeCodeGroup = null)
		{
			var services = new List<JobServiceInfo>();
			var serviceTypes = GetServiceTypes(jobServices);

			foreach (CodeDescriptionPair serviceType in serviceTypes)
			{
				var infos = jobServices.GetServiceInfos(serviceType.Code).ToList();

				if (infos.Count > 0)
				{
					foreach (var info in infos)
					{
						info.ChargeCodeGroup = chargeCodeGroup;
						services.Add(info);
					}
				}
				else
				{
					// We require an empty JobServiceInfo if there are no services for this particular service type.
					services.Add(JobServiceInfo.Empty("", serviceType.Code));
				}
			}

			return services;
		}

		CodeDescriptionPairList GetServiceTypes(JobServiceDependentCollection jobServices)
		{
			CodeDescriptionPairList result;

			if (jobServices.Count > 0)
			{
				result = jobServices[0].Lookups.JobServiceType_List;
			}
			else
			{
				JobService service = jobServices.AddNew();
				result = service.Lookups.JobServiceType_List;
				service.Delete();
			}

			return result;
		}

		#region IAutoRatingAccountingInfo

		public virtual ZString QuoteNumber => null;

		public virtual IAutoRatingChargeInfo[] GetExistingCharges(bool fromAllCompanies = false)
		{
			var job = InvoicingSupporter?.Job;
			if (job == null)
			{
				return Array.Empty<IAutoRatingChargeInfo>();
			}

			if (!fromAllCompanies)
			{
				// The job contains charges of the current company. So, we can return them from the job.
				return (job as IAutoRatingAccountingInfo)?.GetExistingCharges();
			}

			// Load charges from all companies
			var query = new ZQuery();
			query.AddToFilter(JobHeaderSchema.JH_ParentID, job.JH_ParentID);
			query.AddToFilter(JobHeaderSchema.JH_IsActive, true);

			var jobs = job.Factory.Load<IJobHeader>(query);
			var charges = jobs
				.OfType<IAutoRatingAccountingInfo>()
				.SelectMany(j => j.GetExistingCharges())
				.ToArray();

			return charges;
		}

		#endregion
	}

	public abstract class RatingAdapter<T> : RatingAdapter where T : BusinessObject
	{
		public RatingAdapter(T parent)
		{
			Argument.NotNull(parent, "parent");

			Parent = parent;
		}

		public override Collection<IBusiness> AutoRatedFor
		{
			get { return new Collection<IBusiness> { Parent }; }
		}

		public override ZString OperationalJobCode => GetCodePropertyAttributeValue();

		public override sealed ZString JobID => GetCodePropertyAttributeValue();

		ZString GetCodePropertyAttributeValue()
		{
			var codePropertyAttributeExists = System.Attribute.IsDefined(Parent.GetType(), typeof(CodePropertyAttribute));

			if (Parent == null || !codePropertyAttributeExists)
			{
				return null;
			}

			return CodePropertyAttribute.CodeFromBusinessObject(Parent);
		}

		public T Parent { get; }
	}
}
