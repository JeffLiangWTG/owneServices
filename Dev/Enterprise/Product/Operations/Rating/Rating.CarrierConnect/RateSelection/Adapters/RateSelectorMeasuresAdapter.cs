using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.CarrierConnect
{
	class RateSelectorMeasuresAdapter
	{
		readonly BusinessObjectFactory factory;

		public RateSelectorMeasuresAdapter(RateQueryBusinessObject rateQuery, BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			RateQuery = Argument.NotNull(rateQuery, nameof(rateQuery));
		}

		RateQueryBusinessObject RateQuery { get; }

		JobInfoDto JobInfo => jobInfo ??= RateQuery.JobInfoDto;
		JobInfoDto jobInfo;

		string ChargableUnit => chargeableUnit ??= ChargeableAmountCalculator.GetChargeableUnit(
				RateQuery.TransportMode,
				Constants.Weight.Kilograms,
				Constants.Volume.CubicMetres);
		string chargeableUnit;

		public RateableMeasureSet Convert(AdapterType adapterType)
		{
			var measureSet = new RateableMeasureSet(adapterType);

			if (JobInfo is null || !JobInfo.Containers.Any())
			{
				return measureSet;
			}

			AddPackages(measureSet);
			AddChargeable(measureSet);

			if (RateQuery.ContainerMode != Constants.ContainerModes.LCL && RateQuery.ContainerMode != Constants.ContainerModes.Loose)
			{
				JobInfo.Containers.ForEach((container) => SetContainers(measureSet, container, RateQuery.IsContainerized));
			}

			measureSet.Shipments = 1;
			measureSet.LowestBill = 0;

			return measureSet;
		}

		void AddPackages(RateableMeasureSet measureSet)
		{
			var packLines =
				JobInfo
					.Containers
					.Select(c => new { ContainerPK = ZGuid.NewZGuid(), Container = c })
					.Where(c => c.Container?.PackLines?.Any() ?? false)
					.SelectMany(c => c.Container?.PackLines?.Select(p => new
					{
						c.ContainerPK,
						c.Container?.ContainerType,
						c.Container?.Number,
						c.Container?.Commodity,
						PackLine = p
					}));

			var packages = new RateablePartList
			{
				HasContainerType = true,
				HasCommodity = true,
				WeightUnit = Constants.Weight.Kilograms,
				VolumeUnit = Constants.Volume.CubicMetres
			};

			var units = new RateablePartList { HasPackageType = true, HasContainerType = true };

			foreach (var item in packLines)
			{
				var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, item?.ContainerType);
				var commodity = RateQuery.IsContainerized ? item?.Commodity : item?.PackLine?.Commodity;
				var weight = Constants.Weight.Convert(item?.PackLine?.Weight ?? 0, item?.PackLine?.WeightUnit ?? Constants.Weight.Kilograms, Constants.Weight.Kilograms);
				var volume = Constants.Volume.Convert(item?.PackLine?.Volume ?? 0, item?.PackLine?.VolumeUnit ?? Constants.Volume.CubicMetres, Constants.Volume.CubicMetres);
				var containerTypePK = refContainer != null ? NullableHelper.ToNullable(refContainer.PK) : null;
				var packsCount = item?.PackLine?.Count ?? 0;

				packages.AddPart(new RateablePart
				{
					ContainerPK = NullableHelper.ToNullable(item.ContainerPK),
					CommodityCode = commodity,
					Weight = weight,
					Volume = volume,
					PackageCount = packsCount,
					ContainerTypePk = containerTypePK
				});

				if (packsCount > 0)
				{
					units.AddPart(new RateableContainer
					{
						ContainerWeightInKG = weight,
						ContainerVolumeInM3 = volume,
						UnitCount = packsCount,
						ContainerCount = packsCount,
						ContainerPackages = packsCount,
						ContainerNumber = item?.Number,
						PackageType = item?.PackLine?.PackageType,
						ContainerTypePk = containerTypePK,
						TEU = refContainer?.RC_TEU ?? 0
					});
				}
			}

			measureSet.AddPartList(MeasureType.Weight, packages);
			measureSet.AddPartList(MeasureType.Volume, packages);
			measureSet.AddPartList(MeasureType.Package, packages);
			measureSet.AddPartList(MeasureType.LoadingMeters, packages);
			measureSet.AddPartList(MeasureType.Unit, units);
		}

		decimal CalculateChargeableAmount(JobContainerDto container)
		{
			if (Constants.Weight.ContainsCode(ChargableUnit))
			{
				return container
					.PackLines
					.Sum(p => Constants.Weight.Convert(
						p?.ChargeableOverride ?? 0,
						p?.ChargeableUnit ?? Constants.Weight.Kilograms,
						Constants.Weight.Kilograms));
			}

			return container
				.PackLines
				.Sum(p => Constants.Volume.Convert(
					p?.ChargeableOverride ?? 0,
					p?.ChargeableUnit ?? Constants.Volume.CubicMetres,
					Constants.Volume.CubicMetres));
		}

		void AddChargeable(RateableMeasureSet measureSet)
		{
			var partList = new RateablePartList
			{
				HasContainerType = true,
				HasCommodity = true,
				WeightUnit = Constants.Weight.Kilograms,
				VolumeUnit = Constants.Volume.CubicMetres,
				ChargeableUnit = ChargableUnit,
			};

			var parts = JobInfo
				.Containers
				.Select((container) => new
				{
					Chargeable = CalculateChargeableAmount(container),
					container.PackLines[0].Commodity,
					RefContainer = GetRefContainer(container.ContainerType),
				})
				.Where((chargeableContainer) => chargeableContainer.Chargeable > 0)
				.Select((c) => new JobLevelPart
				{
					ChargeableMeasure = new ClientProviderValues(c.Chargeable, c.Chargeable, c.Chargeable),
					ChargeableUnit = ChargableUnit,
					CommodityCode = c.Commodity,
					ContainerTypePk = c.RefContainer is null ? null : NullableHelper.ToNullable(c.RefContainer.PK),
				});

			partList.AddParts(parts);
			measureSet.AddPartList(MeasureType.Chargeable, partList);
		}

		RefContainer GetRefContainer(string containerType) => factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);

		void SetContainers(RateableMeasureSet measures, JobContainerDto container, bool isContainerized)
		{
			var refContainer = GetRefContainer(container?.ContainerType);
			var containerWeightInKG = container?.PackLines?.Sum(p => Constants.Weight.Convert(p?.Weight ?? 0, p?.WeightUnit ?? Constants.Weight.Kilograms, Constants.Weight.Kilograms));
			var containerVolumeInM3 = container?.PackLines?.Sum(p => Constants.Volume.Convert(p?.Volume ?? 0, p?.VolumeUnit ?? Constants.Volume.CubicMetres, Constants.Volume.CubicMetres));
			var containerTotalPackageCount = container?.PackLines?.Sum(p => p?.Count ?? 0);

			var commodity = isContainerized && !string.IsNullOrEmpty(container?.Commodity)
				? container.Commodity
				: container?.PackLines?.Select(p => p?.Commodity).SameOrDefault() ?? string.Empty;

			var containerInfo = CreateContainerInfo();

			measures.AddContainerGroup(
				containerOwnership: null, // TODO: Check if we need this.
				containerNumber: container?.Number,
				containerTypePK: refContainer?.PK ?? ZGuid.Empty,
				containerInfos: containerInfo,
				commodity: commodity,
				weight: new ZDecimal(containerWeightInKG),
				volume: new ZDecimal(containerVolumeInM3));

			IEnumerable<MeasureInfo.ContainerInfo> CreateContainerInfo()
			{
				if (container == null)
				{
					return Enumerable.Empty<MeasureInfo.ContainerInfo>();
				}

				var infos = new List<MeasureInfo.ContainerInfo>();

				var containerCount = container?.Count > 0 ? container.Count.Value : 1;
				var shipmentShare = 1;
				var hasWeightOrVolume = containerWeightInKG > 0 || containerVolumeInM3 > 0;

				if (containerCount == 1 && hasWeightOrVolume)
				{
					var info = new MeasureInfo.ContainerInfo(
						new ZDecimal(containerWeightInKG),
						Constants.Weight.Kilograms,
						new ZDecimal(containerVolumeInM3),
						Constants.Volume.CubicMetres,
						new ZInt(containerTotalPackageCount),
						GetTUECount(),
						container.Number,
						shipmentShare,
						refNumber: string.Empty,
						containerPK: ZGuid.Empty);

					infos.Add(info);
				}
				else
				{
					var perContainerTeu = containerCount > 0 ? GetTUECount() / containerCount : 0;

					for (var i = 0; i < containerCount; i++)
					{
						var info = new MeasureInfo.ContainerInfo(teu: perContainerTeu, shipmentShare: shipmentShare, containerNumber: container.Number);

						infos.Add(info);
					}
				}

				return infos;

				ZDecimal GetTUECount()
				{
					ZDecimal result = 1;
					if (refContainer != null)
					{
						if (container?.Count > 0)
						{
							result = refContainer.RC_TEU * (container.Count ?? 0);
						}
						else
						{
							result = refContainer.RC_TEU;
						}
					}
					else if (container?.Count > 0)
					{
						result = new ZDecimal(container?.Count);
					}

					return result;
				}
			}
		}
	}
}
