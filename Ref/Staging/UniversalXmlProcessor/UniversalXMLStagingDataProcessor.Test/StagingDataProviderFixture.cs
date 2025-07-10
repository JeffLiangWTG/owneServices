using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class StagingDataProviderFixture
	{
		[Test]
		public void GetNextDataBatch_WithFilter()
		{
			var source = new SourceData { SDA_PK = Guid.NewGuid() };
			var tariff1 = new RefCusTariff { ZZ1_PK = new Guid("2D1B356C-3BAA-403E-9563-51C8DE8A5A1F"), ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN" };
			var tariff2 = new RefCusTariff { ZZ1_PK = new Guid("737135A5-B8B5-4095-9D12-4D4205A1D910"), ZZ1_ZZI_NKTariffType = "EXP", ZZ1_ZZZ_NKDataGrouping = "EUN" };
			var tariff3 = new RefCusTariff { ZZ1_PK = new Guid("917663E9-A2ED-495B-8FD3-624345C460EE"), ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "ZA" };
			var info1 = new DataProcessingInformation { DPI_Status = "QUE", DPI_SourceId = source.SDA_PK, DPI_ParentPk = tariff1.ZZ1_PK, DPI_ParentTableCode = "ZZ1" };
			var info2 = new DataProcessingInformation { DPI_Status = "QUE", DPI_SourceId = source.SDA_PK, DPI_ParentPk = tariff2.ZZ1_PK, DPI_ParentTableCode = "ZZ1" };
			var info3 = new DataProcessingInformation { DPI_Status = "QUE", DPI_SourceId = source.SDA_PK, DPI_ParentPk = tariff3.ZZ1_PK, DPI_ParentTableCode = "ZZ1" };
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<DataProcessingInformation>()).Returns(new[] { info1, info2, info3 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff1, tariff2, tariff3 }.AsQueryable());
			using (var provider = new StagingDataProvider(repo.Object))
			{
				var metadata = new Mock<IMetadataProvider>();
				metadata.SetupGet(x => x.FilterData).Returns(true);
				metadata.Setup(x => x.GetConstantPropertyNamesAndValues("RefCusTariff")).Returns(new[]
				{
					Tuple.Create("ZZ1_ZZI_NKTariffType", "IMP"),
					Tuple.Create("ZZ1_ZZZ_NKDataGrouping", "EUN"),
				});
				var nextDataBatch = provider.GetNextDataBatch(source, metadata.Object, null, null).ToArray();
				Assert.That(nextDataBatch, Has.Length.EqualTo(1));
				Assert.That(provider.GetNextDataBatch(source, metadata.Object, null, null).ToArray(),
					Is.EqualTo(new[] { Tuple.Create((object)tariff1, info1) }));
			}
		}

		[Test]
		public void GetNextDataBatch_WithFilter_RelatedEntities()
		{
			var source = new SourceData { SDA_PK = Guid.NewGuid() };
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid(), ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN" };
			var rate1 = new RefCusRate { ZZ2_ZZZ_NKDataGrouping = "EUN" };
			var rate2 = new RefCusRate { ZZ2_ZZZ_NKDataGrouping = "IT" };
			tariff.RefCusRates = new[] { rate1, rate2 };
			var info = new DataProcessingInformation { DPI_Status = "QUE", DPI_SourceId = source.SDA_PK, DPI_ParentPk = tariff.ZZ1_PK, DPI_ParentTableCode = "ZZ1" };
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<DataProcessingInformation>()).Returns(new[] { info }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff }.AsQueryable());

			using (var provider = new StagingDataProvider(repo.Object))
			{
				var metadata = new Mock<IMetadataProvider>();
				metadata.SetupGet(x => x.FilterData).Returns(true);
				metadata.Setup(x => x.GetConstantPropertyNamesAndValues("RefCusRate")).Returns(new[]
				{
					Tuple.Create("ZZ2_ZZZ_NKDataGrouping", "EUN")
				});
				metadata.Setup(x => x.GetProperties("RefCusTariff")).Returns(new[] { "RefCusRate" });

				var result = provider.GetNextDataBatch(source, metadata.Object, null, null).ToArray();
				CollectionAssert.AreEqual(new[] { rate1 }, ((RefCusTariff)result[0].Item1).RefCusRates);
			}
		}

		[Test]
		public void GetNextDataBatch_WithMandatory_RelatedEntities()
		{
			var source = new SourceData { SDA_PK = Guid.NewGuid() };
			var tariff1 = new RefCusTariff { ZZ1_PK = new Guid("2D1B356C-3BAA-403E-9563-51C8DE8A5A1F"), ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN" };
			tariff1.RefCusRates = new[] { new RefCusRate() };
			var tariff2 = new RefCusTariff { ZZ1_PK = new Guid("737135A5-B8B5-4095-9D12-4D4205A1D910"), ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN" };
			var info1 = new DataProcessingInformation { DPI_Status = "QUE", DPI_SourceId = source.SDA_PK, DPI_ParentPk = tariff1.ZZ1_PK, DPI_ParentTableCode = "ZZ1" };
			var info2 = new DataProcessingInformation { DPI_Status = "QUE", DPI_SourceId = source.SDA_PK, DPI_ParentPk = tariff2.ZZ1_PK, DPI_ParentTableCode = "ZZ1" };
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<DataProcessingInformation>()).Returns(new[] { info1, info2 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff1, tariff2 }.AsQueryable());
			using (var provider = new StagingDataProvider(repo.Object))
			{
				var metadata = new Mock<IMetadataProvider>();
				metadata.SetupGet(x => x.FilterData).Returns(true);
				metadata.Setup(x => x.GetProperties("RefCusTariff")).Returns(new[] { "RefCusRate" });
				metadata.Setup(x => x.IsMandatory("RefCusTariff", "RefCusRate")).Returns(true);

				Assert.That(provider.GetNextDataBatch(source, metadata.Object, null, null).ToArray(),
					Is.EqualTo(new[] { Tuple.Create((object)tariff1, info1) }));
			}
		}

		[TestCase("QUE", true, true, "ERR")]
		[TestCase("QUE", true, false, "ERR")]
		[TestCase("QUE", false, true, "ERR")]
		[TestCase("QUE", false, false, "ERR")]
		[TestCase("PRS", true, true, "MER")]
		[TestCase("PRS", true, false, "ERR")]
		[TestCase("PRS", false, true, "ERR")]
		[TestCase("PRS", false, false, "ERR")]
		[TestCase("ERR", true, true, "ERR")]
		[TestCase("ERR", true, false, "ERR")]
		[TestCase("ERR", false, true, "ERR")]
		[TestCase("ERR", false, false, "ERR")]
		public void MarkSourceDataStatus(string dpiStatus, bool autoExpirationResult, bool cloneResult, string sourceStatus)
		{
			var sourceData = new SourceData() { SDA_PK = Guid.NewGuid(), SDA_SourceTime = DateTime.Now, SDA_Status = "PRS" };
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<DataProcessingInformation>()).Returns(new[]
			{
				new DataProcessingInformation { DPI_Status = dpiStatus, DPI_SourceId = sourceData.SDA_PK },
			}.AsQueryable());
			using (var stagingDataProvider = new StagingDataProvider(repo.Object))
			{
				stagingDataProvider.MarkSourceDataStatus(sourceData, autoExpirationResult, cloneResult);
				Assert.AreEqual(sourceStatus, sourceData.SDA_Status);
			}
		}

		[Test]
		public void IsAutoExpiredEnabled()
		{
			var repo = new Mock<IStagingRepository>();
			var sourceData = new SourceData { SDA_Source = DataSourceConstants.Source.InternalWebsite, SDA_SubSource = "US Exchange Rates" };
			using (var provider = new StagingDataProvider(repo.Object))
			{
				Assert.False(provider.IsAutoExpiredEnabled(sourceData));
				var sourceDataInfo = new DataSourceInformation { DSI_SubSource = "US Exchange Rates" };
				repo.Setup(x => x.Get<DataSourceInformation>()).Returns(new[] { sourceDataInfo }.AsQueryable());
				Assert.False(provider.IsAutoExpiredEnabled(sourceData));
				sourceDataInfo.DSI_EnableAutoExpiration = true;
				Assert.True(provider.IsAutoExpiredEnabled(sourceData));
			}
		}

		[Test]
		public void GetDataProcessingInformation()
		{
			var repo = new Mock<IStagingRepository>();
			var sourceData = new SourceData { SDA_PK = Guid.NewGuid() };
			var info1 = new DataProcessingInformation { DPI_SourceId = sourceData.SDA_PK };
			var info2 = new DataProcessingInformation { DPI_SourceId = sourceData.SDA_PK };
			var info3 = new DataProcessingInformation { DPI_SourceId = Guid.NewGuid() };
			repo.Setup(x => x.Get<DataProcessingInformation>()).Returns(new[] { info1, info2, info3 }.AsQueryable());
			using (var provider = new StagingDataProvider(repo.Object))
			{
				CollectionAssert.Contains(provider.GetDataProcessingInformation(sourceData), info1);
				CollectionAssert.Contains(provider.GetDataProcessingInformation(sourceData), info2);
			}
		}

		[Test]
		public void GetProcessingResults()
		{
			var repo = new Mock<IStagingRepository>();
			var processingResult1 = new DataProcessingResult { DPR_SubSource = "BB", };
			var processingResult2 = new DataProcessingResult { DPR_SubSource = "CC", };
			var processingResult3 = new DataProcessingResult { DPR_SubSource = "BB", };
			repo.Setup(x => x.Get<DataProcessingResult>()).Returns(new[] { processingResult1, processingResult2, processingResult3 }.AsQueryable());
			var sourceData = new SourceData { SDA_Source = "AA", SDA_SubSource = "BB", SDA_SourceTime = new DateTime(2018, 01, 01) };

			using (var provider = new StagingDataProvider(repo.Object))
			{
				var result = provider.GetProcessingResults(sourceData).ToArray();
				Assert.AreEqual(2, result.Length);
				CollectionAssert.AreEquivalent(new[] { processingResult1, processingResult3 }, result);
			}
		}

		[Test]
		public void GetSourceData()
		{
			var source1 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_Status = StatusProvider.GetQUEStatus()
			};
			var source2 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_Status = StatusProvider.GetPRSStatus()
			};
			var source3 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_Status = StatusProvider.GetPRSStatus(),
				SDA_NotProcessedUntil = DateTime.UtcNow.AddMinutes(5)
			};
			var source4 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_Status = StatusProvider.GetPRSStatus(),
				SDA_NotProcessedUntil = DateTime.UtcNow.AddMinutes(-1)
			};
			var source5 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.Upload,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_Status = StatusProvider.GetPRSStatus()
			};
			var source6 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.Upload,
				SDA_Filetype = DataSourceConstants.FileType.COM.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_Status = StatusProvider.GetPRSStatus(),
				SDA_NotProcessedUntil = DateTime.UtcNow.AddMinutes(-1)
			};
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<SourceData>()).Returns(new[] { source1, source2, source3, source4, source5, source6 }.AsQueryable());
			using (var provider = new StagingDataProvider(repo.Object))
			{
				var results = provider.GetSourceData().ToArray();
				Assert.Contains(source2, results);
				Assert.Contains(source4, results);
				Assert.Contains(source5, results);
				Assert.Contains(source6, results);
			}
		}

		[Test]
		public void GetDependencySourceData()
		{
			var dependentDataSource = new Dependency("Test2", new DateTime(2017, 1, 2));
			var source1 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SubSource = "Test1",
				SDA_SourceTime = new DateTime(2017, 2, 1),
				SDA_Status = StatusProvider.GetQUEStatus()
			};
			var source2 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SubSource = "Test2",
				SDA_SourceTime = new DateTime(2017, 1, 1),
				SDA_Status = StatusProvider.GetPRSStatus()
			};
			var source3 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SubSource = "Test2",
				SDA_SourceTime = new DateTime(2017, 2, 1),
				SDA_Status = StatusProvider.GetMERStatus()
			};
			var source4 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SubSource = "Test2",
				SDA_SourceTime = new DateTime(2017, 2, 1),
				SDA_Status = StatusProvider.GetPRSStatus()
			};
			var source5 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.Upload,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SubSource = "Test2",
				SDA_SourceTime = new DateTime(2017, 2, 1),
				SDA_Status = StatusProvider.GetMERStatus()
			};
			var source6 = new SourceData
			{
				SDA_Source = DataSourceConstants.Source.Upload,
				SDA_Filetype = DataSourceConstants.FileType.COM.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SubSource = "Test2",
				SDA_SourceTime = new DateTime(2017, 2, 1),
				SDA_Status = StatusProvider.GetMERStatus()
			};
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<SourceData>()).Returns(new[] { source1, source2, source3, source4, source5, source6 }.AsQueryable());
			using (var provider = new StagingDataProvider(repo.Object))
			{
				var results = provider.GetDependencySourceData(dependentDataSource).ToArray();
				Assert.AreEqual(4, results.Length);
				Assert.Contains(source3, results);
				Assert.Contains(source4, results);
				Assert.Contains(source5, results);
				Assert.Contains(source6, results);
			}
		}

		[Test]
		public void GetSingleSourceData()
		{
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			var source1 = new SourceData
			{
				SDA_PK = guid1,
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_Status = StatusProvider.GetQUEStatus(),
				SDA_NotProcessedUntil = DateTime.UtcNow.AddMinutes(-1)
			};
			var source2 = new SourceData
			{
				SDA_PK = guid2,
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_Status = StatusProvider.GetPRSStatus(),
				SDA_NotProcessedUntil = DateTime.UtcNow.AddMinutes(5)
			};
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<SourceData>()).Returns(new[] { source1, source2 }.AsQueryable());
			using (var provider = new StagingDataProvider(repo.Object))
			{
				var result1 = provider.GetSingleSourceData(guid1);
				var result2 = provider.GetSingleSourceData(guid2);
				Assert.AreEqual(source1, result1);
				Assert.AreEqual(source2, result2);
			}
		}

		[Test]
		public void GetNextDataBatch()
		{
			var source = new SourceData { SDA_PK = Guid.NewGuid() };
			var tariff1 = new RefCusTariff { ZZ1_PK = Guid.NewGuid() };
			var tariff2 = new RefCusTariff { ZZ1_PK = new Guid("13877ACD-AAC7-4138-A735-35BFB0DD6EE8") };
			var tariff3 = new RefCusTariff { ZZ1_PK = new Guid("92C2941A-FC94-4146-8795-0CC679FFCABD") };
			var info1 = new DataProcessingInformation { DPI_Status = "PRS", DPI_SourceId = source.SDA_PK, DPI_ParentPk = tariff1.ZZ1_PK, DPI_ParentTableCode = "ZZ1" };
			var info2 = new DataProcessingInformation { DPI_Status = "QUE", DPI_SourceId = source.SDA_PK, DPI_ParentPk = tariff2.ZZ1_PK, DPI_ParentTableCode = "ZZ1" };
			var info3 = new DataProcessingInformation { DPI_Status = "QUE", DPI_SourceId = source.SDA_PK, DPI_ParentPk = tariff3.ZZ1_PK, DPI_ParentTableCode = "ZZ1" };
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<DataProcessingInformation>()).Returns(new[] { info1, info2, info3 }.AsQueryable());
			repo.Setup(x => x.Get<RefCusTariff>()).Returns(new[] { tariff1, tariff2, tariff3 }.AsQueryable());
			using (var provider = new StagingDataProvider(repo.Object))
			{
				Assert.That(provider.GetNextDataBatch(source, new Mock<IMetadataProvider>().Object, 2, null).ToArray(),
					Is.EqualTo(new[] { Tuple.Create((object)tariff2, info2), Tuple.Create((object)tariff3, info3) }));
				Assert.That(provider.GetNextDataBatch(source, new Mock<IMetadataProvider>().Object, 1, null).ToArray(),
					Is.EqualTo(new[] { Tuple.Create((object)tariff2, info2) }));
				Assert.That(provider.GetNextDataBatch(source, new Mock<IMetadataProvider>().Object, 2, info2).ToArray(),
					Is.EqualTo(new[] { Tuple.Create((object)tariff3, info3) }));
			}
		}

		[Test]
		public void GetRelatedEntities()
		{
			var rate1 = new RefCusRate();
			var rate2 = new RefCusRate();
			var tariff = new RefCusTariff { ZZ1_PK = Guid.NewGuid(), RefCusRates = new[] { rate1, rate2 } };
			var repo = new Mock<IStagingRepository>();
			using (var provider = new StagingDataProvider(repo.Object))
			{
				Assert.That(provider.GetRelatedEntities<RefCusTariff, RefCusRate>(tariff).ToArray(), Is.EqualTo(new[] { rate1, rate2 }));
			}
		}

		[Test]
		public void GetRelatedEntityTypes()
		{
			var propertyNames = new[] { "ZZ1_PK", "ZZ1_TariffCode", "RefCusRate", "RefCusTariffUOM" };
			var metadataProvider = new Mock<IMetadataProvider>();
			metadataProvider.Setup(x => x.GetProperties(nameof(RefCusTariff))).Returns(propertyNames);
			var repo = new Mock<IStagingRepository>();
			using (var provider = new StagingDataProvider(repo.Object))
			{
				CollectionAssert.AreEqual(new[] { typeof(RefCusRate), typeof(RefCusTariffUOM) },
					provider.GetRelatedEntityTypes(nameof(RefCusTariff), metadataProvider.Object));
			}
		}

		[Test]
		public void GetFKPropertyName()
		{
			using (var provider = new StagingDataProvider(new Mock<IStagingRepository>().Object))
			{
				Assert.That(provider.GetFKPropertyName(typeof(RefCusRate), typeof(RefCusTariff)),
					Is.EqualTo(nameof(RefCusRate.ZZ2_ZZ1_Tariff)));
				Assert.That(provider.GetFKPropertyName(typeof(RefCusRate), typeof(RefCusTariffNationalCode)),
					Is.EqualTo(nameof(RefCusRate.ZZ2_ZZW_TariffNationalCode)));
			}
		}

		[Test]
		public void TestGetType()
		{
			using (var provider = new StagingDataProvider(new Mock<IStagingRepository>().Object))
			{
				Assert.That(provider.GetTypeFromTblPrefix("ZZ1"), Is.EqualTo(typeof(RefCusTariff)));
				Assert.That(provider.GetTypeFromTblPrefix("ZZ2"), Is.EqualTo(typeof(RefCusRate)));
			}
		}

		[Test]
		public async Task SaveUpdateResults()
		{
			var repoMock = new Mock<IStagingRepository>();
			using (var provider = new StagingDataProvider(repoMock.Object))
			{
				var source = new SourceData { SDA_PK = Guid.NewGuid(), SDA_Source = "AA", SDA_SubSource = "BB", SDA_SourceTime = new DateTime(2022, 10, 01) };
				var updaterResults = new[]
				{
					new SafeObjectUpdaterResult
					{
						Action = ResultAction.Insert,
						ExpirableAncestorPK = Guid.Empty,
						ParentCode = "A",
						ParentPK = Guid.Empty
					},
					new SafeObjectUpdaterResult
					{
						Action = ResultAction.Expire,
						ExpirableAncestorPK = Guid.Empty,
						ParentCode = "A",
						ParentPK = Guid.Empty
					}
				};
				await provider.SaveUpdateResults(source, updaterResults, null);
				repoMock.Verify(x => x.BulkInsertAsync(It.Is<IEnumerable<DataProcessingResult>>(d => d.Count() == 1), null, null));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task SaveUpdateResultsWithDataBase()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connString = TestConnectionString.GetAdmin(dbName);

			var sourceData1 = new SourceData() { SDA_SubSource = "Test", SDA_SourceTime = new DateTime(2023, 10, 1) };
			var sourceData2 = new SourceData() { SDA_SubSource = "Test", SDA_SourceTime = new DateTime(2023, 11, 1) };
			var parentPK1 = Guid.Parse("02F8811E-0307-47D5-B6AD-BFC78115DBF1");
			var parentPK2 = Guid.Parse("52D5BEE7-0F39-4921-90CF-8A9D7519486E");
			var parentPK3 = Guid.Parse("3DC0B4A6-A90C-4EC0-A6FD-C28A261B4C4F");
			var safeObjectUpdaterResult1 = new SafeObjectUpdaterResult { ParentPK = parentPK1, ParentCode = "ZZ1", Action = ResultAction.Insert };
			var safeObjectUpdaterResult2 = new SafeObjectUpdaterResult { ParentPK = parentPK2, ParentCode = "ZZ1", Action = ResultAction.Insert };
			var safeObjectUpdaterResult3 = new SafeObjectUpdaterResult { ParentPK = parentPK1, ParentCode = "ZZ1", Action = ResultAction.Update };
			var safeObjectUpdaterResult4 = new SafeObjectUpdaterResult { ParentPK = parentPK3, ParentCode = "ZZ1", Action = ResultAction.Insert };
			var safeObjectUpdaterResult5 = new SafeObjectUpdaterResult { ParentPK = parentPK2, ParentCode = "ZZ1", Action = ResultAction.Expire };

			using (var repo = new StagingRepository(connString))
			using (var stagingDataProvider = new StagingDataProvider(repo))
			{
				await stagingDataProvider.SaveUpdateResults(sourceData1, [safeObjectUpdaterResult1, safeObjectUpdaterResult2], null);
				Assert.Multiple(() =>
				{
					Assert.AreEqual(2, repo.Get<DataProcessingResult>().Count());
				});
			}

			using (var repo = new StagingRepository(connString))
			using (var stagingDataProvider = new StagingDataProvider(repo))
			{
				await stagingDataProvider.SaveUpdateResults(sourceData2, [safeObjectUpdaterResult3, safeObjectUpdaterResult4, safeObjectUpdaterResult5], null);
				Assert.Multiple(() =>
				{
					var dataProcessResults = repo.Get<DataProcessingResult>();
					Assert.AreEqual(4, dataProcessResults.Count());
					Assert.AreEqual(3, dataProcessResults.Where(o => o.DPR_Status == "QUE").Count());
					Assert.AreEqual("PRS", dataProcessResults.Single(o => o.DPR_ParentPK == parentPK2).DPR_Status);
				});
			}
		}

		[Test]
		public async Task UpdateSDA_NotProcessedUntil()
		{
			var repoMock = new Mock<IStagingRepository>();
			using (var provider = new StagingDataProvider(repoMock.Object))
			{
				var source = new SourceData { SDA_PK = Guid.NewGuid() };
				var date = new DateTime(2023, 06, 15);
				await provider.UpdateSDA_NotProcessedUntil(source.SDA_PK, date);

				repoMock.Verify(x => x.ExecuteSqlCommandAsync($@"
UPDATE {nameof(SourceData)}
SET {nameof(SourceData.SDA_NotProcessedUntil)} = @p0
WHERE {nameof(SourceData.SDA_PK)} = '{source.SDA_PK}'", It.IsAny<object[]>()));
			}
		}

		[Test]
		public void GetQueuedProcessingClone()
		{
			var sourceID = Guid.NewGuid();
			var clone1 = new DataProcessingClone { DPC_PK = Guid.Parse("2ADE9D50-E992-4677-A0EF-1D2C5FAB8C83"), DPC_SourceId = sourceID, DPC_TableCode = "ZZ1", DPC_Status = "QUE" };
			var clone2 = new DataProcessingClone { DPC_PK = Guid.Parse("15E81D9B-EEF7-4461-90CC-5C0BE0059FF8"), DPC_SourceId = sourceID, DPC_TableCode = "ZZ1", DPC_Status = "QUE" };
			var clone3 = new DataProcessingClone { DPC_PK = Guid.Parse("2B077337-4112-4225-8788-9B5FE0CD179B"), DPC_SourceId = sourceID, DPC_TableCode = "ZZ1", DPC_Status = "PRS" };
			var clone4 = new DataProcessingClone { DPC_PK = Guid.Parse("37304AE4-3CE9-4852-AC69-628F1B642318"), DPC_SourceId = Guid.Empty, DPC_TableCode = "ZZ1", DPC_Status = "QUE" };
			var clone5 = new DataProcessingClone { DPC_PK = Guid.Parse("4D25103E-97B9-4E3E-A783-CF2429D68920"), DPC_SourceId = sourceID, DPC_TableCode = "ZZ2", DPC_Status = "QUE" };
			var stagingRepo = new Mock<IStagingRepository>();
			stagingRepo.Setup(x => x.Get<DataProcessingClone>()).Returns(new[] { clone1, clone2, clone3, clone4, clone5 }.AsQueryable());
			using (var stagingProvider = new StagingDataProvider(stagingRepo.Object))
			{
				var startGuid = Guid.Empty;
				var result = stagingProvider.GetQueuedProcessingClone(sourceID, "ZZ1", startGuid);
				Assert.AreEqual(2, result.Count());
				Assert.True(result.All(x => x.DPC_Status == "QUE"));
				Assert.AreEqual(clone2.DPC_PK, result.First().DPC_PK);

				startGuid = clone2.DPC_PK;
				result = stagingProvider.GetQueuedProcessingClone(sourceID, "ZZ1", startGuid);
				Assert.AreEqual(1, result.Count());
				Assert.AreEqual(clone1.DPC_PK, result.First().DPC_PK);
			}
		}

		[Test]
		public async Task CreateDataProcessingCloneRecords()
		{
			var sourceID = Guid.NewGuid();
			var newPK = Guid.NewGuid();
			var updateResult1 = new SafeObjectUpdaterResult { ParentCode = "ZZ1", ParentPK = Guid.Empty };
			var updateResult2 = new SafeObjectUpdaterResult { ParentCode = "ZZ1", ParentPK = Guid.Empty, NewRecordForCloneActionPK = newPK };
			var updateResult3 = new SafeObjectUpdaterResult { ParentCode = "ZZ2", ParentPK = Guid.Empty, NewRecordForCloneActionPK = newPK };
			var stagingRepo = new Mock<IStagingRepository>();
			using (var stagingProvider = new StagingDataProvider(stagingRepo.Object))
			{
				await stagingProvider.CreateDataProcessingCloneRecords(sourceID, [updateResult1, updateResult2, updateResult3], null);
				stagingRepo.Verify(x => x.BulkInsertAsync(It.Is<IEnumerable<DataProcessingClone>>(d => d.First().DPC_SourceId == sourceID && d.First().DPC_NewRecordPK == newPK), null, null), Times.Once);
				stagingRepo.Verify(x => x.BulkInsertAsync(It.Is<IEnumerable<DataProcessingClone>>(d => d.All(c => c.DPC_TableCode == "ZZ1")), null, null));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task CreateDPRForCloneResults()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connString = TestConnectionString.GetAdmin(dbName);

			var publicationTime = DateTime.UtcNow;
			var parentPK1 = Guid.NewGuid();
			var parentPK2 = Guid.NewGuid();
			var newRecordPK1 = Guid.NewGuid();
			var newRecordPK2 = Guid.NewGuid();
			var ancestorPK = Guid.NewGuid();
			var dataSetPK = Guid.NewGuid();

			var processingResult1 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "AA",
				DPR_ParentPK = parentPK1,
				DPR_ParentTableCode = "ZZ1",
				DPR_PublicationTime = publicationTime,
				DPR_Status = "QUE"
			};
			var processingResult2 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = parentPK2,
				DPR_ParentTableCode = "ZZ2",
				DPR_PublicationTime = publicationTime,
				DPR_Status = "PRS",
				DPR_ExpirationTime = publicationTime
			};

			using (var repo = new StagingRepository(connString))
			using (var stagingProvider = new StagingDataProvider(repo))
			{
				repo.Add(processingResult1);
				repo.Add(processingResult2);
				await repo.SaveChangesAsync();

				var cloneProcessResult1 = new CloneProcessResult { OriginalRecordPK = parentPK1, ClonedRecordPK = newRecordPK1, ClonedRecordExpirableAncestorPK = ancestorPK, DataSetPK = dataSetPK };
				var cloneProcessResult2 = new CloneProcessResult { OriginalRecordPK = parentPK2, ClonedRecordPK = newRecordPK2, ClonedRecordExpirableAncestorPK = ancestorPK, DataSetPK = dataSetPK };
				var cloneProcessResult3 = new CloneProcessResult { OriginalRecordPK = Guid.NewGuid(), ClonedRecordPK = newRecordPK2, ClonedRecordExpirableAncestorPK = ancestorPK, DataSetPK = dataSetPK };
				var result = await stagingProvider.CreateDPRForCloneResults(new[] { cloneProcessResult1, cloneProcessResult2, cloneProcessResult3 });
				Assert.AreEqual(2, result);
				var dprRecords = repo.Get<DataProcessingResult>().ToList();
				Assert.AreEqual(4, dprRecords.Count);

				var clonedDPR = dprRecords.First(x => x.DPR_ParentPK == newRecordPK1);
				Assert.AreEqual(processingResult1.DPR_SubSource, clonedDPR.DPR_SubSource);
				Assert.AreEqual(processingResult1.DPR_Status, clonedDPR.DPR_Status);
				Assert.AreEqual(processingResult1.DPR_ParentTableCode, clonedDPR.DPR_ParentTableCode);
				Assert.AreEqual(processingResult1.DPR_PublicationTime, clonedDPR.DPR_PublicationTime);
				Assert.AreEqual(cloneProcessResult1.ClonedRecordExpirableAncestorPK, clonedDPR.DPR_ExpirableAncestorPK);
				Assert.AreEqual(cloneProcessResult1.DataSetPK, clonedDPR.DPR_DatasetPK);

				clonedDPR = dprRecords.First(x => x.DPR_ParentPK == newRecordPK2);
				Assert.AreEqual(processingResult2.DPR_SubSource, clonedDPR.DPR_SubSource);
				Assert.AreEqual(processingResult2.DPR_Status, clonedDPR.DPR_Status);
				Assert.AreEqual(processingResult2.DPR_ParentTableCode, clonedDPR.DPR_ParentTableCode);
				Assert.AreEqual(processingResult2.DPR_PublicationTime, clonedDPR.DPR_PublicationTime);
				Assert.AreEqual(cloneProcessResult2.ClonedRecordExpirableAncestorPK, clonedDPR.DPR_ExpirableAncestorPK);
				Assert.AreEqual(cloneProcessResult2.DataSetPK, clonedDPR.DPR_DatasetPK);
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetNextDataBatch_WithTransform_RefCusApplicability()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connString = TestConnectionString.GetAdmin(dbName);

			var startTime = DateTime.UtcNow;
			var endTime = DateTime.UtcNow.AddYears(50);
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "INT",
				SDA_Filename = "",
				SDA_Filetype = "XML",
				SDA_ContentText = GetNextDataBatch_WithTransform_RefCusApplicability_SourceDataContentText,
				SDA_Status = "PRS",
				SDA_ContentType = "URD",
				SDA_SourceTime = DateTime.Parse("2022-01-18T13:31:25", CultureInfo.InvariantCulture),
				SDA_SubSource = "Test",
			};
			var tariff = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_ZZI_NKTariffType = "IMP",
				ZZ1_TariffCode = "22030",
				ZZ1_ZZZ_NKDataGrouping = "EUN",
				ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN",
				ZZ1_Description = "",
				ZZ1_ZZF_NKTaxOrFeeCode = "",
				ZZ1_CompositeKeyOnZZ5 = "",
				ZZ1_StartDate = startTime,
				ZZ1_EndDate = endTime,
			};
			var rate1 = new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ2_ZY1_NKRateCode = "035",
				ZZ2_ZY1_ZZR_NKRateType = "050",
				ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = "NL",
				ZZ2_RateFormula = "43.92 * [HLT]",
				ZZ2_ZZZ_NKDataGrouping = "NL",
				ZZ2_SelectorFormula = "",
				ZZ2_RX_NKCurrencyOverride = "",
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_StartDate = startTime,
				ZZ2_EndDate = endTime,
			};
			var rate2 = new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ2_ZY1_NKRateCode = "035",
				ZZ2_ZY1_ZZR_NKRateType = "050",
				ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = "NL",
				ZZ2_RateFormula = "47.48 * [HLT]",
				ZZ2_ZZZ_NKDataGrouping = "NL",
				ZZ2_SelectorFormula = "",
				ZZ2_RX_NKCurrencyOverride = "",
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_StartDate = startTime,
				ZZ2_EndDate = endTime,
			};
			var app1 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate1.ZZ2_PK,
				ZZT_ZZA_NKTradeGroup = "1011",
				ZZT_ZZA_ZZZ_NKDataGrouping = "NL",
				ZZT_AdditionalCode = "U313",
				ZZT_OrderNumber = "",
				ZZT_StartDate = startTime,
				ZZT_EndDate = endTime,
			};
			var app2 = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate2.ZZ2_PK,
				ZZT_ZZA_NKTradeGroup = "1011",
				ZZT_ZZA_ZZZ_NKDataGrouping = "NL",
				ZZT_AdditionalCode = "U319",
				ZZT_OrderNumber = "",
				ZZT_StartDate = startTime,
				ZZT_EndDate = endTime,
			};
			var info = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_Status = "QUE",
				DPI_SourceId = sourceData.SDA_PK,
				DPI_ParentPk = tariff.ZZ1_PK,
				DPI_ParentTableCode = "ZZ1",
				DPI_HasDPRRecordWhenError = false
			};
			using (var repo = new StagingRepository(connString))
			{
				repo.Add(sourceData);
				repo.Add(tariff);
				repo.Add(rate1);
				repo.Add(rate2);
				repo.Add(app1);
				repo.Add(app2);
				repo.Add(info);
				await repo.SaveChangesAsync();
			}
			using (var repo = new StagingRepository(connString))
			using (var stagingProvider = new StagingDataProvider(repo))
			{
				var metaDataProvider = new MetadataProvider(GetNextDataBatch_WithTransform_RefCusApplicability_SourceDataContentText, cacheProvider, cacheProviderForOriginal);
				var nextDataBatch = stagingProvider.GetNextDataBatch(sourceData, metaDataProvider, null, null).ToArray();
				Assert.That(nextDataBatch, Has.Length.EqualTo(1));
				var tariffResult = nextDataBatch.First().Item1 as RefCusTariff;
				Assert.That(tariffResult, Is.Not.Null);
				Assert.That(tariffResult.RefCusRateApplicabilities, Has.Count.EqualTo(2));

				CollectionAssert.AreEquivalent(new string[] { rate1.ZZ2_ZY1_NKRateCode, rate2.ZZ2_ZY1_NKRateCode }, tariffResult.RefCusRateApplicabilities.Select(x => x.S01_ZY1_NKRateCode));
				CollectionAssert.AreEquivalent(new string[] { rate1.ZZ2_ZY1_ZZR_NKRateType, rate2.ZZ2_ZY1_ZZR_NKRateType }, tariffResult.RefCusRateApplicabilities.Select(x => x.S01_ZY1_ZZR_NKRateType));
				CollectionAssert.AreEquivalent(new string[] { rate1.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, rate2.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping }, tariffResult.RefCusRateApplicabilities.Select(x => x.S01_ZY1_ZZR_ZZZ_NKDataGrouping));
				CollectionAssert.AreEquivalent(new string[] { rate1.ZZ2_RateFormula, rate2.ZZ2_RateFormula }, tariffResult.RefCusRateApplicabilities.Select(x => x.S01_RateFormula));
				CollectionAssert.AreEquivalent(new string[] { rate1.ZZ2_ZZZ_NKDataGrouping, rate2.ZZ2_ZZZ_NKDataGrouping }, tariffResult.RefCusRateApplicabilities.Select(x => x.S01_ZZZ_NKDataGrouping));
				CollectionAssert.AreEquivalent(new string[] { app1.ZZT_ZZA_NKTradeGroup, app2.ZZT_ZZA_NKTradeGroup }, tariffResult.RefCusRateApplicabilities.Select(x => x.S01_ZZA_NKTradeGroup));
				CollectionAssert.AreEquivalent(new string[] { app1.ZZT_ZZA_ZZZ_NKDataGrouping, app2.ZZT_ZZA_ZZZ_NKDataGrouping }, tariffResult.RefCusRateApplicabilities.Select(x => x.S01_ZZA_ZZZ_NKDataGrouping));
				CollectionAssert.AreEquivalent(new string[] { app1.ZZT_AdditionalCode, app2.ZZT_AdditionalCode }, tariffResult.RefCusRateApplicabilities.Select(x => x.S01_AdditionalCode));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetNextDataBatch_WithTransform_RefCusApplicability_WithRateApplicabilityUOM()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connString = TestConnectionString.GetAdmin(dbName);

			var startTime = DateTime.UtcNow;
			var endTime = DateTime.UtcNow.AddYears(50);
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "INT",
				SDA_Filename = "",
				SDA_Filetype = "XML",
				SDA_ContentText = GetNextDataBatch_WithTransform_RefCusApplicability_WithRateApplicabilityUOM_SourceDataContentText,
				SDA_Status = "PRS",
				SDA_ContentType = "URD",
				SDA_SourceTime = DateTime.Parse("2022-01-18T13:31:25", CultureInfo.InvariantCulture),
				SDA_SubSource = "Test",
			};
			var tariff = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_ZZI_NKTariffType = "IMP",
				ZZ1_TariffCode = "22030",
				ZZ1_ZZZ_NKDataGrouping = "EUN",
				ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN",
				ZZ1_Description = "",
				ZZ1_ZZF_NKTaxOrFeeCode = "",
				ZZ1_CompositeKeyOnZZ5 = "",
				ZZ1_StartDate = startTime,
				ZZ1_EndDate = endTime,
			};
			var rate = new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ2_ZY1_NKRateCode = "035",
				ZZ2_ZY1_ZZR_NKRateType = "050",
				ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = "NL",
				ZZ2_RateFormula = "43.92 * [HLT]",
				ZZ2_ZZZ_NKDataGrouping = "NL",
				ZZ2_SelectorFormula = "",
				ZZ2_RX_NKCurrencyOverride = "",
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_StartDate = startTime,
				ZZ2_EndDate = endTime,
			};
			var app = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate.ZZ2_PK,
				ZZT_ZZA_NKTradeGroup = "1011",
				ZZT_ZZA_ZZZ_NKDataGrouping = "NL",
				ZZT_AdditionalCode = "U313",
				ZZT_OrderNumber = "",
				ZZT_StartDate = startTime,
				ZZT_EndDate = endTime,
			};
			var rateUOM1 = new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_ZZ2_Rate = rate.ZZ2_PK,
				ZXG_UOM = "HLT",
			};
			var rateUOM2 = new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_ZZ2_Rate = rate.ZZ2_PK,
				ZXG_UOM = "MIL",
			};
			var info = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_Status = "QUE",
				DPI_SourceId = sourceData.SDA_PK,
				DPI_ParentPk = tariff.ZZ1_PK,
				DPI_ParentTableCode = "ZZ1",
				DPI_HasDPRRecordWhenError = false
			};

			using (var repo = new StagingRepository(connString))
			{
				repo.Add(sourceData);
				repo.Add(tariff);
				repo.Add(rate);
				repo.Add(app);
				repo.Add(rateUOM1);
				repo.Add(rateUOM2);
				repo.Add(info);
				await repo.SaveChangesAsync();
			}
			using (var repo = new StagingRepository(connString))
			using (var stagingProvider = new StagingDataProvider(repo))
			{
				var metaDataProvider = new MetadataProvider(GetNextDataBatch_WithTransform_RefCusApplicability_WithRateApplicabilityUOM_SourceDataContentText, cacheProvider, cacheProviderForOriginal);
				var nextDataBatch = stagingProvider.GetNextDataBatch(sourceData, metaDataProvider, null, null).ToArray();
				Assert.That(nextDataBatch, Has.Length.EqualTo(1));
				var tariffResult = nextDataBatch.First().Item1 as RefCusTariff;
				Assert.That(tariffResult, Is.Not.Null);
				Assert.That(tariffResult.RefCusRateApplicabilities, Has.Count.EqualTo(1));

				Assert.That(tariffResult.RefCusRateApplicabilities.First().RefCusRateApplicablityUOMs, Has.Count.EqualTo(2));
				CollectionAssert.AreEquivalent(new string[] { rateUOM1.ZXG_UOM, rateUOM2.ZXG_UOM }, tariffResult.RefCusRateApplicabilities.First().RefCusRateApplicablityUOMs.Select(x => x.S02_UOM));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetNextDataBatch_WithTransform_RefCusApplicability_WithExcludedTradeGroupNew()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connString = TestConnectionString.GetAdmin(dbName);

			var startTime = DateTime.UtcNow;
			var endTime = DateTime.UtcNow.AddYears(50);
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "INT",
				SDA_Filename = "",
				SDA_Filetype = "XML",
				SDA_ContentText = GetNextDataBatch_WithTransform_RefCusApplicability_WithExcludedTradeGroupNew_SourceDataContentText,
				SDA_Status = "PRS",
				SDA_ContentType = "URD",
				SDA_SourceTime = DateTime.Parse("2022-01-18T13:31:25", CultureInfo.InvariantCulture),
				SDA_SubSource = "Test",
			};
			var tariff = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_ZZI_NKTariffType = "IMP",
				ZZ1_TariffCode = "22030",
				ZZ1_ZZZ_NKDataGrouping = "EUN",
				ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN",
				ZZ1_Description = "",
				ZZ1_ZZF_NKTaxOrFeeCode = "",
				ZZ1_CompositeKeyOnZZ5 = "",
				ZZ1_StartDate = startTime,
				ZZ1_EndDate = endTime,
			};
			var rate = new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ2_ZY1_NKRateCode = "035",
				ZZ2_ZY1_ZZR_NKRateType = "050",
				ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = "NL",
				ZZ2_RateFormula = "43.92 * [HLT]",
				ZZ2_ZZZ_NKDataGrouping = "NL",
				ZZ2_SelectorFormula = "",
				ZZ2_RX_NKCurrencyOverride = "",
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_StartDate = startTime,
				ZZ2_EndDate = endTime,
			};
			var app = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate.ZZ2_PK,
				ZZT_ZZA_NKTradeGroup = "1011",
				ZZT_ZZA_ZZZ_NKDataGrouping = "NL",
				ZZT_AdditionalCode = "U313",
				ZZT_OrderNumber = "",
				ZZT_StartDate = startTime,
				ZZT_EndDate = endTime,
			};
			var excludedTradeGroup1 = new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZT_Applicability = app.ZZT_PK,
				ZZC_ZZA_NKTradeGroup = "1011",
				ZZC_ZZA_ZZZ_NKDataGrouping = "NL",
			};
			var excludedTradeGroup2 = new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZT_Applicability = app.ZZT_PK,
				ZZC_ZZA_NKTradeGroup = "10",
				ZZC_ZZA_ZZZ_NKDataGrouping = "CC",
			};
			var info = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_Status = "QUE",
				DPI_SourceId = sourceData.SDA_PK,
				DPI_ParentPk = tariff.ZZ1_PK,
				DPI_ParentTableCode = "ZZ1",
				DPI_HasDPRRecordWhenError = false
			};

			using (var repo = new StagingRepository(connString))
			{
				repo.Add(sourceData);
				repo.Add(tariff);
				repo.Add(rate);
				repo.Add(app);
				repo.Add(excludedTradeGroup1);
				repo.Add(excludedTradeGroup2);
				repo.Add(info);
				await repo.SaveChangesAsync();
			}
			using (var repo = new StagingRepository(connString))
			using (var stagingProvider = new StagingDataProvider(repo))
			{
				var metaDataProvider = new MetadataProvider(GetNextDataBatch_WithTransform_RefCusApplicability_WithExcludedTradeGroupNew_SourceDataContentText, cacheProvider, cacheProviderForOriginal);
				var nextDataBatch = stagingProvider.GetNextDataBatch(sourceData, metaDataProvider, null, null).ToArray();
				Assert.That(nextDataBatch, Has.Length.EqualTo(1));
				var tariffResult = nextDataBatch.First().Item1 as RefCusTariff;
				Assert.That(tariffResult, Is.Not.Null);
				Assert.That(tariffResult.RefCusRateApplicabilities, Has.Count.EqualTo(1));

				Assert.That(tariffResult.RefCusRateApplicabilities.First().RefCusExcludedTradeGroupNews, Has.Count.EqualTo(2));
				CollectionAssert.AreEquivalent(new string[] { excludedTradeGroup1.ZZC_ZZA_NKTradeGroup, excludedTradeGroup2.ZZC_ZZA_NKTradeGroup }, tariffResult.RefCusRateApplicabilities.First().RefCusExcludedTradeGroupNews.Select(x => x.S03_ZZA_NKTradeGroup));
				CollectionAssert.AreEquivalent(new string[] { excludedTradeGroup1.ZZC_ZZA_ZZZ_NKDataGrouping, excludedTradeGroup2.ZZC_ZZA_ZZZ_NKDataGrouping }, tariffResult.RefCusRateApplicabilities.First().RefCusExcludedTradeGroupNews.Select(x => x.S03_ZZA_ZZZ_NKDataGrouping));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetNextDataBatch_WithTransform_RefCusApplicability_WithUOMAndExcludedTradeGroupNew()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connString = TestConnectionString.GetAdmin(dbName);

			var startTime = DateTime.UtcNow;
			var endTime = DateTime.UtcNow.AddYears(50);
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "INT",
				SDA_Filename = "",
				SDA_Filetype = "XML",
				SDA_ContentText = GetNextDataBatch_WithTransform_RefCusApplicability_WithUOMAndExcludedTradeGroupNew_SourceDataContentText,
				SDA_Status = "PRS",
				SDA_ContentType = "URD",
				SDA_SourceTime = DateTime.Parse("2022-01-18T13:31:25", CultureInfo.InvariantCulture),
				SDA_SubSource = "Test",
			};
			var tariff = new RefCusTariff
			{
				ZZ1_PK = Guid.NewGuid(),
				ZZ1_ZZI_NKTariffType = "IMP",
				ZZ1_TariffCode = "22030",
				ZZ1_ZZZ_NKDataGrouping = "EUN",
				ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN",
				ZZ1_Description = "",
				ZZ1_ZZF_NKTaxOrFeeCode = "",
				ZZ1_CompositeKeyOnZZ5 = "",
				ZZ1_StartDate = startTime,
				ZZ1_EndDate = endTime,
			};
			var rate = new RefCusRate
			{
				ZZ2_PK = Guid.NewGuid(),
				ZZ2_ZZ1_Tariff = tariff.ZZ1_PK,
				ZZ2_ZY1_NKRateCode = "035",
				ZZ2_ZY1_ZZR_NKRateType = "050",
				ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping = "NL",
				ZZ2_RateFormula = "43.92 * [HLT]",
				ZZ2_ZZZ_NKDataGrouping = "NL",
				ZZ2_SelectorFormula = "",
				ZZ2_RX_NKCurrencyOverride = "",
				ZZ2_ZZW_TariffNationalCode = null,
				ZZ2_StartDate = startTime,
				ZZ2_EndDate = endTime,
			};
			var app = new RefCusApplicability
			{
				ZZT_PK = Guid.NewGuid(),
				ZZT_ZZ2_Rate = rate.ZZ2_PK,
				ZZT_ZZA_NKTradeGroup = "1011",
				ZZT_ZZA_ZZZ_NKDataGrouping = "NL",
				ZZT_AdditionalCode = "U313",
				ZZT_OrderNumber = "",
				ZZT_StartDate = startTime,
				ZZT_EndDate = endTime,
			};
			var rateUOM = new RefCusRateUOM
			{
				ZXG_PK = Guid.NewGuid(),
				ZXG_ZZ2_Rate = rate.ZZ2_PK,
				ZXG_UOM = "HLT",
			};
			var excludedTradeGroup = new RefCusExcludedTradeGroup
			{
				ZZC_PK = Guid.NewGuid(),
				ZZC_ZZT_Applicability = app.ZZT_PK,
				ZZC_ZZA_NKTradeGroup = "1011",
				ZZC_ZZA_ZZZ_NKDataGrouping = "NL",
			};
			var info = new DataProcessingInformation
			{
				DPI_ID = Guid.NewGuid(),
				DPI_Status = "QUE",
				DPI_SourceId = sourceData.SDA_PK,
				DPI_ParentPk = tariff.ZZ1_PK,
				DPI_ParentTableCode = "ZZ1",
				DPI_HasDPRRecordWhenError = false
			};

			using (var repo = new StagingRepository(connString))
			{
				repo.Add(sourceData);
				repo.Add(tariff);
				repo.Add(rate);
				repo.Add(app);
				repo.Add(rateUOM);
				repo.Add(excludedTradeGroup);
				repo.Add(info);
				await repo.SaveChangesAsync();
			}
			using (var repo = new StagingRepository(connString))
			using (var stagingProvider = new StagingDataProvider(repo))
			{
				var metaDataProvider = new MetadataProvider(GetNextDataBatch_WithTransform_RefCusApplicability_WithUOMAndExcludedTradeGroupNew_SourceDataContentText, cacheProvider, cacheProviderForOriginal);
				var nextDataBatch = stagingProvider.GetNextDataBatch(sourceData, metaDataProvider, null, null).ToArray();
				Assert.That(nextDataBatch, Has.Length.EqualTo(1));
				var tariffResult = nextDataBatch.First().Item1 as RefCusTariff;
				Assert.That(tariffResult, Is.Not.Null);
				Assert.That(tariffResult.RefCusRateApplicabilities, Has.Count.EqualTo(1));

				Assert.That(tariffResult.RefCusRateApplicabilities.First().RefCusRateApplicablityUOMs, Has.Count.EqualTo(1));
				Assert.That(tariffResult.RefCusRateApplicabilities.First().RefCusRateApplicablityUOMs.First().S02_UOM, Is.EqualTo(rateUOM.ZXG_UOM));

				Assert.That(tariffResult.RefCusRateApplicabilities.First().RefCusExcludedTradeGroupNews, Has.Count.EqualTo(1));
				Assert.That(tariffResult.RefCusRateApplicabilities.First().RefCusExcludedTradeGroupNews.First().S03_ZZA_NKTradeGroup, Is.EqualTo(excludedTradeGroup.ZZC_ZZA_NKTradeGroup));
				Assert.That(tariffResult.RefCusRateApplicabilities.First().RefCusExcludedTradeGroupNews.First().S03_ZZA_ZZZ_NKDataGrouping, Is.EqualTo(excludedTradeGroup.ZZC_ZZA_ZZZ_NKDataGrouping));
			}
		}

		ICacheProvider cacheProvider;
		ICacheProvider cacheProviderForOriginal;

		[SetUp]
		public void SetUp()
		{
			var cacheProviderMock = new Mock<ICacheProvider>();
			cacheProviderMock.SetupGet(x => x.MetadataCache).Returns(() => new ConcurrentDictionary<string, Lazy<object>>());
			cacheProvider = cacheProviderMock.Object;

			var cacheProviderForOriginalMock = new Mock<ICacheProvider>();
			cacheProviderForOriginalMock.SetupGet(x => x.MetadataCache).Returns(() => new ConcurrentDictionary<string, Lazy<object>>());
			cacheProviderForOriginal = cacheProviderForOriginalMock.Object;
		}

		const string GetNextDataBatch_WithTransform_RefCusApplicability_SourceDataContentText = @"
