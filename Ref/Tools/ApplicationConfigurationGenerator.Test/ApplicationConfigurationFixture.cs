using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ApplicationConfigurationGenerator.Test
{
	[TestFixture]
	public class ApplicationConfigurationFixture
	{
		[Test]
		public async Task CheckXmlOutputForAppConfigAndConfigJsonConfigurations()
		{
			var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var testFolder = Path.Combine(binFolder, "AppConfigTest");
			var xmlFilePath = Path.Combine(testFolder, "ApplicationPathsAndDefaultConfigurations.xml");
			if (File.Exists(xmlFilePath))
			{
				File.Delete(xmlFilePath);
			}

			var applicationConfigurationGen = new ApplicationConfigurationGenerator(testFolder, testFolder);
			await applicationConfigurationGen.Run();
			Assert.That(File.Exists(xmlFilePath));

			var xmlContent = File.ReadAllText(xmlFilePath);
			Assert.That(xmlContent, Does.Contain(@"
  <ApplicationConfiguration>
    <ApplicationPath>..\TestingApplication\CargoWise.RefDbRepo.TestingApplication.exe</ApplicationPath>
    <ApplicationConfigFileName>CargoWise.RefDbRepo.TestingApplication.exe.config</ApplicationConfigFileName>
    <ApplicationJobGroup>Test Group 1</ApplicationJobGroup>
    <ConfigData>
      <KeyData>
        <Name>Test1</Name>
        <Value>TestingNumber1</Value>
      </KeyData>
      <KeyData>
        <Name>Test2</Name>
        <Value>TestingNumber2</Value>
      </KeyData>
      <KeyData>
        <Name>Test3</Name>
        <Value>TestingNumber3</Value>
      </KeyData>
      <KeyData>
        <Name>TestEmpty</Name>
        <Value />
      </KeyData>
      <KeyData>
        <Name>Key</Name>
        <Value>true</Value>
      </KeyData>
      <KeyData>
        <Name>IssueReportingExitCode</Name>
        <Value>7</Value>
      </KeyData>
    </ConfigData>
  </ApplicationConfiguration>"));

			Assert.That(xmlContent, Does.Contain(@"
  <ApplicationConfiguration>
    <ApplicationPath>..\TestingApplication\CargoWise.RefDbRepo.TestingApplicationJson.exe</ApplicationPath>
    <ApplicationConfigFileName>CargoWise.RefDbRepo.TestingApplicationJson.config.json</ApplicationConfigFileName>
    <ApplicationJobGroup>Test Group 2</ApplicationJobGroup>
    <ConfigData>
      <KeyData>
        <Name>Test1Json</Name>
        <Value>TestingNumber1</Value>
      </KeyData>
      <KeyData>
        <Name>Test2Json</Name>
        <Value>TestingNumber2</Value>
      </KeyData>
      <KeyData>
        <Name>Test3Json</Name>
        <Value>TestingNumber3</Value>
      </KeyData>
      <KeyData>
        <Name>TestEmptyJson</Name>
        <Value />
      </KeyData>
      <KeyData>
        <Name>Key</Name>
        <Value>True</Value>
      </KeyData>
      <KeyData>
        <Name>IssueReportingExitCode</Name>
        <Value>7</Value>
      </KeyData>
    </ConfigData>
  </ApplicationConfiguration>"));
		}
	}
}
