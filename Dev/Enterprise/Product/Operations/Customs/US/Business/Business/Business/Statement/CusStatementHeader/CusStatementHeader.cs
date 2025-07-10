using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[CodeProperty(CusStatementHeader.Schema.B2_StatementNumber), DescriptionProperty(CusStatementHeader.Schema.B2_StatementNumber)]
	[SingleObjectAroundARow]
	[UniversalDataContext(UniversalDataBuss.Integration.DataContextType.USStatement)]
	public class CusStatementHeader :
		BaseCusStatementHeader,
		IDocumentSupportable,
		IDocManagerSupport,
		IHaveRequiredDocuments,
		IJobNumberForWorkflow,
		IJobNumber,
		IJobHeaderParent,
		IEDocsProvider,
		Integration.Customs.US.ICusStatementHeader,
		IMessageFailStatusManager,
		IMessageAttachee,
		IStatementForProvider
	{
		#region Constants

		public static class Constants
		{
			internal const string TotalAmountDue = "TotalAmountDue";
		}

		#endregion

		#region Loader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : Customs.Business.AutoCusStatementHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusStatementHeader LoadWithStatementNumber(ZString entryFilerCode, ZString statementNumber, ZGuid companyPK)
			{
				var cusStatementHeaderFilter = CreateNewCusStatementHeaderFilter(entryFilerCode, statementNumber, companyPK);
				return Factory.LoadTop1<CusStatementHeader>(cusStatementHeaderFilter);
			}

			public CusStatementHeader Load(ZString entryFilerCode, ZString entryNumber, ZGuid companyPK)
			{
				var line = new CusStatementLine.Loader(Factory).GetStatementLine(entryFilerCode, entryNumber, SQLComparisonOperator.NotEqual, companyPK);
				return line != null ? line.StatementHeader : null;
			}

			public bool HasDeletedStatement(ZString entryFilerCode, ZString entryNumber, ZGuid companyPK)
			{
				return new CusStatementLine.Loader(Factory).GetStatementLine(entryFilerCode, entryNumber, SQLComparisonOperator.Equal, companyPK) != null;
			}

			public ZQuery CreateNewCusStatementHeaderFilter(ZString entryFilerCode, ZString statementNumber, ZGuid companyPK)
			{
				var cusStatementHeaderFilter = new ZQuery(CusStatementHeaderSchema.B2_EntryFilerCode, entryFilerCode);
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
				cusStatementHeaderFilter.AddToFilter(CusStatementHeaderSchema.B2_GC, companyPK);
				cusStatementHeaderFilter.OrderBy = CusStatementHeaderSchema.Constants.B2_SystemCreateTimeUtc + OrderByClause.Descending;
				return cusStatementHeaderFilter;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusStatementHeader);
			}
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusStatementHeader statementHeader)
				: base(statementHeader)
			{
			}

			CusStatementHeader StatementHeader
			{
				get { return (CusStatementHeader)BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				if (StatementHeader.IsMonthlyStatement)
				{
					Factory.AddFetchHint(CusStatementHeaderSchema.B2_B2_PeriodicStatement, StatementHeader.PK);
				}
				else
				{
					Factory.AddFetchHint(CusStatementLineSchema.B3_B2, StatementHeader.PK);
				}
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				Factory.AddFetchHint(StmALogSchema.SL_Parent, StatementHeader.PK);
				foreach (TableColumn tableColumn in columns)
				{
					switch (tableColumn.ColumnName)
					{
						case CusStatementHeader.Schema.ImporterName:
							Factory.AddFetchHint(OrgHeaderSchema.PK, StatementHeader.B2_OH_Importer);
							break;

						case CusStatementHeader.Schema.TotalAmountDue:
						case CusStatementHeader.Schema.FinalTotalAmountDue:

							if (StatementHeader.IsMonthlyStatement)
							{
								Factory.AddFetchHint(CusStatementHeaderSchema.B2_B2_PeriodicStatement, StatementHeader.PK);
							}
							else
							{
								Factory.AddFetchHint(CusStatementLineSchema.B3_B2, StatementHeader.PK);
							}
							Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, StatementHeader.PK);
							break;
					}
				}
			}
		}

		#endregion

		public new class Schema : AutoCusStatementHeader.Schema
		{
			public const string TotalDeferredTax = "TotalDeferredTax";
			public const string TotalAmountDue = "TotalAmountDue";
			public const string FinalTotalAmountDue = "FinalTotalAmountDue";
			public const string FilterStatementLinesBy = "FilterStatementLinesBy";
			public const string PostARInvoices = "PostARInvoices";
			public const string PostAPInvoices = "PostAPInvoices";
			public const string MakePayment = "MakePayment";
			public const string APFullyPaid = "APFullyPaid";
			public const string APPostedAmount = "APPostedAmount";
			public const string APUnPostedAmount = "APUnPostedAmount";
			public const string APTotalAmount = "APTotalAmount";
			public const string ARPostedAmount = "ARPostedAmount";
			public const string ARUnPostedAmount = "ARUnPostedAmount";
			public const string ARTotalAmount = "ARTotalAmount";
			public const string DifferenceBetweenARInvoiceAndCustomsAmount = "DifferenceBetweenARInvoiceAndCustomsAmount";
			public const string DifferenceBetweenAPInvoiceAndCustomsAmount = "DifferenceBetweenAPInvoiceAndCustomsAmount";
			public const string PaymentStatusDescription = "PaymentStatusDescription";
			public const string PaymentTypeDescription = "PaymentTypeDescription";
			public const string ManagedACH = "ManagedACH";
			public const string ImporterName = "ImporterName";
			public const string ApprovalActionAuthorised = "ApprovalActionAuthorised";
		}

		public CusStatementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Objects Override

		public override bool CanDelete
		{
			get { return false; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("FE5DFA36-D121-4629-A450-50E03990A266", "Statements are created from Customs messages. You cannot delete them this way.");
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.ImportersList))]
		public override ZGuid B2_OH_Importer
		{
			get { return base.B2_OH_Importer; }
			set { base.B2_OH_Importer = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentTypeList))]
		public override ZString B2_PaymentType
		{
			get { return base.B2_PaymentType; }
			set
			{
				base.B2_PaymentType = value;

				if (B2_PaymentParty.IsEmpty)
				{
					B2_PaymentParty = PaymentTypeList.IsPaidByBroker(B2_PaymentType) ? PaymentPartyList.Codes.Broker : PaymentPartyList.Codes.Importer;
				}
			}
		}

		public ZString PaymentTypeDescription
		{
			get { return Lookups.PaymentTypeList.GetDescriptionFromCode(B2_PaymentType); }
		}

		public ZPropertyInfo PaymentTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.PaymentTypeDescription); }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.StatementHeaderStatusList))]
		public override ZString B2_Status
		{
			get { return base.B2_Status; }
			set
			{
				var oldValue = B2_Status;

				base.B2_Status = value;

				if (oldValue != B2_Status)
				{
					if (B2_Status == StatementHeaderStatusList.Codes.Final)
					{
						B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
					}

					//from empty to P, then F
					if (B2_Status != StatementHeaderStatusList.Codes.Deleted)
					{
						MarkAccToBeIntegrated(CusStatementHeaderSchema.B2_Status.Name);
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentStatusList))]
		public override ZString B2_PaymentStatus
		{
			get { return base.B2_PaymentStatus; }
			set
			{
				var oldValue = B2_PaymentStatus;
				base.B2_PaymentStatus = value;

				if ((B2_PaymentStatus == PaymentStatusList.Codes.PaymentFailed && B2_PaymentAuthorizationDate.IsEmpty) ||
					B2_PaymentStatus == PaymentStatusList.Codes.PaymentAuthorizationDeleted ||
					B2_PaymentStatus.IsEmpty)
				{
					B2_PaymentParty = ZString.Empty;
					B2_AccountNo = ZString.Empty;
					B2_PaymentAuthorizationDate = ZDateTime.Empty;
				}

				LogForPaymentStatusChanged(oldValue);
			}
		}

		void LogForPaymentStatusChanged(ZString oldValue)
		{
			var logParents = GetLogParentsForPaymentStatusChanged();

			if (oldValue != PaymentStatusList.Codes.PaymentAuthorizationAccepted && B2_PaymentStatus == PaymentStatusList.Codes.PaymentAuthorizationAccepted)
			{
				MarkAccToBeIntegrated(CusStatementHeaderSchema.B2_PaymentStatus.Name);

				foreach (var logParent in logParents.ToArray())
				{
					logParent?.Logs.AddNew(AutoEvents.Authorised, PaymentAuthorizationAcceptedReference);
				}
			}

			if (oldValue != PaymentStatusList.Codes.PaymentAuthorizationDeleted && B2_PaymentStatus == PaymentStatusList.Codes.PaymentAuthorizationDeleted)
			{
				var query = GetPaymentAuthorizationAcceptedQuery();

				foreach (var logParent in logParents.ToArray())
				{
					var logs = logParent?.Logs.Find(query);

					if (logs != null && logs.Length > 0)
					{
						logs.ForEach(log => log.Cancel());
					}
				}
			}
		}

		IEnumerable<IStmALogParent> GetLogParentsForPaymentStatusChanged()
		{
			yield return this;

			if (IsDailyStatement)
			{
				foreach (var statementLine in ActiveLines.Cast<CusStatementLine>())
				{
					var statementLineDeclaration = statementLine.Declaration;

					var declaration = statementLineDeclaration as JobDeclaration;
					if (declaration != null)
					{
						yield return declaration;
					}

					var reconDeclaration = statementLineDeclaration as ReconDeclaration;
					if (reconDeclaration != null)
					{
						yield return reconDeclaration.ReconWrappedJobDeclaration;
					}
				}
			}
		}

		public Tuple<ZString, ZString> GetCheckIfValidToSendAuthorizationOrPaymentMessage()
		{
			Tuple<ZString, ZString> result = null;

			var isPaymentPartyBroker = this.IsPaymentPartyBroker;
			if (isPaymentPartyBroker && !Env.Security.USCustomsImportStatementSendAuthMsgBroker.IsAllowed)
			{
				result = new Tuple<ZString, ZString>(Env.Security.GetErrorMessageForNotAllowed(Env.Security.USCustomsImportStatementSendAuthMsgBroker), Env.Security.USCustomsImportStatementSendAuthMsgBroker.HumanReadableName);
			}
			else if (!isPaymentPartyBroker && !Env.Security.USCustomsImportStatementSendAuthMsgImporter.IsAllowed)
			{
				result = new Tuple<ZString, ZString>(Env.Security.GetErrorMessageForNotAllowed(Env.Security.USCustomsImportStatementSendAuthMsgImporter), Env.Security.USCustomsImportStatementSendAuthMsgImporter.HumanReadableName);
			}
			else
			{
				var companyPK = this.Company != null ? this.Company.PK : GlbCompany.CurrentCompany.PK;
				if (USCustomsDataRegistry.Instance.RequireApprovalPriorAuthorizingStatement.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty) &&
					!this.ApprovalActionAuthorised)
				{
					result = new Tuple<ZString, ZString>(NotAuthorised, NotAuthorisedCaption);
				}
			}
			return result;
		}
		internal const string NotAuthorised = "Permission to 'Send Payment Authorization' needs to be granted via Actions -> Grant Authorization Permission prior to sending.";
		internal const string NotAuthorisedCaption = "Statement Not Authorised";

		public const string PaymentAuthorizationAcceptedReference = "Payment Authorization Accepted";

		public void ResetPaymentStatus()
		{
			B2_PaymentStatus = ZString.Empty;

			var previousAuthorizationDate = (ZDateTime)B2_PaymentAuthorizationDateInfo.OriginalValue;
			Logs.AddNew(Events.AuthorisationWithdrawn, "Payment Status reset. " + (previousAuthorizationDate.IsValid ? "Previous Payment Authorized on " + previousAuthorizationDate.ToString("dd/MMM/yy") : ""));

			var logs = Logs.Find(GetPaymentAuthorizationAcceptedQuery());
			if (logs.Length > 0)
			{
				logs.ForEach(log => log.Cancel());
			}
		}

		public ZString PaymentStatusDescription
		{
			get { return Lookups.PaymentStatusList.GetDescriptionFromCode(B2_PaymentStatus); }
		}

		public ZPropertyInfo PaymentStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.PaymentStatusDescription); }
		}

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentPartyList))]
		public override ZString B2_PaymentParty
		{
			get { return base.B2_PaymentParty; }
			set { base.B2_PaymentParty = value; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override ZDateTime B2_PaymentAuthorizationDate
		{
			get { return base.B2_PaymentAuthorizationDate; }
			set
			{
				bool hasChanged = base.B2_PaymentAuthorizationDate != value;
				base.B2_PaymentAuthorizationDate = value;

				if (hasChanged)
				{
					foreach (CusStatementLine statementLine in ActiveLines)
					{
						var declaration = statementLine.Declaration;
						if (declaration != null)
						{
							declaration.US_PaymentDate = B2_PaymentAuthorizationDate;
						}
					}
				}
			}
		}

		public ZDateTime PaymentDateCalculated
		{
			get
			{
				var result = ZDateTime.Today;
				var workingDays = CustomsWorkingDays.GetInstance(Factory);
				return workingDays.GetAnotherStandardWorkingDay(result.ToDateTime(), -1).Date;
			}
		}

		internal bool PaymentDateChangedByMessageProcessor;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && B2_PaymentAuthorizationDate.IsValid && PaymentDateChangedByMessageProcessor)
			{
				var factory = new BusinessObjectFactory();

				foreach (CusStatementLine statementLine in ActiveLines)
				{
					var declaration = statementLine.Declaration;
					if (declaration != null)
					{
						var declarationLoaded = factory.Load<JobDeclaration>(declaration.PK);

						var addInfo = new AddInfoJobDeclaration(declarationLoaded.JE_AddInfoInfo);

						if (addInfo.US_PaymentDate != B2_PaymentAuthorizationDate)
						{
							ErrorReporter.ReportOnce("JobDeclaration.PaymentDate changes not saved", "Assign this to Joo: JobDeclaration.PaymentDate " + addInfo.US_PaymentDate + ", CusStatementHeader.PaymentAuthorizationDate " + B2_PaymentAuthorizationDate); // Column name used in error message, not key
							break;
						}
					}
				}
			}

			PaymentDateChangedByMessageProcessor = false;

			if (saveSucceeded)
			{
				new StatementAccIntegration(ServiceTaskLogger).Integrate(new MessageProcessingStatementAccInvoiceIntegrationDataProvider(this));
			}
		}

		public override ZString B2_CheckNo
		{
			get { return base.B2_CheckNo; }
			set
			{
				bool hasChanges = base.B2_CheckNo != value;
				base.B2_CheckNo = value;

				if (hasChanges && !IsCopying)
				{
					if (!B2_CheckNo.IsEmpty)
					{
						B2_PaymentParty = PaymentPartyList.Codes.Importer;
					}
				}
			}
		}

		#endregion

		#region Collections

		/// <summary>
		/// It only loads CusStatementLines whose B2_B3 == this.PK. It does not loads all lines for periodic monthly statements
		/// </summary>
		[ChildEditable(false)]
		public CusStatementLineCollection StatementLines
		{
			get
			{
#if DEBUG
				if (IsMonthlyStatement)
				{
					ErrorReporter.ReportOnce("StatementLines for a monthly statement", "You are not supposed to access this collection from this statement as this is a monthly statement");
				}
#endif

				if (fStatementLine == null)
				{
					fStatementLine = new CusStatementLineCollection(this);
					fStatementLine.Load();
					RegisterEditableChildObject(fStatementLine);
				}
				return fStatementLine;
			}
		}
		CusStatementLineCollection fStatementLine;

		/// <summary>
		/// This is not BusinessObject.IsDeleted. An entry can be deleted from a statement and statement has such entries with its status marked 'Deleted'
		/// </summary>
		[ChildEditableTestExclude]
		public MonthlyDeletedStatementLinesCollection MonthlyDeletedLines
		{
			get
			{
				if (fMonthlyDeletedLines == null)
				{
					fMonthlyDeletedLines = new MonthlyDeletedStatementLinesCollection(this);
					fMonthlyDeletedLines.Load();
				}
				return fMonthlyDeletedLines;
			}
		}
		MonthlyDeletedStatementLinesCollection fMonthlyDeletedLines;

		/// <summary>
		/// This is not BusinessObject.IsDeleted. An entry can be deleted from a statement and statement has such entries with its status marked 'Deleted'
		/// </summary>
		public CusStatementLineStatusCollection DailyDeletedLines
		{
			get { return fDailyDeletedLines ?? (fDailyDeletedLines = new CusStatementLineStatusCollection(this, StatementLineStatusList.Codes.Deleted)); }
		}
		CusStatementLineStatusCollection fDailyDeletedLines;

		public CusStatementLineStatusCollection ActiveLines
		{
			get { return fActiveLines ?? (fActiveLines = new CusStatementLineStatusCollection(this, StatementLineStatusList.Codes.Active)); }
		}
		CusStatementLineStatusCollection fActiveLines;

		/// <summary>
		/// For preliminary statements, this should return all the lines including deleted. For final statements, it returns only active lines.
		/// </summary>
		public BusinessObjectCollection AllOrActiveLines
		{
			get { return B2_Status == StatementHeaderStatusList.Codes.Preliminary ? StatementLines : ActiveLines; }
		}

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		[ChildEditable(false)] // Issue 00214657 Otherwise while performing ValidateAll, this collection is touched even for daily statements. Users cannot modify these records any way
		public CusStatementHeaderCollection DailyStatements
		{
			get
			{
#if DEBUG
				if (IsDailyStatement)
				{
					ErrorReporter.ReportOnce("DailyStatements", "You are trying to access DailyStatements collection from a daily statement");
				}
#endif

				if (dailyStatements == null)
				{
					dailyStatements = new CusStatementHeaderCollection(this);
					RegisterEditableChildObject(dailyStatements);
				}
				return dailyStatements;
			}
		}
		CusStatementHeaderCollection dailyStatements;

		public CusStatementHeaderCollection DailyStatementsForAccountingRecon
		{
			get
			{
				if (IsDailyStatement)
				{
					ErrorReporter.ReportOnce("DailyStatements", "You are trying to access DailyStatements collection from a daily statement");
				}

				if (dailyStatementsForAccountingRecon == null)
				{
					dailyStatementsForAccountingRecon = new CusStatementHeaderCollection(this, true);
				}
				return dailyStatementsForAccountingRecon;
			}
		}
		CusStatementHeaderCollection dailyStatementsForAccountingRecon;

		public CusStatementLineAccountingReconCollection StatementLinesForAccountingRecon
		{
			get
			{
				if (fStatementLinesForAccountingRecon == null)
				{
					fStatementLinesForAccountingRecon = new CusStatementLineAccountingReconCollection(this);
				}
				return fStatementLinesForAccountingRecon;
			}
		}
		CusStatementLineAccountingReconCollection fStatementLinesForAccountingRecon;

		public StatementFeeCodeLineCollection StatementFeeCodes
		{
			get
			{
				if (fStatementFeeCodes == null)
				{
					fStatementFeeCodes = new StatementFeeCodeLineCollection();
					var codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);
					foreach (var code in codes)
					{
						fStatementFeeCodes.Add(new StatementFeeCodeLine(this, code.ZZD_Code, code.ZZD_Description));
					}
				}
				return fStatementFeeCodes;
			}
		}
		StatementFeeCodeLineCollection fStatementFeeCodes;

		#endregion

		#region New Properties

		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.LineFilterByOptions))]
		[MaxLength(3)]
		public ZString FilterStatementLinesBy
		{
			get { return filterStatementLinesBy; }
			set
			{
				bool hasChanges = (FilterStatementLinesBy != value);

				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(FilterStatementLinesByInfo, ref filterStatementLinesBy, value);
				}

				if (hasChanges)
				{
					if (IsDailyStatement)
					{
						ActiveBusinessObjectCollection<CusStatementLine>.RefreshAll(Factory);
					}
					else
					{
						ActiveBusinessObjectCollection<CusStatementHeader>.RefreshAll(Factory);
					}
				}
			}
		}
		ZString filterStatementLinesBy;

		public ZPropertyInfo FilterStatementLinesByInfo
		{
			get { return GetZPropertyInfo(Schema.FilterStatementLinesBy); }
		}

		public bool HasLinesWithDeletionPending
		{
			get
			{
				if (IsMonthlyStatement)
				{
					foreach (CusStatementHeader statementHeader in DailyStatements)
					{
						if (statementHeader.HasLinesWithDeletionPending)
						{
							return true;
						}
					}
					return false;
				}
				else
				{
					return StatementLines.HasLinesWithDeletionPending;
				}
			}
		}

		public bool HasDeletedLines
		{
			get
			{
				if (IsMonthlyStatement)
				{
					foreach (CusStatementHeader statementHeader in DailyStatements)
					{
						if (statementHeader.HasDeletedLines)
						{
							return true;
						}
					}
					return false;
				}
				else
				{
					foreach (CusStatementLine statementLine in StatementLines)
					{
						if (statementLine.IsStatusDeleted)
						{
							return true;
						}
					}
					return false;
				}
			}
		}
		/// <summary>
		/// Customs issues a monthly statement near to the payment date of the month
		/// </summary>
		public bool IsOnAMonthlyStatement
		{
			get { return MonthlyStatementHeader != null; }
		}

		public bool IsPaymentInProgress
		{
			get { return B2_PaymentStatus == PaymentStatusList.Codes.PaymentInProgress; }
		}
		public ZString StatementType
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsMonthlyStatement)
				{
					result = "Monthly";
				}
				else if (IsDailyStatement)
				{
					result = "Daily";
				}

				return result;
			}
		}

		public ZBool IsMonthlyStatement
		{
			get { return B2_IsMonthlyStatement; }
		}

		public ZBool IsDailyStatement
		{
			get { return !B2_StatementNumber.IsEmpty && !IsMonthlyStatement; }
		}

		/// <summary>
		/// If so, a monthly statement will be issue at a set date and debit will happen at the date of a month, not daily
		/// </summary>
		public ZBool IsPeriodicDailyStatement
		{
			get { return IsDailyStatement && Business.PaymentTypeList.IsPeriodicPayment(B2_PaymentType); }
		}

		public ZBool IsPreliminary
		{
			get { return B2_Status == StatementHeaderStatusList.Codes.Preliminary; }
		}

		//used by document filters
		public ZBool IsFinalOrDeleted
		{
			get { return B2_Status == StatementHeaderStatusList.Codes.Final || B2_Status == StatementHeaderStatusList.Codes.Deleted; }
		}

		public bool IsFinal
		{
			get { return B2_Status == StatementHeaderStatusList.Codes.Final; }
		}

		public bool IsStatusDeleted
		{
			get { return B2_Status == StatementHeaderStatusList.Codes.Deleted; }
		}

		public ZBool IsPaid
		{
			get { return PaymentAuthorizationAccepted || B2_PaymentStatus == PaymentStatusList.Codes.PaymentAuthorizationAccepted || B2_Status == StatementHeaderStatusList.Codes.Final; }
		}

		public ZString StatementStatusDescription
		{
			get { return Lookups.StatementHeaderStatusList.GetDescriptionFromCode(B2_Status); }
		}

		public ZPropertyInfo StatementStatusDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(StatementStatusDescription)); }
		}

		public ZDecimal TotalAmountDue
		{
			get
			{
				if (IsMonthlyStatement)
				{
					return GetPayableAmountForMonthlyStatementFromMessage(Constants.TotalAmountDue, true);
				}
				else
				{
					ZDecimal result = B2_StatementAmount;
					if (B2_Status == StatementHeaderStatusList.Codes.Final)
					{
						result += GetTotalFeeAmountForDeletedLines();
					}
					return result;
				}
			}
		}

		public ZPropertyInfo TotalAmountDueInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.TotalAmountDue); }
		}

		public ZDecimal FinalTotalAmountDue
		{
			get { return IsMonthlyStatement ? GetPayableAmountForMonthlyStatementFromMessage(Constants.TotalAmountDue, false) : B2_StatementAmount; }
		}

		public ZPropertyInfo FinalTotalAmountDueInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.FinalTotalAmountDue); }
		}

		public ZDecimal BrokerPaymentAmount
		{
			get { return IsPaidByBroker ? FinalTotalAmountDue : ZDecimal.Zero; }
		}

		public ZString ImporterName
		{
			get { return Importer != null ? Importer.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo ImporterNameInfo
		{
			get { return GetZPropertyInfo(Schema.ImporterName); }
		}

		public ZBool IsTaxDeferred
		{
			get { return TotalDeferredTax > 0m; }
		}

		public ZBool IsPMSType
		{
			get { return PaymentTypeList.IsPeriodicPayment(B2_PaymentType); }
		}

		public ZBool IsPaymentPartyBroker
		{
			get { return B2_PaymentParty.IsEmpty ? PaymentTypeList.IsPaidByBroker(B2_PaymentType) : IsPaidByBroker; }
		}

		public ZString ImporterCustomsIDForDocument
		{
			get
			{
				var result = ZString.Empty;
				var importerCustomsID = B2_ImporterCustomsID;
				if (!SocialSecurityNumberValidator.IsValidSSN(importerCustomsID))
				{
					result = importerCustomsID;
				}

				return result;
			}
		}

		#endregion

		#region New Methods

		internal void DeactivateStatementLine(ZString entryFilerCode, ZString entryNumber)
		{
			var statementLine = StatementLines.GetStatementLineFor(entryFilerCode, entryNumber);
			if (statementLine != null)
			{
				statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;
				ActiveLines.Rebuild();
				if (StatementLines.AreAllLinesDeleted)
				{
					B2_Status = StatementHeaderStatusList.Codes.Deleted;
				}
			}
		}

		internal bool CanDeactivateStatementLine
		{
			get { return IsPreliminary && !PaymentStatusList.IsPaidOrPaymentInProgress(B2_PaymentStatus); }
		}

		#endregion

		#region Payment Authorised/Accepted

		public ZBool PaymentAuthorizationAccepted
		{
			get
			{
				var logs = Logs.Find(GetPaymentAuthorizationAcceptedQuery());
				return logs.Length > 0;
			}
		}

		ZQuery GetPaymentAuthorizationAcceptedQuery()
		{
			var query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, PaymentAuthorizationAcceptedReference);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, ZBool.False);

			return query;
		}
		public ZBool ApprovalActionAuthorised
		{
			get
			{
				var mostRecentLog = ApprovalActionStatusLogs.MostRecentLog;
				return mostRecentLog != null && mostRecentLog.SL_Reference == PermissionGranted;
			}
		}
		public const string PermissionGranted = "Permission Granted for Payment Authorization";
		public const string PermissionRevoked = "Permission Revoked for Payment Authorization";

		LogsForNominatedEvent ApprovalActionStatusLogs
		{
			get { return approvalActionStatusLogs ?? (approvalActionStatusLogs = new LogsForNominatedEvent(Logs, Events.Authorised)); }
		}
		LogsForNominatedEvent approvalActionStatusLogs;

		public void AddAuthorisationLog()
		{
			var logReference = ApprovalActionAuthorised ? PermissionRevoked : PermissionGranted;
			ApprovalActionStatusLogs.CancelAll();
			ApprovalActionStatusLogs.AddNew(logReference);

			try
			{
				this.Factory.Save();
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
		}

		#endregion

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			if (property.Name == Schema.FilterStatementLinesBy ||
				property.Name == Schema.PostARInvoices ||
				property.Name == Schema.PostAPInvoices ||
				property.Name == Schema.MakePayment ||
				property.Name == Schema.B2_CheckNo)
			{
				return false;
			}
			return true;
		}

		#endregion

		#region Accounting Integration

		#region Calculated Amounts from Accounting

		public ZBool APFullyPaid
		{
			get { return AccountingAP_ARInvoiceQueryResult.APFullyPaid; }
		}

		public ZPropertyInfo APFullyPaidInfo
		{
			get { return GetZPropertyInfo(Schema.APFullyPaid); }
		}

		public ZDecimal APPostedAmount
		{
			get { return AccountingAP_ARInvoiceQueryResult.APPostedAmount; }
		}

		public ZPropertyInfo APPostedAmountInfo
		{
			get { return GetZPropertyInfo(Schema.APPostedAmount); }
		}

		public ZDecimal APUnPostedAmount
		{
			get { return AccountingAP_ARInvoiceQueryResult.APUnPostedAmount; }
		}

		public ZPropertyInfo APUnPostedAmountInfo
		{
			get { return GetZPropertyInfo(Schema.APUnPostedAmount); }
		}

		public ZDecimal APTotalAmount
		{
			get { return APPostedAmount + APUnPostedAmount; }
		}

		public ZPropertyInfo APTotalAmountInfo
		{
			get { return GetZPropertyInfo(Schema.APTotalAmount); }
		}

		public ZDecimal ARPostedAmount
		{
			get { return AccountingAP_ARInvoiceQueryResult.ARPostedAmount; }
		}

		public ZPropertyInfo ARPostedAmountInfo
		{
			get { return GetZPropertyInfo(Schema.ARPostedAmount); }
		}

		public ZDecimal ARUnPostedAmount
		{
			get { return AccountingAP_ARInvoiceQueryResult.ARUnPostedAmount; }
		}

		public ZPropertyInfo ARUnPostedAmountInfo
		{
			get { return GetZPropertyInfo(Schema.ARUnPostedAmount); }
		}

		public ZDecimal ARTotalAmount
		{
			get { return ARPostedAmount + ARUnPostedAmount; }
		}

		public ZPropertyInfo ARTotalAmountInfo
		{
			get { return GetZPropertyInfo(Schema.ARTotalAmount); }
		}

		public ZDecimal DifferenceBetweenAPInvoiceAndCustomsAmount
		{
			get
			{
				return Math.Abs(BrokerPaymentAmount - (APPostedAmount + APUnPostedAmount));
			}
		}

		public ZPropertyInfo DifferenceBetweenAPInvoiceAndCustomsAmountInfo
		{
			get { return GetZPropertyInfo(Schema.DifferenceBetweenAPInvoiceAndCustomsAmount); }
		}

		public ZDecimal DifferenceBetweenARInvoiceAndCustomsAmount
		{
			get
			{
				return Math.Abs(BrokerPaymentAmount - (ARPostedAmount + ARUnPostedAmount));
			}
		}

		public ZPropertyInfo DifferenceBetweenARInvoiceAndCustomsAmountInfo
		{
			get { return GetZPropertyInfo(Schema.DifferenceBetweenARInvoiceAndCustomsAmount); }
		}

		public ZBool HasDiscrepancyBetweenInvoicesAndCustomsAmount
		{
			get
			{
				return DifferenceBetweenAPInvoiceAndCustomsAmount != 0m ||
					DifferenceBetweenARInvoiceAndCustomsAmount != 0m;
			}
		}

		AP_ARInvoiceQueryResult AccountingAP_ARInvoiceQueryResult
		{
			get
			{
				if (!accountingAP_ARInvoiceQueryResult.HasValue)
				{
					AP_ARInvoiceQueryResult result = new AP_ARInvoiceQueryResult();

					if (IsMonthlyStatement)
					{
						result.APFullyPaid = DailyStatements.Count > 0;

						foreach (CusStatementHeader dailyStatement in DailyStatements)
						{
							AP_ARInvoiceQueryResult dailyResult = dailyStatement.AccountingAP_ARInvoiceQueryResult;

							result.Merge(dailyResult);
						}
					}
					else
					{
						var lines = this.GetStatementLinesToAutorate();

						result.APFullyPaid = lines.Any();

						foreach (CusStatementLine line in lines)
						{
							result.Merge(line.AccountingAP_ARInvoiceQueryResult);
						}
					}

					accountingAP_ARInvoiceQueryResult = result;
				}
				return accountingAP_ARInvoiceQueryResult.Value;
			}
		}
		AP_ARInvoiceQueryResult? accountingAP_ARInvoiceQueryResult;

		public void RefreshAP_ARInvoiceQueryResult()
		{
			accountingAP_ARInvoiceQueryResult = null;

			if (IsMonthlyStatement)
			{
				foreach (CusStatementHeader daily in DailyStatements)
				{
					daily.RefreshAP_ARInvoiceQueryResult();
				}
			}
			else
			{
				StatementLines.RefreshAP_ARInvoiceDetails();
			}

			RefreshBinding();
		}

		#endregion

		#region NonPersistent Options

		public ZBool PostARInvoices
		{
			get { return postARInvoices; }
			set
			{
				postARInvoices = value;
				PostARInvoicesInfo.RefreshBinding();
			}
		}
		ZBool postARInvoices;

		public ZPropertyInfo PostARInvoicesInfo
		{
			get { return GetZPropertyInfo(Schema.PostARInvoices); }
		}

		public ZBool PostAPInvoices
		{
			get { return postAPInvoices; }
			set
			{
				postAPInvoices = value;
				PostAPInvoicesInfo.RefreshBinding();
			}
		}
		ZBool postAPInvoices;

		public ZPropertyInfo PostAPInvoicesInfo
		{
			get { return GetZPropertyInfo(Schema.PostAPInvoices); }
		}

		public ZBool MakePayment
		{
			get { return makePayment; }
			set
			{
				makePayment = value;
				MakePaymentInfo.RefreshBinding();
			}
		}
		ZBool makePayment;

		public ZPropertyInfo MakePaymentInfo
		{
			get { return GetZPropertyInfo(Schema.MakePayment); }
		}

		#endregion

		#region Notifications

		public string GetWarningNotificationsBeforePerformingAccIntegration()
		{
			ZStringBuilder result = new ZStringBuilder();

			if (IsPreliminary)
			{
				if (PostAPInvoices || MakePayment)
				{
					result.Append(PrelimStatus);
				}
			}

			if (IsPeriodicDailyStatement && MakePayment &&
				USCustomsDataRegistry.Instance.MonthlyStatementPaymentPosting.GetFallBackValueAtAllLevels(Company.PK.ToGuid(), Guid.Empty, Guid.Empty) != MonthlyStatementPaymentPostingOptionList.Codes._1)
			{
				AccountingIntegrationOptions options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(B2_GC.ToGuid(), Guid.Empty, Guid.Empty);
				if (options != null && options.EnableAccountingIntegration)
				{
					result.Append(OnePaymentPerMonthlyStatement);
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		public const string PrelimStatus = "This statement is still preliminary and you have selected to integrate with AP or Payment.";
		public const string OnePaymentPerMonthlyStatement = "You have selected to make a payment for a periodic daily statement. System will not be able to make a payment automatically when its monthly statement is finalised as payment will already have made for this statement.";

		public string GetErrorNotificationsBeforePerformingAccIntegration()
		{
			ZStringBuilder result = new ZStringBuilder();

			if (B2_Status == StatementHeaderStatusList.Codes.Deleted)
			{
				result.Append(StatementIsDeleted);
			}

			if (!PostARInvoices && !PostAPInvoices && !MakePayment)
			{
				result.Append(NoAccIntegrationOptionIsSelected);
			}
			else
			{
				if (!IsPaidByBroker)
				{
					result.Append(NotPaidByBroker);
				}

				if (B2_StatementAmount == 0)
				{
					result.Append(NothingToPay);
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		public const string NotPaidByBroker = "This statement is not indicated as paid by broker.";
		public const string StatementIsDeleted = "This statement is deleted.";
		public const string NoAccIntegrationOptionIsSelected = "No option is selected for 'Perform Acc Integration'";
		internal const string NothingToPay = "None of entries on this statement have amounts to pay to Customs.";

		#endregion

		//invoked by users
		public AutoBillingResult PerformAccIntegration()
		{
			var result = new StatementAccIntegration(null).Integrate(new UserInvokedStatementAccInvoiceIntegrationDataProvider(this));

			RefreshAP_ARInvoiceQueryResult();

			return result;
		}

		public bool IsPaidByBroker
		{
			get { return B2_PaymentParty == PaymentPartyList.Codes.Broker; }
		}

		public ZBool ManagedACH
		{
			get { return PaymentTypeList.IsPaidByImporter(B2_PaymentType) && IsPaidByBroker; }
		}

		public ZPropertyInfo ManagedACHInfo
		{
			get { return GetZPropertyInfo(Schema.ManagedACH); }
		}

		internal void MarkAccPaymentIntegrated()
		{
			if (HasStatusJustBeenChanged)
			{
				AccIntegrationFlags.Remove(CusStatementHeaderSchema.B2_Status.Name);
			}
		}

		internal virtual void MarkAccInvoicingIntegrated()
		{
			if (HasPaymentJustBeenAccepted)
			{
				AccIntegrationFlags.Remove(CusStatementHeaderSchema.B2_PaymentStatus.Name);
			}
		}

		internal bool HasPaymentJustBeenAccepted
		{
			get { return B2_PaymentStatus == PaymentStatusList.Codes.PaymentAuthorizationAccepted && AccIntegrationFlags.ContainsKey(CusStatementHeaderSchema.B2_PaymentStatus.Name); }
		}

		internal bool HasStatusJustBeenChanged
		{
			get { return AccIntegrationFlags.ContainsKey(CusStatementHeaderSchema.B2_Status.Name); }
		}

		void MarkAccToBeIntegrated(string columnName)
		{
			AccIntegrationFlags[columnName] = true;
		}

		Dictionary<string, bool> AccIntegrationFlags
		{
			get { return accIntegrationFlags ?? (accIntegrationFlags = new Dictionary<string, bool>()); }
		}
		Dictionary<string, bool> accIntegrationFlags;

		#endregion

		#region Related Business Objects

		public CusStatementHeader MonthlyStatementHeader
		{
			get { return !IsMonthlyStatement ? Factory.Load<CusStatementHeader>(B2_B2_PeriodicStatement) : null; }
		}

		public OrgHeaderWrapper ImporterWrapper
		{
			get { return importerWrapper ?? (importerWrapper = OrgHeaderWrapper.New(Importer)); }
		}
		OrgHeaderWrapper importerWrapper;

		internal LoggingInformation ServiceTaskLogger
		{
			get { return serviceTaskLogger; }
			set { serviceTaskLogger = value; }
		}
		LoggingInformation serviceTaskLogger;

		#endregion

		#region Documents Properties

		public ZString StatementFor
		{
			get { return this.GetStatementFor(); }
		}

		public ZDecimal TotalDuty
		{
			get { return GetTotalPayableAmountForAllLines(Core.Constants.USCustoms.FeeCodes.Duty); }
		}

		public ZDecimal TotalTaxAmount
		{
			get { return TotalPayableTax + TotalDeferredTax; }
		}

		public ZDecimal TotalPayableTax
		{
			get { return GetTotalPayableAmountForAllLines(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable); }
		}

		public ZDecimal TotalDeferredTax
		{
			get { return GetAmount(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, true); }
		}

		public ZPropertyInfo TotalDeferredTaxInfo
		{
			get { return GetZPropertyInfo(Schema.TotalDeferredTax); }
		}

		public ZDecimal TotalCVD
		{
			get { return GetTotalPayableAmountForAllLines(Core.Constants.USCustoms.FeeCodes.CountervailingDuty); }
		}

		public ZDecimal TotalADD
		{
			get { return GetTotalPayableAmountForAllLines(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty); }
		}

		public ZDecimal TotalInterestAmountForReconciliationSummary
		{
			get { return GetTotalPayableAmountForAllLines(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest); }
		}

		public ZDecimal TotalAmountsPayable
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (IsMonthlyStatement)
				{
					foreach (CusStatementHeader daily in DailyStatements)
					{
						result += daily.TotalAmountsPayable;
					}
				}
				else
				{
					foreach (CusStatementLine line in StatementLines)
					{
						if (!line.IsStatusDeleted)
						{
							result += line.B3_CustomsFeesTotal;
						}
					}
				}
				return result;
			}
		}

		public ZInt TotalNumberRevenueProducingEntries
		{
			get
			{
				ZInt count = ZInt.Zero;

				if (IsMonthlyStatement)
				{
					foreach (CusStatementHeader daily in DailyStatements)
					{
						count += daily.TotalNumberRevenueProducingEntries;
					}
				}
				else
				{
					foreach (CusStatementLine line in StatementLines)
					{
						if (line.HasAmountsToPay)
						{
							count++;
						}
					}
				}
				return count;
			}
		}

		public ZString TotalNumberRevenueProducingEntriesForPrint
		{
			get { return TotalNumberRevenueProducingEntries.ToString(); }
		}

		public ZInt TotalNumberNonRevenueProducingEntries
		{
			get
			{
				ZInt count = ZInt.Zero;

				if (IsMonthlyStatement)
				{
					foreach (CusStatementHeader daily in DailyStatements)
					{
						count += daily.TotalNumberNonRevenueProducingEntries;
					}
				}
				else
				{
					foreach (CusStatementLine line in StatementLines)
					{
						if (!line.HasAmountsToPay)
						{
							count++;
						}
					}
				}
				return count;
			}
		}

		public ZString TotalNumberNonRevenueProducingEntriesForPrint
		{
			get { return TotalNumberNonRevenueProducingEntries.ToString(); }
		}

		#region FinalTotals - Only Active Lines

		public ZDecimal FinalTotalDuty
		{
			get { return GetTotalPayableAmountForActiveLines(Core.Constants.USCustoms.FeeCodes.Duty); }
		}

		public ZDecimal FinalTotalTaxAmount
		{
			get { return FinalTotalPayableTax + FinalTotalDeferredTax; }
		}

		public ZDecimal FinalTotalPayableTax
		{
			get { return GetTotalPayableAmountForActiveLines(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable); }
		}

		public ZDecimal FinalTotalDeferredTax
		{
			get { return GetAmount(Core.Constants.USCustoms.FeeCodes.ExciseTaxDeferred, false); }
		}

		public ZDecimal FinalTotalCVD
		{
			get { return GetTotalPayableAmountForActiveLines(Core.Constants.USCustoms.FeeCodes.CountervailingDuty); }
		}

		public ZDecimal FinalTotalADD
		{
			get { return GetTotalPayableAmountForActiveLines(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty); }
		}

		public ZDecimal FinalTotalInterestAmountForReconciliationSummary
		{
			get { return GetTotalPayableAmountForActiveLines(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest); }
		}

		public ZDecimal FinalTotalUserFees
		{
			get { return GetUserFees(false); }
		}

		ZDecimal GetUserFees(bool forAllLines)
		{
			ZDecimal result = ZDecimal.Zero;

			if (IsMonthlyStatement)
			{
				foreach (CusStatementHeader daily in DailyStatements)
				{
					result += daily.GetUserFees(forAllLines);
				}
			}
			else
			{
				foreach (CusStatementLine line in StatementLines)
				{
					if (forAllLines || line.IsActive)
					{
						result += line.UserFees;
					}
				}
			}

			return result;
		}

		public ZInt FinalTotalNumberRevenueProducingEntries
		{
			get
			{
				ZInt count = ZInt.Zero;

				if (IsMonthlyStatement)
				{
					foreach (CusStatementHeader daily in DailyStatements)
					{
						count += daily.FinalTotalNumberRevenueProducingEntries;
					}
				}
				else
				{
					foreach (CusStatementLine line in StatementLines)
					{
						if (line.IsActive && line.HasAmountsToPay)
						{
							count++;
						}
					}
				}
				return count;
			}
		}

		public ZString FinalTotalNumberRevenueProducingEntriesForPrint
		{
			get { return FinalTotalNumberRevenueProducingEntries.ToString(); }
		}

		public ZInt FinalTotalNumberNonRevenueProducingEntries
		{
			get
			{
				ZInt count = ZInt.Zero;

				if (IsMonthlyStatement)
				{
					foreach (CusStatementHeader daily in DailyStatements)
					{
						count += daily.FinalTotalNumberNonRevenueProducingEntries;
					}
				}
				else
				{
					foreach (CusStatementLine line in StatementLines)
					{
						if (line.IsActive && !line.HasAmountsToPay)
						{
							count++;
						}
					}
				}
				return count;
			}
		}

		public ZString FinalTotalNumberNonRevenueProducingEntriesForPrint
		{
			get { return FinalTotalNumberNonRevenueProducingEntries.ToString(); }
		}

		#endregion

		public ZDecimal GetTotalFeeAmountForDeletedLines()
		{
			ZDecimal result = ZDecimal.Zero;

			if (IsMonthlyStatement)
			{
				foreach (CusStatementHeader daily in DailyStatements)
				{
					result += daily.GetTotalFeeAmountForDeletedLines();
				}
			}
			else
			{
				foreach (CusStatementLine line in StatementLines)
				{
					if (line.IsStatusDeleted)
					{
						result += line.B3_CustomsFeesTotal;
					}
				}
			}
			return result;
		}

		public ZDecimal GetTotalPayableAmountForAllLines(string chargeType)
		{
			return GetPayableAmount(chargeType, true);
		}

		public ZDecimal GetTotalPayableAmountForActiveLines(string chargeType)
		{
			return GetPayableAmount(chargeType, false);
		}

		/// <summary>
		/// For warehouse entries, duty & other fees are not payable even though they are in statement messages.
		/// Only HMF is payable. 
		/// </summary>
		ZDecimal GetPayableAmount(ZString chargeType, bool forAllLines)
		{
			ZDecimal result = ZDecimal.Zero;

			if (IsMonthlyStatement)
			{
				result = GetPayableAmountForMonthlyStatementFromMessage(chargeType, forAllLines);
			}
			else
			{
				foreach (CusStatementLine line in StatementLines)
				{
					if (forAllLines || line.IsActive)
					{
						result += line.GetPayableAmount(chargeType);
					}
				}
			}
			return result;
		}

		ZDecimal GetPayableAmountForMonthlyStatementFromMessage(ZString chargeType, bool forAllLines)
		{
			var result = ZDecimal.Zero;
			if (totalChargesCached == null)
			{
				totalChargesCached = new CachedProperty<Dictionary<ZString, Dictionary<ZString, ZDecimal>>>(Factory, delegate
					{
						var allTotalChargedFromMessage = new Dictionary<ZString, Dictionary<ZString, ZDecimal>>();
						var lastStatementMessage = (MQEDIMessage)Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement, EDIMessage.Direction.Receive);
						if (lastStatementMessage != null)
						{
							GetChargesAndFeesFromMessage(allTotalChargedFromMessage, lastStatementMessage);
						}
						return allTotalChargedFromMessage;
					});
			}

			Dictionary<ZString, ZDecimal> feesTotal = null;
			if (totalChargesCached.Value.TryGetValue(forAllLines ? StatementHeaderStatusList.Codes.Preliminary : StatementHeaderStatusList.Codes.Final, out feesTotal))
			{
				ZDecimal resultFromCachedCharges;
				if (feesTotal.TryGetValue(chargeType, out resultFromCachedCharges))
				{
					result = resultFromCachedCharges;
				}
			}
			return result;
		}
		CachedProperty<Dictionary<ZString, Dictionary<ZString, ZDecimal>>> totalChargesCached;

		/// <summary>
		/// Periodic Monthly Statement : Record Identifiers Q1, Q2, and QA are mandatory detail payment records.
		/// Record Identifiers Q3, Q4, and QE are mandatory total payment due records for preliminary or final statements.
		/// Record Identifiers Q5, Q6, and QJ are mandatory preliminary amount records (final statement).
		/// </summary>
		/// <param name="allTotalChargedFromMessage">Parse message and get all charges to that dictionary</param>
		/// <param name="lastStatementMessage">Latest Periodic Monthly Statement message</param>
		void GetChargesAndFeesFromMessage(Dictionary<ZString, Dictionary<ZString, ZDecimal>> allTotalChargedFromMessage, MQEDIMessage lastStatementMessage)
		{
			var isFinalMessage = ((APLB)lastStatementMessage.MessageBlock.B).StatementStatus == "F";
			var preliminaryTotals = new Dictionary<ZString, ZDecimal>();
			var finalTotals = new Dictionary<ZString, ZDecimal>();

			var dictionaryToAddQ3_QE = isFinalMessage ? finalTotals : preliminaryTotals;
			foreach (var block in lastStatementMessage.MessageBlock.MessageBlocks)
			{
				if (block is PMSQ3)
				{
					var pmsq3 = (PMSQ3)block;
					dictionaryToAddQ3_QE.Add(Core.Constants.USCustoms.FeeCodes.Duty, pmsq3.TotalDuty);
					dictionaryToAddQ3_QE.Add(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, pmsq3.TotalTax);
				}
				else if (block is PMSQ4)
				{
					var pmsq4 = (PMSQ4)block;
					dictionaryToAddQ3_QE.Add(Constants.TotalAmountDue, pmsq4.TotalAmountDue);
					dictionaryToAddQ3_QE.Add(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, pmsq4.TotalAntidumpingDuty);
					dictionaryToAddQ3_QE.Add(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, pmsq4.TotalCountervailingDuty);
				}
				else if (block is PMSQE)
				{
					var pmsqe = (PMSQE)block;
					if (!pmsqe.FirstFeeClassCode.IsEmpty)
					{
						dictionaryToAddQ3_QE.Add(pmsqe.FirstFeeClassCode, pmsqe.FirstFeeAmount);
					}

					if (!pmsqe.SecondFeeClassCode.IsEmpty)
					{
						dictionaryToAddQ3_QE.Add(pmsqe.SecondFeeClassCode, pmsqe.SecondFeeAmount);
					}

					if (!pmsqe.ThirdFeeClassCode.IsEmpty)
					{
						dictionaryToAddQ3_QE.Add(pmsqe.ThirdFeeClassCode, pmsqe.ThirdFeeAmount);
					}

					if (!pmsqe.FourthFeeClassCode.IsEmpty)
					{
						dictionaryToAddQ3_QE.Add(pmsqe.FourthFeeClassCode, pmsqe.FourthFeeAmount);
					}

					if (!pmsqe.FifthFeeClassCode.IsEmpty)
					{
						dictionaryToAddQ3_QE.Add(pmsqe.FifthFeeClassCode, pmsqe.FifthFeeAmount);
					}
				}
				else if (block is PMSQ5)
				{
					var pmsq5 = (PMSQ5)block;
					preliminaryTotals.Add(Core.Constants.USCustoms.FeeCodes.Duty, pmsq5.TotalDuty);
					preliminaryTotals.Add(Core.Constants.USCustoms.FeeCodes.ExciseTaxPayable, pmsq5.TotalTax);
				}
				else if (block is PMSQ6)
				{
					var pmsq6 = (PMSQ6)block;
					preliminaryTotals.Add(Constants.TotalAmountDue, pmsq6.TotalAmountDue);
					preliminaryTotals.Add(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, pmsq6.TotalAntidumpingDuty);
					preliminaryTotals.Add(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, pmsq6.TotalCountervailingDuty);
				}
				else if (block is PMSQJ)
				{
					var pmsqj = (PMSQJ)block;
					if (!pmsqj.FirstFeeClassCode.IsEmpty)
					{
						preliminaryTotals.Add(pmsqj.FirstFeeClassCode, pmsqj.FirstFeeAmount);
					}

					if (!pmsqj.SecondFeeClassCode.IsEmpty)
					{
						preliminaryTotals.Add(pmsqj.SecondFeeClassCode, pmsqj.SecondFeeAmount);
					}

					if (!pmsqj.ThirdFeeClassCode.IsEmpty)
					{
						preliminaryTotals.Add(pmsqj.ThirdFeeClassCode, pmsqj.ThirdFeeAmount);
					}

					if (!pmsqj.FourthFeeClassCode.IsEmpty)
					{
						preliminaryTotals.Add(pmsqj.FourthFeeClassCode, pmsqj.FourthFeeAmount);
					}

					if (!pmsqj.FifthFeeClassCode.IsEmpty)
					{
						preliminaryTotals.Add(pmsqj.FifthFeeClassCode, pmsqj.FifthFeeAmount);
					}
				}
			}
			allTotalChargedFromMessage.Add(StatementHeaderStatusList.Codes.Preliminary, preliminaryTotals);
			allTotalChargedFromMessage.Add(StatementHeaderStatusList.Codes.Final, finalTotals);
		}

		ZDecimal GetAmount(ZString chargeType, bool forAllLines)
		{
			ZDecimal result = ZDecimal.Zero;

			if (IsMonthlyStatement)
			{
				foreach (CusStatementHeader daily in DailyStatements)
				{
					result += daily.GetAmount(chargeType, forAllLines);
				}
			}
			else
			{
				foreach (CusStatementLine line in StatementLines)
				{
					if (forAllLines || line.IsActive)
					{
						result += line.GetAmount(chargeType);
					}
				}
			}
			return result;
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return CreateNewDocumentSupporter(); }
		}

		protected DocumentSupporter CreateNewDocumentSupporter()
		{
			return new CusStatementHeaderDocumentSupporter(this);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CusStatementHeader);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IHaveRequiredDocuments Members

		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return B2_StatementNumber; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return ZString.Empty; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return ZString.Empty; }
		}

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return CusStatementHeaderSchema.Constants.Prefix; }
		}

		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes
		{
			get { return Array.Empty<ZString>(); }
		}

		[ChildEditable(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					fRequiredDocuments.Load();
					RegisterEditableChildObject(fRequiredDocuments);
				}
				return fRequiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection fRequiredDocuments;

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return this; }
		}

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents()
		{
		}

		#endregion

		#region Workflow and Tracking Interfaces Implementation

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return B2_StatementNumber; }
		}

		#endregion

		#region IJobHeaderParent Members

		public void SetJobNumberFieldOnSaving()
		{
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IWorkflowProvider Members

		protected override IColumnValueRanker GetTemplateSelectionCriteriaCore()
		{
			var job = new JobHeader.Loader(this).Load();
			if (job != null)
			{
				job.SetDefaultsForJob();
			}

			var result = base.GetTemplateSelectionCriteriaCore();
			result.Add(ProcessTaskTemplateSchema.P0_GB, FindBranchPKByBranchDesignationFromRegistry(), ZGuid.Empty);

			return result;
		}

		ZGuid FindBranchPKByBranchDesignationFromRegistry()
		{
			var branch = this.BranchByBranchDesignationFromRegistry;
			return branch == null ? ZGuid.Empty : branch.PK;
		}

		public GlbBranch BranchByBranchDesignationFromRegistry
		{
			get
			{
				if (branchByBranchDesignationFromRegistryCached == null)
				{
					branchByBranchDesignationFromRegistryCached = new CachedProperty<GlbBranch>(Factory, delegate
					{
						return this.FindBranchByBranchDesignationFromRegistry();
					});
				}
				return branchByBranchDesignationFromRegistryCached.Value;
			}
		}
		CachedProperty<GlbBranch> branchByBranchDesignationFromRegistryCached;

		protected override IEnumerable<IZType> GetClientsInTemplateSelectionOrder()
		{
			if (B2_OH_Importer.IsValid)
			{
				yield return B2_OH_Importer;
			}

			var job = new JobHeader.Loader(this).Load();
			if (job != null)
			{
				yield return job.LocalChargesPK;
			}
			yield return ZGuid.Empty;
		}

		string IJobNumberForWorkflow.JobNumber
		{
			get { return B2_StatementNumber; }
		}

		#endregion

		#endregion

		#region IMessageFailStatusManager Members

		bool IMessageFailStatusManager.IsMessageTypeSupported(ZString messageType)
		{
			bool result = false;
			switch (messageType)
			{
				case ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction: // no status
				case ACEApplicationIdentifierCodeList.Codes.StatementUpdate:
					result = true;
					break;
			}
			return result;
		}

		void IMessageFailStatusManager.SetFailStatus(MQEDIMessage message)
		{
		}

		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.CustomsStatement; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		#endregion

		#region IMessageAttachee Members

		ZString IMessageAttachee.MessageStatus
		{
			get { return B2_PaymentStatus; }
			set { B2_PaymentStatus = value; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return this; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return this.B2_StatementNumber; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return Logs; }
		}

		GlbBranch IMessageAttachee.Branch
		{
			get { return null; }
		}

		#endregion

		#region Type Safe

		public new CusStatementHeaderValidation Validation
		{
			get { return (CusStatementHeaderValidation)base.Validation; }
		}

		public new CusStatementHeaderLookups Lookups
		{
			get { return (CusStatementHeaderLookups)base.Lookups; }
		}

		protected override Customs.Business.CusStatementHeaderLookups GetNewLookups()
		{
			return new CusStatementHeaderLookups(this);
		}

		protected override Customs.Business.CusStatementHeaderValidation GetNewValidation()
		{
			return new CusStatementHeaderValidation(this);
		}

		#endregion

		public ZString ImporterCustomsIDForDisplay
		{
			get
			{
				if (SocialSecurityNumberValidator.IsValidSSN(B2_ImporterCustomsID) && !Env.Security.OrgDetailsViewPersonalInformation.IsAllowed)
				{
					return SocialSecurityNumberValidator.SSNWithMask;
				}
				return B2_ImporterCustomsID;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("47DC09BE-B0E4-4221-AF61-85A145F2318E", "Statement {0}", B2_StatementNumber).TrimEnd();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				return Res.GetString("18748B34-76E7-4F9C-A165-2BC61055B567", "Statement - {0}", B2_StatementNumber);
			}
		}
	}
}
