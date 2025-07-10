using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business.CusReconBase;
using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[CodeProperty(AutoCusReconDeclaration.Schema.CRD_JobReferenceNumber), DescriptionProperty(AutoCusReconDeclaration.Schema.CRD_JobReferenceNumber)]
	[SingleObjectAroundARow]
	public class ConsolidatedDeclaration : CusReconBase.CusReconDeclaration, Integration.Customs.IConsolidatedDeclaration, IDocManagerSupport, IDocumentSupportable, IWorkflowProvider
	{
		public ConsolidatedDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusReconDeclaration.Schema
		{
			public const string BranchCode = nameof(ConsolidatedDeclaration.BranchCode);
			public const string BranchName = nameof(ConsolidatedDeclaration.BranchName);
		}

		public new static readonly ConsolidatedDeclarationTypeDecider TypeDecider = new ConsolidatedDeclarationTypeDecider();

		public static class ApplicationCodes
		{
			public const string TSW = nameof(TSW);
			public const string CMR = nameof(CMR);
			public static bool IsConsolidatedDeclarationApplicationCode(string code) => code == TSW || code == CMR;
		}

		public virtual FilterBusinessObjectDefaults DefaultAttachDeclarationFilter => null;

		public static ConsolidatedDeclaration GetConsolidatedDeclaration(BaseJobDeclaration declaration)
		{
			return GetOriginalCachedConsolidatedDeclarationCore(declaration) ?? declaration.Factory.Load<ConsolidatedDeclaration>(QueryConsolidatedDeclarationPKCore(declaration));
		}

		public static bool IsConsolidated(BaseJobDeclaration declaration)
		{
			return declaration.JE_EntryStatus == ConsolidatedEntryStatusList.Codes.AppliedToConsolidation || GetOriginalCachedConsolidatedDeclarationCore(declaration) != null || QueryConsolidatedDeclarationPKCore(declaration).IsValid;
		}

		public bool HasConsolidatedEntryChanges => OutstandingAmendmentLogManager.HasConsolidatedEntryChanges;

		public OutstandingAmendmentLogManager OutstandingAmendmentLogManager => outstandingAmendmentLogManager ??= new OutstandingAmendmentLogManager(this);
		OutstandingAmendmentLogManager outstandingAmendmentLogManager;

		[ReadOnly(true)]
		public override ZString CRD_ApplicationCode
		{
			get => base.CRD_ApplicationCode;
			set => base.CRD_ApplicationCode = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("4B238F05-24EC-428F-A3EE-AA6880181666", Caption = "Job Number")]
		public override ZString CRD_JobReferenceNumber
		{
			get => base.CRD_JobReferenceNumber;
			set => base.CRD_JobReferenceNumber = value;
		}

		[ResourceStringData("C5EF83E9-9712-46D0-BD13-96EAF71ED519", Caption = "Entry Period Date")]
		public override ZDate CRD_PeriodTo
		{
			get => base.CRD_PeriodTo;
			set => base.CRD_PeriodTo = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("57B8FCB3-6949-4FED-9E5C-D834947C7545", Caption = "Message Status", MediumCaption = "Msg. Status", ShortCaption = "Msg. Stat.")]
		public override ZString CRD_MessageStatus
		{
			get => base.CRD_MessageStatus;
			set => base.CRD_MessageStatus = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("3C610FE6-080D-4AD1-8527-84F3AA2E67D1", Caption = "Customs Status")]
		public override ZString CRD_CustomsStatus
		{
			get => base.CRD_CustomsStatus;
			set => base.CRD_CustomsStatus = value;
		}

		[ResourceStringData("664706D4-9F9C-4B89-AE39-5757D0F81D62", Caption = "Branch")]
		public virtual ZString BranchCode => Branch?.GB_Code ?? ZString.Empty;

		[ResourceStringData("27D9D11C-CAF2-417C-AABD-C755F2131FAA", Caption = "Branch Name")]
		public virtual ZString BranchName => Branch?.GB_BranchName ?? ZString.Empty;

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		public BaseJobDeclaration LeadDeclaration
		{
			get
			{
				if (leadDeclaration == null || (leadDeclaration.PK != CRD_JE_LeadDeclaration))
				{
					leadDeclaration = Factory.Load<BaseJobDeclaration>(CRD_JE_LeadDeclaration);
				}
				return leadDeclaration;
			}
		}
		BaseJobDeclaration leadDeclaration;

		public IConsolidatedJobDeclarationCollection<BaseJobDeclaration> JobDeclarations
		{
			get
			{
				if (jobDeclarations == null)
				{
					jobDeclarations = CreateNewJobDeclarationCollection();
					jobDeclarations.Load();
					foreach (var dec in jobDeclarations)
					{
						SetOriginalCachedConsolidatedDeclarationCore(dec, this);
					}
					jobDeclarations.CountChanged += (o, e) =>
					{
						if (!jobDeclarations.Contains(leadDeclaration))
						{
							leadDeclaration = null;
							CRD_JE_LeadDeclaration = jobDeclarations.FirstOrDefault()?.PK ?? ZGuid.Empty;
						}
					};
				}
				return jobDeclarations;
			}
		}
		IConsolidatedJobDeclarationCollection<BaseJobDeclaration> jobDeclarations;
		protected virtual IConsolidatedJobDeclarationCollection<BaseJobDeclaration> CreateNewJobDeclarationCollection() => new ConsolidatedJobDeclarationCollection<BaseJobDeclaration>(this);

		///
		/// <summary>
		/// Build a virtual aggregate declaration for messaging sending. It contains all data to be sent to customs from all consolidated declarations.
		/// </summary>
		/// 
		/// <remarks>
		/// Data to be aggregated are:
		/// -- Entry lines
		/// -- Packages
		/// -- Containers
		/// -- Messages
		/// -- Job number (using that from the consolidated entry)
		/// -- Numeric values, such as weight, amount etc., which will be added up
		/// -- Representative fields from the lead declaration, such as Entry Number
		///
		/// Data to be aggregated by child class (overriding OnAggregateDeclarationBuilt):
		/// -- Usually identical values across declarations that may need resolution if they are not
		/// -- Properties defined at child level
		/// </remarks>
		///
		public BaseJobDeclaration BuildAggregateJobDeclaration()
		{
			if (LeadDeclaration is null)
			{
				return null;
			}

			// create a new readonly factory so that the virtual aggregate declaration will never be saved unintentionally
			var factoryReadOnly = new ReadOnlyBusinessObjectFactory { ReportErrorOnSaveAttempt = false, RefreshEnabled = false };
			foreach (var jobDeclaration in JobDeclarations)
			{
				factoryReadOnly.ImportFromAnotherFactory(jobDeclaration);
				factoryReadOnly.AddFetchHint(CusEntryHeaderSchema.CH_ClusterKey, jobDeclaration.JE_ClusterKey);
				factoryReadOnly.AddFetchHint(CusEntryLineSchema.CL_ClusterKey, jobDeclaration.JE_ClusterKey);
				factoryReadOnly.AddFetchHint(JobComInvoiceHeaderSchema.JZ_ClusterKey, jobDeclaration.JE_ClusterKey);
				factoryReadOnly.AddFetchHint(JobComInvoiceLineSchema.JI_ClusterKey, jobDeclaration.JE_ClusterKey);
			}

			var aggregateDeclaration = (BaseJobDeclaration)factoryReadOnly.New(LeadDeclaration.GetType());
			(aggregateDeclaration as IBusinessObjectInternals).IsCopying = true;

			// these suspensions are required for the life of the aggregated declaration.  They will 'clean up' when deleted.
			aggregateDeclaration.MergeManager.SuspendRequiresMergeCalculation();
			aggregateDeclaration.SuspendMarkApportionmentDirty();
			aggregateDeclaration.SuspendMarkingAsNeedingValidation();
			aggregateDeclaration.SuspendValidation();

			var declarationToAggreateAsLead = (BaseJobDeclaration)factoryReadOnly.Load(LeadDeclaration.GetType(), LeadDeclaration.PK);
			if (LeadDeclaration.HasMessageInitiator)
			{
				aggregateDeclaration.MessageInitiator = LeadDeclaration.MessageInitiator;
			}
			aggregateDeclaration.CopyPersistentValuesFrom(declarationToAggreateAsLead);
			aggregateDeclaration.JE_DeclarationReference = CRD_JobReferenceNumber;

			aggregateDeclaration.ActiveEntryHeaders.RemoveAll();
			var aggregateEntryHeader = (CusEntryHeader)factoryReadOnly.New(declarationToAggreateAsLead.ActiveEntryHeaders[0].GetType());

			aggregateEntryHeader.SuspendMarkingAsNeedingValidation();
			aggregateEntryHeader.SuspendValidation();

			aggregateEntryHeader.CopyPersistentValuesFrom(declarationToAggreateAsLead.ActiveEntryHeaders[0]);
			aggregateDeclaration.ActiveEntryHeaders.Add(aggregateEntryHeader);
			if (LeadDeclaration.DeclarationNumber.IsValid)
			{
				aggregateDeclaration.DeclarationNumber = LeadDeclaration.DeclarationNumber;
			}
			// copy system columns which were excluded from CopyPersistentValuesFrom, so that we can tell the matching entry header when importing
			aggregateEntryHeader.CH_ClusterKey = declarationToAggreateAsLead.ActiveEntryHeaders[0].CH_ClusterKey;
			aggregateEntryHeader.CH_SystemCreateTimeUtc = declarationToAggreateAsLead.ActiveEntryHeaders[0].CH_SystemCreateTimeUtc;

			aggregateDeclaration.DocAddresses.RemoveAndDeleteAll();
			MoveAllBizO(declarationToAggreateAsLead.DocAddresses, aggregateDeclaration.DocAddresses);

			foreach (var message in Messages.ToArray())
			{
				aggregateEntryHeader.Messages.Add(factoryReadOnly.ImportFromAnotherFactorySafe(message as EDIMessage));
			}

			// load declarations into readonly factory and aggregate. value/FK changes must not persist
			foreach (var jobDeclaration in JobDeclarations.OrderBy(dec => dec.JE_DeclarationReference).Select(dec => (BaseJobDeclaration)factoryReadOnly.Load(LeadDeclaration.GetType(), dec.PK)))
			{
				MergeDeclarationIntoAggregateDeclaration(jobDeclaration, aggregateDeclaration, aggregateEntryHeader);
			}

			(aggregateDeclaration as IBusinessObjectInternals).IsCopying = true;
			OnAggregateDeclarationBuilt(aggregateDeclaration);

			foreach (var line in aggregateDeclaration.InvoiceLines)
			{
				((ICommonInvoice)line).AllCharges.Load();
			}
			foreach (BasePackage package in aggregateDeclaration.Packages)
			{
				package.Declaration = aggregateDeclaration;
			}
			aggregateDeclaration.Bills.Cast<Bill>().ForEach(bill => bill.RefreshChildBills());

			SetOriginalCachedConsolidatedDeclarationCore(aggregateDeclaration, this);

			baselineChangeSetBeforeAggregation = factoryReadOnly.GetChanges();

			(aggregateDeclaration as IBusinessObjectInternals).IsCopying = true;

			return aggregateDeclaration;
		}

		protected virtual void MergeDeclarationIntoAggregateDeclaration(BaseJobDeclaration jobDeclaration, BaseJobDeclaration aggregateDeclaration, CusEntryHeader aggregateEntryHeader)
		{
			if (jobDeclaration.PK != LeadDeclaration.PK)
			{
				aggregateDeclaration.GrossWeight += jobDeclaration.GrossWeight;
				aggregateDeclaration.Volume += jobDeclaration.Volume;
				aggregateEntryHeader.CH_TotalPaid += jobDeclaration.ActiveEntryHeaders[0].CH_TotalPaid;
			}
			MoveAllBizO(jobDeclaration.InvoiceLines, aggregateDeclaration.InvoiceLines);
			MoveAllBizO(jobDeclaration.Invoices, aggregateDeclaration.Invoices);

			// setting JZ_JE triggers caching of properties, They need refreshing afterwards.
			aggregateEntryHeader.ResetInvoiceHeadersAndLines();

			MoveAllBizO(jobDeclaration.ActiveEntryHeaders[0].MergedLines, aggregateEntryHeader.MergedLines);
			MoveAllBizO(jobDeclaration.ActiveEntryHeaders[0].AllEntryLines, aggregateEntryHeader.AllEntryLines);
			aggregateDeclaration.Packages.AddRange(jobDeclaration.Packages.ToArray());
			aggregateEntryHeader.Packages.AddRange(jobDeclaration.ActiveEntryHeaders[0].Packages.ToArray());
			aggregateDeclaration.PackingGroups.AddRange(jobDeclaration.PackingGroups.ToArray());
			MoveAllBizO(jobDeclaration.CusContainers, aggregateDeclaration.CusContainers);
			MoveAllBizO(jobDeclaration.Bills, aggregateDeclaration.Bills);
		}

		protected void MoveAllBizO(IBusinessObjectCollection col1, IBusinessObjectCollection col2)
		{
			foreach (var bizo in col1.ToArray())
			{
				try
				{
					((IBusinessObjectInternals)bizo).IsCopying = true;
					bizo.SuspendValidation();

					col1.RemoveFromRelationship(bizo);
					col2.Add(bizo);

					if (bizo is IClusterKeyEntity clusterEntity)
					{
						clusterEntity.ClusterKeyPty.Value = (ZInt)clusterEntity.ClusterKeyPty.OriginalValue;
					}
				}
				finally
				{
					((IBusinessObjectInternals)bizo).IsCopying = false;
				}
			}
		}

		IFactoryChangeSet baselineChangeSetBeforeAggregation;

		/// <summary>
		/// Called after aggregate declaration is built. Used to customise the resulting BizO, such as merging inconsistent columns, or adding certain addon data.
		/// </summary>
		/// <param name="aggregateDeclaration">the resulting aggregate declaration BizO</param>
		protected virtual void OnAggregateDeclarationBuilt(BaseJobDeclaration aggregateDeclaration) { }
		/// <summary>
		/// Called before aggregate declaration is imported. Used to recover columns changed during building, so that the changes will not pollute the importing factory.
		/// Do not change the calling ConsolidatedDeclaration BizO and its factory. Please only prepare aggregate declaraton for copying back.
		/// </summary>
		/// <param name="aggregateDeclaration">the built aggregate declaration BizO to be imported after processing</param>
		protected virtual void OnAggregateDeclarationImporting(BaseJobDeclaration aggregateDeclaration) { }
		/// <summary>
		/// Called after aggregate declaration is imported. Used to copy any columns/tables or perform any actions child specific.
		/// </summary>
		/// <param name="aggregateDeclaration">the built aggregate declaration BizO to be imported after common importing</param>
		protected virtual void OnAggregateDeclarationImported(BaseJobDeclaration aggregateDeclaration) { }
		/// <summary>
		/// Called after consolidated declaration is created with customs declarations.
		/// </summary>
		public virtual void OnCreatedWithCustomsDeclarations() { }

		///
		/// <summary>
		/// Reverse of BuildAggregateJobDeclaration. Reads the aggregate declaration with changes for messaging purpose, copy back to another factory.
		/// </summary>
		/// <param name="aggregateDeclaration">
		/// The built aggregate declaration returned by BuildAggregateJobDeclaration. The changes made to it including children will be imported back to a savable factory.
		/// </param>
		/// <param name="factoryToImportInto">
		/// When null (default) a separate factory will be imported into and saved. Otherwise the factory passed in will contain the changes from aggregate declaration and the caller must save the factory as is appropriate.
		/// </param>
		public void ImportAggregateDeclaration(BaseJobDeclaration aggregateDeclaration, BusinessObjectFactory factoryToImportInto = null, bool importMessages = true)
		{
			var factoryForSaving = factoryToImportInto ?? new BusinessObjectFactory();
			var factoryForImporting = aggregateDeclaration.Factory;
			var consolidatedDeclarationToSave = (ConsolidatedDeclaration)factoryForSaving.Load(GetType(), PK);
			if (!consolidatedDeclarationToSave.JobDeclarations.Select(_ => _.PK).ToHashSet().SetEquals(JobDeclarations.Select(_ => _.PK)))
			{
				throw new ZConcurrencyCheckFailureException("Consolidated entry has been changed by other users. Please reopen it and submit again.", "Cannot save", false);
			}

			consolidatedDeclarationToSave.OnAggregateDeclarationImporting(aggregateDeclaration);

			//This code is only for developer to find all possible changed BO during development. NZ has done the work. We don't need this check for the project that has released to client. 
			if (!ReleasedCountries.Contains(consolidatedDeclarationToSave.CountryCode))
			{
				ReportUnexpectedChangedObjects(factoryForImporting);
			}

			// import lead declaration
			// first reset aggregate values before import, because they have been changed in building and are not to be captured
			var leadDeclaration = consolidatedDeclarationToSave.LeadDeclaration;
			leadDeclaration.CopyPersistentValuesFrom(aggregateDeclaration, new BusinessObjectCloneArgs(new[] { JobDeclarationSchema.JE_DeclarationReference.Name, JobDeclarationSchema.JE_TotalWeight.Name, JobDeclarationSchema.JE_TotalWeightUnit.Name, JobDeclarationSchema.JE_TotalVolume.Name, JobDeclarationSchema.JE_TotalVolumeUnit.Name }));

			// import entry header
			var headerToSave = leadDeclaration.ActiveEntryHeaders[0];
			// find the entry header created to capture changes, as there is a possibility an empty entry header was created by country messaging handling, which is discarded
			// the built entry header from this class should have system columns populated, whereas the empty one does not
			var headerToCopy = aggregateDeclaration.CustomsEntryHeaders.Single(_ => _.CH_ClusterKey == headerToSave.CH_ClusterKey && _.CH_SystemCreateTimeUtc == headerToSave.CH_SystemCreateTimeUtc);
			headerToSave.CopyPersistentValuesFrom(headerToCopy, new BusinessObjectCloneArgs(new[] { CusEntryHeaderSchema.CH_JE.Name, CusEntryHeaderSchema.CH_TotalPaid.Name, CusEntryHeaderSchema.CH_HighestLineNumber.Name }));

			// import messages
			if (importMessages)
			{
				ImportMessages(aggregateDeclaration, factoryToImportInto);
			}

			baselineChangeSetBeforeAggregation = null;

			consolidatedDeclarationToSave.OnAggregateDeclarationImported(aggregateDeclaration);

			consolidatedDeclarationToSave.SyncJobDeclarationsStatus();

			if (factoryForSaving != factoryToImportInto)
			{
				factoryForSaving.Save();
			}
		}

		void ReportUnexpectedChangedObjects(BusinessObjectFactory factoryForImporting)
		{
			// check if there are changed BizOs besides the ones aggregated. If there are, we need to account for them.
			var changeSetAfterMessageHandling = factoryForImporting.GetChanges();
			var changedObjects = changeSetAfterMessageHandling.GetChangedObjects().Select(changeSet => changeSet.SessionInstance).Except(baselineChangeSetBeforeAggregation.GetChangedObjects().Select(changeSet => changeSet.SessionInstance)).ToArray();
			var addedObjects = changeSetAfterMessageHandling.GetAddedObjects().Except(baselineChangeSetBeforeAggregation.GetAddedObjects());

			var permittedTypes = PermittedNewObjectTypes;
			var unexpectedObjects = addedObjects.Where(bo => bo.GetType() is Type boType && !permittedTypes.Any(t => t.IsAssignableFrom(boType))).ToArray();
			if (changedObjects.Any(bo => !(bo is ProcessTask)) || unexpectedObjects.Any())
			{
				throw new DeveloperNotificationException($@"Unexpected changed BizOs in message submission:
{string.Join(System.Environment.NewLine, changedObjects.Select(bo => $"Changed: {bo.HumanReadableName}").Concat(unexpectedObjects.Select(bo => $"Added: {bo.HumanReadableName}")))}.");
			}
		}

		protected virtual Type[] PermittedNewObjectTypes => new[] { typeof(EDIMessage), typeof(EDIMessageAttach), typeof(StmALog), typeof(GenAddOnColumn), typeof(StmEvent), typeof(ProcessQueue), typeof(MailItem), typeof(MailRecipient), typeof(MailAttachment) };

		public void ImportMessages(BaseJobDeclaration aggregateDeclaration, BusinessObjectFactory factoryToImportInto = null)
		{
			var factoryForSaving = factoryToImportInto ?? new BusinessObjectFactory();
			var factoryForImporting = aggregateDeclaration.Factory;
			var consolidatedDeclarationToSave = (ConsolidatedDeclaration)factoryForSaving.Load(GetType(), PK);
			var changeSetAfterMessageHandling = factoryForImporting.GetChanges();
			var addedObjects = changeSetAfterMessageHandling.GetAddedObjects().Except(baselineChangeSetBeforeAggregation.GetAddedObjects());

			foreach (var messageForImporting in aggregateDeclaration.CustomsEntryHeaders[0].Messages.Cast<EDIMessage>())
			{
				var messageForSaving = (EDIMessage)factoryForSaving.Load(messageForImporting.GetType(), messageForImporting.PK);
				if (messageForSaving != null)
				{
					messageForSaving.CopyPersistentValuesFrom(messageForImporting, new BusinessObjectCloneArgs(new[] { EDIMessageSchema.EM_LinkTable.Name, EDIMessageSchema.EM_LinkUniqueID.Name }));
				}
				else
				{
					messageForSaving = (EDIMessage)factoryForSaving.ImportFromAnotherFactory(messageForImporting);
					messageForSaving.EM_LinkedObject = consolidatedDeclarationToSave;
				}
			}

			foreach (var messageAttach in addedObjects.Where(bo => bo is EDIMessageAttach))
			{
				factoryForSaving.ImportFromAnotherFactory(messageAttach).HasChanges = true;
			}

			if (factoryForSaving != factoryToImportInto)
			{
				factoryForSaving.Save();
			}
		}

		public override void Delete()
		{
			JobDeclarations.RemoveAll();
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateReferenceNumberIfNeeded();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public virtual void CalculateHeaderFees()
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("2D4B4A02-0DF5-4FFF-B905-C5FFDFDA7CC9", "Consolidated Declaration");
				if (!CRD_JobReferenceNumber.IsEmpty)
				{
					result += " - " + CRD_JobReferenceNumber;
				}
				return result;
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
		}

		protected override CusReconBase.CusReconDeclarationValidation GetNewValidation() => new ConsolidatedDeclarationValidation(this);

		protected internal void PopulateReferenceNumberIfNeeded()
		{
			if (!IsInDatabase)
			{
				CRD_JobReferenceNumber = GenerateConsolidatedDeclarationNumber(Factory);
			}
		}

		ZString GenerateConsolidatedDeclarationNumber(BusinessObjectFactory factory)
		{
			var generatorTarget = new ConsolidatedDeclarationNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = GetNewNumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.ConsolidatedDeclarationNumber,
				FountainGetter = Env.NumberFountains.GetConsolidatedDeclarationNumberFountain,
				PrimaryTarget = generatorTarget
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.ValueProviders.AddRange(new FreightValueSource(LeadDeclaration));

			generator.Generate();
			generator.EnforceMaxLengths();
			return generatorTarget.Value.ToUpper();
		}

		NumberGeneratorContext GetNewNumberGeneratorContext()
		{
			return GlbBranch.CurrentBranch.PK.IsEmpty ? new NumberGeneratorContext() : new NumberGeneratorContext(GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
		}

		#region IDocManagerSupportMembers

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ConsolidatedDeclaration)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Override

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = CreateNewDocumentSupporter());
		DocumentSupporter documentSupporter;

		protected virtual DocumentSupporter CreateNewDocumentSupporter()
		{
			return new BaseConsolidatedDeclarationDocumentSupporter(this);
		}

		#endregion

		#region Syncing for Message Processing
		public void SyncStatusAfterMessageProcessing(EDIMessage message)
		{
			if (GlbStaff.CurrentUser.GS_Code != User.ServiceUserCode)
			{
				throw new DeveloperNotificationException("Trying to sync consolidated declarations' status from a front-end process");
			}

			if (message == null || !message.HasChanges || message.EM_LinkedObject != this)
			{
				throw new DeveloperNotificationException("Trying to sync consolidated declarations' status with no accompanying message processing");
			}

			SyncJobDeclarationsStatus();
		}

		void SyncJobDeclarationsStatus()
		{
			if (LeadDeclaration.JE_EntryStatusInfo.HasChanges || CRD_CustomsStatusInfo.HasChanges)
			{
				var newEntryStatus = LeadDeclaration.JE_EntryStatusInfo.HasChanges ? LeadDeclaration.JE_EntryStatus : CRD_CustomsStatus;
				CRD_CustomsStatus = newEntryStatus;
				JobDeclarations.ForEach(dec => dec.JE_EntryStatus = newEntryStatus);
			}
			if (LeadDeclaration.JE_MessageStatusInfo.HasChanges || CRD_MessageStatusInfo.HasChanges)
			{
				var newMessageStatus = LeadDeclaration.JE_MessageStatusInfo.HasChanges ? LeadDeclaration.JE_MessageStatus : CRD_MessageStatus;
				CRD_MessageStatus = newMessageStatus;
				JobDeclarations.ForEach(dec => dec.JE_MessageStatus = newMessageStatus);
			}
			if (LeadDeclaration.JE_EntrySubmittedDateInfo.HasChanges)
			{
				JobDeclarations.ForEach(dec => dec.JE_EntrySubmittedDate = LeadDeclaration.JE_EntrySubmittedDate);
			}
			SyncJobDeclarationsStatusCore();
		}

		protected virtual void SyncJobDeclarationsStatusCore()
		{ }
		#endregion

		internal static void SetOriginalCachedConsolidatedDeclarationCore(BaseJobDeclaration declaration, ConsolidatedDeclaration consolidatedDeclaration)
		{
			var key = GetOriginalConsolidatedDeclarationCachedKey(declaration);
			declaration.Factory.ClearCachedValue<ConsolidatedDeclaration>(key);
			declaration.Factory.ClearCachedValue<ZGuid>(key);
			if (consolidatedDeclaration != null)
			{
				declaration.Factory.GetCachedValue(key, () => consolidatedDeclaration);
			}
		}

		static ConsolidatedDeclaration GetOriginalCachedConsolidatedDeclarationCore(BaseJobDeclaration declaration)
		{
			return declaration.Factory.GetCachedValue<ConsolidatedDeclaration>(GetOriginalConsolidatedDeclarationCachedKey(declaration), () => null);
		}

		static ZGuid QueryConsolidatedDeclarationPKCore(BaseJobDeclaration declaration)
		{
			return declaration.Factory.GetCachedValue(GetOriginalConsolidatedDeclarationCachedKey(declaration), () => declaration.Factory.Load<CusReconEntry>(new ZQuery(CusReconEntrySchema.CRE_CH_OriginalEntry, declaration.CustomsEntryHeaders.Select(_ => _.PK))).FirstOrDefault()?.CRE_CRD ?? ZGuid.Empty);
		}

		static string GetOriginalConsolidatedDeclarationCachedKey(BaseJobDeclaration declaration) => string.Join("|", "OriginalConsolidatedDeclaration", declaration.PK);

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<ConsolidatedDeclarationProcessTask, ConsolidatedDeclaration>(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria() => new ColumnValueRanker();

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.ConsolidatedDeclarationWorkflowDescriptorCode;
		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider() => null;

		IProcessHeaderCollection IWorkflowProvider.Workflows
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

		ZString[] ReleasedCountries => new ZString[]
		{
			Core.Constants.CountryCodes.NewZealand
		};
	}
}
