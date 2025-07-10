using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefCountryStatesServiceFixture
	{
		static string TblPrefix => "RW";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCountryStates);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCountryStates { RW_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), RW_Description = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.RW_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCountryStates { RW_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), RW_Description = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.RW_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.RW_Description).ToArray(), Is.EqualTo(expected));
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
			Assert.That(dataSets.Select(x => x.RW_Description).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(Now.AddDays(1), "this is the description");
			p.RW_Code = "NN";
			p.RW_RegionName = "New region";

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RW_Description, Is.EqualTo("this is the description"));
			Assert.That(result.RW_Code, Is.EqualTo("NN"));
			Assert.That(result.RW_RegionName, Is.EqualTo("New region"));
		}

		[Test]
		public void GetLatest_Data_RefLanguageText()
		{
			var p = CreateData(Now.AddDays(1), "this is the description");
			p.RW_Code = "NN";
			p.RW_RegionName = "New region";

			repo.Create(() => new RefLanguageText { RLT_PK = Guid.NewGuid(), RLT_ColumnName = "RW_Description", RLT_Language = "EN", RLT_ParentId = p.RW_PK, RLT_ParentTableCode = "RW", RLT_Text = "another language" });
			repo.Create(() => new RefLanguageText { RLT_PK = Guid.NewGuid(), RLT_ColumnName = "RN_Anything", RLT_Language = "EN", RLT_ParentId = p.RW_PK, RLT_ParentTableCode = "RN", RLT_Text = "another table" });

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RW_Description, Is.EqualTo("this is the description"));
			Assert.That(result.RW_Code, Is.EqualTo("NN"));
			Assert.That(result.RW_RegionName, Is.EqualTo("New region"));
			Assert.That(result.RefLanguageTexts != null);
			Assert.AreEqual(result.RefLanguageTexts.Length, 1);
			Assert.AreEqual(result.RefLanguageTexts.First().RLT_Text, "another language");
		}

		RefCountryStates CreateData(DateTime dateTime, string description = null)
		{
			var result = repo.Create(() => new RefCountryStates { RW_PK = Guid.NewGuid(), RW_Description = description });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RW_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCountryStates> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCountryStates> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCountryStatesService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RW_Description);
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
