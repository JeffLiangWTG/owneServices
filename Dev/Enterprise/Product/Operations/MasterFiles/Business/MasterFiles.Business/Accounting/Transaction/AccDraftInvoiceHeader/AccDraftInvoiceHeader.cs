using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.AIH_InternalReference)]
	public class AccDraftInvoiceHeader : AutoAccDraftInvoiceHeader, IDocManagerSupport, IWorkflowProvider, IEDocsParsingSupport, IConversationProvider, IConversationParentHyperlinkProvider, ISupportAccProcessLogging, IConversationEmailBehaviorProvider
	{
		public AccDraftInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public AccDraftInvoiceJobClusterCollection JobClusters
		{
			get
			{
				if (jobClusters == null)
				{
					jobClusters = new AccDraftInvoiceJobClusterCollection(this, Factory);
					jobClusters.Load();
				}
				return jobClusters;
			}
		}
		AccDraftInvoiceJobClusterCollection jobClusters;

		public AccDraftInvoiceJobReferenceCollection JobReferences
		{
			get
			{
				if (jobReferences == null)
				{
					jobReferences = new AccDraftInvoiceJobReferenceCollection(this, Factory);
					jobReferences.Load();
				}
				return jobReferences;
			}
		}
		AccDraftInvoiceJobReferenceCollection jobReferences;

		public ZString CreditorName => Creditor?.OH_FullName ?? ZString.Empty;

		public ZString OriginalTransactionDescription => OriginalTransaction?.AH_Desc ?? ZString.Empty;

		public bool IsInLocalCurrency => AIH_RX_NKTransactionCurrency == Company.LocalCurrency.Code;

		public override ZGuid AIH_AH_PostedTransactionHeader
		{
			get
			{
				return base.AIH_AH_PostedTransactionHeader;
			}
			set
			{
				if (AIH_AH_PostedTransactionHeader == value)
				{
					return;
				}

				var newStatus = AIH_Status;
				if (!AIH_AH_PostedTransactionHeader.IsEmpty && value.IsEmpty)
				{
					//The only known scenario here is when the draft invoice is linked to an incomplete invoice that has been deleted.
					newStatus = Constants.AccDraftInvoiceHeaderStatus.ApprovedForPosting;
				}
				else if (AIH_AH_PostedTransactionHeader.IsEmpty && !value.IsEmpty)
				{
					newStatus = Constants.AccDraftInvoiceHeaderStatus.Processed;
				}

				base.AIH_AH_PostedTransactionHeader = value;
				if (newStatus != AIH_Status)
				{
					AIH_Status = newStatus;
				}
			}
		}

		public override ZString AIH_Status
		{
			get { return base.AIH_Status; }
			set
			{
				if (AIH_Status != value)
				{
					var lifeCycleHandler = ObjectFactory.Get<IAccDraftInvoiceStatusChangeValidator>();
					var result = lifeCycleHandler.CanChangeStatusTo(this, value);
					if (result.CanUpdate)
					{
						var oldStatus = base.AIH_Status;
						base.AIH_Status = value;
						StatusUpdateSuccessful?.Invoke((this, oldStatus));
					}
					else
					{
						StatusUpdateFailed?.Invoke((this, result.ValidationErrors));
					}
				}
			}
		}

		public event Action<(AccDraftInvoiceHeader draftInvoice, string[] validationErrors)> StatusUpdateFailed;
		public event Action<(AccDraftInvoiceHeader draftInvoice, string oldStatus)> StatusUpdateSuccessful;

		public bool HasReconciliationRun { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "To reuse code between CW and Glow")]
		public ZString PostingStatus
		{
			get
			{
				if (postingStatus == null)
				{
					using (var cmd = Db.Connection.Command("SELECT AIHP_PostingStatus FROM GetDraftInvoicePostingStatus(@DraftInvoiceHeaderPK)"))
					{
						cmd.AddParameter("@DraftInvoiceHeaderPK", SqlDbType.UniqueIdentifier, PK.ToGuid());

						var result = cmd.ExecuteScalar();
						if (result != DBNull.Value)
						{
							postingStatus = (string)result;
						}
					}
				}

				return postingStatus;
			}
		}
		string postingStatus;

		public ZDateTime SystemCreateTimeLocal => AIH_SystemCreateTimeUtc.ToLocalBranchTime();

		public ZDateTime SystemLastEditTimeLocal => AIH_SystemLastEditTimeUtc.ToLocalBranchTime();

		public ZString OpenInPortal
		{
			get
			{
				return Res.GetString("APDraftInvoicePrintingUserControl|7F76F695-B09E-4AF4-B1E9-83808403CCE0", "Open in Portal");
			}
		}

		public ZPropertyInfo OpenInPortalInfo => GetZPropertyInfo(nameof(OpenInPortal));

		#region Draft Invocie Exchange Rate

		public AccDraftInvoiceExRateCollection ExchangeRates
		{
			get
			{
				if (exchangeRates == null)
				{
					exchangeRates = new AccDraftInvoiceExRateCollection(this, Factory);
					exchangeRates.Load();
				}

				return exchangeRates;
			}
		}
		AccDraftInvoiceExRateCollection exchangeRates;

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.AccountingDraftInvoiceHeader);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

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

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (tasks == null)
				{
					tasks = this.GetOrCreateProcessTaskCollection(() => new AccDraftInvoiceHeaderProcessTaskCollection(this));
					RegisterEditableChildObject(tasks);
				}
				return tasks;
			}
		}
		ProcessTaskCollection tasks;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZGuid IWorkflowProviderCore.PK => PK;
		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.AccDraftInvoiceCode;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, AIH_OH_Creditor, ZGuid.Empty);
			return result;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);

			if (IsInDatabase && AIH_StatusInfo.HasChanges)
			{
				Logs.AddNew(AutoEvents.StatusUpdated, $"Status Updated | FROM: {AIH_StatusInfo.OriginalValue} | TO: {AIH_Status}");
			}
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			ExchangeRates.RemoveAndDeleteAll();

			base.Delete();
		}

		#endregion

		#region Properties

		public override ZGuid AIH_GB_Branch
		{
			get { return base.AIH_GB_Branch; }
			set
			{
				base.AIH_GB_Branch = value;
				if (Branch != null)
				{
					AIH_GC_Company = Branch.GB_GC;
				}

				if (!IsValidationSuspended && GlbBranchCombinationValidation.ShouldValidateCombination(Branch))
				{
					Validation.ValidateAIH_GE_Department();
				}
			}
		}

		public int LocalCurrencyDecimals => Company.GetLocalDecimals();

		public int OSCurrencyDecimals => TransactionCurrency != null ? TransactionCurrency.Decimals : LocalCurrencyDecimals;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AIH_ExpectedOSTotalAmount
		{
			get => base.AIH_ExpectedOSTotalAmount;
			set => base.AIH_ExpectedOSTotalAmount = value;
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AIH_ExpectedOSTaxAmount
		{
			get => base.AIH_ExpectedOSTaxAmount;
			set => base.AIH_ExpectedOSTaxAmount = value;
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AIH_ExpectedOSExTaxAmount
		{
			get => base.AIH_ExpectedOSExTaxAmount;
			set => base.AIH_ExpectedOSExTaxAmount = value;
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AIH_GB_Branch = GlbBranch.CurrentBranch.PK;
			AIH_GE_Department = GlbDepartment.CurrentDepartment.PK;
			AIH_RX_NKTransactionCurrency = Company?.GC_RX_NKLocalCurrency ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData
		{
			get
			{
				// return a data structure which represents the company code and job regex.
				//e.g. {"CompanyCode":"DNZ","JobRegexList":["<shipment regex here>","<consol regex here>","<declaration regex here>"]}
				var jobNumberHelper = ObjectFactory.Get<IJobNumberHelper>();
				return JsonConvert.SerializeObject(new UtilityDataInfo()
				{
					CompanyCode = this.Company.GC_Code,
					JobRegexList = jobNumberHelper.GetJobNumberCustomisationRegistries()
						.Select(x => {
							var fallbackFromCompanyLevel = x.Registry.GetFallBackValueAtAllLevels(AIH_GC_Company.ToGuid(), Guid.Empty, Guid.Empty);
							return jobNumberHelper.GetJobNumberRegEx(x.Prefix, fallbackFromCompanyLevel);
						})
						.Append(jobNumberHelper.GetGenericJobTypeRegEx())
						.ToArray()
				});
			}
		}

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName) => AIH_Status != Core.Constants.AccDraftInvoiceHeaderStatus.Analyzing;

		class UtilityDataInfo
		{
			public string CompanyCode { get; set; }
			public IReadOnlyCollection<string> JobRegexList { get; set; }
		}

		#endregion

		#region eConversation

		JobConversation Conversation
		{
			get
			{
				if (!IsInDatabase)
				{
					return null;
				}
				return conversation ?? (conversation = GetOrCreateConversation());
			}
		}
		JobConversation conversation;

		JobConversation GetOrCreateConversation()
		{
			var result = JobConversation.GetOrCreate(this);
			RegisterEditableChildObject(result);
			return result;
		}

		#endregion

		#region IConversationProvider Members

		void IConversationProvider.RunConversationUpdateActionBeforeSaving() { }

		JobConversation IConversationProvider.eConversation => Conversation;

		ModuleIdentifier IConversationProvider.ParentModule => null;

		ControllerID IConversationProvider.ParentController => AIH_TransactionType == TransactionTypes.Invoice
			? ControllerIDs.APInvoiceFromDraftInvoice
			: ControllerIDs.APCreditNoteFromDraftInvoice;

		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants =>
			Enumerable.Empty<EConversation.Business.RelatedParty>();

		bool IConversationProvider.SendEmailNotificationsOnSave => true;

		string IConversationProvider.EmailSubjectContentOverride => default;

		string IConversationProvider.FromAddressOverride => default;

		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		#endregion

		#region IConversationParentHyperlinkProvider Members

		bool IConversationParentHyperlinkProvider.ShouldUseThisProviderForHyperlink(IConversationParticipant participant) => true;

		string IConversationParentHyperlinkProvider.GetHyperlinkToConversationParent()
		{
			var glowUri = GlowRegistry.Instance.GlowPortalsUri.Value;

			if (!glowUri.EndsWith("/", StringComparison.InvariantCulture))
			{
				glowUri += "/";
			}

			return $"{glowUri}PAY/Desktop#/formFlow/ddecb92840d649b59bd10c9a2303f3a2/{PK}";
		}

		#endregion

		#region IConversationEmailBehaviorProvider Members

		bool IConversationEmailBehaviorProvider.ShouldExcludeSender => true;

		bool IConversationEmailBehaviorProvider.ShouldSendEmailFromSender => true;

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var humanReadableNameBuilder = new ZStringBuilder();
				humanReadableNameBuilder.Append(AIH_TransactionType == TransactionTypes.Invoice ? Res.GetString("055bfc87-6834-47c5-8902-0d16e6d82df7", "Draft Invoice") : Res.GetString("dbbfa497-650e-460d-950e-a2bc4f02564f", "Draft Credit Note"));
				humanReadableNameBuilder.Append(" ");
				humanReadableNameBuilder.Append(string.IsNullOrEmpty(AIH_TransactionNumber) ? AIH_InternalReference : AIH_TransactionNumber);

				return humanReadableNameBuilder.ToString();
			}
		}

		protected override void ReloadCore()
		{
			postingStatus = null;
			base.ReloadCore();
		}

		#region ISupportAccProcessLogging

		ZGuid ISupportAccProcessLogging.ParentId => PK;

		bool ISupportAccProcessLogging.ShouldLog => true;

		string ISupportAccProcessLogging.ParentTableCode => AccDraftInvoiceHeaderSchema.Constants.Prefix;

		IAccProcessLog[] ISupportAccProcessLogging.Logs => throw new NotImplementedException(); //Will be implemented ina future workitem.

		IAccProcessLogger ISupportAccProcessLogging.Logger => logger ?? (logger = new AccDraftInvoiceProcessingErrorLogger(Factory));
		IAccProcessLogger logger;

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AIH_TransactionType = "INV";
		}
#endif
	}
}
