using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefShippingLineServiceFixture
	{
		static string TblPrefix => "RSL";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefShippingLine);

		[TestCase("1746513B-85ED-4030-BFFA-20CADBA34E75", new[] { "AAAA", "BBBB" })]
		[TestCase("1746513B-85ED-4030-BFFA-20CADBA34E78", new[] { "BBBB" })]
		[TestCase("1746513B-85ED-4030-BFFA-20CADBA34E7F", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefShippingLine { RSL_PK = new Guid("1746513B-85ED-4030-BFFA-20CADBA34E77"), RSL_StandardCarrierAlphaCode = "AAAA" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.RSL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefShippingLine { RSL_PK = new Guid("1746513B-85ED-4030-BFFA-20CADBA34E7E"), RSL_StandardCarrierAlphaCode = "BBBB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.RSL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.RSL_StandardCarrierAlphaCode).ToArray();
			Assert.That(dataSets, Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateData(Now, "AA");
			var p2 = CreateData(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.RSL_StandardCarrierAlphaCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(Now.AddDays(1), "ZZZZ");
			p.RSL_CargoSphereRatesAvailable = true;
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RSL_StandardCarrierAlphaCode, Is.EqualTo("ZZZZ"));
			Assert.That(result.RSL_CargoSphereRatesAvailable, Is.EqualTo(true));
		}

		[Test]
		public void GetLatest_RefShippingLineMessagingRequirement_Data()
		{
			var shippingLine = CreateData(Now.AddDays(1), "AAAA");
			var messagingRequirement = repo.Create(() => new RefShippingLineMessagingRequirement
			{
				RSR_PK = Guid.NewGuid(),
				RSR_RSL_ShippingLine = shippingLine.RSL_PK,
				RSR_RST_NKType = "AAA",
				RSR_IsBookingRequest = true,
				RSR_IsShippingInstruction = false
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RSL_StandardCarrierAlphaCode, Is.EqualTo("AAAA"));

			Assert.That(result.RefShippingLineMessagingRequirements != null);
			Assert.That(result.RefShippingLineMessagingRequirements[0].RSR_RST_NKType, Is.EqualTo(messagingRequirement.RSR_RST_NKType));
			Assert.That(result.RefShippingLineMessagingRequirements[0].RSR_IsBookingRequest, Is.EqualTo(messagingRequirement.RSR_IsBookingRequest));
		}

		[Test]
		public void GetLatest_RefShippingLineEBLProvider_Data()
		{
			var shippingLine = CreateData(Now.AddDays(1), "AAAA");
			var eblProvider = repo.Create(() => new RefShippingLineEBLProvider
			{
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = shippingLine.RSL_PK,
				RSE_Name = "EBL Name",
				RSE_IsAvailable = true,
				RSE_IsDefault = false
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RSL_StandardCarrierAlphaCode, Is.EqualTo("AAAA"));

			Assert.That(result.RefShippingLineEBLProviders != null);
			Assert.That(result.RefShippingLineEBLProviders[0].RSE_Name, Is.EqualTo(eblProvider.RSE_Name));
			Assert.That(result.RefShippingLineEBLProviders[0].RSE_IsAvailable, Is.EqualTo(eblProvider.RSE_IsAvailable));
			Assert.That(result.RefShippingLineEBLProviders[0].RSE_IsDefault, Is.EqualTo(eblProvider.RSE_IsDefault));
		}

		RefShippingLine CreateData(DateTime dateTime, string carrierCode = null)
		{
			var result = repo.Create(() => new RefShippingLine { RSL_PK = Guid.NewGuid(), RSL_StandardCarrierAlphaCode = carrierCode });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RSL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefShippingLine> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefShippingLine> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefShippingLineService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RSL_StandardCarrierAlphaCode);
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
