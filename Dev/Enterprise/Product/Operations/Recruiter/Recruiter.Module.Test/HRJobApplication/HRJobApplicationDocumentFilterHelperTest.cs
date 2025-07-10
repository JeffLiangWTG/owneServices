using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using GlowIndexQueryService.Business;

namespace Enterprise.Recruiter.Module.Testing
{
	public class HRJobApplicationDocumentFilterHelperTest : TestCaseWithFactory
	{
		public class MockGlowIndexQueryEngine : IGlowIndexQueryEngine
		{
			public GlowIndexQueryResultCollection Query(GlowIndexQueryParam queryParam)
			{
				Query_QueryParam = queryParam;
				return Query_ReturnValue;
			}

			public IEnumerable<ZGuid> QueryPk(GlowIndexQueryParam queryParam)
			{
				throw new NotImplementedException();
			}

			public SearchFieldCollection GetSearchFields(string entityType)
			{
				throw new NotImplementedException();
			}

			public ISet<string> GetGlowEntityTypes()
			{
				throw new NotImplementedException();
			}

			public ISet<string> GetGlowEntityCategories()
			{
				throw new NotImplementedException();
			}

			public ZArchitecture.Core.CodeDescriptionPairList GetListByRuleId(Guid lookupRuleId)
			{
				throw new NotImplementedException();
			}

			public GlowIndexQueryResultCollection Query_ReturnValue;
			public GlowIndexQueryParam Query_QueryParam;
			public bool IsConnected => true;
		}

		public void TestGetResumeKeywordsSubQueryLucene_Success()
		{
			var searchTerm = "hi";
			var engine = new MockGlowIndexQueryEngine();
			var results = new GlowIndexQueryResultCollection();
			results.Results = new Collection<GlowIndexQueryResult>(Enumerable.Range(0, 2).Select(i => NewGlowIndexQueryResultWithTestData()).ToList());
			results.MaximumResults = results.Results.Count;
			results.Status = GlowIndexQueryStatus.Success;
			engine.Query_ReturnValue = results;
			var query = HRJobApplicationDocumentFilterHelper.GetResumeKeywordsSubQueryLucene(searchTerm, engine);
			// output string, ie query.LiteralTextADO, should look something like this:
			//  IN (SELECT  FROM dbo.HRJobApplicationDocument WHERE (HPD_PK in (CONVERT('14a3ee23-72f9-4e26-b027-75b95e4aa9f7', 'System.Guid'), CONVERT('3ce24faf-6ba9-4a4f-abe9-0eb44579d928', 'System.Guid'))))
			// language=regex
			var expected = " IN \\(SELECT  FROM dbo.HRJobApplicationDocument WHERE \\(HPD_PK in \\(CONVERT\\('([0-9a-f-]{36})', 'System.Guid'\\), CONVERT\\('([0-9a-f-]{36})', 'System.Guid'\\)\\)\\)\\)";
			var match = Regex.Match(query.LiteralTextADO, expected);
			Assert($"Couldn't match expected generated SQL query to the actual query.\r\nExpectedFormat={expected}\r\nActualQuery={query.LiteralTextADO}", match.Success);
			var guids = new List<string> { match.Groups[1].Captures[0].Value, match.Groups[2].Captures[0].Value };
			AssertContainsExactElementsInAnyOrder(engine.Query_ReturnValue.Results.Select(result => result.PK), guids);
		}

		public void TestGetResumeKeywordsSubQueryLucene_NoResult()
		{
			// assert ZQuery.EmptyResult is added to sub query on no lucene results
			var searchTerm = "hi";
			var engine = new MockGlowIndexQueryEngine();
			engine.Query_ReturnValue = new GlowIndexQueryResultCollection();
			var query = HRJobApplicationDocumentFilterHelper.GetResumeKeywordsSubQueryLucene(searchTerm, engine);
			AssertEquals("Generated sub-query should have nothing added to it", @" IN (SELECT  FROM dbo.HRJobApplicationDocument)", query.LiteralTextADO);
		}

