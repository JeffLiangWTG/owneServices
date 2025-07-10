using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefVesselZZServiceFixture
	{
		static string TblPrefix => "ZZO";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefVesselZZ);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefVesselZZ { ZZO_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZO_Code = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZO_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefVesselZZ { ZZO_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZO_Code = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZO_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZO_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB", "BB" })]
		[TestCase(1, 3, new[] { "BB", "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Vessel_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateVessel(Now, "AA", "ZA", "AA1");
			var p2 = CreateVessel(Now.AddDays(2), "BB", "ZA", "BB1");
			var p3 = CreateVessel(Now.AddDays(2), "BB", "ZA", "BB2");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZO_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Vessel_Data()
		{
			var p = CreateVessel(Now.AddDays(1), "AA");
			p.ZZO_LloydsNumber = "123";
			p.ZZO_RadioCallSign = "34";
			p.ZZO_RN_NKCountryOfReg = "AU";
			p.ZZO_VesselType = "T";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZO_LloydsNumber, Is.EqualTo("123"));
			Assert.That(result.ZZO_NetRegisterTon, Is.EqualTo(0));
			Assert.That(result.ZZO_RadioCallSign, Is.EqualTo("34"));
			Assert.That(result.ZZO_RN_NKCountryOfReg, Is.EqualTo("AU"));
			Assert.That(result.ZZO_VesselType, Is.EqualTo("T"));
			Assert.That(result.ZZO_YearOfConstruction, Is.EqualTo(0));
		}

		[Test]
		public void GetLatest_VesselArrival_Data()
		{
			var vessel = CreateVessel(Now.AddDays(1), "AA");
			var vesselArrival1 = repo.Create(() => new RefVesselArrival
			{
				ZYA_PK = Guid.NewGuid(),
				ZYA_ZZO_Vessel = vessel.ZZO_PK,
				ZYA_ArrivalDate = Now,
				ZYA_VoyageNumber = "1"
			});
			var vesselArrival2 = repo.Create(() => new RefVesselArrival
			{
				ZYA_PK = Guid.NewGuid(),
				ZYA_ZZO_Vessel = vessel.ZZO_PK,
				ZYA_ArrivalDate = Now.AddDays(1),
				ZYA_VoyageNumber = "2"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefVesselArrivals;
			Assert.That(result.Length, Is.EqualTo(2));
			var vesselArrival = result.First(x => x.ZYA_VoyageNumber == "1");
			Assert.That(vesselArrival.ZYA_ArrivalDate, Is.EqualTo(vesselArrival1.ZYA_ArrivalDate));
			vesselArrival = result.First(x => x.ZYA_VoyageNumber == "2");
			Assert.That(vesselArrival.ZYA_ArrivalDate, Is.EqualTo(vesselArrival2.ZYA_ArrivalDate));
		}

		RefVesselZZ CreateVessel(DateTime dateTime, string code = null, string dataGrouping = null, string radioCallSign = null)
		{
			var result = repo.Create(() => new RefVesselZZ { ZZO_PK = Guid.NewGuid(), ZZO_Code = code, ZZO_ZZZ_NKDataGrouping = dataGrouping, ZZO_RadioCallSign = radioCallSign });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZO_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefVesselZZ> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefVesselZZ> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefVesselZZService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZO_Code);
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
