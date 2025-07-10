using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class StagingDataProviderIntergrationFixture
	{
		[Test]
		public async Task GetSourceData_Intergration()
		{
			var source = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = DataSourceConstants.Source.InternalWebsite,
				SDA_Filetype = DataSourceConstants.FileType.XML.ToString(),
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_Status = StatusProvider.GetPRSStatus(),
				SDA_SubSource = "BB",
				SDA_ContentText = string.Empty
			};
			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(source);
				await repo.SaveChangesAsync();
				using (var provider = new StagingDataProvider(repo))
				{
					var results = provider.GetSourceData().ToArray();
					Assert.Contains(source, results);
				}
			}
		}

		[Test]
		public async Task UpdateDescendentDPRWithExpiredAncestor()
		{
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "AA",
				SDA_Filetype = "TXT",
				SDA_Status = "PRS",
				SDA_SubSource = "BB",
				SDA_ContentText = "",
				SDA_SourceTime = new DateTime(2020, 04, 01)
			};
			var processingResult1 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "ZZ1",
				DPR_PublicationTime = new DateTime(2020, 04, 01),
				DPR_Status = "PRS"
			};
			var processingResult2 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "ZZ1",
				DPR_PublicationTime = new DateTime(2020, 03, 01),
				DPR_Status = "PRS",
				DPR_ExpirationTime = new DateTime(2020, 04, 01)
			};
			var processingResult3 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "ZZ8",
				DPR_PublicationTime = new DateTime(2020, 03, 01),
				DPR_ExpirableAncestorPK = processingResult1.DPR_ParentPK,
				DPR_Status = "QUE"
			};
			var processingResult4 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "ZZ8",
				DPR_PublicationTime = new DateTime(2020, 03, 01),
				DPR_ExpirableAncestorPK = processingResult2.DPR_ParentPK,
				DPR_Status = "QUE"
			};
			var processingResult5 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "ZZ1",
				DPR_PublicationTime = new DateTime(2020, 03, 01),
				DPR_Status = "PRS",
				DPR_ExpirationTime = new DateTime(2020, 05, 01)
			};
			var processingResult6 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "ZZ8",
				DPR_PublicationTime = new DateTime(2020, 03, 01),
				DPR_ExpirableAncestorPK = processingResult5.DPR_ParentPK,
				DPR_Status = "QUE"
			};
			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(processingResult1);
				repo.Add(processingResult2);
				repo.Add(processingResult3);
				repo.Add(processingResult4);
				repo.Add(processingResult5);
				repo.Add(processingResult6);
				await repo.SaveChangesAsync();
				using (var provider = new StagingDataProvider(repo))
				{
					await provider.UpdateDescendentDPRWithExpiredAncestor("ZZ8", sourceData);
				}
			}
			using (var repo = new StagingRepository(connectionString))
			{
				var results = repo.Get<DataProcessingResult>().Where(x => x.DPR_PK == processingResult4.DPR_PK).ToArray();
				Assert.AreEqual(1, results.Length);
				Assert.AreEqual("PRS", results[0].DPR_Status);
				Assert.AreEqual(new DateTime(2020, 04, 01), results[0].DPR_ExpirationTime);

				var processingResults = repo.Get<DataProcessingResult>().Where(x => x.DPR_PK == processingResult6.DPR_PK).ToArray();
				Assert.AreEqual(1, processingResults.Length);
				Assert.AreEqual("PRS", processingResults[0].DPR_Status);
				Assert.AreEqual(new DateTime(2020, 05, 01), processingResults[0].DPR_ExpirationTime);
			}
		}

		[Test]
		public async Task MarkDataProcessingInformationAsIngored()
		{
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "AA",
				SDA_Filetype = "TXT",
				SDA_Status = "PRS",
				SDA_SubSource = "BB",
				SDA_ContentText = ""
			};

			var info1 = new DataProcessingInformation { DPI_ID = Guid.NewGuid(), DPI_SourceId = sourceData.SDA_PK, DPI_Status = "PRS", DPI_ParentTableCode = "ZZ1" };
			var info2 = new DataProcessingInformation { DPI_ID = Guid.NewGuid(), DPI_SourceId = sourceData.SDA_PK, DPI_Status = "QUE", DPI_ParentTableCode = "ZZ1" };
			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(sourceData);
				repo.Add(info1);
				repo.Add(info2);
				await repo.SaveChangesAsync();

				using (var provider = new StagingDataProvider(repo))
				{
					await provider.MarkDataProcessingInformationAsIgnored(sourceData);
				}
			}
			using (var repo = new StagingRepository(connectionString))
			{
				info1 = repo.Get<DataProcessingInformation>().FirstOrDefault(x => x.DPI_ID == info1.DPI_ID);
				Assert.AreEqual("PRS", info1.DPI_Status);
				info2 = repo.Get<DataProcessingInformation>().FirstOrDefault(x => x.DPI_ID == info2.DPI_ID);
				Assert.AreEqual("IGR", info2.DPI_Status);
			}
		}

		[Test]
		public async Task SaveUpdateResults()
		{
			var processingResult1 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2017, 01, 01),
				DPR_Status = "QUE"
			};
			var processingResult2 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2018, 01, 01),
				DPR_Status = "QUE"
			};
			var processingResult3 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2018, 01, 01),
				DPR_Status = "QUE"
			};

			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(processingResult1);
				repo.Add(processingResult2);
				repo.Add(processingResult3);
				await repo.SaveChangesAsync();
			}

			using (var repo = new StagingRepository(connectionString))
			{
				var updaterResult1 = new SafeObjectUpdaterResult { ParentPK = processingResult1.DPR_ParentPK, ParentCode = "GG" };
				var updaterResult2 = new SafeObjectUpdaterResult { ParentPK = processingResult2.DPR_ParentPK, ParentCode = "GG" };
				var updaterResult3 = new SafeObjectUpdaterResult { ParentPK = Guid.NewGuid(), ParentCode = "GG" };

				var sourceData = new SourceData { SDA_Source = "AA", SDA_SubSource = "BB", SDA_SourceTime = new DateTime(2018, 01, 01) };
				using (var provider = new StagingDataProvider(repo))
				{
					await provider.SaveUpdateResults(sourceData, [updaterResult1, updaterResult2, updaterResult3], null);
					await provider.CleanUpOldUpdateResultsAsync(sourceData);
					var processingResults = repo.Get<DataProcessingResult>().ToArray();
					Assert.AreEqual(4, processingResults.Length);
					Assert.AreEqual(4, processingResults.Count(x => x.DPR_PublicationTime == new DateTime(2018, 01, 01)));
				}
			}
		}

		[Test]
		public void EagerLoadStagingEntities()
		{
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "AA",
				SDA_Filetype = "TXT",
				SDA_Status = "PRS",
				SDA_SubSource = "BB",
				SDA_ContentText = ""
			};
			using (var repo = new StagingRepository(connectionString))
			{
				var tariff = new RefCusTariff
				{
					ZZ1_PK = Guid.NewGuid(),
					ZZ1_TariffCode = "811010",
					ZZ1_Description = "Unwrought antimony; powders",
					ZZ1_StartDate = DateTime.Now.AddDays(-1),
					ZZ1_EndDate = DateTime.Now.AddDays(1),
					ZZ1_ZZF_NKTaxOrFeeCode = "VAT",
					ZZ1_ZZZ_NKDataGrouping = "ZA",
					ZZ1_CompositeKeyOnZZ5 = string.Empty,
					ZZ1_ZZI_ZZZ_NKDataGrouping = "ZA"
				};
				var uom = new RefCusTariffUOM
				{
					ZZ8_ZZ1_Tariff = tariff.ZZ1_PK,
					ZZ8_PK = Guid.NewGuid(),
					ZZ8_ZZZ_NKDataGrouping = "ZA",
					ZZ8_Type = "CU1",
					ZZ8_UOM = "KGM"
				};
				var info = new DataProcessingInformation
				{
					DPI_ID = Guid.NewGuid(),
					DPI_SourceId = sourceData.SDA_PK,
					DPI_Status = "QUE",
					DPI_ParentTableCode = "ZZ1",
					DPI_ParentPk = tariff.ZZ1_PK
				};
				repo.Add(sourceData);
				repo.Add(info);
				repo.Add(tariff);
				repo.Add(uom);
				repo.SaveChanges();
			}
			Tuple<object, DataProcessingInformation>[] results = null;
			using (var repo = new StagingRepository(connectionString))
			{
				using (var provider = new StagingDataProvider(repo))
				{
					var metadata = new Mock<IMetadataProvider>();
					metadata.Setup(x => x.GetProperties(typeof(RefCusTariff).Name)).Returns(new[] { nameof(RefCusTariffUOM) });
					results = provider.GetNextDataBatch(sourceData, metadata.Object, null, null).ToArray();
				}
			}
			var resultTariff = (RefCusTariff)results[0].Item1;
			var resultUom = resultTariff.RefCusTariffUOMs.ToArray()[0];
			Assert.NotNull(resultUom);
		}

		[Test]
		public async Task UpdateAllExpiredDataProcessingRecords()
		{
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "AA",
				SDA_Filetype = "TXT",
				SDA_Status = "PRS",
				SDA_SubSource = "BB",
				SDA_ContentText = "",
				SDA_SourceTime = new DateTime(2018, 1, 1)
			};
			var processingResult1 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2017, 01, 01),
				DPR_Status = "QUE"
			};
			var processingResult2 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2018, 01, 01),
				DPR_Status = "QUE"
			};
			var processingResult3 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2016, 01, 01),
				DPR_Status = "QUE",
				DPR_ExpirationTime = new DateTime(2017, 01, 01)
			};
			var processingResult4 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2016, 01, 01),
				DPR_Status = "QUE",
				DPR_ExpirationTime = new DateTime(2019, 01, 01)
			};
			var processingResult5 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2016, 01, 01),
				DPR_Status = "ERR",
				DPR_ExpirationTime = new DateTime(2019, 01, 01)
			};

			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(sourceData);
				repo.Add(processingResult1);
				repo.Add(processingResult2);
				repo.Add(processingResult3);
				repo.Add(processingResult4);
				repo.Add(processingResult5);
				await repo.SaveChangesAsync();
			}

			using (var repo = new StagingRepository(connectionString))
			{
				using (var provider = new StagingDataProvider(repo))
				{
					await provider.UpdateAllExpiredDataProcessingResultAsync(sourceData);
					var processingResult = repo.Get<DataProcessingResult>().FirstOrDefault(x => x.DPR_PK == processingResult2.DPR_PK);
					Assert.AreEqual("QUE", processingResult.DPR_Status);
					processingResult = repo.Get<DataProcessingResult>().FirstOrDefault(x => x.DPR_PK == processingResult1.DPR_PK);
					Assert.AreEqual("PRS", processingResult.DPR_Status);
					Assert.AreEqual(new DateTime(2018, 1, 1), processingResult.DPR_ExpirationTime);
					processingResult = repo.Get<DataProcessingResult>().FirstOrDefault(x => x.DPR_PK == processingResult3.DPR_PK);
					Assert.AreEqual("PRS", processingResult.DPR_Status);
					Assert.AreEqual(new DateTime(2017, 1, 1), processingResult.DPR_ExpirationTime);
					processingResult = repo.Get<DataProcessingResult>().First(x => x.DPR_PK == processingResult4.DPR_PK);
					Assert.AreEqual("PRS", processingResult.DPR_Status);
					Assert.AreEqual(new DateTime(2019, 1, 1), processingResult.DPR_ExpirationTime);
					processingResult = repo.Get<DataProcessingResult>().First(x => x.DPR_PK == processingResult5.DPR_PK);
					Assert.AreEqual("ERR", processingResult.DPR_Status);
					Assert.AreEqual(new DateTime(2019, 1, 1), processingResult.DPR_ExpirationTime);
				}
			}
		}

		[Test]
		public async Task TestUpdateDataProcessingRecordStatusAndExpirationTime()
		{
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "AA",
				SDA_Filetype = "TXT",
				SDA_Status = "PRS",
				SDA_SubSource = "Test Update Status And Expiration",
				SDA_ContentText = "",
				SDA_SourceTime = new DateTime(2018, 1, 1)
			};
			var processingResult1 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "Test Update Status And Expiration",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2022, 01, 01),
				DPR_Status = "QUE"
			};
			var processingResult2 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "Test Update Status And Expiration",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2023, 01, 01),
				DPR_ExpirationTime = new DateTime(2024, 07, 01),
				DPR_Status = "QUE"
			};

			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(processingResult1);
				repo.Add(processingResult2);
				await repo.SaveChangesAsync();
			}

			using (var repo = new StagingRepository(connectionString))
			{
				using (var provider = new StagingDataProvider(repo))
				{
					await provider.UpdateDataProcessingResultStatusAndExpirationTime(DataProcessingStatus.PRS, new DateTime(2024, 01, 01),
						new[] { processingResult1.DPR_ParentPK, processingResult2.DPR_ParentPK }, sourceData);
					var processingResult = repo.Get<DataProcessingResult>().FirstOrDefault(x => x.DPR_PK == processingResult1.DPR_PK);
					Assert.That(processingResult, Is.Not.Null);
					Assert.That(processingResult.DPR_Status, Is.EqualTo(DataProcessingStatus.PRS.ToString()));
					Assert.That(processingResult.DPR_ExpirationTime, Is.EqualTo(new DateTime(2024, 01, 01)));
					processingResult = repo.Get<DataProcessingResult>().FirstOrDefault(x => x.DPR_PK == processingResult2.DPR_PK);
					Assert.That(processingResult, Is.Not.Null);
					Assert.That(processingResult.DPR_Status, Is.EqualTo(DataProcessingStatus.PRS.ToString()));
					Assert.That(processingResult.DPR_ExpirationTime, Is.EqualTo(new DateTime(2024, 07, 01)));
				}
			}
		}

		[Test]
		public async Task TestUpdateDataProcessingRecordExpirationTime()
		{
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "AA",
				SDA_Filetype = "TXT",
				SDA_Status = "PRS",
				SDA_SubSource = "Test Update Expiration Time",
				SDA_ContentText = "",
				SDA_SourceTime = new DateTime(2018, 1, 1)
			};
			var processingResult1 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "Test Update Expiration Time",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2022, 01, 01),
				DPR_Status = "QUE"
			};
			var processingResult2 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "Test Update Expiration Time",
				DPR_ParentPK = Guid.NewGuid(),
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2023, 01, 01),
				DPR_ExpirationTime = new DateTime(2024, 07, 01),
				DPR_Status = "QUE"
			};

			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(processingResult1);
				repo.Add(processingResult2);
				await repo.SaveChangesAsync();
			}

			using (var repo = new StagingRepository(connectionString))
			{
				using (var provider = new StagingDataProvider(repo))
				{
					await provider.UpdateDataProcessingResultExpirationTime(new DateTime(2024, 01, 01),
						new[] { processingResult1.DPR_ParentPK, processingResult2.DPR_ParentPK }, sourceData);
					var processingResult = repo.Get<DataProcessingResult>().FirstOrDefault(x => x.DPR_PK == processingResult1.DPR_PK);
					Assert.That(processingResult, Is.Not.Null);
					Assert.That(processingResult.DPR_ExpirationTime, Is.EqualTo(new DateTime(2024, 01, 01)));
					processingResult = repo.Get<DataProcessingResult>().FirstOrDefault(x => x.DPR_PK == processingResult2.DPR_PK);
					Assert.That(processingResult, Is.Not.Null);
					Assert.That(processingResult.DPR_ExpirationTime, Is.EqualTo(new DateTime(2024, 07, 01)));
				}
			}
		}

		[Test]
		public async Task CleanUpOldUpdateResultsAsync()
		{
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "AA",
				SDA_Filetype = "TXT",
				SDA_Status = "PRS",
				SDA_SubSource = "BB",
				SDA_ContentText = "",
				SDA_SourceTime = new DateTime(2018, 1, 1)
			};
			var pk = Guid.NewGuid();
			var processingResult1 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = pk,
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2016, 01, 01),
				DPR_Status = "QUE"
			};
			var processingResult2 = new DataProcessingResult
			{
				DPR_PK = Guid.NewGuid(),
				DPR_SubSource = "BB",
				DPR_ParentPK = pk,
				DPR_ParentTableCode = "CC",
				DPR_PublicationTime = new DateTime(2017, 01, 01),
				DPR_Status = "ERR",
				DPR_ExpirationTime = new DateTime(2017, 01, 01)
			};

			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(sourceData);
				repo.Add(processingResult1);
				repo.Add(processingResult2);
				await repo.SaveChangesAsync();
			}

			using (var repo = new StagingRepository(connectionString))
			{
				using (var provider = new StagingDataProvider(repo))
				{
					await provider.CleanUpOldUpdateResultsAsync(sourceData);
					var count = repo.Get<DataProcessingResult>().Count(x => x.DPR_ParentTableCode == "CC");
					Assert.AreEqual(1, count);
					Assert.AreEqual("ERR", repo.Get<DataProcessingResult>().First(x => x.DPR_ParentTableCode == "CC").DPR_Status);
				}
			}
		}

		[Test]
		public async Task UpdateEmptyDPIParentPKStatusToErr()
		{
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "AA",
				SDA_Filetype = "TXT",
				SDA_Status = "PRS",
				SDA_SubSource = "BB",
				SDA_ContentText = "",
				SDA_SourceTime = new DateTime(2018, 1, 1)
			};
			var info1 = new DataProcessingInformation { DPI_ID = Guid.NewGuid(), DPI_SourceId = sourceData.SDA_PK, DPI_Status = "QUE", DPI_ParentTableCode = "ZZ1", DPI_ParentPk = null };
			var info2 = new DataProcessingInformation { DPI_ID = Guid.NewGuid(), DPI_SourceId = sourceData.SDA_PK, DPI_Status = "PRS", DPI_ParentTableCode = "ZZ1", DPI_ParentPk = Guid.NewGuid() };
			var info3 = new DataProcessingInformation { DPI_ID = Guid.NewGuid(), DPI_SourceId = sourceData.SDA_PK, DPI_Status = "PRS", DPI_ParentTableCode = "ZZ1", DPI_ParentPk = null };
			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(sourceData);
				repo.Add(info1);
				repo.Add(info2);
				repo.Add(info3);
				await repo.SaveChangesAsync();

				using (var provider = new StagingDataProvider(repo))
				{
					await provider.UpdateEmptyDPIParentPKStatusToErrAsync(sourceData);
				}
			}
			using (var repo = new StagingRepository(connectionString))
			{
				info1 = repo.Get<DataProcessingInformation>().FirstOrDefault(x => x.DPI_ID == info1.DPI_ID);
				Assert.AreEqual("ERR", info1.DPI_Status);
				info2 = repo.Get<DataProcessingInformation>().FirstOrDefault(x => x.DPI_ID == info2.DPI_ID);
				Assert.AreEqual("PRS", info2.DPI_Status);
				info3 = repo.Get<DataProcessingInformation>().FirstOrDefault(x => x.DPI_ID == info3.DPI_ID);
				Assert.AreEqual("PRS", info3.DPI_Status);
			}
		}

		string connectionString = string.Empty;

		[OneTimeSetUp]
		public void SetUp()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			connectionString = TestConnectionString.GetAdmin(dbName);
		}
	}
}
