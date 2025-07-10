using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Metadata.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using CoreConsts = Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	#region Rate Attachments Notification

	public delegate void QuoteAttachmentNotificationsEventHandler(object sender, QuoteAttachmentNotificationsEventArgs args);

	public class QuoteAttachmentNotificationsEventArgs : EventArgs
	{
		public QuoteAttachmentNotificationsEventArgs(QuoteAttachmentsErrorType type, List<RateAttachment> mandatoryAttachments)
			: base()
		{
			this.ErrorType = type;
			this.MandatoryAttachments = mandatoryAttachments;
		}

		public QuoteAttachmentNotificationsEventArgs(QuoteAttachmentsErrorType type)
			: this(type, null)
		{
		}

		public readonly QuoteAttachmentsErrorType ErrorType;

		public readonly List<RateAttachment> MandatoryAttachments;
	}

	public enum QuoteAttachmentsErrorType
	{
		MandatoryRemoved,
		PricingTemplateRemoved,
		WrongPricingTemplateAdded
	}

	#endregion

	#region Document Print Mode Enumeration

	public enum QuotationDocumentMode
	{
		Draft,
		Final,
		Reprint,
		Unknown,
	}

	#endregion

	[UniversalDataContext(DataContextType.Quotation)]
	[CodeProperty(AutoRatingHeader.Schema.TH_QuoteNumber), DescriptionProperty(AutoRatingHeader.Schema.TH_QuoteNumber)]
	[BusinessContext(CargoWise.Definitions.BusinessContext.Quotation)]
	[UniversalCopyWithExtendedEntities]
	[UserDefinedValues]
	[MetadataContext(MetadataContext.Quote)]
	public class Quote
		: RatingHeader
		, ITemplateCopyable
		, IEDocsProvider
		, IWorkflowProvider
		, IDocAddresses
		, IJobHeaderParent
		, ICustomFieldProvider
		, ISalesRelationActivity
		, IImportParentRelatedActivityInfoOnNew
		, IQuote
		, ISupportTradeDetailImporting
		, IWorkflowTriggerEventSource
		, IUniversalXMLNoteParent
	{
		public Quote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TH_QuoteDate = ZDate.Today;
			TH_QuoteEndDate = DefaultQuoteEndDate;

			var followUpDays = RatingDataRegistry.Instance.QuoteFollowUpDays.Value;
			if (followUpDays > 0 && followUpDays < 366)
			{
				TH_FollowUpDate = ZDateTime.Today.AddDays(followUpDays);
			}

			TH_PrintInheritedOriginCharges = PrintInheritedOriginChargesDefault;
			TH_PrintInheritedDestinationCharges = PrintInheritedDestinationChargesDefault;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			OriginalTH_FollowUpDate = TH_FollowUpDate;
		}

		ZDateTime OriginalTH_FollowUpDate;

		static bool PrintInheritedOriginChargesDefault
		{
			get { return DocumentsDataRegistry.Instance.PrintInheritedOriginChargesDefault.Value; }
		}

		static bool PrintInheritedDestinationChargesDefault
		{
			get { return (bool)DocumentsDataRegistry.Instance.PrintInheritedDestinationChargesDefault.Value; }
		}

		#endregion

		#region Accept Quote

		public bool TryAcceptQuote(out ClientRate clientRate, bool reload = true)
		{
			clientRate = null;

			if (!InternalApproveQuoteWhenAccepting())
			{
				return false;
			}

			LockAndAccept();
			UpdateCFXOnOrganisation();

			if (reload)
			{
				existingClientRate = null;
			}

			clientRate = ExistingClientRate;
			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;

			if (clientRate == null)
			{
				clientRate = CreateNewRateFromQuote(rowFactory);
			}
			else
			{
				UpdateOrInsertIntoExistingRates(clientRate, reload, rowFactory);
			}

			return clientRate != null;
		}

		ClientRate ExistingClientRate
		{
			get
			{
				if (existingClientRate == null)
				{
					var existingRateFilter = new ZQuery(RatingHeaderSchema.TH_RateType, SQLComparisonOperator.Equal, (ZString)RatingConstants.RatingHeaderTypes.ClientRate);
					existingRateFilter.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_OH, SQLComparisonOperator.Equal, TH_OH);
					existingRateFilter.AddToFilter(RatingHeaderSchema.TH_GC, Company.PK);

					existingClientRate = Factory.LoadTop1<ClientRate>(existingRateFilter);
				}

				return existingClientRate;
			}
		}
		ClientRate existingClientRate;

		void LockAndAccept()
		{
			TH_IsLocked = true;
			TH_Accepted = ZDateTime.Today;
			Logs.AddNew(AutoEvents.QuotationAccepted);
		}

		void UpdateCFXOnOrganisation()
		{
			if (Header == null)
			{
				return;
			}

			var jobCodes = GetAirSeaJobTypes();

			if (!string.IsNullOrEmpty(jobCodes.air))
			{
				AccCFXConfigurations.SetUplifts(jobCodes.air, OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, TH_AirCFX);
				AccCFXConfigurations.SetUplifts(jobCodes.air, OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, TH_ExportAirCFX);
			}

			if (!string.IsNullOrEmpty(jobCodes.sea))
			{
				AccCFXConfigurations.SetUplifts(jobCodes.sea, OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, TH_SeaCFX);
				AccCFXConfigurations.SetUplifts(jobCodes.sea, OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, TH_ExportSeaCFX);
			}
		}

		(string air, string sea) GetAirSeaJobTypes()
		{
			var jobTypeAndTrnModePairs = AllEntries.SelectMany(InferJobTypeAndTrnMode).Distinct().ToArray();
			var air = jobTypeAndTrnModePairs.FirstOrDefault(x => x.transportMode == CoreConsts.TransportModes.Air).jobTypeCode;
			var sea = jobTypeAndTrnModePairs.FirstOrDefault(x => x.transportMode == CoreConsts.TransportModes.Sea).jobTypeCode;

			return (air, sea);
		}

		IEnumerable<(string jobTypeCode, string transportMode)> InferJobTypeAndTrnMode(RateEntry entry)
		{
			var rateType = RatingConstants.RateCategory.GetRateType(entry.TI_RateCategory);
			var transportMode = RatingConstants.GetTransportModeFromMode(entry.TI_Mode);

			if (!RatingConstants.JobTypesByRateType.TryGetValue(rateType, out var jobTypes))
			{
				return Array.Empty<(string, string)>();
			}

			return jobTypes.Select(x => (x.Code, transportMode.ToString()));
		}

		public bool HasOverlappingClientRate()
		{
			existingClientRate = null;
			if (ExistingClientRate != null)
			{
				ReloadCollectionsWithNoFilter();
				ExistingClientRate.ReloadCollectionsWithNoFilter();

				foreach (var pair in EntryCollectionsExcludingSummary)
				{
					var quoteEntries = pair.Value.LoadedCollection;
					var clientRateEntries = ExistingClientRate.EntryCollections[pair.Key].LoadedCollection;

					foreach (RateEntry quoteEntry in quoteEntries)
					{
						foreach (RateEntry clientRateEntry in clientRateEntries)
						{
							if (quoteEntry.IsDuplicateExcludingColumns(clientRateEntry, nameof(RateEntry.TI_TH))
								&& DoDatesOverlap(clientRateEntry))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		bool DoDatesOverlap(RateEntry rateEntry)
		{
			if (rateEntry.TI_RateEndDate.IsEmpty && TH_QuoteEndDate.IsEmpty)
			{
				return true;
			}

			if (rateEntry.TI_RateEndDate.IsEmpty)
			{
				return rateEntry.TI_RateStartDate <= TH_QuoteEndDate;
			}
			if (TH_QuoteEndDate.IsEmpty)
			{
				return TH_QuoteDate <= rateEntry.TI_RateEndDate;
			}

			return !(rateEntry.TI_RateEndDate < TH_QuoteDate || TH_QuoteEndDate < rateEntry.TI_RateStartDate);
		}

		#region Accepting When No Rates Exist

		ClientRate CreateNewRateFromQuote(RowFactory rowFactory)
		{
			var newRate = (ClientRate)Factory.New(TypesAndCodes.GetType((ZString)RatingConstants.RatingHeaderTypes.ClientRate));
			newRate.CopyPersistentValuesFrom(this);

			newRate.TH_IsLocked = false;
			newRate.TH_RateType = RatingConstants.RatingHeaderTypes.ClientRate;
			newRate.TH_QuoteNumber = ZString.Empty;
			newRate.TH_NewRateEndDate = TH_QuoteEndDate;
			newRate.TH_OH = this.TH_OH;
			newRate.TH_GS_NKFirstSignatory = ZString.Empty;
			newRate.TH_GS_NKSecondSignatory = ZString.Empty;

			foreach (var pair in EntryCollectionsExcludingSummary)
			{
				var quoteCollection = pair.Value.LoadedCollection;
				AddNewRateEntriesToRate(quoteCollection, newRate.EntryCollections[pair.Key], rowFactory);
			}

			newRate.TH_QuoteEndDate = ZDate.Empty;

			return newRate;
		}

		void AddNewRateEntriesToRate(RateEntryCollection quoteCollection, RateEntryCollection rateCollection, RowFactory rowFactory)
		{
			foreach (QuoteEntry oldQuoteEntry in quoteCollection)
			{
				InsertNewRate(oldQuoteEntry, rateCollection, rowFactory);
			}
		}

		#endregion

		#region Accepted When Rates Exist

		void UpdateOrInsertIntoExistingRates(ClientRate acceptedRate, bool reload, RowFactory rowFactory)
		{
			if (reload)
			{
				ReloadCollectionsWithNoFilter();
				acceptedRate.ReloadCollectionsWithNoFilter();
			}

			foreach (var pair in EntryCollectionsExcludingSummary)
			{
				var quoteCollection = pair.Value.LoadedCollection;
				var rateCollection = acceptedRate.EntryCollections[pair.Key].LoadedCollection;

				UpdateOrInsertIntoExistingRates(quoteCollection, rateCollection, rowFactory);
			}
		}

		void UpdateOrInsertIntoExistingRates(RateEntryCollection quoteCollection, RateEntryCollection rateCollection, RowFactory rowFactory)
		{
			foreach (RateEntry quoteEntry in quoteCollection)
			{
				ExpireExistingRates(quoteEntry, rateCollection);
				InsertNewRate(quoteEntry, rateCollection, rowFactory);
			}
		}

		void ExpireExistingRates(RateEntry quoteEntry, RateEntryCollection rateCollection)
		{
			// Earliest start date that overlaps with the Quotation
			var earliestDate = TH_QuoteDate;
			foreach (RateEntry rateEntry in rateCollection.Where(x => !x.IsPublished && DoesQuoteMatchRateForExpiry(quoteEntry, x)))
			{
				if (rateEntry.TI_RateStartDate >= TH_QuoteDate
					&& rateEntry.TI_RateEndDate <= TH_QuoteEndDate)
				{
					// Expire the rate that would be overwritten by new quotation
					rateEntry.TI_RateStartDate = earliestDate.AddDays(-1);
					rateEntry.TI_RateEndDate = earliestDate.AddDays(-1);
					continue;
				}

				rateEntry.IsAccepting = true;

				if ((rateEntry.TI_RateEndDate >= TH_QuoteDate
					|| rateEntry.TI_RateEndDate.IsEmpty)
					&& rateEntry.TI_RateStartDate <= TH_QuoteDate)
				{
					rateEntry.TI_RateEndDate = TH_QuoteDate.AddDays(-1);
					RateLineHelper.UpdateAllRateLinesEndDate(rateEntry.RateLines.Cast<RateLine>(), rateEntry.TI_RateEndDate);
				}

				if (rateEntry.TI_RateStartDate > rateEntry.TI_RateEndDate)
				{
					rateEntry.TI_RateStartDate = rateEntry.TI_RateEndDate;
				}

				if (rateEntry.TI_RateStartDate > TH_QuoteDate
					&& rateEntry.TI_RateStartDate < TH_QuoteEndDate)
				{
					rateEntry.TI_RateStartDate = TH_QuoteEndDate.AddDays(1);
					RateLineHelper.UpdateAllRateLinesStartDate(rateEntry.RateLines.Cast<RateLine>(), rateEntry.TI_RateStartDate);
				}

				if (rateEntry.TI_RateStartDate < earliestDate)
				{
					earliestDate = rateEntry.TI_RateStartDate;
				}
			}
		}

		bool DoesQuoteMatchRateForExpiry(RateEntry quoteEntry, RateEntry rateEntry)
		{
			if (!quoteEntry.IsDuplicateExcludingColumns(rateEntry, nameof(RateEntry.TI_TH))
				|| !DoDatesOverlap(rateEntry))
			{
				return false;
			}

			if (rateEntry.TI_MatchContainerRateClass != quoteEntry.TI_MatchContainerRateClass)
			{
				return false;
			}
			if (!rateEntry.TI_MatchContainerRateClass && rateEntry.TI_RC != quoteEntry.TI_RC)
			{
				return false;
			}
			if (rateEntry.TI_MatchContainerRateClass && rateEntry.ContainerClass != quoteEntry.ContainerClass)
			{
				return false;
			}

			return true;
		}

		#endregion

		void InsertNewRate(RateEntry quoteEntry, RateEntryCollection rateCollection, RowFactory rowFactory)
		{
			var newRateEntry = quoteEntry.DeepClone(rateCollection, rowFactory: rowFactory);
			newRateEntry.TI_RateStartDate = quoteEntry.Parent.TH_QuoteDate;
			newRateEntry.TI_RateEndDate = quoteEntry.Parent.TH_QuoteEndDate;
			newRateEntry.IsAccepting = true;
		}

		#endregion

		#region Cancel Quote

		public void CancelQuote()
		{
			IsCancelled = true;
			SetReadOnlyIncludingChildren(true);
			Validation.ValidateAll();
			Logs.AddNew(Events.QuotationCancelled);
		}

		#endregion

		#region Reactivate

		public void Reactivate()
		{
			TH_IsCancelled = false;
			SetReadOnlyIncludingChildren(false);
		}

		#endregion

		#region Save Quote

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!IsImportingQuotedBooking && (HasChanges || (CurrentOneOffQuote != null && CurrentOneOffQuote.HasChanges)))
			{
				InternalApproveQuoteWhenSaving();

				if (!TH_OneTimeQuote)
				{
					new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges(CurrentOneOffQuote != null && CurrentOneOffQuote.HasChanges));
				}
			}
		}

		bool IsImportingQuotedBooking => (ParentQuotedBooking as ISupportDataImporting)?.IsImportingData ?? false;

		protected override void RunPreSaveValidationCore()
		{
			if (!TH_OneTimeQuote)
			{
				QuoteFormatEntries.LoadEntries();
				QuoteFormatEntries.Cast<QuoteEntry>().ForEach(qe => qe.Validation.ValidateTI_QuotePageIncoTerm());
			}

			base.RunPreSaveValidationCore();
		}

		public override void OnSaving()
		{
			DeactivateJobHeaderWhenIsCancelled();
			SetQuoteNumber();
			base.OnSaving();
			SetStartEndDateForOneOffQuotes();
			OnCancelPreviousAmendment();
		}

		#region DeleteJobHeaderWhenIsCancelled

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		#endregion

		#region Quote Number

		public void SetQuoteNumber()
		{
			if (!IsInDatabase)
			{
				if (TH_QuoteNumber.IsEmpty || isInProcessOfFixingFountain)  // New Quotation
				{
					TH_QuoteNumber = NumberFountainForQuoteNumber.GetNextFormatted(Factory);
					isInProcessOfFixingFountain = false;
				}
				else if (!QuoteNumberAlreadySet)    // Amendment Quotation
				{
					TH_QuoteNumber = QuoteNumberWithoutAmendmentSuffix;
					TH_QuoteNumber = GetNewQuoteNumberForAmendment();
				}
				QuoteNumberAlreadySet = true;
			}
		}

		public bool QuoteNumberAlreadySet {  get; set; }

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (uniqueIndexFailureHandler == null)
				{
					uniqueIndexFailureHandler = new QuoteCustomisedNumberUniqueIndexFailureHandler(this);
				}

				yield return uniqueIndexFailureHandler;
			}
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;
		bool isInProcessOfFixingFountain;

		class QuoteCustomisedNumberUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public QuoteCustomisedNumberUniqueIndexFailureHandler(Quote quote)
				: base(RatingHeaderSchema.Constants.Indexes.FK_UX__TH_OH_TH_GC_TH_RateType_TH_QuoteNumber_TH_GlobalRateLevel, quote)
			{
				this.quote = quote;
			}

			[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			protected override CargoWise.Data.DbCommand CommandToFindMaxValueInDatabase(CargoWise.Data.DbConnection connection)
			{
				var includeBranchCode = RatingDataRegistry.Instance.IncludeBranchCodeInQuoteNumber.Value;
				var suffixLength = quote.FailedSaveQuoteNumber.Length - GetQuoteNumberWithoutAmendmentSuffix(quote.FailedSaveQuoteNumber).Length;
				string columnReplacement;
				string branchCodeSql;
				string branchCode = null;
				if (includeBranchCode)
				{
					branchCode = Env.CurrentBranch.Code;
					var prefixLength = 1 + branchCode.Length;
					columnReplacement = FormattableString.Invariant($"SUBSTRING({RatingHeaderSchema.TH_QuoteNumber.Name}, {1 + prefixLength}, {quote.FailedSaveQuoteNumber.Length - prefixLength - suffixLength})");
					branchCodeSql = FormattableString.Invariant($"\r\n\tAND SUBSTRING(TH_QuoteNumber, 2, {branchCode.Length}) = @BranchCode");
				}
				else
				{
					columnReplacement = RatingHeaderSchema.TH_QuoteNumber.Name;
					branchCodeSql = string.Empty;
				}

				var sqlText = FormattableString.Invariant($@"SELECT MAX({columnReplacement})
	FROM {RatingHeaderSchema.Constants.SqlSchemaName}.{RatingHeaderSchema.Constants.TableName}
	WHERE ISNUMERIC({columnReplacement}) = 1
		AND LEN({RatingHeaderSchema.TH_QuoteNumber.Name}) = @QuoteNumberLength")
					+ branchCodeSql;

				quote.isInProcessOfFixingFountain = true;

				var command = connection.Command(sqlText);
				command.AddParameter("@QuoteNumberLength", SqlDbType.Int, quote.FailedSaveQuoteNumber.Length - suffixLength);
				if (includeBranchCode)
				{
					command.AddParameter("@BranchCode", SqlDbType.VarChar, branchCode.Length, branchCode);
				}
				return command;
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return this.quote.NumberFountainForQuoteNumber; }
			}

			readonly Quote quote;
		}

		#endregion

		#region NumberFountainForQuoteNumber

		protected virtual INumberFountainProxy NumberFountainForQuoteNumber
		{
			get
			{
				return (RatingDataRegistry.Instance.IncludeBranchCodeInQuoteNumber.Value) ? Env.NumberFountains.QuoteCustomisedNumber(Env.CurrentBranch.Code) : Env.NumberFountains.QuoteNumber;
			}
		}

		#endregion

		#endregion

		#region Approve Quote

		public bool InternalApproveQuote()
		{
			if (InternalApprove(ApprovalDialog.WishToApprove, true))
			{
				confirmResult = true;
				return true;
			}

			return false;
		}

		bool InternalApproveQuoteWhenAccepting()
		{
			return InternalApprove(ApprovalDialog.WishToApproveWhenAccepting, false);
		}

		bool InternalApproveQuoteWhenFinalizing()
		{
			return InternalApprove(ApprovalDialog.WishToApproveWhenFinalizing, false);
		}

		void InternalApproveQuoteWhenSaving()
		{
			if (IsApproved && IsInternalApprovalRequired && HasChangesForApproval)
			{
				ClearApproval();
			}

			if (!IsApproved && IsApproveValid())
			{
				if (!IsInternalApprovalRequired)
				{
					ApproveQuote();
				}
				else
				{
					if (IsApproveAllowed(Env.Security))
					{
						if (confirmResult == null)
						{
							confirmResult = ConfirmApprovalDialog(ApprovalDialog.WishToApproveWhenSaving);
						}

						if (confirmResult.Value)
						{
							ApproveQuote();
						}
					}
				}
			}
		}

		bool? confirmResult;

		bool HasChangesForApproval
		{
			get
			{
				foreach (RateEntryCollection collection in EntryCollectionsExcludingSummary.Values)
				{
					if (collection.HasChanges)
					{
						return true;
					}
				}

				return QuotationClientAddress.HasChanges;
			}
		}

		public bool IsInternalApprovalRequired
		{
			get
			{
				if (TH_OneTimeQuote)
				{
					return DataRegistryRating.Instance.SpotQuoteRequireInternalApproval.Value;
				}

				return DataRegistryRating.Instance.QuoteRequireInternalApproval.Value;
			}
		}

		bool IsApproveAllowed(SecurityCore security)
		{
			if (TH_OneTimeQuote)
			{
				return IsOneOffQuoteApproveAllowed(security);
			}

			return security.QuotationApprove.IsAllowed;
		}

		bool IsApproveValid()
		{
			return !TH_OneTimeQuote || IsOneOffQuoteApproveValid();
		}

		bool IsOneOffQuoteApproveValid()
		{
			var jobHeader = new JobHeader.Loader(this).Load();
			if (jobHeader == null)
			{
				return true;
			}
			var hasCharges = ValidOneOffQuoteChargesExist;
			if (IsLocalClientRole)
			{
				return !jobHeader.LocalChargesPK.IsEmpty || !hasCharges;
			}
			else if (CurrentOneOffQuote.TT_OrgRole == CoreConsts.OrgRoles.OverseasAgent)
			{
				return !jobHeader.AgentCollectPK.IsEmpty || !hasCharges;
			}
			else
			{
				return !hasCharges;
			}
		}

		bool IsOneOffQuoteApproveAllowed(SecurityCore security)
		{
			var job = (IJobWithRevenueTotal)new JobHeader.Loader(this).Load();
			var revenue = job != null ? job.JH_TotalRevenue : 0;
			var unapprovedQuoteCheckPoint = new UnapprovedQuoteValidationHelper(security).GetSecurityCheckPoint(revenue);
			var isNoSecurityRequired = unapprovedQuoteCheckPoint.Code == security.None.Code;
			var isApprovalAllowed = isNoSecurityRequired || unapprovedQuoteCheckPoint.IsAllowed;

			return isApprovalAllowed;
		}

		public bool IsApproved
		{
			get
			{
				if (isApproved == null)
				{
					isApproved = TH_OneTimeQuote ? (bool)CurrentOneOffQuote.TT_QuoteApprovedByManager : Logs.Find(GetApprovedLogQuery(PK, ZBool.False)).Length > 0;
				}

				return isApproved.Value;
			}
		}
		bool? isApproved;

		internal static ZQuery GetApprovedLogQuery(ZGuid pk, ZBool isCancelled)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.QuotationInternallyApproved.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, pk);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, isCancelled);

			return query;
		}

		#region Events

		public event EventHandler<ApprovalDialogEventArgs> ShowApprovalDialog;
		public void OnShowApprovalDialog(ApprovalDialogEventArgs args)
		{
			if (ShowApprovalDialog != null)
			{
				ShowApprovalDialog(this, args);
			}
		}
		public class ApprovalDialogEventArgs : CancelEventArgs
		{
			public ApprovalDialogEventArgs(ApprovalDialog dialog)
			{
				this.dialog = dialog;
			}

			public ApprovalDialog Dialog
			{
				get { return dialog; }
			}

			readonly ApprovalDialog dialog;
		}
		public enum ApprovalDialog
		{
			WishToApprove,
			WishToApproveWhenAccepting,
			WishToApproveWhenFinalizing,
			WishToApproveWhenSaving,
		}

		public delegate void ApprovalMessageEventHandler(ApprovalMessage approvalMessage);
		public event ApprovalMessageEventHandler ShowApprovalMessage;
		public void OnShowApprovalMessage(ApprovalMessage approvalMessage)
		{
			if (ShowApprovalMessage != null)
			{
				ShowApprovalMessage(approvalMessage);
			}
		}
		public enum ApprovalMessage
		{
			AlreadyApprovedMessage,
			HaveNoRightsMessage,
			LoginFailedMessage,
			MissingLocalClientMessage,
			MissingOverseasAgentMessage
		}

		public event EventHandler<QuoteApprovalSecurityEventArgs> QuoteApprovalSecurityNotGranted;
		public void OnQuoteApprovalSecurityNotGranted(QuoteApprovalSecurityEventArgs args)
		{
			if (QuoteApprovalSecurityNotGranted != null)
			{
				QuoteApprovalSecurityNotGranted(this, args);
			}
		}
		public class QuoteApprovalSecurityEventArgs : CancelEventArgs
		{
			public SecurityOverridenLogin OverrideLogin
			{
				get { return fOverrideLogin; }
			}

			readonly SecurityOverridenLogin fOverrideLogin = new SecurityOverridenLogin();
		}

		#endregion

		bool InternalApprove(ApprovalDialog dialog, bool errorIfAlreadyApproved)
		{
			if (!IsApproveValid())
			{
				if (CurrentOneOffQuote.TT_OrgRole == CoreConsts.OrgRoles.OverseasAgent)
				{
					OnShowApprovalMessage(ApprovalMessage.MissingOverseasAgentMessage);
				}
				else
				{
					OnShowApprovalMessage(ApprovalMessage.MissingLocalClientMessage);
				}

				return false;
			}

			if (IsApproved)
			{
				if (errorIfAlreadyApproved)
				{
					OnShowApprovalMessage(ApprovalMessage.AlreadyApprovedMessage);
				}

				return true;
			}

			if (!IsInternalApprovalRequired)
			{
				return ApproveQuote();
			}

			if (!IsApproveAllowed(Env.Security))
			{
				return SwitchSecurityAndApproveQuote();
			}

			return ConfirmApprovalDialog(dialog) && ApproveQuote();
		}

		bool ApproveQuote()
		{
			if (!Globals.IsUserInteractive)
			{ return false; }
			if (TH_OneTimeQuote)
			{
				CurrentOneOffQuote.TT_QuoteApprovedByManager = true;
			}

			Logs.AddNew(Events.QuotationInternallyApproved);

			isApproved = true;

			return true;
		}

		void ClearApproval()
		{
			foreach (var log in Logs.Find(GetApprovedLogQuery(PK, ZBool.False)))
			{
				log.Cancel();
			}

			if (TH_OneTimeQuote)
			{
				CurrentOneOffQuote.TT_QuoteApprovedByManager = false;
			}

			isApproved = false;
		}

		bool ConfirmApprovalDialog(ApprovalDialog dialog)
		{
			var args = new ApprovalDialogEventArgs(dialog);
			OnShowApprovalDialog(args);

			return !args.Cancel;
		}

		bool SwitchSecurityAndApproveQuote()
		{
			var args = new QuoteApprovalSecurityEventArgs();
			OnQuoteApprovalSecurityNotGranted(args);

			if (!args.Cancel)
			{
				if (args.OverrideLogin.UserSecurity == null)
				{
					OnShowApprovalMessage(ApprovalMessage.LoginFailedMessage);
				}
				else if (!IsApproveAllowed(args.OverrideLogin.UserSecurity))
				{
					OnShowApprovalMessage(ApprovalMessage.HaveNoRightsMessage);
				}
				else
				{
					return ApproveQuote();
				}
			}

			return false;
		}

		#endregion

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (!IsDeleted)
				{
					result.AddRange(OneOffQuote);
				}

				return result.ToArray();
			}
		}

		#region Properties

		#region IsTrackingQuote

		public virtual bool IsTrackingQuote
		{
			get { return false; }
		}

		#endregion

		#region IsInComparisonMode

		public bool IsInComparisonMode { get; set; }

		#endregion

		protected override bool IsCFXEditable
		{
			get { return true; }
		}

		#region TH_QuoteDate

		public override ZDate TH_QuoteDate
		{
			get { return base.TH_QuoteDate; }
			set
			{
				base.TH_QuoteDate = value;

				foreach (var entry in AllEntries)
				{
					entry.TI_RateStartDate = value;
				}
			}
		}

		#endregion

		#region TH_QuoteEndDate

		public override ZDate TH_QuoteEndDate
		{
			get { return base.TH_QuoteEndDate; }
			set
			{
				base.TH_QuoteEndDate = value;

				foreach (var entry in AllEntries)
				{
					entry.TI_RateEndDate = value;
				}
			}
		}

		#endregion

		#region Entries

		protected override IBusinessObjectCollection GetAllEntriesCore()
		{
			return new ActiveQuoteEntryCollection(this);
		}

		#endregion

		#region Document Print Mode

		public QuotationDocumentMode DocumentPrintMode { get; set; }

		#endregion

		public bool HideClientReplyLink
		{
			get
			{
				if (DocumentPrintMode == QuotationDocumentMode.Draft || DocumentPrintMode == QuotationDocumentMode.Unknown)
				{
					return true;
				}

				var statusListForHiding = new List<ZString>() {
					QuoteStatusOptions.Used,
					QuoteStatusOptions.Cancelled,
					QuoteStatusOptions.ClientAccepted,
					QuoteStatusOptions.Accepted
				};

				if (statusListForHiding.Contains(QuoteStatus))
				{
					return true;
				}

				return false;
			}
		}

		public bool IsLocalClientRole => CurrentOneOffQuote.TT_OrgRole == CoreConsts.OrgRoles.LocalClient;

		public string QuotationAcceptText
		{
			get
			{
				if (ParentQuotedBooking != null
					&& ParentQuotedBooking.ForwardingShipment == null)
				{
					return ResString.GetMultilingualString("b71b6c80-6ac9-4348-9730-420ccfea05da", "Accept One Off Quote");
				}
				return ResString.GetMultilingualString("1df37a08-be8a-400f-8292-04c1dfbd844b", "Accept Quotation");
			}
		}

		public string QuotationAcceptTooltip
		{
			get
			{
				if (ParentQuotedBooking != null
					&& ParentQuotedBooking.ForwardingShipment == null)
				{
					return ResString.GetMultilingualString("b71b6c80-6ac9-4348-9730-420ccfea05da", "Accept One Off Quote");
				}
				return ResString.GetMultilingualString("1df37a08-be8a-400f-8292-04c1dfbd844b", "Accept Quotation");
			}
		}

		#region TH_OH

		[ReadOnlyMember(nameof(SameClientCopy))]
		public override ZGuid TH_OH
		{
			get { return QuotationClientAddress.OrganisationPK; }
			set
			{
				if (this.IsClientRate())
				{
					throw new DeveloperNotificationException("This field not supported for RateType 'SAL'");
				}

				var hasChanged = TH_OH != value;
				QuotationClientAddress.OrganisationPK = value;

				if (hasChanged && Header != null)
				{
					SetCFXDefaultValues();
					SetDefaultSignatures();
				}
			}
		}

		public override ZPropertyInfo TH_OHInfo
		{
			get { return GetWrappedZPropertyInfo(AutoRatingHeader.Schema.TH_OH, x => QuotationClientAddress.OrganisationPKInfo); }
		}

		#endregion

		#region TH_GC

		[LightValidationTestExempt]
		public override ZGuid TH_GC
		{
			get { return base.TH_GC; }
			set
			{
				var hasChanged = TH_GC != value;
				base.TH_GC = value;

				if (hasChanged && Header != null)
				{
					SetCFXDefaultValues();
					SetDefaultSignatures();
				}
			}
		}

		#endregion

		#region SetCFXDefaultValues

		void SetCFXDefaultValues()
		{
			if (Header == null)
			{
				return;
			}

			var jobCodes = GetAirSeaJobTypes();

			TH_AirCFX = AccCFXConfigurations.GetRecord(jobCodes.air, OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air)?.JCF_CFXPercentage ?? ZDecimal.Zero;
			TH_AirCFXInfo.RefreshBinding();

			TH_ExportAirCFX = AccCFXConfigurations.GetRecord(jobCodes.air, OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air)?.JCF_CFXPercentage ?? ZDecimal.Zero;
			TH_ExportAirCFXInfo.RefreshBinding();

			TH_SeaCFX = AccCFXConfigurations.GetRecord(jobCodes.sea, OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea)?.JCF_CFXPercentage ?? ZDecimal.Zero;
			TH_SeaCFXInfo.RefreshBinding();

			TH_ExportSeaCFX = AccCFXConfigurations.GetRecord(jobCodes.sea, OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea)?.JCF_CFXPercentage ?? ZDecimal.Zero;
			TH_ExportSeaCFXInfo.RefreshBinding();
		}

		#endregion

		#region SetDefaultSignatures

		public void SetDefaultSignatures()
		{
			using (SuspendSettingHasChanges())
			{
				if (Header != null && HeaderStaffAssignments?.OverallSalesRepStaff != null)
				{
					TH_GS_NKFirstSignatory = HeaderStaffAssignments.OverallSalesRepStaff.GS_Code;
				}
				else
				{
					TH_GS_NKFirstSignatory = GlbStaff.CurrentUser.GS_Code;
				}

				TH_GS_NKSecondSignatory = ZString.Empty;
			}
		}

		#endregion

		#region Quotation Number

		public ZString QuoteNumberWithoutAmendmentSuffix => GetQuoteNumberWithoutAmendmentSuffix(TH_QuoteNumber);

		static ZString GetQuoteNumberWithoutAmendmentSuffix(string quoteNumber) => quoteNumber.Contains("/")
			? quoteNumber.Substring(0, quoteNumber.IndexOf("/"))
			: quoteNumber;

		[ReadOnly(true)]
		public override ZString TH_QuoteNumber
		{
			get { return base.TH_QuoteNumber; }
			set { base.TH_QuoteNumber = value; }
		}

		#endregion

		#region Quote Status

		// Note: If there is any change in this class, please also consider update the QuoteStatus class in Glow.
		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public sealed class QuoteStatusOptions
		{
			/// <summary>
			/// A quote accepted for the client and converted into client rates.
			/// This is the last state for the quote.
			/// </summary>
			public static ZString Accepted
			{
				get { return ResString.GetMultilingualString("1e4b731e-91d8-44a1-9990-5917474b1c3f", "Accepted"); }
			}

			/// <summary>
			/// A quote starts in this state.
			/// </summary>
			public static ZString Active
			{
				get { return ResString.GetMultilingualString("aa51d897-2685-4de1-baef-43bc0edd10c2", "Active"); }
			}

			/// <summary>
			/// When an approved quote is printed as Final it then becomes Finalized.
			/// </summary>
			public static ZString Finalized
			{
				get { return ResString.GetMultilingualString("245f4c2e-52e2-4afc-b9c7-900af8416f1f", "Finalized"); }
			}

			/// <summary>
			/// Quotes can be canceled at any stage except the Accepted state.
			/// </summary>
			public static ZString Cancelled
			{
				get { return ResString.GetMultilingualString("258bdd00-9ee7-4e8a-9ec4-539a432c25a6", "Canceled"); }
			}

			/// <summary>
			/// Quotes can expire at any stage except ClientAccepted, or Accepted, or Used.
			/// </summary>
			public static ZString Expired
			{
				get { return ResString.GetMultilingualString("56cc9b85-da7f-4fb9-9dd3-f2be8b43e068", "Expired"); }
			}

			public static ZString Used
			{
				get { return ResString.GetMultilingualString("e6216829-ae23-4e75-ba70-06b696c42520", "Used"); }
			}

			/// <summary>
			/// After being active, a quote can be approved or used.
			/// </summary>
			public static ZString Approved
			{
				get { return ResString.GetMultilingualString("4e0b78c5-20f2-4b7f-a9c3-8f23e1c57b03", "Approved"); }
			}

			/// <summary>
			/// A state just before Accepted where the client has accepted
			/// a quote themselves. This still needs to be Accepted afterwards.
			/// </summary>
			public static ZString ClientAccepted
			{
				get { return ResString.GetMultilingualString("5a0e5d1c-0c4f-4061-9a3c-ae325fd77ce3", "Client Accepted"); }
			}
		}

		/* Here's a diagram of how the states work.

		  ┌──────────────────────────────────────────────────────────────┐
		  │ Can be cancelled                                             │
		  │                                                              │
		  │   ┌──────────────────────────────────────────────────────┐   │
		  │   │                                                      │   │
		  │   │    Can expire                                        │   │
		  │   │                               ┌────────────┐         │   │
		  │   │                               │            │         │   │
		  │   │                               │   Active   │         │   │
		  │   │                               │            │         │   │
		  │   │                               └─┬───┬──────┘         │   │
		  │   │   ┌──────────────────┐          │   │                │   │
		  │   │   │  Print as draft  │◄─────────┘   │                │   │
		  │   │   └──────────────────┘              │                │   │
		  │   │               ▲           ┌─────────▼─────────────┐  │   │
		  │   │               └───────────┤       Approved        │  │   │
		  │   │                           │(according to registry)│  │   │
		  │   │               ┌───────────┴───────────────────────┘  │   │
		  │   │               │                                      │   │
		  │   │  ┌────────────▼───┐          ┌────────────┐          │   │
		  │   │  │                │          │            │          │   │
		  │   │  │  Print as final├──────────►  Finalized │          │   │
		  │   │  │                │          │            │          │   │
		  │   │  └────────────────┘ ┌────────┴──────┬─────┴────────┐ │   │
		  │   │                     │               │              │ │   │
		  │   │                     │               │              │ │   │
		  │   │                     │               │              │ │   │
		  │   │                     │               │              │ │   │
		  │   └─────────────────────┼───────────────┼──────────────┼─┘   │
		  │                         │               │              │     │
		  │                         │       ┌───────▼────────────┐ │     │
		  │                         │       │                    │ │     │
		  │                         │       │ ClientAccepted     │ │     │
		  │                         │       │                    │ │     │
		  │                         │       └────────┬───────────┘ │     │
		  │                         │                │             │     │
		  └─────────────────────────┼────────────────┼─────────────┼─────┘
									│                │             │Consolidated
									│                │             │Autorated For BWQ
							   ┌────▼─────┐          │    ┌────────▼─┐
							   │ Accepted ◄──────────┘    │   Used   │
							   └──────────┘               └──────────┘

		*/
		// Note: This QuoteStatus must match up against QuotationsFilterBusinessObject.cs
		// GetQuoteStatusQuery
		public ZString QuoteStatus
		{
			get
			{
				if (TH_IsCancelled)
				{
					return QuoteStatusOptions.Cancelled;
				}

				if (!TH_Accepted.IsEmpty)
				{
					return QuoteStatusOptions.Accepted;
				}

				if (TH_IsOneOffQuoteConsumed)
				{
					return QuoteStatusOptions.Used;
				}

				if (IsClientAccepted)
				{
					return QuoteStatusOptions.ClientAccepted;
				}

				if (TH_QuoteEndDate >= ZDateTime.Today || TH_QuoteEndDate.IsEmpty)
				{
					if (TH_IsLocked)
					{
						return QuoteStatusOptions.Finalized;
					}
					else
					{
						if (IsApproved)
						{
							return QuoteStatusOptions.Approved;
						}

						return QuoteStatusOptions.Active;
					}
				}
				else
				{
					return QuoteStatusOptions.Expired;
				}
			}
		}

		public bool IsClientAccepted => !TH_ClientAccepted.IsEmpty;

		public ZPropertyInfo QuoteStatusInfo
		{
			get { return GetZPropertyInfo(nameof(QuoteStatus)); }
		}

		#endregion

		#region TH_QuoteCancellationReason

		[List("Lookups.ActiveQuoteCancellationReasonCodes")]
		public override ZString TH_QuoteCancellationReason
		{
			get { return base.TH_QuoteCancellationReason; }
			set { base.TH_QuoteCancellationReason = value; }
		}

		public ZString QuoteCancellationReasonDescription
		{
			get { return Lookups.QuoteCancellationReasonCodes.GetDescriptionFromCode(TH_QuoteCancellationReason); }
		}

		#endregion

		#region Is One Off Quote In Database

		public ZBool IsOneOffQuoteInDatabase
		{
			get { return TH_OneTimeQuote && IsInDatabase; }
		}

		public ZPropertyInfo IsOneOffQuoteInDatabaseInfo
		{
			get { return GetZPropertyInfo(nameof(IsOneOffQuoteInDatabase)); }
		}

		#endregion

		#region DisplayInfo / RatingHeaderTypeDescription

		protected override string RatingHeaderTypeDescriptionCore =>
			Env.Registry.Rating.QuoteTitleText;

		public override ZString DisplayInfo()
			=> RatingHeaderTypeDescription + " " + TH_QuoteNumber;

		#endregion

		#endregion

		#region Validation

		public new QuoteValidation Validation
		{
			get { return (QuoteValidation)base.Validation; }
		}

		protected override RatingHeaderValidation GetNewValidation()
		{
			return new QuoteValidation(this);
		}

		#endregion

		#region Copy / Amend Quotes Functionality

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedQuote = (Quote)base.CloneInternal(args);
			using (clonedQuote.GetValidationSuspender())
			{
				foreach (StmNote note in Notes.GetAllNotes())
				{
					clonedQuote.Notes.Add(note.Clone());
				}
			}

			return clonedQuote;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		public ZString GetNewQuoteNumberForAmendment()
		{
			var highestUnlockedAmendment = GetMostRecentAmendment(true);

			char newSuffix;
			if (highestUnlockedAmendment == null)
			{
				newSuffix = 'A';
			}
			else
			{
				var highestSuffix = highestUnlockedAmendment.TH_QuoteNumber.Substring(highestUnlockedAmendment.TH_QuoteNumber.IndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
				var highestSuffixAscii = (int)char.Parse(highestSuffix);
				newSuffix = (char)(highestSuffixAscii + 1);
			}

			return QuoteNumberWithoutAmendmentSuffix + "/" + newSuffix;
		}

		public Quote GetMostRecentAmendment(bool canReturnLockedAndCurrentQuote)
		{
			var quoteFilter = new ZQuery(RatingHeaderSchema.TH_QuoteNumber, SQLComparisonOperator.StartsWith, QuoteNumberWithoutAmendmentSuffix);
			if (!canReturnLockedAndCurrentQuote)
			{
				quoteFilter.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_IsLocked, SQLComparisonOperator.Equal, false);
			}

			quoteFilter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Quote);

			var quotes = Factory.Load<Quote>(quoteFilter);

			Quote result = null;
			var highestSuffixAscii = 0;

			// Get the highest suffixed quote
			foreach (var matchingQuote in quotes)
			{
				var suffixAscii = 0;
				if (matchingQuote.TH_QuoteNumber.Contains("/", StringComparison.OrdinalIgnoreCase))
				{
					var suffix = matchingQuote.TH_QuoteNumber.Substring(matchingQuote.TH_QuoteNumber.IndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
					suffixAscii = char.Parse(suffix);
				}

				result = suffixAscii > highestSuffixAscii && (canReturnLockedAndCurrentQuote || matchingQuote != this) ? matchingQuote : result;
				highestSuffixAscii = highestSuffixAscii > suffixAscii ? highestSuffixAscii : suffixAscii;
			}

			return result;
		}

		/// <summary>
		/// This method is called when a Quote is copied. It's given the copy
		/// to initialise any fields that ought to be different to the source.
		/// </summary>
		/// <param name="newHeader"></param>
		protected override void SetNewValuesInHeader(RatingHeader newHeader)
		{
			base.SetNewValuesInHeader(newHeader);
			newHeader.TH_Accepted = ZDateTime.Empty;
			newHeader.TH_ClientAccepted = ZDateTime.Empty;
			newHeader.TH_QuoteDate = ZDate.Today;
			newHeader.TH_QuoteEndDate = newHeader.DefaultQuoteEndDate;
			newHeader.TH_IsLocked = false;
			newHeader.TH_IsCancelled = false;
			newHeader.TH_QuoteCancellationReason = ZString.Empty;
		}

		[ThreadStatic]
		static List<ZGuid> SameClientCopyList;

		public ZBool SameClientCopy
		{
			get
			{
				if (SameClientCopyList == null)
				{
					return false;
				}
				else
				{
					return SameClientCopyList.Contains(PK);
				}
			}
			set
			{
				if (SameClientCopyList == null)
				{
					SameClientCopyList = new List<ZGuid>();
				}

				if (value)
				{
					SameClientCopyList.Add(PK);
				}
				else
				{
					SameClientCopyList.Remove(PK);
				}
			}
		}

		[ThreadStatic]
		static List<ZGuid> AmendmentCopyList;

		public ZBool AmendmentCopy
		{
			get
			{
				if (AmendmentCopyList == null)
				{
					return false;
				}
				else
				{
					return AmendmentCopyList.Contains(PK);
				}
			}
			set
			{
				if (AmendmentCopyList == null)
				{
					AmendmentCopyList = new List<ZGuid>();
				}

				if (value)
				{
					AmendmentCopyList.Add(PK);
				}
				else
				{
					AmendmentCopyList.Remove(PK);
				}
			}
		}

		delegate void CancelPreviousAmendmentEventHandler();
		event CancelPreviousAmendmentEventHandler CancelPreviousAmendment;

		void OnCancelPreviousAmendment()
		{
			if (CancelPreviousAmendment != null)
			{
				CancelPreviousAmendment();
			}
		}

		void CancelAmendmentHandler()
		{
			if (!IsDeleted && !TH_OneTimeQuote)
			{
				TH_IsCancelled = ZBool.True;
			}
		}

		public override RatingHeader CopyIncludingChildren()
		{
			var copiedHeader = (Quote)Clone();

			foreach (var entry in AllEntries.ToList())
			{
				entry.DeepClone(copiedHeader.EntryCollections[entry.TI_RateCategory]);
			}

			SetNewValuesInHeader(copiedHeader);

			if (TH_OneTimeQuote && CurrentOneOffQuote != null)
			{
				copiedHeader.fOneOffQuotes.RemoveAndDeleteAll();
				copiedHeader.fOneOffQuotes.Add((RateOneOffShipment)CurrentOneOffQuote.Clone());

				CopyJobBillingInformation(copiedHeader);
			}
			copiedHeader.TH_IsOneOffQuoteConsumed = false;

			return copiedHeader;
		}

		void CopyJobBillingInformation(Quote clonedQuote)
		{
			var clonedJob = new JobHeader.Loader(clonedQuote).TryCreate();

			if (clonedJob == null)
			{
				return;
			}

			clonedJob.CopyJobBillingInformation(
				jobHeaderParent: this,
				skipTransactionInfo: true,
				overrideComment: true,
				resetGSTTaxDefault: false,
				copyExchangeRates: false,
				copyRateAudit: false);

			// automatically update charges' cost and sell exchange rates. Just for One Off Quote.
			clonedJob.UpdateExchangeRates(true);
		}

		#endregion

		#region Notes

		/// <summary>
		/// Call this in the GUI loading events so the business layer will start capturing
		/// the note added event. This method is fired from the GUI and not in the constructor
		/// of this object so as to not impact performance when each object is loaded in the grid,
		/// rather than a form. We only care about this event when the object is loaded in a form.
		/// </summary>
		public void HookupNoteAddedListener()
		{
			Notes.NoteAdded += new NoteAddedEventHandler(Notes_NoteAdded);
		}

		void Notes_NoteAdded(NoteAddedEventArgs args)
		{
			var note = args.NoteAdded;
			if (note.ST_Description == PredefinedNoteTypes.Instance.QuoteCoverPageText.Description && !note.ST_IsCustomDescription)
			{
				var textInRegistry = CoverPageText;
				note.ST_NoteText = textInRegistry.IsEmpty ? (ZString)Res.GetString("810b8434-40bc-484d-be1f-64b06b9b66f0", "You have not specified default cover page text in the registry.") : textInRegistry;
			}
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);

				var client = QuotationClientAddress.Organisation;
				if (client != null)
				{
					result.Add(client);
				}

				if (CurrentOneOffQuote != null)
				{
					var consignor = CurrentOneOffQuote.PickUpDocAddress.Organisation;
					var consignee = CurrentOneOffQuote.DeliveryDocAddress.Organisation;

					if (consignee != null)
					{
						result.Add(consignee);
					}
					if (consignor != null)
					{
						result.Add(consignor);
					}
				}

				return result.ToArray();
			}
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				var result = new StmNoteContexts();

				if (CurrentOneOffQuote != null)
				{
					var transportMode = CurrentOneOffQuote.TT_TransportMode;
					var containerMode = CurrentOneOffQuote.BookingContainerMode;

					result = StmNoteContextUtils.GetContextFromTransportModeImportExport(Core.Constants.GlobalModuleNamesConstants.Forwarding, "", transportMode, containerMode);
					result.Module |= base.NoteContextsForRelatedNotes.Module;
					result.Direction |= base.NoteContextsForRelatedNotes.Direction;
					result.FreightMode |= base.NoteContextsForRelatedNotes.FreightMode;

					if (CurrentOneOffQuote.IsImport())
					{
						result.Direction |= StmNoteContextDirection.I;
					}

					if (CurrentOneOffQuote.IsExport())
					{
						result.Direction |= StmNoteContextDirection.E;
					}

					if (CurrentOneOffQuote.IsDomestic())
					{
						result.Direction |= StmNoteContextDirection.D;
					}

					if (CurrentOneOffQuote.IsImport() || CurrentOneOffQuote.IsExport())
					{
						result.Direction |= StmNoteContextDirection.B;
					}
				}

				return result;
			}
		}

		#endregion

		#region Published Agents

		public OrgAddressCollection PublishedAirFreightAgents
		{
			get { return GetPublishedAgents(Core.Constants.RateMode.AIR); }
		}

		public OrgAddressCollection PublishedSeaFreightAgents
		{
			get { return GetPublishedAgents(Core.Constants.RateMode.SEA); }
		}

		OrgAddressCollection GetPublishedAgents(string transportMode)
		{
			var addresses = new OrgAddressCollection(Factory);

			if (transportMode == Core.Constants.RateMode.AIR)
			{
				var airCollection = EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection;

				foreach (RateEntry entry in airCollection)
				{
					AddPublishedAgents(addresses, transportMode, entry.Origin(), AgentDirectionList.Codes.Export, entry);
					AddPublishedAgents(addresses, transportMode, entry.Destination(), AgentDirectionList.Codes.Import, entry);
				}
			}
			else
			{
				var fclCollection = EntryCollections[RatingConstants.RateCategory.FCL].LoadedCollection;

				foreach (RateEntry entry in fclCollection)
				{
					AddPublishedAgents(addresses, transportMode, entry.Origin(), AgentDirectionList.Codes.Export, entry);
					AddPublishedAgents(addresses, transportMode, entry.Destination(), AgentDirectionList.Codes.Import, entry);
				}

				var lclCollection = EntryCollections[RatingConstants.RateCategory.LCL].LoadedCollection;

				foreach (RateEntry entry in lclCollection)
				{
					AddPublishedAgents(addresses, transportMode, entry.Origin(), AgentDirectionList.Codes.Export, entry);
					AddPublishedAgents(addresses, transportMode, entry.Destination(), AgentDirectionList.Codes.Import, entry);
				}
			}

			if (TH_OneTimeQuote)
			{
				if ((transportMode == Core.Constants.RateMode.AIR && CurrentOneOffQuote.IsAir) ||
					(transportMode == Core.Constants.RateMode.SEA && CurrentOneOffQuote.IsSea))
				{
					AddPublishedAgents(addresses, transportMode, CurrentOneOffQuote.ReceivalLocation, AgentDirectionList.Codes.Export);
					AddPublishedAgents(addresses, transportMode, CurrentOneOffQuote.DeliveryLocation, AgentDirectionList.Codes.Import);
				}
			}

			return addresses;
		}

		void AddPublishedAgents(OrgAddressCollection publishedAgents, ZString transportMode, ILocation location, ZString direction)
		{
			AddPublishedAgents(publishedAgents, transportMode, location, direction, null);
		}

		void AddPublishedAgents(OrgAddressCollection publishedAgents, ZString transportMode, ILocation location, ZString direction, RateEntry entry)
		{
			OrgAddress address = null;

			if (entry != null && entry.AgentOverride != null)
			{
				address = entry.AgentOverride.MainAddress;
			}
			else if (location != null && GlbBranch.CurrentBranch.GB_RL_NKHomePort != location.Code)
			{
				var uNLOCO = location as RefUNLOCO;
				if (uNLOCO != null && (transportMode == Core.Constants.RateMode.AIR || transportMode == Core.Constants.RateMode.SEA))
				{
					address = uNLOCO.GetPublishedAgent(transportMode, direction);
				}
			}

			if (address != null && !publishedAgents.Contains(address))
			{
				publishedAgents.Add(address);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				OneOffQuote.RemoveAndDeleteAll();
				SelectedPages.RemoveAndDeleteAll();
				base.Delete();
				WorkflowItems.RemoveAndDeleteAll();
				ViewRelatedActivityPivot.DeleteAllPivots(this);
			}
		}

		#endregion

		#region One Off Quotation

		public void InitialiseOneOffQuotation()
		{
			if (fOneOffQuotes == null)
			{
				fOneOffQuotes = new RateOneOffShipmentCollection(this);
				RegisterEditableChildObject(fOneOffQuotes);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		public bool ValidOneOffQuoteChargesExist
		{
			get
			{
				var charges = LoadCharges();

				return HasFreightCharges(charges);
			}
		}

		bool HasFreightCharges(JobCharge[] charges)
		{
			var result = false;
			if (charges.Length > 0)
			{
				result = true;

				if (CurrentOneOffQuote != null)
				{
					var prepaidCollect = CurrentOneOffQuote.RatingAdapter.PaymentTerm.GetPrepaidCollect(CostSell.Revenue, ChargeCodeGroupList.Codes.Freight);
					var freightChargesApplicable = CurrentOneOffQuote.IsImport() && prepaidCollect == CoreConsts.PaymentType.Collect;

					if (freightChargesApplicable && !charges.Any(x => x.JR_AC == Env.Registry.FreightChargeCode))
					{
						result = false;
					}
				}
			}

			return result;
		}

		public bool HasNonZeroSellAmt
		{
			get
			{
				var charges = LoadCharges();

				return HasFreightCharges(charges) && charges.Any(x => x.JR_OSSellAmt != 0);
			}
		}

		JobCharge[] LoadCharges()
		{
			var job = new JobHeader.Loader(this).Load();

			return job != null ? Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, job.PK)) : Array.Empty<JobCharge>();
		}

		[ChildEditable(false)]
		// Use different collection name so this property will not be accessed and initialized during copy - data will be loaded directly from factory/db
		[UniversalCopyCollectionEntity(RateOneOffShipmentSchema.Constants.TableName, RateOneOffShipmentSchema.Constants.TT_TH, OverrideCollectionName = "OneOffShipment")]
		public RateOneOffShipmentCollection OneOffQuote
		{
			get
			{
				InitialiseOneOffQuotation();
				if (TH_OneTimeQuote && (fOneOffQuotes.Count == 0 || fOneOffQuotes[0].IsDeleted))
				{
					var oneOffShipment = Factory.LoadTop1<RateOneOffShipment>(new ZQuery(RateOneOffShipmentSchema.TT_TH, PK));
					if (oneOffShipment != null && !oneOffShipment.IsDeleted)
					{
						fOneOffQuotes.Add(oneOffShipment);
					}
					else
					{
						var oneOff = fOneOffQuotes.AddNew();
						oneOff.TT_TH = PK;
					}
				}
				return fOneOffQuotes;
			}
		}
		RateOneOffShipmentCollection fOneOffQuotes;

		public RateOneOffShipment CurrentOneOffQuote
		{
			get { return OneOffQuote.Count > 0 ? OneOffQuote[0] : null; }
		}

		public override ZBool TH_OneTimeQuote
		{
			get { return base.TH_OneTimeQuote; }
			set
			{
				base.TH_OneTimeQuote = value;

				if (value)
				{
					InitialiseOneOffQuotation();
					var oneOff = fOneOffQuotes.AddNew();
					oneOff.TT_TH = PK;
				}

				SetStartEndDateForOneOffQuotes();
				SelectedPages.SetupPricingPageSelectedPage(value);
			}
		}

		void SetStartEndDateForOneOffQuotes()
		{
			if (TH_OneTimeQuote)
			{
				foreach (var rateEntry in AllEntries)
				{
					rateEntry.TI_RateStartDate = TH_QuoteDate;
					rateEntry.TI_RateEndDate = TH_QuoteEndDate;
				}
			}
		}

		#endregion

		#region Quote Format Entry Collection

		QuoteFormatEntryCollection fQuoteFormatEntries;

		public QuoteFormatEntryCollection QuoteFormatEntries
		{
			get
			{
				if (fQuoteFormatEntries == null)
				{
					fQuoteFormatEntries = new QuoteFormatEntryCollection(Factory, this);
				}
				return fQuoteFormatEntries;
			}
		}

		#endregion

		#region Document Attachments

		[ChildEditable(true)]
		public RateAttachmentCollection SelectedPages
		{
			get
			{
				if (fSelectedPages == null)
				{
					fSelectedPages = new RateAttachmentCollection(this);
					RegisterEditableChildObject(fSelectedPages);
					fSelectedPages.Load();
				}
				return fSelectedPages;
			}
		}
		RateAttachmentCollection fSelectedPages;

		RateAttachmentSetCollection fAvailablePages;

		public RateAttachmentSetCollection AvailablePages
		{
			get
			{
				if (fAvailablePages == null)
				{
					fAvailablePages = new RateAttachmentSetCollection(this, Factory);
					fAvailablePages.SetReadOnlyIncludingChildren(true);
					fAvailablePages.Load();
				}
				return fAvailablePages;
			}
		}

		#region Quote Attachment Notification Event Handling

		public event QuoteAttachmentNotificationsEventHandler QuoteAttachmentsNotifications;

		internal void OnQuoteAttachmentNotifications(QuoteAttachmentNotificationsEventArgs args)
		{
			if (QuoteAttachmentsNotifications != null)
			{
				QuoteAttachmentsNotifications(this, args);
			}
		}

		#endregion

		#endregion

		#region Minimum Cost Markup

		public bool HasMinimumCostMarkupNotMetWarning()
		{
			foreach (var entry in AllEntries)
			{
				foreach (var rateLine in entry.RateLines.Cast<RateLine>().Where(r => r.UsesCostBasedCalculator()))
				{
					foreach (var item in rateLine.RateLineItems.Cast<RateLineItem>())
					{
						item.Validation.ValidateTM_Value();
						var warnings = item.TM_ValueInfo.GetWarnings().Select(x => x.Message);

						if (warnings.Any(warning => warning.StartsWith(ErrorMessages.MinimumCostMarkupNotMetPart1, StringComparison.OrdinalIgnoreCase)
							&& warning.EndsWith(ErrorMessages.MinimumCostMarkupNotMetPart2, StringComparison.OrdinalIgnoreCase)))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		#endregion

		#region Cover Page Text

		public ZString CoverPageText
		{
			get
			{
				if (IsNewClient)
				{
					if (TH_OneTimeQuote)
					{
						return Env.Registry.Rating.QuoteCoverPageTextOneOffNew;
					}
					else
					{
						return Env.Registry.Rating.QuoteCoverPageTextNew;
					}
				}
				else
				{
					if (TH_OneTimeQuote)
					{
						return Env.Registry.Rating.QuoteCoverPageTextOneOffExisting;
					}
					else
					{
						return Env.Registry.Rating.QuoteCoverPageTextExisting;
					}
				}
			}
		}

		bool IsNewClient
		{
			get
			{
				if (!IsNewClientSet)
				{
					fIsNewClient = !IsAnyOtherHeaderWithSameOrgAndType();
					IsNewClientSet = true;
				}

				return fIsNewClient;
			}
		}

		bool fIsNewClient;
		bool IsNewClientSet;

		internal override bool IsAnyOtherHeaderWithSameOrgAndType()
		{
			var addressCode = DocAddressTypes.GetCode(Factory, DocAddressType.QuotationClientAddress);

			var docAddressesFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressesFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressesFilter.AddToFilter(OrgAddressSchema.OA_OH, TH_OH);
			docAddressesFilter.AddSubQuery(orgAddressesFilter, JoinCondition.And);
			docAddressesFilter.AddToFilter(JobDocAddress.GetFilter(addressCode, RatingHeaderSchema.Constants.Prefix, 0), JoinCondition.And);

			var filter = new ZDBOnlyQuery(typeof(Quote));
			filter.AddSubQuery(RatingHeaderSchema.PK, docAddressesFilter, JoinCondition.And);

			filter.AddToFilter(RatingHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
			filter.AddToFilter(RatingHeaderSchema.TH_GC, TH_GC);
			filter.AddToFilter(RatingHeaderSchema.TH_RateType, TH_RateType);

			return Factory.ExistsInDatabase(RatingHeader.Schema.TableName, filter);
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			var newQuote = (Quote)this.CopyIncludingChildren();

			if (SameClientCopy)
			{
				newQuote.TH_OH = TH_OH;
				newQuote.RelatedParentActivityPivotCollection.AddActivity(this);
			}
			else
			{
				newQuote.TH_OH = ZGuid.Empty;
			}

			newQuote.SetDefaultSignatures();

			if (AmendmentCopy)
			{
				newQuote.TH_QuoteNumber = GetNewQuoteNumberForAmendment();
				newQuote.CancelPreviousAmendment = new CancelPreviousAmendmentEventHandler(CancelAmendmentHandler);
			}
			else
			{
				newQuote.TH_QuoteNumber = ZString.Empty;
			}

			var followUpDays = RatingDataRegistry.Instance.QuoteFollowUpDays.Value;
			if (followUpDays > 0 && followUpDays < 366)
			{
				newQuote.TH_FollowUpDate = ZDateTime.Today.AddDays(followUpDays);
			}
			else if (followUpDays == 0)
			{
				newQuote.TH_FollowUpDate = ZDateTime.Empty;
			}

			return newQuote;
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new QuoteDocumentSupporter(this); }
		}

		#region Document Supporter

		public class QuoteDocumentSupporter : RatingHeaderDocumentSupporter
		{
			public QuoteDocumentSupporter(Quote quote)
				: base(quote)
			{
			}

			protected override BusinessObject DeliveryObject => (BusinessObject)(Quote.ParentQuotedBooking);

			public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contact, DocumentDirection direction)
			{
				return Quote.TH_OneTimeQuote && Quote.QuotationClientAddress.E2_AddressOverride ? Quote.QuotationClientAddress : null;
			}

			public override PrintTask BuildPrintTaskCore(IStmMenuItem menuItem)
			{
				var documentCommand = menuItem as DocumentCommand;
				if (documentCommand != null)
				{
					return new DocumentPrintSet(documentCommand, null);
				}
				else
				{
					return base.BuildPrintTaskCore(menuItem);
				}
			}

			Quote Quote
			{
				get { return (Quote)BusinessObject; }
			}

#if DEBUG
			public DeliveryInstructions DeliveryInstructionsForTest;
#endif
			public override void RunTask(PrintTask task)
			{
				if (task != null && Quote.DocumentPrintMode == QuotationDocumentMode.Final)
				{
					var destination = task.RunWithPartialInstructions(AllowedDeliveryOptions.All, NewDeliveryInstructions(task), Env.Security.None);

					if (destination != DeliveryInstructionDestination.UserCancelled)
					{
						LockAndSaveQuote();
					}
				}
				else if (task != null && Quote.DocumentPrintMode == QuotationDocumentMode.Draft)
				{
					var deliveryInstructions = NewDeliveryInstructions(task);
					deliveryInstructions.IsDraft = true;
					deliveryInstructions.IsDraft_ReadOnly = true;
					task.RunWithPartialInstructions(AllowedDeliveryOptions.All, deliveryInstructions, Env.Security.None);
#if DEBUG
					DeliveryInstructionsForTest = deliveryInstructions;
#endif
				}
				else
				{
					base.RunTask(task);
				}
			}

#if DEBUG
			internal
#endif
			void LockAndSaveQuote()
			{
				Quote.Logs.AddNew(Events.QuotationFinalisedPrinted);
				Quote.TH_IsLocked = true;
				ZExceptionReporting.ProcessWithSaveExceptionHandling(Quote.Factory.Save, null, true);
				Quote.ReadOnly = true;
				Quote.SetReadOnlyIncludingChildren(true);
			}

			void SetPrintModeOnDocument()
			{
				if (Quote.TH_IsLocked)
				{
					Quote.DocumentPrintMode = QuotationDocumentMode.Reprint;
				}
				else
				{
					var isApproved = !Quote.IsInternalApprovalRequired || Quote.IsApproved;
					var setFinal = isApproved
						? (bool)Quote.OnSetFinalMode()
						: Quote.InternalApproveQuoteWhenFinalizing();

					if (setFinal)
					{
						Quote.DocumentPrintMode = QuotationDocumentMode.Final;
					}
					else
					{
						Quote.DocumentPrintMode = QuotationDocumentMode.Draft;
					}
				}
			}

			protected override bool PerformChecks(IStmMenuItem menuItem)
			{
				if (!base.PerformChecks(menuItem))
				{
					return false;
				}

				if (Quote.HasMinimumCostMarkupNotMetWarning() && !Quote.OnMinimumCostMarkupNotMet())
				{
					return false;
				}

				if (Quote.TH_OneTimeQuote && !Quote.ValidOneOffQuoteChargesExist && !Quote.OnSpotQuoteChargesIncorrect())
				{
					return false;
				}

				if (menuItem == null || menuItem.SU_MenuName == Core.Constants.MenuNameConstantsForPrinting.QuotationPack || menuItem.SU_DraftOption == DraftOptionsList.Codes.Both)
				{
					if (!Quote.TH_OneTimeQuote)
					{
						var pages = new PricingPageCollection(Quote);
						pages.LoadStandard();

						if (pages.Count == 0)
						{
							Quote.OnNoTradeLanesToPrint();
							return false;
						}
					}

					SetPrintModeOnDocument();
				}

				return true;
			}
		}

		#endregion

		#region One Off Quote Specific

		public QuoteEntry FirstMatchingEntryForOneOffQuote
		{
			get
			{
				// Even though there may be more than 1 relevant matching entry (FCL) for one-off quotes,
				// the Doc Wrapper only needs the first one to get some basic information.
				// Actual charges are taken from the collection returned from AutoRating the one-off quote

				QuoteEntry result = null;

				var dummyFactory = new BusinessObjectFactory();
				if (CurrentOneOffQuote != null)
				{
					var oneOffDetails = CurrentOneOffQuote;
					QuoteEntry matched = null;

					var criteria = new RatingCriteria(oneOffDetails.RatingAdapter, dummyFactory);
					var rateFilter = new FreightRateEntryFilter(criteria, false, dummyFactory, new Enterprise.Integration.DummyLogger());
					matched = rateFilter.FindMatchingRateEntryForOneOffQuote(this);
					if (matched != null)
					{
						result = matched;
					}
				}

				if (result == null && QuoteFormatEntries.Count > 0)
				{
					result = QuoteFormatEntries[0];
				}

				if (result == null && CurrentOneOffQuote != null)
				{
					var dummyFactory1 = dummyFactory;
					result = dummyFactory1.New<QuoteEntry>();
					var mode = (CurrentOneOffQuote.RatingAdapter).FreightMode;
					if (mode == FreightMode.COU && CurrentOneOffQuote.Mode == Core.Constants.RateMode.COU)
					{
						result.TI_RateCategory = RatingConstants.RateCategory.AIR;
						result.TI_Mode = Core.Constants.RateMode.COU;
					}
					else if ((mode & FreightMode.AIR) != 0)
					{
						result.TI_RateCategory = RatingConstants.RateCategory.AIR;
						if ((mode & FreightMode.Containerised) != 0)
						{
							result.TI_Mode = Core.Constants.RateMode.ULD;
						}
						else
						{
							result.TI_Mode = Core.Constants.RateMode.LSE;
						}
					}
					else
					{
						if ((mode & FreightMode.NonContainerised) != 0)
						{
							result.TI_RateCategory = RatingConstants.RateCategory.LCL;
							result.TI_Mode = CurrentOneOffQuote.Mode;
						}
						else
						{
							result.TI_RateCategory = RatingConstants.RateCategory.FCL;
							if ((mode & FreightMode.ROA) != 0)
							{
								result.TI_Mode = Core.Constants.RateMode.ROA;
							}
							else if ((mode & FreightMode.RAI) != 0)
							{
								result.TI_Mode = Core.Constants.RateMode.RAI;
							}
							else
							{
								result.TI_Mode = Core.Constants.RateMode.SEA;
							}
						}
					}

					result.TI_OriginLRC = CurrentOneOffQuote.TT_RL_NKReceivalLocation;
					result.TI_DestinationLRC = CurrentOneOffQuote.TT_RL_NKDeliveryLocation;
					result.TI_RS_NKServiceLevel_NI = CurrentOneOffQuote.TT_RS_NKServiceLevel;
					result.TI_RH_NKCommodityCode = CurrentOneOffQuote.TT_RH_NKCommodity;
					result.RateLines.RemoveAndDeleteAll();
					result.TI_TH = PK;
					result.Parent = this;
				}

				return result;
			}
		}

		#endregion

		#region Events

		public class SetFinalModeArgs : EventArgs
		{
			public ZBool Result { get; set; }
		}

		public event EventHandler SetFinalMode;

		ZBool OnSetFinalMode()
		{
			if (SetFinalMode != null)
			{
				var args = new SetFinalModeArgs();
				SetFinalMode(this, args);
				return args.Result;
			}
			else
			{
				return ZBool.False;
			}
		}

		public event EventHandler NoTradeLanesToPrint;

		void OnNoTradeLanesToPrint()
		{
			if (NoTradeLanesToPrint != null)
			{
				NoTradeLanesToPrint(this, EventArgs.Empty);
			}
		}

		public event CancelEventHandler SpotQuoteChargesIncorrect;

		bool OnSpotQuoteChargesIncorrect()
		{
			if (SpotQuoteChargesIncorrect != null)
			{
				var args = new CancelEventArgs();
				SpotQuoteChargesIncorrect(this, args);
				return !args.Cancel;
			}

			return false;
		}

		public delegate bool MinimumCostMarkupNotMetEventHandler();
		public event MinimumCostMarkupNotMetEventHandler MinimumCostMarkupNotMet;

		bool OnMinimumCostMarkupNotMet()
		{
			return MinimumCostMarkupNotMet == null || MinimumCostMarkupNotMet();
		}

		#endregion

		#region TrailingPageImages

		public List<Image> TrailingPageImages
		{
			get { return trailingPageImages ?? (trailingPageImages = GetTrailingPageImagesArray()); }
		}

		List<Image> GetTrailingPageImagesArray()
		{
			var images = new List<Image>();
			foreach (RateAttachment attachment in SelectedPages)
			{
				if (attachment.IsImage && attachment.Image != null)
				{
					images.Add(attachment.Image);
				}
			}

			return images;
		}

		List<Image> trailingPageImages;

		#endregion

		#endregion

		#region Create Quote From Trade Lane

		/// <summary>
		/// Populates the quote's information based on the given Trade Lane
		/// </summary>
		/// <param name="tradeDetail">The OrgTradeDetail from which the quote details should be retrieved</param>
		public bool ImportTradeDetailData(OrgTradeDetail tradeDetail)
		{
			Argument.NotNull(tradeDetail, "tradeDetail");

			var importer = new QuoteTradeDetailImporter(this);
			return importer.Import(tradeDetail);
		}

		#endregion

		#region Calendar Reminders

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				FailedSaveQuoteNumber = TH_QuoteNumber;
				TH_QuoteNumber = ZString.Empty;
			}

			base.OnSaved(saveSucceeded);
			if (saveSucceeded && ShouldCreateReminder)
			{
				new QuoteReminder(this).CreateAppointment();
			}

			if (saveSucceeded)
			{
				OriginalTH_FollowUpDate = TH_FollowUpDate;
			}

			OnQuoteSaved?.Invoke(this, new SavedEventArgs(saveSucceeded));
		}

		ZString FailedSaveQuoteNumber { get; set; }

		public event EventHandler OnQuoteSaved;

		public class SavedEventArgs : EventArgs
		{
			public SavedEventArgs(bool hasSaveSucceeded) : base()
			{
				HasSaveSucceeded = hasSaveSucceeded;
			}

			public bool HasSaveSucceeded;
		}

		internal bool ShouldCreateReminder
		{
			get { return !TH_FollowUpDate.IsEmpty && Header != null && TH_FollowUpDate != OriginalTH_FollowUpDate; }
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new QuoteFetchStrategy(this);
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IWorkflowProvider Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, TH_OH, ZGuid.Empty);
			return result;
		}

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

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new QuotationProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}
		QuotationProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.QuotationWorkflowDescriptorCode; }
		}

		#endregion

		#region IAutoAdminLogTarget Members

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return IsAutoLogged; }
		}

		#endregion

		#region IUpdateAuditFields Members

		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges => true;

		#endregion

		public override OrgHeader Header
		{
			get { return QuotationClientAddress.Organisation; }
		}

		#region QuotationClientAddress

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "E2_ParentID", DisableCopyMethodLink = true, MakeRelatedEntitySavedByFactoryMethod = "MakePersistentEvenIfEmpty")]
		public JobDocAddress QuotationClientAddress
		{
			get
			{
				if (fQuotationClientAddress == null || fQuotationClientAddress.IsDeleted)
				{
					UnRegisterListChangedCalledRefreshBinding(fQuotationClientAddress);
					fQuotationClientAddress = GetNewQuotationClientAddress();
					fQuotationClientAddress.E2_OA_AddressInfo.ValueChanged += new EventHandler(OrganisationPKInfo_ValueChanged);
					fQuotationClientAddress.OrganisationPKInfo.ValueChanged += new EventHandler(OrganisationPKInfo_ValueChanged);
					RegisterListChangedCalledRefreshBinding(fQuotationClientAddress);
				}
				return fQuotationClientAddress;
			}
		}
		JobDocAddress fQuotationClientAddress;

		void OrganisationPKInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Header != null)
			{
				SetCFXDefaultValues();
				SetDefaultSignatures();
			}
		}

		JobDocAddress GetNewQuotationClientAddress()
		{
			return DocAddresses.FindOrCreateWithRequirement(ClientDocAddressRequirement);
		}

		JobDocAddressRequirement ClientDocAddressRequirement
		{
			get { return fClientDocAddressRequirement ?? (fClientDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.QuotationClientAddress, AddressType.ARM)); }
		}

		JobDocAddressRequirement fClientDocAddressRequirement;

		#region IDocAddresses Members

		#region DocAddresses

		[ChildEditable(true)]
		public virtual JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}

		JobDocAddressDependentCollection fDocAddresses;

		#endregion

		#region SupportedAddressTypes

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return GetSupportedAddressTypes(); }
		}

		protected DocAddressType[] GetSupportedAddressTypes()
		{
			return new[] { DocAddressType.QuotationClientAddress };
		}

		#endregion

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.QuotationClientAddress:
					return new JobDocAddressRequirement();
				default:
					return null;
			}
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
			var job = new JobHeader.Loader(this).Load();
			if (job is IAutoRatingAccountingUtils autoRatingAccountingUtils)
			{
				autoRatingAccountingUtils?.UpdateChargesDescription();
			}
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.MaintainShipmentShipments;
		}

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new QuoteDocAddressValidation(addressToValidate);
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return
					QuoteStatus != QuoteStatusOptions.Accepted &&
					QuoteStatus != QuoteStatusOptions.ClientAccepted;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (QuoteStatus == QuoteStatusOptions.Accepted)
				{
					return ResString.GetMultilingualString("e7f43634-549a-4bd6-b1b5-0cbfcdfae107", "The selected quote has already been accepted and cannot be deleted.");
				}
				else if (QuoteStatus == QuoteStatusOptions.ClientAccepted)
				{
					return ResString.GetMultilingualString("0fadc53f-0a1d-4671-a1ab-a5b1b31c480a", "The selected quote has already been accepted by the client and cannot be deleted.");
				}

				return null;
			}
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.OnJobCreating(JobHeader job) { }

		void IJobHeaderParent.OnJobCreated(JobHeader job) { }

		void IJobHeaderParent.OnJobDeleting(JobHeader job) { }

		void IJobHeaderParent.OnJobDeleted(JobHeader job) { }

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			if (TH_QuoteNumber.IsEmpty)
			{
				SetQuoteNumber();
			}
		}

		string IJobNumber.JobNumber
		{
			get { return TH_QuoteNumber; }
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return true; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.Quotations; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return Header; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return TH_OHInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get
			{
				var result = string.Join("; ", new ZString[] { Res.GetString("e36f3e1c-b243-4fb3-a73f-7ff8782a3f12", "Formal Quotation"), QuoteStatus }.Where(x => !x.IsEmpty));
				var companyCountryCode = CompanyCountryCode;
				if (!companyCountryCode.IsEmpty)
				{
					result = string.Format(CultureInfo.InvariantCulture, "({0}) {1}", companyCountryCode, result);
				}

				return result;
			}
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		ZString CompanyCountryCode
		{
			get
			{
				var company = Company;
				if (company == null)
				{
					return ZString.Empty;
				}

				return company.GC_RN_NKCountryCode;
			}
		}

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region ISalesRelationActivity Members

		public ISalesRelationModel SalesRelationModel
		{
			get
			{
				if (salesRelationModel == null)
				{
					salesRelationModel = new SalesRelationModel(this);
					RegisterEditableChildObject(salesRelationModel);
				}

				return salesRelationModel;
			}
		}

		SalesRelationModel salesRelationModel;

		ZString ISalesRelationActivity.ActivityNotePropertyName
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (parentActivity.Client != null)
			{
				TH_OH = parentActivity.Client.PK;
			}

			return true;
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		public IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders
		{
			get
			{
				var quotedBookingWorkflowProvider = ParentQuotedBooking as IWorkflowProviderCore;
				if (quotedBookingWorkflowProvider != null)
				{
					return new IWorkflowProviderCore[1] { quotedBookingWorkflowProvider };
				}

				return Array.Empty<IWorkflowProviderCore>();
			}
		}

		public IGlbCompany JobHeaderCompany => Company;

		public IQuotedBooking ParentQuotedBooking { get; private set; }

		public bool HasBindBooking
		{
			get
			{
				if (ParentQuotedBooking == null)
				{
					var bookingByQuote = Factory.LoadTop1<CommonShipment>(new ZQuery(JobShipmentSchema.JS_TH_OneTimeQuote, PK));
					return bookingByQuote != null;
				}
				return ParentQuotedBooking.ForwardingShipment != null;
			}
		}

		#endregion

		#region ICustomFieldProvide
		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var parent = this;
				var properties = new UserDefinedPropertyCollection(parent).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, parent, properties);
			}
			return customBusinessObject;
		}
		CustomBusinessObject customBusinessObject;
		#endregion

		public void SetParentQuotedBooking(IQuotedBooking quotedBooking)
		{
			ParentQuotedBooking = quotedBooking;
		}

		protected override Logs GetNewLogs()
		{
			return new QuoteLogs(this);
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			QuotationClientAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		}

#endif
	}
}
