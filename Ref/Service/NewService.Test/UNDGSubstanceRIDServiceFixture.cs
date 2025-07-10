using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class UNDGSubstanceRIDServiceFixture
	{
		static string TblPrefix => "RID";
		static short DataSetId => Helper.GetDataSetId(DataSet.UNDGSubstanceADR);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new UNDGSubstanceRID { RID_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), RID_PSN = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.RID_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new UNDGSubstanceRID { RID_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), RID_PSN = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.RID_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.RID_PSN).ToArray(), Is.EqualTo(expected));
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
			Assert.That(dataSets.Select(x => x.RID_PSN).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Attribute_Data()
		{
			var substance = CreateSubstance(Now.AddDays(1));
			var attr = repo.Create(() => new UNDGAttributeZZ
			{
				DAZ_Type = "AA",
				DAZ_ParentPK = substance.RID_PK,
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
			var substance = repo.Create(() => new UNDGSubstanceRID
			{
				RID_PK = Guid.NewGuid(),
				RID_BulkContainerTankIns = "a",
				RID_BulkContainerTankProv = "b",
				RID_CarriageBulkSpecialProv = "c",
				RID_CarriagePackagesSpecialProv = "d",
				RID_Class = "e",
				RID_ClassificationCode = "f",
				RID_ColisExpressCode = "g",
				RID_ExceptedQuantityCode = "h",
				RID_HazardIDNumber = "i",
				RID_IBCIns = "j",
				RID_IsActive = true,
				RID_Labels = "a",
				RID_LQ2MaxAmt = 0,
				RID_LQ2MaxAmtUQ = "b",
				RID_LQMaxAmt = 0,
				RID_LQMaxAmtUQ = "c",
				RID_MixedPackProv = "d",
				RID_PackIns = "e",
				RID_PackProv = "f",
				RID_PG = "g",
				RID_PSN = "ORGANOTIN PESTICIDE, LIQUID, TOXIC, FLAMMABLE",
				RID_SpecialProvisions = "i",
				RID_TankCode = "j",
				RID_TankSpecProv = "k",
				RID_TransportCategory = "l",
				RID_UNNO = "3019",
				RID_Variant = "c",
				RID_CarriageLoadingSpecialProv = "a"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = substance.RID_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RID_BulkContainerTankIns, Is.EqualTo(substance.RID_BulkContainerTankIns));
			Assert.That(result.RID_BulkContainerTankProv, Is.EqualTo(substance.RID_BulkContainerTankProv));
			Assert.That(result.RID_CarriageBulkSpecialProv, Is.EqualTo(substance.RID_CarriageBulkSpecialProv));
			Assert.That(result.RID_CarriagePackagesSpecialProv, Is.EqualTo(substance.RID_CarriagePackagesSpecialProv));
			Assert.That(result.RID_Class, Is.EqualTo(substance.RID_Class));
			Assert.That(result.RID_ClassificationCode, Is.EqualTo(substance.RID_ClassificationCode));
			Assert.That(result.RID_ColisExpressCode, Is.EqualTo(substance.RID_ColisExpressCode));
			Assert.That(result.RID_ExceptedQuantityCode, Is.EqualTo(substance.RID_ExceptedQuantityCode));
			Assert.That(result.RID_HazardIDNumber, Is.EqualTo(substance.RID_HazardIDNumber));
			Assert.That(result.RID_IBCIns, Is.EqualTo(substance.RID_IBCIns));
			Assert.That(result.RID_IsActive, Is.EqualTo(substance.RID_IsActive));
			Assert.That(result.RID_Labels, Is.EqualTo(substance.RID_Labels));
			Assert.That(result.RID_LQ2MaxAmt, Is.EqualTo(substance.RID_LQ2MaxAmt));
			Assert.That(result.RID_LQ2MaxAmtUQ, Is.EqualTo(substance.RID_LQ2MaxAmtUQ));
			Assert.That(result.RID_LQMaxAmt, Is.EqualTo(substance.RID_LQMaxAmt));

			Assert.That(result.RID_LQMaxAmtUQ, Is.EqualTo(substance.RID_LQMaxAmtUQ));
			Assert.That(result.RID_MixedPackProv, Is.EqualTo(substance.RID_MixedPackProv));
			Assert.That(result.RID_PackIns, Is.EqualTo(substance.RID_PackIns));
			Assert.That(result.RID_PackProv, Is.EqualTo(substance.RID_PackProv));
			Assert.That(result.RID_PG, Is.EqualTo(substance.RID_PG));
			Assert.That(result.RID_PSN, Is.EqualTo(substance.RID_PSN));
			Assert.That(result.RID_SpecialProvisions, Is.EqualTo(substance.RID_SpecialProvisions));
			Assert.That(result.RID_TankCode, Is.EqualTo(substance.RID_TankCode));

			Assert.That(result.RID_TankSpecProv, Is.EqualTo(substance.RID_TankSpecProv));
			Assert.That(result.RID_TransportCategory, Is.EqualTo(substance.RID_TransportCategory));
			Assert.That(result.RID_UNNO, Is.EqualTo(substance.RID_UNNO));
			Assert.That(result.RID_Variant, Is.EqualTo(substance.RID_Variant));
			Assert.That(result.RID_CarriageLoadingSpecialProv, Is.EqualTo(substance.RID_CarriageLoadingSpecialProv));
		}

		UNDGSubstanceRID CreateSubstance(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new UNDGSubstanceRID { RID_PK = Guid.NewGuid(), RID_PSN = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RID_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.UNDGSubstanceRID> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.UNDGSubstanceRID> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new UNDGSubstanceRIDService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RID_PSN);
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
