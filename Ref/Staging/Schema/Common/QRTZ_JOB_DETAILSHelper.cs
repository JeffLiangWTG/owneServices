using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Quartz;
using Quartz.Util;
using Quartz.Xml.JobSchedulingData20;

namespace CargoWise.RefDbRepo.Staging.Common
{
	public static class QRTZ_JOB_DETAILSHelper
	{
		public static void SetCalculatedProperties(QRTZ_JOB_DETAILS qrtzJob)
		{
			Argument.NotNull(qrtzJob, nameof(qrtzJob));
			if (qrtzJob.JOB_DATA != null)
			{
				var map = Serializer.Deserialize<JobDataMap>(qrtzJob.JOB_DATA);
				if (map != null)
				{
					qrtzJob.CountryCode = map.FirstOrDefault(x => x.Key == nameof(QRTZ_JOB_DETAILS.CountryCode)).Value?.ToString();
					qrtzJob.ProgramArgs = map.FirstOrDefault(x => x.Key == nameof(QRTZ_JOB_DETAILS.ProgramArgs)).Value?.ToString();
					qrtzJob.ProgramExePath = map.FirstOrDefault(x => x.Key == nameof(QRTZ_JOB_DETAILS.ProgramExePath)).Value?.ToString();
				}
			}
		}

		public static async Task<List<jobdetailType>> GetJobDetailsFromXmlFiles(IEnumerable<string> fileNames)
		{
			var jobDetails = new List<jobdetailType>();
			if (fileNames == null || !fileNames.Any())
			{
				return jobDetails;
			}
			foreach (var fileName in fileNames)
			{
				var filePath = FileUtil.ResolveFile(fileName) ?? fileName;
				using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var sr = new StreamReader(stream))
				{
					var xmlContent = await sr.ReadToEndAsync();
					var xmlSerializer = new XmlSerializer(typeof(QuartzXmlConfiguration20));
					using (var xmlReader = XmlReader.Create(new StringReader(xmlContent)))
					{
						var data = (QuartzXmlConfiguration20)xmlSerializer.Deserialize(xmlReader);
						if (data != null && data.schedule != null)
						{
							jobDetails.AddRange(data.schedule.Where(x => x?.job != null).SelectMany(x => x.job));
						}
						else
						{
							throw new ArgumentException("Job definition data from XML was null after deserialization");
						}
					}
				}
			}

			return jobDetails;
		}

		public static byte[] SerializeToJobData(string programExePath, string programArgs, string countryCode)
		{
			var dicMap = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(programExePath))
			{
				dicMap.Add(nameof(QRTZ_JOB_DETAILS.ProgramExePath), programExePath);
			}
			if (!string.IsNullOrEmpty(programArgs))
			{
				dicMap.Add(nameof(QRTZ_JOB_DETAILS.ProgramArgs), programArgs);
			}
			if (!string.IsNullOrEmpty(countryCode))
			{
				dicMap.Add(nameof(QRTZ_JOB_DETAILS.CountryCode), countryCode);
			}
			return SerializeToJobData(dicMap);
		}

		public static byte[] SerializeToJobData(Dictionary<string, string> mapDictionary)
		{
			if (mapDictionary == null)
			{
				return null;
			}
			var jobDataMap = new JobDataMap(mapDictionary);
			return Serializer.Serialize(jobDataMap);
		}
	}
}
