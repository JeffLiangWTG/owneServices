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
	class RefMessagingBussPackageInfoServiceFixture
	{
		static string TblPrefix => "ZMP";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefMessagingBussPackageInfo);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefMessagingBussPackageInfo { ZMP_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZMP_PackageName = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZMP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefMessagingBussPackageInfo { ZMP_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZMP_PackageName = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZMP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZMP_PackageName).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_MessagingBussPackageInfo_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateMessagingBussPackageInfo(Now, "AA");
			var p2 = CreateMessagingBussPackageInfo(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZMP_PackageName).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_MessagingBussPackageInfo_Data()
		{
			var p = repo.Create(() => new RefMessagingBussPackageInfo
			{
				ZMP_PK = Guid.NewGuid(),
				ZMP_PackageName = "AA"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZMP_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZMP_PackageName, Is.EqualTo(p.ZMP_PackageName));
		}

		[Test]
		public void GetLatest_MessagingBussPackageVersion_Data()
		{
			var packageInfo = CreateMessagingBussPackageInfo(Now.AddDays(1));
			var packageVersion = repo.Create(() => new RefMessagingBussPackageVersion
			{
				ZMV_PK = Guid.NewGuid(),
				ZMV_ZMP_PackageInfo = packageInfo.ZMP_PK,
				ZMV_Version = "XX"
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefMessagingBussPackageVersions.ElementAt(0);
			Assert.That(1, Is.EqualTo(dataSets.ElementAt(0).RefMessagingBussPackageVersions.Length));
			Assert.That(result.ZMV_Version, Is.EqualTo(packageVersion.ZMV_Version));
		}

		[Test]
		public void GetLatest_MessagingBussCarrierInfo_Data()
		{
			var packageInfo = CreateMessagingBussPackageInfo(Now.AddDays(1));
			var carrierInfo = repo.Create(() => new RefMessagingBussCarrierInfo
			{
				ZMC_CarrierCode = "A",
				ZMC_CountryCode = "EN",
				ZMC_ZMP_PackageInfo = packageInfo.ZMP_PK
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefMessagingBussCarrierInfoes.FirstOrDefault();

			Assert.IsNotNull(result);
			Assert.That(result.ZMC_CarrierCode, Is.EqualTo(carrierInfo.ZMC_CarrierCode));
			Assert.That(result.ZMC_CountryCode, Is.EqualTo(carrierInfo.ZMC_CountryCode));
		}

		RefMessagingBussPackageInfo CreateMessagingBussPackageInfo(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefMessagingBussPackageInfo { ZMP_PK = Guid.NewGuid(), ZMP_PackageName = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZMP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefMessagingBussPackageInfo> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefMessagingBussPackageInfo> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefMessagingBussPackageInfoService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZMP_PackageName);
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
