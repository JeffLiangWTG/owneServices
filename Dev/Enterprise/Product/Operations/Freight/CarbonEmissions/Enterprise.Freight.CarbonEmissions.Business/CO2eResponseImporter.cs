using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public abstract class CO2eResponseImporter : ICO2eResponseImporter
	{
		protected bool SkipApplicableCheck { get; private set; }
		protected bool IsManualCalculation { get; private set; }

		protected readonly IXmlImportLogger logger;
		protected readonly BusinessObjectFactory factory;
		protected readonly EmissionsCalculationLogger EmissionsLogger;

		public CO2eResponseImporter(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
			SkipApplicableCheck = false;
			IsManualCalculation = false;
			EmissionsLogger = new EmissionsCalculationLogger();
		}

		public CO2eResponseImporter(BusinessObjectFactory factory) : this(factory, new XmlSessionTracker(new SimpleLogger()))
		{
			this.logger = new XmlSessionTracker(new SimpleLogger());
			SkipApplicableCheck = true;
			IsManualCalculation = true;
		}

		public string LogMessages => string.Join("\r\n", logger.Logs);

		public abstract bool ImportGreenHouseGasEmission(Shipment universalShipment, ICO2eCalculationSupporter supporter, string name = default, (decimal TotalCO2e, IEnumerable<BusinessObject> Transports) previousCO2eValue = default);

		public void ImportGreenHouseGasEmission(ITopLevelDataObject topLevelDataObject, ICO2eCalculationSupporter supporter, string name = default, (decimal TotalCO2e, IEnumerable<BusinessObject> Transports) previousCO2eValue = default)
		{
			if (topLevelDataObject is Shipment shipment)
			{
				var succeeded = ImportGreenHouseGasEmission(shipment, supporter, name, previousCO2eValue);
				if (succeeded && supporter.SaveEmissionsLogToNoteOnCalculated)
				{
					EmissionsLogger.SaveToNote(supporter as IStmNoteParent);
				}

				supporter.OnCalculated(succeeded);
			}
		}

		protected void AddStartedLog()
		{
			logger.Log(LogType.Information, Res.GetString("09929eda-3a25-4968-9cce-32115e909358", "Importing greenhouse gas emissions calculation result."));
		}

		protected void AddCalculatedLog(ICO2eProvider provider)
		{
			var bizObj = provider as BusinessObject;
			if (bizObj != null)
			{
				logger.Log(LogType.Information, Res.GetString("ddcd5bdd-f1a7-44f0-a5ba-10e35060c735", "CO2e is calculated for {0}: {1}", bizObj.GetType().Name, bizObj.HumanReadableName));
			}
		}

		protected void AddNotApplicableLog(string humanReadableName)
		{
			logger.Log(LogType.Warning, Res.GetString("28ff2170-7b46-45cd-9e24-1d29b1e407d2", "Cannot populate CO2e for {0} because CO2e Calculation input parameters have been changed", humanReadableName));
		}

		protected string GetTotalCO2eExceedMaximumError(decimal value)
		{
			return Res.GetString("1ece4929-50f1-4ba9-b8c0-c1872696bbf9",
						"Total CO2e {0} is greater than maximum limit {1}, it will be discarded", value, MaximumCO2eValue(JobCO2eSchema.JCO_TotalCO2e));
		}

		protected string GetMissingCO2eValueError()
		{
			return Res.GetString("53450c73-d147-4913-acf9-0c4d32a5d31d", "Missing CO2e value in the response.");
		}

		protected ZDecimal MaximumCO2eValue(SchemaDecimalColumn column)
		{
			int databasePrecision = column.Precision;
			int databaseScale = column.Scale;

			var maxValueStr = new string('9', databasePrecision - databaseScale) + "." + new string('9', databaseScale);
			return ZDecimal.Parse(maxValueStr);
		}
	}
}
