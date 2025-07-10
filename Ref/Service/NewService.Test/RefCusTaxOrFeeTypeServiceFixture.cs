using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusTaxOrFeeTypeServiceFixture
	{
		static string TblPrefix => "ZX0";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusTaxOrFeeType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusTaxOrFeeType { ZX0_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZX0_Description = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusTaxOrFeeType { ZX0_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZX0_Description = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			var result = dataSets.Select(x => x.ZX0_Description).ToArray();
			Assert.That(dataSets.Select(x => x.ZX0_Description).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Procedure_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateTaxCode(Now, "AA");
			var p2 = CreateTaxCode(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZX0_Description).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Procedure_Data()
		{
			var taxType = repo.Create(() => new RefCusTaxOrFeeType
			{
				ZX0_PK = Guid.NewGuid(),
				ZX0_Description = "AA",
				ZX0_TaxOrFeeType = "TYP",
			});
			var tax = repo.Create(() => new RefCusTaxOrFee
			{
				ZZF_PK = Guid.NewGuid(),
				ZZF_Code = "ANY",
				ZZF_ZX0_NKTaxOrFeeType = "TYP",
				ZZF_Value = 1,
				ZZF_Description = "DES",
				ZZF_StartDate = Now.AddDays(-1),
				ZZF_EndDate = Now.AddDays(1),
				ZZF_Maximum = 2,
				ZZF_Minimum = 1
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = taxType.ZX0_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZX0_Description, Is.EqualTo(taxType.ZX0_Description));
			Assert.That(result.ZX0_TaxOrFeeType, Is.EqualTo(taxType.ZX0_TaxOrFeeType));

			var taxesResult = dataSets.ElementAt(0).RefCusTaxOrFees;
			Assert.That(taxesResult != null && taxesResult.Length > 0);
			Assert.That(taxesResult.ElementAt(0) != null);
			Assert.That(taxesResult.ElementAt(0).ZZF_Code, Is.EqualTo("ANY"));
			Assert.That(taxesResult.ElementAt(0).ZZF_Value, Is.EqualTo(1));
		}

		RefCusTaxOrFeeType CreateTaxCode(DateTime dateTime, string description = null)
		{
			var result = repo.Create(() => new RefCusTaxOrFeeType { ZX0_PK = Guid.NewGuid(), ZX0_Description = description });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusTaxOrFeeType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusTaxOrFeeType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusTaxOrFeeTypeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(o => o.ZX0_Description);
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
