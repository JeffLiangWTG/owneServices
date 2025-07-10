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
	class RefCarrierCodeServiceUpToVersion33Fixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "ZZ4";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCarrierCode);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var carrier1 = repo.Create(() => new RefCarrierCode { ZZ4_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZ4_Code = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = carrier1.ZZ4_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier1.ZZ4_PK);
			var carrier2 = repo.Create(() => new RefCarrierCode { ZZ4_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZ4_Code = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = carrier2.ZZ4_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier2.ZZ4_PK);
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZ4_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		public async Task GetDataCoreLinqCanBeTranslated_RefCarrierCodeServiceUpToVersion33()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefDataGrouping
				{
					ZZZ_PK = new Guid("766D6B70-8B32-42DB-AEAB-45DCC2578320"),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "AU DataGrouping",
					ZZZ_ZZZ_NKGrouping = "AU"
				});
				repository.Add(new RefDataSetInformation
				{
					RDS_PK = new Guid("881C2C26-A851-4F14-ACF6-CD5E2AC21CD6"),
					RDS_DataSetId = DataSetId,
					RDS_DataSetTableCode = "ZZ4",
					RDS_TableName = "RefCarrierCode",
					RDS_DataSetName = "RefCarrierCode",
					RDS_IsPush = false,
					RDS_PriorityLevel = 1
				});
				repository.Add(new RefDataSetInformation
				{
					RDS_PK = new Guid("88C08F97-21C6-4945-84A1-592EE2EC41C0"),
					RDS_DataSetId = Helper.GetDataSetId(DataSet.RefVesselZZ),
					RDS_DataSetTableCode = "ZZO",
					RDS_TableName = "RefVesselZZ",
					RDS_DataSetName = "RefVesselZZ",
					RDS_IsPush = false,
					RDS_PriorityLevel = 1
				});
				await repository.SaveChangesAsync();

				repository.Add(new RefVesselZZ
				{
					ZZO_PK = new Guid("2BEE0946-2758-4596-924A-7860242B4F93"),
					ZZO_Code = "AA",
					ZZO_ZZZ_NKDataGrouping = "AU",
					ZZO_RN_NKCountryOfReg = "AU",
					ZZO_RadioCallSign = "123"
				});
				await repository.SaveChangesAsync();

				repository.Add(new RefCarrierCode
				{
					ZZ4_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					ZZ4_Code = "BB",
					ZZ4_Description = "test refCarrierCode",
					ZZ4_ZZZ_NKDataGrouping = "AU"
				});
				await repository.SaveChangesAsync();

				repository.Add(new RefCarrierCodeAttribute
				{
					ZZG_PK = new Guid("67DF0F0C-7BAF-4CCB-AC71-870160A925DC"),
					ZZG_Name = "AttName",
					ZZG_Value = "AttValue",
					ZZG_ZZ4_CarrierCode = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7")
				});
				await repository.SaveChangesAsync();

				repository.Add(new RefCarrierVesselPivot
				{
					ZZQ_PK = new Guid("119F97CA-8BCF-40DB-AF5A-B47944ED6D53"),
					ZZQ_ZZ4 = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					ZZQ_ZZO = new Guid("2BEE0946-2758-4596-924A-7860242B4F93")
				});
				await repository.SaveChangesAsync();

				var control1 = repository.Get<RefDbVersionControl>().FirstOrDefault(x => x.RVC_ParentCode == "ZZO");
				var control2 = repository.Get<RefDbVersionControl>().FirstOrDefault(x => x.RVC_ParentCode == "ZZ4");
				control1.RVC_DataSetId = Helper.GetDataSetId(DataSet.RefVesselZZ);
				control1.RVC_LastUpdatedUTC = Now;
				control1.RVC_IsPublished = true;
				control2.RVC_DataSetId = Helper.GetDataSetId(DataSet.RefCarrierCode);
				control2.RVC_LastUpdatedUTC = Now;
				control2.RVC_IsPublished = true;
				repository.Update(control1);
				repository.Update(control2);
				await repository.SaveChangesAsync();

				var service = new RefCarrierCodeService(repository);
				var refCusRulingEnumerable = service.GetDataUpToV33(null, Now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), null, DataSetId).OrderBy(x => x.ZZ4_Code);
				Assert.DoesNotThrow(() =>
				{
					var refCusRulingResults = refCusRulingEnumerable.ToArray();
					Assert.That(refCusRulingResults.Any());
				});
			}
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_CarrierCode_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var carrier1 = CreateCarrier(Now, "AA");
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier1.ZZ4_PK);
			var carrier2 = CreateCarrier(Now.AddDays(2), "BB");
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier2.ZZ4_PK);
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZ4_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_CarrierCodeAttribute_Data()
		{
			var carrier = CreateCarrier(Now.AddDays(1));
			var attr = repo.Create(() => new RefCarrierCodeAttribute
			{
				ZZG_Name = "AA",
				ZZG_ZZ4_CarrierCode = carrier.ZZ4_PK,
				ZZG_Value = "BB"
			});
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCarrierCodeAttributes.ElementAt(0);
			Assert.That(result.ZZG_Name, Is.EqualTo(attr.ZZG_Name));
			Assert.That(result.ZZG_Value, Is.EqualTo(attr.ZZG_Value));
		}

		[Test]
		public void GetLatest_CarrierCode_Data()
		{
			var carrier = CreateCarrier(Now.AddDays(1), "AA");
			carrier.ZZ4_Description = "BB";
			carrier.ZZ4_ZZZ_NKDataGrouping = "AU";
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZ4_Code, Is.EqualTo(carrier.ZZ4_Code));
			Assert.That(result.ZZ4_Description, Is.EqualTo(carrier.ZZ4_Description));
			Assert.That(result.ZZ4_ZZZ_NKDataGrouping, Is.EqualTo(carrier.ZZ4_ZZZ_NKDataGrouping));
		}

		[Test]
		public void GetLatest_Pivot_Data()
		{
			var carrier = CreateCarrier(Now.AddDays(1));
			var pivot = Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			var vessel = repo.Create(() => new RefVesselZZ
			{
				ZZO_PK = new Guid("2F6DFC8A-90B1-47E2-A588-985CB80E34B1"),
				ZZO_Code = "AA",
				ZZO_RN_NKCountryOfReg = "AU",
				ZZO_RadioCallSign = "123"
			});
			pivot.ZZQ_ZZO = vessel.ZZO_PK;
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = DateTime.UtcNow.AddDays(-1), RVC_ParentPK = vessel.ZZO_PK, RVC_DataSetId = Helper.GetDataSetId(DataSet.RefVesselZZ), RVC_ParentCode = "ZZO", RVC_IsPublished = true });

			pivot = Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			vessel = repo.Create(() => new RefVesselZZ
			{
				ZZO_PK = new Guid("3F6DFC8A-90B1-47E2-A588-985CB80E34B2"),
				ZZO_Code = "AA",
				ZZO_RN_NKCountryOfReg = "AU",
				ZZO_RadioCallSign = "456"
			});
			pivot.ZZQ_ZZO = vessel.ZZO_PK;
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = DateTime.UtcNow.AddDays(-1), RVC_ParentPK = vessel.ZZO_PK, RVC_DataSetId = Helper.GetDataSetId(DataSet.RefVesselZZ), RVC_ParentCode = "ZZO", RVC_IsPublished = true });

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCarrierVesselPivots.ElementAt(0).RefVesselZZ;
			Assert.That(result.ZZO_Code, Is.EqualTo("AA"));
			Assert.That(result.ZZO_RN_NKCountryOfReg, Is.EqualTo("AU"));
			Assert.That(result.ZZO_RadioCallSign, Is.EqualTo("456"));
			Assert.That(dataSets.ElementAt(0).RefCarrierVesselPivots.Length, Is.EqualTo(1));
		}

		[Test]
		public void GetLatest_Pivot_DataWithDeletedMaxPkVessel()
		{
			var carrier = CreateCarrier(Now.AddDays(1));
			var pivot = Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			var vessel = repo.Create(() => new RefVesselZZ
			{
				ZZO_PK = new Guid("4F6DFC8A-90B1-47E2-A588-985CB80E34BD"),
				ZZO_Code = "AA",
				ZZO_RN_NKCountryOfReg = "AU",
				ZZO_RadioCallSign = "123"
			});
			pivot.ZZQ_ZZO = vessel.ZZO_PK;
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = vessel.ZZO_PK, RVC_DataSetId = DataSetId, RVC_ParentCode = "ZZO", RVC_IsPublished = true, RVC_Deleted = true });

			pivot = Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			vessel = repo.Create(() => new RefVesselZZ
			{
				ZZO_PK = new Guid("3F6DFC8A-90B1-47E2-A588-985CB80E34BD"),
				ZZO_Code = "AA",
				ZZO_RN_NKCountryOfReg = "AU",
				ZZO_RadioCallSign = "456"
			});
			pivot.ZZQ_ZZO = vessel.ZZO_PK;
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = vessel.ZZO_PK, RVC_DataSetId = DataSetId, RVC_ParentCode = "ZZO", RVC_IsPublished = true });

			var dataSets = GetDataSets(Now);
			Assert.That(dataSets.ElementAt(0).RefCarrierVesselPivots.Length, Is.EqualTo(0));
		}

		RefCarrierCode CreateCarrier(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCarrierCode { ZZ4_PK = Guid.NewGuid(), ZZ4_Code = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZ4_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCarrierCode> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpointPK = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpointPK);
		}

		IEnumerable<Models.RefCarrierCode> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpointPK = null)
		{
			var service = new RefCarrierCodeService(repo);
			return service.GetDataUpToV33(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpointPK, null, DataSetId).OrderBy(x => x.ZZ4_Code);
		}

		T CreateChild<T>(string parentProperty, Guid parentPk, string codeProperty = null, string code = null) where T : new()
		{
			return Helper.CreateChild<T>(repo, parentProperty, parentPk, codeProperty, code);
		}

		DateTime Now;
		ObjectReferenceDataRepository repo;
		[SetUp]
		public void SetUp()
		{
			Now = DateTime.UtcNow;
			repo = new ObjectReferenceDataRepository();
			repo.Create(() => new RefDataSetInformation
			{
				RDS_PK = new Guid("88C08F97-21C6-4945-84A1-592EE2EC41C0"),
				RDS_DataSetId = Helper.GetDataSetId(DataSet.RefVesselZZ),
				RDS_DataSetTableCode = "ZZO",
				RDS_TableName = "RefVesselZZ",
				RDS_DataSetName = "RefVesselZZ",
				RDS_IsPush = false,
				RDS_PriorityLevel = 1
			});
		}
	}
}
