using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "External solution inherits this class implementation.")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public class SupervisorOverridesContext
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Context")]
		public const string Unknown = "Unknown";
		public const string SavingDeclaration = "SavingDeclaration";
		public const string SendingMessages = "SendingMessages";
	}

	[DebuggerDisplay("Supervisor name = {SupervisorName}; Context = {context}; MessagesForLog.Count = {MessagesForLog.Count}")]
	public class SupervisorOverrides : AutoSupervisorOverrides
	{
		#region Constants
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "External solution inherits this class implementation.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public class Constants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant message log")]
			public const string DeclarationHasAnyMessageErrors = "Sending with message errors";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant message log")]
			public const string MergeBy = "Merge By: '{0}' -> '{1}'";
		}

		#endregion

		#region Schema
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : AutoSupervisorOverrides.Schema
		{
			public const string SupervisorMessage = "SupervisorMessage";
			public const int SupervisorMessageMaxLength = 10000;
		}

		#endregion

		public SupervisorOverrides(IBusiness businessEntity, string context)
			: base(businessEntity.Factory)
		{
			this.businessEntity = businessEntity;
			this.context = context;
		}
		protected IBusiness businessEntity;
		protected string context = SupervisorOverridesContext.Unknown;

		#region Bound Properties

		#region SupervisorName

		[List(nameof(Lookups) + "." + nameof(SupervisorOverridesLookups.Users))]
		public override ZString SupervisorName
		{
			get { return base.SupervisorName; }
			set { base.SupervisorName = value; }
		}

		public GlbStaff Supervisor
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, SupervisorName); }
		}

		#endregion

		#endregion

		#region Properties

		[Password]
		public override ZString SupervisorPassword
		{
			get => base.SupervisorPassword;
			set => base.SupervisorPassword = value;
		}

		public SupervisorOverridesLookups Lookups
		{
			get { return lookups ?? (lookups = new SupervisorOverridesLookups(this)); }
		}
		SupervisorOverridesLookups lookups;

		public MessageLogCollection AuthorisedMessagesForLog
		{
			get
			{
				if (authorisedMessagesForLog == null)
				{
					authorisedMessagesForLog = new MessageLogCollection(Factory);
				}
				return authorisedMessagesForLog;
			}
		}
		MessageLogCollection authorisedMessagesForLog;

		public MessageLogCollection UnAuthorisedMessagesForLog
		{
			get
			{
				if (unAuthorisedMessagesForLog == null)
				{
					unAuthorisedMessagesForLog = new MessageLogCollection(Factory);
				}
				return unAuthorisedMessagesForLog;
			}
		}
		MessageLogCollection unAuthorisedMessagesForLog;

		public ZBool ContextIsUnknown
		{
			get { return context == SupervisorOverridesContext.Unknown; }
		}

		public ZBool ContextIsSavingDeclaration
		{
			get { return context == SupervisorOverridesContext.SavingDeclaration; }
		}

		public ZBool ContextIsSendingMessages
		{
			get { return context == SupervisorOverridesContext.SendingMessages; }
		}

		public ZBool HasDeclarationAsBusinessEntity
		{
			get { return businessEntity is BaseJobDeclaration; }
		}

		public ZBool SupervisorShouldApproveChanges
		{
			get { return UnAuthorisedMessagesForLog.Count != 0; }
		}

		public ZBool ShouldLogAuthorisedChanges
		{
			get { return AuthorisedMessagesForLog.Count != 0; }
		}

		#endregion

		#region implementation

		public void CreateMessages()
		{
			CreateMessagesCore();
		}

		protected virtual void CreateMessagesCore()
		{
			BaseJobDeclaration declaration = businessEntity as BaseJobDeclaration;
			if (HasDeclarationAsBusinessEntity && ContextIsSavingDeclaration)
			{
				CheckSavingDeclaration(declaration);
			}
			else if (ContextIsSendingMessages)
			{
				CheckMessageErrors(businessEntity);
			}
		}

		protected virtual void CheckSavingDeclaration(BaseJobDeclaration declaration)
		{
			CheckMergeBy(declaration);
		}

		protected virtual void CheckMessageErrors(IBusiness entity)
		{
			var messageErrorsAsString = GetMessageErrorCollector(entity).GetMessageErrors().ToUniqueMessageListString();

			if (messageErrorsAsString.Length > 0 && CheckSecurityRightForCheckpointRequired(Env.Security.AllowMessageErrors))
			{
				AddMessageLog(Env.Security.AllowMessageErrors, Constants.DeclarationHasAnyMessageErrors);
			}
		}

		protected virtual CustomsNotificationCollector GetMessageErrorCollector(IBusiness entity)
		{
			return new CustomsNotificationCollector(entity, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		protected virtual void CheckMergeBy(BaseJobDeclaration declaration)
		{
			var checkMergeByRequired = declaration.JE_MergeByInfo.HasChanges || !declaration.IsInDatabase;

			if (checkMergeByRequired)
			{
				var mergeBy = declaration.Importer != null ? declaration.GetEffectiveMergeCustomsInvoiceLinesBy(declaration.Importer) : ZString.Empty;
				var defaultJeMergeBy = mergeBy.IsEmpty ? new ZString(OrgConstants.MergeInvoiceLines.Tariff) : mergeBy;
				checkMergeByRequired = declaration.JE_MergeBy != defaultJeMergeBy;

				if (checkMergeByRequired && CheckSecurityRightForCheckpointRequired(Env.Security.MergeByDefault))
				{
					AddMessageLog(Env.Security.MergeByDefault, string.Format(CultureInfo.InvariantCulture, Constants.MergeBy, defaultJeMergeBy, declaration.JE_MergeBy));
				}
			}
		}

		protected virtual ZBool CheckSecurityRightForCheckpointRequired(SecurityCheckpoint checkpoint) => true;

		#endregion

		public void LogSupervisorActions(Logs logs)
		{
			foreach (MessageLog messageLog in UnAuthorisedMessagesForLog)
			{
				var log = logs.AddNew(Events.Authorised, messageLog.Message.Left(StmALog.Schema.SL_ReferenceMaxLength));
				log.SL_GS_NKUser = SupervisorName;
			}
		}

		public void LogAuthorisedActions(Logs logs)
		{
			foreach (MessageLog messageLog in AuthorisedMessagesForLog)
			{
				var log = logs.AddNew(Events.Authorised, messageLog.Message.Left(StmALog.Schema.SL_ReferenceMaxLength));
				log.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
			}
		}

		protected void AddMessageLog(SecurityCheckpoint checkpoint, string message)
		{
			if (checkpoint.IsAllowed)
			{
				AuthorisedMessagesForLog.Add(new MessageLog(message, checkpoint.Code));
			}
			else
			{
				UnAuthorisedMessagesForLog.Add(new MessageLog(message, checkpoint.Code));
			}
		}

		protected ZBool AnyStaffOrGroupHasSecurityRight(string code)
		{
			return Factory.GetCachedValue<ZBool>("AnyStaffOrGroupHasSecurityRight:" + code, delegate
			{
				var branchpks = from GlbBranch branch in GlbCompany.CurrentCompany.ActiveBranches select branch.PK;
				var result = branchpks.Any();
				if (result)
				{
					var matchingBranchPKsString = string.Join(", ", branchpks.Select(x => x.ToSqlGuid()));
					var collection = new DynamicBusinessObjectCollection(Factory);
					collection.Load(System.FormattableString.Invariant($@"SELECT {GlbStaffSchema.Constants.PK}, {GlbStaffSchema.Constants.GS_IsController}
FROM {GlbStaffSchema.Constants.SqlSchemaName}.{GlbStaffSchema.Constants.TableName}
WHERE {GlbStaffSchema.Constants.GS_IsActive} = 1
AND {GlbStaffSchema.Constants.GS_GB_HomeBranch} IN ({matchingBranchPKsString})"));
					var staffs = collection.Cast<DynamicBusinessObject>().Select(x =>
					{
						var gs_PK = (ZGuid)x[GlbStaffSchema.PK];
						var gs_IsController = (ZBool)x[GlbStaffSchema.GS_IsController];
						return (gs_PK, gs_IsController);
					}).ToArray();
					result = staffs.Any(x => x.gs_IsController) || StaffsHaveSecurityRight(code, staffs.Select(x => x.gs_PK).ToArray());
				}
				return result;
			});
		}

		public ZBool StaffsHaveSecurityRight(string code, ZGuid[] staffPKs)
		{
			var securityCollection = new GlbSecurityCollection(Factory);
			var groupLinkQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GG);
			groupLinkQuery.AddToFilter(GlbGroupLinkSchema.GK_GS, staffPKs);
			var groupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.PK);
			groupSubQuery.AddToFilter(GlbGroupSchema.GG_IsActive, ZBool.True);
			groupLinkQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, groupSubQuery, JoinCondition.And);
			var query = new ZDBOnlyQuery(typeof(GlbSecurity));
			query.AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GS, staffPKs);
			query.AddSubQuery(GlbSecuritySchema.GU_GG, groupLinkQuery, JoinCondition.Or);
			securityCollection.Load(query);

			var securityCalculator = new SecurityCalculatorForStaff(Factory, securityCollection,
				Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffPKs)));
			var checkpoint = Env.Security.FindCheckPoint(code);
			return checkpoint != null && staffPKs.Any(x => securityCalculator.IsRightAllowed(checkpoint, x.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid()));
		}
	}
}
