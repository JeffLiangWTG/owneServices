using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class DataSetHeaderIntegrationTest
	{
		[Test]
		[CreateDatabase("B703BEC7DED146578F228CAE99A57A5B", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestGetLastestCreatedTimeUTCLinQCanBeTranslated()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "B703BEC7DED146578F228CAE99A57A5B";
			var createdTime = new DateTime(2024, 10, 11);
			await ArrangeRefCusTariff(dbName, createdTime: createdTime);

			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/RefCusTariffUpdate/GetLastestCreatedTimeUTC()");
				string result;

				Assert.DoesNotThrowAsync(async () =>
				{
					using (var client = factory.CreateClient())
					using (var response = await client.GetAsync(uri))
					{
						response.EnsureSuccessStatusCode();
						result = await response.Content.ReadAsStringAsync();

						var getLastCreatedTimeUtcResponse = JsonConvert.DeserializeObject<RefODataSingleResultResponse<string>>(result);
						Assert.That(DateTimeOffset.Parse(getLastCreatedTimeUtcResponse.Value, CultureInfo.InvariantCulture), Is.EqualTo(new DateTimeOffset(createdTime, TimeSpan.Zero)));
					}
				});
			}
		}

		[TestCase(true, "2024-10-11")]
		[TestCase(false, "0001-01-01")]
		[CreateDatabase("8FC02564026E428A85C356695458D138", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestGetLatestUpdatedTimeUTCLinQCanBeTranslated(bool isPublished, string updatedTimeStr)
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "8FC02564026E428A85C356695458D138";
			var updatedTime = DateTime.Parse(updatedTimeStr, CultureInfo.InvariantCulture);
			await ArrangeRefCusTariff(dbName, updatedTime: updatedTime, isPublished: isPublished);

			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/RefCusTariffUpdate/GetLatestUpdatedTimeUTC()");
				string result;

				Assert.DoesNotThrowAsync(async () =>
				{
					using (var client = factory.CreateClient())
					using (var response = await client.GetAsync(uri))
					{
						response.EnsureSuccessStatusCode();
						result = await response.Content.ReadAsStringAsync();

						var getLastCreatedTimeUtcResponse = JsonConvert.DeserializeObject<RefODataSingleResultResponse<string>>(result);
						Assert.That(DateTimeOffset.Parse(getLastCreatedTimeUtcResponse.Value, CultureInfo.InvariantCulture), Is.EqualTo(new DateTimeOffset(updatedTime, TimeSpan.Zero)));
					}
				});
			}
		}

		[Test]
		[CreateDatabase("531FADF59A444CA6BFDEB782EAE76C34", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestGetCreatedBetweenLinQCanBeTranslated()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "531FADF59A444CA6BFDEB782EAE76C34";
			var createdTime = new DateTime(2024, 10, 11);
			await ArrangeRefCusTariff(dbName, createdTime: createdTime);

			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/RefCusTariffUpdate/Default.GetCreatedBetween(afterCreatedTimeUTC=2024-10-10T00:00:00.0000000Z,beforeOrEqualCreatedTimeUTC=2024-10-12T00:00:00.0000000Z)");
				string result;

				Assert.DoesNotThrowAsync(async () =>
				{
					using (var client = factory.CreateClient())
					using (var response = await client.GetAsync(uri))
					{
						response.EnsureSuccessStatusCode();
						result = await response.Content.ReadAsStringAsync();

						var getCreatedBetweenResponse = JsonConvert.DeserializeObject<RefODataMultipleResultsResponse<RefCusTariff>>(result);
						Assert.That(getCreatedBetweenResponse.Value.Count, Is.EqualTo(1));

						var tariff = getCreatedBetweenResponse.Value.First();
						Assert.That(tariff.ZZ1_TariffCode, Is.EqualTo("CODE2"));
						Assert.That(tariff.ZZ1_IAMUnique, Is.EqualTo(1));
						Assert.That(tariff.ZZ1_Description, Is.EqualTo("Tariff 1"));
					}
				});
			}
		}

		[Test]
		[CreateDatabase("02F6E1CA57AD4A118BF82236DD2C5FB1", DbSchema.RefDbRepoSafe)]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TestGetModifiedBetweenLinQCanBeTranslated()
		{
			const string dbName = CreateDatabaseAttribute.DbNamePrefix + "02F6E1CA57AD4A118BF82236DD2C5FB1";
			var updatedTime = new DateTime(2024, 10, 11);
			await ArrangeRefCusTariff(dbName, updatedTime: updatedTime);

			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			using (var factory = IntegrationTestHelper.WebAppFactory.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll(typeof(IReferenceDataRepository));
					services.AddScoped<IReferenceDataRepository>(sp => repo);
				});
			}))
			{
				var uri = new Uri("http://localhost/odata/RefCusTariffUpdate/Default.GetModifiedBetween(afterModifiedTimeUTC=2024-10-10T00:00:00.0000000Z,beforeOrEqualModifiedTimeUTC=2024-10-12T00:00:00.0000000Z)");
				string result;

				Assert.DoesNotThrowAsync(async () =>
				{
					using (var client = factory.CreateClient())
					using (var response = await client.GetAsync(uri))
					{
						response.EnsureSuccessStatusCode();
						result = await response.Content.ReadAsStringAsync();

						var getModifiedBetweenResponse = JsonConvert.DeserializeObject<RefODataMultipleResultsResponse<RefCusTariff>>(result);
						Assert.That(getModifiedBetweenResponse.Value.Count, Is.EqualTo(1));

						var tariff = getModifiedBetweenResponse.Value.First();
						Assert.That(tariff.ZZ1_TariffCode, Is.EqualTo("CODE2"));
						Assert.That(tariff.ZZ1_IAMUnique, Is.EqualTo(1));
						Assert.That(tariff.ZZ1_Description, Is.EqualTo("Tariff 1"));
					}
				});
			}
		}

		static async Task ArrangeRefCusTariff(string dbName, DateTime? createdTime = null, DateTime? updatedTime = null, bool isPublished = false)
		{
			var tariffPK = Guid.Parse("E6056F93-C8BA-4639-8B31-242C87B1AF25");
			var dataGrouping = new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_DataGrouping = "BR",
				ZZZ_Description = "group"
			};
			var tariffType = new RefCusTariffType
			{
				ZZI_PK = Guid.NewGuid(),
				ZZI_Description = "TariffType",
				ZZI_TariffType = "Type1",
				ZZI_ZZ9_NKNomenclatureGroupType = "ZA",
				ZZI_ZZZ_NKDataGrouping = "BR"
			};
			var tariff = new RefCusTariff
			{
				ZZ1_PK = tariffPK,
				ZZ1_ZZI_TariffType = tariffType.ZZI_PK,
				ZZ1_TariffCode = "CODE2",
				ZZ1_IAMUnique = 1,
				ZZ1_Description = "Tariff 1",
				ZZ1_StartDate = new DateTime(1900, 1, 1),
				ZZ1_EndDate = new DateTime(2000, 1, 1),
				ZZ1_ZZF_NKTaxOrFeeCode = "ABC",
				ZZ1_ZZZ_NKDataGrouping = "BR",
				ZZ1_CompositeKeyOnZZ5 = "AAX"
			};
			var refDataSetInformationRecord = new RefDataSetInformation
			{
				RDS_PK = Guid.NewGuid(),
				RDS_DataSetId = 23,
				RDS_TableName = "RefCusTariff",
				RDS_DataSetName = "RefCusTariff",
				RDS_DataSetTableCode = "ZZ1",
				RDS_PriorityLevel = 0,
				RDS_IsPush = false
			};

			using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				entities.Add(refDataSetInformationRecord);
				entities.Add(dataGrouping);
				entities.Add(tariffType);
				entities.Add(tariff);
				await entities.SaveChangesAsync(null, true);
			}

			if (createdTime.HasValue)
			{
				using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
				{
					var refDbVersionControl = entities.Get<RefDbVersionControl>().First(rvc => rvc.RVC_ParentPK == tariffPK);
					refDbVersionControl.RVC_CreatedTimeUTC = createdTime.Value;
					refDbVersionControl.RVC_IsPublished = isPublished;
					entities.Update(refDbVersionControl);
					await entities.SaveChangesAsync(null, true);
				}
			}

			if (updatedTime.HasValue)
			{
				using (var entities = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
				{
					var refDbVersionControl = entities.Get<RefDbVersionControl>().First(rvc => rvc.RVC_ParentPK == tariffPK);
					refDbVersionControl.RVC_LastUpdatedUTC = updatedTime.Value;
					refDbVersionControl.RVC_IsPublished = isPublished;
					entities.Update(refDbVersionControl);
					await entities.SaveChangesAsync(null, true);
				}
			}
		}
	}
}
