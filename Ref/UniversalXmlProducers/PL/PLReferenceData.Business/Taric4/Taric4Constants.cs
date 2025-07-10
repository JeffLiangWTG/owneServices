using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.PLReferenceData.Services;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4
{
	public static class Taric4Constants
	{
		public const string NORMALIZED_BASE_FILE_NAME = "base.xml";

		public static class GroupNodes
		{
			public const string FindMeasureByDatesResponseHistory = "findMeasureByDatesResponseHistory";
			public const string FindPublicationSigleByDatesResponseHistory = "findPublicationSigleByDatesResponseHistory";
			public const string FindBaseRegulationByDatesResponseHistory = "findBaseRegulationByDatesResponseHistory";
			public const string FindModificationRegulationByDatesResponseHistory = "findModificationRegulationByDatesResponseHistory";
			public const string FindProrogationRegulationByDatesResponseHistory = "findProrogationRegulationByDatesResponseHistory";
			public const string FindMeasureConditionCodeByDatesResponse = "findMeasureConditionCodeByDatesResponse";
			public const string FindFullTemporaryStopRegulationByDatesResponseHistory = "findFullTemporaryStopRegulationByDatesResponseHistory";
			public const string FindExplicitAbrogationRegulationByDatesResponseHistory = "findExplicitAbrogationRegulationByDatesResponseHistory";
			public const string FindCompleteAbrogationRegulationByDatesResponseHistory = "findCompleteAbrogationRegulationByDatesResponseHistory";
			public const string FindQuotaDefinitionByDatesResponseHistory = "findQuotaDefinitionByDatesResponseHistory";
		}

		public static class ItemNodes
		{
			public const string Measure = "Measure";
			public const string PublicationSigle = "PublicationSigle";
			public const string BaseRegulation = "BaseRegulation";
			public const string ModificationRegulation = "ModificationRegulation";
			public const string ProrogationRegulation = "ProrogationRegulation";
			public const string MeasureConditionCode = "MeasureConditionCode";
			public const string FullTemporaryStopRegulation = "FullTemporaryStopRegulation";
			public const string ExplicitAbrogationRegulation = "ExplicitAbrogationRegulation";
			public const string CompleteAbrogationRegulation = "CompleteAbrogationRegulation";
			public const string QuotaDefinition = "QuotaDefinition";
		}

		internal static class Collections
		{
			public static IDictionary<string, string> GetItemNodesToGroupNodes() => MapItemToGroup();

			public static IDictionary<string, string> GetGroupNodesToItemNodes() => MapItemToGroup(true);

			public static IEnumerable<string> GetSupportedItemNodes() => GetAllStringConstants(typeof(ItemNodes));

			public static IEnumerable<string> GetSupportedGroupNodes() => GetAllStringConstants(typeof(GroupNodes));

			static IEnumerable<string> GetAllStringConstants(Type t) => t
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(x => x.IsLiteral && !x.IsInitOnly && x.FieldType == typeof(string))
				.Select(x => (string)x.GetRawConstantValue());

			static IDictionary<string, string> MapItemToGroup(bool revert = false)
			{
				var result = new Dictionary<string, string>();
				var groups = GetSupportedGroupNodes();
				var items = GetSupportedItemNodes();
				foreach (var item in items)
				{
					if (revert)
					{
						result.Add(FindGroup(item), item);
					}
					else
					{
						result.Add(item, FindGroup(item));
					}
				}
				return result;

				string FindGroup(string item) => groups.Single(x => x.Contains("find" + item + "By"));
			}

		}

		public static class PropertyNodes
		{
			public const string Metainfo = "metainfo";
			public const string OpType = "opType";
			public const string OriginType = "origin";
			public const string Hjid = "hjid";
		}

		public static string NORMALIZED_BASE_FILE_FULL_PATH => Path.Combine(ApplicationConfig.Instance.DownloadsPLPath, NORMALIZED_BASE_FILE_NAME);

		public const int MaxUpdateDownloadRetry = 3;
	}
}
