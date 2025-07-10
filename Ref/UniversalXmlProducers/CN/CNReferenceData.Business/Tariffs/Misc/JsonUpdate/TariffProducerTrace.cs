using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class TariffProducerTrace
	{
		protected TariffProducerTrace() { }

		public DateTime UpdatesDetectedTime { get; set; }
		public DateTime LatestCheckForUpdatesTime { get; set; }
		public DateTime LatestGetUpdatesTime { get; set; }
		public int CountOfUpdates { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
			{
				DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
				DateFormatString = "yyyy-MM-dd HH:mm:ss"
			});
		}

		public void SaveToLocalFile()
		{
			File.WriteAllText(TrackingFileLocation, ToString());
		}

		public static readonly string TrackingFileLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + ".trace");

		public static TariffProducerTrace ReadFromLocalFile()
		{
			TariffProducerTrace trace = null;

			if (File.Exists(TrackingFileLocation))
			{
				trace = JsonConvert.DeserializeObject<TariffProducerTrace>(File.ReadAllText(TrackingFileLocation));
			}

			if (trace == null)
			{
				var today = GlobalOption.Instance.Now.Date;
				trace = new TariffProducerTrace
				{
					UpdatesDetectedTime = today.AddDays(-1),
					LatestGetUpdatesTime = today.AddDays(-1),
					LatestCheckForUpdatesTime = today,
				};
			}

			return trace;
		}
	}
}
