using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class SpotRateEntryCreator
	{
		public SpotRateEntryCreator(RatingCriteria criteria)
		{
			if (criteria == null)
			{
				throw new ArgumentException("Criteria required");
			}

			Criteria = criteria;
			Factory = Criteria.Factory;
		}

		RatingCriteria Criteria { get; }
		BusinessObjectFactory Factory { get; }

		#region Create Generic Spot Rate Entry

		RateEntry CreateSpotRateEntry(bool isCosting, ZGuid? orgPk)
		{
			var notSavedFactory = new BusinessObjectFactory { NameForDebugging = "Rating Spot Rate Factory" };
			notSavedFactory.Saving += x => { throw new InvalidOperationException("This Factory is not intended to be saved"); };

			var entry = notSavedFactory.New<RateEntry>();
			entry.TI_GC_Publisher = Env.CurrentCompany.PK;
			entry.IsSpotEntry = true;
			entry.TI_TH = NewRatingHeaderPK(notSavedFactory, isCosting);

			if (orgPk.HasValue && orgPk.Value != ZGuid.Empty)
			{
				entry.Parent.TH_OH = orgPk.Value;
			}

			entry.TI_OriginLRC = Criteria.OriginCode;
			entry.TI_DestinationLRC = Criteria.DestinationCode;
			entry.TI_RateStartDate = ZDate.Empty;
			entry.TI_RateEndDate = ZDate.Empty;

			return entry;
		}

		RateEntry CreateContainerSpotRateEntry(bool isCosting, SpotRateInfo spotRateInfo, ZGuid containerPK, ZGuid containerType)
		{
			var notSavedFactory = new BusinessObjectFactory { NameForDebugging = "Rating Container Spot Rate Factory" };
			notSavedFactory.Saving += x => { throw new InvalidOperationException("This Factory is not intended to be saved"); };

			var entry = notSavedFactory.New<RateEntry>();
			entry.TI_GC_Publisher = Env.CurrentCompany.PK;
			entry.IsSpotEntry = true;
			entry.ContainerPKForSpotEntry = containerPK;
			entry.TI_TH = NewRatingHeaderPK(notSavedFactory, isCosting);

			if (spotRateInfo.Creditor != null)
			{
				entry.Parent.TH_OH = spotRateInfo.Creditor.PK;
			}

			entry.TI_OriginLRC = Criteria.OriginCode;
			entry.TI_DestinationLRC = Criteria.DestinationCode;
			entry.TI_RateStartDate = ZDate.Empty;
			entry.TI_RateEndDate = ZDate.Empty;
			entry.TI_RC = containerType;

			return entry;
		}

		ZGuid NewRatingHeaderPK(BusinessObjectFactory factory, bool isCosting)
		{
			if (isCosting)
			{
				return factory.New<Costing>().PK;
			}

			return Criteria.GatewayConfiguration.ContinueAutoratingRevenueFromIntercompanyTariff
				? factory.New<IntercompanyTariff>().PK
				: factory.New<ClientRate>().PK;
		}

		#endregion

		#region Freight Spot Rate

		public (IRateEntry, string) CreateFreightSpotRate(bool isCosting)
		{
			var companyGuid = Criteria.Company.PK.ToGuid();
			var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.GetFreightChargeCode(companyGuid));

			if (freightChargeCode == null)
			{
				return (null, ErrorMessages.InvalidFreightChargeCodeErrorMessage);
			}

			var orgPK = isCosting
				? Criteria.CostSpotRateInfo.Creditor?.PK
				: Criteria.GetDebtorPK(freightChargeCode);

			var entry = CreateSpotRateEntry(isCosting, orgPK);
			SetEntryModeAndCategoryFromCriteria(entry);

			var freightSpotRateInfo = isCosting ? Criteria.CostSpotRateInfo : Criteria.SellSpotRateInfo;

			entry.TI_RS_NKServiceLevel_NI = Criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Client);
			entry.TI_RX_NKCurrency = freightSpotRateInfo.Rate.Currency.Code;

			var line = entry.RateLines.AddNew();
			line.TL_AC = freightChargeCode.PK;
			line.TL_RateCalculator = UnitCalculator.Code;
			line.Calculator[Calculator.Items.Operator.UNT] = freightSpotRateInfo.Rate.Amount;
			line.TL_Rounding = RatingRoundingTypes.Chargeable;
			line.SpotRateDescription = freightSpotRateInfo.GetAutoratedValueTypeDescription();
			line.TL_WeightVolume = entry.Unit;

			return (entry, string.Empty);
		}

		public (IEnumerable<IRateEntry>, string) CreateContainerFreightSpotRateEntries(bool isCosting, IEnumerable<ContainerSpotRates> containerSpotRates)
		{
			var companyGuid = Criteria.Company.PK.ToGuid();
			var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.GetFreightChargeCode(companyGuid));
			if (freightChargeCode == null)
			{
				return (Enumerable.Empty<IRateEntry>(), ErrorMessages.InvalidFreightChargeCodeErrorMessage);
			}

			var result = new List<IRateEntry>();
			var validSpotRates = isCosting ? containerSpotRates.Where(x => x.CostSpotRateIsValid) : containerSpotRates.Where(x => x.SellSpotRateIsValid);

			foreach (var spotRate in validSpotRates)
			{
				var containerPK = spotRate.ContainerPK;
				var containerSpotRateInfo = isCosting ? spotRate.CostSpotRate : spotRate.SellSpotRate;

				var entry = CreateContainerSpotRateEntry(isCosting, containerSpotRateInfo, containerPK, spotRate.RefContainerPK);
				SetEntryModeAndCategoryFromCriteria(entry);

				entry.TI_RS_NKServiceLevel_NI = Criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Client);
				entry.TI_RX_NKCurrency = containerSpotRateInfo.Rate.Currency.Code;

				var line = entry.RateLines.AddNew();
				line.TL_AC = freightChargeCode.PK;
				line.TL_RateCalculator = UnitCalculator.Code;
				line.Calculator[Calculator.Items.Operator.UNT] = containerSpotRateInfo.Rate.Amount;
				line.TL_Rounding = RatingRoundingTypes.Chargeable;
				line.TL_WeightVolume = RatingConstants.Units.CN;
				line.SpotRateDescription = containerSpotRateInfo.GetAutoratedValueTypeDescription();

				result.Add(entry);
			}

			return (result, string.Empty);
		}

		void SetEntryModeAndCategoryFromCriteria(RateEntry entry)
		{
			if (Criteria.IsAirFreight)
			{
				entry.TI_RateCategory = RatingConstants.RateCategory.AIR;
				entry.TI_Mode = Criteria.IsContainerised ? Core.Constants.RateMode.ULD : Core.Constants.RateMode.LSE;
				entry.Unit = Criteria.IsContainerised ? QuantityUnit.CN : Env.Registry.FreightWeightUnit;
			}
			else
			{
				if ((Criteria.FreightMode & FreightMode.Containerised) != 0)
				{
					entry.TI_RateCategory = RatingConstants.RateCategory.FCL;
					if ((Criteria.FreightMode & FreightMode.SEA) != 0)
					{
						entry.TI_Mode = Core.Constants.RateMode.SEA;
					}
					else if ((Criteria.FreightMode & FreightMode.ROA) != 0)
					{
						entry.TI_Mode = Core.Constants.RateMode.ROA;
					}
					else if ((Criteria.FreightMode & FreightMode.RAI) != 0)
					{
						entry.TI_Mode = Core.Constants.RateMode.RAI;
					}

					entry.Unit = QuantityUnit.CN;
				}
				else
				{
					entry.TI_RateCategory = RatingConstants.RateCategory.LCL;
					entry.TI_Mode = Criteria.FreightMode.ToString();
					entry.Unit = Criteria.JobMeasures.GetUnit(MeasureType.Chargeable);
				}

				if ((Criteria.FreightMode & FreightMode.SEA) != 0)
				{
					SetConversionFactor(entry, ConversionFactor.Standard.Metric.Sea);
				}
				else if ((Criteria.FreightMode & FreightMode.ROA) != 0)
				{
					entry.Unit = Env.Registry.FreightWeightUnit;

					var origin = entry.Origin();
					if (origin != null && origin.Country != null && (origin.Country.RN_Code == Constants.CountryCodes.Australia || origin.Country.RN_Code == Constants.CountryCodes.UnitedStates))
					{
						SetConversionFactor(entry, ConversionFactor.Standard.Metric.RoadAUUS);
					}
					else
					{
						SetConversionFactor(entry, ConversionFactor.Standard.Metric.Road);
					}
				}
				else if ((Criteria.FreightMode & FreightMode.RAI) != 0)
				{
					SetConversionFactor(entry, ConversionFactor.Standard.Metric.Rail);
				}
			}
		}

		void SetConversionFactor(RateEntry entry, ConversionFactor conversionFactor)
		{
			foreach (RateLine line in entry.RateLines)
			{
				if (line.ConversionFactor.IsEmpty && line.TL_ActualPercentage != 100 && line.TL_WeightVolume != QuantityUnit.LM)
				{
					line.ConversionFactor = conversionFactor;
				}
			}
		}

		#endregion

		#region Job Service Rate

		public IEnumerable<IRateEntry> GetAllJobServiceRates(bool isCosting)
		{
			var serviceInfos = Criteria.JobServices.Where(info => info.IsEnabled &&
				info.IsCostForSpotRate == isCosting &&
				(
					(!info.Rate.IsEmpty && !info.Unit.IsEmpty)
					||
					!info.TotalCost.IsEmpty
				)).ToList();

			foreach (var serviceInfo in serviceInfos)
			{
				var serviceSpotRate = CreateJobServiceRate(serviceInfo, Criteria.Company, isCosting);
				if (serviceSpotRate != null)
				{
					yield return serviceSpotRate;
				}
			}
		}

		IRateEntry CreateJobServiceRate(JobServiceInfo serviceInfo, GlbCompany company, bool isCosting)
		{
			var chargeCode = GetChargeCodeForService(serviceInfo, company.PK.ToGuid());

			if (chargeCode == null)
			{
				return null;
			}

			var orgPk = isCosting || serviceInfo.IsContractorCreditor
				? serviceInfo.Contractor?.PK
				: null;

			var entry = CreateSpotRateEntry(isCosting, orgPk);
			entry.TI_Mode = Core.Constants.RateMode.ALL;
			entry.TI_RX_NKCurrency = !serviceInfo.Currency.IsEmpty ? serviceInfo.Currency : company.LocalCurrency.RX_Code;
			entry.JobServiceForSpotEntry = serviceInfo;
			entry.TI_ContractNumber = serviceInfo.ServiceReference.Substring(0, AutoRateEntry.Schema.TI_ContractNumberMaxLength);
			if (!serviceInfo.IsHiddenService)
			{
				if (!serviceInfo.Container.IsEmpty)
				{
					entry.ContainerPKForSpotEntry = serviceInfo.Container;
				}
				if (!serviceInfo.ContainerType.IsEmpty)
				{
					entry.TI_RC = serviceInfo.ContainerType;
				}
			}

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			line.TL_RX_NKCurrency = entry.TI_RX_NKCurrency;
			line.SpotRateDescription = Res.GetString("7122e619-8f8d-49bb-a766-2d87e1fe7e7e", "{0} Service", serviceInfo.ServiceDescription);

			if (serviceInfo.Unit == JobServiceInfo.Constants.Codes.FlatRate
				|| serviceInfo.TotalCost != 0
				|| serviceInfo.UseTotalCostAndIgnoreRate)
			{
				line.TL_RateCalculator = FlatCalculator.Code;
				line.Calculator[Calculator.Items.Operator.BAS] = serviceInfo.TotalCost != 0 || serviceInfo.UseTotalCostAndIgnoreRate ? serviceInfo.TotalCost : serviceInfo.Rate;
			}
			else
			{
				line.TL_RateCalculator = UnitCalculator.Code;
				line.TL_WeightVolume = GetServiceUnit(serviceInfo);
				line.Calculator[Calculator.Items.Operator.UNT] = serviceInfo.Rate;

				if (QuantityUnit.IsWeight(line.TL_WeightVolume) || QuantityUnit.IsVolume(line.TL_WeightVolume))
				{
					line.TL_Rounding = RatingRoundingTypes.Chargeable;
				}
			}

			return entry;
		}

		AccChargeCode GetChargeCodeForService(JobServiceInfo serviceInfo, Guid companyPK)
		{
			var chargeCodes = Factory.GetCachedValue("AdhocServiceChargeCodes.AC_GC." + companyPK, () => GetAdhocServiceCharges(companyPK));
			var result = chargeCodes.SingleOrDefault(x => x.AC_ChargeGroup == serviceInfo.ChargeCodeGroup && x.AC_ChargeSubGroup == serviceInfo.ServiceCode);
			if (result != null)
			{
				return result;
			}

			var registryItem = GetJobServiceChargeCodeFromRegistry(serviceInfo);
			if (registryItem != null)
			{
				var registryItemPK = registryItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				result = Factory.Load<AccChargeCode>(registryItemPK);
			}

			return result;
		}

		AccChargeCode[] GetAdhocServiceCharges(Guid companyPK)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_IsAdhocServiceCharge, true);
			query.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, companyPK);

			return Factory.Load<AccChargeCode>(query);
		}

		static ChargeCodeRegistryItem GetJobServiceChargeCodeFromRegistry(JobServiceInfo serviceInfo)
		{
			var isDestinationService = serviceInfo.ChargeCodeGroup == ChargeCodeGroupList.Codes.Destination;

			switch (serviceInfo.ServiceCode)
			{
				case ChargeCodeSubGroupList.CartageDemurrageTotal:
					return isDestinationService
						? RatingDataRegistry.Instance.DestinationDemurrageServiceChargeCode
						: RatingDataRegistry.Instance.OriginDemurrageServiceChargeCode;

				case ChargeCodeSubGroupList.ContainerDetention:
					return isDestinationService
						? RatingDataRegistry.Instance.DestinationDetentionServiceChargeCode
						: RatingDataRegistry.Instance.OriginDetentionChargeCode;

				case ChargeCodeSubGroupList.Labor:
					return isDestinationService
						? RatingDataRegistry.Instance.DestinationLaborServiceChargeCode
						: RatingDataRegistry.Instance.OriginLaborServiceChargeCode;

				case ChargeCodeSubGroupList.Storage:
					return isDestinationService
						? RatingDataRegistry.Instance.DestinationStorageServiceChargeCode
						: RatingDataRegistry.Instance.OriginStorageChargeCode;

				case ChargeCodeSubGroupList.CarrierStorage:
					return isDestinationService
						? RatingDataRegistry.Instance.DestinationCarrierStorageChargeCode
						: RatingDataRegistry.Instance.OriginCarrierStorageChargeCode;

				case ChargeCodeSubGroupList.MergedDemurrageDetention:
					return isDestinationService
						? RatingDataRegistry.Instance.DestinationMergedDemurrageDetentionChargeCode
						: RatingDataRegistry.Instance.OriginMergedDemurrageDetentionChargeCode;

				default:
					return null;
			}
		}

		ZString GetServiceUnit(JobServiceInfo serviceInfo)
		{
			switch (serviceInfo.Unit)
			{
				case JobServiceInfo.Constants.Codes.Day:
					return QuantityUnit.DY;

				case JobServiceInfo.Constants.Codes.Hour:
					return QuantityUnit.HR;

				case JobServiceInfo.Constants.Codes.ServiceOccurrence:
					return QuantityUnit.SV;

				case JobServiceInfo.Constants.Codes.Chargeable:
					var chargeableUnit = GetUnitFromMeasure(MeasureType.Chargeable);
					return chargeableUnit;

				case JobServiceInfo.Constants.Codes.PickUpDistance:
					return GetUnitFromMeasure(MeasureType.PickupDistance);

				case JobServiceInfo.Constants.Codes.DeliveryDistance:
					return GetUnitFromMeasure(MeasureType.DeliveryDistance);

				case JobServiceInfo.Constants.Codes.Container:
					return QuantityUnit.CN;

				default:
					return ZString.Empty;
			}
		}

		ZString GetUnitFromMeasure(MeasureType measureType)
			=> Criteria.JobMeasures.GetUnit(measureType);

		#endregion
	}
}