<UniversalReferenceData>
    <DataSource>Test</DataSource>
    <PublicationTime>2022-01-18T13:31:25</PublicationTime>
    <UpdateType>Full</UpdateType>
    <Schema>
        <EntityType Name=""RefCusTariff"">
            <Key>
                <PropertyRef Name=""ZZ1_TariffCode"" />
                <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRateApplicability"" Type=""RefCusRateApplicability"" />
            <Property Name=""ZZ1_TariffCode"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRateApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""S01_ZY1_NKRateCode"" />
                <PropertyRef Name=""S01_ZY1_ZZR_NKRateType"" />
                <PropertyRef Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""S01_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""S01_AdditionalCode"" />
                <PropertyRef Name=""S01_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""S01_EndDate"" Type=""datetime"" />
            <Property Name=""S01_RateFormula"" Type=""varchar"" />
            <Property Name=""S01_StartDate"" Type=""datetime"" />
            <Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" />
            <Property Name=""S01_ZY1_ZZR_NKRateType"" Type=""varchar"" />
            <Property Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""S01_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
            <Property Name=""S01_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""S01_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
    </Schema>
    <OriginalSchema>
        <EntityType Name=""RefCusTariff"">
            <Key>
                <PropertyRef Name=""ZZ1_TariffCode"" />
                <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRate"" Type=""RefCusRate"" />
            <Property Name=""ZZ1_TariffCode"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRate"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusApplicability"" />
                <PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
                <PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
                <PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
            <Property Name=""ZZ2_EndDate"" Type=""datetime"" />
            <Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
            <Property Name=""ZZ2_StartDate"" Type=""datetime"" />
            <Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" />
            <Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" />
            <Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""ZZT_AdditionalCode"" />
                <PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
            <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
            <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
            <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
    </OriginalSchema>
