using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Web.Model.Conversion
{
	/// <summary>
	/// This is a utility class to convert job related information of RateQueryBusinessObject to a set of measures used mostly for charge calculations. 
	/// </summary>
	public class RateQueryBusinessObjectToRateableMeasureSetConverter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="factory"></param>
		public RateQueryBusinessObjectToRateableMeasureSetConverter(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly BusinessObjectFactory factory;

		/// <summary>
		/// Convert
		/// </summary>
		/// <param name="rateQuery"></param>
		/// <param name="adapterType"></param>
		/// <returns></returns>
		public RateableMeasureSet Convert(RateQueryBusinessObject rateQuery, AdapterType adapterType)
		{
			var measureSet = new RateableMeasureSet(adapterType);

			if (rateQuery.SourceEndpoint != SourceEndpoint.JobCharges)
			{
				if (rateQuery.ContainerTypes != null)
				{
					foreach (var item in rateQuery.ContainerTypes.Values)
					{
						measureSet.AddContainer(item.PK);
					}
				}

				return measureSet;
			}

			var jobInfo = rateQuery.RateQuery.JobInfo;
			if (jobInfo != null && (jobInfo.Containers?.Length > 0))
			{
				var totalWeightInKG =
					jobInfo
					.Containers
					.Sum(c => c?.PackLines?.Sum(p => Constants.Weight.Convert(p?.Weight ?? 0, p?.WeightUnit ?? Constants.Weight.Kilograms, Constants.Weight.Kilograms)));

				var totalVolumeInM3 =
					jobInfo
					.Containers
					.Sum(c => c?.PackLines?.Sum(p => Constants.Volume.Convert(p?.Volume ?? 0, p?.VolumeUnit ?? Constants.Volume.CubicMetres, Constants.Volume.CubicMetres)));

				var packLines =
					jobInfo
					.Containers
					.Select(c => new { ContainerPK = ZGuid.NewZGuid(), Container = c })
					.Where(c => c.Container?.PackLines?.Any() ?? false)
					.SelectMany(c =>
						c.Container?.PackLines?.Select(p => new { c.ContainerPK, c.Container?.ContainerTypeCWCode, c.Container?.Number, c.Container?.Commodity, PackLine = p }));

				var packages = new RateablePartList();
				packages.HasContainerType = true;
				packages.HasCommodity = true;
				packages.WeightUnit = Constants.Weight.Kilograms;
				packages.VolumeUnit = Constants.Volume.CubicMetres;

				var units = new RateablePartList();
				units.HasPackageType = true;
				units.HasContainerType = true;

				foreach (var item in packLines)
				{
					var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, item?.ContainerTypeCWCode);
					var commodity = rateQuery.IsContainerized ? item?.Commodity : item?.PackLine?.Commodity;
					var weight = Constants.Weight.Convert(item?.PackLine?.Weight ?? 0, item?.PackLine?.WeightUnit ?? Constants.Weight.Kilograms, Constants.Weight.Kilograms);
					var volume = Constants.Volume.Convert(item?.PackLine?.Volume ?? 0, item?.PackLine?.VolumeUnit ?? Constants.Volume.CubicMetres, Constants.Volume.CubicMetres);
					var containerTypePK = refContainer != null ? NullableHelper.ToNullable(refContainer.PK) : null;
					var packsCount = item?.PackLine?.Unit ?? 0;

					var package = new RateablePart();
					package.ContainerPK = NullableHelper.ToNullable(item.ContainerPK);
					package.CommodityCode = commodity;
					package.Weight = weight;
					package.Volume = volume;
					package.PackageCount = packsCount;
					package.ContainerTypePk = containerTypePK;
					packages.AddPart(package);

					if (packsCount > 0)
					{
						var unit = new RateableContainer();
						unit.ContainerWeightInKG = weight;
						unit.ContainerVolumeInM3 = volume;
						unit.UnitCount = packsCount;
						unit.ContainerCount = packsCount;
						unit.ContainerPackages = packsCount;
						unit.ContainerNumber = item?.Number;
						unit.PackageType = item?.PackLine?.PackageType;
						unit.ContainerTypePk = containerTypePK;
						unit.TEU = refContainer?.RC_TEU ?? 0;

						units.AddPart(unit);
					}
				}

				measureSet.AddPartList(MeasureType.Weight, packages);
				measureSet.AddPartList(MeasureType.Volume, packages);
				measureSet.AddPartList(MeasureType.Package, packages);
				measureSet.AddPartList(MeasureType.LoadingMeters, packages);
				measureSet.AddPartList(MeasureType.Unit, units);

				var chargeableCommodity = GetChargableCommodity(rateQuery.IsContainerized, jobInfo.Containers);
				var chargeableUnit = ChargeableAmountCalculator.GetChargeableUnit(rateQuery.TransportMode, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

				decimal chargeableAmount = 0;
				if (jobInfo.ChargeableOverride > 0)
				{
					chargeableAmount = jobInfo.ChargeableOverride;
				}
				else
				{
					chargeableAmount = GetChargeableAmount(rateQuery.IsDomesticFreightForChargeableWeightCalculations, rateQuery.TransportMode, chargeableUnit, totalWeightInKG ?? 0, Constants.Weight.Kilograms, totalVolumeInM3 ?? 0, Constants.Volume.CubicMetres);
				}

				measureSet.Shipments = 1;
				measureSet.LowestBill = 0;

				measureSet.SetChargeableWithCommodity(chargeableCommodity, chargeableUnit, chargeableAmount, chargeableAmount, chargeableAmount);

				foreach (var container in jobInfo.Containers)
				{
					SetContainers(measureSet, container, rateQuery.IsContainerized);
				}
			}

			return measureSet;
		}

		void SetContainers(RateableMeasureSet measures, JobContainer container, bool isContainerized)
		{
			var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, container?.ContainerTypeCWCode);
			var containerWeightInKG = container?.PackLines?.Sum(p => Constants.Weight.Convert(p?.Weight ?? 0, p?.WeightUnit ?? Constants.Weight.Kilograms, Constants.Weight.Kilograms));
			var containerVolumeInM3 = container?.PackLines?.Sum(p => Constants.Volume.Convert(p?.Volume ?? 0, p?.VolumeUnit ?? Constants.Volume.CubicMetres, Constants.Volume.CubicMetres));
			var containerTotalPackageCount = container?.PackLines?.Sum(p => p?.Unit ?? 0);

			var commodity = isContainerized && !string.IsNullOrEmpty(container?.Commodity) ? container.Commodity : container?.PackLines?.Select(p => p?.Commodity).SameOrDefault() ?? string.Empty;

			var containerInfo = CreateContainerInfo();

			measures.AddContainerGroup(
				containerOwnership: container?.Ownership,
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

				var containerCount = container?.Unit > 0 ? container.Unit.Value : 1;
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
						container?.Number,
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
						if (container?.Unit > 0)
						{
							result = refContainer.RC_TEU * (container.Unit ?? 0);
						}
						else
						{
							result = refContainer.RC_TEU;
						}
					}
					else if (container?.Unit > 0)
					{
						result = new ZDecimal(container?.Unit);
					}

					return result;
				}
			}
		}

		string GetChargableCommodity(bool isContainerized, IEnumerable<JobContainer> jobContainers)
		{
			var commodities = isContainerized ?
				jobContainers.Select(c => c?.Commodity).Distinct().ToArray() :
				jobContainers.SelectMany(c => c.PackLines?.Select(p => p?.Commodity) ?? System.Array.Empty<string>()).Distinct().ToArray();

			if (commodities.Length == 1)
			{
				return commodities[0] ?? string.Empty;
			}

			return string.Empty;
		}

		decimal GetChargeableAmount(bool isDomestic, string transportMode, string chargeableUnit, decimal weight, string weightUnit, decimal volume, string volumeUnit)
		{
			return ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
			{
				Weight = new ZWeight(weight, weightUnit),
				Volume = new ZVolume(volume, volumeUnit),
				LoadingLength = new Quantity(0, Constants.LoadingLength.LoadingMeters),
				TargetUnit = chargeableUnit,
				ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(isDomestic, transportMode, chargeableUnit)
			}).Chargeable.Amount;
		}
	}
}
