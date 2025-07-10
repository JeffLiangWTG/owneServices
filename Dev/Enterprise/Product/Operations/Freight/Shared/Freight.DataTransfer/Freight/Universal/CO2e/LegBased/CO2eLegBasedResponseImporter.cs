using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using EZC = Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class CO2eLegBasedResponseImporter : CO2eResponseImporter
	{
		RefUNLOCO.Loader UNLOCOLoader => unlocoLoader ??= new RefUNLOCO.Loader(factory);
		RefUNLOCO.Loader unlocoLoader;
		readonly List<WeightData> cacheEmptyContainerLogs = [];

		public CO2eLegBasedResponseImporter(BusinessObjectFactory factory, IXmlImportLogger logger) : base(factory, logger)
		{
		}

		public CO2eLegBasedResponseImporter(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override bool ImportGreenHouseGasEmission(UniversalShipment dataObject, ICO2eCalculationSupporter supporter, string humanReadableName, (decimal TotalCO2e, IEnumerable<BusinessObject> Transports) previousCO2eValue)
		{
			if (supporter is not ICO2eLegBasedSupporter topLevelBO)
			{
				return false;
			}

			if (!SkipApplicableCheck && !topLevelBO.IsCO2eResponseApplicable(dataObject))
			{
				HandleNotApplicableCO2eResponse(dataObject, topLevelBO, humanReadableName);
				return false;
			}

			if (!AreCO2eValuesValid(dataObject, topLevelBO, out var reason))
			{
				HandleInvalidCO2eValues(topLevelBO, reason);
				return false;
			}

			AddStartedLog();

			UpdateAllLegsStatus(topLevelBO, CO2eStatusList.Codes.Current);
			HandleCO2eForSubShipments(dataObject, topLevelBO, previousCO2eValue.Transports);
			HandleCO2eForLegs(dataObject, topLevelBO, previousCO2eValue.Transports);
			HandleCO2eForTopLevel(dataObject, topLevelBO, previousCO2eValue.TotalCO2e);

			EmissionsLogger.LogHeader(supporter, previousCO2eValue.TotalCO2e, IsManualCalculation);
			EmissionsLogger.LogJobLevelParameters(topLevelBO);
			cacheEmptyContainerLogs.ForEach(dto => EmissionsLogger.LogEmptyContainerParameters(topLevelBO, dto));
			cacheEmptyContainerLogs.Clear();
			return true;
		}

		void HandleCO2eForTopLevel(UniversalShipment dataObject, ICO2eLegBasedSupporter topLevelBO, decimal previousCO2eValue)
		{
			if (topLevelBO != null)
			{
				var additionalEmissions = topLevelBO.GetTotalEmptyContainerEmissions();
				var additionalSupportersHaveBeenCalculated = true;

				if (topLevelBO.AdditionalCalculationSupporters.Length > 0)
				{
					if (topLevelBO.AdditionalCalculationSupporters.All(x => x.Supporter.HasBeenCalculated()))
					{
						additionalEmissions += topLevelBO.GetTotalAdditionalSupportersEmission();
					}
					else
					{
						additionalSupportersHaveBeenCalculated = false;
					}
				}

				topLevelBO.ImportEmission(dataObject.GreenhouseGasEmission, additionalEmissions: additionalEmissions);

				if (additionalSupportersHaveBeenCalculated)
				{
					topLevelBO.SetCO2eStatus(CO2eStatusList.Codes.Current);
					topLevelBO.RecordLog(CO2eEventType.Updated, topLevelBO.GetTotalCO2e().ToString(), previousCO2eValue);
				}
				else
				{
					topLevelBO.SetCO2eStatus(CO2eStatusList.Codes.Pending);
				}

				AddCalculatedLog(topLevelBO);
			}
		}

		void HandleCO2eForLegs(UniversalShipment dataObject, ICO2eLegBasedSupporter topLevelBO, IEnumerable<BusinessObject> legsWithPreviousCO2eValues)
		{
			ForEachLeg(dataObject, topLevelBO, (leg, transportLegDataObject) =>
			{
				var previousCO2eValueForLeg = legsWithPreviousCO2eValues?
					.OfType<ICO2eParent>()
					.FirstOrDefault(x => IsMatchingLeg((ICO2eLegProvider)x, transportLegDataObject))?.GetCO2ePerTonneInKg() ?? 0m;

				leg.ImportEmission(transportLegDataObject.GreenhouseGasEmission);
				leg.RecordLog(CO2eEventType.Updated, UpdateLogForLeg(leg), previousCO2eValueForLeg);
				AddCalculatedLog(leg);
				return true;
			});
		}

		bool IsMatchingLeg(ICO2eLegProvider previousLeg, TransportLeg transportLeg)
		{
			var loadPort = UNLOCOLoader.Load(previousLeg.LoadPort);
			var dischargePort = UNLOCOLoader.Load(previousLeg.DiscPort);

			return loadPort.MatchesUNLOCODataObject(transportLeg.PortOfLoading) &&
				   dischargePort.MatchesUNLOCODataObject(transportLeg.PortOfDischarge);
		}

		void ForEachLeg(UniversalShipment dataObject, ICO2eLegBasedSupporter topLevelBO, Func<ICO2eLegProvider, TransportLeg, bool> action)
		{
			if (topLevelBO.Legs != null && dataObject.TransportLegCollection != null && topLevelBO.ShouldPopulateCO2eForLegs)
			{
				foreach (var transportLegDataObject in dataObject.TransportLegCollection)
				{
					var leg = GetMatchingLeg(topLevelBO, transportLegDataObject);

					if (leg != null)
					{
						using (leg?.WithTempCurrentCO2eCalcSupporter(topLevelBO))
						{
							if (!action(leg, transportLegDataObject))
							{
								return;
							}
						}
					}
				}
			}
		}

		string UpdateLogForLeg(ICO2eLegProvider leg) => leg.GetTotalCO2e() == 0 ? Res.GetString("26c3a5d2-8f66-48e3-848d-260b4e8aafe7", "ERR") : Weight.ConvertSafe(leg.GetTotalCO2e(), Weight.Kilograms, Weight.Kilograms, false).ToString();

		void UpdateAllLegsStatus(ICO2eLegBasedSupporter topLevelBO, string status)
		{
			if (topLevelBO.Legs != null && topLevelBO.ShouldPopulateCO2eForLegs)
			{
				topLevelBO.ForEachLeg(leg => leg.SetCO2eStatus(status));
			}
		}

		ICO2eLegProvider GetMatchingLeg(ICO2eLegBasedSupporter topLevelBO, TransportLeg transportLegDataObject)
		{
			return topLevelBO.Legs.Cast<ICO2eLegProvider>().FirstOrDefault(bizO =>
			{
				var loadPort = UNLOCOLoader.Load(bizO.LoadPort);
				var dischargePort = UNLOCOLoader.Load(bizO.DiscPort);

				return loadPort.MatchesUNLOCODataObject(transportLegDataObject.PortOfLoading)
					&& dischargePort.MatchesUNLOCODataObject(transportLegDataObject.PortOfDischarge);
			});
		}

		void HandleCO2eForSubShipments(UniversalShipment dataObject, ICO2eLegBasedSupporter topLevelBO, IEnumerable<BusinessObject> legsWithPreviousCO2eValues)
		{
			if (dataObject.SubShipmentCollection != null)
			{
				foreach (var subShipment in dataObject.SubShipmentCollection)
				{
					HandleCO2eForEmptyContainers(subShipment, topLevelBO);
					HandleCO2eForLegs(subShipment, topLevelBO, legsWithPreviousCO2eValues);
				}
			}
		}

		void HandleCO2eForEmptyContainers(UniversalShipment dataObject, ICO2eLegBasedSupporter topLevelBO)
		{
			ForEachEmptyContainer(dataObject, topLevelBO, (container, emptyContainerDataObject) =>
			{
				container.ImportEmission(emptyContainerDataObject?.EmptyPickup?.GreenhouseGasEmission, CO2eTypes.EmptyPickup);
				container.ImportEmission(emptyContainerDataObject?.EmptyReturn?.GreenhouseGasEmission, CO2eTypes.EmptyReturn);
				AddCalculatedLog(container);
				cacheEmptyContainerLogs.Add(emptyContainerDataObject);
				return true;
			});
		}

		void ForEachEmptyContainer(UniversalShipment dataObject, ICO2eLegBasedSupporter topLevelBO, Func<ICO2eEmptyContainerProvider, WeightData, bool> action)
		{
			if (!topLevelBO.EmptyContainers.IsNullOrEmpty() && dataObject.EmptyContainerCollection != null)
			{
				foreach (var emptyContainerDataObject in dataObject.EmptyContainerCollection)
				{
					var container = topLevelBO.EmptyContainers
						.Select(x => x.Container)
						.WhereNotNull()
						.FirstOrDefault(c => c.ContainerJobID.Equals(emptyContainerDataObject?.ContainerJobID));

					if (container != null && !action(container, emptyContainerDataObject))
					{
						return;
					}
				}
			}
		}

		void HandleInvalidCO2eValues(ICO2eLegBasedSupporter topLevelBO, string reason)
		{
			logger.Log(LogType.Warning, reason);
			topLevelBO.RecordLog(CO2eEventType.Rejected, reason);
			topLevelBO.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
			UpdateAllLegsStatus(topLevelBO, CO2eStatusList.Codes.Rejected);
		}

		void HandleNotApplicableCO2eResponse(UniversalShipment dataObject, ICO2eLegBasedSupporter topLevelBO, string humanReadableName)
		{
			if (dataObject != null)
			{
				topLevelBO.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
				UpdateAllLegsStatus(topLevelBO, CO2eStatusList.Codes.NotCurrent);
			}
			topLevelBO.RecordLog(CO2eEventType.Rejected, (EZC.NoResString)"Input value(s) have changed");
			AddNotApplicableLog(humanReadableName);
		}

		bool AreCO2eValuesValid(UniversalShipment dataObject, ICO2eLegBasedSupporter topLevelBo, out string reason)
		{
			reason = ZString.Empty;

			if (dataObject.GreenhouseGasEmission == null || !dataObject.GreenhouseGasEmission.CO2e.HasValue)
			{
				reason = GetMissingCO2eValueError();
				return false;
			}

			if (!IsGreenhouseGasEmissionValid(dataObject.GreenhouseGasEmission, (EZC.NoResString)"Total", out reason))
			{
				return false;
			}

			var legReason = string.Empty;

			ForEachLeg(dataObject, topLevelBo, (leg, transportLegDataObject) =>
			{
				if (!IsGreenhouseGasEmissionValid(transportLegDataObject.GreenhouseGasEmission, (EZC.NoResString)"Transport Leg", out legReason))
				{
					return false;
				}
				return true;
			});

			if (legReason.Length > 0)
			{
				reason = legReason;
				return false;
			}

			var emptyContainerReason = string.Empty;

			ForEachEmptyContainer(dataObject, topLevelBo, (container, emptyContainerDataObject) =>
			{
				if (!IsGreenhouseGasEmissionValid(emptyContainerDataObject?.EmptyPickup?.GreenhouseGasEmission, (EZC.NoResString)"Empty Pickup", out emptyContainerReason))
				{
					return false;
				}

				if (!IsGreenhouseGasEmissionValid(emptyContainerDataObject?.EmptyReturn?.GreenhouseGasEmission, (EZC.NoResString)"Empty Return", out emptyContainerReason))
				{
					return false;
				}

				return true;
			});

			if (emptyContainerReason.Length > 0)
			{
				reason = emptyContainerReason;
				return false;
			}

			return true;
		}

		public bool IsGreenhouseGasEmissionValid(GreenhouseGasEmission greenhouseGasEmission, string name, out string reason)
		{
			reason = string.Empty;

			if (!greenhouseGasEmission.IsTotalCO2eValueValid(out var value))
			{
				reason = GetTotalCO2eExceedMaximumError(value);
				return false;
			}

			if (!greenhouseGasEmission.IsCO2eValueValid(out value, out var isTEU))
			{
				reason = InvalidCO2eErrorMessage(name, isTEU, value, MaximumCO2eValue(isTEU ? JobCO2eSchema.JCO_CO2ePerTEUInKg : JobCO2eSchema.JCO_CO2ePerTonneInKg));
				return false;
			}

			return true;
		}

		string InvalidCO2eErrorMessage(string name, bool isTEU, decimal value, ZDecimal maximumValue)
		{
			return Res.GetString("ddd08eee-307b-42e2-b00a-583489f0abb3", "{0} CO2e/{1} {2} is greater than maximum limit {3}, it will be discarded", name, isTEU ? (EZC.NoResString)"teu" : (EZC.NoResString)"tonne", value, maximumValue);
		}
	}
}
