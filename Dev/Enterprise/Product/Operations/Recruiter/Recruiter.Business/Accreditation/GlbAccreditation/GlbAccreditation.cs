using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditation : AutoGlbAccreditation, IGlbAccreditation
	{
		public GlbAccreditation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("EBCF5551-AFFA-4553-9073-04E5B63B01CD", "Accreditation"); }
		}

		#region Start & Complete Attempt

		public void StartAttempt(GlbPerson person)
		{
			StartAttempt(person, ZDateTime.UtcNow.Date);
		}

		public void StartAttempt(GlbPerson person, ZDate commencedDate)
		{
			if (ShouldCreateNewAttempt(person, commencedDate))
			{
				CreateNewAttempt(person, commencedDate);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It's just a collection of conditions")]
		public bool ShouldCreateNewAttempt(GlbPerson person, ZDate commencedDate)
		{
			if (person.Factory != Factory)
			{
				ErrorReporter.ReportOnce("GlbAccreditation.ShouldCreateNewAttempt.DifferentFactory", "You passed a GlbPerson with a different factory to GlbAccreditation.Factory.");
			}

			var shouldCreate = false;

			var attempt = GetCurrentAttemptForPerson(person, commencedDate);
			if (attempt == null)
			{
				var lastAttempt = GetLastAttemptForPerson(person);

				bool isPastExpiry = lastAttempt != null && commencedDate > lastAttempt.HAA_ExpiryDate;
				bool isExpiring = lastAttempt != null && !isPastExpiry && commencedDate > lastAttempt.HAA_ExpiryDate.AddMonths(-RecruiterDataRegistry.Instance.RefresherAttemptGracePeriodMonths.Value);
				bool isWithinExpiry = lastAttempt != null && commencedDate >= lastAttempt.HAA_CommencementDate && commencedDate <= lastAttempt.HAA_ExpiryDate;
				bool isCompleted = lastAttempt != null && lastAttempt.IsCompleted;
				bool isPastDue = lastAttempt != null && !isCompleted && commencedDate > lastAttempt.HAA_CompletionDueDate;
				bool isWithinDue = lastAttempt != null && !isCompleted && commencedDate >= lastAttempt.HAA_CommencementDate && commencedDate <= lastAttempt.HAA_CompletionDueDate;
				bool isStillAlive = lastAttempt != null && lastAttempt.ChainOfPreRequesiteAttempts.Any(a => a.HAA_ExpiryDate >= commencedDate);

				if (isCompleted && isWithinExpiry)
				{
					if (HAC_IsRefresher && isExpiring)
					{
						shouldCreate = true;
					}

					return shouldCreate;
				}

				if (!isCompleted && isWithinDue)
				{
					return shouldCreate;
				}

				var fullExpiryDate = GetFullAccreditationExpiryDate(person, commencedDate);
				bool isFirstTimer = !fullExpiryDate.IsValid && !HAC_IsRefresher && lastAttempt == null; // first timer, starts main accred
				bool noRefresherDoingMain = fullExpiryDate.IsValid && RefresherAccreditation == null; // has previously been certified but there is no refresher

				if (isCompleted && isPastExpiry && (fullExpiryDate.IsValid || noRefresherDoingMain) && !isStillAlive)
				{
					shouldCreate = true;
				}
				else if (!isCompleted && isPastDue)
				{
					shouldCreate = true;
				}
				else if (isFirstTimer)
				{
					shouldCreate = true;
				}
				else if (fullExpiryDate.IsValid && lastAttempt == null)
				{
					shouldCreate = true;
				}
				else if (isCompleted && isPastExpiry && !HAC_IsRefresher && !isStillAlive)
				{
					shouldCreate = true;
				}
			}

			return shouldCreate;
		}

		internal void CreateNewAttempt(GlbPerson person, ZDate commencedDate, AccreditationUpdater updater = null)
		{
			var attempt = Factory.New<GlbAccreditationAttempt>();
			attempt.HAA_HAC = PK;
			attempt.HAA_PER = person.PK;
			attempt.HAA_CommencementDate = commencedDate;

			if (updater != null && updater.IsFirstExamType)
			{
				attempt.HAA_CompletionDueDate = commencedDate.AddDays(updater.CompletionToleranceDays);
			}
			else if (updater != null && updater.IsTimePeriodType)
			{
				attempt.HAA_CompletionDueDate = updater.ToDate.AddDays(updater.AdditionalCompletionToleranceDays);
			}
			else
			{
				var suggestedDueDate = commencedDate.AddDays(HAC_MustCompleteInDays);

				foreach (var requirement in Requirements)
				{
					if (requirement != null)
					{
						var oldAttempt = person.GetLastAttempt(requirement.PK);

						if (oldAttempt != null && oldAttempt.IsCompleted)
						{
							var newSuggestedDueDate = oldAttempt.HAA_CompletionDate.AddDays(HAC_MustCompleteInDays);

							if (oldAttempt.IsCompleted && newSuggestedDueDate > suggestedDueDate)
							{
								suggestedDueDate = newSuggestedDueDate;
							}
						}
					}
				}

				attempt.HAA_CompletionDueDate = suggestedDueDate;
			}

			attempt.HAA_ExpiryDate = attempt.HAA_CompletionDueDate;

			person.AccreditationAttemptCollection.Add(attempt);
		}

		public void CompleteAttemptIfRequired(GlbPerson person)
		{
			CompleteAttemptIfRequired(person, ZDateTime.UtcNow.Date, false, new AccreditationUpdaterParameters());
		}

		public bool CompleteAttemptIfRequired(GlbPerson person, ZDate currentDate, bool useCache, AccreditationUpdaterParameters resyncParameters)
		{
			var attempt = GetCurrentAttemptForPerson(person, currentDate);
			if (attempt != null)
			{
				attempt.SetTempResyncParams(resyncParameters != null, resyncParameters);
			}

			return CompleteAttemptCore(person, currentDate, attempt, useCache);
		}

		bool CompleteAttemptCore(GlbPerson person, ZDate currentDate, GlbAccreditationAttempt attempt, bool useCache)
		{
			if (attempt != null && attempt.IsStarted && !attempt.IsCompleted && AllRequirementsCompleted(person, currentDate))
			{
				var maxGroupCompletedDate = ZDateTime.Empty;

				foreach (GlbAccreditationJobSkillGroup g in Groups)
				{
					var date = g.GetCompletedDateForPerson(person, attempt, useCache);

					if (date.IsEmpty)
					{
						return false;
					}

					if (date > maxGroupCompletedDate || maxGroupCompletedDate.IsEmpty)
					{
						maxGroupCompletedDate = date;
					}
				}

				if (!maxGroupCompletedDate.IsValid)
				{
					return false;
				}

				foreach (var req in Requirements)
				{
					var completionDate = person.GetLastAttempt(req.PK).HAA_CompletionDate;
					if (completionDate > maxGroupCompletedDate)
					{
						maxGroupCompletedDate = completionDate;
					}
				}

				if (attempt.HAA_CommencementDate > maxGroupCompletedDate)
				{
					return false;
				}

				attempt.HAA_CompletionDate = maxGroupCompletedDate.Date;

				if (attempt.HAA_CompletionDate.IsValid)
				{
					foreach (var accreditation in SubsequentAccreditations)
					{
						var attemptToExtend = person.GetCurrentAttempt(accreditation.PK, currentDate) as GlbAccreditationAttempt;
						if (attemptToExtend != null)
						{
							attemptToExtend.HAA_CompletionDueDate = maxGroupCompletedDate.AddDays(HAC_MustCompleteInDays).Date;
							attemptToExtend.HAA_ExpiryDate = attemptToExtend.HAA_CompletionDueDate;

							attemptToExtend.Accreditation.CompleteAttemptCore(person, currentDate, attemptToExtend, useCache);
						}
					}
				}

				return true;
			}

			return false;
		}

		public ZBool IsCompleted(GlbPerson person, ZDate date)
		{
			var lastAttempt = GetLastCompletedAttemptForPerson(person);
			if (lastAttempt == null)
			{
				return ZBool.False;
			}

			var result = lastAttempt.IsCompleted && !lastAttempt.CheckIsExpired(date);
			if (!result && !HAC_IsRefresher && RefresherAccreditation != null)
			{
				result = RefresherAccreditation.IsCompleted(person, date);
			}

			return result;
		}

		internal GlbAccreditationAttempt GetCurrentAttemptForPerson(GlbPerson person, ZDate currentDate)
		{
			return person.GetCurrentAttempt(PK, currentDate) as GlbAccreditationAttempt;
		}

		internal GlbAccreditationAttempt GetLastAttemptForPerson(GlbPerson person)
		{
			return person.GetLastAttempt(PK) as GlbAccreditationAttempt;
		}

		internal GlbAccreditationAttempt GetLastCompletedAttemptForPerson(GlbPerson person)
		{
			return person.GetLastAttempt(PK, true) as GlbAccreditationAttempt;
		}

		#endregion

		#region Properties

		public ZDate GetFullAccreditationExpiryDate(GlbPerson person, ZDate attemptCommencementDate)
		{
			if (!HAC_IsRefresher)
			{
				return ZDate.Empty;
			}

			var expiryDate = ZDate.Invalid;

			if (MainAccreditation != null)
			{
				var query = new ZQuery();
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_PER, person.PK);
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_CommencementDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, attemptCommencementDate);
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_CompletionDate, SQLComparisonOperator.NotEqual, DBNull.Value);
				query.AddToFilter(GlbAccreditationAttemptSchema.HAA_HAC, MainAccreditation.PK);
				query.OrderBy = GlbAccreditationAttemptSchema.Constants.HAA_CommencementDate + OrderByClause.Descending;

				var lastAttempt = Factory.LoadTop1<GlbAccreditationAttempt>(query);

				if (lastAttempt != null)
				{
					expiryDate = lastAttempt.HAA_ExpiryDate;
				}
			}

			return expiryDate;
		}

		public GlbAccreditation MainAccreditation
		{
			get
			{
				if (!HAC_IsRefresher)
				{
					return null;
				}

				if (mainAccreditation == null)
				{
					var mainAccreditationQuery = new ZQuery(GlbAccreditationSchema.HAC_CertificateCode, HAC_CertificateCode);
					mainAccreditationQuery.AddToFilter(GlbAccreditationSchema.HAC_IsRefresher, false);
					mainAccreditation = Factory.LoadTop1<GlbAccreditation>(mainAccreditationQuery);
				}

				return mainAccreditation;
			}
		}

		GlbAccreditation mainAccreditation;

		public bool AllRequirementsCompleted(GlbPerson person, ZDate date)
		{
			return RequirementPivotCollection.All(r => r.AccreditationParent.IsCompleted(person, date));
		}

		public GlbAccreditation RefresherAccreditation
		{
			get
			{
				if (HAC_IsRefresher)
				{
					return null;
				}

				if (refresherAccreditation == null)
				{
					var refresherAccreditationQuery = new ZQuery(GlbAccreditationSchema.HAC_CertificateCode, HAC_CertificateCode);
					refresherAccreditationQuery.AddToFilter(GlbAccreditationSchema.HAC_IsRefresher, true);
					refresherAccreditation = Factory.LoadTop1<GlbAccreditation>(refresherAccreditationQuery);
				}

				return refresherAccreditation;
			}
		}

		GlbAccreditation refresherAccreditation;

		[List("Lookups.CertificateCodesList")]
		public override ZString HAC_CertificateCode
		{
			get => base.HAC_CertificateCode;
			set
			{
				base.HAC_CertificateCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateHAC_IsRefresher();
				}
			}
		}

		public override ZBool HAC_IsRefresher
		{
			get => base.HAC_IsRefresher;
			set
			{
				base.HAC_IsRefresher = value;

				if (!HAC_IsRefresher)
				{
					HAC_RefresherCertificateExpiryType = ZString.Empty;
				}
			}
		}

		[ReadOnlyMember(nameof(HAC_RefresherCertExpirationType_ReadOnly))]
		[List("Lookups.RefresherCertExpirationTypesList")]
		[ResourceStringData("NPBO:Enterprise.Recruiter.Business.GlbAccreditation|RefresherCertExpiration", Caption = "Refresher Cert. Expiration")]
		public override ZString HAC_RefresherCertificateExpiryType
		{
			get => base.HAC_RefresherCertificateExpiryType;
			set => base.HAC_RefresherCertificateExpiryType = value;
		}

