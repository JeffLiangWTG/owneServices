using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefVesselServiceFixture
	{
		static string TblPrefix => "RV";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefVessel);

		[Test]
		public void GetLatest_RefVessel()
		{
			CreateRefVessel(Now.AddDays(1), "Code");
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RV_Code, Is.EqualTo("Code"));
		}

		RefVessel CreateRefVessel(DateTime dateTime, string code)
		{
			var result = repo.Create(() => new RefVessel { RV_PK = Guid.NewGuid(), RV_Code = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RV_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefVessel> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefVesselService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : Now.AddDays(100), checkpoint, null, DataSetId);
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
