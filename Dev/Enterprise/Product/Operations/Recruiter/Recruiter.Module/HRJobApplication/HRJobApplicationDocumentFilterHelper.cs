using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.Recruiter.Module
{
	public static class HRJobApplicationDocumentFilterHelper
	{
		static IGlowIndexQueryEngine GlowQueryEngine { get => glowQueryEngine = glowQueryEngine ?? ObjectFactory.Get<IGlowIndexQueryEngine>(); }
		[ThreadStatic]
		static IGlowIndexQueryEngine glowQueryEngine;

		public static ZDBOnlySubQuery GetResumeKeywordsSubQuery(ZString keywords)
			=> GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.Value
				? GetResumeKeywordsSubQueryLucene(keywords, GlowQueryEngine)
				: GetResumeKeywordsSubQuerySQL(keywords);

		public const string SEARCH_FIELD = @"ApplicationDocumentContent";
		public const string ENTITY_TYPE = @"IHRJobApplicationDocument";
		public const int DEFAULT_QUERY_RESULTS = 200;

		public static GlowIndexQueryParam CreateGlowIndexQueryParam(string searchTerm)
			=> new GlowIndexQueryParam(searchTerm, SEARCH_FIELD, ENTITY_TYPE, DEFAULT_QUERY_RESULTS, includeCount: true);

		public static ZDBOnlySubQuery GetResumeKeywordsSubQueryLucene(string searchTerm, IGlowIndexQueryEngine engine)
		{
			var queryParam = CreateGlowIndexQueryParam(searchTerm);
			var luceneResult = engine.Query(queryParam);

			var sqlQuery = new ZDBOnlySubQuery(typeof(HRJobApplicationDocument), HRJobApplicationDocumentSchema.HPD_HP);

			if (luceneResult is null || luceneResult.Status != GlowIndexQueryStatus.Success || luceneResult.Results.Count == 0)
			{
				_ = sqlQuery.AddToFilter(ZQuery.NoResultQuery);
				return sqlQuery;
			}

			if (luceneResult.Results.Count < luceneResult.MaximumResults)
			{
				var message = Res.GetString(
					"EC56AA5B-1A86-4CC7-BB47-C3CD4B838E64",
					"Found too many job application documents with matching resume keywords; returned the first {0} of {1} results. Please narrow down your search.", luceneResult.Results.Count, luceneResult.MaximumResults);

				Globals.Message.ShowWarning(
					message,
					Res.GetString("1493251B-1EAC-43A2-A183-FB774B6E9623", "Too many items"));
			}

			var lucenePKs = ResultsToPKs(luceneResult.Results);

			_ = sqlQuery.AddToFilter(HRJobApplicationDocumentSchema.PK, lucenePKs);
			return sqlQuery;
		}

		public static IEnumerable<ZGuid> ResultsToPKs(ICollection<GlowIndexQueryResult> results)
			=> results
				.Select(r => ZGuid.TryParse(r.PK, out ZGuid zGuid) ? zGuid : ZGuid.Empty)
				.Where(r => r != ZGuid.Empty);

		public static ZDBOnlySubQuery GetResumeKeywordsSubQuerySQL(ZString keywords)
		{
			var query = new ZDBOnlySubQuery(typeof(HRJobApplicationDocument), HRJobApplicationDocumentSchema.HPD_HP);
			var keywordList = SplitKeywords(keywords);

			if (keywordList.Any())
			{
				var whereClause = string.Join(" OR ", keywordList.Select(x => FormattableString.Invariant($@"HPD_Content.exist('(/Resume/NonXMLResume/TextResume/span/text()[contains(lower-case(.),''{x.Replace("'", "''''")}'')])') = 1 OR HPD_Content.exist('(/Resume/NonXMLResume/TextResume/text()[contains(lower-case(.),''{x.Replace("'", "''''")}'')])') = 1"))); // SQL Server function, not translatable
				query.AddFilterAndZSQLParameterCollection(whereClause, null);
			}

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression Pattern, not translatable, Not translatable")]
		static IEnumerable<ZString> SplitKeywords(ZString keywords)
		{
			IEnumerable<ZString> result = null;
			const string pattern = "\\s\"(.*?)\"\\s";
			const char separator = ' ';

			var input = keywords.Trim().ToLower();

			if (!input.IsEmpty)
			{
				input = FormattableString.Invariant($"{separator}{input}{separator}");
				var matches = Regex.Matches(input, pattern);

				if (matches.Count > 0)
				{
					result = matches.Cast<Match>().Select(x => x.Groups[1].Value)
							.Concat(Regex.Replace(input, pattern, separator.ToString())
									.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries))
							.Select(x => (ZString)x);
				}
				else
				{
					result = input.Split(separator);
				}
			}

			return result?.Select(x => x.Trim()).Where(x => !x.IsEmpty).Distinct() ?? Enumerable.Empty<ZString>();
		}
	}
}
