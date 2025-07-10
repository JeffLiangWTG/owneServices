using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class DispatchInstructionDocumentsParserTest
	{
		[TestCase(true)]
		[TestCase(false)]
		public void TestExportXml(bool isProd)
		{
			using (var expectedStreamProfile = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.RefCusProfile_BR_{ProfileType(isProd)}.xml"))
			using (var expectedStreamType = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.RefCusProfileType_BR_{ProfileType(isProd)}.xml"))
			using (var expectedStreamQuestion = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.RefCusProfileQuestion_BR_{ProfileType(isProd)}.xml"))
			using (var expectedStreamTariff = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.RefCusTariff_BR_{TariffType(isProd)}.xml"))
			using (var expectedStreamTariffType = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.RefCusTariffType_BR_{TariffType(isProd)}.xml"))
			using (var inputStream = Utils.GetManifestResourceStream($"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Input.DocumentType_result.txt"))
			{
				var parser = new DispatchInstructionDocumentsParser($"BR RefCusProfile {ProfileType(isProd)}", isProd);
				parser.OverridePublicationDate(new DateTime(2021, 11, 26, 00, 00, 00));
				parser.ExportToXMLFile(inputStream.JsonStreamToObject<IEnumerable<DossierDataDTO>>(),
					outputFileName: TestOutputProfilePath(isProd),
					outputTypeFileName: TestOutputProfileTypePath(isProd),
					outputQuestionFileName: TestOutputProfileQuestionPath(isProd),
					outputTariffFileName: TestOutputTariffPath(isProd),
					outputTariffTypeFileName: TestOutputTariffTypePath(isProd)
				);

				using (var outputStreamProfile = new FileStream(TestOutputProfilePath(isProd), FileMode.Open))
				using (var outputStreamType = new FileStream(TestOutputProfileTypePath(isProd), FileMode.Open))
				using (var outputStreamQuestion = new FileStream(TestOutputProfileQuestionPath(isProd), FileMode.Open))
				using (var outputStreamTariff = new FileStream(TestOutputTariffPath(isProd), FileMode.Open))
				using (var outputStreamTariffType = new FileStream(TestOutputTariffTypePath(isProd), FileMode.Open))
				{
					StreamCompareHelper.CompareStreamContent(expectedStreamProfile, outputStreamProfile);
					StreamCompareHelper.CompareStreamContent(expectedStreamType, outputStreamType);
					StreamCompareHelper.CompareStreamContent(expectedStreamQuestion, outputStreamQuestion);
					StreamCompareHelper.CompareStreamContent(expectedStreamTariff, outputStreamTariff);
					StreamCompareHelper.CompareStreamContent(expectedStreamTariffType, outputStreamTariffType);
				}
			}
		}

		[TearDown]
		public void TestCleanup()
		{
			File.Delete(TestOutputProfilePath(true));
			File.Delete(TestOutputProfileTypePath(true));
			File.Delete(TestOutputProfileQuestionPath(true));
			File.Delete(TestOutputTariffPath(true));
			File.Delete(TestOutputTariffTypePath(true));

			File.Delete(TestOutputProfilePath(false));
			File.Delete(TestOutputProfileTypePath(false));
			File.Delete(TestOutputProfileQuestionPath(false));
			File.Delete(TestOutputTariffPath(false));
			File.Delete(TestOutputTariffTypePath(false));
		}
		string ProfileType(bool isProd) => isProd ? Constants.ProfileTypes.Codes.DispatchInstructionDocument : Constants.ProfileTypes.Codes.DispatchInstructionDocumentTest;

		string TariffType(bool isProd) => isProd ? Constants.TariffTypes.Codes.EADOC : Constants.TariffTypes.Codes.EADTE;

		string TestOutputProfilePath(bool isTest) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"CargoWise.RefDbRepo.BRReferenceData.Tests.Profiles.TestFiles.Output.Temp.RefCusProfile_BR_{ProfileType(isTest)}.xml");

		string TestOutputProfileTypePath(bool isTest) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusProfileType_BR_{ProfileType(isTest)}.xml");

		string TestOutputProfileQuestionPath(bool isTest) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusProfileQuestion_BR_{ProfileType(isTest)}.xml");

		string TestOutputTariffPath(bool isTest) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariff_BR_{TariffType(isTest)}.xml");

		string TestOutputTariffTypePath(bool isTest) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"CargoWise.RefDbRepo.BRReferenceData.Tests.Tariffs.TestFiles.Output.Temp.RefCusTariffType_BR_{TariffType(isTest)}.xml");
	}
}
