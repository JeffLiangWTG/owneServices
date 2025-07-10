using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.RefDbRepo.ComplianceListReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Test
{
	[TestFixture]
	public class ComplianceListParserTestFixture
	{
		string ExportFilePath => GetTestFilePath("ComplianceListParserTestOutput.xml");

		[Test]
		public void ComplianceListParserTest()
		{
			var publicationDate = new DateTime(2020, 1, 1);
			var expectedFilePath = GetTestFilePath("ComplianceListParserTestExpected.xml");

			var parser = new ComplianceListParser(TestSourceList(), ExportFilePath, publicationDate);
			parser.ExportXml();

			var expectedDoc = new XmlDocument();
			expectedDoc.Load(expectedFilePath);
			var resultDoc = new XmlDocument();
			resultDoc.Load(ExportFilePath);
			Assert.AreEqual(expectedDoc.InnerXml, resultDoc.InnerXml);
		}

		public static List<RefComplianceListResponse> TestSourceList()
		{
			var sourceList = new List<RefComplianceListResponse>();
			sourceList.Add(new RefComplianceListResponse
			{
				RCL_ListName = "Most Wanted",
				RCL_ListCode = "US-TEST1",
				RCL_IsActive = true,
				RCL_ListPublisher = "National Police Agency",
				RCL_PublisherDescription = "The American national Police force",
				RCL_ListDescription = "The NPA's Most Wanted List",
				RCL_ListType = "Law Enforcement",
				RCL_PublisherJurisdiction = "United States",
				RCL_MainSourceURL = "www.mainsource.com/LE",
				RCL_SecondarySourceURL = "www.secondarysource.com/LE",
				RCL_IntegrationDate = new DateTime(2020, 1, 1),
				RCL_LastUpdatedDate = new DateTime(2021, 1, 1)
			});
			sourceList.Add(new RefComplianceListResponse
			{
				RCL_ListName = "Current Sanctions",
				RCL_ListCode = "CA-TEST2",
				RCL_IsActive = false,
				RCL_ListPublisher = "Department of Foreign Affairs",
				RCL_PublisherDescription = "Manages Canada's diplomatic and consular relations",
				RCL_ListDescription = "List of countries Canada has imposed sanctions against.",
				RCL_ListType = "Financial Sanctions",
				RCL_PublisherJurisdiction = "Canada",
				RCL_MainSourceURL = "www.mainsource.com/ImposedSanctions",
				RCL_SecondarySourceURL = "www.secondarysource.com/ImposedSanctions",
				RCL_IntegrationDate = new DateTime(2020, 2, 1),
				RCL_LastUpdatedDate = new DateTime(2021, 2, 1)
			});
			sourceList.Add(new RefComplianceListResponse
			{
				RCL_ListName = "Embargoes",
				RCL_ListCode = "AU-TEST3",
				RCL_IsActive = true,
				RCL_ListPublisher = "Department of Trade",
				RCL_PublisherDescription = "Aims at advancing the interests of Australia and Australians internationally",
				RCL_ListDescription = "Embargoes issues by the Department of Trade",
				RCL_ListType = "Trade Restrictions",
				RCL_PublisherJurisdiction = "Australia",
				RCL_MainSourceURL = "www.mainsource.com/EmbargoesDoT",
				RCL_SecondarySourceURL = "www.secondarysource.com/EmbargoesDoT",
				RCL_IntegrationDate = new DateTime(2020, 3, 1),
				RCL_LastUpdatedDate = new DateTime(2021, 3, 1)
			});
			
			return sourceList;
		}

		string GetTestFilePath(string fileName)
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"TestFiles\{fileName}");
		}

		[TearDown]
		protected void TearDown()
		{
			if (File.Exists(ExportFilePath))
			{
				try
				{
					File.Delete(ExportFilePath);
				}
				catch
				{
				}
			}
		}
	}
}