#if DEBUG

		public bool HAC_IsRefresher_ReadOnly
		{
			get { return Globals.IsTest; }
		}

		public bool HAC_CertificateCode_ReadOnly
		{
			get { return Globals.IsTest; }
		}

#endif

		public bool HAC_RefresherCertExpirationType_ReadOnly => !HAC_IsRefresher;

		public override ZBool HAC_IsAutoNumber
		{
			get => base.HAC_IsAutoNumber;
			set
			{
				base.HAC_IsAutoNumber = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateHAC_IsRefresher();
				}
			}
		}

		#endregion

		#region Overrides

		public override void Delete()
		{
			base.Delete();

			Attempts.RemoveAndDeleteAll();
			RequirementPivotCollection.DeleteAll();
			Groups.RemoveAndDeleteAll();
		}

		#endregion

		#region Collections

		GlbAccreditationAttemptCollection attempts;
		public GlbAccreditationAttemptCollection Attempts
		{
			get { return attempts ?? (attempts = new GlbAccreditationAttemptCollection(this)); }
		}

		IGlbAccreditationJobSkillGroupCollection IGlbAccreditation.Groups
		{
			get { return Groups; }
		}

		GlbAccreditationJobSkillGroupCollection groups;
		[ChildEditable]
		public GlbAccreditationJobSkillGroupCollection Groups
		{
			get
			{
				if (groups == null)
				{
					groups = new GlbAccreditationJobSkillGroupCollection(this);
					groups.Load();
					RegisterEditableChildObject(groups);
				}

				return groups;
			}
		}

		GlbAccreditationRequirementPivotCollection requirementPivotCollection;
		[ChildEditable]
		public GlbAccreditationRequirementPivotCollection RequirementPivotCollection
		{
			get
			{
				if (requirementPivotCollection == null)
				{
					requirementPivotCollection = new GlbAccreditationRequirementPivotCollection(this);
					RegisterEditableChildObject(requirementPivotCollection);
				}

				return requirementPivotCollection;
			}
		}

		GlbAccreditationDependentCollection subsequentAccreditations;
		public GlbAccreditationDependentCollection SubsequentAccreditations
		{
			get
			{
				if (subsequentAccreditations == null)
				{
					var query = new ZDBOnlyQuery(typeof(GlbAccreditation));

					var pivotSubQuery = new ZDBOnlySubQuery(typeof(GlbAccreditationRequirementPivot), GlbAccreditationRequirementPivotSchema.HAR_HAC);
					pivotSubQuery.AddToFilter(GlbAccreditationRequirementPivotSchema.HAR_HAC_Parent, PK);

					query.AddSubQuery(pivotSubQuery, JoinCondition.And);

					subsequentAccreditations = new GlbAccreditationDependentCollection(this, query);
					subsequentAccreditations.Load();
				}

				return subsequentAccreditations;
			}
		}

		GlbAccreditationDependentCollection requirements;
		[ChildEditable]
		public GlbAccreditationDependentCollection Requirements
		{
			get
			{
				if (requirements == null)
				{
					var query = new ZDBOnlyQuery(typeof(GlbAccreditation));

					var pivotSubQuery = new ZDBOnlySubQuery(typeof(GlbAccreditationRequirementPivot), GlbAccreditationRequirementPivotSchema.HAR_HAC_Parent);
					pivotSubQuery.AddToFilter(GlbAccreditationRequirementPivotSchema.HAR_HAC, PK);

					query.AddSubQuery(pivotSubQuery, JoinCondition.And);

					requirements = new GlbAccreditationDependentCollection(this, query);
					requirements.Load();
					RegisterEditableChildObject(requirements);
				}

				return requirements;
			}
		}

		IGlbAccreditationDependantCollection IGlbAccreditation.Requirements => Requirements;

		#endregion

		GlbAccreditationTreeModel glbAccreditationTreeModel;

		public GlbAccreditationTreeModel GlbAccreditationTreeModel
		{
			get
			{
				if (glbAccreditationTreeModel == null)
				{
					glbAccreditationTreeModel = new GlbAccreditationTreeModel(this);
					RegisterEditableChildObject(glbAccreditationTreeModel);
				}
				return glbAccreditationTreeModel;
			}
			set => glbAccreditationTreeModel = value;
		}

		IGlbAccreditationTreeModel IGlbAccreditation.GlbAccreditationTreeModel
		{
			get { return GlbAccreditationTreeModel; }
		}

		internal void ClearCache()
		{
			foreach (GlbAccreditationJobSkillGroup @group in Groups)
			{
				@group.ClearCache();
			}
		}
	}
}
