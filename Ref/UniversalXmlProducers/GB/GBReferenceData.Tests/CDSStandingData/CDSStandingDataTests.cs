using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.Common.CommonHelpers;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData
{
	[TestFixture]
	abstract class CDSStandingDataTests
	{
		[Test]
		public void ParserTest()
		{
			var wrapper = new ManifestClientWrapper
			{
				TestDate = new DateTime(2022, 2, 11, 12, 13, 14)
			};

			using (var expectedTestStream = assembly.GetManifestResourceStream($"CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Output.RefCusCodeListZZ_CDS_{CodeListID}.xml"))
			{
				RunParser(wrapper);
				using (var resultStream = new FileStream(Path.Combine(outputPath, $"GB_RefCusCodeListZZ_CDS_{CodeListID}.xml"), FileMode.Open))
				{
					using (TextReader trResult = new StreamReader(resultStream))
					using (TextReader trExpected = new StreamReader(expectedTestStream))
					{
						var result = trResult.ReadToEnd();
						var expected = trExpected.ReadToEnd();

						Assert.That(result, Is.EqualTo(expected));
					}
				}
			}
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"GB\CDSStandingData\");
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(outputPath))
			{
				Directory.Delete(outputPath, true);
			}
		}

		Assembly assembly;
		protected string outputPath;

		protected abstract string CodeListID { get; }

		protected abstract void RunParser(IWebClientWrapper wrapper);
	}
}
