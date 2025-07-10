using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class UNDGSubstanceADNServiceFixture
	{
		static string TblPrefix => "ADN";
		static short DataSetId => Helper.GetDataSetId(DataSet.UNDGSubstanceADN);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new UNDGSubstanceADN { ADN_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ADN_PSN = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ADN_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new UNDGSubstanceADN { ADN_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ADN_PSN = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ADN_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ADN_PSN).ToArray(), Is.EqualTo(expected));
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
			Assert.That(dataSets.Select(x => x.ADN_PSN).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Attribute_Data()
		{
			var substance = CreateSubstance(Now.AddDays(1));
			var attr = repo.Create(() => new UNDGAttributeZZ
			{
				DAZ_Type = "AA",
				DAZ_ParentPK = substance.ADN_PK,
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
			var substance = repo.Create(() => new UNDGSubstanceADN
			{
				ADN_PK = Guid.NewGuid(),
				ADN_BlueCones = 1,
				ADN_CarriagePermittedBulk = false,
				ADN_Class = "a",
				ADN_ClassificationCode = "b",
				ADN_ExceptedQuantityCode = "c",
				ADN_Labels = "d",
				ADN_LQ2MaxAmt = 0,
				ADN_LQ2MaxAmtUQ = "e",
				ADN_LQMaxAmt = 0,
				ADN_LQMaxAmtUQ = "f",
				ADN_PG = "g",
				ADN_PSN = "ORGANOTIN PESTICIDE, LIQUID, TOXIC, FLAMMABLE",
				ADN_SpecialProvisions = "h",
				ADN_UNNO = "3019",
				ADN_Variant = "c",
				ADN_CarriagePermittedDetails = "i",
				ADN_CarriagePermittedPacks = false,
				ADN_CarriagePermittedTanks = false,
				ADN_EquipBreathingApparatus = false,
				ADN_EquipEscapeDevice = false,
				ADN_EquipGasDetector = false,
				ADN_EquipmentDetails = "j",
				ADN_EquipPPE = false,
				ADN_EquipToximeter = false,
				ADN_LoadingSpecialProv = "k",
				ADN_LoadingSpecialProvNote = "l",
				ADN_OperationSpecialProv = "m",
				ADN_OperationSpecialProvNote = "n",
				ADN_UnloadingSpecialProv = "o",
				ADN_UnloadingSpecialProvNote = "p",
				ADN_Ventilation = "q",
				ADN_IsActive = true
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = substance.ADN_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ADN_BlueCones, Is.EqualTo(substance.ADN_BlueCones));
			Assert.That(result.ADN_CarriagePermittedBulk, Is.EqualTo(substance.ADN_CarriagePermittedBulk));
			Assert.That(result.ADN_CarriagePermittedDetails, Is.EqualTo(substance.ADN_CarriagePermittedDetails));
			Assert.That(result.ADN_CarriagePermittedPacks, Is.EqualTo(substance.ADN_CarriagePermittedPacks));
			Assert.That(result.ADN_CarriagePermittedTanks, Is.EqualTo(substance.ADN_CarriagePermittedTanks));
			Assert.That(result.ADN_Class, Is.EqualTo(substance.ADN_Class));
			Assert.That(result.ADN_ClassificationCode, Is.EqualTo(substance.ADN_ClassificationCode));
			Assert.That(result.ADN_EquipBreathingApparatus, Is.EqualTo(substance.ADN_EquipBreathingApparatus));
			Assert.That(result.ADN_EquipEscapeDevice, Is.EqualTo(substance.ADN_EquipEscapeDevice));
			Assert.That(result.ADN_EquipGasDetector, Is.EqualTo(substance.ADN_EquipGasDetector));
			Assert.That(result.ADN_EquipmentDetails, Is.EqualTo(substance.ADN_EquipmentDetails));
			Assert.That(result.ADN_EquipPPE, Is.EqualTo(substance.ADN_EquipPPE));
			Assert.That(result.ADN_EquipToximeter, Is.EqualTo(substance.ADN_EquipToximeter));
			Assert.That(result.ADN_ExceptedQuantityCode, Is.EqualTo(substance.ADN_ExceptedQuantityCode));
			Assert.That(result.ADN_Labels, Is.EqualTo(substance.ADN_Labels));

			Assert.That(result.ADN_LoadingSpecialProv, Is.EqualTo(substance.ADN_LoadingSpecialProv));
			Assert.That(result.ADN_LoadingSpecialProvNote, Is.EqualTo(substance.ADN_LoadingSpecialProvNote));
			Assert.That(result.ADN_LQ2MaxAmt, Is.EqualTo(substance.ADN_LQ2MaxAmt));
			Assert.That(result.ADN_LQ2MaxAmtUQ, Is.EqualTo(substance.ADN_LQ2MaxAmtUQ));
			Assert.That(result.ADN_LQMaxAmt, Is.EqualTo(substance.ADN_LQMaxAmt));
			Assert.That(result.ADN_LQMaxAmtUQ, Is.EqualTo(substance.ADN_LQMaxAmtUQ));
			Assert.That(result.ADN_OperationSpecialProv, Is.EqualTo(substance.ADN_OperationSpecialProv));
			Assert.That(result.ADN_OperationSpecialProvNote, Is.EqualTo(substance.ADN_OperationSpecialProvNote));

			Assert.That(result.ADN_PG, Is.EqualTo(substance.ADN_PG));
			Assert.That(result.ADN_PSN, Is.EqualTo(substance.ADN_PSN));
			Assert.That(result.ADN_SpecialProvisions, Is.EqualTo(substance.ADN_SpecialProvisions));
			Assert.That(result.ADN_UnloadingSpecialProv, Is.EqualTo(substance.ADN_UnloadingSpecialProv));
			Assert.That(result.ADN_UnloadingSpecialProvNote, Is.EqualTo(substance.ADN_UnloadingSpecialProvNote));
			Assert.That(result.ADN_UNNO, Is.EqualTo(substance.ADN_UNNO));
			Assert.That(result.ADN_Variant, Is.EqualTo(substance.ADN_Variant));
			Assert.That(result.ADN_Ventilation, Is.EqualTo(substance.ADN_Ventilation));
			Assert.That(result.ADN_IsActive, Is.EqualTo(substance.ADN_IsActive));
		}

		UNDGSubstanceADN CreateSubstance(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new UNDGSubstanceADN { ADN_PK = Guid.NewGuid(), ADN_PSN = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ADN_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.UNDGSubstanceADN> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.UNDGSubstanceADN> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new UNDGSubstanceADNService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ADN_PSN);
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
