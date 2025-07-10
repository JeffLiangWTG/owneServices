using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCarrierCodeServiceFixture
	{
		static string TblPrefix => "ZZ4";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCarrierCode);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var carrier1 = repo.Create(() => new RefCarrierCode { ZZ4_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZ4_Code = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = carrier1.ZZ4_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier1.ZZ4_PK);
			var carrier2 = repo.Create(() => new RefCarrierCode { ZZ4_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZ4_Code = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = carrier2.ZZ4_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier2.ZZ4_PK);
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZ4_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_CarrierCode_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var carrier1 = CreateCarrier(Now, "AA");
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier1.ZZ4_PK);
			var carrier2 = CreateCarrier(Now.AddDays(2), "BB");
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier2.ZZ4_PK);
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZ4_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_CarrierCodeAttribute_Data()
		{
			var carrier = CreateCarrier(Now.AddDays(1));
			var attr = repo.Create(() => new RefCarrierCodeAttribute
			{
				ZZG_Name = "AA",
				ZZG_ZZ4_CarrierCode = carrier.ZZ4_PK,
				ZZG_Value = "BB"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = attr.ZZG_PK, RVC_DataSetId = DataSetId, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCarrierCodeAttributes.ElementAt(0);
			Assert.That(result.ZZG_Name, Is.EqualTo(attr.ZZG_Name));
			Assert.That(result.ZZG_Value, Is.EqualTo(attr.ZZG_Value));
		}

		[Test]
		public void GetLatest_CarrierCode_Data()
		{
			var carrier = CreateCarrier(Now.AddDays(1), "AA");
			carrier.ZZ4_Description = "BB";
			carrier.ZZ4_ZZZ_NKDataGrouping = "AU";
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZ4_Code, Is.EqualTo(carrier.ZZ4_Code));
			Assert.That(result.ZZ4_Description, Is.EqualTo(carrier.ZZ4_Description));
			Assert.That(result.ZZ4_ZZZ_NKDataGrouping, Is.EqualTo(carrier.ZZ4_ZZZ_NKDataGrouping));
		}

		[Test]
		public void GetLatest_Pivot_Data()
		{
			var carrier = CreateCarrier(Now.AddDays(1));
			var pivot = Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			var vessel = repo.Create(() => new RefVesselZZ
			{
				ZZO_PK = Guid.NewGuid(),
				ZZO_Code = "AA",
				ZZO_RN_NKCountryOfReg = "AU"
			});
			pivot.ZZQ_ZZO = vessel.ZZO_PK;
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = vessel.ZZO_PK, RVC_DataSetId = DataSetId, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCarrierVesselPivots.ElementAt(0).RefVesselZZ;
			Assert.That(result.ZZO_Code, Is.EqualTo("AA"));
			Assert.That(result.ZZO_RN_NKCountryOfReg, Is.EqualTo("AU"));
		}

		[Test]
		public void GetLatest_CodeLanguage_Data()
		{
			var carrier = CreateCarrier(Now.AddDays(1));
			var attr = repo.Create(() => new RefCarrierCodeLanguage
			{
				ZCL_PK = Guid.NewGuid(),
				ZCL_Description = "AAA",
				ZCL_ZX6_NKLanguage = "ABC",
				ZCL_ZZ4_CarrierCode = carrier.ZZ4_PK,
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = attr.ZCL_PK, RVC_DataSetId = DataSetId, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefCarrierVesselPivot>(repo, nameof(RefCarrierVesselPivot.ZZQ_ZZ4), carrier.ZZ4_PK);
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCarrierCodeLanguages.ElementAt(0);
			Assert.That(result.ZCL_Description, Is.EqualTo(attr.ZCL_Description));
			Assert.That(result.ZCL_ZX6_NKLanguage, Is.EqualTo(attr.ZCL_ZX6_NKLanguage));
		}

		RefCarrierCode CreateCarrier(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCarrierCode { ZZ4_PK = Guid.NewGuid(), ZZ4_Code = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZ4_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCarrierCode> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpointPK = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpointPK);
		}

		IEnumerable<Models.RefCarrierCode> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpointPK = null)
		{
			var service = new RefCarrierCodeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpointPK, null, DataSetId).OrderBy(x => x.ZZ4_Code);
		}

		T CreateChild<T>(string parentProperty, Guid parentPk, string codeProperty = null, string code = null) where T : new()
		{
			return Helper.CreateChild<T>(repo, parentProperty, parentPk, codeProperty, code);
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
