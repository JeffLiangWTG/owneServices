using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class CO2eLocationBasedResponseImporter : CO2eResponseImporter
	{
		public CO2eLocationBasedResponseImporter(BusinessObjectFactory factory, IXmlImportLogger logger) : base(factory, logger)
		{
		}

		public CO2eLocationBasedResponseImporter(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override bool ImportGreenHouseGasEmission(Shipment dataObject, ICO2eCalculationSupporter supporter, string name = null, (decimal TotalCO2e, IEnumerable<BusinessObject> Transports) previousCO2eValue = default)
		{
			if (supporter is not ICO2eLocationBasedSupporter topLevelBO)
			{
				return false;
			}

			if (!SkipApplicableCheck && topLevelBO.GetCO2eStatus() == CO2eStatusList.Codes.NotCurrent)
			{
				topLevelBO.RecordLog(CO2eEventType.Rejected, (NoResString)"Input value(s) have changed");
				AddNotApplicableLog(name);
				return false;
			}

			if (!AreCO2eValuesValid(dataObject, out var reason))
			{
				HandleInvalidCO2eValues(topLevelBO, reason);
				return false;
			}

			AddStartedLog();

			HandleCO2eForTopLevel(dataObject, topLevelBO, previousCO2eValue.TotalCO2e);
			return true;
		}

		void HandleCO2eForTopLevel(Shipment dataObject, ICO2eLocationBasedSupporter topLevelBO, decimal previousCO2eValue)
		{
			if (topLevelBO != null)
			{
				topLevelBO.ImportEmission(dataObject.GreenhouseGasEmission);
				topLevelBO.SetCO2eStatus(CO2eStatusList.Codes.Current);
				topLevelBO.RecordLog(CO2eEventType.Updated, topLevelBO.GetTotalCO2e().ToString(), previousCO2eValue);
				AddCalculatedLog(topLevelBO);
			}
		}

		bool AreCO2eValuesValid(Shipment dataObject, out string reason)
		{
			reason = string.Empty;

			if (dataObject.GreenhouseGasEmission == null || !dataObject.GreenhouseGasEmission.CO2e.HasValue)
			{
				reason = GetMissingCO2eValueError();
				return false;
			}

			if (!dataObject.GreenhouseGasEmission.IsTotalCO2eValueValid(out var value))
			{
				reason = GetTotalCO2eExceedMaximumError(value);
				return false;
			}

			return true;
		}

		void HandleInvalidCO2eValues(ICO2eLocationBasedSupporter topLevelBO, string reason)
		{
			logger.Log(LogType.Warning, reason);
			topLevelBO.RecordLog(CO2eEventType.Rejected, reason);
			topLevelBO.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
		}
	}
}
