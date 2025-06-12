using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ESCompliance.Transforms.UniTrans2SII.Tests.Transforms.UniTrans2SII
{
    [TestClass]
    public class MapUniTrans2SIIFixtures
    {
        #region Member Variables

	    readonly Dictionary<string, string> euCountries = new Dictionary<string, string>
	    {
		    { "SE", "Swede" }, 
		    { "UK", "United Kingdom" }, 
		    { "DE", "Germany" }, 
		    { "BE", "Belgium" }, 
		    { "FI", "Finland" }, 
		    { "MT", "Malta" }, 
		    { "HR", "Croatia" }, 
		    { "CZ", "Czech Republic" }, 
		    { "LT", "Lithuania" }, 
		    { "CY", "Cyprus" }, 
		    { "HU", "Hungary" }, 
		    { "FR", "France" }, 
		    { "SI", "Slovenia" }, 
		    { "ES", "Spain" }, 
		    { "EL", "Greece" }, 
		    { "RO", "Romania" }, 
		    { "PL", "Poland" }, 
		    { "SK", "Slovakia" }, 
		    { "LU", "Luxembourg" }, 
		    { "LV", "Latvia" }, 
		    { "IE", "Ireland" }, 
		    { "AT", "Austria" }, 
		    { "PT", "Portugal" }, 
		    { "EE", "Estonia" }, 
		    { "NL", "Netherlands" }, 
		    { "BG", "Bulgaria" }, 
		    { "DK", "Denmark" }, 
		    { "IT", "Italy" }		
	    };

	    readonly Dictionary<string, string> testInputOutputFiles = new Dictionary<string, string>();

		const string TestFilesNamespace = "TestFiles";
	    const string SubscriptionType = "ESCMSG";
		const string SenderId = "HYEDUSUAT";
		const string RecipientId = "ESCompliance";

	    MapTester mapTester;
	    CodeMapper mockCodeMapper;
	    ContextAccessor mockContextAccessor;
	    DataModelAccessor mockDataModelAccessor;

        #endregion

        [TestInitialize]
        public void Setup()
        {
	        mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
	        mockDataModelAccessor = MockRepository.GenerateMock<DataModelAccessor>();

			foreach (var euCountry in euCountries)
			{
			    var country = euCountry;
				mockCodeMapper.Expect(x => x.GetRecipientCode("ESCompliance", "ESCompliance", "CW1-to-SII transformation", "EUCountryCode", "CountryName", country.Key)).Repeat.Any().Return(euCountry.Value);
			}

	        mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty",
				"http://schemas.microsoft.com/BizTalk/2003/system-properties")).Repeat.Any().Return(RecipientId);
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty",
				"http://schemas.microsoft.com/BizTalk/2003/system-properties")).Repeat.Any().Return(SenderId);

	        var extensionObjects = new Dictionary<string, object>
			{ 
		        { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", new DateMapper() },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS4", mockDataModelAccessor }
	        };

	        mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

	        SetupTestFiles();
        }

	    void SetupTestFiles()
	    {
		    const string testFileExtension = ".xml";
			const string inputFileEnding = "_input.xml";
			const string outputFileEnding = "_output.xml";

		    var executingAssembly = Assembly.GetExecutingAssembly();
		    var folderName = string.Format("{0}.TestFiles.", executingAssembly.GetName().Name);
		    var testFiles = executingAssembly
			    .GetManifestResourceNames()
				.Where(r => r.StartsWith(folderName) && r.EndsWith(testFileExtension))
				.ToList();

			var inputFiles = testFiles.Where(x => x.EndsWith(inputFileEnding)).ToList();
			var outputFiles = testFiles.Where(x => x.EndsWith(outputFileEnding)).ToList();

		    testInputOutputFiles.Clear();
		    foreach (var inputFile in inputFiles)
		    {
				var match = Regex.Match(inputFile, folderName + "(?<fileName>.*)" + inputFileEnding);
			    if (match.Success)
			    {
					var fileName = match.Groups["fileName"].ToString();
					var outputFile = folderName + fileName + outputFileEnding;
				    if (outputFiles.Contains(outputFile))
				    {
						testInputOutputFiles.Add(fileName + inputFileEnding, fileName + outputFileEnding);
				    }
			    }			
		    }
	    }

	    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestMapUniTrans2SII()
        {
	        foreach (var inputOutputFilesPair in testInputOutputFiles)
	        {
		        var inputFileName = inputOutputFilesPair.Key;
		        var outputFileName = inputOutputFilesPair.Value;
		        var sourceFile = TestFilesNamespace + "." + inputFileName;
				var expectedOutputFile = TestFilesNamespace + "." + outputFileName;
		        mapTester.ExecuteCompiled<MapUniTrans2SII>(sourceFile, expectedOutputFile);

				// mapTester.ExecuteCompiledWithXslDebug<MapUniTrans2SII>(sourceFile, expectedOutputFile, @"C:\eServices\eHub\Products\ESCompliance\CargoWise.eHub.Products.ESCompliance.Transforms.UniTrans2SII\MapUniTrans2SII.xsl");
	        }

	        mockCodeMapper.VerifyAllExpectations();
	        mockContextAccessor.VerifyAllExpectations();
	        mockDataModelAccessor.VerifyAllExpectations();
        }

	    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
	    public void TestGetRecipientCode()
	    {
		    foreach (var euCountry in euCountries)
		    {
			    var country = euCountry;
				var countryName = mockCodeMapper.GetRecipientCode("ESCompliance", "ESCompliance", "CW1-to-SII transformation", "EUCountryCode", "CountryName", country.Key);
				Assert.AreEqual(country.Value, countryName);
		    }
	    }
    }
}
