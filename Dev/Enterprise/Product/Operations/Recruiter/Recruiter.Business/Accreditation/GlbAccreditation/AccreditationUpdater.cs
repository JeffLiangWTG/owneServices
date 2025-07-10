using System;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public abstract class AccreditationUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string CompletionToleranceDays = "CompletionToleranceDays";
			public const string FromDate = "FromDate";
			public const string ToDate = "ToDate";
			public const string AdditionalCompletionToleranceDays = "AdditionalCompletionToleranceDays";
			public const string IsFirstExamType = "IsFirstExamType";
			public const string IsTimePeriodType = "IsTimePeriodType";
			public const string DeleteExistingCertificates = "DeleteExistingCertificates";
		}

		protected AccreditationUpdater(BusinessObjectFactory factory) : base(factory)
		{ }

		protected bool isCancelled;
		readonly int maxRetries = 5;

		#region Properties

		[List("Lookups.AccreditationList")]
		public ZGuid ParentAccreditationPK { get; set; }
		public bool ParentAccreditationPK_ReadOnly => IsForAccreditation;

		GlbAccreditation parentAccreditation;
		public GlbAccreditation ParentAccreditation
		{
			get
			{
				if (parentAccreditation == null || parentAccreditation.PK != ParentAccreditationPK)
				{
					parentAccreditation = !ParentAccreditationPK.IsEmpty ? Factory.Load<GlbAccreditation>(ParentAccreditationPK) : null;
				}

				return parentAccreditation;
			}
			set
			{
				parentAccreditation = value;
				ParentAccreditationPK = value?.PK ?? ZGuid.Empty;
			}
		}

		#region CompletionToleranceDays

		public ZInt CompletionToleranceDays
		{
			get => completionToleranceDays;
			set
			{
				if (completionToleranceDays != value)
				{
					SetNonPersistentPropertyValue(CompletionToleranceDaysInfo, ref completionToleranceDays, value);

					if (!IsValidationSuspended)
					{
						ValidateCompletionToleranceDays();
					}
				}
			}
		}
		ZInt completionToleranceDays;

		public ZPropertyInfo CompletionToleranceDaysInfo
		{
			get { return GetZPropertyInfo(Schema.CompletionToleranceDays); }
		}

		#endregion

		#region FromDate

		public ZDate FromDate
		{
			get => fromDate;
			set
			{
				if (fromDate != value)
				{
					SetNonPersistentPropertyValue(FromDateInfo, ref fromDate, value);

					if (!IsValidationSuspended)
					{
						ValidateFromDate();
					}
				}
			}
		}
		ZDate fromDate;

		public ZPropertyInfo FromDateInfo
		{
			get { return GetZPropertyInfo(Schema.FromDate); }
		}

		#endregion

		#region ToDate

		public ZDate ToDate
		{
			get => toDate;
			set
			{
				if (toDate != value)
				{
					SetNonPersistentPropertyValue(ToDateInfo, ref toDate, value);

					if (!IsValidationSuspended)
					{
						ValidateToDate();
					}
				}
			}
		}
		ZDate toDate;

		public ZPropertyInfo ToDateInfo
		{
			get { return GetZPropertyInfo(Schema.ToDate); }
		}

		#endregion

		#region AdditionalCompletionToleranceDays

		public ZInt AdditionalCompletionToleranceDays
		{
			get => additionalCompletionToleranceDays;
			set
			{
				if (additionalCompletionToleranceDays != value)
				{
					SetNonPersistentPropertyValue(AdditionalCompletionToleranceDaysInfo, ref additionalCompletionToleranceDays, value);

					if (!IsValidationSuspended)
					{
						ValidateAdditionalCompletionToleranceDays();
					}
				}
			}
		}
		ZInt additionalCompletionToleranceDays;

		public ZPropertyInfo AdditionalCompletionToleranceDaysInfo
		{
			get { return GetZPropertyInfo(Schema.AdditionalCompletionToleranceDays); }
		}

		#endregion

		#region Types

		public ZBool DeleteExistingCertificates
		{
			get => deleteExistingCertificates;
			set => SetNonPersistentPropertyValue(DeleteExistingCertificatesInfo, ref deleteExistingCertificates, value);
		}

		ZBool deleteExistingCertificates;

		public ZPropertyInfo DeleteExistingCertificatesInfo
		{
			get { return GetZPropertyInfo(Schema.DeleteExistingCertificates); }
		}

		public ZBool IsFirstExamType
		{
			get => !IsTimePeriodType;
			set
			{
				IsFirstExamTypeInfo.RefreshBinding();
				IsTimePeriodType = !value;
			}
		}

		public ZPropertyInfo IsFirstExamTypeInfo
		{
			get { return GetZPropertyInfo(Schema.IsFirstExamType); }
		}

		public ZBool IsTimePeriodType
		{
			get => isTimePeriodType;
			set => SetNonPersistentPropertyValue(IsTimePeriodTypeInfo, ref isTimePeriodType, value);
		}

		public ZPropertyInfo IsTimePeriodTypeInfo
		{
			get { return GetZPropertyInfo(Schema.IsTimePeriodType); }
		}

		ZBool isTimePeriodType;

		#endregion

		public AccreditationUpdaterLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new AccreditationUpdaterLookups(this);
				}

				return fLookups;
			}
		}

		AccreditationUpdaterLookups fLookups;

		#endregion

		#region Validation

		public void ValidateAll()
		{
			ValidateCompletionToleranceDays();
			ValidateFromDate();
			ValidateToDate();
			ValidateAdditionalCompletionToleranceDays();
		}

		public virtual void ValidateCompletionToleranceDays()
		{
			CompletionToleranceDaysInfo.ClearAllNotifications();
			if (IsFirstExamType)
			{
				MandatoryValidation.CheckNotNegative(CompletionToleranceDaysInfo);
				MandatoryValidation.CheckNotZero(CompletionToleranceDaysInfo);
			}
		}

		public virtual void ValidateFromDate()
		{
			FromDateInfo.ClearAllNotifications();

			if (IsTimePeriodType)
			{
				MandatoryValidation.CheckEntered(FromDateInfo);
				TypeValidation.CheckValidZDateAndRange(FromDateInfo);

				if (FromDate.IsValid)
				{
					if (FromDate > ZDate.Today)
					{
						FromDateInfo.AddError(DateInFutureErrorMessage);
					}

					if (ToDate.IsValid)
					{
						if (ToDate < FromDate)
						{
							FromDateInfo.AddError(ToDateBeforeFromDateErrorMessage);
						}
					}
				}
			}
		}

		public virtual void ValidateToDate()
		{
			ToDateInfo.ClearAllNotifications();

			if (IsTimePeriodType)
			{
				MandatoryValidation.CheckEntered(ToDateInfo);
				TypeValidation.CheckValidZDateAndRange(ToDateInfo);

				if (ToDate.IsValid)
				{
					if (ToDate > ZDate.Today)
					{
						ToDateInfo.AddError(DateInFutureErrorMessage);
					}

					if (FromDate.IsValid)
					{
						if (ToDate < FromDate)
						{
							ToDateInfo.AddError(ToDateBeforeFromDateErrorMessage);
						}
					}
				}
			}
		}

		public static string ToDateBeforeFromDateErrorMessage => Res.GetString("043d921d-39af-4ca7-ab29-afad2319b8cd", "To Date cannot be earlier than From Date.");
		public static string DateInFutureErrorMessage => Res.GetString("d883e50f-d811-4e47-aa16-ad1c37e6dbd7", "Date cannot be in the future.");

		public virtual void ValidateAdditionalCompletionToleranceDays()
		{
			AdditionalCompletionToleranceDaysInfo.ClearAllNotifications();
			if (IsTimePeriodType)
			{
				MandatoryValidation.CheckNotNegative(AdditionalCompletionToleranceDaysInfo);
			}
		}

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DeleteExistingCertificates = true;
		}

		#endregion

		public abstract ZBool IsForAccreditation { get; }
		public abstract ZBool IsForPerson { get; }

		public abstract void Run();

		public abstract void DeleteAttemptsAndCertificates(IGlbAccreditation targetAccreditation = null);

		public void TryDeleteAttemptsAndCertificates(IGlbAccreditation targetAccreditation = null)
		{
			var deleteRetryCounter = 1;
			bool retry;

			do
			{
				try
				{
					retry = false;
					DeleteAttemptsAndCertificates(targetAccreditation);
				}
				catch (ZSaveConcurrencyException)
				{
					if (deleteRetryCounter == maxRetries)
					{
						throw;
					}

					retry = true;
					deleteRetryCounter++;
					Thread.Sleep(100);
				}
			} while (retry);
		}

		protected void DeleteCertificatesForPerson(GlbPerson person, IGlbAccreditation targetAccreditation)
		{
			var accreditationAttemptsForPerson = person.AccreditationAttemptCollection.OfType<GlbAccreditationAttempt>().Where(x => targetAccreditation == null || targetAccreditation.PK == x.HAA_HAC);

			var certificateCodes = accreditationAttemptsForPerson.Select(a => a.CertificateCode).ToArray();
			var completionDates = accreditationAttemptsForPerson.Where(a => a.HAA_CompletionDate != ZDate.Empty).Select(a => a.HAA_CompletionDate).ToArray();
			person.Factory.Load<GenRegCertAccredMaintList>(GetCertificatesQuery(person, certificateCodes, completionDates))?.DeleteAll();
		}

		protected ZDBOnlyQuery GetCertificatesQuery(GlbAccreditation accreditation)
		{
			var query = new ZDBOnlyQuery(typeof(GenRegCertAccredMaintList));
			query.AddToFilter(GenRegCertAccredMaintListSchema.XZ_RefNumber, SQLComparisonOperator.StartsWith, "A");
			query.AddToFilter(GenRegCertAccredMaintListSchema.XZ_Type, accreditation.HAC_CertificateCode);
			query.AddToFilter(GenRegCertAccredMaintListSchema.XZ_Comment, SQLComparisonOperator.StartsWith, accreditation.HAC_Code);

			var attemptSubQuery = new ZDBOnlySubQuery(typeof(GlbAccreditationAttempt), GlbAccreditationAttemptSchema.HAA_CompletionDate);
			attemptSubQuery.AddToFilter(GlbAccreditationAttemptSchema.HAA_HAC, accreditation.PK);

			query.AddSubQuery(GenRegCertAccredMaintListSchema.XZ_IssueDate, attemptSubQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetCertificatesQuery(GlbPerson person, ZString[] certificateCodes, ZDate[] attemptCompletionDates)
		{
			var query = new ZDBOnlyQuery(typeof(GenRegCertAccredMaintList));
			query.AddToFilter(GenRegCertAccredMaintListSchema.XZ_RefNumber, SQLComparisonOperator.StartsWith, "A");
			query.AddToFilter(GenRegCertAccredMaintListSchema.XZ_Type, certificateCodes);
			query.AddToFilter(GenRegCertAccredMaintListSchema.XZ_IssueDate, attemptCompletionDates);

			var applicantSubQuery = new ZDBOnlySubQuery(typeof(HRJobApplicant), HRJobApplicantSchema.PK);
			var personSubQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
			personSubQuery.AddToFilter(GlbPersonSchema.PK, person.PK);

			applicantSubQuery.AddSubQuery(HRJobApplicantSchema.HA_PER, personSubQuery, JoinCondition.And);

			query.AddSubQuery(GenRegCertAccredMaintListSchema.XZ_ParentID, applicantSubQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetPersonsQuery(ZGuid accreditationPk)
		{
			var query = new ZDBOnlyQuery(typeof(GlbPerson));
			var applicantSubQuery = new ZDBOnlySubQuery(typeof(HRJobApplicant), HRJobApplicantSchema.HA_PER);

			var campaignItemSub = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID);
			campaignItemSub.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientTableCode,
				HRJobApplicantSchema.Constants.Prefix);

			var settingsQuery = new ZDBOnlySubQuery(typeof(ExamSetting), ExamSettingSchema.EXS_G0);
			var skillGroupPivotSub = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillPivot), GlbAccreditationJobSkillPivotSchema.HAJ_HS);
			var groupSub = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillPivotSchema.HAJ_HJG);
			if (!accreditationPk.IsEmpty)
			{
				groupSub.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentID, accreditationPk);
				groupSub.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentTableCode, GlbAccreditationSchema.Constants.Prefix);

				var groupSub2 = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillGroupSchema.PK);
				groupSub2.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentTableCode, GlbAccreditationJobSkillGroupSchema.Constants.Prefix);

				var innerGroupSub = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillGroupSchema.PK);
				innerGroupSub.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentID, accreditationPk);
				innerGroupSub.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentTableCode, GlbAccreditationSchema.Constants.Prefix);

				groupSub2.AddSubQuery(GlbAccreditationJobSkillGroupSchema.HJG_ParentID, innerGroupSub, JoinCondition.And);

				groupSub.AddAsUnionQuery(groupSub2);
			}

			skillGroupPivotSub.AddSubQuery(groupSub, JoinCondition.And);
			campaignItemSub.AddSubQuery(GlbCompanyCampaignItemSchema.G8_G0, settingsQuery, JoinCondition.And);

			applicantSubQuery.AddSubQuery(campaignItemSub, JoinCondition.And);

			query.AddSubQuery(GlbPersonSchema.PK, applicantSubQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetExamAttemptQuery(AccreditationUpdater updater, ZGuid accreditationPk, ZGuid[] applicantPks)
		{
			var query = new ZDBOnlyQuery(typeof(ExamAttempt));

			var campaignItemSub = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), ExamAttemptSchema.EXA_G8);
			if (applicantPks != null)
			{
				campaignItemSub.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientID, applicantPks);
			}

			var settingsQuery1 = new ZDBOnlySubQuery(typeof(ExamSetting), ExamSettingSchema.EXS_G0);
			var settingsQuery2 = new ZDBOnlySubQuery(typeof(ExamSetting), ExamSettingSchema.EXS_ExamVersion);

			var skillGroupPivotSub = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillPivot), GlbAccreditationJobSkillPivotSchema.HAJ_HS);
			var groupSub = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillPivotSchema.HAJ_HJG);
			if (!accreditationPk.IsEmpty)
			{
				groupSub.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentID, accreditationPk);
				groupSub.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentTableCode, GlbAccreditationSchema.Constants.Prefix);

				var groupSub2 = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillGroupSchema.PK);
				groupSub2.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentTableCode, GlbAccreditationJobSkillGroupSchema.Constants.Prefix);

				var innerGroupSub = new ZDBOnlySubQuery(typeof(GlbAccreditationJobSkillGroup), GlbAccreditationJobSkillGroupSchema.PK);
				innerGroupSub.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentID, accreditationPk);
				innerGroupSub.AddToFilter(GlbAccreditationJobSkillGroupSchema.HJG_ParentTableCode, GlbAccreditationSchema.Constants.Prefix);

				groupSub2.AddSubQuery(GlbAccreditationJobSkillGroupSchema.HJG_ParentID, innerGroupSub, JoinCondition.And);

				groupSub.AddAsUnionQuery(groupSub2);
			}

			skillGroupPivotSub.AddSubQuery(groupSub, JoinCondition.And);
			campaignItemSub.AddSubQuery(GlbCompanyCampaignItemSchema.G8_G0, settingsQuery1, JoinCondition.And);

			query.AddSubQuery(campaignItemSub, JoinCondition.And);

			if (updater.IsTimePeriodType)
			{
				query.AddToFilter(ExamAttemptSchema.EXA_TestCommencedUtc, SQLComparisonOperator.GreaterThanOrEqualTo, updater.FromDate);
				query.AddToFilter(ExamAttemptSchema.EXA_TestCompletedUtc, SQLComparisonOperator.LessThanOrEqualTo, updater.ToDate.AddDays(updater.AdditionalCompletionToleranceDays));
			}
			else
			{
				query.AddToFilter(ExamAttemptSchema.EXA_TestCommencedUtc, SQLComparisonOperator.NotEqual, DBNull.Value);
				query.AddToFilter(ExamAttemptSchema.EXA_TestCompletedUtc, SQLComparisonOperator.NotEqual, DBNull.Value);
			}

			query.AddSubQuery(ExamAttemptSchema.EXA_Version, settingsQuery2, JoinCondition.And);

			query.OrderBy = ExamAttemptSchema.EXA_TestCommencedUtc.Name + OrderByClause.Ascending;
			return query;
		}

		public void Cancel()
		{
			isCancelled = true;
		}

		public event EventHandler RunBegin;
		protected void OnRunBegin()
		{
			RunBegin?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler RunEnd;
		protected void OnRunEnd()
		{
			RunEnd?.Invoke(this, EventArgs.Empty);
		}

		protected void OnExamAttemptProcessed(long applicantsProcessed, long applicantsTotal, long examsProcessed, long examsTotal)
		{
			var args = new ExamAttemptProcessedEventArgs(applicantsProcessed, applicantsTotal, examsProcessed, examsTotal);
			examAttemptProcessed?.Invoke(this, args);
		}

		protected void OnPersonProcessed(long personsProcessed, long personsTotal)
		{
			var args = new PersonProcessedEventArgs(personsProcessed, personsTotal);
			personProcessed?.Invoke(this, args);
		}

		ExamAttemptProcessedEventHandler examAttemptProcessed;
		public event ExamAttemptProcessedEventHandler ExamAttemptProcessed
		{
			add { examAttemptProcessed += value; }
			remove { examAttemptProcessed -= value; }
		}

		PersonProcessedEventHandler personProcessed;
		public event PersonProcessedEventHandler PersonProcessed
		{
			add { personProcessed += value; }
			remove { personProcessed -= value; }
		}
	}

	public class AccreditationUpdaterParameters
	{
		public AccreditationUpdaterParameters(ZBool isFirstExamType, ZInt additionalCompletionToleranceDays, ZInt completionToleranceDays, ZDate toDate)
		{
			IsFirstExamType = isFirstExamType;
			AdditionalCompletionToleranceDays = additionalCompletionToleranceDays;
			CompletionToleranceDays = completionToleranceDays;
			ToDate = toDate;
			IsCompletingExam = false;
		}

		public AccreditationUpdaterParameters()
		{
			IsCompletingExam = true;
		}

		public ZBool IsFirstExamType { get; }
		public ZInt AdditionalCompletionToleranceDays { get; }
		public ZInt CompletionToleranceDays { get; }
		public ZBool IsCompletingExam { get; }
		public ZDate ToDate { get; }
	}
}
