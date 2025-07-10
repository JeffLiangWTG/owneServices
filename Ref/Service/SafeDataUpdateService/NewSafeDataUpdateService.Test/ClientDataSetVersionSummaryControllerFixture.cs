using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	public class ClientDataSetVersionSummaryControllerFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetClientDataSetVersionSummary_Integration()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var entities = new ReferenceDataRepository(false, GetConnectionString(dbName)))
			{
				entities.Add(new RefDataSetInformation
				{
					RDS_DataSetId = 1,
					RDS_DataSetName = "RefDataGrouping",
					RDS_PK = Guid.NewGuid(),
					RDS_DataSetTableCode = "ZZZ",
					RDS_TableName = "RefDataGrouping",
					RDS_PriorityLevel = 0,
					RDS_LastUpdatedUTC = new DateTime(2022, 03, 01)
				});
				entities.Add(new RefDataSetInformation
				{
					RDS_DataSetId = 900,
					RDS_DataSetName = "SpecialRefDataGrouping",
					RDS_PK = Guid.NewGuid(),
					RDS_DataSetTableCode = "ZZZ",
					RDS_TableName = "RefDataGrouping",
					RDS_PriorityLevel = 1,
					RDS_LastUpdatedUTC = new DateTime(2022, 03, 20)
				});

				await entities.SaveChangesAsync(null);
				var controller = new ClientDataSetVersionSummaryController(entities, new Mock<IClientLicenseInformationProvider>().Object);
				var dataSetTimestamps = controller.GetDataSetsTimestamp();

				var refDataGroupingDataSet = dataSetTimestamps.FirstOrDefault(x => x.Key == "RefDataGrouping");
				Assert.That(refDataGroupingDataSet, Is.Not.Null);
				Assert.That(refDataGroupingDataSet.Value, Is.EqualTo(new DateTime(2022, 03, 01)));

				var specialRefDataGroupingDataSet = dataSetTimestamps.FirstOrDefault(x => x.Key == "SpecialRefDataGrouping");
				Assert.That(specialRefDataGroupingDataSet, Is.Not.Null);
				Assert.That(specialRefDataGroupingDataSet.Value, Is.EqualTo(new DateTime(2022, 03, 20)));
			}
		}

		[Test]
		public void GetDataSetsTimestamp_NonExistentCodes()
		{
			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<RefDbVersionControl>()).Returns(
			new[] { new RefDbVersionControl { RVC_ParentCode = "NONEX1", RVC_IsPublished = true }, new RefDbVersionControl { RVC_ParentCode = "NONEX2", RVC_IsPublished = true } }.AsQueryable());
			var controller = new ClientDataSetVersionSummaryController(repo.Object, new Mock<IClientLicenseInformationProvider>().Object);
			Assert.DoesNotThrow(() => controller.GetDataSetsTimestamp());
		}

		[Test]
		public void GetClientDataSetVersionSummary()
		{
			var version1 = new ClientRefDbVersionControl
			{
				CVC_DataSet = "RefCusCodeType",
				CVC_SystemType = "TST",
				CVC_DataSetTimestamp = DateTime.MinValue,
				CVC_IsInUse = true
			};
			var version2 = new ClientRefDbVersionControl
			{
				CVC_DataSet = "RefCusCodeType",
				CVC_DataSetTimestamp = DateTime.MaxValue,
				CVC_SystemType = "PRD",
				CVC_IsInUse = true
			};
			var version3 = new ClientRefDbVersionControl
			{
				CVC_DataSet = "RefDataGrouping",
				CVC_DataSetTimestamp = DateTime.MaxValue,
				CVC_SystemType = "TST",
				CVC_IsInUse = true
			};
			var version4 = new ClientRefDbVersionControl
			{
				CVC_DataSet = "RefDataGrouping",
				CVC_DataSetTimestamp = DateTime.MaxValue,
				CVC_SystemType = "TST",
				CVC_IsInUse = false
			};

			var repo = new Mock<IReferenceDataRepository>();
			repo.Setup(x => x.Get<ClientRefDbVersionControl>()).Returns(new[] { version1, version2, version3, version4 }.AsQueryable());
			var controller = new ClientDataSetVersionSummaryController(repo.Object, new Mock<IClientLicenseInformationProvider>().Object);
			var r = controller.Get(null).ToArray();
			Assert.That(r.Length, Is.EqualTo(2));
			Assert.That(r[0].NoOfCustomersUpdated, Is.EqualTo(1));
			Assert.That(r[0].NoOfCustomersFailed, Is.EqualTo(1));
			Assert.That(r[0].NoOfProductionCustomersFailed, Is.EqualTo(1));
			Assert.That(r[1].NoOfCustomersUpdated, Is.EqualTo(0));
			Assert.That(r[1].NoOfCustomersFailed, Is.EqualTo(1));
			Assert.That(r[1].NoOfProductionCustomersFailed, Is.EqualTo(0));
		}

		[Test]
		public async Task RemoveInactiveClients()
		{
			var now = DateTimeOffset.UtcNow;
			var repo = new Mock<IReferenceDataRepository>();
			var clientVersion1 = new ClientRefDbVersionControl
			{
				CVC_DataSet = "Set1",
				CVC_ClientId = "ID1",
				CVC_SystemType = "T1"
			};
			var clientVersion2 = new ClientRefDbVersionControl
			{
				CVC_DataSet = "Set2",
				CVC_ClientId = "ID2",
				CVC_SystemType = "T2"
			};
			repo.Setup(x => x.Get<ClientRefDbVersionControl>()).Returns(new[] { clientVersion1, clientVersion2 }.AsQueryable());
			var licenseProvider = new Mock<IClientLicenseInformationProvider>();
			licenseProvider.Setup(x => x.GetLicenceInformationInactive()).Returns(Task.FromResult(new[] { "ID1" }));
			await new ClientDataSetVersionSummaryController(repo.Object, licenseProvider.Object).RemoveInactiveClients();
			repo.Verify(x => x.Delete(clientVersion1));
		}
	}
}
