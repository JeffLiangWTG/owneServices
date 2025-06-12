#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using eServices.BuildTools.SqlAgentTool;
using Moq;
using NUnit.Framework;

namespace eServices.BuildTools.Tests.SqlAgentTool;

internal class SqlAgentToolTests
{
	[Test]
	public async Task JobsXmlTest()
	{
		var jobsXmlFile = new FileInfo(Path.GetTempFileName());
		try
		{
			var sqlJobXml = typeof(SqlAgentToolTests).GetResourceAsString("SqlJobsConfig.xml", null);
			var serverJobService = new Mock<ServerJobsService>(null, jobsXmlFile, new List<string>()) { CallBase = true };
			serverJobService.Setup(x => x.GetSqlXml()).ReturnsAsync(sqlJobXml);

			await serverJobService.Object.SaveXml();

			var expected = typeof(SqlAgentToolTests).GetResourceAsString("ExpectedConfig.xml");
			var actual = ReadFileText(jobsXmlFile);
			Assert.That(actual, Is.EqualTo(expected));
		}
		finally
		{
			jobsXmlFile?.Delete();
		}
	}

	[Test]
	public async Task ExportTest()
	{
		var sourceDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"{nameof(SqlAgentToolTests)}.{nameof(ExportTest)}.{DateTime.Now.ToBinary()}"));
		try
		{
			var sqlJobXml = typeof(SqlAgentToolTests).GetResourceAsString("SqlJobsConfig.xml", null);
			var serverJobService = new Mock<ServerJobsService>(null, null, new List<string>()) { CallBase = true };
			serverJobService.Setup(x => x.GetSqlXml()).ReturnsAsync(sqlJobXml);
			var sourceJobService = new SourceJobsService(sourceDir, []);

			var commandHandler = new CommandHandlerService(serverJobService.Object, sourceJobService);
			await commandHandler.Export("Default", false);

			var expected = typeof(SqlAgentToolTests).GetResourceAsString("ExpectedSource.xml");
			var actual = ReadJobsFolder(sourceDir);
			Assert.That(actual, Is.EqualTo(expected));
		}
		finally
		{
			sourceDir?.Delete(true);
		}
	}

	[Test]
	public async Task ExportJobsTest()
	{
		var sourceDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"{nameof(SqlAgentToolTests)}.{nameof(ExportJobsTest)}.{DateTime.Now.ToBinary()}"));
		try
		{
			var serverXml = typeof(SqlAgentToolTests).GetResourceAsString("InputServer.xml");
			var sourceXml = typeof(SqlAgentToolTests).GetResourceAsString("InputSource.xml");
			WriteJobsFolder(sourceXml, sourceDir);
			List<string> jobs = ["Backup BizTalk Server (BizTalkMgmtDb)", "CommandLogCleanup"];
			var serverJobService = new Mock<ServerJobsService>(null, null, new List<string>()) { CallBase = true };
			serverJobService.Setup(x => x.GetSqlXml()).ReturnsAsync(serverXml);
			var sourceJobService = new SourceJobsService(sourceDir, []);

			var commandHandler = new CommandHandlerService(serverJobService.Object, sourceJobService);
			await commandHandler.Export("Staging", false);

			var expected = typeof(SqlAgentToolTests).GetResourceAsString("ExpectedSource.xml");
			var actual = ReadJobsFolder(sourceDir);
			Assert.That(actual, Is.EqualTo(expected));
		}
		finally
		{
			sourceDir?.Delete(true);
		}
	}

	[Test]
	public async Task ScriptTest()
	{
		var sourceDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"{nameof(SqlAgentToolTests)}.{nameof(ScriptTest)}.{DateTime.Now.ToBinary()}"));
		var outputFile = new FileInfo(Path.GetTempFileName());
		try
		{
			var sqlJobXml = typeof(SqlAgentToolTests).GetResourceAsString("SqlJobsConfig.xml", null);
			var serverXml = typeof(SqlAgentToolTests).GetResourceAsString("InputServer.xml");
			var serverJobService = new Mock<ServerJobsService>(null, null, new List<string>(), "localhost") { CallBase = true };
			serverJobService.SetupSequence(x => x.GetSqlXml())
				.ReturnsAsync(sqlJobXml)
				.ReturnsAsync(serverXml);
			var sourceJobService = new SourceJobsService(sourceDir, []);

			var commandHandler = new CommandHandlerService(serverJobService.Object, sourceJobService);
			await commandHandler.Export("Default", false);
			await commandHandler.Script("Default", outputFile, false);

			var expected = typeof(SqlAgentToolTests).GetResourceAsString("ExpectedScript.sql");
			var actual = ReadFileText(outputFile);
			Assert.That(actual, Is.EqualTo(expected));
		}
		finally
		{
			sourceDir?.Delete(true);
			outputFile?.Delete();
		}
	}

	[Test]
	public async Task PublishTest()
	{
		var sourceDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"{nameof(SqlAgentToolTests)}.{nameof(PublishTest)}.{DateTime.Now.ToBinary()}"));
		try
		{
			var serverXml = typeof(SqlAgentToolTests).GetResourceAsString("InputServer.xml");
			var sourceXml = typeof(SqlAgentToolTests).GetResourceAsString("InputSource.xml");
			WriteJobsFolder(sourceXml, sourceDir);
			List<string> jobs = ["Backup BizTalk Server (BizTalkMgmtDb)", "CommandLogCleanup"];
			var serverJobService = new Mock<ServerJobsService>(null, null, jobs, "localhost") { CallBase = true };
			serverJobService.Setup(x => x.GetSqlXml()).ReturnsAsync(serverXml);
			serverJobService.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<string>())).Returns(Task.FromResult(1));
			var sourceJobService = new SourceJobsService(sourceDir, jobs);

			var commandHandler = new CommandHandlerService(serverJobService.Object, sourceJobService);
			await commandHandler.Publish("Staging", false);

			var expected = typeof(SqlAgentToolTests).GetResourceAsString("ExpectedScript.sql");
			serverJobService.Verify(x => x.ExecuteNonQueryAsync(expected));
		}
		finally
		{
			sourceDir?.Delete(true);
		}
	}

	static string ReadJobsFolder(DirectoryInfo dir)
	{
		return new XElement("Source", dir.EnumerateDirectories().Select(jobDir =>
			new XElement("Folder", new XAttribute("Name", jobDir.Name), jobDir.EnumerateFiles().Select(file =>
				new XElement("File", new XAttribute("Name", file.Name), new XCData(ReadFileText(file))))))).ToString();
	}

	static void WriteJobsFolder(string source, DirectoryInfo dir)
	{
		var sourceXml = XElement.Parse(source);
		foreach (var folder in sourceXml.Elements())
		{
			var jobDir = dir.CreateSubdirectory(folder.Attribute("Name").Value);
			foreach (var file in folder.Elements())
			{
				File.WriteAllText(Path.Combine(jobDir.FullName, file.Attribute("Name").Value), file.Value);
			}
		}
	}

	static string ReadFileText(FileInfo file)
	{
		using var rdr = file.OpenText();
		return rdr.ReadToEnd();
	}
}
#endif