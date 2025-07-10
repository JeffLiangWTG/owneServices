using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI.BrowserInterop;

namespace Enterprise.Rating.GUI.RateSelector
{
	public class RateQueryFilterData : IUpdateFilterData
	{
		public readonly bool IsMultiRoute = RatingDataRegistry.Instance.MultiModalRatingCost.Value;

		public RateQueryFilterData(string jobShortcutUrl, RatingCriteria criteria)
		{
			JobShortcutUrl = jobShortcutUrl;
			Criteria = criteria;
			JobID = criteria.JobID;
			JobType = criteria.AdapterType;
			RateFilterDto = CreateRateFilterDto();
			RateQueryDto = CreateRateQueryDto();
		}

		public string JobID { get; }

		public string JobShortcutUrl { get; }

		public AdapterType JobType { get; }

		public RateQueryDto RateQueryDto { get; }

		public RateFilterDto RateFilterDto { get; }

		RatingCriteria Criteria { get; }

		RateQueryDto CreateRateQueryDto()
		{
			var effectiveDate = Criteria.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride);

			return new RateQueryDto
			{
				TransportMode = Criteria.InvoicingSupporter.TransportMode,
				ContainerMode = Criteria.InvoicingSupporter.ContainerMode,
				Origin = Criteria.DefaultFilterValueForOriginCode,
				Destination = Criteria.DefaultFilterValueForDestinationCode,
				EffectiveDate = !effectiveDate.IsEmpty ? effectiveDate.ToDateTime() : null,
				RateTypes = [Criteria.RateTypeToUse.ToString()],
				Context = RateQueryDto.RateSearchContext.AutoRating,
				JobInfo = CreateJobInfoDto()
			};
		}

		JobInfoDto CreateJobInfoDto()
		{
			// If this ever needs to be accurate (i.e if it ever needs to be able to be converted back to a RateableMeasuresSet)
			// You can likely implement the reverse of RateQueryBusinessObjectToRateableMeasureSetConverter
			var rateableMeasureSet = Criteria.RateableMeasures;
			var containerMode = Criteria.InvoicingSupporter.ContainerMode.ToString();
			return containerMode switch
			{
				Core.Constants.ContainerModes.LCL or Core.Constants.ContainerModes.Loose => HandleLooseModes(rateableMeasureSet),
				_ => HandleFullContainerModes(rateableMeasureSet)
			};
		}

		RateFilterDto CreateRateFilterDto()
		{
			var orgHeaders = Criteria.PossibleServiceProviders;

			var rateFilterDto = new RateFilterDto
			{
				ServiceProviderCode = orgHeaders
					.Where(orgHeader => !orgHeader.OH_Code.IsEmpty)
					.Select(orgHeader => orgHeader.OH_Code)
					.Distinct()
					.ToList(),
				CarrierContractNumber = Criteria.CarrierContractNumbers
					.Where(contractNumber => !contractNumber.IsEmpty)
					.Distinct()
					.ToList(),
				CarrierServiceLevel = Criteria.ServiceLevel.ServiceLevelData
					.Where(serviceLevelInfo => serviceLevelInfo is { ServiceLevelType: ServiceLevelType.Carrier, ServiceLevel.IsEmpty: false })
					.Select(serviceLevelInfo => serviceLevelInfo.ServiceLevel)
					.Distinct()
					.ToList(),
				NamedAccount = Criteria.NamedAccount.IsEmpty ? [] : [Criteria.NamedAccount],
				Consignee = Criteria.Consignee == null ? [] : [Criteria.Consignee.OH_Code],
				Consignor = Criteria.Consignor == null ? [] : [Criteria.Consignor.OH_Code],
				PaymentTerm = Criteria.PaymentTerm.PaymentTermInfoCollection
					.Where(paymentTermInfo => paymentTermInfo.InfoType == PaymentTermType.PrepaidCollect && !((ZString)paymentTermInfo.Value).IsEmpty)
					.Select(paymentTermInfo => (ZString)paymentTermInfo.Value)
					.Distinct().ToList(),
				FirstLoad = Criteria.GetFirstLoadCode(Enterprise.Integration.Accounting.CostSell.Cost).IsEmpty ? [] : [Criteria.GetFirstLoadCode(Enterprise.Integration.Accounting.CostSell.Cost)],
				LastDischarge = Criteria.GetLastDischargeCode(Enterprise.Integration.Accounting.CostSell.Cost).IsEmpty ? [] : [Criteria.GetLastDischargeCode(Enterprise.Integration.Accounting.CostSell.Cost)],
				FirstRouteSetLoad = Criteria.GetFirstRouteSetLoadCode(Enterprise.Integration.Accounting.CostSell.Cost).IsEmpty ? [] : [Criteria.GetFirstRouteSetLoadCode(Enterprise.Integration.Accounting.CostSell.Cost)],
				LastRouteSetDischarge = Criteria.GetLastRouteSetDischargeCode(Enterprise.Integration.Accounting.CostSell.Cost).IsEmpty ? [] : [Criteria.GetLastRouteSetDischargeCode(Enterprise.Integration.Accounting.CostSell.Cost)],
			};

			return rateFilterDto;
		}

		JobInfoDto HandleLooseModes(RateableMeasureSet rateableMeasureSet)
		{
			var jobContainerList = new List<JobContainerDto>();
			var packageList = rateableMeasureSet.GetPartList(MeasureType.Package);

			// TODO: better handling in WI00879608 - [C3] Prevent launching C3 from an empty Consol (packageList == null or 0)
			if (packageList == null || packageList.Count == 0)
			{
				return new JobInfoDto();
			}

			var commodities = packageList.Select(p => p.CommodityCode).Distinct();
			if (rateableMeasureSet.GetPartList(MeasureType.Chargeable) is JobLevelPart chargeable)
			{
				jobContainerList.Add(new JobContainerDto
				{
					Commodity = string.Join(", ", commodities),
					PackLines =
					[
						new JobPackLineDto
						{
							Commodity = string.Join(", ", commodities),
							ChargeableOverride = chargeable.ChargeableMeasure?.Actual ?? ZDecimal.Zero,
							ChargeableUnit = chargeable.ChargeableUnit
						}
					],
				});
			}

			return new JobInfoDto { Containers = jobContainerList };
		}

		JobInfoDto HandleFullContainerModes(RateableMeasureSet rateableMeasureSet)
		{
			var jobContainerList = new List<JobContainerDto>();
			var containerList = rateableMeasureSet.GetPartList(MeasureType.ContainerCount);
			foreach (var rateablePart in containerList)
			{
				var container = (RateableContainer)rateablePart;
				var containerType = container.ContainerTypePk.HasValue ? Criteria.Factory.Load<RefContainer>(container.ContainerTypePk.Value).RC_Code : null;
				jobContainerList.Add(new JobContainerDto
				{
					ContainerType = containerType,
					Number = container.ContainerNumber,
					Count = container.ContainerCount,
					Commodity = container.CommodityCode,
					ContainerQuality = container.ContainerQuality,
					PackLines =
					[
						new JobPackLineDto
						{
							Count = container.ContainerPackages,
							Commodity = container.CommodityCode,
							Weight = container.ContainerWeightInKG,
							WeightUnit = QuantityUnit.KG,
							Volume = container.ContainerVolumeInM3,
							VolumeUnit = QuantityUnit.M3
						}
					],
				});
			}

			return new JobInfoDto { Containers = jobContainerList };
		}
	}
}
