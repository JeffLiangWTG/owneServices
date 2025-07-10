using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruitment.Common
{
	public static class RatingValues
	{
		public const int Unset = -1;
		public const int Suitable = 1;
		public const int Potential = 2;
		public const int Unsuitable = 3;

		public static string NameFromValue(int val)
		{
			switch (val)
			{
				case -1:
					return nameof(Unset);
				case 1:
					return nameof(Suitable);
				case 2:
					return nameof(Potential);
				case 3:
					return nameof(Unsuitable);
				default:
					return (NoResString)"(unknown)";
			}
		}
	}

	public class Candidate : NonPersistentBusinessObject
	{
		public HRJobApplication Application { get; }
		public HRJobApplicant Applicant => Application?.Applicant;
		public HRJobRole Role => Application?.JobOpening?.JobRole;
		public HRJobApplicationDocument Document => Application?.Documents.MaxBySafe(d => d.HPD_SystemCreateTimeUtc);
		public StorageDocsBase Resume => FindResume(Application) ?? FindResume(Applicant);
		public GlbStaff ResponsibleRecruiter => Application.JobOpening.ControlledBy;

		public TaskViewBusinessObjectCollection TasksView
		{
			get
			{
				if (tasksView == null)
				{
					tasksView = new TaskViewBusinessObjectCollection(Factory, Application.WorkflowItems.Tasks);
					tasksView.AllowNew = false;
				}
				return tasksView;
			}
		}
		TaskViewBusinessObjectCollection tasksView;

		public IEnumerable<StmALog> Logs => Application
			.Logs
			.Find(l => l.SL_Parent == Application.PK);

		public bool HasActiveRejectionLogs => Logs
			.Where(l => l.Event.SE_Code == AutoEvents.RejectionEmailQueued.Code)
			.Any(l => !l.IsCancelled);

		public IEnumerable<StmALog> EventLogs => Logs
			.Where(l => l.Event != null && l.Event.SE_Code == AutoEvents.RecruitmentCandidateEvent.Code);

		public IEnumerable<StmALog> RejectionLogs => Logs.Where(l => l.Event.SE_Code == AutoEvents.RejectionEmailQueued.Code);

		public ZPropertyInfo RatingInfo => GetZPropertyInfo(nameof(Rating));
		public ZInt Rating
		{
			get => int.TryParse(Application.HP_ApplicationOverallRating, out var parsedRating) ? parsedRating : RatingValues.Unset;
			set
			{
				var factory = new BusinessObjectFactory();
				var currentApp = factory.Load<HRJobApplication>(Application.PK) ?? Application;

				if (currentApp.HP_ApplicationOverallRating != value.ToString())
				{
					var old = int.TryParse(currentApp.HP_ApplicationOverallRating, out var parsedRating) ? parsedRating : RatingValues.Unset;
					Application.HP_ApplicationOverallRating = value.ToString();

					Application.LogRCEEvent(
						HRJobApplicationEvent.RatingChanged,
						FormattableString.Invariant($"{RatingValues.NameFromValue(old)} => {RatingValues.NameFromValue(value)}"));

					OnCandidateRatingChanged?.Invoke(this, new CandidateRatingChangeEventArgs(Rating));

					ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null);

					RatingInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(nameof(Status));

		[MaxLength(3)]
		public ZString Status
		{
			get => Application.HP_CurrentStatus;
			set
			{
				var factory = new BusinessObjectFactory();
				var currentApp = factory.Load<HRJobApplication>(Application.PK) ?? Application;

				if (currentApp.HP_CurrentStatus != value)
				{
					var old = currentApp.HP_CurrentStatus;
					Application.HP_CurrentStatus = value;

					Application.LogRCEEvent(
						HRJobApplicationEvent.StatusChanged,
						FormattableString.Invariant($"{old} => {value}"));

					ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null);

					StatusInfo.RefreshBinding();
				}
			}
		}

		public ZWrappedPropertyInfo Rating_SuitableInfo => GetWrappedZPropertyInfo(nameof(Rating), x => RatingInfo);
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public ZBool Rating_Suitable
		{
			get => Rating == RatingValues.Suitable;
			set => SetRating(RatingValues.Suitable);
		}

		public ZWrappedPropertyInfo Rating_PotentialInfo => GetWrappedZPropertyInfo(nameof(Rating), x => RatingInfo);
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public ZBool Rating_Potential
		{
			get => Rating == RatingValues.Potential;
			set => SetRating(RatingValues.Potential);
		}

		public HRTaskStage Stage
		{
			get
			{
				var processTasks = Application.WorkflowItems.Tasks.Cast<ProcessTask>().OrderBy(l => l.P9_Sequence);
				var currentTask = processTasks
					.FirstOrDefault(l => l.P9_Status == "ASN" || l.P9_Status == "OPN" || (l.P9_Status == "CLS" && l.P9_Outcome == "FAI"));
				var lastTask = processTasks
					.LastOrDefault();

				return currentTask is null ?
						lastTask != null ?
							new HRTaskStage { Sequence = int.MaxValue, Description = (NoResString)"Job completed" } :
							new HRTaskStage { Sequence = int.MaxValue, Description = string.Empty } :
						new HRTaskStage { Sequence = currentTask.P9_Sequence, Description = currentTask.P9_Description };
			}
		}

		public ZWrappedPropertyInfo Rating_UnsuitableInfo => GetWrappedZPropertyInfo(nameof(Rating), x => RatingInfo);
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public ZBool Rating_Unsuitable
		{
			get => Rating == RatingValues.Unsuitable;
			set => SetRating(RatingValues.Unsuitable);
		}

		void SetRating(int value) => Rating = Rating != value ? value : RatingValues.Unset;

		public ZString PreviousExperienceDetails
		{
			get
			{
				if (Document == null)
				{
					return null;
				}
				return Document.HPD_PastRoles.IsEmpty ? GetPreviousExperienceDetails() : Document.HPD_PastRoles;
			}
			set
			{
				Document.HPD_PastRoles = value;
			}
		}

		public ZPropertyInfo SourceInfo => GetZPropertyInfo(nameof(Source));

		[MaxLength(3)]
		public ZString Source
		{
			get => Application.HP_SourceType;
			set
			{
				var factory = new BusinessObjectFactory();
				var currentApp = factory.Load<HRJobApplication>(Application.PK) ?? Application;

				if (currentApp.HP_SourceType != value)
				{
					var old = currentApp.HP_SourceType;
					Application.HP_SourceType = value;

					Application.LogRCEEvent(
						HRJobApplicationEvent.SourceChanged,
						FormattableString.Invariant($"{old} => {value}"));

					ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null);

					SourceInfo.RefreshBinding();
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Placeholder for future functionality")]
		public ZString OtherCommentary => "This will be added to the table during the cleanup phase";

		public CommunicationContactRowBusinessObjectCollection CommunicationContactRows
			=> communicationContactRows ?? (communicationContactRows = new CommunicationContactRowBusinessObjectCollection(Factory, this));
		CommunicationContactRowBusinessObjectCollection communicationContactRows;

		public Candidate(BusinessObjectFactory factory, ZGuid applicationPk)
			: this(factory, factory.Load<HRJobApplication>(applicationPk)) { }

		public Candidate(HRJobApplication application)
			: this(application.Factory, application) { }

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		Candidate(BusinessObjectFactory factory, HRJobApplication application)
			: base(factory)
		{
			Application = Argument.NotNull(application, nameof(application));

			if (!Application.IsNull)
			{
				applicationGuid = Application.PK;
				RegisterEditableChildObject(Applicant);
				RegisterEditableChildObject(Application);
				Application.DeletedByDataRefresh += (o, e) => Delete(true);
			}
		}

		Candidate(BusinessObjectFactory factory)
		: base(factory)
		{ }

		public static Candidate CreateUncommittedRow(BusinessObjectFactory factory)
			=> new Candidate(factory);

		static StorageDocsBase FindResume(IDocManagerSupport host)
			=> host?.DocManagerInfo.AllEDocs.Cast<StorageDocsBase>().Where(IsViewableResume).MaxBySafe(doc => doc.SC_Date);

		static bool IsViewableResume(StorageDocsBase doc)
			=> doc.SC_DocType == RecruiterDataRegistry.Instance.DocTypeCVCode && PreviewableDocumentHelper.IsSupported(doc.SC_DataType);

		protected virtual ZString BuildPrevExpDetails(IXPathNavigable doc)
		{
			var employerNodeList = ((XmlDocument)doc).SelectNodes(@"Resume/StructuredXMLResume/EmploymentHistory/EmployerOrg");
			var prevExpStringBuilder = new StringBuilder();
			const string emptyRecord = "???";
			foreach (XmlNode employer in employerNodeList)
			{
				var employerName = employer.SelectSingleNode("EmployerOrgName")?.InnerText ?? emptyRecord;
				var positionNodeList = employer.SelectNodes("PositionHistory");
				foreach (XmlNode position in positionNodeList)
				{
					var title = position.SelectSingleNode((NoResString)"Title")?.InnerText ?? emptyRecord;
					_ = prevExpStringBuilder.Append($"{title} - {employerName}{System.Environment.NewLine}");
				}
			}
			return prevExpStringBuilder.ToString().TrimEnd();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "default exception message")]
		protected ZString GetPreviousExperienceDetails()
		{
			const string defaultMessage = "Error: The previous experience details could not be read.";
			if (Document == null || string.IsNullOrEmpty(Document.HPD_Content))
			{
				return ZString.Empty;
			}
			var doc = new XmlDocument { PreserveWhitespace = true };
			try
			{
				doc.LoadXml(Document.HPD_Content);
				return BuildPrevExpDetails(doc);
			}
			catch (XmlException ex)
			{
				ErrorReporter.ReportOnce($"Unable to load the xml for this candidate", ex);
				return defaultMessage;
			}
			catch (XPathException ex)
			{
				ErrorReporter.ReportOnce($"Unable to parse the employment history for this candidate", ex);
				return defaultMessage;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce($"Unknown exception while trying to read employment history for this candidate", ex);
				return defaultMessage;
			}
		}

		protected override ZString HumanReadableNameCore
			=> Res.GetString("32559c07-67bb-48bb-a2c8-17d3b64c8b5f", "Candidate");

		protected override ZString HumanReadableShortcutNameCore
			=> FormattableString.Invariant($"{HumanReadableName}: {Applicant?.HA_FullName}");

		public override bool Equals(object obj) => obj is Candidate other && (other.Application?.PK.Equals(applicationGuid) ?? false);
		public override int GetHashCode() => applicationGuid.GetHashCode();
		readonly ZGuid applicationGuid;

		public override string ToString() => FormattableString.Invariant($"Candidate({Applicant?.HA_FullName})");
		public GroupedEConversation EConversation => Application.EConversation;

		#region Create Work Item

		IEnumerable<WorkItemTemplateProperties> workItemTemplatePropertiesInRegistry
			=> RecruitmentDataRegistry.Instance.WorkItemTemplateProperties.Value.Cast<WorkItemTemplateProperties>();

		public CodeDescriptionPairList PairListFromWorkItemList
		{
			get
			{
				if (pairListFromWorkItemList == null)
				{
					pairListFromWorkItemList = new CodeDescriptionPairList();
				}

				pairListFromWorkItemList.Clear();

				foreach (var t in workItemTemplatePropertiesInRegistry)
				{
					if (!string.IsNullOrEmpty(t.FriendlyName))
					{
						pairListFromWorkItemList.AddPair(t.FriendlyName, (NoResString)"template");
					}
				}

				return pairListFromWorkItemList;
			}
		}

		CodeDescriptionPairList pairListFromWorkItemList;

		public WorkItemTemplateProperties SelectedTemplate
		{
			get
			{
				if (_selectedTemplate == null)
				{
					_selectedTemplate = workItemTemplatePropertiesInRegistry.FirstOrDefault();
				}
				return _selectedTemplate;
			}
			set => _selectedTemplate = value;
		}
		WorkItemTemplateProperties _selectedTemplate;

		[List("PairListFromWorkItemList")]
		public ZString SelectedTemplateDescription
		{
			get => SelectedTemplate != null
					? SelectedTemplate.FriendlyName
					: (ZString)string.Empty;
			set => SelectedTemplate = workItemTemplatePropertiesInRegistry.SingleOrDefault(t => t.FriendlyName.ToUpper() == value.ToUpper());
		}
		#endregion

		#region Event Handler

		public event EventHandler<CandidateRatingChangeEventArgs> OnCandidateRatingChanged;

		#endregion
	}
}
