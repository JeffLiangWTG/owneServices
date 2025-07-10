using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Common.Test
{
	[TestFixture]
	class QRTZ_JOB_DETAILSHelperFixture
	{
		[TestCase(null, null, null)]
		[TestCase("AU", null, null)]
		[TestCase(null, "AA", null)]
		[TestCase(null, null, "CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe")]
		[TestCase("AU", "BB", "..\\..\\UniversalXMLProducers\\net8.0\\CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe")]
		public void SetCalculatedProperties(string countryCode, string programArgs, string programExePath)
		{
			var quartzJob = new QRTZ_JOB_DETAILS
			{
				SCHED_NAME = "RefDbRepoQuartzServer",
				JOB_NAME = "Job 1",
				JOB_GROUP = "Group 1",
				JOB_DATA = QRTZ_JOB_DETAILSHelper.SerializeToJobData(programExePath, programArgs, countryCode)
			};
			Assert.DoesNotThrow(() => QRTZ_JOB_DETAILSHelper.SetCalculatedProperties(quartzJob));
			if (!string.IsNullOrEmpty(countryCode))
			{
				Assert.AreEqual(countryCode, quartzJob.CountryCode);
			}
			if (!string.IsNullOrEmpty(programArgs))
			{
				Assert.AreEqual(programArgs, quartzJob.ProgramArgs);
			}
			if (!string.IsNullOrEmpty(programExePath))
			{
				Assert.AreEqual(programExePath, quartzJob.ProgramExePath);
			}
		}

		[Test]
		public async Task GetJobDetailsFromXmlFiles()
		{
			var fileNames = new string[0];
			var result = await QRTZ_JOB_DETAILSHelper.GetJobDetailsFromXmlFiles(fileNames);
			Assert.AreEqual(0, result.Count);

			var folderPath = Path.Combine(AppContext.BaseDirectory, @"..\net8.0\Configuration");
			fileNames = Directory.GetFiles(folderPath, "*.xml");
			Assert.DoesNotThrowAsync(async () => result = await QRTZ_JOB_DETAILSHelper.GetJobDetailsFromXmlFiles(fileNames));
			Assert.Greater(result.Count, 290);
			var job = result.First(x => x.name == "RefAirline UXML");
			Assert.AreEqual("Air Team", job.group);
			Assert.AreEqual("CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzProcessorRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common", job.jobtype);
			Assert.True(job.durable);
			Assert.False(job.recover);
			Assert.AreEqual(1, job.jobdatamap.entry.Length);
			var entry = job.jobdatamap.entry[0];
			Assert.AreEqual("ProgramExePath", entry.key);
			Assert.AreEqual(@"..\..\UniversalXMLProducers\net8.0\CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer.exe", entry.value);

			job = result.First(x => x.name == "UXML Merger");
			Assert.AreEqual("Reference Data", job.group);
			Assert.AreEqual("CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzAppRunner, CargoWise.RefDbRepo.Staging.Schedulers.Common", job.jobtype);
			Assert.AreEqual(1, job.jobdatamap.entry.Length);
			entry = job.jobdatamap.entry[0];
			Assert.AreEqual("ProgramExePath", entry.key);
			Assert.AreEqual(@"..\net8.0\CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.exe", entry.value);

			job = result.First(x => x.name == "Data Updater Scheduler");
			Assert.AreEqual("Reference Data", job.group);
			Assert.AreEqual("CargoWise.RefDbRepo.Staging.Schedulers.Common.QuartzDataUpdater, CargoWise.RefDbRepo.Staging.Schedulers.Common", job.jobtype);
			Assert.Null(job.jobdatamap);

			var fileNamesString = @"..\net8.0\Configuration\QuartzAppRunnerJobs.xml,..\net8.0\Configuration\SingleInstanceQuartzProcessorRunnerJobs.xml";
			fileNames = fileNamesString.Split(',');
			Assert.DoesNotThrowAsync(async () => result = await QRTZ_JOB_DETAILSHelper.GetJobDetailsFromXmlFiles(fileNames));
			Assert.GreaterOrEqual(result.Count, 10);
		}

		[Test]
		public void GetJobDetailsFromXmlFiles_ThrowExceptionIfJobDefinitionIsNull()
		{
			var testFile = Path.Combine(AppContext.BaseDirectory, "TestFiles\\QuartzJobsWithNoDefinition.xml");
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await QRTZ_JOB_DETAILSHelper.GetJobDetailsFromXmlFiles(new[] { testFile }));
			Assert.AreEqual("Job definition data from XML was null after deserialization", exception.Message);
		}

		[TestCase(null, null, null, "{}")]
		[TestCase("AU", null, null, "{\"CountryCode\":\"AU\"}")]
		[TestCase(null, "AA", null, "{\"ProgramArgs\":\"AA\"}")]
		[TestCase(null, null, "CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe", "{\"ProgramExePath\":\"CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe\"}")]
		[TestCase("AU", "BB", "..\\..\\UniversalXMLProducers\\net6.0\\AUReferenceData.CmdLine.exe", "{\"ProgramExePath\":\"..\\\\..\\\\UniversalXMLProducers\\\\net6.0\\\\AUReferenceData.CmdLine.exe\",\"ProgramArgs\":\"BB\",\"CountryCode\":\"AU\"}")]
		public void SerializeToJobData(string countryCode, string programArgs, string programExePath, string expectedResult)
		{
			var jobData = QRTZ_JOB_DETAILSHelper.SerializeToJobData(programExePath, programArgs, countryCode);
			var jobDataStr = Encoding.UTF8.GetString(jobData);
			Assert.AreEqual(expectedResult, jobDataStr);
		}

		[Test]
		public void SerializeToJobData_Dictionary()
		{
			Dictionary<string, string> mapDic = null;
			var jobData = QRTZ_JOB_DETAILSHelper.SerializeToJobData(mapDic);
			Assert.Null(jobData);

			mapDic = new Dictionary<string, string>();
			jobData = QRTZ_JOB_DETAILSHelper.SerializeToJobData(mapDic);
			Assert.AreEqual("{}", Encoding.UTF8.GetString(jobData));

			mapDic.Add("Key1", "Value1");
			mapDic.Add("Key2", "Value2");
			jobData = QRTZ_JOB_DETAILSHelper.SerializeToJobData(mapDic);
			Assert.AreEqual("{\"Key1\":\"Value1\",\"Key2\":\"Value2\"}", Encoding.UTF8.GetString(jobData));
		}
	}
}
