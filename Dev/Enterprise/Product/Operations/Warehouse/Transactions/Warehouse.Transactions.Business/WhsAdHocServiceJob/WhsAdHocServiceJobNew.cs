using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	[CodeProperty(AutoWhsAdHocServiceJob.Schema.WSJ_JobNumber), DescriptionProperty(Schema.WSJ_CustomerReference)]
	public class WhsAdHocServiceJob : AutoWhsAdHocServiceJob, IWhsAdHocServiceJob, ICanDelete, IRatingSupporter, IDocManagerSupport, IEDocsProvider, IHaveServices, IJobHeaderParent, IJobInvoicingPlugIn, INumberFountainConsumer
	{
		public new abstract class Schema : AutoWhsAdHocServiceJob.Schema
		{
			public const string BillingDate = "BillingDate";
			public const string IsFinalised = "IsFinalised";
			public const string Client = "Client";
			public const string Warehouse = "Warehouse";
		}

		#region Constructor

		public WhsAdHocServiceJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			BillingDate = ZDateTime.Today;
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return WSJ_JobNumber.IsEmpty ? HumanReadableNameWithoutID : ZString.Format("{0} {1}", HumanReadableNameWithoutID, WSJ_JobNumber); }
		}

		ZString HumanReadableNameWithoutID => Res.GetString("b0ff3035-e5aa-4a35-84b6-74c2dc950d2c", "Warehouse Ad Hoc Service Job");

		#endregion

		#region Related Entities

		#region Warehouse

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WSJ_WW_Whs);

		#endregion

		#endregion

		#region Properties

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region ClientPK

		[RelatedBusinessObject("Client")]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Lookups.Clients")]
		[ResourceStringData("WhsAdHocServiceJob|Client", Caption = "Client")]
		public override ZGuid WSJ_OH_Client
		{
			get => base.WSJ_OH_Client;
			set
			{
				base.WSJ_OH_Client = value;
				SetJobHeaderDefault(jobHeader);
			}
		}

		#endregion

		#region WSJ_WW_Whs

		[RelatedBusinessObject("Warehouse")]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Lookups.Warehouses")]
		[ResourceStringData("WhsAdHocServiceJob|Warehouse", Caption = "Warehouse")]
		public override ZGuid WSJ_WW_Whs
		{
			get => base.WSJ_WW_Whs;
			set => base.WSJ_WW_Whs = value;
		}

		#endregion

		#region BillingDate

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public ZDateTime BillingDate
		{
			get
			{
				return WSJ_BillingDate;
			}
			set
			{
				WSJ_BillingDate = (ZDate)value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateBillingDate();
				}
				BillingDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo BillingDateInfo
		{
			get { return GetZPropertyInfo(Schema.BillingDate); }
		}

		#endregion

		#region WSJ_JobNumber

		[ReadOnly(true)]
		[ResourceStringData("AdHocJobNumber", Caption = "Ad Hoc Job Number", ShortCaption = "Ad Hoc Job #")]
		public override ZString WSJ_JobNumber
		{
			get => base.WSJ_JobNumber;
			set => base.WSJ_JobNumber = value;
		}

		#endregion

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			// tested in AdHocServiceJobFilterControl.cs
			return new WhsAdHocServiceJobFetchStrategy(this);
		}

		#endregion

		#region Status

		WhsAdHocServiceJobStatuses Statuses => new WhsAdHocServiceJobStatuses();

		public ZString Status
		{
			get
			{
				var status = Statuses.GetDescriptionFromCode("NEW");
				if (IsFinalised)
				{
					status = Statuses.GetDescriptionFromCode("FIN");
				}
				else if (IsInDatabase)
				{
					status = Statuses.GetDescriptionFromCode("ENT");
				}
				return status;
			}
		}

		#endregion

		#region IsFinalising

		public bool IsFinalising => FinaliseSemaphore.IsSuspended;

		Semaphore FinaliseSemaphore => finaliseSemaphore ??= new Semaphore();
		Semaphore finaliseSemaphore;

		#endregion

		#region IsFinalised

		public bool IsFinalised => WSJ_IsFinalised;

		#endregion

		#region ReadOnly

		protected bool StandardReadOnly => ReadOnly || IsFinalised;

		#endregion

		#region Save

		#region OnFactorySaving

		public override void OnSaving()
		{
			base.OnSaving();

			if (WSJ_CustomerReference.IsEmpty)
			{
				WSJ_CustomerReference = WSJ_JobNumber;
			}
		}

		#endregion

		#region OnFactorySaved

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			ClearJobNoAndCustomerReferenceIfSaveFailed(saveSucceeded);
		}

		void ClearJobNoAndCustomerReferenceIfSaveFailed(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				if (WSJ_CustomerReference.EqualsIgnoringCase(WSJ_JobNumber))
				{
					WSJ_CustomerReference = "";
				}

				WSJ_JobNumber = "";
			}
		}

		#endregion

		#endregion

		#region SetJobNumberIfRequired

		void SetJobNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(WSJ_JobNumberInfo, JobNumberFountain);
			}
		}

		#endregion

		#region CreateJobHeader

		JobHeader CreateJobHeader()
		{
			var jobHeader = new JobHeader.Loader(this).TryLoadOrCreateWithMutex();

			if (jobHeader != null)
			{
				RegisterEditableChildObject(jobHeader);
			}

			SetJobHeaderDefault(jobHeader);

			return jobHeader;
		}

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new WhsAdHocServiceJobFountainUniqueIndexFailureHandler(this)); }
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		class WhsAdHocServiceJobFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public WhsAdHocServiceJobFountainUniqueIndexFailureHandler(WhsAdHocServiceJob job)
				: base(WhsAdHocServiceJobSchema.Constants.Indexes.NR_UC__WSJ_JobNumber, job)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix => Env.NumberFountains.WorkItemNo;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			var job = JobHeader;
			if (job != null)
			{
				job.Dispose();
				job.Delete();
			}

			base.Delete();
		}

		#endregion

		#region Finalise

		public bool FinaliseAdHocServiceJob(INotifications notifications, bool confirmFinalise = true)
		{
			var result = false;
			if (HasChanges || !IsInDatabase)
			{
				notifications.AddError(Res.GetString("1f6e3468-99f1-487d-ab89-c5b2e50b4222", "Save all changes before Finalizing this Ad Hoc Service Job."));
			}
			else
			{
				result = FinaliseAdHocServiceJobCore(notifications.AddError, () => !confirmFinalise || ConfirmFinalise());
			}
			return result;

			bool ConfirmFinalise()
			{
				var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: new ZGuid("526214f3-729b-437b-87ee-785451d787f1"),
					caption: Res.GetString("f54cbc42-e587-41e1-8e40-01a4edac9502", "Finalize Ad Hoc Service Job"),
					buttons: ZMessageBoxButtons.YesNo,
					icon: ZMessageBoxIcon.Warning,
					context: null,
					resultsNotToSave: new[] { ZDialogResult.No },
					showCheckboxOnly: true);

				var message = Res.GetString("7ee3403e-23fe-4ce9-9cc7-b827083a7c4e",
@"On finalization, this Ad Hoc Service Job will become read-only so that it cannot be modified.
This finalization process cannot be undone once saved.

Do you wish to finalize this Ad Hoc Service Job?");
				var defaultableQueryUserEventArgs = new DefaultableQueryUserEventArgs(dialogDefaultContext, message, false);
				notifications.QueryUser(defaultableQueryUserEventArgs);
				return defaultableQueryUserEventArgs.Response;
			}
		}

		public bool FinaliseAdHocServiceJobWithNoNotifications(Action<string> onError)
		{
			return FinaliseAdHocServiceJobCore(onError, () => true);
		}

		bool FinaliseAdHocServiceJobCore(Action<string> onError, Func<bool> getConfirm)
		{
			var result = false;
			if (IsFinalised)
			{
				onError(Res.GetString("552b0b31-c0f4-4ee8-83eb-76cc8f443c26", "Ad Hoc Service Job already Finalized."));
			}
			else
			{
				using (new SemaphoreManager(FinaliseSemaphore))
				{
					Services.Cast<WhsJobService>().ForEach(s => s.Validation.ValidateES_CompletedDateTimeOffset());
					if (Services.Cast<WhsJobService>().Any(s => !s.ES_CompletedDateTimeOffset.IsValid))
					{
						onError(Res.GetString("b2fa83ab-aa0e-4ac4-899a-438986b9095b", "Cannot Finalize Ad Hoc Service Job until all Services have been Completed."));
					}
					else if (getConfirm())
					{
						WSJ_IsFinalised = true;
						Logs.AddNew(Events.ItemDocumentJobFinalised, "Ad Hoc Service Job");
						SetReadOnlyIncludingChildren(true);
						result = true;
					}
				}
			}

			return result;
		}

		#endregion

		#region IJobInvoicingPlugIn

		protected IJobInvoicingSupporter GetInvoicingSupporterCore()
		{
			return new WhsAdHocServiceJobInvoicingSupporter(this);
		}

		#endregion

		#region IJobParent

		protected bool AllowInvoiceDeletionCore
		{
			get { return false; }
		}

		#endregion

		#region IHaveServices Members

		public ZString TableCode
		{
			get => WhsAdHocServiceJobSchema.Constants.Prefix;
		}

		public ZString TransportMode
		{
			get => ZString.Empty;
		}

		public ZString ContainerMode
		{
			get => ZString.Empty;
		}

		public IHaveServices[] DependentServiceParents => Array.Empty<IHaveServices>();

		public BusinessObject ServiceParent
		{
			get => this;
		}

		public bool NeedsServiceEvents
		{
			get => false;
		}

		[ChildEditable]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = GetJobServicesDependentCollection(this, Factory);
					services.Load();
					RegisterEditableChildObject(services);
					AfterServiceLoaded();
				}
				return services;
			}
		}
		WhsJobServiceDependentCollection services;

		protected WhsJobServiceDependentCollection GetJobServicesDependentCollection(WhsAdHocServiceJob adhocServiceJob, BusinessObjectFactory factory)
		{
			return new WhsJobServiceDependentCollection(adhocServiceJob, factory);
		}

		protected void AfterServiceLoaded()
		{
			Services.SetReadOnlyIncludingChildren(StandardReadOnly);
		}

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		IBranch IHaveServices.ServiceBranch => Warehouse?.RelatedCompanyBranch;

		#endregion

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => WSJ_JobNumber;
			set
			{
				WSJ_JobNumber = value;
				if (WSJ_CustomerReference.IsEmpty)
				{
					WSJ_CustomerReference = WSJ_JobNumber;
				}
			}
		}
		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.WorkItemNo;

		#endregion

		#region JobNumberFountain

		protected virtual INumberFountainProxy JobNumberFountain
		{
			get { return Env.NumberFountains.WorkItemNo; }
		}

		#endregion

		#region IJobHeaderParent Members

		public void SetJobNumberFieldOnSaving()
		{
			SetJobNumberIfRequired();
		}

		public void OnJobCreating(JobHeader job)
		{
		}

		public void OnJobCreated(JobHeader job)
		{
		}

		public void OnJobDeleting(JobHeader job)
		{
		}

		public void OnJobDeleted(JobHeader job)
		{
		}

		public bool RequiresJobHeaderMutex
		{
			get; set;
		}

		public bool AllowInvoiceDeletion => true;

		public string JobNumber => WSJ_JobNumber;

		#endregion

		#region IJobInvoicingPlugIn Member

		public IJobInvoicingSupporter InvoicingSupporter => invoicingSupporter ?? (invoicingSupporter = GetInvoicingSupporterCore());
		IJobInvoicingSupporter invoicingSupporter;

		#endregion

		#region JobHeader

		public JobHeader JobHeader
		{
			get
			{
				if (jobHeader == null || jobHeader.IsDeleted)
				{
					jobHeader = CreateJobHeader();
				}
				return jobHeader;
			}
		}

		JobHeader jobHeader;

		#endregion

		#region SetJobHeaderDefault

		void SetJobHeaderDefault(JobHeader jobHeader)
		{
			var client = Client;
			if (client != null && jobHeader != null && jobHeader.JH_OA_LocalChargesAddr.IsEmpty)
			{
				jobHeader.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			}
		}

		#endregion

		#region ICanDelete Member

		public override bool CanDelete => CanDeleteCore;

		bool CanDeleteCore => !IsFinalised;

		public override MultilingualString ReasonForNotAbleToDelete => CanDelete ? null : CantDeleteReasonMsg;

		public static MultilingualString CantDeleteReasonMsg
		{
			get { return ResString.GetMultilingualString("1e61a0d0-9bd8-4cdf-8531-3a72e364e325", "You cannot delete finalized ad hoc service jobs.", WhsAdHocServiceJobStatuses.Descriptions.Entered); }
		}

		#endregion

		#region IRatingSupporter Member

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider => new WhsAdHocServiceJobRatingAdaptersProvider(this);

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new WhsAdHocServiceJobDocManagerInfo(this));
		DocManagerInfo docManagerInfo;

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new WhsAdHocServiceJobDocumentSupporter(this));
		DocumentSupporter documentSupporter;

		public EDocsProviderSupporter GetEDocsProviderSupporter() => new JobInvoicingEDocsProviderSupporter(this);

		#endregion

		#region Notes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var notes = base.NoteTypesCore;
				notes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
				notes.Add(PredefinedNoteTypes.Instance.ClientVisibleJobNotes);
				notes.Add(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation);
				notes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
				notes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);

				return notes;
			}
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				var result = base.NoteContextsForRelatedNotes;
				result.Module |= StmNoteContextModule.W;
				return result;
			}
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);

				var client = Client;
				if (client != null)
				{
					result.Add(client);
				}

				var warehouse = Warehouse;
				if (warehouse != null)
				{
					result.Add(warehouse);
				}
				return result.ToArray();
			}
		}

		#endregion

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			WSJ_JobNumber = ZString.Empty;
		}

		internal StmNoteContexts GetNoteContextsForRelatedNotes()
		{
			return NoteContextsForRelatedNotes;
		}

