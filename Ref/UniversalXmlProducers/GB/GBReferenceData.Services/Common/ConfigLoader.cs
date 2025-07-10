using System;
using System.IO;
using System.Xml;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Common
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<Pending>")]
	public class ConfigLoader<T> where T : new()
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
		public static T LoadConfigFileSources(IConfigProvider configProvider)
		{
			try
			{
				using (var f = File.OpenRead(configProvider.ConfigFile))
				{
					return LoadConfigStream(f);
				}
			}
			catch (Exception exception)
			{
				throw new ApplicationException($"Failed to load configuration file: {configProvider.ConfigFile}", exception);
			}
		}

		static T LoadConfigStream(Stream configFile)
		{
			var configs = new T();

			var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

			using (var streamReader = XmlReader.Create(configFile))
			{
				configs = (T)serializer.Deserialize(streamReader);
			}

			return configs;
		}
	}
}
