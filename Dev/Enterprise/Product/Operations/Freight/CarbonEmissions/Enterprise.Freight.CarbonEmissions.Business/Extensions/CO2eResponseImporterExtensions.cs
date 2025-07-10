using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class CO2eResponseImporterExtensions
	{
		public static ICO2eResponseImporter GetResponseImporter(this ICO2eCalculationSupporter supporter, BusinessObjectFactory factory, IXmlImportLogger logger = null)
		{
			var supporterType = supporter.GetType();
			var attribute = supporterType.GetCustomAttribute<ResponseImporterAttribute>()
					   ?? supporterType.GetInterfaces()
						   .Select(interfaceType => interfaceType.GetCustomAttribute<ResponseImporterAttribute>())
						   .FirstOrDefault(attr => attr != null);

			if (attribute == null)
			{
				ErrorReporter.ReportOnce("ICO2eCalculationSupporterExtensions.GetResponseImporter", new InvalidOperationException($"No ResponseImporterAttribute found on {supporterType.Name}"));
				return null;
			}

			var importerType = Type.GetType(attribute.ImporterTypeName);
			if (importerType == null)
			{
				ErrorReporter.ReportOnce("ICO2eCalculationSupporterExtensions.GetResponseImporter", new InvalidOperationException($"Type {attribute.ImporterTypeName} could not be found."));
				return null;
			}

			var constructor = importerType.GetConstructor(new[] { typeof(BusinessObjectFactory), typeof(IXmlImportLogger) });

			if (constructor != null && logger != null)
			{
				return constructor.Invoke(new object[] { factory, logger }) as ICO2eResponseImporter;
			}
			else if (logger == null)
			{
				constructor = importerType.GetConstructor(new[] { typeof(BusinessObjectFactory) });
				if (constructor != null)
				{
					return constructor.Invoke(new object[] { factory }) as ICO2eResponseImporter;
				}
				else
				{
					ErrorReporter.ReportOnce("ICO2eCalculationSupporterExtensions.GetResponseImporter", new InvalidOperationException($"No suitable constructor found for {importerType.Name}"));
					return null;
				}
			}
			return null;
		}
	}
}
