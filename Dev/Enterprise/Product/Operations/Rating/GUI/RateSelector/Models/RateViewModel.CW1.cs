using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	/// <summary>
	/// This ViewModel represents a CW1 rate and is used for displaying
	/// information in the user interface relating directly to the CW1 rate.
	/// </summary>
	public class CW1RateViewModel : RateViewModel
	{
		public OrgHeader ServiceProviderOrg { get; set; }
		public string ServiceProviderCode { get; set; }
		public string ServiceProviderName { get; set; }
		public string TransitTime { get; set; }
		public string ServiceLevel { get; set; }
		public bool IsTactRate { get; set; }
		public string Consignor { get; set; }
		public string Consignee { get; set; }

		public ZGuid RelatedTabContainerTypePk { get; }
		public ZString RelatedTabCommodityCode { get; }

		public CW1RateViewModel()
		{
		}

		public CW1RateViewModel(RateSelectorContext context, RatingCriteria criteria, ZGuid relatedTabContainerTypePk, ZString relatedTabCommodityCode, IEnumerable<IRateLine> lines, IDisposable effectiveDateDisposer)
		{
			Criteria = Argument.NotNull(criteria, nameof(criteria));
			Context = Argument.NotNull(context, nameof(context));
			Lines = Argument.NotNull(lines, nameof(lines));

			RelatedTabContainerTypePk = relatedTabContainerTypePk;
			RelatedTabCommodityCode = relatedTabCommodityCode;

#pragma warning disable 061

			var entries = lines.Select(l => l.ParentRateEntry).Distinct().ToList();
			var startDate = entries.Max(x => x.TI_RateStartDate);
			var endDate = entries.Min(x => x.TI_RateEndDate);
			var allCommodities = entries.Where(x => !string.IsNullOrEmpty(x.TI_RH_NKCommodityCode)).Select(x => (string)x.TI_RH_NKCommodityCode).Distinct();
			var allCommodityGroups = entries.Where(x => !string.IsNullOrEmpty(x.CommodityGroup))
				.SelectMany(x => Context.Filters.GetCommodityGroupsFromCommodityCode(x.CommodityGroup).Distinct());
			var autoRateInfo = entries.FirstOrDefault(x => x.IsFreightEntry()) ?? entries.First();
			var carriers = entries
				.Select(x => Context.Factory.Load<OrgHeader>(x.TI_OH_TransportProvider))
				.WhereNotNull();
			var carrier = IEnumerableExtensions.DistinctBy(carriers, x => x.PK)
				.SingleOrDefault();
			var serviceProviders = entries
				.Select(x => Context.Factory.Load<OrgHeader>(x.ServiceProviderPK()))
				.WhereNotNull();
			var serviceProvider = IEnumerableExtensions.DistinctBy(serviceProviders, x => x.PK)
				.SingleOrDefault();
			var contractNumber = entries
				.Select(x => x.TI_ContractNumber)
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.SingleOrDefault();
			var carrierServiceLevel = entries
				.Select(x => x.TI_PL_NKCarrierServiceLevel)
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.SingleOrDefault();
			var container = entries
				.Select(x => x.Container)
				.Where(x => x != null)
				.Distinct()
				.SingleOrDefault();

			CarrierServiceLevel = carrierServiceLevel;
			ContractNumber = contractNumber;
			ContainerType = container?.RC_Code;
			CommodityCodes = allCommodities.ToList();
			Commodities = string.Join(",", CommodityCodes);
			CommodityGroups = allCommodityGroups;
			Origin = criteria.OriginCode;
			Destination = criteria.DestinationCode;
			StartDate = startDate.IsValid ? startDate.ToDateTime() : DateTime.MinValue;
			ExpiryDate = endDate.IsValid ? endDate.ToDateTime() : null;
			Consignor = autoRateInfo.Consignor?.OH_Code;
			Consignee = autoRateInfo.Consignee?.OH_Code;
			TransitTime = autoRateInfo.TI_TransitTime;
			CarrierOrg = carrier;
			CarrierName = carrier?.OH_FullName ?? string.Empty;
			CarrierCode = carrier?.OH_Code ?? string.Empty;
			ServiceProviderOrg = serviceProvider;
			ServiceProviderCode = serviceProvider?.OH_Code ?? string.Empty;
			ServiceProviderName = serviceProvider?.OH_FullName ?? string.Empty;
			EffectiveDateDisposer = effectiveDateDisposer;

#pragma warning restore 0618
		}

		#region IClonable

		public override object Clone()
		{
			return new CW1RateViewModel(Context, Criteria, RelatedTabContainerTypePk, RelatedTabCommodityCode, Lines.Cast<RateLine>(), EffectiveDateDisposer);
		}

		#endregion

		public override IEnumerable<AutoRateInfo> GetAutoRateInfos()
		{
			var selectedCharges = Charges
				.SelectMany(c => c.Charges)
				.Where(c => c.IsSelected && !c.IsIncluded)
				.ToArray();

			var autoRateInfos = selectedCharges.Select(c => c.AutoRateInfo).WhereNotNull().ToArray();
			return autoRateInfos;
		}

		protected override void SetCharges(IEnumerable<AutoRateInfo> autoRateInfoCollection, string logs)
		{
			foreach (var info in autoRateInfoCollection)
			{
				var viewModel = new CW1ChargeViewModel(info, Context);

				if (info.IsFreight)
				{
					FreightCharges.Add(viewModel);
				}
				else
				{
					OtherCharges.Add(viewModel);
				}
			}
		}

		protected override void UpdateCriteria(RatingCriteria criteria, IEnumerable<string> originalChargeCodeGroups)
		{
			Creditors creditors = null;

			if (ServiceProviderOrg != null)
			{
				var source = new List<string> { (NoResString)"CW1 Calculated Rate Selector" }; // internal use

				if (originalChargeCodeGroups.IsNullOrEmpty() || originalChargeCodeGroups.Any(string.IsNullOrWhiteSpace))
				{
					creditors = Creditors.New(OrgWithSource.New(ServiceProviderOrg, source));
				}
				else
				{
					creditors = new Creditors();
					foreach (var chargeCodeGroup in originalChargeCodeGroups)
					{
						creditors[chargeCodeGroup].Add(1, new[] { OrgWithSource.New(ServiceProviderOrg, source) });
					}
				}
			}

			criteria.Creditors = creditors;
			criteria.Carrier = CarrierOrg;
			criteria.CarrierServiceLevelOverride = string.IsNullOrWhiteSpace(CarrierServiceLevel)
				? new List<ZString>()
				: new List<ZString> { CarrierServiceLevel };
			criteria.CarrierContractNumbers = string.IsNullOrWhiteSpace(ContractNumber)
				? Array.Empty<ZString>()
				: new ZString[] { ContractNumber };
		}

		RateSelectorContext Context { get; }
		RatingCriteria Criteria { get; }
	}
}
