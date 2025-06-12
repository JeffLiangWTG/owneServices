using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace eHubDeploymentTools
{
	class PropertyHelper
	{
		public static List<Property> GetPropertyListFromFile()
		{
			var propertyList = new List<Property>();
			var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var settingsCollection = configuration.AppSettings.Settings;

			foreach (KeyValueConfigurationElement element in settingsCollection)
			{
				var property = Property.Create(element.Key, element.Value);
				if (property != null)
				{
					propertyList.Add(property);
				}
			}

			return propertyList;
		}


		public static void SaveProperties(List<Property> propertyList)
		{
			var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var settingsCollection = configuration.AppSettings.Settings;

			foreach (var property in propertyList)
			{
				settingsCollection[property.Key].Value = property.Value;
			}

			configuration.Save();
		}
	}
}
