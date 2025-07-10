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
	class UNDGCountryReferenceServiceFixture
	{
		static string TblPrefix => "DCR";
		static short DataSetId => Helper.GetDataSetId(DataSet.UNDGCountryReference);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BBB", "CCC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CCC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new UNDGCountryReference { DCR_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), DCR_Code = "BBB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.DCR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new UNDGCountryReference { DCR_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), DCR_Code = "CCC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.DCR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.DCR_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AAA", "BBB" })]
		[TestCase(1, 3, new[] { "BBB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AAA", "BBB" })]
		[TestCase(null, 1, new[] { "AAA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Ruling_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateCountryReference("AAA", Now);
			var p2 = CreateCountryReference("BBB", Now.AddDays(2));
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.DCR_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_CountryReference_Data()
		{
			var p = repo.Create(() => new UNDGCountryReference
			{
				DCR_PK = Guid.NewGuid(),
				DCR_Code = "TST",
				DCR_Description = "Description",
				DCR_Type = "PSA",
				DCR_HasFlashPointLower = true,
				DCR_HasFlashPointUpper = true,
				DCR_FlashPointLowerCentigrade = 0,
				DCR_FlashPointUpperCentigrade = 0,
				DCR_RN_NKCountry = "ZZ",
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.DCR_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.DCR_Code, Is.EqualTo(p.DCR_Code));
			Assert.That(result.DCR_Description, Is.EqualTo(p.DCR_Description));
			Assert.That(result.DCR_Type, Is.EqualTo(p.DCR_Type));
			Assert.That(result.DCR_HasFlashPointLower, Is.EqualTo(p.DCR_HasFlashPointLower));
			Assert.That(result.DCR_HasFlashPointUpper, Is.EqualTo(p.DCR_HasFlashPointUpper));
			Assert.That(result.DCR_FlashPointLowerCentigrade, Is.EqualTo(p.DCR_FlashPointLowerCentigrade));
			Assert.That(result.DCR_FlashPointUpperCentigrade, Is.EqualTo(p.DCR_FlashPointUpperCentigrade));
			Assert.That(result.DCR_RN_NKCountry, Is.EqualTo(p.DCR_RN_NKCountry));
		}

		[Test]
		public void GetLatest_CountryReferencePivot_Data()
		{
			var reference = CreateCountryReference("TST", DateTime.UtcNow.AddDays(1));
			var pivot = repo.Create(() => new UNDGCountryReferencePivot
			{
				DCP_PK = Guid.NewGuid(),
				DCP_DCR = reference.DCR_PK,
				DCP_Standard = "T",
				DCP_UNNO = "Test",
				DCP_Variant = "V"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).UNDGCountryReferencePivots.ElementAt(0);
			Assert.That(1, Is.EqualTo(dataSets.ElementAt(0).UNDGCountryReferencePivots.Length));
			Assert.That(result.DCP_Standard, Is.EqualTo(pivot.DCP_Standard));
			Assert.That(result.DCP_UNNO, Is.EqualTo(pivot.DCP_UNNO));
			Assert.That(result.DCP_Variant, Is.EqualTo(pivot.DCP_Variant));
		}

		UNDGCountryReference CreateCountryReference(string code, DateTime dateTime)
		{
			var result = repo.Create(() => new UNDGCountryReference { DCR_PK = Guid.NewGuid(), DCR_Code = code, DCR_Description = "Description", DCR_Type = "PSA" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.DCR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.UNDGCountryReference> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.UNDGCountryReference> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new UNDGCountryReferenceService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.DCR_Code);
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