</UniversalReferenceData>";

		const string GetNextDataBatch_WithTransform_RefCusApplicability_WithRateApplicabilityUOM_SourceDataContentText = @"
<UniversalReferenceData>
    <DataSource>Test</DataSource>
    <PublicationTime>2022-01-18T13:31:25</PublicationTime>
    <UpdateType>Full</UpdateType>
    <Schema>
        <EntityType Name=""RefCusTariff"">
            <Key>
                <PropertyRef Name=""RefCusRateApplicability"" />
                <PropertyRef Name=""ZZ1_TariffCode"" />
                <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRateApplicability"" Type=""RefCusRateApplicability"" />
            <Property Name=""ZZ1_TariffCode"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRateApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusRateApplicabilityUOM"" />
                <PropertyRef Name=""S01_ZY1_NKRateCode"" />
                <PropertyRef Name=""S01_ZY1_ZZR_NKRateType"" />
                <PropertyRef Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""S01_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""S01_AdditionalCode"" />
                <PropertyRef Name=""S01_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRateApplicabilityUOM"" Type=""RefCusRateApplicabilityUOM"" />
            <Property Name=""S01_EndDate"" Type=""datetime"" />
            <Property Name=""S01_RateFormula"" Type=""varchar"" />
            <Property Name=""S01_StartDate"" Type=""datetime"" />
            <Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" />
            <Property Name=""S01_ZY1_ZZR_NKRateType"" Type=""varchar"" />
            <Property Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""S01_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
            <Property Name=""S01_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""S01_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRateApplicabilityUOM"" Data=""true"">
            <Key>
                <PropertyRef Name=""S02_UOM"" />
            </Key>
            <Property Name=""S02_UOM"" Type=""varchar"" MaxLength=""10"" />
        </EntityType>
    </Schema>
    <OriginalSchema>
        <EntityType Name=""RefCusTariff"">
            <Key>
                <PropertyRef Name=""ZZ1_TariffCode"" />
                <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRate"" Type=""RefCusRate"" />
            <Property Name=""ZZ1_TariffCode"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRate"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusApplicability"" />
                <PropertyRef Name=""RefCusRateUOM"" />
                <PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
                <PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
                <PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
            <Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
            <Property Name=""ZZ2_EndDate"" Type=""datetime"" />
            <Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
            <Property Name=""ZZ2_StartDate"" Type=""datetime"" />
            <Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" />
            <Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" />
            <Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""ZZT_AdditionalCode"" />
                <PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
            <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
            <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
            <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRateUOM"" Data=""true"">
            <Key>
                <PropertyRef Name=""ZXG_UOM"" />
            </Key>
            <Property Name=""ZXG_UOM"" Type=""varchar"" />
        </EntityType>
    </OriginalSchema>