		public void TestGetResumeKeywordsSubQueryLucene_SendsCorrectGlowParams()
		{
			// assert search field and entity type are set in glow query param correctly
			var searchTerm = "hi";
			var engine = new MockGlowIndexQueryEngine();
			engine.Query_ReturnValue = new GlowIndexQueryResultCollection();
			var query = HRJobApplicationDocumentFilterHelper.GetResumeKeywordsSubQueryLucene(searchTerm, engine);
			AssertEquals("Generated sub-query should have nothing added to it", searchTerm, engine.Query_QueryParam.Keyword);
			AssertEquals("EntityType should be set correctly.", HRJobApplicationDocumentFilterHelper.ENTITY_TYPE, engine.Query_QueryParam.EntityType);
			AssertEquals("Search field should be set correctly.", HRJobApplicationDocumentFilterHelper.SEARCH_FIELD, engine.Query_QueryParam.SearchFieldForEntityType);
			Assert("We need to request the count to display to the user when results have been truncated", engine.Query_QueryParam.IncludeCount);
		}

		public void TestGetResumeKeywordsSubQuerySQL()
		{
			ZQuery GetResumeKeywordsQuery(ZString key)
			{
				var query = new ZDBOnlyQuery(typeof(HRJobApplication));
				var applicationDocSubQuery = HRJobApplicationDocumentFilterHelper.GetResumeKeywordsSubQuerySQL(key);
				query.AddSubQuery(applicationDocSubQuery, JoinCondition.And);
				return query;
			}

			var collection = new HRJobApplicationCollection(Factory);
			var application1 = Factory.NewWithValidTestData<HRJobApplication>();
			var application2 = Factory.NewWithValidTestData<HRJobApplication>();
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant3 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant4 = Factory.NewWithValidTestData<HRJobApplicant>();
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			application1.HP_HA = applicant.PK;
			var doc1 = application1.Documents.AddNew();
			doc1.HPD_Content = "<Resume><NonXMLResume><TextResume>KW1<span></span><span>KW2</span><span>KW3</span><span>KW4 KW5</span></TextResume></NonXMLResume></Resume>";
			application2.HP_HA = applicant2.PK;
			var doc2 = application2.Documents.AddNew();
			doc2.HPD_Content = "<Resume><NonXMLResume><TextResume><span></span>Keyword1<span></span>Keyword2<span>Keyword3</span><span>Keyword4 Keyword5</span></TextResume></NonXMLResume></Resume>";
			var application3 = campaign.Applications.AddNew();
			application3.HP_HA = applicant3.PK;
			var doc3 = application3.Documents.AddNew();
			doc3.HPD_Content = "<Resume><NonXMLResume><TextResume><span>Keyword3</span>KW3<span></span><span>KW3 Keyword3</span><span>He said, \"Don't quote me.\"</span></TextResume></NonXMLResume></Resume>";
			var application4 = campaign.Applications.AddNew();
			application4.HP_HA = applicant4.PK;
			var doc4 = application4.Documents.AddNew();
			doc4.HPD_Content = "<ResumeXYZ>ABC</ResumeXYZ>";
			Factory.Save();
			collection.RefreshFromDb();
			AssertContainsExactElementsInAnyOrder(collection, new[] { application1, application2, application3, application4 });
			var keywords = "@@@@@";
			collection.AdditionalFilter = GetResumeKeywordsQuery(keywords);
			collection.RefreshFromDb();
			AssertEquals(0, collection.Count);
			keywords = "ABC KW3 DEF";
			collection.AdditionalFilter = GetResumeKeywordsQuery(keywords);
			collection.RefreshFromDb();
			AssertContainsExactElementsInAnyOrder(collection, new[] { application1, application3 });
			keywords = "XYZ Keyword3 TTT";
			collection.AdditionalFilter = GetResumeKeywordsQuery(keywords);
			collection.RefreshFromDb();
			AssertContainsExactElementsInAnyOrder(collection, new[] { application2, application3 });
			keywords = "AAA \"CC OO\" DDD \"KW3 Keyword3\" CCC";
			collection.AdditionalFilter = GetResumeKeywordsQuery(keywords);
			collection.RefreshFromDb();
			AssertContainsExactElementsInAnyOrder(collection, new[] { application3 });
			keywords = "KW1 Keyword1";
			collection.AdditionalFilter = GetResumeKeywordsQuery(keywords);
			collection.RefreshFromDb();
			AssertContainsExactElementsInAnyOrder(collection, new[] { application1, application2 });
			keywords = "KW3 Keyword3";
			collection.AdditionalFilter = GetResumeKeywordsQuery(keywords);
			collection.RefreshFromDb();
			AssertContainsExactElementsInAnyOrder(collection, new[] { application1, application2, application3 });
			keywords = "\"DON'T QuOtE ME.\"";
			collection.AdditionalFilter = GetResumeKeywordsQuery(keywords);
			collection.RefreshFromDb();
			AssertContainsExactElementsInAnyOrder(collection, new[] { application3 });
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				keywords = "KW2 Keyword3";
				collection.AdditionalFilter = GetResumeKeywordsQuery(keywords);
				collection.RefreshFromDb();
				var query = TestConnection.ExecutedCommandsAndQueryPlans.Single(t => t.Item1.Contains("/Resume/NonXMLResume/TextResume/"));
				var queryAnalyzer = new QueryPlanalyzer(query.Item2.Single());
				AssertEquals(false, queryAnalyzer.TableScans.Any());
				AssertEquals(true, queryAnalyzer.IndexScans.Any(x => x.IndexKind == "PrimaryXML"));
			}
		}

