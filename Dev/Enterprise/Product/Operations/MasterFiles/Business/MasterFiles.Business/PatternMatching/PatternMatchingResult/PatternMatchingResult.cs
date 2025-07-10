using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Required for the creation of pattern table")]
	public class PatternMatchingResult : AutoPatternMatchingResult
	{
		public PatternMatchingResult(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class StatusCodes
		{
			public const string PotentialDuplicate = "PDU";
			public const string NoDuplicates = "NDU";
			public const string TemporaryIgnore = "TIG";
			public const string PermanentIgnore = "PIG";
			public const string Excluded = "EXC";
			public const string Error = "ERR";
			public const string Queued = "QUE";
			public const string QueuedForProcessing = "QUP";
		}

		public const double NonValidScoringResult = 0;
		public const double IgnoredRecordRecreationStatus = 0.8;

		public CodeDescriptionPairList ScoreConfidenceCodeDescriptionPairList
		{
			get
			{
				return Factory.GetCachedValue("PatternMatchingResult|ScoreConfidenceCodeDescriptionPairList", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.None), ResString.GetMultilingualString("fa91375f-6054-40b3-8096-8e8120554ae0", "None"));
					list.AddPair(Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.Low), ResString.GetMultilingualString("9f3aa27f-8504-494e-bddc-9e745a4552c5", "Low"));
					list.AddPair(Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.Medium), ResString.GetMultilingualString("529293b3-1025-47da-a08a-abb28f9cc874", "Medium"));
					list.AddPair(Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.High), ResString.GetMultilingualString("a318f9cd-32cb-47d6-b816-9e437831ed7e", "High"));
					list.AddPair(Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.Exact), ResString.GetMultilingualString("2b383786-fc1a-42c0-becc-69a84bea054b", "Exact"));
					list.AddPair(Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.Undefined), ResString.GetMultilingualString("2941dfd6-e212-4134-a538-c30c550487bd", "Undefined"));
					return list;
				});
			}
		}

		ZQuery IgnoredResultQuery
		{
			get
			{
				var query1 = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, PMT_MasterPK)
					.AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, PMT_TargetPK)
					.AddToFilter(PatternMatchingResultSchema.PMT_Status, new[] { StatusCodes.PermanentIgnore, StatusCodes.TemporaryIgnore });
				var query2 = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, PMT_TargetPK)
					.AddToFilter(PatternMatchingResultSchema.PMT_TargetPK, PMT_MasterPK)
					.AddToFilter(PatternMatchingResultSchema.PMT_Status, new[] { StatusCodes.PermanentIgnore, StatusCodes.TemporaryIgnore });

				var queryForIgnored = query1.AddToFilter(query2, JoinCondition.Or);
				queryForIgnored.OrderBy = PatternMatchingResultSchema.PMT_FoundTimeUtc.Name;
				return queryForIgnored;
			}
		}

		public static class SchemaConstants
		{
			public const string PMT_Confidence = "PMT_Confidence";
			public const string PMT_StatusDescription = "PMT_StatusDescription";
			public const string PMT_TargetOrgCode = "PMT_TargetOrgCode";
			public const string PMT_TargetOrgName = "PMT_TargetOrgName";
			public const string PMT_TargetOrgUNLOCO = "PMT_TargetOrgUNLOCO";
			public const string PMT_TargetOrgType = "PMT_TargetOrgType";
			public const string ExcludedByList = "ExcludedByList";
			public const string ExcludedByForEveryone = "ExcludedByForEveryone";
		}

		//This needs to be revised when ConfidenceRating in CargoWise.Tools.DuplicateDetector has a method to provide Confidence string using score
		public ZString PMT_Confidence
		{
			get
			{
				if (PMT_ScorePercent <= 25)
				{
					return ScoreConfidenceCodeDescriptionPairList[Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.None), StringComparison.Ordinal].Description;
				}
				else if (PMT_ScorePercent <= 50)
				{
					return ScoreConfidenceCodeDescriptionPairList[Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.Low), StringComparison.Ordinal].Description;
				}
				else if (PMT_ScorePercent <= 80)
				{
					return ScoreConfidenceCodeDescriptionPairList[Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.Medium), StringComparison.Ordinal].Description;
				}
				else if (PMT_ScorePercent <= 99)
				{
					return ScoreConfidenceCodeDescriptionPairList[Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.High), StringComparison.Ordinal].Description;
				}
				else
				{
					return ScoreConfidenceCodeDescriptionPairList[Enum.GetName(typeof(ConfidenceRating), ConfidenceRating.Exact), StringComparison.Ordinal].Description;
				}
			}
		}

		public ZPropertyInfo PMT_ConfidenceInfo => GetZPropertyInfo(SchemaConstants.PMT_Confidence);

		public ZString PMT_StatusDescription => StatusCodeDescriptions[PMT_Status, StringComparison.Ordinal].Description;

		public ZString FinalStatus
		{
			get
			{
				using (Factory.IsOwnedByCurrentThread ? null : Db.DisposableActionForDbConnection())
				{
					var factory = Factory.IsOwnedByCurrentThread ? Factory : new ReadOnlyBusinessObjectFactory();
					var ignoredResultCollection = factory.Load<PatternMatchingResult>(IgnoredResultQuery);

					ZString finalStatus;
					if (ignoredResultCollection.Any(r => r.PMT_Status == StatusCodes.PermanentIgnore))
					{
						finalStatus = StatusCodes.PermanentIgnore;
					}
					else if (ignoredResultCollection.Any(r => r.PMT_GS_NKExcludeBy == GlbStaff.CurrentUser.GS_Code))
					{
						finalStatus = StatusCodes.TemporaryIgnore;
					}
					else
					{
						finalStatus = PMT_Status;
					}

					return finalStatus;
				}
			}
		}

		public ZString FinalStatusDescription => StatusCodeDescriptions[FinalStatus, StringComparison.Ordinal].Description;

		public CodeDescriptionPairList StatusCodeDescriptions
		{
			get
			{
				return Factory.GetCachedValue("PatternMatchingResult|StatusCodeDescriptions", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(StatusCodes.Error, ResString.GetMultilingualString("1190c0c0-9dc3-42d5-a282-688f28dd9992", "Error"));
					list.AddPair(StatusCodes.Excluded, ResString.GetMultilingualString("f8d7c4ce-13b5-4435-8ecc-3ef7b8203391", "Excluded"));
					list.AddPair(StatusCodes.NoDuplicates, ResString.GetMultilingualString("a80b1ce0-4dc0-45a7-bfeb-2cd6ec881c54", "No duplicates"));
					list.AddPair(StatusCodes.PermanentIgnore, ResString.GetMultilingualString("35506812-16fa-443a-81fb-f98d78f91a9c", "Permanently ignored"));
					list.AddPair(StatusCodes.PotentialDuplicate, ResString.GetMultilingualString("bdddf56a-6900-46a8-a977-b7ad55a8a4e7", "Potential duplicate"));
					list.AddPair(StatusCodes.TemporaryIgnore, ResString.GetMultilingualString("9c74b776-5ae3-4c63-9970-1d6b6cb51911", "Temporarily ignored"));
					return list;
				});
			}
		}

		public ZString PMT_TargetOrgCode => TargetOrganisation.OH_Code;

		public ZPropertyInfo PMT_TargetOrgCodeInfo => GetZPropertyInfo(SchemaConstants.PMT_TargetOrgCode);

		public ZString PMT_TargetOrgName => TargetOrganisation.OH_FullName;

		public ZPropertyInfo PMT_TargetOrgNameInfo => GetZPropertyInfo(SchemaConstants.PMT_TargetOrgName);

		public ZString PMT_TargetOrgUNLOCO => TargetOrganisation.UNLOCO.RL_Code;

		public ZPropertyInfo PMT_TargetOrgUNLOCOInfo => GetZPropertyInfo(SchemaConstants.PMT_TargetOrgUNLOCO);

		public ZString PMT_TargetOrgType => TargetOrganisation.OrganisationTypesAsString;

		public ZPropertyInfo PMT_TargetOrgTypeInfo => GetZPropertyInfo(SchemaConstants.PMT_TargetOrgType);

		public ZString ExcludedByList => GetExcludeByString(false);

		public ZPropertyInfo ExcludedByListInfo => GetZPropertyInfo(SchemaConstants.ExcludedByList);

		public ZString ExcludedByForEveryone => GetExcludeByString(true);

		public ZPropertyInfo ExcludedByForEveryoneInfo => GetZPropertyInfo(SchemaConstants.ExcludedByForEveryone);

		ZString GetExcludeByString(bool isForPIG)
		{
			var result = string.Empty;

			using (Factory.IsOwnedByCurrentThread ? null : Db.DisposableActionForDbConnection())
			{
				var factory = Factory.IsOwnedByCurrentThread ? Factory : new ReadOnlyBusinessObjectFactory();
				var ignoredResultCollection = factory.Load<PatternMatchingResult>(IgnoredResultQuery);
				var finalStatus = ignoredResultCollection.Length > 0 ? ignoredResultCollection.Any(r => r.PMT_Status == StatusCodes.PermanentIgnore) ? (ZString)StatusCodes.PermanentIgnore : (ZString)StatusCodes.TemporaryIgnore : PMT_Status;

				if (finalStatus == StatusCodes.PermanentIgnore)
				{
					if (isForPIG)
					{
						result = ignoredResultCollection.FirstOrDefault(r => r.PMT_Status == StatusCodes.PermanentIgnore)?.PMT_GS_NKExcludeBy;
					}
					else
					{
						result = string.Join(", ", ignoredResultCollection.Where(r => r.PMT_Status == StatusCodes.TemporaryIgnore).Select(p => p.PMT_GS_NKExcludeBy));
					}
				}
				else if (finalStatus == StatusCodes.TemporaryIgnore)
				{
					if (!isForPIG)
					{
						result = string.Join(", ", ignoredResultCollection.Where(r => r.PMT_Status == StatusCodes.TemporaryIgnore).Select(p => p.PMT_GS_NKExcludeBy));
					}
				}
			}

			return result;
		}

		public OrgHeader TargetOrganisation => Factory.Load<OrgHeader>(PMT_TargetPK);

		public GlbPerson TargetPerson => Factory.Load<GlbPerson>(PMT_TargetPK);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public static PatternMatchingResult CreatePatternMatchingResult(string masterSourceTable, ZGuid masterPK, BusinessObjectFactory factory, string status)
		{
			var record = factory.New<PatternMatchingResult>();
			record.PMT_MasterPK = masterPK;
			record.PMT_MasterTableCode = masterSourceTable;
			record.PMT_Status = status;
			record.PMT_FoundTimeUtc = DateTime.UtcNow;
			record.PMT_ScorePercent = 0;
			record.SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
			return record;
		}
	}
}