</UniversalReferenceData>";

		const string GetNextDataBatch_WithTransform_RefCusApplicability_WithExcludedTradeGroupNew_SourceDataContentText = @"
<UniversalReferenceData>
    <DataSource>Test</DataSource>
    <PublicationTime>2022-01-18T13:31:25</PublicationTime>
    <UpdateType>Full</UpdateType>
    <Schema>
        <EntityType Name=""RefCusTariff"">
            <Key>
                <PropertyRef Name=""RefCusRateApplicability"" />
                <PropertyRef Name=""ZZ1_TariffCode"" />
                <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRateApplicability"" Type=""RefCusRateApplicability"" />
            <Property Name=""ZZ1_TariffCode"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRateApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusExcludedTradeGroupNew"" />
                <PropertyRef Name=""S01_ZY1_NKRateCode"" />
                <PropertyRef Name=""S01_ZY1_ZZR_NKRateType"" />
                <PropertyRef Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""S01_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""S01_AdditionalCode"" />
                <PropertyRef Name=""S01_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
            <Property Name=""S01_EndDate"" Type=""datetime"" />
            <Property Name=""S01_RateFormula"" Type=""varchar"" />
            <Property Name=""S01_StartDate"" Type=""datetime"" />
            <Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" />
            <Property Name=""S01_ZY1_ZZR_NKRateType"" Type=""varchar"" />
            <Property Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""S01_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
            <Property Name=""S01_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""S01_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusExcludedTradeGroupNew"" Data=""true"">
            <Key>
                <PropertyRef Name=""S03_ZZA_NKTradeGroup"" />
                <PropertyRef Name=""S03_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""S03_ZZA_NKTradeGroup"" Type=""nvarchar""/>
            <Property Name=""S03_ZZA_ZZZ_NKDataGrouping"" Type=""nvarchar""/>
        </EntityType>
    </Schema>
    <OriginalSchema>
        <EntityType Name=""RefCusTariff"">
            <Key>
                <PropertyRef Name=""ZZ1_TariffCode"" />
                <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRate"" Type=""RefCusRate"" />
            <Property Name=""ZZ1_TariffCode"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRate"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusApplicability"" />
                <PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
                <PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
                <PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
            <Property Name=""ZZ2_EndDate"" Type=""datetime"" />
            <Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
            <Property Name=""ZZ2_StartDate"" Type=""datetime"" />
            <Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" />
            <Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" />
            <Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar""/>
            <Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusExcludedTradeGroup"" />
                <PropertyRef Name=""ZZT_AdditionalCode"" />
                <PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
            <Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
            <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
            <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
            <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
            <Key>
                <PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
                <PropertyRef Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
    </OriginalSchema>
