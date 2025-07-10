using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefAirlineProductCodeServiceFixture
	{
		static string TblPrefix => "RAR";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefAirlineProductCode);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var product1 = repo.Create(() => new RefAirlineProductCode { RAR_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), RAR_Code = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = product1.RAR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefAirlineProductCodeCommodityCodePivot>(repo, nameof(RefAirlineProductCodeCommodityCodePivot.RPC_RAR), product1.RAR_PK);
			var product2 = repo.Create(() => new RefAirlineProductCode { RAR_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), RAR_Code = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = product2.RAR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefAirlineProductCodeCommodityCodePivot>(repo, nameof(RefAirlineProductCodeCommodityCodePivot.RPC_RAR), product2.RAR_PK);
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.RAR_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_ProductCode_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var product1 = CreateProduct(Now, "123", "AA");
			Helper.CreateChild<RefAirlineProductCodeCommodityCodePivot>(repo, nameof(RefAirlineProductCodeCommodityCodePivot.RPC_RAR), product1.RAR_PK);
			var product2 = CreateProduct(Now.AddDays(2), "456", "BB");
			Helper.CreateChild<RefAirlineProductCodeCommodityCodePivot>(repo, nameof(RefAirlineProductCodeCommodityCodePivot.RPC_RAR), product2.RAR_PK);
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.RAR_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_ProductCode_Data()
		{
			var product = CreateProduct(Now.AddDays(1), "123", "AA");
			product.RAR_Description = "ABC";
			Helper.CreateChild<RefAirlineProductCodeCommodityCodePivot>(repo, nameof(RefAirlineProductCodeCommodityCodePivot.RPC_RAR), product.RAR_PK);
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RAR_AirlineID, Is.EqualTo(product.RAR_AirlineID));
			Assert.That(result.RAR_Code, Is.EqualTo(product.RAR_Code));
			Assert.That(result.RAR_Description, Is.EqualTo(product.RAR_Description));
		}

		[Test]
		public void GetLatest_Pivot_Data()
		{
			var product = CreateProduct(Now.AddDays(1), "123", "AA");
			var pivot = Helper.CreateChild<RefAirlineProductCodeCommodityCodePivot>(repo, nameof(RefAirlineProductCodeCommodityCodePivot.RPC_RAR), product.RAR_PK);
			var commodity = repo.Create(() => new RefAirlineCommodityCode
			{
				RAC_PK = Guid.NewGuid(),
				RAC_AirlineID = "123",
				RAC_Code = "AA",
				RAC_Description = "ABC",
			});
			pivot.RPC_RAC = commodity.RAC_PK;
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = commodity.RAC_PK, RVC_DataSetId = DataSetId, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefAirlineProductCodeCommodityCodePivots.ElementAt(0).RefAirlineCommodityCode;
			Assert.That(result.RAC_AirlineID, Is.EqualTo("123"));
			Assert.That(result.RAC_Code, Is.EqualTo("AA"));
			Assert.That(result.RAC_Description, Is.EqualTo("ABC"));
		}

		RefAirlineProductCode CreateProduct(DateTime dateTime, string airlineID, string code)
		{
			var result = repo.Create(() => new RefAirlineProductCode { RAR_PK = Guid.NewGuid(), RAR_AirlineID = airlineID, RAR_Code = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RAR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefAirlineProductCode> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpointPK = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpointPK);
		}

		IEnumerable<Models.RefAirlineProductCode> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpointPK = null)
		{
			var service = new RefAirlineProductCodeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpointPK, null, DataSetId).OrderBy(x => x.RAR_Code);
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
