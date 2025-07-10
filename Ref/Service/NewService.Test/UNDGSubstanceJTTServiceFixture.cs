using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class UNDGSubstanceJTTServiceFixture
	{
		static string TblPrefix => "JTT";
		static short DataSetId => Helper.GetDataSetId(DataSet.UNDGSubstanceADR);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new UNDGSubstanceJTT { JTT_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), JTT_PSN = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.JTT_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new UNDGSubstanceJTT { JTT_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), JTT_PSN = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.JTT_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.JTT_PSN).ToArray(), Is.EqualTo(expected));
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
			Assert.That(dataSets.Select(x => x.JTT_PSN).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Attribute_Data()
		{
			var substance = CreateSubstance(Now.AddDays(1));
			var attr = repo.Create(() => new UNDGAttributeZZ
			{
				DAZ_Type = "AA",
				DAZ_ParentPK = substance.JTT_PK,
				DAZ_Descriptor = "BB",
				DAZ_Index = "1",
				DAZ_ParentCode = TblPrefix
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
			var substance = repo.Create(() => new UNDGSubstanceJTT
			{
				JTT_PK = Guid.NewGuid(),
				JTT_UNNO = "3019",
				JTT_Variant = "a",
				JTT_PSN = "ORGANOTIN PESTICIDE, LIQUID, TOXIC, FLAMMABLE",
				JTT_Class = "1",
				JTT_ClassificationCode = "b",
				JTT_PG = "c",
				JTT_Labels = "d",
				JTT_SpecialProvisions = "e",
				JTT_TransportCategory = "f",
				JTT_HazardIDNumber = "100",
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = substance.JTT_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.JTT_UNNO, Is.EqualTo(substance.JTT_UNNO));
			Assert.That(result.JTT_Variant, Is.EqualTo(substance.JTT_Variant));
			Assert.That(result.JTT_PSN, Is.EqualTo(substance.JTT_PSN));
			Assert.That(result.JTT_Class, Is.EqualTo(substance.JTT_Class));
			Assert.That(result.JTT_ClassificationCode, Is.EqualTo(substance.JTT_ClassificationCode));
			Assert.That(result.JTT_PG, Is.EqualTo(substance.JTT_PG));
			Assert.That(result.JTT_Labels, Is.EqualTo(substance.JTT_Labels));
			Assert.That(result.JTT_SpecialProvisions, Is.EqualTo(substance.JTT_SpecialProvisions));
			Assert.That(result.JTT_TransportCategory, Is.EqualTo(substance.JTT_TransportCategory));
			Assert.That(result.JTT_HazardIDNumber, Is.EqualTo(substance.JTT_HazardIDNumber));
		}

		UNDGSubstanceJTT CreateSubstance(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new UNDGSubstanceJTT { JTT_PK = Guid.NewGuid(), JTT_PSN = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.JTT_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.UNDGSubstanceJTT> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.UNDGSubstanceJTT> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new UNDGSubstanceJTTService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.JTT_PSN);
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