		GlowIndexQueryResult NewGlowIndexQueryResultWithTestData() => new GlowIndexQueryResult(ZGuid.NewZGuid().ToString(), "foo");
		public void TestMessageBoxShownWhenResultsExceedLuceneLimit()
		{
			var searchTerm = "hi";
			var engine = new MockGlowIndexQueryEngine();
			var results = new GlowIndexQueryResultCollection();
			var returnValue = Enumerable.Repeat(NewGlowIndexQueryResultWithTestData(), Constants.MAXIMUM_QUERY_RESULTS_RETURNED);
			results.Results = new Collection<GlowIndexQueryResult>(returnValue.ToList());
			results.MaximumResults = Constants.MAXIMUM_QUERY_RESULTS_RETURNED + 1;
			results.Status = GlowIndexQueryStatus.Success;
			engine.Query_ReturnValue = results;
			_ = HRJobApplicationDocumentFilterHelper.GetResumeKeywordsSubQueryLucene(searchTerm, engine);
			CombineAssertions(() =>
			{
				AssertEquals("Too many items", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals($"Found too many job application documents with matching resume keywords; returned the first {Constants.MAXIMUM_QUERY_RESULTS_RETURNED} of {Constants.MAXIMUM_QUERY_RESULTS_RETURNED + 1} results. Please narrow down your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Message box wasn't a warning box", UnitTestUserNotification.Instance.LastMessage.WasWarning);
			});
		}

		public void TestResultsToPKs()
		{
			var results = new Collection<GlowIndexQueryResult>(Enumerable.Range(0, 3).Select(i => NewGlowIndexQueryResultWithTestData()).ToList());
			var actual = HRJobApplicationDocumentFilterHelper.ResultsToPKs(results);
			AssertArrayEqualsByElements("Query results weren't converted into PKs", new string[] { results[0].PK, results[1].PK, results[2].PK }, actual.Select(z => z.ToString()).ToArray());
		}

		public void TestQueryParamsCreatedCorrectly()
		{
			var searchTerm = "hi there";
			var queryParams = HRJobApplicationDocumentFilterHelper.CreateGlowIndexQueryParam(searchTerm);

			AssertEquals("hi there" , queryParams.Keyword);
			AssertEquals(HRJobApplicationDocumentFilterHelper.SEARCH_FIELD, queryParams.SearchFieldForEntityType);
			AssertEquals(HRJobApplicationDocumentFilterHelper.ENTITY_TYPE, queryParams.EntityType);
			AssertEquals(HRJobApplicationDocumentFilterHelper.DEFAULT_QUERY_RESULTS, queryParams.MaxQueryResults);
		}
	}
}
