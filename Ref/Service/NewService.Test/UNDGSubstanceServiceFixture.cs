using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class UNDGSubstanceServiceFixture
	{
		static string TblPrefix => "DG";
		static short DataSetId => Helper.GetDataSetId(DataSet.UNDGSubstance);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new UNDGSubstance { DG_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), DG_PSN = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.DG_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new UNDGSubstance { DG_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), DG_PSN = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.DG_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.DG_PSN).ToArray(), Is.EqualTo(expected));
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
			Assert.That(dataSets.Select(x => x.DG_PSN).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Attribute_Data()
		{
			var substance = CreateSubstance(Now.AddDays(1));
			var attr = repo.Create(() => new UNDGAttribute
			{
				DA_Type = "AA",
				DA_DG = substance.DG_PK,
				DA_Descriptor = "BB",
				DA_Index = "1",
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).UNDGAttributes.ElementAt(0);
			Assert.That(result.DA_Type, Is.EqualTo(attr.DA_Type));
			Assert.That(result.DA_Descriptor, Is.EqualTo(attr.DA_Descriptor));
			Assert.That(result.DA_Index, Is.EqualTo(attr.DA_Index));
		}

		[Test]
		public void GetLatest_Reference_Data()
		{
			var substance = CreateSubstance(Now.AddDays(1));
			var refer = repo.Create(() => new UNDGReference
			{
				DR_Code = "AA",
				DR_DG = substance.DG_PK,
				DR_RN_NKCountry = "AU",
				DR_Type = "1",
				DR_Description = "Desc"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).UNDGReferences.ElementAt(0);
			Assert.That(result.DR_Code, Is.EqualTo(refer.DR_Code));
			Assert.That(result.DR_RN_NKCountry, Is.EqualTo(refer.DR_RN_NKCountry));
			Assert.That(result.DR_Type, Is.EqualTo(refer.DR_Type));
			Assert.That(result.DR_Description, Is.EqualTo(refer.DR_Description));
		}

		[Test]
		public void Getlatest_AttributesAndReferences_Count()
		{
			var substance = CreateSubstance(Now.AddDays(1));
			var attr1 = repo.Create(() => new UNDGAttribute
			{
				DA_PK = Guid.NewGuid(),
				DA_Type = "A1",
				DA_DG = substance.DG_PK,
				DA_Descriptor = "B1",
				DA_Index = "1",
			});
			var attr2 = repo.Create(() => new UNDGAttribute
			{
				DA_PK = Guid.NewGuid(),
				DA_Type = "A2",
				DA_DG = substance.DG_PK,
				DA_Descriptor = "B2",
				DA_Index = "2",
			});
			var refer1 = repo.Create(() => new UNDGReference
			{
				DR_PK = Guid.NewGuid(),
				DR_Code = "R1",
				DR_DG = substance.DG_PK,
				DR_RN_NKCountry = "AU",
				DR_Type = "1",
				DR_Description = "Desc1"
			});
			var refer2 = repo.Create(() => new UNDGReference
			{
				DR_PK = Guid.NewGuid(),
				DR_Code = "R2",
				DR_DG = substance.DG_PK,
				DR_RN_NKCountry = "AU",
				DR_Type = "2",
				DR_Description = "Desc2"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.AreEqual(2, result.UNDGAttributes.Length);
			Assert.AreEqual(2, result.UNDGReferences.Length);
		}

		[Test]
		public void GetLatest_Substance_Data()
		{
			var substance = repo.Create(() => new UNDGSubstance
			{
				DG_PK = Guid.NewGuid(),
				DG_Class = "1",
				DG_Variation = "a",
				DG_Variant = "b",
				DG_UsrUSDOTShippingName = "c",
				DG_UNTankIns = "d",
				DG_UNNO = "e",
				DG_UlineEMS = "f",
				DG_TreatAs = "g",
				DG_CodedStow = "i",
				DG_DglPhrase = "j",
				DG_EMS = "k",
				DG_ExceptedQuantityCode = "l",
				DG_ExpLim = "m",
				DG_EXVector = "n",
				DG_FlashPoint = "o",
				DG_IBCIns = "p"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = substance.DG_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.DG_Class, Is.EqualTo(substance.DG_Class));
			Assert.That(result.DG_Variation, Is.EqualTo(substance.DG_Variation));
			Assert.That(result.DG_Variant, Is.EqualTo(substance.DG_Variant));
			Assert.That(result.DG_UsrUSDOTShippingName, Is.EqualTo(substance.DG_UsrUSDOTShippingName));
			Assert.That(result.DG_UNTankIns, Is.EqualTo(substance.DG_UNTankIns));
			Assert.That(result.DG_UNNO, Is.EqualTo(substance.DG_UNNO));
			Assert.That(result.DG_UlineEMS, Is.EqualTo(substance.DG_UlineEMS));
			Assert.That(result.DG_TreatAs, Is.EqualTo(substance.DG_TreatAs));
			Assert.That(result.DG_CodedStow, Is.EqualTo(substance.DG_CodedStow));
			Assert.That(result.DG_DglPhrase, Is.EqualTo(substance.DG_DglPhrase));
			Assert.That(result.DG_EMS, Is.EqualTo(substance.DG_EMS));
			Assert.That(result.DG_ExceptedQuantityCode, Is.EqualTo(substance.DG_ExceptedQuantityCode));
			Assert.That(result.DG_ExpLim, Is.EqualTo(substance.DG_ExpLim));
			Assert.That(result.DG_FlashPoint, Is.EqualTo(substance.DG_FlashPoint));
			Assert.That(result.DG_IBCIns, Is.EqualTo(substance.DG_IBCIns));
		}

		UNDGSubstance CreateSubstance(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new UNDGSubstance { DG_PK = Guid.NewGuid(), DG_PSN = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.DG_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.UNDGSubstance> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.UNDGSubstance> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new UNDGSubstanceService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.DG_PSN);
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
