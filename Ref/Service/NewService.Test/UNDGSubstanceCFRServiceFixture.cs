using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class UNDGSubstanceCFRServiceFixture
	{
		static string TblPrefix => "CFR";
		static short DataSetId => Helper.GetDataSetId(DataSet.UNDGSubstanceCFR);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new UNDGSubstanceCFR { CFR_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), CFR_PSN = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.CFR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new UNDGSubstanceCFR { CFR_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), CFR_PSN = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.CFR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.CFR_PSN).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Substance_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var substance1 = CreateSubstance(Now, "AA");
			var substance2 = CreateSubstance(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.CFR_PSN).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Attribute_Data()
		{
			var substance = CreateSubstance(Now.AddDays(1));
			var attr = repo.Create(() => new UNDGAttributeZZ
			{
				DAZ_Type = "AA",
				DAZ_ParentPK = substance.CFR_PK,
				DAZ_Descriptor = "BB",
				DAZ_Index = "1",
				DAZ_ParentCode = "CFR"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).UNDGAttributeZZs.ElementAt(0);
			Assert.That(result.DAZ_Type, Is.EqualTo(attr.DAZ_Type));
			Assert.That(result.DAZ_Descriptor, Is.EqualTo(attr.DAZ_Descriptor));
			Assert.That(result.DAZ_Index, Is.EqualTo(attr.DAZ_Index));
			Assert.That(result.DAZ_ParentCode, Is.EqualTo(attr.DAZ_ParentCode));
		}

		[Test]
		public void GetLatest_Substance_Data()
		{
			var substance = repo.Create(() => new UNDGSubstanceCFR
			{
				CFR_PK = Guid.NewGuid(),
				CFR_Variant = "b",
				CFR_SpecialProvisions = "c",
				CFR_UNNO = "e",
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = substance.CFR_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.CFR_Variant, Is.EqualTo(substance.CFR_Variant));
			Assert.That(result.CFR_SpecialProvisions, Is.EqualTo(substance.CFR_SpecialProvisions));
			Assert.That(result.CFR_UNNO, Is.EqualTo(substance.CFR_UNNO));
		}

		UNDGSubstanceCFR CreateSubstance(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new UNDGSubstanceCFR { CFR_PK = Guid.NewGuid(), CFR_PSN = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.CFR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.UNDGSubstanceCFR> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.UNDGSubstanceCFR> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new UNDGSubstanceCFRService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.CFR_PSN);
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
