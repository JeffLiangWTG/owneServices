using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Quartz.Util;
using Quartz.Xml.JobSchedulingData20;

namespace CargoWise.RefDbRepo.ApplicationConfigurationGenerator
{
	class JobDetailFileReader : IJobDetailReader
	{
		public JobDetailFileReader(IEnumerable<string> fileNames)
		{
			this.fileNames = fileNames;
		}

		readonly IEnumerable<string> fileNames;

		public async Task<List<jobdetailType>> GetJobDetails()
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
	}
}