</UniversalReferenceData>";

		const string GetNextDataBatch_WithTransform_RefCusApplicability_WithUOMAndExcludedTradeGroupNew_SourceDataContentText = @"
<UniversalReferenceData>
    <DataSource>Test For Merger query Data</DataSource>
    <PublicationTime>2022-01-18T13:31:25</PublicationTime>
    <UpdateType>Full</UpdateType>
    <Schema>
        <EntityType Name=""RefCusTariff"">
            <Key>
                <PropertyRef Name=""ZZ1_TariffCode"" />
                <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRateApplicability"" Type=""RefCusRateApplicability"" />
            <Property Name=""ZZ1_TariffCode"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRateApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusRateApplicabilityUOM"" />
                <PropertyRef Name=""S01_ZY1_NKRateCode"" />
                <PropertyRef Name=""S01_ZY1_ZZR_NKRateType"" />
                <PropertyRef Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""S01_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""RefCusExcludedTradeGroupNew"" />
                <PropertyRef Name=""S01_AdditionalCode"" />
                <PropertyRef Name=""S01_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRateApplicabilityUOM"" Type=""RefCusRateApplicabilityUOM"" />
            <Property Name=""S01_EndDate"" Type=""datetime"" />
            <Property Name=""S01_RateFormula"" Type=""varchar"" />
            <Property Name=""S01_StartDate"" Type=""datetime"" />
            <Property Name=""S01_ZY1_NKRateCode"" Type=""varchar"" />
            <Property Name=""S01_ZY1_ZZR_NKRateType"" Type=""varchar"" />
            <Property Name=""S01_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""S01_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
            <Property Name=""S01_AdditionalCode"" Type=""nvarchar"" />
            <Property Name=""S01_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""S01_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRateApplicabilityUOM"" Data=""true"">
            <Key>
                <PropertyRef Name=""S02_UOM"" />
            </Key>
            <Property Name=""S02_UOM"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusExcludedTradeGroupNew"" Data=""true"">
            <Key>
                <PropertyRef Name=""S03_ZZA_NKTradeGroup"" />
                <PropertyRef Name=""S03_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""S03_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""S03_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
    </Schema>
    <OriginalSchema>
        <EntityType Name=""RefCusTariff"">
            <Key>
                <PropertyRef Name=""ZZ1_TariffCode"" />
                <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusRate"" Type=""RefCusRate"" />
            <Property Name=""ZZ1_TariffCode"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRate"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusApplicability"" />
                <PropertyRef Name=""RefCusRateUOM"" />
                <PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
                <PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
                <PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
                <PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
            <Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
            <Property Name=""ZZ2_EndDate"" Type=""datetime"" />
            <Property Name=""ZZ2_RateFormula"" Type=""varchar"" />
            <Property Name=""ZZ2_StartDate"" Type=""datetime"" />
            <Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" />
            <Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" />
            <Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" />
            <Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusExcludedTradeGroup"" />
                <PropertyRef Name=""ZZT_AdditionalCode"" />
                <PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
            <Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" />
            <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
            <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
            <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusRateUOM"" Data=""true"">
            <Key>
                <PropertyRef Name=""ZXG_UOM"" />
            </Key>
            <Property Name=""ZXG_UOM"" Type=""varchar"" />
        </EntityType>
        <EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
            <Key>
                <PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
                <PropertyRef Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" />
            </Key>
            <Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""varchar"" />
            <Property Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" />
        </EntityType>
    </OriginalSchema>
</UniversalReferenceData>";
	}
}
