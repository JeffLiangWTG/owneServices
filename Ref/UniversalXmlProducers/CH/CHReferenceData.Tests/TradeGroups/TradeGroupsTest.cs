using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.TradeGroups;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Services.TradeGroups;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.TradeGroups
{
	[TestFixture]
	class TradeGroupsTest
	{
		[Test]
		public void TestDownloadAndConvert()
		{
			using (var expectedTestStream = classType.GetTestStream("TestFiles.Output.edecCountryCodes_1_0_converted.xml"))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				using var edecCountryCodesZip = classType.GetZippedTestStream("TestFiles.Input.edecCountryCodes_1_0.xml");
				mockHttp.When(DownloadUrl).WithUserAgent().Respond("application/zip", edecCountryCodesZip);
				var client = mockHttp.ToHttpClient();
				var download = DownloadTradeGroups.DownloadAndUnzip(client);
				var parser = new TradeGroupsParser(download);
				using (var outputFile = new TemporaryOutputFile(@"TradeGroups\RefTradeGroupsZZ_CH.xml"))
				{
					parser.ConvertToRefXML(outputFile.FullPath, "CH Trade Groups", ActualTestDate);
					var message = File.ReadAllText(outputFile.FullPath);
					using (var converterResultStream = new FileStream(outputFile.FullPath, FileMode.Open))
					{
						var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(converterResultStream));
						var expectedXml = XDocument.Load(expectedTestStream);
						Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml));
					}
				}
			}
		}

		[Test]
		public void TestTradeGroupsParser()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.edecCountryCodes_1_0.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"TradeGroups\TestTradeGroupsParser.xml"))
			{
				new TradeGroupsParser(download).ConvertToRefXML(outputFile.FullPath, "CH Trade Groups", ActualTestDate);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				using (var expectedStream = classType.GetTestStream("TestFiles.Output.edecCountryCodes_1_0_converted.xml"))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));
					var expectedXml = XDocument.Load(expectedStream);
					Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml));
				}
			}
		}

		[Test]
		public void TestWriterConfiguration() => Assert.Multiple(() =>
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.edecCountryCodes_1_0.xml"),
			};

			using (var outputFile = new TemporaryOutputFile($@"TradeGroups\{nameof(TestWriterConfiguration)}.xml"))
			{
				new TradeGroupsParser(download).ConvertToRefXML(outputFile.FullPath, "CH Trade Groups", ActualTestDate);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));

					void assertIsKey(string entity, string property)
					{
						Assert.That(actualXml.XPathSelectElement($@"//EntityType[@Name='{entity}']/Key/PropertyRef[@Name='{property}']"), Is.Not.Null, $"{entity}.{property} is key");
					}
					void assertIsNotKey(string entity, string property)
					{
						Assert.That(actualXml.XPathSelectElement($@"//EntityType[@Name='{entity}']/Key/PropertyRef[@Name='{property}']"), Is.Null, $"{entity}.{property} is not key");
					}

					assertIsKey("RefCusTradeGroup", "ZZA_TradeGroup");
					assertIsKey("RefCusTradeGroup", "ZZA_ZZZ_NKDataGrouping");
					assertIsNotKey("RefCusTradeGroup", "ZZA_StartDate");
					assertIsNotKey("RefCusTradeGroup", "ZZA_EndDate");
					assertIsKey("RefCusTradeGroupCountry", "ZZB_RN_NKTradeGroupCountryCode");
					assertIsNotKey("RefCusTradeGroupCountry", "ZZB_StartDate");
					assertIsNotKey("RefCusTradeGroupCountry", "ZZB_EndDate");
				}
			}
		});

		[Test]
		public void TestDataForTodayAndInTheFutureAndInThePastIsImported()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.TestDataForTodayAndInTheFutureAndInThePastIsImported.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"TradeGroups\TestTradeGroupsParser.xml"))
			{
				new TradeGroupsParser(download).ConvertToRefXML(outputFile.FullPath, "CH Trade Groups", ActualTestDate);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));

					void assertAssignment(string group, string country, params DateRange[] expectedAssignmentDateRanges)
					{
						var assignments = actualXml.XPathSelectElements($"//RefCusTradeGroup[ZZA_TradeGroup='{group}']/RefCusTradeGroupCountry[ZZB_RN_NKTradeGroupCountryCode='{country}']");
						var assignmentsMessage = "Found assignments: " + string.Join(", ", assignments.Select(a => new DateRange(a)));

						var missingAssignmentDateRanges = new List<DateRange>();
						missingAssignmentDateRanges.AddRange(expectedAssignmentDateRanges);
						foreach (var assignment in assignments)
						{
							missingAssignmentDateRanges.Remove(new DateRange(assignment));
						}
						Assert.That(missingAssignmentDateRanges.Count, Is.Zero, $"Group {group} Country {country}:\nMissing assignments for dates: {string.Join(" ", missingAssignmentDateRanges)}\n{assignmentsMessage}");

						var unexpectedAssignmentDateRanges = (from a in assignments select new DateRange(a)).ToList();
						foreach (var expectedAssignmentDateRange in expectedAssignmentDateRanges)
						{
							unexpectedAssignmentDateRanges.Remove(expectedAssignmentDateRange);
						}
						Assert.That(unexpectedAssignmentDateRanges.Count, Is.Zero, $"Group {group} Country {country}:\nUnexpected assignments for dates: {string.Join(" ", unexpectedAssignmentDateRanges)}\n{assignmentsMessage}");
					}

					Assert.Multiple(() =>
					{
						assertAssignment("1", "C1", new DateRange(2020, 2022));
						assertAssignment("1", "C2", new DateRange(2020, 2022));
						assertAssignment("1", "C3", new DateRange(2020, 2021));
						assertAssignment("1", "C4", new DateRange(2021, 2022));

						assertAssignment("2", "C5", new DateRange(2020, 2020));
						assertAssignment("3", "C5", new DateRange(2020, 2021));
						assertAssignment("4", "C5", new DateRange(2021, 2022));
						assertAssignment("5", "C5", new DateRange(2022, 2022));

						assertAssignment("1", "C6", new DateRange(2020, 2020));
						assertAssignment("1", "C7", new DateRange(2020, 2021));
						assertAssignment("1", "C8", new DateRange(2021, 2022));
						assertAssignment("1", "C9", new DateRange(2022, 2022));
					});
				}
			}
		}

		[Test]
		public void TestLatestCountryDescriptionIsImported()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.TestLatestCountryDescriptionIsImported.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"TradeGroups\TestLatestCountryDescriptionIsImported.xml"))
			{
				new TradeGroupsParser(download).ConvertToRefXML(outputFile.FullPath, "CH Trade Groups", ActualTestDate);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));

					void assertDescription(string group, string country, string expectedDescriptiopn)
					{
						var assignments = actualXml.XPathSelectElements($"//RefCusTradeGroup[ZZA_TradeGroup='{group}']/RefCusTradeGroupCountry[ZZB_RN_NKTradeGroupCountryCode='{country}']");
						Assert.That(assignments.Count, Is.EqualTo(1), "Only one RefCusTradeGroupCountry expected");
						Assert.That(assignments.First().Element("ZZB_Description")?.Value, Is.EqualTo(expectedDescriptiopn));
						Assert.That(assignments.First().Element("ZZB_Description")?.Value, Is.EqualTo(expectedDescriptiopn));
					}

					Assert.Multiple(() =>
					{
						assertDescription("1", "C1", "Country 1c");
					});
				}
			}
		}

		[Test]
		public void TestTradeGroupsInTheFarFutureAreIgnored() => Assert.Multiple(() =>
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.TestTradeGroupsInTheFarFutureAreIgnored.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"TradeGroups\TestTradeGroupsInTheFarFutureAreIgnored.xml"))
			{
				new TradeGroupsParser(download).ConvertToRefXML(outputFile.FullPath, "CH Trade Groups", ActualTestDate);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));

					Assert.That(actualXml.XPathSelectElements($"//RefCusTradeGroup[ZZA_TradeGroup='1']").Count, Is.EqualTo(1), "only one group is converted");
					Assert.That(actualXml.XPathSelectElement($"//RefCusTradeGroup[ZZA_TradeGroup='1']/ZZA_Description")?.Value, Is.EqualTo("Group 1 actual"), "actual group is converted");
				}
			}
		});

		[Test]
		public void TestDateTruncation()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.TestDateTruncation.xml"),
			};
			using (var outputFile = new TemporaryOutputFile(@"TradeGroups\TestTradeGroupsParser.xml"))
			{
				new TradeGroupsParser(download).ConvertToRefXML(outputFile.FullPath, "CH Trade Groups", ActualTestDate);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));
					;

					Assert.Multiple(() =>
					{
						Assert.That(actualXml.XPathSelectElement($"//RefCusTradeGroup[ZZA_TradeGroup='1']/ZZA_EndDate")?.Value, Is.EqualTo("2079-06-06T23:59:00"), "1: endDate after valid range");
						Assert.That(actualXml.XPathSelectElement($"//RefCusTradeGroup[ZZA_TradeGroup='2']/ZZA_StartDate")?.Value, Is.EqualTo("2079-06-05T00:00:00"), "2: startDate near end of valid range");
						Assert.That(actualXml.XPathSelectElement($"//RefCusTradeGroup[ZZA_TradeGroup='2']/ZZA_EndDate")?.Value, Is.EqualTo("2079-06-05T23:59:00"), "2: endDate near end of valid range");
					});
				}
			}
		}

		[Test]
		public void TestOverlappingDateRangesAreCombined()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.TestOverlappingDateRangesAreCombined.xml"),
			};
			using (var outputFile = new TemporaryOutputFile(@"TradeGroups\TestOverlappingDateRangesAreNotImported.xml"))
			{
				new TradeGroupsParser(download).ConvertToRefXML(outputFile.FullPath, "CH Trade Groups", ActualTestDate);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));
					;

					Assert.Multiple(() =>
					{
						Assert.That(actualXml.XPathSelectElement($"//RefCusTradeGroup[ZZA_TradeGroup='1']/RefCusTradeGroupCountry[ZZB_Description='C1 older']"), Is.Null, "C1 older");
						Assert.That(actualXml.XPathSelectElement($"//RefCusTradeGroup[ZZA_TradeGroup='1']/RefCusTradeGroupCountry[ZZB_Description='C1 newer']"), Is.Not.Null, "C1 newer");

						Assert.That(actualXml.XPathSelectElements($"//RefCusTradeGroup[ZZA_TradeGroup='1']/RefCusTradeGroupCountry[ZZB_RN_NKTradeGroupCountryCode='C2']").Count(), Is.EqualTo(1), "C2 only one");
						Assert.That(actualXml.XPathSelectElement($"//RefCusTradeGroup[ZZA_TradeGroup='1']/RefCusTradeGroupCountry[ZZB_RN_NKTradeGroupCountryCode='C2']/ZZB_StartDate")?.Value, Is.EqualTo("2020-01-01T00:00:00"), "C2 lower StartDate");
						Assert.That(actualXml.XPathSelectElement($"//RefCusTradeGroup[ZZA_TradeGroup='1']/RefCusTradeGroupCountry[ZZB_RN_NKTradeGroupCountryCode='C2']/ZZB_EndDate")?.Value, Is.EqualTo("2022-12-31T23:59:00"), "C2 higher EndDate");
					});
				}
			}
		}

		struct DateRange
		{
			internal readonly DateTime StartDate;
			internal readonly DateTime EndDate;

			internal DateRange(int startYear, int endYear)
			{
				StartDate = new DateTime(startYear, 1, 1, 0, 0, 0);
				EndDate = new DateTime(endYear, 12, 31, 23, 59, 0);
			}

			internal DateRange(XElement refCusTradeGroupCountry)
			{
				StartDate = DateTime.Parse(refCusTradeGroupCountry.Element("ZZB_StartDate").Value, CultureInfo.InvariantCulture);
				EndDate = DateTime.Parse(refCusTradeGroupCountry.Element("ZZB_EndDate").Value, CultureInfo.InvariantCulture);
			}

			public override string ToString()
			{
				return $"[{StartDate}, {EndDate}]";
			}
		}

		Type classType => GetType();

		const string DownloadUrl = "https://edec.douane.swiss/data/edecCountryCodes_1_0.zip";
		readonly DateTime ActualTestDate = new DateTime(2021, 7, 1);
	}
}
