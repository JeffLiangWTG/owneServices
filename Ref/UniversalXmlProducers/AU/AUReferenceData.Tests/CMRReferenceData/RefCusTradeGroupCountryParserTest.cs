using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class RefCusTradeGroupCountryParserTest
	{
		string TestFileFolderName => "RefCusTradeGroupCountry";

		string TextFileName => "PRSPCTRY-P1-EDMAIN-2306090141.txt";

		Dictionary<string, List<RefCusTradeGroupCountry>> ExpectedOutput
		{
			get
			{
				return new Dictionary<string, List<RefCusTradeGroupCountry>>
				{
					{ "AANZ", new List<RefCusTradeGroupCountry>(){ new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "BN", ZZB_StartDate = new DateTime(2021, 11, 04) }, new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "ID", ZZB_StartDate = new DateTime(2021, 11, 04) } }},
					{ "CA", new List<RefCusTradeGroupCountry>(){ new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "CA", ZZB_StartDate = new DateTime(2021, 09, 07) } }},
					{ "DC", new List<RefCusTradeGroupCountry>(){ new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "AF", ZZB_StartDate = new DateTime(2021, 09, 07) }, new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "AO", ZZB_StartDate = new DateTime(2021, 09, 07) }, new RefCusTradeGroupCountry() { ZZB_RN_NKTradeGroupCountryCode = "AS", ZZB_StartDate = new DateTime(2021, 09, 07) } } },
				};
			}
		}

		[Test]
		public void TestParser()
		{
			var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles", TestFileFolderName);
			using (var txtStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, TextFileName)))
			using (var txtReader = new StreamReader(txtStream))
			{
				var actualOutput = RefCusTradeGroupCountryParser.Parse(txtReader.ReadToEnd());
				Assert.AreEqual(ExpectedOutput.Count, actualOutput.Count, "Both dictionaries contain the same number of elements");
				Assert.IsTrue(actualOutput.All(x => ExpectedOutput[x.Key].Count.Equals(x.Value.Count)), "Both dictionaries contain the same elements");
			}
		}

		[SetUp]
		public void Setup()
		{
			executingAssembly = Assembly.GetExecutingAssembly();
		}
		Assembly executingAssembly;
	}
}
