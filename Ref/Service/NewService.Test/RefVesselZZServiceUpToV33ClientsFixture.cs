using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefVesselZZServiceUpToV33ClientsFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "ZZO";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefVesselZZ);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefVesselZZ { ZZO_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZO_Code = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZO_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefVesselZZ { ZZO_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZO_Code = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZO_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZO_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefVesselZZServiceUpToV33()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefDataGrouping
				{
					ZZZ_PK = new Guid("D7F07DDE-CA74-41D8-9617-7AD4F02D5C92"),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "AU DataGrouping",
					ZZZ_ZZZ_NKGrouping = "AU"
				});
				await repository.SaveChangesAsync();

				repository.Add(new RefVesselZZ
				{
					ZZO_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					ZZO_Code = "BB",
					ZZO_ZZZ_NKDataGrouping = "AU",
					ZZO_RadioCallSign = "VRBI8",
					ZZO_VesselType = "CV",
					ZZO_RN_NKCountryOfReg = "",
					ZZO_LloydsNumber = "",
				});
				await repository.SaveChangesAsync();

				var service = new RefVesselZZService(repository);
				var refVesselZZEnumerable = service.GetDataUpToV33(null, Now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), null, DataSetId).OrderBy(x => x.ZZO_Code);
				Assert.DoesNotThrow(() =>
				{
					var refVesselZZResults = refVesselZZEnumerable.ToArray();
					Assert.NotNull(refVesselZZResults);
				});
			}
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Vessel_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateVessel(Now, "AA", "AA1", "ZA");
			var p2 = CreateVessel(Now.AddDays(2), "BB", "BB1", "ZA");
			var p3 = CreateVessel(Now.AddDays(2), "BB", "BB2", "ZA");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZO_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 1, new[] { "AA1" })]
		public void GetLatest_Vessel_FilterForVersion33EdgeCaseMaxPkRecordNotSent(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateVessel(Now, "AA", "AA1", "ZA");
			var p2 = CreateVessel(Now.AddDays(-2), "BB", "BB1", "ZA", "7D82C85C-E40C-46B3-9176-CA4DDD399C85");
			var p3 = CreateVessel(Now, "BB", "BB2", "ZA", "6D82C85C-E40C-46B3-9176-CA4DDD399C85");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZO_RadioCallSign).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 1, new[] { "AA1", "BB1" })]
		public void GetLatest_Vessel_FilterForVersion33EdgeCaseNewRecordIsMaxPkAndSent(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateVessel(Now, "AA", "AA1", "ZA");
			var p2 = CreateVessel(Now, "BB", "BB1", "ZA", "7D82C85C-E40C-46B3-9176-CA4DDD399C85");
			var p3 = CreateVessel(Now.AddDays(-2), "BB", "BB2", "ZA", "6D82C85C-E40C-46B3-9176-CA4DDD399C85");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZO_RadioCallSign).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Vessel_Data()
		{
			var p = CreateVessel(Now.AddDays(1), "AA", "AA1");
			p.ZZO_LloydsNumber = "123";
			p.ZZO_RadioCallSign = "34";
			p.ZZO_RN_NKCountryOfReg = "AU";
			p.ZZO_VesselType = "T";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZO_LloydsNumber, Is.EqualTo("123"));
			Assert.That(result.ZZO_NetRegisterTon, Is.EqualTo(0));
			Assert.That(result.ZZO_RadioCallSign, Is.EqualTo("34"));
			Assert.That(result.ZZO_RN_NKCountryOfReg, Is.EqualTo("AU"));
			Assert.That(result.ZZO_VesselType, Is.EqualTo("T"));
			Assert.That(result.ZZO_YearOfConstruction, Is.EqualTo(0));
		}

		RefVesselZZ CreateVessel(DateTime dateTime, string code = null, string radioCallSign = null, string dataGrouping = null, string zZO_PKstring = null)
		{
			var result = repo.Create(() => new RefVesselZZ { ZZO_PK = zZO_PKstring == null ? Guid.NewGuid() : new Guid(zZO_PKstring), ZZO_Code = code, ZZO_ZZZ_NKDataGrouping = dataGrouping, ZZO_RadioCallSign = radioCallSign });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZO_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefVesselZZ> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefVesselZZ> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefVesselZZService(repo);
			return service.GetDataUpToV33(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZO_Code);
		}

		DateTime Now;
		ObjectReferenceDataRepository repo;
		[SetUp]
		public void SetUp()
		{
			Now = DateTime.UtcNow;
			repo = new ObjectReferenceDataRepository();
		}
	}
}
