using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCurrencyServiceFixture
	{
		static string TblPrefix => "RX";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCurrency);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCurrency { RX_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), RX_Code = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.RX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCurrency { RX_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), RX_Code = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.RX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.RX_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Currency_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateCurrency(Now, "AA");
			var p2 = CreateCurrency(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.RX_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_RefCurrency_Data()
		{
			var p = repo.Create(() => new RefCurrency
			{
				RX_PK = Guid.NewGuid(),
				RX_Code = "BRL",
				RX_Desc = "Brazilian Real"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.RX_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RX_Code, Is.EqualTo(p.RX_Code));
			Assert.That(result.RX_Desc, Is.EqualTo(p.RX_Desc));
		}

		[Test]
		public void GetLatest_Data_RefLanguageText()
		{
			var p = CreateCurrency(Now.AddDays(1), "NN");
			p.RX_Desc = "New currency";

			repo.Create(() => new RefLanguageText { RLT_PK = Guid.NewGuid(), RLT_ColumnName = "RX_Desc", RLT_Language = "EN", RLT_ParentId = p.RX_PK, RLT_ParentTableCode = "RX", RLT_Text = "another language" });

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RX_Code, Is.EqualTo("NN"));
			Assert.That(result.RX_Desc, Is.EqualTo("New currency"));
			Assert.That(result.RefLanguageTexts != null);
			Assert.AreEqual(result.RefLanguageTexts.Length, 1);
			Assert.AreEqual(result.RefLanguageTexts.First().RLT_Text, "another language");
		}

		RefCurrency CreateCurrency(DateTime dateTime, string code = "XX")
		{
			var result = repo.Create(() => new RefCurrency { RX_PK = Guid.NewGuid(), RX_Code = code, RX_Desc = "Description" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RX_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCurrency> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCurrency> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCurrencyService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RX_Code);
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
