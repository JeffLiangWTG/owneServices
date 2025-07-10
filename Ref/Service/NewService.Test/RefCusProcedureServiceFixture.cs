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
	class RefCusProcedureServiceFixture
	{
		static string TblPrefix => "ZZ6";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusProcedure);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusProcedure { ZZ6_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZ6_ProcedureCode = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZ6_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusProcedure { ZZ6_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZ6_ProcedureCode = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZ6_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZ6_ProcedureCode).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Procedure_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateProcedure(Now, "AA");
			var p2 = CreateProcedure(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZ6_ProcedureCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Procedure_Data()
		{
			var p = repo.Create(() => new RefCusProcedure
			{
				ZZ6_PK = Guid.NewGuid(),
				ZZ6_Category = "AA",
				ZZ6_Concession = "BB",
				ZZ6_Description = "CC",
				ZZ6_PreviousProcedureCode = "D",
				ZZ6_ProcedureCode = "E",
				ZZ6_ZZZ_NKDataGrouping = "AU",
				ZZ6_CalculateDuty = true,
				ZZ6_IsGuaranteeConsumed = "N",
				ZZ6_IsGuaranteeReleased = "Y",
				ZZ6_IntoInwardProcessing = "N",
				ZZ6_OutOfInwardProcessing = "N",
				ZZ6_IntoOutwardProcessing = "I",
				ZZ6_OutofOutwardProcessing = "I",
				ZZ6_IntoTemporaryImport = "Y",
				ZZ6_OutOfTemporaryImport = "Y",
				ZZ6_IntoTemporaryExport = "N",
				ZZ6_OutOfTemporaryExport = "N",
				ZZ6_IsTransit = "I"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZ6_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZ6_Category, Is.EqualTo(p.ZZ6_Category));
			Assert.That(result.ZZ6_Concession, Is.EqualTo(p.ZZ6_Concession));
			Assert.That(result.ZZ6_Description, Is.EqualTo(p.ZZ6_Description));
			Assert.That(result.ZZ6_PreviousProcedureCode, Is.EqualTo(p.ZZ6_PreviousProcedureCode));
			Assert.That(result.ZZ6_ProcedureCode, Is.EqualTo(p.ZZ6_ProcedureCode));
			Assert.That(result.ZZ6_ZZZ_NKDataGrouping, Is.EqualTo(p.ZZ6_ZZZ_NKDataGrouping));
			Assert.That(result.ZZ6_CalculateDuty, Is.EqualTo(p.ZZ6_CalculateDuty));
			Assert.That(result.ZZ6_IsGuaranteeConsumed, Is.EqualTo(p.ZZ6_IsGuaranteeConsumed));
			Assert.That(result.ZZ6_IsGuaranteeReleased, Is.EqualTo(p.ZZ6_IsGuaranteeReleased));
			Assert.That(result.ZZ6_IntoInwardProcessing, Is.EqualTo(p.ZZ6_IntoInwardProcessing));
			Assert.That(result.ZZ6_OutOfInwardProcessing, Is.EqualTo(p.ZZ6_OutOfInwardProcessing));
			Assert.That(result.ZZ6_IntoOutwardProcessing, Is.EqualTo(p.ZZ6_IntoOutwardProcessing));
			Assert.That(result.ZZ6_OutofOutwardProcessing, Is.EqualTo(p.ZZ6_OutofOutwardProcessing));

			Assert.That(result.ZZ6_IntoTemporaryImport, Is.EqualTo(p.ZZ6_IntoTemporaryImport));
			Assert.That(result.ZZ6_OutOfTemporaryImport, Is.EqualTo(p.ZZ6_OutOfTemporaryImport));
			Assert.That(result.ZZ6_IntoTemporaryExport, Is.EqualTo(p.ZZ6_IntoTemporaryExport));
			Assert.That(result.ZZ6_OutOfTemporaryExport, Is.EqualTo(p.ZZ6_OutOfTemporaryExport));
			Assert.That(result.ZZ6_IsTransit, Is.EqualTo(p.ZZ6_IsTransit));
		}

		[Test]
		public void GetLatest_ProcedureAttribute_Data()
		{
			var procedure = CreateProcedure(Now.AddDays(1));
			var procedureAttribute = repo.Create(() => new RefCusProcedureAttribute
			{
				ZXB_PK = Guid.NewGuid(),
				ZXB_ZZ6_ProcedureCode = procedure.ZZ6_PK,
				ZXB_Name = "XX",
				ZXB_Value = "XX"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusProcedureAttributes.ElementAt(0);
			Assert.That(1, Is.EqualTo(dataSets.ElementAt(0).RefCusProcedureAttributes.Length));
			Assert.That(result.ZXB_Name, Is.EqualTo(procedureAttribute.ZXB_Name));
			Assert.That(result.ZXB_Value, Is.EqualTo(procedureAttribute.ZXB_Value));
		}

		[Test]
		public void GetLatest_ProcedureLanguage_Data()
		{
			var procedure = CreateProcedure(Now.AddDays(1));
			var lang = repo.Create(() => new RefCusProcedureLanguage
			{
				ZXV_Description = "A",
				ZXV_ZX6_NKLanguage = "EN",
				ZXV_ZZ6_Procedure = procedure.ZZ6_PK
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusProcedureLanguages.FirstOrDefault();

			Assert.IsNotNull(result);
			Assert.That(result.ZXV_Description, Is.EqualTo(lang.ZXV_Description));
			Assert.That(result.ZXV_ZX6_NKLanguage, Is.EqualTo(lang.ZXV_ZX6_NKLanguage));
		}

		RefCusProcedure CreateProcedure(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusProcedure { ZZ6_PK = Guid.NewGuid(), ZZ6_ProcedureCode = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZ6_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusProcedure> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusProcedure> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusProcedureService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZ6_ProcedureCode);
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
