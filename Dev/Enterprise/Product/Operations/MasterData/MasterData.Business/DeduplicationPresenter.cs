using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationPresenter
	{
		readonly IEnumerable<ScoringResult> scoringResults;
		readonly IDeduplicationGlowObject master;
		readonly IEnumerable<IDeduplicationGlowObject> targetGlows;
		readonly DeduplicationProvider helper;
		readonly List<DeduplicationPresenterModel> presenterModels = new List<DeduplicationPresenterModel>();
		readonly IDeduplicationDebuggerParticipant debuggerParticipant;
		const string DebuggerParticipantName = "DeduplicationPresenter";

		public DeduplicationPresenter(IDeduplicationGlowObject master, IEnumerable<IDeduplicationGlowObject> targetGlows, IEnumerable<ScoringResult> scoringResults)
			: this(master, targetGlows, scoringResults, new DeduplicationProvider())
		{
		}

		public DeduplicationPresenter(IDeduplicationGlowObject master, IEnumerable<IDeduplicationGlowObject> targetGlows, IEnumerable<ScoringResult> scoringResults, DeduplicationProvider helper)
		{
			this.master = master;
			this.targetGlows = targetGlows;
			this.scoringResults = scoringResults;
			this.helper = helper;
			debuggerParticipant = new DeduplicationDebuggerParticipant(DebuggerParticipantName);
			DeduplicationUtils.DebuggerHubInstance.Register(debuggerParticipant);
		}

		public IEnumerable<DeduplicationPresenterModel> GeneratePresenterModels()
		{
			using (var timer = new DeduplicationPerformanceMonitor())
			{
				presenterModels.Clear();
				var mainGroupings = scoringResults.GroupBy(x => x.TargetType);

				foreach (var mainGrouping in mainGroupings)
				{
					var ratings = mainGrouping
						.GroupBy(x => x.ConfidenceRating)
						.OrderByDescending(x => x.Key)
						.Select(x => x.Key);

					ScoreResultsByRating(ratings, mainGrouping.Key);
				}

				debuggerParticipant.Send(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName, scoringResults, nameof(GeneratePresenterModels), timer.ElapsedDuration, master);

				PopulateResultModelsForAssociations();

				return presenterModels;
			}
		}

		#region Populate Result Models For Associations

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		void PopulateResultModelsForAssociations()
		{
			if (master is DeduplicationGlbPerson deduplicationGlbPerson)
			{
				var masterPerson = Factory.Load<GlbPerson>(deduplicationGlbPerson.PER_PK);

				if (masterPerson != null)
				{
					var contactStr = Res.GetString("7ADCEDCC-E1D9-4D38-B0D2-CB5544972CD1", "Contact");
					var staffStr = Res.GetString("2E37022E-FFDA-4C62-930C-8896BA4DB440", "Staff");
					var applicantStr = Res.GetString("E48DC0CD-5E89-43B6-A350-5020BAD0E662", "Job Applicant");
					var masterPrimaryPk = masterPerson.PrimaryRelationship?.PPR_PrimaryId.ToGuid() ?? Guid.Empty;
					var masterValues = GetAssociationInfo(masterPerson, contactStr, masterPrimaryPk, staffStr, applicantStr);

					foreach (var glowDeduplication in targetGlows.Where(u => presenterModels.Any(v => v.TargetID == u.PK)))
					{
						var targetPerson = Factory.Load<GlbPerson>(glowDeduplication.PK);
						if (targetPerson != null)
						{
							var targetPrimaryPk = targetPerson.PrimaryRelationship?.PPR_PrimaryId.ToGuid() ?? Guid.Empty;
							var targetValues = GetAssociationInfo(targetPerson, contactStr, targetPrimaryPk, staffStr, applicantStr);
							var emptyValue = new AssociationInfo(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false, string.Empty, Guid.Empty, false);
							var maxLength = Math.Max(masterValues.Length, targetValues.Length);

							for (var index = 0; index < maxLength; index++)
							{
								var masterInfo = masterValues.Length > index ? masterValues[index] : emptyValue;
								var targetInfo = targetValues.Length > index ? targetValues[index] : emptyValue;

								presenterModels.Add(GetModel(masterInfo.Type, targetInfo.Type, DeduplicationProvider.Constants.Type, masterInfo.PK, targetInfo.PK, glowDeduplication.PK));
								presenterModels.Add(GetModel(masterInfo.Name, targetInfo.Name, DeduplicationProvider.Constants.Name, masterInfo.PK, targetInfo.PK, glowDeduplication.PK));
								presenterModels.Add(GetModel(masterInfo.IsPrimary, targetInfo.IsPrimary, DeduplicationProvider.Constants.PrimaryWorkplace, masterInfo.PK, targetInfo.PK, glowDeduplication.PK));
								presenterModels.Add(GetModel(masterInfo.UNLOCO, targetInfo.UNLOCO, DeduplicationProvider.Constants.UNLOCO, masterInfo.PK, targetInfo.PK, glowDeduplication.PK));
								presenterModels.Add(GetModel(masterInfo.City, targetInfo.City, DeduplicationProvider.Constants.City, masterInfo.PK, targetInfo.PK, glowDeduplication.PK));
								presenterModels.Add(GetModel(masterInfo.State, targetInfo.State, DeduplicationProvider.Constants.State, masterInfo.PK, targetInfo.PK, glowDeduplication.PK));
								presenterModels.Add(GetModel(masterInfo.Country, targetInfo.Country, DeduplicationProvider.Constants.Country, masterInfo.PK, targetInfo.PK, glowDeduplication.PK));
								presenterModels.Add(GetModel(masterInfo.Active, targetInfo.Active, DeduplicationProvider.Constants.Active, masterInfo.PK, targetInfo.PK, glowDeduplication.PK));
								presenterModels.Add(GetModel(masterInfo.Email, targetInfo.Email, DeduplicationProvider.Constants.Email, masterInfo.PK, targetInfo.PK, glowDeduplication.PK));
							}
						}
					}
				}
			}
		}

		AssociationInfo[] GetAssociationInfo(GlbPerson person, string contactStr, Guid primaryPk, string staffStr, string applicantStr)
		{
			return person.ContactCollection.Cast<OrgContact>()
				.Select(u => GetContactInfo(contactStr, u, primaryPk))
				.Concat(person.StaffCollection.Select(u => GetStaffInfo(staffStr, u, primaryPk)))
				.Concat(person.ApplicantCollection.Cast<Integration.Recruiter.IHRJobApplicant>().Select(u => GetApplicantInfo(applicantStr, u, primaryPk)))
				.ToArray();
		}

		protected AssociationInfo GetApplicantInfo(string applicantStr, Integration.Recruiter.IHRJobApplicant applicant, Guid primaryPK)
		{
			return new AssociationInfo(applicantStr, applicant.HA_FullName, string.Empty, applicant.HA_City, applicant.HA_State, GetCountryName(applicant.HA_RN_NKCountry), true, applicant.HA_EmailAddress, applicant.PK.ToGuid(), primaryPK == applicant.PK);
		}

		protected AssociationInfo GetStaffInfo(string staffStr, GlbStaff staff, Guid primaryPK)
		{
			return new AssociationInfo(staffStr, staff.GS_FullNameInternal, string.Empty, staff.GS_CityInternal, staff.GS_StateInternal, GetCountryName(staff.GS_RN_NKCountryCodeInternal), staff.GS_IsActive, staff.GS_EmailAddressInternal, staff.PK.ToGuid(), primaryPK == staff.PK);
		}

		protected AssociationInfo GetContactInfo(string contactStr, OrgContact contact, Guid primaryPK)
		{
			var address = contact.OrgAddress ?? contact.Header?.MainAddress;

			return new AssociationInfo(contactStr, contact.OC_ContactName, address?.OA_RL_NKRelatedPortCode, address?.OA_City, address?.OA_State, GetCountryName(address?.OA_RN_NKCountryCode), contact.OC_IsActive, contact.OC_Email, contact.PK.ToGuid(), primaryPK == contact.PK);
		}

		protected DeduplicationPresenterModel GetModel(string masterValue, string targetValue, string columnName, Guid masterChildPK, Guid targetChildPK, Guid targetPK)
		{
			return new DeduplicationPresenterModel
			{
				ChildComparisonResultScore = 0,
				ChildConfidenceRating = ConfidenceRating.Undefined,
				ChildDisplayNameForMasterColumns = columnName,
				ChildDisplayNameForTargetColumns = columnName,
				ChildDisplayModeForType = DeduplicationDisplayMode.Detailed,
				ChildGroupNameForType = DeduplicationProvider.Constants.ActiveAssociations,
				ChildMasterValue = masterValue,
				ChildScore = 0,
				ChildScoringResultGroupRating = ConfidenceRating.Undefined,
				ChildTargetValue = targetValue,
				ChildTargetID = targetChildPK,
				ChildMasterID = masterChildPK,
				Confidence = ConfidenceRating.Undefined,
				GroupNameForType = DeduplicationProvider.Constants.ActiveAssociations,
				MainScore = 0,
				GlowTargetType = typeof(IGlbPerson),
				MasterType = master.GetType(),
				Master = helper.GetHeading(master),
				MasterValue = columnName,
				TargetID = targetPK
			};
		}

		string GetCountryName(string countryCode)
		{
			RefCountry country = null;

			if (!string.IsNullOrEmpty(countryCode))
			{
				country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			}

			return country?.RN_DescMultilingual;
		}

		public class AssociationInfo
		{
			public AssociationInfo(string type, string name, string unloco, string city, string state, string country, bool active, string email, Guid pk, bool isPrimary)
			{
				Type = type;
				Name = name;
				UNLOCO = unloco;
				City = city;
				State = state;
				Country = country;
				Active = active.ToString();
				Email = email;
				PK = pk;
				IsPrimary = isPrimary.ToString();
			}

			public string Type { get; }
			public string Name { get; }
			public string UNLOCO { get; }
			public string City { get; }
			public string State { get; }
			public string Country { get; }
			public string Active { get; }
			public string Email { get; }
			public Guid PK { get; }
			public string IsPrimary { get; }
		}

		#endregion

		void ScoreResultsByRating(IEnumerable<ConfidenceRating> ratings, Type type)
		{
			foreach (var rating in ratings)
			{
				var scoringResultsByTargetAndRating = scoringResults.Where(x => x.TargetType == type && x.ConfidenceRating == rating);

				ScoreResultsByTargetAndRating(scoringResultsByTargetAndRating, rating, type);
			}
		}

		void ScoreResultsByTargetAndRating(IEnumerable<ScoringResult> scoringResultsByTargetAndRating, ConfidenceRating rating, Type type)
		{
			foreach (var scoringResult in scoringResultsByTargetAndRating)
			{
				List<ResultTypeGrouping> childResults = null;

				if (scoringResult.ChildResults.Any())
				{
					childResults = scoringResult.ChildResults
						.Where(result => result.MasterType != null)
						.GroupBy(result => result.MasterType)
						.Select(result => new ResultTypeGrouping { ResultType = result.Key, GroupedResult = result })
						.ToList();
				}
				else
				{
					childResults = new List<ResultTypeGrouping>
					{
						new ResultTypeGrouping
						{
							ResultType = scoringResult.MasterType,
							GroupedResult = new ScoringResultGrouping<Type, ScoringResult>(
								scoringResult.MasterType,
								scoringResult)
						}
					};
				}

				PopulateResultModels(childResults, scoringResult, rating, type);
			}
		}

		void PopulateResultModels(IEnumerable<ResultTypeGrouping> childResults, ScoringResult scoringResult, ConfidenceRating rating, Type type)
		{
			foreach (var childResult in childResults)
			{
				var mainMasterHeading = GetHeading(scoringResult, true);
				var mainTargetHeading = GetHeading(scoringResult, false);

				foreach (var childScoringResult in childResult.GroupedResult)
				{
					var childConfidenceRating = helper.GetConfidenceForScore(childScoringResult.Score);
					var businessObjectElement = new BusinessObjectElement
					{
						ChildResult = childResult,
						ChildScoringResult = childScoringResult,
						ChildConfidenceRating = childConfidenceRating,
						MainMasterHeading = mainMasterHeading,
						MainRating = rating,
						MainScoringResult = scoringResult,
						MainTarget = mainTargetHeading,
						MainTargetHeading = mainTargetHeading,
						MainType = type
					};

					presenterModels.AddRange(TransformToPresenterModels(businessObjectElement));
				}
			}
		}

		IEnumerable<DeduplicationPresenterModel> TransformToPresenterModels(BusinessObjectElement element)
		{
			var childResultAndCompResults = element.ChildScoringResult.ComparisonResults.Select(u => (ChildResult: element.ChildScoringResult, CompResult: u));

			foreach (var childResult in element.ChildScoringResult.ChildResults)
			{
				childResultAndCompResults = childResultAndCompResults.Concat(childResult.ComparisonResults.Select(u => (childResult, u)));
			}

			foreach (var childResultAndCompResult in childResultAndCompResults.OrderBy(x => x.CompResult.MasterFieldNames.First()))
			{
				yield return GetModel(element, childResultAndCompResult.CompResult, childResultAndCompResult.ChildResult);
			}
		}

		DeduplicationPresenterModel GetModel(BusinessObjectElement element, ComparisonResult compResult, ScoringResult childScoringResult)
		{
			var masterValue = GetComparisonValue(childScoringResult, compResult, true);
			var targetValue = GetComparisonValue(childScoringResult, compResult, false);

			return new DeduplicationPresenterModel
			{
				ChildComparisonResultScore = childScoringResult.ConfidenceRating == ConfidenceRating.Exact && masterValue != targetValue ? compResult.Score - 0.1 : compResult.Score,
				ChildConfidenceRating = childScoringResult.ConfidenceRating == ConfidenceRating.Exact && masterValue != targetValue ? ConfidenceRating.High : helper.GetConfidenceForScore(compResult.Score),
				ChildDisplayNameForMasterColumns = helper.GetDisplayNameForColumns(compResult.MasterFieldNames),
				ChildDisplayNameForTargetColumns = helper.GetDisplayNameForColumns(compResult.TargetFieldNames),
				ChildDisplayModeForType = helper.GetDisplayModeForType(element.ChildResult.ResultType),
				ChildGroupNameForType = helper.GetGroupNameForType(element.ChildResult.ResultType),
				ChildMasterValue = masterValue,
				ChildScore = childScoringResult.Score,
				ChildScoringResultGroupRating = element.ChildConfidenceRating,
				ChildTargetValue = targetValue,
				ChildTargetID = childScoringResult.TargetPK,
				ChildMasterID = childScoringResult.MasterPK,
				Confidence = element.MainRating,
				GroupNameForType = helper.GetGroupNameForType(element.MainType),
				MainScore = element.MainScoringResult.Score,
				GlowTargetType = element.MainType,
				MasterType = master.GetType(),
				Master = helper.GetHeading(master),
				MasterValue = element.MainMasterHeading,
				Target = element.MainTarget,
				TargetID = element.MainScoringResult.TargetPK,
				TargetValue = element.MainTargetHeading
			};
		}

		string GetHeading(ScoringResult scoringResult, bool shouldUseMaster)
		{
			var heading = string.Empty;

			var type = shouldUseMaster
				? scoringResult.MasterType
				: scoringResult.TargetType;

			var isMultiSourceData = type != null && (type.GetInterfaces().Contains(typeof(IMultiSourceData<string>)) || type.GetInterfaces().Contains(typeof(IMultiSourceData<DateTime>)));

			if (shouldUseMaster)
			{
				heading = helper.GetMasterHeading(
					master as IDeduplicationMaster,
					scoringResult.MasterPK,
					isMultiSourceData ? scoringResult.MultiSourceMasterType : scoringResult.MasterType);
			}
			else
			{
				heading = helper.GetTargetHeading(
					targetGlows,
					scoringResult.TargetPK,
					isMultiSourceData ? scoringResult.MultiSourceTargetType : scoringResult.TargetType);
			}

			return heading;
		}

		string GetComparisonValue(ScoringResult scoringResult, ComparisonResult comparisonResult, bool shouldUseMaster)
		{
			var heading = string.Empty;

			var type = shouldUseMaster
				? scoringResult.MasterType
				: scoringResult.TargetType;

			var isMultiSourceData = type != null && (type.GetInterfaces().Contains(typeof(IMultiSourceData<string>)) || type.GetInterfaces().Contains(typeof(IMultiSourceData<DateTime>)));
			var isMultiSourceDomain = type != null && type == typeof(MultiSourceDomain);

			if (shouldUseMaster)
			{
				heading = scoringResult.MasterPK != Guid.Empty ? helper.GetMasterComparisonValue(
					master,
					scoringResult.MasterPK,
					isMultiSourceData ? scoringResult.MultiSourceMasterType : scoringResult.MasterType,
					comparisonResult.MasterFieldNames,
					isMultiSourceDomain)
					: string.Empty;
			}
			else
			{
				heading = scoringResult.TargetPK != Guid.Empty ? helper.GetTargetComparisonValue(
					targetGlows.Cast<IDeduplicationGlowObject>(),
					scoringResult.TargetPK,
					isMultiSourceData ? scoringResult.MultiSourceTargetType : scoringResult.TargetType,
					comparisonResult.TargetFieldNames,
					isMultiSourceDomain)
					: string.Empty;
			}

			return heading;
		}

		class ResultTypeGrouping
		{
			public Type ResultType { get; set; }

			public IGrouping<Type, ScoringResult> GroupedResult { get; set; }
		}

		class BusinessObjectElement
		{
			public ScoringResult ChildScoringResult { get; set; }
			public ResultTypeGrouping ChildResult { get; set; }
			public ScoringResult MainScoringResult { get; set; }
			public ConfidenceRating MainRating { get; set; }
			public Type MainType { get; set; }
			public ConfidenceRating ChildConfidenceRating { get; set; }
			public string MainTarget { get; set; }
			public string MainMasterHeading { get; set; }
			public string MainTargetHeading { get; set; }
		}
	}
}
