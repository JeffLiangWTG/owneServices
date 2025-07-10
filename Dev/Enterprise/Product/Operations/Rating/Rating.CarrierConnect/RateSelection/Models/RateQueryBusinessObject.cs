#nullable enable
using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	public sealed class RateQueryBusinessObject : NonPersistentBusinessObject
	{
		RateQueryDto Parent { get; }

		public RateQueryBusinessObject(RateQueryDto parent)
		{
			Parent = parent;
		}

		public string TransportMode => Parent.TransportMode;

		public string ContainerMode => Parent.ContainerMode;

		public FreightMode FreightMode => FreightRatingHelper.CalculateFreightMode(Parent.TransportMode, Parent.ContainerMode);

		public string Origin => Parent.Origin;

		public string Destination => Parent.Destination;

		public DateTime? EffectiveDate => Parent.EffectiveDate;

		public RateType RateType
		{
			get
			{
				return Parent.RateTypes
					.Select(rateTypeName => RateSelectorService.SupportedRateTypes[rateTypeName])
					.Aggregate<RateType, RateType>(0, (result, rateType) => result | rateType);
			}
		}

		public RateQueryDto.RateSearchContext Context => Parent.Context;

		public JobInfoDto JobInfoDto => Parent.JobInfo;

		public bool IsContainerized => (FreightMode.Containerised & FreightMode) == FreightMode.Containerised;

		public bool HasMeasures => JobInfoDto?.Containers?.Any() ?? false;
	}
}
