using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using SectionTypes = Enterprise.MasterFiles.Business.AccGLHeader.Constants.SectionTypes;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderBulkUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AccGLHeaderBulkUpdater(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region Trading Statement Start Account

		public AccGLHeader TradingStatementStartAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_TradingStatementStartAccount); }
		}
		[List("GLHeadersTSStart")]
		public ZGuid AG_TradingStatementStartAccount
		{
			get { return fAG_TradingStatementStartAccount; }
			set
			{
				if (fAG_TradingStatementStartAccount != value)
				{
					fAG_TradingStatementStartAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_TradingStatementStartAccount();
				}

				AG_TradingStatementStartAccountInfo.RefreshBinding();
			}
		}

		ZGuid fAG_TradingStatementStartAccount = ZGuid.Empty;

		public ZPropertyInfo AG_TradingStatementStartAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_TradingStatementStartAccount)); }
		}

		void ValidateAG_TradingStatementStartAccount()
		{
			AG_TradingStatementStartAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_TradingStatementStartAccountInfo);
		}

		#endregion

		#region Trading Statement End Account

		public AccGLHeader TradingStatementEndAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_TradingStatementEndAccount); }
		}
		[List("GLHeadersTSEnd")]
		public ZGuid AG_TradingStatementEndAccount
		{
			get { return fAG_TradingStatementEndAccount; }
			set
			{
				if (fAG_TradingStatementEndAccount != value)
				{
					fAG_TradingStatementEndAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_TradingStatementEndAccount();
				}

				AG_TradingStatementEndAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_TradingStatementEndAccount = ZGuid.Empty;

		public ZPropertyInfo AG_TradingStatementEndAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_TradingStatementEndAccount)); }
		}

		void ValidateAG_TradingStatementEndAccount()
		{
			AG_TradingStatementEndAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_TradingStatementEndAccountInfo);
		}

		#endregion

		#region Overheads Start Account

		public AccGLHeader OverheadsStartAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_OverheadsStartAccount); }
		}
		[List("GLHeadersOVStart")]
		public ZGuid AG_OverheadsStartAccount
		{
			get { return fAG_OverheadsStartAccount; }
			set
			{
				if (fAG_OverheadsStartAccount != value)
				{
					fAG_OverheadsStartAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_OverheadsStartAccount();
				}

				AG_OverheadsStartAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_OverheadsStartAccount = ZGuid.Empty;

		public ZPropertyInfo AG_OverheadsStartAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_OverheadsStartAccount)); }
		}

		void ValidateAG_OverheadsStartAccount()
		{
			AG_OverheadsStartAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_OverheadsStartAccountInfo);
		}

		#endregion

		#region Overheads End Account

		public AccGLHeader OverheadsEndAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_OverheadsEndAccount); }
		}
		[List("GLHeadersOVEnd")]
		public ZGuid AG_OverheadsEndAccount
		{
			get { return fAG_OverheadsEndAccount; }
			set
			{
				if (fAG_OverheadsEndAccount != value)
				{
					fAG_OverheadsEndAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_OverheadsEndAccount();
				}

				AG_OverheadsEndAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_OverheadsEndAccount = ZGuid.Empty;

		public ZPropertyInfo AG_OverheadsEndAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_OverheadsEndAccount)); }
		}

		void ValidateAG_OverheadsEndAccount()
		{
			AG_OverheadsEndAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_OverheadsEndAccountInfo);
		}

		#endregion

		#region ProfitAndLossAppropriation Start Account

		public AccGLHeader ProfitAndLossAppropriationStartAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_ProfitAndLossAppropriationStartAccount); }
		}
		[List("GLHeadersAPStart")]
		public ZGuid AG_ProfitAndLossAppropriationStartAccount
		{
			get { return fAG_ProfitAndLossAppropriationStartAccount; }
			set
			{
				if (fAG_ProfitAndLossAppropriationStartAccount != value)
				{
					fAG_ProfitAndLossAppropriationStartAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_ProfitAndLossAppropriationStartAccount();
				}

				AG_ProfitAndLossAppropriationStartAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_ProfitAndLossAppropriationStartAccount = ZGuid.Empty;

		public ZPropertyInfo AG_ProfitAndLossAppropriationStartAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_ProfitAndLossAppropriationStartAccount)); }
		}

		void ValidateAG_ProfitAndLossAppropriationStartAccount()
		{
			AG_ProfitAndLossAppropriationStartAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_ProfitAndLossAppropriationStartAccountInfo);
		}

		#endregion

		#region ProfitAndLossAppropriation End Account

		public AccGLHeader ProfitAndLossAppropriationEndAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_ProfitAndLossAppropriationEndAccount); }
		}

		[List("GLHeadersAPEnd")]
		public ZGuid AG_ProfitAndLossAppropriationEndAccount
		{
			get { return fAG_ProfitAndLossAppropriationEndAccount; }
			set
			{
				if (fAG_ProfitAndLossAppropriationEndAccount != value)
				{
					fAG_ProfitAndLossAppropriationEndAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_ProfitAndLossAppropriationEndAccount();
				}

				AG_ProfitAndLossAppropriationEndAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_ProfitAndLossAppropriationEndAccount = ZGuid.Empty;

		public ZPropertyInfo AG_ProfitAndLossAppropriationEndAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_ProfitAndLossAppropriationEndAccount)); }
		}

		void ValidateAG_ProfitAndLossAppropriationEndAccount()
		{
			AG_ProfitAndLossAppropriationEndAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_ProfitAndLossAppropriationEndAccountInfo);
		}

		#endregion

		#region OwnersEquity Start Account

		public AccGLHeader OwnersEquityStartAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_OwnersEquityStartAccount); }
		}
		[List("GLHeadersOEStart")]
		public ZGuid AG_OwnersEquityStartAccount
		{
			get { return fAG_OwnersEquityStartAccount; }
			set
			{
				if (fAG_OwnersEquityStartAccount != value)
				{
					fAG_OwnersEquityStartAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_OwnersEquityStartAccount();
				}

				AG_OwnersEquityStartAccountInfo.RefreshBinding();
			}
		}

		ZGuid fAG_OwnersEquityStartAccount = ZGuid.Empty;

		public ZPropertyInfo AG_OwnersEquityStartAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_OwnersEquityStartAccount)); }
		}

		void ValidateAG_OwnersEquityStartAccount()
		{
			AG_OwnersEquityStartAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_OwnersEquityStartAccountInfo);
		}

		#endregion

		#region OwnersEquity End Account

		public AccGLHeader OwnersEquityEndAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_OwnersEquityEndAccount); }
		}
		[List("GLHeadersOEEnd")]
		public ZGuid AG_OwnersEquityEndAccount
		{
			get { return fAG_OwnersEquityEndAccount; }
			set
			{
				if (fAG_OwnersEquityEndAccount != value)
				{
					fAG_OwnersEquityEndAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_OwnersEquityEndAccount();
				}

				AG_OwnersEquityEndAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_OwnersEquityEndAccount = ZGuid.Empty;

		public ZPropertyInfo AG_OwnersEquityEndAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_OwnersEquityEndAccount)); }
		}

		void ValidateAG_OwnersEquityEndAccount()
		{
			AG_OwnersEquityEndAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_OwnersEquityEndAccountInfo);
		}

		#endregion

		#region Liabilities Start Account

		public AccGLHeader LiabilitiesStartAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_LiabilitiesStartAccount); }
		}
		[List("GLHeadersLIStart")]
		public ZGuid AG_LiabilitiesStartAccount
		{
			get { return fAG_LiabilitiesStartAccount; }
			set
			{
				if (fAG_LiabilitiesStartAccount != value)
				{
					fAG_LiabilitiesStartAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_LiabilitiesStartAccount();
				}

				AG_LiabilitiesStartAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_LiabilitiesStartAccount = ZGuid.Empty;

		public ZPropertyInfo AG_LiabilitiesStartAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_LiabilitiesStartAccount)); }
		}

		void ValidateAG_LiabilitiesStartAccount()
		{
			AG_LiabilitiesStartAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_LiabilitiesStartAccountInfo);
		}

		#endregion

		#region Liabilities End Account

		public AccGLHeader LiabilitiesEndAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_LiabilitiesEndAccount); }
		}
		[List("GLHeadersLIEnd")]
		public ZGuid AG_LiabilitiesEndAccount
		{
			get { return fAG_LiabilitiesEndAccount; }
			set
			{
				if (fAG_LiabilitiesEndAccount != value)
				{
					fAG_LiabilitiesEndAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					// ValidateAG_LiabilitiesEndAccount();
				}

				AG_LiabilitiesEndAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_LiabilitiesEndAccount = ZGuid.Empty;

		public ZPropertyInfo AG_LiabilitiesEndAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_LiabilitiesEndAccount)); }
		}

		void ValidateAG_LiabilitiesEndAccount()
		{
			AG_LiabilitiesEndAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_LiabilitiesEndAccountInfo);
		}

		#endregion

		#region Assets Start Account

		public AccGLHeader AssetsStartAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_AssetsStartAccount); }
		}
		[List("GLHeadersASStart")]
		public ZGuid AG_AssetsStartAccount
		{
			get { return fAG_AssetsStartAccount; }
			set
			{
				if (fAG_AssetsStartAccount != value)
				{
					fAG_AssetsStartAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					// ValidateAG_AssetsStartAccount();
				}

				AG_AssetsStartAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_AssetsStartAccount = ZGuid.Empty;

		public ZPropertyInfo AG_AssetsStartAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_AssetsStartAccount)); }
		}

		void ValidateAG_AssetsStartAccount()
		{
			AG_AssetsStartAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_AssetsStartAccountInfo);
		}

		#endregion

		#region Assets End Account

		public AccGLHeader AssetsEndAccount
		{
			get { return Factory.Load<AccGLHeader>(AG_AssetsEndAccount); }
		}
		[List("GLHeadersASEnd")]
		public ZGuid AG_AssetsEndAccount
		{
			get { return fAG_AssetsEndAccount; }
			set
			{
				if (fAG_AssetsEndAccount != value)
				{
					fAG_AssetsEndAccount = value;

					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
					//ValidateAG_AssetsEndAccount();
				}

				AG_AssetsEndAccountInfo.RefreshBinding();
			}
		}
		ZGuid fAG_AssetsEndAccount = ZGuid.Empty;

		public ZPropertyInfo AG_AssetsEndAccountInfo
		{
			get { return GetZPropertyInfo(nameof(AG_AssetsEndAccount)); }
		}

		void ValidateAG_AssetsEndAccount()
		{
			AG_AssetsEndAccountInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(AG_AssetsEndAccountInfo);
		}

		#endregion

		#endregion

		#region Lookups

		public AccGLHeaderCollection GLHeadersTSStart
		{
			get
			{
				if (fGLHeadersTSStart == null)
				{
					fGLHeadersTSStart = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersTSStart;
			}
		}
		AccGLHeaderCollection fGLHeadersTSStart;

		public AccGLHeaderCollection GLHeadersTSEnd
		{
			get
			{
				if (fGLHeadersTSEnd == null)
				{
					fGLHeadersTSEnd = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersTSEnd;
			}
		}
		AccGLHeaderCollection fGLHeadersTSEnd;

		public AccGLHeaderCollection GLHeadersOVStart
		{
			get
			{
				if (fGLHeadersOVStart == null)
				{
					fGLHeadersOVStart = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersOVStart;
			}
		}
		AccGLHeaderCollection fGLHeadersOVStart;

		public AccGLHeaderCollection GLHeadersOVEnd
		{
			get
			{
				if (fGLHeadersOVEnd == null)
				{
					fGLHeadersOVEnd = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersOVEnd;
			}
		}
		AccGLHeaderCollection fGLHeadersOVEnd;

		public AccGLHeaderCollection GLHeadersAPStart
		{
			get
			{
				if (fGLHeadersAPStart == null)
				{
					fGLHeadersAPStart = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersAPStart;
			}
		}
		AccGLHeaderCollection fGLHeadersAPStart;

		public AccGLHeaderCollection GLHeadersAPEnd
		{
			get
			{
				if (fGLHeadersAPEnd == null)
				{
					fGLHeadersAPEnd = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersAPEnd;
			}
		}
		AccGLHeaderCollection fGLHeadersAPEnd;

		public AccGLHeaderCollection GLHeadersOEStart
		{
			get
			{
				if (fGLHeadersOEStart == null)
				{
					fGLHeadersOEStart = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersOEStart;
			}
		}
		AccGLHeaderCollection fGLHeadersOEStart;

		public AccGLHeaderCollection GLHeadersOEEnd
		{
			get
			{
				if (fGLHeadersOEEnd == null)
				{
					fGLHeadersOEEnd = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersOEEnd;
			}
		}
		AccGLHeaderCollection fGLHeadersOEEnd;

		public AccGLHeaderCollection GLHeadersLIStart
		{
			get
			{
				if (fGLHeadersLIStart == null)
				{
					fGLHeadersLIStart = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersLIStart;
			}
		}
		AccGLHeaderCollection fGLHeadersLIStart;

		public AccGLHeaderCollection GLHeadersLIEnd
		{
			get
			{
				if (fGLHeadersLIEnd == null)
				{
					fGLHeadersLIEnd = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersLIEnd;
			}
		}
		AccGLHeaderCollection fGLHeadersLIEnd;

		public AccGLHeaderCollection GLHeadersASStart
		{
			get
			{
				if (fGLHeadersASStart == null)
				{
					fGLHeadersASStart = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersASStart;
			}
		}
		AccGLHeaderCollection fGLHeadersASStart;

		public AccGLHeaderCollection GLHeadersASEnd
		{
			get
			{
				if (fGLHeadersASEnd == null)
				{
					fGLHeadersASEnd = new AccGLHeaderCollection(Factory);
				}

				return fGLHeadersASEnd;
			}
		}
		AccGLHeaderCollection fGLHeadersASEnd;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateAG_TradingStatementStartAccount();
			ValidateAG_TradingStatementEndAccount();

			ValidateAG_OverheadsStartAccount();
			ValidateAG_OverheadsEndAccount();

			ValidateAG_ProfitAndLossAppropriationStartAccount();
			ValidateAG_ProfitAndLossAppropriationEndAccount();

			ValidateAG_OwnersEquityStartAccount();
			ValidateAG_OwnersEquityEndAccount();

			ValidateAG_AssetsStartAccount();
			ValidateAG_AssetsEndAccount();

			ValidateAG_LiabilitiesStartAccount();
			ValidateAG_LiabilitiesEndAccount();

			ValidateSection(AG_TradingStatementStartAccountInfo, AG_TradingStatementEndAccountInfo, TradingStatementStartAccount, TradingStatementEndAccount);

			ValidateSection(AG_OverheadsStartAccountInfo, AG_OverheadsEndAccountInfo, OverheadsStartAccount, OverheadsEndAccount);

			ValidateSection(AG_ProfitAndLossAppropriationStartAccountInfo, AG_ProfitAndLossAppropriationEndAccountInfo, ProfitAndLossAppropriationStartAccount, ProfitAndLossAppropriationEndAccount);

			ValidateSection(AG_OwnersEquityStartAccountInfo, AG_OwnersEquityEndAccountInfo, OwnersEquityStartAccount, OwnersEquityEndAccount);

			ValidateSection(AG_LiabilitiesStartAccountInfo, AG_LiabilitiesEndAccountInfo, LiabilitiesStartAccount, LiabilitiesEndAccount);

			ValidateSection(AG_AssetsStartAccountInfo, AG_AssetsEndAccountInfo, AssetsStartAccount, AssetsEndAccount);

			if (NoProfitAndLossSectionsDefined)
			{
				ZString message = ErrorMessageForAtLeastOneProfitAndLossSection;
				AG_TradingStatementStartAccountInfo.AddError(message);
				AG_OverheadsStartAccountInfo.AddError(message);
				AG_ProfitAndLossAppropriationStartAccountInfo.AddError(message);
			}

			if (NoBalanceSheetSectionsDefined)
			{
				ZString message = ErrorMessageForAtLeastOneBalanceSheetSection;
				AG_OwnersEquityStartAccountInfo.AddError(message);
				AG_AssetsStartAccountInfo.AddError(message);
				AG_LiabilitiesStartAccountInfo.AddError(message);
			}

			List<ZString> messages = CheckAllGLAccountsBelongToOnlyOneSection();

			foreach (ZString message in messages)
			{
				AG_TradingStatementStartAccountInfo.AddError(message);
				AG_OverheadsStartAccountInfo.AddError(message);
				AG_ProfitAndLossAppropriationStartAccountInfo.AddError(message);
				AG_OwnersEquityStartAccountInfo.AddError(message);
				AG_AssetsStartAccountInfo.AddError(message);
				AG_LiabilitiesStartAccountInfo.AddError(message);
			}
		}

		void ValidateSection(ZPropertyInfo startInfo, ZPropertyInfo endInfo, AccGLHeader start, AccGLHeader end)
		{
			// StartInfo.ClearAllNotifications();
			// EndInfo.ClearAllNotifications();

			if (start != null && end != null)
			{
				if (String.Compare(start.AG_AccountNum, end.AG_AccountNum) > 0)
				{
					endInfo.AddError(ErrorMessageForEndDateGreaterThanStartDate);
				}
			}
			else if (start != null && end == null)
			{
				endInfo.AddError(ErrorMessageForStartAndEndAccountsAreMandatory);
			}
			else if (start == null && end != null)
			{
				startInfo.AddError(ErrorMessageForStartAndEndAccountsAreMandatory);
			}
		}

		bool NoProfitAndLossSectionsDefined
		{
			get
			{
				return AG_TradingStatementStartAccount.IsEmpty
					&& AG_OverheadsStartAccount.IsEmpty
					&& AG_ProfitAndLossAppropriationStartAccount.IsEmpty;
			}
		}

		bool NoBalanceSheetSectionsDefined
		{
			get
			{
				return AG_OwnersEquityStartAccount.IsEmpty
					&& AG_AssetsStartAccount.IsEmpty
					&& AG_LiabilitiesStartAccount.IsEmpty;
			}
		}

		List<ZString> CheckAllGLAccountsBelongToOnlyOneSection()
		{
			AccGLHeaderCollection allHeaders = new AccGLHeaderCollection(Factory);
			allHeaders.Load();

			Dictionary<ZGuid, ZStringBuilder> headerCount = new Dictionary<ZGuid, ZStringBuilder>();

			foreach (AccGLHeader header in allHeaders)
			{
				headerCount.Add(header.PK, new ZStringBuilder());

				if (IsInSection(header.AG_AccountNum, TradingStatementStartAccount, TradingStatementEndAccount))
				{
					headerCount[header.PK].Append(SectionTypes.Codes.TradingStatement);
				}

				if (IsInSection(header.AG_AccountNum, OverheadsStartAccount, OverheadsEndAccount))
				{
					headerCount[header.PK].Append(SectionTypes.Codes.Overheads);
				}

				if (IsInSection(header.AG_AccountNum, ProfitAndLossAppropriationStartAccount, ProfitAndLossAppropriationEndAccount))
				{
					headerCount[header.PK].Append(SectionTypes.Codes.ProfitAndLossAppropriation);
				}

				if (IsInSection(header.AG_AccountNum, OwnersEquityStartAccount, OwnersEquityEndAccount))
				{
					headerCount[header.PK].Append(SectionTypes.Codes.OwnersEquity);
				}

				if (IsInSection(header.AG_AccountNum, AssetsStartAccount, AssetsEndAccount))
				{
					headerCount[header.PK].Append(SectionTypes.Codes.Assets);
				}

				if (IsInSection(header.AG_AccountNum, LiabilitiesStartAccount, LiabilitiesEndAccount))
				{
					headerCount[header.PK].Append(SectionTypes.Codes.Liabilities);
				}
			}

			bool thereAreHeadersWithoutASection = false;
			foreach (AccGLHeader header in allHeaders)
			{
				if (headerCount[header.PK].Length == 0)
				{
					thereAreHeadersWithoutASection = true;
					break;
				}
			}

			bool thereAreHeadersInOverlappingSections = false;

			foreach (AccGLHeader header in allHeaders)
			{
				if (headerCount[header.PK].Length > 2)
				{
					thereAreHeadersInOverlappingSections = true;
					break;
				}
			}

			List<ZString> messages = new List<ZString>();

			if (thereAreHeadersWithoutASection)
			{
				messages.Add(ErrorMessageForSectionsDontCoverAllHeaders);
			}

			if (thereAreHeadersInOverlappingSections)
			{
				messages.Add(ErrorMessageForOverlappingSections);
			}

			return messages;
		}

		public static string ErrorMessageForEndDateGreaterThanStartDate
		{
			get { return Res.GetString("528acda2-b269-4568-a872-0fac4b345e8e", "The 'End' account must be greater than the 'Start' account"); }
		}
		public static string ErrorMessageForStartAndEndAccountsAreMandatory
		{
			get { return Res.GetString("fa9c1acc-42f2-4af5-abef-01da07bb6167", "You must nominate both start and end accounts for this section"); }
		}

		public static string ErrorMessageForOverlappingSections
		{
			get { return Res.GetString("b39930cd-cb97-4d1a-a1a3-7e9a4a17c6cc", "The sections cannot overlap. Please ensure the each GL Account is one and only one section."); }
		}
		public static string ErrorMessageForSectionsDontCoverAllHeaders
		{
			get { return Res.GetString("23774140-4606-4344-8a60-ac0496d32d6e", "The sections must cover all GL Headers. Please ensure all GL Accounts are included in a section."); }
		}

		public static string ErrorMessageForAtLeastOneProfitAndLossSection
		{
			get { return Res.GetString("00747f0c-3a61-4bec-901a-54bb4fa6cc18", "You must nominate at least one section in the Profit & Loss"); }
		}
		public static string ErrorMessageForAtLeastOneBalanceSheetSection
		{
			get { return Res.GetString("4633f9bc-22d1-4147-93ef-e78798f41a00", "You must nominate at least one section in the Balance Sheet"); }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AG_TradingStatementStartAccount = GetStartOfSection(SectionTypes.Codes.TradingStatement);
			AG_TradingStatementEndAccount = GetEndOfSection(SectionTypes.Codes.TradingStatement);

			AG_OverheadsStartAccount = GetStartOfSection(SectionTypes.Codes.Overheads);
			AG_OverheadsEndAccount = GetEndOfSection(SectionTypes.Codes.Overheads);

			AG_ProfitAndLossAppropriationStartAccount = GetStartOfSection(SectionTypes.Codes.ProfitAndLossAppropriation);
			AG_ProfitAndLossAppropriationEndAccount = GetEndOfSection(SectionTypes.Codes.ProfitAndLossAppropriation);

			AG_OwnersEquityStartAccount = GetStartOfSection(SectionTypes.Codes.OwnersEquity);
			AG_OwnersEquityEndAccount = GetEndOfSection(SectionTypes.Codes.OwnersEquity);

			AG_AssetsStartAccount = GetStartOfSection(SectionTypes.Codes.Assets);
			AG_AssetsEndAccount = GetEndOfSection(SectionTypes.Codes.Assets);

			AG_LiabilitiesStartAccount = GetStartOfSection(SectionTypes.Codes.Liabilities);
			AG_LiabilitiesEndAccount = GetEndOfSection(SectionTypes.Codes.Liabilities);
		}

		ZGuid GetStartOfSection(string sectionType)
		{
			ZQuery findGLHeaderQuery = new ZQuery(AccGLHeaderSchema.AG_Column, sectionType);
			AccGLHeaderCollection headers = new AccGLHeaderCollection(Factory, findGLHeaderQuery);
			headers.Load();

			ZString lowestAccountNumber = headers.Count >= 1 ? headers[0].AG_AccountNum : ZString.Empty;
			ZGuid lowestAccountPK = headers.Count >= 1 ? headers[0].PK : ZGuid.Empty;

			foreach (AccGLHeader header in headers)
			{
				if (string.Compare(header.AG_AccountNum, lowestAccountNumber) < 0)
				{
					lowestAccountNumber = header.AG_AccountNum;
					lowestAccountPK = header.PK;
				}
			}

			return lowestAccountPK;
		}

		ZGuid GetEndOfSection(string sectionType)
		{
			ZQuery findGLHeaderQuery = new ZQuery(AccGLHeaderSchema.AG_Column, sectionType);
			AccGLHeaderCollection headers = new AccGLHeaderCollection(Factory, findGLHeaderQuery);
			headers.Load();

			ZString highestAccountNumber = ZString.Empty;
			ZGuid highestAccountPK = ZGuid.Empty;

			foreach (AccGLHeader header in headers)
			{
				if (string.Compare(header.AG_AccountNum, highestAccountNumber) > 0)
				{
					highestAccountNumber = header.AG_AccountNum;
					highestAccountPK = header.PK;
				}
			}

			return highestAccountPK;
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			AccGLHeaderCollection headers = new AccGLHeaderCollection(Factory);
			headers.Load();

			foreach (AccGLHeader header in headers)
			{
				header.AG_Column = GetSectionForAccount(header.AG_AccountNum);
			}
		}

		ZString GetSectionForAccount(ZString accountNum)
		{
			ZString section = ZString.Empty;

			if (IsInSection(accountNum, TradingStatementStartAccount, TradingStatementEndAccount))
			{
				section = SectionTypes.Codes.TradingStatement;
			}
			else if (IsInSection(accountNum, OverheadsStartAccount, OverheadsEndAccount))
			{
				section = SectionTypes.Codes.Overheads;
			}
			else if (IsInSection(accountNum, ProfitAndLossAppropriationStartAccount, ProfitAndLossAppropriationEndAccount))
			{
				section = SectionTypes.Codes.ProfitAndLossAppropriation;
			}
			else if (IsInSection(accountNum, OwnersEquityStartAccount, OwnersEquityEndAccount))
			{
				section = SectionTypes.Codes.OwnersEquity;
			}
			else if (IsInSection(accountNum, AssetsStartAccount, AssetsEndAccount))
			{
				section = SectionTypes.Codes.Assets;
			}
			else if (IsInSection(accountNum, LiabilitiesStartAccount, LiabilitiesEndAccount))
			{
				section = SectionTypes.Codes.Liabilities;
			}

			return section;
		}

		bool IsInSection(ZString accountNum, AccGLHeader startAccount, AccGLHeader endAccount)
		{
			bool result = false;

			if (startAccount != null && endAccount != null)
			{
				ZString start = startAccount.AG_AccountNum;
				ZString end = endAccount.AG_AccountNum;
				result = (String.Compare(accountNum, start) >= 0) && (String.Compare(accountNum, end) <= 0);
			}

			return result;
		}
	}
}