#endif
		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			Factory.SuspendValidation();
			try
			{
				_ = JobHeader;
			}
			finally
			{
				Factory.ResumeValidation();
			}

			base.RunPreSaveValidationCore();
		}

		#endregion
	}

	#region Document Support

	public class WhsAdHocServiceJobDocumentSupporter : DocumentSupporter
	{
		public WhsAdHocServiceJobDocumentSupporter(WhsAdHocServiceJob job)
			: base(job)
		{
		}

		protected WhsAdHocServiceJob AdHocServiceJob => (WhsAdHocServiceJob)BusinessObject;

		public override BusinessContext BusinessContext => BusinessContext.WhsAdHocServiceJob;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsAdHocServiceJobCustomizeDocuments;

		/// <summary>
		/// GetDocumentWrappersInternal for WhsAdHocServiceJob is not implemented. It was not needed only IEdocs was required. Don't just call this blindly.
		/// </summary>
		/// <param name="dataContext"></param>
		/// <param name="commandBeingRun"></param>
		/// <returns></returns>
		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, AdHocServiceJob);

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			//Note: this has not been implemented as it is not needed at this time.
			return null;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Constants.DataContext.WhsAdHocServiceJob
			};
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(AdHocServiceJob.Client, null);
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => false;
	}

	#endregion
}
