using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.Business
{
	public abstract partial class DuplicationModelDetail : NonPersistentBusinessObject
	{
		[ThreadSafe]
		static readonly Dictionary<string, string> dataSourceNameMap = new Dictionary<string, string>
		{
			[DeduplicationProvider.Constants.OrganisationNames] = DeduplicationProvider.Constants.Name,
			[DeduplicationProvider.Constants.PersonNames] = DeduplicationProvider.Constants.Name,
			[DeduplicationProvider.Constants.PhoneNumbers] = DeduplicationProvider.Constants.Number,
			[DeduplicationProvider.Constants.Websites] = DeduplicationProvider.Constants.Website,
			[DeduplicationProvider.Constants.Domains] = DeduplicationProvider.Constants.Domain,
			[DeduplicationProvider.Constants.Emails] = DeduplicationProvider.Constants.Email,
			[DeduplicationProvider.Constants.Birthdays] = DeduplicationProvider.Constants.Birthday
		};

		[ThreadSafe]
		static readonly IEnumerable<string> SupportMergeGroupTypes = new[]
		{
			DeduplicationProvider.Constants.Addresses,
			DeduplicationProvider.Constants.Contacts
		};

		protected DuplicationModelDetail(DeduplicationPresenterModel presenterModel)
		{
			if (presenterModel.ChildGroupNameForType != DeduplicationProvider.Constants.ActiveAssociations)
			{
				Score = presenterModel.ChildScoringResultGroupRating == ConfidenceRating.None ? 0 : presenterModel.ChildScore;
				MergeMode = GetDefaultMergeMode(presenterModel);
				Similarity = DedupeTranslationHelper.GetConfidenceDescription(presenterModel.ChildScoringResultGroupRating.ToString());
				ConfidenceScore = ZString.Format("{0}%", Score * 100);

				if (MergeMode != DedupeMergeMode.NotSupported)
				{
					MergeModeList.SupportAdd();

					if (presenterModel.ChildScoringResultGroupRating != ConfidenceRating.None)
					{
						MergeModeList.SupportMerge();
					}

					SelfBizoPk = GetSelfBizOPk(presenterModel);
					OpponentBizoPk = GetOpponentBizoPk(presenterModel);
				}
			}
		}

		protected DuplicationModelDetail(IEnumerable<DeduplicationPresenterModel> presenterModels)
			: this(presenterModels.First())
		{
		}

		public ZPropertyInfo MergeModeStringInfo => GetZPropertyInfo(nameof(MergeModeString));

		[BusinessObjectTestExclude]
		[List(nameof(MergeModeList))]
		public ZString MergeModeString
		{
			get => MergeModeList.GetDescriptionFromCode(MergeMode.ToString());
			set
			{
				if (MergeMode != DedupeMergeMode.NotSupported)
				{
					var code = MergeModeList.GetCodeFromDescription(value);
					if (code != null && Enum.TryParse<DedupeMergeMode>(code, out var mergeMode))
					{
						MergeMode = mergeMode;
					}
				}

				MergeModeStringInfo.RefreshBinding();
			}
		}

		public DuplicationMergeModeList MergeModeList { get; } = new DuplicationMergeModeList();

		protected bool MergeModeString_ReadOnly => MergeModeList.Count <= 1;

		public DedupeMergeMode MergeMode { get; set; }

		public Guid SelfBizoPk { get; }

		public Guid OpponentBizoPk { get; }

		public double Score { get; }

		public static IEnumerable<DuplicationModelDetailGroup> GetDuplicationModelDetails(IEnumerable<DeduplicationPresenterModel> presenterModels)
		{
			return presenterModels
				.GroupBy(model => model.ChildGroupNameForType)
				.Select(childGroup => dataSourceNameMap.TryGetValue(childGroup.Key, out var sourceName)
					? GetModelDetailGroupForListDisplay(childGroup, sourceName)
					: GetModelDetailGroupForDetailDisplay(childGroup)
				)
				.ToArray();
		}

		public static bool IsGroupTypeSupportMerge(string groupType)
		{
			return SupportMergeGroupTypes.Contains(groupType);
		}

		protected abstract Guid GetSelfBizOPk(DeduplicationPresenterModel presenterModel);

		protected abstract Guid GetOpponentBizoPk(DeduplicationPresenterModel presenterModel);

		DedupeMergeMode GetDefaultMergeMode(DeduplicationPresenterModel presenterModel)
		{
			if (GetSelfBizOPk(presenterModel) != Guid.Empty && IsGroupTypeSupportMerge(presenterModel.ChildGroupNameForType))
			{
				return presenterModel.ChildScoringResultGroupRating >= ConfidenceRating.Medium ? DedupeMergeMode.Merge : DedupeMergeMode.Add;
			}

			return DedupeMergeMode.NotSupported;
		}

		#region Display Columns

		static IEnumerable<string> GetDisplayColumns(string sourceName, bool withSimilarity)
		{
			var displayColumns = new List<string>();

			if (withSimilarity)
			{
				displayColumns.Add(DeduplicationProvider.Constants.Similarity);
				displayColumns.Add(DeduplicationProvider.Constants.ConfidenceScore);
			}

			displayColumns.Add(sourceName);
			displayColumns.Add(DeduplicationProvider.Constants.Source);

			return displayColumns;
		}

		static IEnumerable<string> GetMasterDisplayColumns(IGrouping<string, DeduplicationPresenterModel> modelGroup)
		{
			return modelGroup.GroupBy(x => x.ChildDisplayNameForMasterColumns).Select(x => x.Key).ToArray();
		}

		static IEnumerable<string> GetCandidateDisplayColumns(IGrouping<string, DeduplicationPresenterModel> modelGroup)
		{
			var displayColumns = new List<string>();

			if (modelGroup.Key != DeduplicationProvider.Constants.ActiveAssociations)
			{
				displayColumns.Add(DeduplicationProvider.Constants.Similarity);
				displayColumns.Add(DeduplicationProvider.Constants.ConfidenceScore);
			}

			displayColumns.AddRange(modelGroup.GroupBy(x => x.ChildDisplayNameForTargetColumns).Select(x => x.Key));

			return displayColumns;
		}

		#endregion Display Columns

		#region Implementation

		static DuplicationModelDetailGroup GetModelDetailGroupForListDisplay(IGrouping<string, DeduplicationPresenterModel> modelGroup, string sourceName)
		{
			var masterColumns = GetDisplayColumns(sourceName, withSimilarity: false);
			var candidateColumns = GetDisplayColumns(sourceName, withSimilarity: true);

			var models = modelGroup.ToArray();
			var masterDetails = models.Select(model => new DuplicationModelMasterDetail(model));
			var candidateDetails = models.Select(model => new DuplicationModelCandidateDetail(model));

			return new DuplicationModelDetailGroup(modelGroup.Key, masterDetails, candidateDetails, masterColumns, candidateColumns);
		}

		static DuplicationModelDetailGroup GetModelDetailGroupForDetailDisplay(IGrouping<string, DeduplicationPresenterModel> modelGroup)
		{
			var masterColumns = GetMasterDisplayColumns(modelGroup);
			var candidateColumns = GetCandidateDisplayColumns(modelGroup);

			var modelGroups = modelGroup.GroupBy(model => (model.ChildMasterID, model.ChildTargetID)).ToArray();
			var masterDetails = modelGroups.Select(group => new DuplicationModelMasterDetail(group));
			var candidateDetails = modelGroups.Select(group => new DuplicationModelCandidateDetail(group));

			return new DuplicationModelDetailGroup(modelGroup.Key, masterDetails, candidateDetails, masterColumns, candidateColumns);
		}

		void SetProperty(string property, string value)
		{
			PropertyInfo propertyInfo;

			if (property is null || (propertyInfo = typeof(DuplicationModelDetail).GetProperty(property)) is null)
			{
				var errorMessage = Res.GetString("8ef559e0-c33d-4460-ae84-ceb1cfb93a6c", "A new column [{0}] has been added or an existing column has been renamed", property);
				ExceptionReporter.Instance.ReportDeveloperException("cd93a439-9b1b-40be-883c-7c6024e9acf8", errorMessage, new InvalidOperationException(errorMessage));

				return;
			}

			if (propertyInfo.PropertyType == typeof(ZBool))
			{
				propertyInfo.SetValue(this, new ZBool(value == "TRUE"));
			}
			else
			{
				propertyInfo.SetValue(this, new ZString(value));
			}
		}

		#endregion Implementation

		public sealed class DuplicationModelMasterDetail : DuplicationModelDetail
		{
			public DuplicationModelMasterDetail(DeduplicationPresenterModel presenterModel)
				: base(presenterModel)
			{
				if (!string.IsNullOrEmpty(presenterModel.ChildMasterValue))
				{
					SetProperty(dataSourceNameMap[presenterModel.ChildGroupNameForType], presenterModel.ChildMasterValue);
					Source = DedupeTranslationHelper.GetModelSource(presenterModel.ChildDisplayNameForMasterColumns);
				}
			}

			public DuplicationModelMasterDetail(IEnumerable<DeduplicationPresenterModel> presenterModels)
				: base(presenterModels)
			{
				presenterModels.ForEach(model => SetProperty(model.ChildDisplayNameForMasterColumns, model.ChildMasterValue));
			}

			protected override Guid GetSelfBizOPk(DeduplicationPresenterModel presenterModel) => presenterModel.ChildMasterID;

			protected override Guid GetOpponentBizoPk(DeduplicationPresenterModel presenterModel) => presenterModel.ChildTargetID;
		}

		public sealed class DuplicationModelCandidateDetail : DuplicationModelDetail
		{
			public DuplicationModelCandidateDetail(DeduplicationPresenterModel presenterModel)
				: base(presenterModel)
			{
				if (!string.IsNullOrEmpty(presenterModel.ChildTargetValue))
				{
					SetProperty(dataSourceNameMap[presenterModel.ChildGroupNameForType], presenterModel.ChildTargetValue);
					Source = DedupeTranslationHelper.GetModelSource(presenterModel.ChildDisplayNameForTargetColumns);
				}
			}

			public DuplicationModelCandidateDetail(IEnumerable<DeduplicationPresenterModel> presenterModels)
				: base(presenterModels)
			{
				presenterModels.ForEach(model => SetProperty(model.ChildDisplayNameForTargetColumns, model.ChildTargetValue));
			}

			protected override Guid GetSelfBizOPk(DeduplicationPresenterModel presenterModel) => presenterModel.ChildTargetID;

			protected override Guid GetOpponentBizoPk(DeduplicationPresenterModel presenterModel) => presenterModel.ChildMasterID;
		}
	}
}
