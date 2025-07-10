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
	class RefCusCodeListServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "ZZD";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusCodeList);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusCodeList { ZZD_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZD_Code = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZD_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			CreateDummyAttrTransportMode(dataset1);
			var dataset2 = repo.Create(() => new RefCusCodeList { ZZD_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZD_Code = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZD_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			CreateDummyAttrTransportMode(dataset2);
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZD_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefCusCodeListService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefDataGrouping
				{
					ZZZ_PK = new Guid("74900788-5100-468A-939C-0078A41220EE"),
					ZZZ_DataGrouping = "AU",
					ZZZ_Description = "AU DataGrouping",
					ZZZ_ZZZ_NKGrouping = "AU"
				});
				repository.Add(new RefCusCodeType
				{
					ZZK_PK = new Guid("E7C95CAB-444B-47DD-8E09-98A271147708"),
					ZZK_CodeType = "CC",
					ZZK_Description = "test cusCodeType",
					ZZK_ZZZ_NKDataGrouping = "AU",
				});
				await repository.SaveChangesAsync();

				repository.Add(new RefCusCodeList
				{
					ZZD_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					ZZD_Code = "BB",
					ZZD_ZZK_NKCodeType = "CC",
					ZZD_Description = "test cusCode description",
					ZZD_ZZZ_NKDataGrouping = "AU",
					ZZD_StartDate = Now,
					ZZD_EndDate = Now.AddDays(1)
				});
				await repository.SaveChangesAsync();

				var service = new RefCusCodeListService(repository);
				var refCusCodeListEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), null, DataSetId).OrderBy(x => x.ZZD_Code);
				Assert.DoesNotThrow(() =>
				{
					var refCusCodeListResults = refCusCodeListEnumerable.ToArray();
					Assert.NotNull(refCusCodeListResults);
				});
			}
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_CodeList_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var codeList1 = CreateCodeList(Now, "AA");
			CreateDummyAttrTransportMode(codeList1);
			var codeList2 = CreateCodeList(Now.AddDays(2), "BB");
			CreateDummyAttrTransportMode(codeList2);
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZD_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_CodeListAttribute_Data()
		{
			var codeList = CreateCodeList(Now.AddDays(1));
			var attr = repo.Create(() => new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = "AA",
				ZZE_ZZD_CodeList = codeList.ZZD_PK,
				ZZE_Value = "BB"
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusCodeListAttributes.ElementAt(0);
			Assert.That(result.ZZE_ZXE_NKName, Is.EqualTo(attr.ZZE_ZXE_NKName));
			Assert.That(result.ZZE_Value, Is.EqualTo(attr.ZZE_Value));
		}

		[Test]
		public void GetLatest_CodeListLanguage_Data()
		{
			var codeList = CreateCodeList(Now.AddDays(1));
			var language = repo.Create(() => new RefCusCodeListLanguage
			{
				ZXA_PK = Guid.NewGuid(),
				ZXA_ZZD_CodeList = codeList.ZZD_PK,
				ZXA_ZX6_NKLanguage = "EN",
				ZXA_Description = "English"
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusCodeListLanguages.ElementAt(0);
			Assert.That(result.ZXA_ZX6_NKLanguage, Is.EqualTo(language.ZXA_ZX6_NKLanguage));
			Assert.That(result.ZXA_Description, Is.EqualTo(language.ZXA_Description));
		}

		[Test]
		public void GetLatest_CodeList_Data()
		{
			var codeList = repo.Create(() => new RefCusCodeList
			{
				ZZD_PK = Guid.NewGuid(),
				ZZD_Code = "AA",
				ZZD_ZZK_NKCodeType = "BB",
				ZZD_Description = "CC",
				ZZD_EndDate = Now.AddDays(2),
				ZZD_ZZZ_NKDataGrouping = "AU",
				ZZD_StartDate = Now
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = codeList.ZZD_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			CreateDummyAttrTransportMode(codeList);

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZD_Code, Is.EqualTo(codeList.ZZD_Code));
			Assert.That(result.ZZD_ZZK_NKCodeType, Is.EqualTo(codeList.ZZD_ZZK_NKCodeType));
			Assert.That(result.ZZD_Description, Is.EqualTo(codeList.ZZD_Description));
			Assert.That(result.ZZD_EndDate, Is.EqualTo(codeList.ZZD_EndDate));
			Assert.That(result.ZZD_ZZZ_NKDataGrouping, Is.EqualTo(codeList.ZZD_ZZZ_NKDataGrouping));
			Assert.That(result.ZZD_StartDate, Is.EqualTo(codeList.ZZD_StartDate));
		}

		[Test]
		public void GetLatest_CodeOrAttributeTransportMode_Data()
		{
			var codeList = CreateCodeList(Now.AddDays(1));
			var attr = repo.Create(() => new RefCusCodeListAttribute
			{
				ZZE_PK = Guid.NewGuid(),
				ZZE_ZXE_NKName = "AA",
				ZZE_ZZD_CodeList = codeList.ZZD_PK,
				ZZE_Value = "BB"
			});
			var tranCode = repo.Create(() => new RefCusCodeOrAttributeTransportMode
			{
				ZZU_PK = Guid.NewGuid(),
				ZZU_TransportMode = "AIR",
				ZZU_ZZD_CodeList = codeList.ZZD_PK,
				ZZU_ZZE_Attribute = null,
				ZZU_DataSetCode = "ZZD",
				ZZU_DataSetPK = codeList.ZZD_PK
			});
			var tranAttr = repo.Create(() => new RefCusCodeOrAttributeTransportMode
			{
				ZZU_PK = Guid.NewGuid(),
				ZZU_TransportMode = "ROA",
				ZZU_ZZD_CodeList = null,
				ZZU_ZZE_Attribute = attr.ZZE_PK,
				ZZU_DataSetCode = "ZZD",
				ZZU_DataSetPK = attr.ZZE_ZZD_CodeList
			});

			var dataSets = GetDataSets(Now);
			var resultCode = dataSets.ElementAt(0).RefCusCodeOrAttributeTransportModes.ElementAt(0);
			Assert.That(resultCode.ZZU_TransportMode, Is.EqualTo("AIR"));
			var resultAttr = dataSets.ElementAt(0).RefCusCodeListAttributes.ElementAt(0).RefCusCodeOrAttributeTransportModes.ElementAt(0);
			Assert.That(resultAttr.ZZU_TransportMode, Is.EqualTo("ROA"));
		}

		RefCusCodeList CreateCodeList(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusCodeList { ZZD_PK = Guid.NewGuid(), ZZD_Code = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZD_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		void CreateDummyAttrTransportMode(RefCusCodeList codeList)
		{
			var attr = repo.Create(() => new RefCusCodeListAttribute
			{
				ZZE_ZXE_NKName = "AA",
				ZZE_ZZD_CodeList = codeList.ZZD_PK,
				ZZE_Value = "BB"
			});
			repo.Create(() => new RefCusCodeOrAttributeTransportMode
			{
				ZZU_TransportMode = "TRN",
				ZZU_ZZE_Attribute = attr.ZZE_PK,
				ZZU_ZZD_CodeList = null
			});
		}

		IEnumerable<Models.RefCusCodeList> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusCodeList> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusCodeListService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZD_Code);
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
