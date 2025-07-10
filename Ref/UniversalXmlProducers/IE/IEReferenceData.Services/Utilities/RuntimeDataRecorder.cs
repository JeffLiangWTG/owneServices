using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public static class RuntimeDataRecorder
	{
		public enum Keys
		{
			ExciseDutyRatePublicateDate_Mineral_Oil,
			ExciseDutyRatePublicateDate_Alcohol_Products,
			ExciseDutyRatePublicateDate_Tobacco_Products,
			DownloadROSErrorList
		}

		static object @lock = new object();
		static RuntimeDataRecorder()
		{
			lock (@lock)
			{
				if (!File.Exists(LogFilePath))
				{
					using (File.Create(LogFilePath))
					{ }
				}
			}
		}

		static string LogFilePath { get; } = GetLogFilePath();
		static string GetLogFilePath()
		{
			const string logFileExtention = ".RuntimeData.json";
			var executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
			var directoryPath = Path.GetDirectoryName(executingAssemblyLocation);
			var logFileName = Path.GetFileNameWithoutExtension(executingAssemblyLocation) + logFileExtention;
			return Path.Combine(directoryPath, logFileName);
		}

		public static void Write(Keys key, object value)
		{
			lock (@lock)
			{
				JObject log = Read() ?? new JObject();
				log[key.ToString()]= JToken.FromObject(value);
				File.WriteAllText(LogFilePath, log.ToString());
			}
		}

		public static object Read(Keys key)
		{
			var record = Read();
			if (record == null)
			{
				return null;
			}
			else
			{
				var result = (JValue)record[key.ToString()];
				return result?.Value ?? default;
			}
		}

		static JObject Read()
		{
			dynamic result = null;
			if (File.Exists(LogFilePath))
			{
				var json = File.ReadAllText(LogFilePath);
				try
				{
					result = JsonConvert.DeserializeObject<dynamic>(json);
				}
				catch (JsonReaderException ex)
				{
					Console.Error.WriteLine(ex);
				}
			}
			return result;
		}
	}
}
