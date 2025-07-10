using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.NewService
{
	public class XmlToRefApplicationAttribute
	{
		IEnumerable<RefApplicationAttribute> CachedApplicationAttributes => _cachedApplicationAttributes;
		IEnumerable<RefApplicationAttribute> _cachedApplicationAttributes;
		IEnumerable<ApplicationConfiguration> CachedApplicationConfigurations => _cachedApplicationConfigurations;
		IEnumerable<ApplicationConfiguration> _cachedApplicationConfigurations;

		public IEnumerable<ApplicationConfiguration> GetApplicationConfigurations(string filePath)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));
			if (CachedApplicationConfigurations != null)
			{
				return CachedApplicationConfigurations;
			}
			return GetApplicationConfigurationsCore(filePath);
		}

		IEnumerable<ApplicationConfiguration> GetApplicationConfigurationsCore(string xmlFilePath)
		{
			Argument.NotNullOrEmpty(xmlFilePath, nameof(xmlFilePath));
			if (CachedApplicationConfigurations != null)
			{
				return CachedApplicationConfigurations;
			}

			var serializer = new XmlSerializer(typeof(List<ApplicationConfiguration>));
			var xmlObjects = new List<ApplicationConfiguration>();
			using (var xmlReader = XmlReader.Create(xmlFilePath))
			{
				xmlObjects = serializer.Deserialize(xmlReader) as List<ApplicationConfiguration>;
			}
			_cachedApplicationConfigurations = xmlObjects;
			return xmlObjects;
		}

		public IEnumerable<RefApplicationAttribute> GetApplicationAttributes(string filePath)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));
			if (CachedApplicationAttributes != null)
			{
				return CachedApplicationAttributes;
			}
			return GetApplicationAttributesCore(filePath);
		}

		IEnumerable<RefApplicationAttribute> GetApplicationAttributesCore(string xml)
		{
			Argument.NotNullOrEmpty(xml, nameof(xml));
			if (CachedApplicationAttributes != null)
			{
				return CachedApplicationAttributes;
			}

			var xmlObjects = GetApplicationConfigurationsCore(xml);
			var result = new List<RefApplicationAttribute>();
			if (xmlObjects != null && xmlObjects.Any())
			{
				foreach (var xmlObj in xmlObjects)
				{
					result.AddRange(CreateApplicationAttribute(xmlObj));
				}
			}
			_cachedApplicationAttributes = result;
			return result;
		}

		static IEnumerable<RefApplicationAttribute> CreateApplicationAttribute(ApplicationConfiguration applicationConfiguration)
		{
			Argument.NotNull(applicationConfiguration, nameof(applicationConfiguration));
			foreach (var config in applicationConfiguration.ConfigData)
			{
				yield return new RefApplicationAttribute
				{
					RAA_ConfigFilePath = applicationConfiguration.ApplicationConfigFileName,
					RAA_AttributeName = config.Name,
					RAA_Value = config.Value,
					RAA_RAT_NKType = "",
					RAA_PK = Guid.NewGuid(),
					RAA_JobGroup = applicationConfiguration.ApplicationJobGroup
				};
			}
		}
	}
}
