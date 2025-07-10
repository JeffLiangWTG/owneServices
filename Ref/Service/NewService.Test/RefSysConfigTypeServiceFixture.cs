using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefSysConfigTypeServiceFixture
	{
		static string TblPrefix => "ZRT";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefSysConfigType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BBB", "CCC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CCC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefSysConfigType { ZRT_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZRT_ConfigCode = "BBB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZRT_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefSysConfigType { ZRT_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZRT_ConfigCode = "CCC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZRT_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZRT_ConfigCode).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AAA", "BBB" })]
		[TestCase(1, 3, new[] { "BBB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AAA", "BBB" })]
		[TestCase(null, 1, new[] { "AAA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Ruling_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateSysConfigType("AAA", Now);
			var p2 = CreateSysConfigType("BBB", Now.AddDays(2));
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZRT_ConfigCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_SysConfigType_Data()
		{
			var p = repo.Create(() => new RefSysConfigType
			{
				ZRT_PK = Guid.NewGuid(),
				ZRT_ConfigCode = "TST",
				ZRT_Description = "Description",
				ZRT_LongDescription = "LongDescription"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZRT_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZRT_ConfigCode, Is.EqualTo(p.ZRT_ConfigCode));
			Assert.That(result.ZRT_Description, Is.EqualTo(p.ZRT_Description));
			Assert.That(result.ZRT_LongDescription, Is.EqualTo(p.ZRT_LongDescription));
		}

		[Test]
		public void GetLatest_SysConfig_Data()
		{
			var configType = CreateSysConfigType("TST", DateTime.UtcNow.AddDays(1));
			var config = repo.Create(() => new RefSysConfig
			{
				ZRC_PK = Guid.NewGuid(),
				ZRC_ZRT_NKConfigCode = configType.ZRT_ConfigCode,
				ZRC_BitValue = false,
				ZRC_DecimalValue = 0,
				ZRC_StringValue = "StrVal",
				ZRC_StartDate = DateTime.MinValue,
				ZRC_EndDate = DateTime.MaxValue
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefSysConfigs.ElementAt(0);
			Assert.That(1, Is.EqualTo(dataSets.ElementAt(0).RefSysConfigs.Length));
			Assert.That(result.ZRC_StringValue, Is.EqualTo(config.ZRC_StringValue));
			Assert.That(result.ZRC_DecimalValue, Is.EqualTo(config.ZRC_DecimalValue));
			Assert.That(result.ZRC_BitValue, Is.EqualTo(config.ZRC_BitValue));
			Assert.That(result.ZRC_StartDate, Is.EqualTo(config.ZRC_StartDate));
			Assert.That(result.ZRC_EndDate, Is.EqualTo(config.ZRC_EndDate));
		}

		RefSysConfigType CreateSysConfigType(string code, DateTime dateTime)
		{
			var result = repo.Create(() => new RefSysConfigType { ZRT_PK = Guid.NewGuid(), ZRT_ConfigCode = code, ZRT_Description = "Description", ZRT_LongDescription = "LongDescription" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZRT_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefSysConfigType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefSysConfigType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefSysConfigTypeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZRT_ConfigCode);
		}

		static void SetUpRepo<T>(Mock<ObjectReferenceDataRepository> repoMock, params T[] objs) where T : class
		{
			repoMock.Setup(x => x.Get<T>()).Returns(objs.AsQueryable());
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
