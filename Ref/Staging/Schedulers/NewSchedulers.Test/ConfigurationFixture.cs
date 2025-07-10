using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewSchedulers.Test
{
	[TestFixture]
	class ConfigurationFixture
	{
		[Test]
		public void PathExistsForAllProgramsInConfigurationXmlTest()
		{
			Assert.Greater(schedulerConfigurationXmlFiles.Length, 0);
			foreach (var file in schedulerConfigurationXmlFiles)
			{
				var doc = new XmlDocument();
				doc.Load(file);

				foreach (XmlNode node in doc.SelectNodes("//*[local-name()='job-scheduling-data']/*[local-name()='schedule']/*[local-name()='job']/*[local-name()='job-data-map']/*[local-name()='entry'] [contains( ./*[local-name()='key'],'ProgramExePath')]/*[local-name()='value']"))
				{
					Assert.That(File.Exists(Path.Combine(localDirectory, node.InnerText)), "File not found: " + node.InnerText);
				}
			}
		}

		[Test]
		public void NoJobsHaveCountryCode()
		{
			Assert.Greater(schedulerConfigurationXmlFiles.Length, 0);

			var strBuilder = new StringBuilder();
			foreach (var file in schedulerConfigurationXmlFiles)
			{
				var doc = new XmlDocument();
				doc.Load(file);

				var jobNodes = doc.SelectNodes("//*[local-name()='job-scheduling-data']/*[local-name()='schedule']/*[local-name()='job']");
				foreach (XmlNode node in jobNodes)
				{
					var jobName = node.SelectSingleNode("*[local-name()='name']").InnerText;
					var entryKeys = node.SelectNodes("./*[local-name()='job-data-map']/*[local-name()='entry'][contains( ./*[local-name()='key'],'CountryCode')]");
					foreach (XmlNode entryKey in entryKeys)
					{
						strBuilder.AppendLine(CultureInfo.InvariantCulture, $"{jobName} in {Path.GetFileName(file)} should not have CountryCode node.");
					}
				}
			}
			var errMessage = strBuilder.ToString();
			Assert.True(string.IsNullOrEmpty(errMessage), errMessage);
		}

		[Test]
		public void AllProgramsHaveConfiguration()
		{
			var producerProgramsDirectoryInNet8 = Path.Combine(localDirectory, @"..\..\UniversalXMLProducers\net8.0\");
			var executableFiles = Directory.GetFiles(producerProgramsDirectoryInNet8, "*.exe").ToList();
			var exclusionsList = new[]
			{
				"CargoWise.RefDbRepo.IHSReferenceData.CmdLine.exe",
				"CargoWise.RefDbRepo.ILReferenceData.CmdLine.exe",
				"CargoWise.RefDbRepo.MXReferenceData.CmdLine.exe",
				"CargoWise.RefDbRepo.MXRefLocoMap.exe",
				"CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffEndDateFinder.exe",
				"chromedriver.exe",
				"testhost.exe"
			};
			executableFiles.RemoveAll(x => exclusionsList.Any(y => x.EndsWith(y, StringComparison.OrdinalIgnoreCase)));

			foreach (var file in schedulerConfigurationXmlFiles)
			{
				var doc = new XmlDocument();
				doc.Load(file);

				foreach (XmlNode node in doc.SelectNodes("//*[local-name()='job-scheduling-data']/*[local-name()='schedule']/*[local-name()='job']/*[local-name()='job-data-map']/*[local-name()='entry'] [contains( ./*[local-name()='key'],'ProgramExePath')]/*[local-name()='value']"))
				{
					var exePath = Path.Combine(localDirectory, node.InnerText);
					executableFiles.Remove(exePath);
					CollectionAssert.DoesNotContain(exclusionsList, exePath, $"Please remove {exePath} from the exclusions list in this test");
				}
			}
			Assert.AreEqual(0, executableFiles.Count, $"The following executables do not have schedule configurations + {string.Join(Environment.NewLine, executableFiles)}");
		}

		[Test]
		public void AllJobsHaveGroup()
		{
			foreach (var file in schedulerConfigurationXmlFiles)
			{
				var doc = new XmlDocument();
				doc.Load(file);

				var jobNodes = doc.SelectNodes("//*[local-name()='job-scheduling-data']/*[local-name()='schedule']/*[local-name()='job']");
				foreach (XmlNode node in jobNodes)
				{
					var jobName = node.SelectSingleNode("*[local-name()='name']").InnerText;
					Assert.NotNull(node.SelectSingleNode("*[local-name()='group']"), $"{jobName} should have group node");
				}
			}
		}

		[Test]
		public void AllJobsHaveCorrectJobType()
		{
			foreach (var file in schedulerConfigurationXmlFiles)
			{
				var doc = new XmlDocument();
				doc.Load(file);

				var jobNodes = doc.SelectNodes("//*[local-name()='job-scheduling-data']/*[local-name()='schedule']/*[local-name()='job']");
				foreach (XmlNode node in jobNodes)
				{
					var jobName = node.SelectSingleNode("*[local-name()='name']").InnerText;
					var jobType = node.SelectSingleNode("*[local-name()='job-type']").InnerText.Split(',');
					Assert.AreEqual(2, jobType.Length);

					var assembly = Assembly.Load(jobType[1]);
					Assert.NotNull(assembly, $"Assembly: {jobType[1]} defined in {jobName} could not be found");
					var type = assembly.GetType($"{jobType[0]}");
					Assert.NotNull(type, $"Type: {jobType[0]} defined in {jobName} could not be found");
				}
			}
		}

		[Test]
		public void CheckDuplicatesOfProgramExePathAndProgramArgs()
		{
			Assert.Multiple(() =>
			{
				var programProps = new List<string>();
				foreach (var file in schedulerConfigurationXmlFiles)
				{
					if (Path.GetFileName(file) == "QuartzDataUpdaterJobs.xml")
					{
						continue;
					}
					var doc = new XmlDocument();
					doc.Load(file);
					var jobNodes = doc.SelectNodes("//*[local-name()='job-scheduling-data']/*[local-name()='schedule']/*[local-name()='job']");
					foreach (XmlNode node in jobNodes)
					{
						var programExePathNode = node.SelectSingleNode("*[local-name()='job-data-map']/*[local-name()='entry'] [contains( ./*[local-name()='key'],'ProgramExePath')]/*[local-name()='value']");
						Assert.NotNull(programExePathNode);
						var programExePath = programExePathNode.InnerText;

						var programArgsNode = node.SelectSingleNode("*[local-name()='job-data-map']/*[local-name()='entry'] [contains( ./*[local-name()='key'],'ProgramArgs')]/*[local-name()='value']");
						var programArgs = programArgsNode?.InnerText ?? "";
						programProps.Add($"{programExePath} {programArgs}");
					}
				}
				var deduplication = programProps.Distinct().ToList();
				Assert.AreEqual(0, programProps.Count - deduplication.Count);
			});
		}

		[Test]
		public void XmlIsValid()
		{
			var settings = new XmlReaderSettings();
			settings.ValidationType = ValidationType.Schema;
			settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
			settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
			settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
			settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

			var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Configuration");
			settings.Schemas.Add("http://quartznet.sourceforge.net/JobSchedulingData", Path.Combine(path, "job_scheduling_data_2_0.xsd"));

			// Create the XmlReader object.
			using (var reader = XmlReader.Create(Path.Combine(path, "QuartzProcessorRunnerJobs.xml"), settings))
			{
				// Parse the file. 
				while (reader.Read())
				{ }
			}
		}

		static void ValidationCallBack(object sender, ValidationEventArgs args)
		{
			if (args.Severity == XmlSeverityType.Warning)
			{
				Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
			}
			else
			{
				Assert.Fail("\tValidation error: " + args.Message);
			}
		}

		[SetUp]
		public void Setup()
		{
			localDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
			schedulerConfigurationXmlFiles = Directory.GetFiles(Path.Combine(localDirectory, @"Configuration"), "*.xml");
		}
		string localDirectory;
		string[] schedulerConfigurationXmlFiles;
	}
}
