using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[DependentBusinessObject(typeof(GlbAccreditation), "Attempts")]
	public class GlbAccreditationAttempt : AutoGlbAccreditationAttempt, IGlbAccreditationAttempt, IDocumentSupportable, IWorkflowProvider, IEmailAddressGetterForTrigger
	{
		public GlbAccreditationAttempt(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState =>
			IsDeleting ? AutologState.NotLogged : AutologState.AutoLogged;

		public override bool SupportsNotes => false;

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("B08DDD56-E0AF-4A4F-B1D9-662A141B72DB", "Accreditation Attempt {0} - {1}", AccreditationCode, PersonFullName); }
		}

		public GlbAccreditation Accreditation
		{
			get { return Factory.Load<GlbAccreditation>(HAA_HAC); }
		}

		IGlbAccreditation IGlbAccreditationAttempt.Accreditation
		{
			get { return Accreditation; }
		}

		public ZBool IsStarted
		{
			get { return !HAA_CommencementDate.IsEmpty; }
		}

		public ZBool IsCompleted
		{
			get { return !HAA_CompletionDate.IsEmpty; }
		}

		public ZBool CheckIsExpired(ZDate checkDate)
		{
			return (checkDate > HAA_CompletionDueDate && IsStarted && !IsCompleted) || (IsCompleted && checkDate > HAA_ExpiryDate);
		}

		public ZString AccreditationCode
		{
			get { return Accreditation?.HAC_Code ?? ZString.Empty; }
		}

		public ZString AccreditationDescription
		{
			get { return Accreditation?.HAC_Description ?? ZString.Empty; }
		}

		public ZString PersonFullName
		{
			get { return Person?.PER_FullName ?? ZString.Empty; }
		}

		AccreditationPersonProxyCollection reqsProxyCollection;
		public IAccreditationPersonProxyCollection RequirementsProxyCollection
		{
			get
			{
				if (reqsProxyCollection == null)
				{
					reqsProxyCollection = new AccreditationPersonProxyCollection(Accreditation, Person);
					reqsProxyCollection.PopulateCollection();
				}

				return reqsProxyCollection;
			}
		}

		#region Certificate

		public ZString CertificateCode
		{
			get { return Accreditation?.HAC_CertificateCode ?? ZString.Empty; }
		}

		public GenRegCertAccredMaintList Certificate
		{
			get
			{
				if (certificate == null)
				{
					if (Person == null)
					{
						return null;
					}

					foreach (HRJobApplicant applicant in Person.ApplicantCollection)
					{
						var certificates = applicant?.Certificates;
						if (certificates != null)
						{
							var additionalFilter = certificates.AdditionalFilter;
							try
							{
								certificates.AdditionalFilter = new ZQuery(GenRegCertAccredMaintListSchema.XZ_IssueDate, SQLComparisonOperator.EqualToDatePartOnly, HAA_CompletionDate);
								certificate = certificates.GetFirstCertificate(CertificateCode);
							}
							finally
							{
								certificates.AdditionalFilter = additionalFilter;
							}

							if (certificate != null)
							{
								RegisterEditableChildObject(certificate);
								break;
							}
						}
					}
				}

				return certificate;
			}
		}
		GenRegCertAccredMaintList certificate;

		public ZString CertificateNumber
		{
			get { return Certificate?.XZ_RefNumber ?? ZString.Empty; }
		}

		public ZDate CertificateIssueDate
		{
			get { return (ZDate)(Certificate?.XZ_IssueDate ?? ZDate.Empty); }
		}

		public ZDate CertificateExpiryDate
		{
			get { return (ZDate)(Certificate?.XZ_ExpiryOrDueDate ?? ZDate.Empty); }
		}

		ZString status = ZString.Empty;
		public ZString Status
		{
			get
			{
				if (status == ZString.Empty)
				{
					if (IsCompleted)
					{
						if (CheckIsExpired(ZDate.Today))
						{
							status = AttemptStatus.Expired;
						}
						else
						{
							status = AttemptStatus.Completed;
						}
					}
					else if (CheckIsExpired(ZDate.Today))
					{
						status = AttemptStatus.FailedToComplete;
					}
					else if (IsStarted)
					{
						status = AttemptStatus.Started;
					}
				}

				return status;
			}
		}

		#endregion

		public static class AttemptStatus
		{
			public static string Started
			{
				get { return Res.GetString("931E0A0C-E387-46E7-89A2-0FA5B2627FD8", "Commenced"); }
			}

			public static string Completed
			{
				get { return Res.GetString("BDAD6A0F-90DD-4CF4-9097-753B76595D8E", "Completed"); }
			}

			public static string Expired
			{
				get { return Res.GetString("A1EB314A-224F-4EC1-B293-184DFDA08851", "Expired"); }
			}

			public static string FailedToComplete
			{
				get { return Res.GetString("AEDE32FA-B421-4FD4-9B3B-6A835565AFD7", "Failed To Complete"); }
			}
		}

		public ZString Progress
		{
			get
			{
				var attempts = Person.AccreditationAttemptCollection as GlbAccreditationAttemptCollection;
				if (attempts.Cast<GlbAccreditationAttempt>().Any(a => a.PK != PK && a.HAA_HAC == HAA_HAC && a.HAA_CommencementDate > HAA_CommencementDate))
				{
					return Res.GetString("302137B9-51EA-430D-BCF9-8F9967A23FDA", "N/A");
				}

				return string.Format(CultureInfo.InvariantCulture, "{0}/{1}",
					Accreditation.Groups.Cast<GlbAccreditationJobSkillGroup>()
						.Count(g => !g.GetCompletedDateForPerson(Person, this, false).IsEmpty), Accreditation.Groups.Count);
			}
		}

		decimal? averageWeightedScore;
		public ZDecimal AverageWeightedScore
		{
			get
			{
				if (!averageWeightedScore.HasValue)
				{
					decimal totalWeightedScore = 0;
					int totalWeight = 0;
					foreach (var group in Accreditation.Groups.Cast<GlbAccreditationJobSkillGroup>())
					{
						var groupAverageScore = group.GetAverageWeightedScoreForPerson(Person, HAA_CommencementDate, HAA_CompletionDueDate);
						totalWeightedScore += groupAverageScore * group.HJG_Threshold;
						totalWeight += group.HJG_Threshold;
					}

					averageWeightedScore = totalWeight > 0 ? totalWeightedScore / totalWeight : 0;
				}

				return averageWeightedScore.Value;
			}
		}

		GlbAccreditationAttemptTreeModel treeModel;
		public GlbAccreditationAttemptTreeModel TreeModel
		{
			get
			{
				if (treeModel == null)
				{
					treeModel = new GlbAccreditationAttemptTreeModel(this, Person);
					RegisterEditableChildObject(treeModel);
				}
				return treeModel;
			}
			set => treeModel = value;
		}

		IGlbAccreditationTreeModel IGlbAccreditationAttempt.TreeModel
		{
			get { return TreeModel; }
		}

		class PersonAccreditationAttempt : IPersonAccreditationAttempt
		{
			public Guid HAA_PK { get; set; }
			public Guid HAA_HAC { get; set; }
			public ZDate HAA_CommencementDate { get; set; }
			public ZDate HAA_CompletionDate { get; set; }
			public ZDate HAA_ExpiryDate { get; set; }
			public bool HAC_IsRefresher { get; set; }
			public string HAC_Code { get; set; }
			public string HAC_CertificateCode { get; set; }
		}

		class AccreditationChainLink
		{
			public Guid HAC_PK { get; set; }
			public string HAC_Code { get; set; }
			public string HAC_Code_Parent { get; set; }
			public bool HAC_IsRefresher { get; set; }
			public string HAC_CertificateCode { get; set; }
			public string HAC_CertificateCode_Parent { get; set; }
		}

		IEnumerable<IPersonAccreditationAttempt> chainOfPreRequesiteAttempts;
		public IEnumerable<IPersonAccreditationAttempt> ChainOfPreRequesiteAttempts
		{
			get
			{
				if (chainOfPreRequesiteAttempts == null)
				{
					chainOfPreRequesiteAttempts = GetChainOfAttempts(HAA_HAC);
					var specMappings = RecruiterDataRegistry.Instance.CertificateCodeSpecialialisationMapping.Value;
					var mapping = specMappings.Cast<CertificationCodeMapping>().FirstOrDefault(m => m.SpecialisationCode == Accreditation.HAC_CertificateCode);

					if (mapping != null)
					{
						var query = new ZQuery(GlbAccreditationSchema.HAC_CertificateCode, mapping.MainCode);
						query.AddToFilter(GlbAccreditationSchema.HAC_IsRefresher, false);

						var accred = Factory.LoadTop1<GlbAccreditation>(query);
						if (accred != null)
						{
							chainOfPreRequesiteAttempts = chainOfPreRequesiteAttempts.Union(GetChainOfAttempts(accred.PK.ToGuid()));
						}
					}
				}

				return chainOfPreRequesiteAttempts;
			}
		}

		protected ZDate? earliestAttemptCommencementDate;
		public ZDate EarliestAttemptCommencementDate
		{
			get
			{
				if (earliestAttemptCommencementDate.HasValue)
				{
					return earliestAttemptCommencementDate.Value;
				}

				var preReqAttempts = ChainOfPreRequesiteAttempts.Where(a => a.HAA_CommencementDate < HAA_CommencementDate);

				var latestAttempt = preReqAttempts.FirstOrDefault();
				if (latestAttempt == null || latestAttempt.HAA_ExpiryDate < HAA_CommencementDate)
				{
					earliestAttemptCommencementDate = HAA_CommencementDate;
				}
				else
				{
					var preReqAccreditations = GetAccreditationChainLinks();
					earliestAttemptCommencementDate = GetEarliestCommencementDate(preReqAccreditations, preReqAttempts, latestAttempt);
				}

				return earliestAttemptCommencementDate.Value;
			}
		}

		ZDate GetEarliestCommencementDate(IEnumerable<AccreditationChainLink> accreditations, IEnumerable<IPersonAccreditationAttempt> attempts, IPersonAccreditationAttempt currentAttempt)
		{
			var parentCerts = accreditations.Where(ac => ac.HAC_Code == currentAttempt.HAC_Code).Select(ac2 => ac2.HAC_CertificateCode_Parent);
			var parentCodes = accreditations.Where(ac => parentCerts.Contains(ac.HAC_CertificateCode) && ac.HAC_Code != currentAttempt.HAC_Code).Select(ac2 => ac2.HAC_Code);

			var aliveParents = attempts
				.Where(a =>
					parentCodes.Contains(a.HAC_Code)
					&&
						(
							a.HAA_ExpiryDate >= currentAttempt.HAA_CommencementDate
						));

			var parentDates = new List<ZDate>();
			foreach (var parent in aliveParents)
			{
				parentDates.Add(GetEarliestCommencementDate(accreditations, attempts, parent));
			}

			return parentDates.Count > 0 ? parentDates.Min() : currentAttempt.HAA_CommencementDate;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<AccreditationChainLink> GetAccreditationChainLinks()
		{
			if (HAA_HAC.IsEmpty)
			{
				return Array.Empty<AccreditationChainLink>();
			}

			var accreds = new List<AccreditationChainLink>();
			const string sql = @"
SELECT HAC_PK, HAC_Code, HAC_Code_Parent, HAC_IsRefresher, HAC_CertificateCode, HAC_CertificateCode_Parent
FROM dbo.GetAccreditationsRequirementsStructure(@MainAccreditationPK)"; // SQL Query text is untranslatable

			using (var cmd = Db.Connection.Command(sql)) // running a stored procedure
			{
				cmd.AddParameter("@MainAccreditationPK", SqlDbType.UniqueIdentifier, HAA_HAC.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var hac_pk = reader.GetGuid(0);
						var hac_code = reader.GetString(1);
						var hac_code_parent = reader.GetString(2);
						var isRefresher = reader.GetBoolean(3);
						var certCode = reader.GetString(4);
						var certCodeParent = reader.GetString(5);

						accreds.Add(new AccreditationChainLink()
						{
							HAC_PK = hac_pk,
							HAC_Code = hac_code,
							HAC_Code_Parent = hac_code_parent,
							HAC_IsRefresher = isRefresher,
							HAC_CertificateCode = certCode,
							HAC_CertificateCode_Parent = certCodeParent
						});
					}
				}
			}

			return accreds;
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<PersonAccreditationAttempt> GetChainOfAttempts(ZGuid mainAccredPk)
		{
			if (HAA_PER.IsEmpty)
			{
				return Array.Empty<PersonAccreditationAttempt>();
			}

			var attempts = new List<PersonAccreditationAttempt>();

			const string sql = @"
SELECT HAA_PK, HAA_HAC, HAA_CommencementDate, HAA_CompletionDate, HAA_ExpiryDate, HAC_IsRefresher, HAC_Code, HAC_CertificateCode
FROM dbo.GetCompletedRelatedAccreditationAttemptsWithRefreshers(@PersonPK, @MainAccreditationPK)"; // SQL Query text is untranslatable

			using (var cmd = Db.Connection.Command(sql))  // running a stored procedure
			{
				cmd.AddParameter("@PersonPK", SqlDbType.UniqueIdentifier, HAA_PER.ToGuid());
				cmd.AddParameter("@MainAccreditationPK", SqlDbType.UniqueIdentifier, mainAccredPk.ToGuid());

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var haa_pk = reader.GetGuid(0);
						var haa_hac = reader.GetGuid(1);
						var commenced = reader.GetDateTime(2);
						var completed = reader.GetDateTime(3);
						var expiry = reader.GetDateTime(4);
						var isRefresher = reader.GetBoolean(5);
						var accCode = reader.GetString(6);
						var certCode = reader.GetString(7);

						attempts.Add(new PersonAccreditationAttempt()
						{
							HAA_PK = haa_pk,
							HAA_HAC = haa_hac,
							HAA_CommencementDate = new ZDate(commenced.Year, commenced.Month, commenced.Day),
							HAA_CompletionDate = new ZDate(completed.Year, completed.Month, completed.Day),
							HAA_ExpiryDate = new ZDate(expiry.Year, expiry.Month, expiry.Day),
							HAC_IsRefresher = isRefresher,
							HAC_Code = accCode,
							HAC_CertificateCode = certCode
						});
					}
				}
			}

			return attempts.OrderByDescending(i => i.HAA_CommencementDate).ThenBy(i => i.HAC_IsRefresher);
		}

		public override ZDate HAA_CompletionDueDate
		{
			get => base.HAA_CompletionDueDate;
			set
			{
				base.HAA_CompletionDueDate = value;
				if (HAA_CompletionDate.IsEmpty)
				{
					HAA_ExpiryDate = HAA_CompletionDueDate;
				}
			}
		}

		public bool HAA_CompletionDueDate_ReadOnly => !HAA_CompletionDate.IsEmpty;

		public override ZDate HAA_CompletionDate
		{
			get { return base.HAA_CompletionDate; }
			set
			{
				base.HAA_CompletionDate = value;
				if (!HAA_CompletionDate.IsEmpty && HAA_CompletionDate.IsValid)
				{
					SetExpiryAndAddNewCertificate();
				}
			}
		}

		public ZString RelatedLocation => Person?.PrimarySource?.UNLOCO ?? ZString.Empty;

		[SuppressMessage("Enterprise", "EDI003", Justification = "Doesn't reach 50 chars")]
		void SetExpiryAndAddNewCertificate()
		{
			if (Person == null)
			{
				return;
			}

			IHRJobApplicant applicant = Person.ApplicantCollection.Cast<IHRJobApplicant>()
				.OrderByDescending(a => a.HA_SystemCreateTimeUtc)
				.FirstOrDefault();

			if (applicant == null)
			{
				return;
			}

			var certExists = Certificate != null;

			if (!certExists)
			{
				certificate = Factory.New<GenRegCertAccredMaintList>();
				RegisterEditableChildObject(certificate);

				certificate.XZ_RefNumber = ZString.Empty;
				certificate.XZ_IssueDate = HAA_CompletionDate;
				certificate.XZ_StateOrProvinceOfIssuance = ZString.Empty;
			}

			if (Accreditation.HAC_IsRefresher
				&&
					(Accreditation.HAC_RefresherCertificateExpiryType.IsEmpty
					|| Accreditation.HAC_RefresherCertificateExpiryType == RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault))
			{
				var lastAttempt = Person.GetLastAttempt(Accreditation.PK, PK) as GlbAccreditationAttempt;
				bool isPastExpiry = lastAttempt != null && HAA_CommencementDate > lastAttempt.HAA_ExpiryDate;
				bool isCompleted = lastAttempt != null && lastAttempt.IsCompleted;

				ZDate expiryDate = lastAttempt == null || !isCompleted || isPastExpiry
					? Accreditation.GetFullAccreditationExpiryDate(Person, HAA_CommencementDate)
					: lastAttempt.HAA_ExpiryDate;

				if (!expiryDate.IsValid)
				{
					HAA_ExpiryDate = HAA_CompletionDate.AddMonths(Accreditation.HAC_ValidityMonths);

					expiryDate = Person.ApplicantCollection.Cast<IHRJobApplicant>()
							.SelectMany(x => x.CertificatesBizoCollection.Cast<GenRegCertAccredMaintList>()
							.Where(c => c.XZ_Type == CertificateCode && c.XZ_ExpiryOrDueDate.IsValid))
							.Select(x => x.XZ_ExpiryOrDueDate.Date)
							.Concat(new[] { HAA_CompletionDate })
							.Max();
				}
				else
				{
					HAA_ExpiryDate = expiryDate.AddMonths(Accreditation.HAC_ValidityMonths);
				}

				if (!certExists)
				{
					certificate.XZ_ExpiryOrDueDate = expiryDate.AddMonths(Accreditation.HAC_ValidityMonths);
				}
			}
			else
			{
				HAA_ExpiryDate = HAA_CompletionDate.AddMonths(Accreditation.HAC_ValidityMonths);

				if (!certExists)
				{
					certificate.XZ_ExpiryOrDueDate = HAA_CompletionDate.AddMonths(Accreditation.HAC_ValidityMonths);
				}
			}

			if (!certExists)
			{
				certificate.XZ_ParentTableCode = HRJobApplicantSchema.Constants.Prefix;
				certificate.XZ_ParentID = applicant.PK;
				certificate.MasterParent = applicant as ICertificatesProvider;
				certificate.XZ_Type = CertificateCode;
				certificate.XZ_RN_NKCountryOfIssuance = Person.PER_RN_NKCountryInternal;
				certificate.XZ_Comment = string.Format(CultureInfo.CurrentCulture, (NoResString)"{0} Completed: {1}, Expiry: {2}", // Comment is visible only for edi and shouldn't be translated
					AccreditationCode, certificate.XZ_IssueDate.ToShortDateString(),
					certificate.XZ_ExpiryOrDueDate.ToShortDateString());
			}

			if (IsResyncRelatedSuspended)
			{
				return;
			}

			resyncSpecialisations = true;
		}

		bool resyncSpecialisations;

		protected virtual AccreditationUpdaterForPerson GetAccreditationUpdater()
		{
			return new AccreditationUpdaterForPerson();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			AddLogs();
		}

		protected override void OnFactorySaving()
		{
			AssignCertificateNumber();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			try
			{
				if (resyncSpecialisations)
				{
					resyncSpecialisations = false;
					if (saveSucceeded)
					{
						var specMappings = RecruiterDataRegistry.Instance.CertificateCodeSpecialialisationMapping.Value;
						var updater = GetAccreditationUpdater();
						updater.SetPersons(new[] { Person });
						foreach (CertificationCodeMapping mapping in specMappings)
						{
							if (mapping.MainCode.Equals(Accreditation.HAC_CertificateCode))
							{
								var query = new ZQuery(GlbAccreditationSchema.HAC_CertificateCode, mapping.SpecialisationCode);
								query.AddToFilter(GlbAccreditationSchema.HAC_IsRefresher, false);

								var accred = Factory.LoadTop1<GlbAccreditation>(query);
								if (accred == null)
								{
									continue;
								}

								var lastAttempt = accred.GetLastCompletedAttemptForPerson(Person);
								if (lastAttempt == null || lastAttempt.HAA_CommencementDate >= EarliestAttemptCommencementDate)
								{
									updater.ParentAccreditation = accred;
									updater.FromDate = EarliestAttemptCommencementDate;
									updater.ToDate = HAA_ExpiryDate;
									updater.IsFirstExamType = false;
									updater.AdditionalCompletionToleranceDays = 10;

									if (resyncRelatedParameters != null)
									{
										var parameters = resyncRelatedParameters;
										if (parameters != null && !parameters.IsCompletingExam)
										{
											updater.IsFirstExamType = parameters.IsFirstExamType;
											updater.AdditionalCompletionToleranceDays = parameters.AdditionalCompletionToleranceDays;
											updater.CompletionToleranceDays = parameters.CompletionToleranceDays;
											updater.ToDate = parameters.ToDate;
										}
									}

									updater.Run();
								}
							}
						}
					}
				}
			}
			finally
			{
				resyncSuspender?.Dispose();
				resyncSuspender = null;
			}
		}

		void AssignCertificateNumber()
		{
			if (Accreditation != null && Accreditation.HAC_IsAutoNumber && Certificate != null && Certificate.XZ_RefNumber.IsEmpty)
			{
				Certificate.XZ_RefNumber = Env.NumberFountains.AccreditationCertificateID.GetNextFormatted(Factory);
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);

			if (Person != null && (!IsInDatabase || HAA_CommencementDateInfo.HasChanges) && !HAA_CommencementDate.IsEmpty)
			{
				Person.Logs.HasChanges = true; //it allows the template creation
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(Person, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			}
		}

		void AddLogs()
		{
			var logs = Logs;
			if (logs != null)
			{
				var logReference = FormattableString.Invariant($"{AccreditationCode} - {AccreditationDescription}"); // Log message not to be translated

				logs.LogsNotInDB.Where(x => (x.SL_SE_NKEvent == AutoEvents.AccreditationAttemptCommencedCode || x.SL_SE_NKEvent == AutoEvents.AccreditationAttemptCompletedCode) && x.SL_Reference == logReference).DeleteAll();

				if ((!IsInDatabase || HAA_CommencementDateInfo.HasChanges) && !HAA_CommencementDate.IsEmpty)
				{
					logs.AddNew(new EventValue(AutoEvents.AccreditationAttemptCommenced, reference: logReference, eventTime: new ZDateTimeOffset(HAA_CommencementDate), deferFiringWorkflow: true));
				}

				if ((!IsInDatabase || HAA_CompletionDueDateInfo.HasChanges) && !HAA_CompletionDueDate.IsEmpty)
				{
					logs.AddNew(new EventValue(AutoEvents.AccreditationAttemptCompleted, reference: logReference, eventTime: new ZDateTimeOffset(HAA_CompletionDueDate), isEstimate: true, deferFiringWorkflow: true));
				}

				if ((!IsInDatabase || HAA_CompletionDateInfo.HasChanges) && !HAA_CompletionDate.IsEmpty)
				{
					logs.AddNew(new EventValue(AutoEvents.AccreditationAttemptCompleted, reference: logReference, eventTime: new ZDateTimeOffset(HAA_CompletionDate), deferFiringWorkflow: true));
				}
			}
		}

		#region Delete

		public override void Delete()
		{
			WorkflowItems.Reload(true);
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable]
		[ChildEditableTestExclude]
		public GlbAccreditationAttemptProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GlbAccreditationAttemptProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		GlbAccreditationAttemptProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			// Must match GlbAccreditationAttempt.GetTemplateSelectionCriteria
			// and GlbAccreditationAttemptFormCustomisationSettingProvider.GetPropertiesThatAffectWorkflow
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, AccreditationCode, ZString.Empty);
			return result;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.GlbAccreditationAttemptWorkflowDescriptorCode; }
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new GlbAccreditationAttemptDocumentSupporter(this); }
		}

		internal void SetTempResyncParams(bool resync, AccreditationUpdaterParameters parameters)
		{
			if (!resync)
			{
				resyncRelatedSuspend++;
			}
			resyncRelatedParameters = parameters;
			resyncSuspender?.Dispose();
			resyncSuspender = null;

			resyncSuspender = new DisposableAction(delegate
			{ resyncRelatedParameters = null; resyncRelatedSuspend--; });
		}

		IDisposable resyncSuspender;

		bool IsResyncRelatedSuspended
		{
			get { return resyncRelatedSuspend > 0; }
		}
		AccreditationUpdaterParameters resyncRelatedParameters;
		int resyncRelatedSuspend;

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			HAA_HAC = accred.PK;
		}

#endif
		#endregion

		public IEnumerable<string> GetEmailAddressesFromTriggerParty(string triggerParty)
		{
			return Person?.GetEmailAddressesFromTriggerParty(triggerParty) ?? Enumerable.Empty<string>();
		}
	}
}
