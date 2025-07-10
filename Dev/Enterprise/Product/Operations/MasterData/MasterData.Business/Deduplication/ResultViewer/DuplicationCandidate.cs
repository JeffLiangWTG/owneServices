using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public abstract class DuplicationCandidate : NonPersistentBusinessObject
	{
		protected DuplicationCandidate(DeduplicationPresenterModel presenterModel, DeduplicationOrgHeader orgHeader)
			: this(presenterModel)
		{
			if (orgHeader is null)
			{
				return;
			}

			Name = orgHeader.OH_FullName;
			Type = orgHeader.OrganisationTypesAsString;
			IsActive = orgHeader.OH_IsActive;
			IsDummy = orgHeader.IsDummy;
		}

		protected DuplicationCandidate(DeduplicationPresenterModel presenterModel, DeduplicationGlbPerson glbPerson)
			: this(presenterModel)
		{
			if (glbPerson is null)
			{
				Type = TranslatedMasterIsNullDescription;
				return;
			}

			Name = glbPerson.PER_FullName;
			Type = glbPerson.Master.Info;
			IsActive = glbPerson.PER_IsActive;
			IsDummy = glbPerson.IsDummy;
		}

		DuplicationCandidate(DeduplicationPresenterModel presenterModel)
		{
			Confidence = presenterModel?.Confidence ?? ConfidenceRating.Undefined;
			Score = presenterModel?.MainScore ?? 0;
		}

		protected DuplicationCandidate()
		{
			IsDummy = true;
			Status = Res.GetString("777f038d-e5ae-42d7-bbfa-be29b14a4079", "No duplicates");
		}

		public ZPropertyInfo ConfidenceDescriptionInfo => GetZPropertyInfo(nameof(ConfidenceDescription));
		public ZString ConfidenceDescription => DedupeTranslationHelper.GetConfidenceDescription(Confidence.ToString());

		public ZPropertyInfo ConfidenceScoreInfo => GetZPropertyInfo(nameof(ConfidenceScore));
		public ZString ConfidenceScore => ZString.Format("{0}%", Confidence == ConfidenceRating.None ? 0 : Score * 100);

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));
		public ZString Type { get; }

		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));
		public ZString Name { get; }

		public ZPropertyInfo ActiveInfo => GetZPropertyInfo(nameof(Active));
		public ZString Active => IsActive ? Res.GetString("ccbfaa02-e4c9-4d8f-a8c3-7816a270aa43", "Yes") : Res.GetString("47ad4c8f-cd31-40a2-bb48-afcb40bb1cfb", "No");

		#region MDM Admin Panel

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(nameof(Status));
		public ZString Status
		{
			get => status;
			private set
			{
				status = value;
				StatusInfo.RefreshBinding();
			}
		}
		string status;

		public ZPropertyInfo IgnoredByStaffInfo => GetZPropertyInfo(nameof(IgnoredByStaff));
		public ZString IgnoredByStaff
		{
			get => ignoredByStaff;
			private set
			{
				ignoredByStaff = value;
				IgnoredByStaffInfo.RefreshBinding();
			}
		}
		string ignoredByStaff;

		public ZPropertyInfo IgnoredForEveryoneStaffInfo => GetZPropertyInfo(nameof(IgnoredForEveryoneStaff));
		public ZString IgnoredForEveryoneStaff
		{
			get => ignoredForEveryoneStaff;
			private set
			{
				ignoredForEveryoneStaff = value;
				IgnoredForEveryoneStaffInfo.RefreshBinding();
			}
		}
		string ignoredForEveryoneStaff;

		public bool IsIgnored
		{
			get
			{
				var isIgnored = false;

				if (!string.IsNullOrEmpty(IgnoredForEveryoneStaff))
				{
					isIgnored = true;
				}
				else if (!string.IsNullOrEmpty(IgnoredByStaff))
				{
					var staffs = IgnoredByStaff.Split(',').ToList();
					if (staffs.Any(s => s.Trim().Equals(GlbStaff.CurrentUser.GS_Code)))
					{
						isIgnored = true;
					}
				}

				return isIgnored;
			}
		}

		public virtual void SetAdminPanelProperties(PatternMatchingResult matchingResult)
		{
			if (!string.IsNullOrEmpty(matchingResult?.FinalStatus))
			{
				Status = matchingResult.FinalStatusDescription;
				IgnoredByStaff = matchingResult.ExcludedByList;
				IgnoredForEveryoneStaff = matchingResult.ExcludedByForEveryone;
			}
		}

		#endregion MDM Admin Panel

		public Guid TargetPK { get; set; }
		public ConfidenceRating Confidence { get; }
		public double Score { get; }
		public bool IsActive { get; set; }
		public bool IsDummy { get; }
		public bool IsSameCountryAsMaster { get; set; } = true;
		protected abstract string TranslatedMasterIsNullDescription { get; }

		public IEnumerable<DeduplicationPresenterModel> DeduplicationPresenterModels { get; set; }
	}
}
