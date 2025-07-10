using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Accounting.Netting
{
	public class NettingImportInitiator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public NettingImportInitiator(BusinessObjectFactory factory) : base(factory)
		{
		}

		public NettingImportInitiator(BusinessObjectFactory factory, ZString nettingSystemCode, ZString ledger, ZString paymentStatus, ZDateTime startDate, ZDateTime endDate, bool includeNettingCentreTransactions)
			: this(factory)
		{
			NettingSystemCode = nettingSystemCode;
			Ledger = ledger;
			SettlementStatus = paymentStatus;
			StartDate = startDate;
			EndDate = endDate;
			IncludeTransactionsForNettingCentre = includeNettingCentreTransactions;
		}

		#region NettingSystemCode

		[MaxLength(10)]
		[List(nameof(NettingSystemCodes))]
		public ZString NettingSystemCode
		{
			get { return nettingSystemCode; }
			set
			{
				if (value != nettingSystemCode)
				{
					SetNonPersistentPropertyValue(NettingSystemCodeInfo, ref nettingSystemCode, value);
					TrailLog = GetTrailLog();
				}

				ValidateNettingSystemCode();
			}
		}
		ZString nettingSystemCode;

		void ValidateNettingSystemCode()
		{
			NettingSystemCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(NettingSystemCodeInfo, NettingSystemCodes, ResString.GetMultilingualString("C601C4A0-DA2F-4EE8-AD10-CC369F9DC2F1", "Netting System"));
		}

		public CodeDescriptionPairList NettingSystemCodes
		{
			get
			{
				if (nettingSystemCodes == null)
				{
					nettingSystemCodes = new CodeDescriptionPairList();
					foreach (var nettingSystem in NettingSystems)
					{
						nettingSystemCodes.AddPair(nettingSystem.NS_Code, nettingSystem.NS_Description);
					}
				}

				return nettingSystemCodes;
			}
		}
		CodeDescriptionPairList nettingSystemCodes;

		public ZPropertyInfo NettingSystemCodeInfo => GetZPropertyInfo(nameof(NettingSystemCode));

		NettingSystem[] NettingSystems => Factory.GetCachedValue("NettingImportInitiator_NettingSystems" + this.PK.ToStringKey()
			, () => Factory.Load<NettingSystem>(new ZQuery()));
		NettingSystem NettingSystem => Factory.LoadTop1<NettingSystem>(new ZQuery(NettingSystemSchema.NS_Code, NettingSystemCode));

		#endregion

		#region Ledger

		[List(nameof(LedgerTypes))]
		public ZString Ledger
		{
			get { return ledger; }
			set
			{
				if (value != ledger)
				{
					SetNonPersistentPropertyValue(LedgerInfo, ref ledger, value);
				}

				ValidateLedger();
			}
		}
		ZString ledger;

		public CodeDescriptionPairList LedgerTypes
		{
			get
			{
				if (ledgerTypes == null)
				{
					ledgerTypes = new CodeDescriptionPairList();
					ledgerTypes.AddPair("BTH", Res.GetString("0B86F503-9CA7-499D-BCC4-F468976762DF", "Both"));
					ledgerTypes.AddPair(ZArchitecture.Core.LedgerTypes.AccountsPayable, Res.GetString("CFE2014C-1665-4671-A845-2CF03BCB5AB8", "Accounts Payable"));
					ledgerTypes.AddPair(ZArchitecture.Core.LedgerTypes.AccountsReceivable, Res.GetString("6A3ACE99-1B1B-4627-8116-53A21A6C6CD3", "Accounts Receivable"));
				}
				return ledgerTypes;
			}
		}
		CodeDescriptionPairList ledgerTypes;

		public ZPropertyInfo LedgerInfo => GetZPropertyInfo(nameof(Ledger));

		void ValidateLedger()
		{
			LedgerInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(LedgerInfo, LedgerTypes, ResString.GetMultilingualString("E7C32ADC-FCB9-46D6-8385-1873EF96F868", "Ledger"));
		}
		#endregion

		#region SettlementStatus
		[List(nameof(SettlementStatusList))]
		public ZString SettlementStatus
		{
			get { return settlementStatus; }
			set
			{
				if (value != settlementStatus)
				{
					SetNonPersistentPropertyValue(SettlementStatusInfo, ref settlementStatus, value);
				}

				ValidateSettlementStatus();
			}
		}
		ZString settlementStatus;
		public CodeDescriptionPairList SettlementStatusList
		{
			get
			{
				if (settlementStatusList == null)
				{
					settlementStatusList = new CodeDescriptionPairList();
					settlementStatusList.AddPair(ALL, Res.GetString("FF4E0602-3B7E-4654-9740-0EB07919889A", "ALL Transactions"));
					settlementStatusList.AddPair(Paid, Res.GetString("860134B3-B314-4010-9343-1BE83148D51F", "Only include Fully paid Transactions for Netting"));
					settlementStatusList.AddPair(Unpaid, Res.GetString("4A1F74AB-0C73-4A30-8FC4-5B76BCBC3446", "Only include Unpaid Transactions for Netting"));
				}

				return settlementStatusList;
			}
		}
		CodeDescriptionPairList settlementStatusList;

		public ZPropertyInfo SettlementStatusInfo => GetZPropertyInfo(nameof(SettlementStatus));

		void ValidateSettlementStatus()
		{
			SettlementStatusInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(SettlementStatusInfo, SettlementStatusList, ResString.GetMultilingualString("FA4B6749-1E1A-40B6-BC9B-0DACCE20D776", "Payment Status"));
		}

		#endregion

		#region Start Date
		public ZDateTime StartDate
		{
			get { return startDate; }
			set
			{
				if (value != startDate)
				{
					SetNonPersistentPropertyValue(StartDateInfo, ref startDate, value);
				}

				ValidateStartDate();
			}
		}
		ZDateTime startDate = new DateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1);
		public ZPropertyInfo StartDateInfo => GetZPropertyInfo(nameof(StartDate));
		void ValidateStartDate()
		{
			StartDateInfo.ClearAllNotifications();
			if (StartDate.IsEmpty)
			{
				StartDateInfo.AddError(Res.GetString("DB8523E5-C268-4F05-B3C1-4E7C839B3B31", "Please pick an Import Start Date"));
			}
		}
		#endregion

		#region End Date
		public ZDateTime EndDate
		{
			get { return endDate; }
			set
			{
				if (value != endDate)
				{
					SetNonPersistentPropertyValue(EndDateInfo, ref endDate, value);
				}

				ValidateEndDate();
			}
		}
		ZDateTime endDate = new DateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, DateTime.DaysInMonth(ZDateTime.Now.Year, ZDateTime.Now.Month));
		public ZPropertyInfo EndDateInfo => GetZPropertyInfo(nameof(EndDate));
		void ValidateEndDate()
		{
			EndDateInfo.ClearAllNotifications();
			if (EndDate.IsEmpty)
			{
				EndDateInfo.AddError(Res.GetString("BA51799A-90AF-475C-AAEE-68AAB7934E8A", "Please pick an Import End Date"));
			}
		}
		#endregion

		public ZBool IncludeTransactionsForNettingCentre { get; set; }

		public string TrailLog { get; set; }

		public ZInt NumberOfQueuedTransactions { get; private set; }

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateNettingSystemCode();
			ValidateLedger();
			ValidateSettlementStatus();
			ValidateStartDate();
			ValidateEndDate();
		}

		public int QueueTransactionsForImport()
		{
			int totalTransactionCount = 0;

			var collection = new DynamicBusinessObjectCollection(Factory);
			var parameters = new ZSqlParameterCollection();

			string sql = @"SELECT
								GC_OH_OrgProxy
							FROM
								dbo.NettingSystem 
								INNER JOIN dbo.GlbCompany ON NS_GC = GC_PK
							WHERE
								NS_Code = @NS_Code";
			parameters.Add("@NS_Code", NettingSystemCode, NettingSystemSchema.NS_Code);
			collection.Load(sql, parameters);

			if (collection.Count == 1)
			{
				var orgHeader = Factory.Load<OrgHeader>((ZGuid)collection[0][GlbCompanySchema.Constants.GC_OH_OrgProxy]);
				if (orgHeader != null)
				{
					var communicationModes = orgHeader.EDICommunicationsModes.FindByModuleAndFileFormat(EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction);

					List<ZString> participatingCompanies = new List<ZString>();
					foreach (var comm in communicationModes)
					{
						participatingCompanies.Add(comm.EK_Destination.Substring(3, 3));
					}

					if (IncludeTransactionsForNettingCentre)
					{
						if (orgHeader.IsProxyOrgOfAnyCompany(false))
						{
							participatingCompanies.AddRange(orgHeader.CompanyProxies(false).Select(x => x.GC_Code));
						}
					}

					if (participatingCompanies.Count > 1)
					{
						foreach (var sourceCompany in participatingCompanies)
						{
							foreach (var targeCompany in participatingCompanies)
							{
								if (sourceCompany != targeCompany)
								{
									if (Ledger == "BTH")
									{
										totalTransactionCount += QueueTransactions(sourceCompany, targeCompany, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
										totalTransactionCount += QueueTransactions(sourceCompany, targeCompany, ZArchitecture.Core.LedgerTypes.AccountsPayable);
									}
									else
									{
										totalTransactionCount += QueueTransactions(sourceCompany, targeCompany, Ledger);
									}
								}
							}
						}
					}

					NumberOfQueuedTransactions = totalTransactionCount;
					AddLog(string.Format(CultureInfo.InvariantCulture, Res.GetString("0CDB5AA1-240A-4D32-82E0-64C920618BD4", @"{7}- User: '{0}', Ledger: '{1}', Payment Status: '{2}', Dates From: '{3}' To: '{4}', NC part of Netting: '{5}', Total Queued Transaction(s): {6}"
						, GlbStaff.CurrentUser.GS_Code
						, Ledger
						, SettlementStatus
						, StartDate.ToShortDateString()
						, EndDate.ToShortDateString()
						, IncludeTransactionsForNettingCentre ? 'Y' : 'N'
						, totalTransactionCount.ToString(CultureInfo.InvariantCulture)
						, ZDateTime.Now.ToStandardDateTimeString())));
				}
			}

			return totalTransactionCount;
		}

		void AddLog(string data)
		{
			var log = Factory.New<StmALog>();
			using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
			{
				log.SL_Parent = NettingSystem.PK;
				log.SL_Table = NettingSystemSchema.Constants.TableName;
				log.SL_SE_NKEvent = NettingImportLogEventCode;
				log.SL_Reference = data;
				log.SL_EventTime = ZDateTime.Now;
			}

			Factory.Save();
		}

		int QueueTransactions(ZString sourceCompany, ZString targeCompany, ZString ledgerType)
		{
			using (var cmd = Db.Connection.Command("QueueTransactionsForNetting"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@SourceCompanyCode", SqlDbType.VarChar, sourceCompany.ToString());
				cmd.AddParameter("@TargetCompanyCode", SqlDbType.VarChar, targeCompany.ToString());
				cmd.AddParameter("@Ledger", SqlDbType.VarChar, ledgerType.ToString());
				cmd.AddParameter("@ImportStartDate", SqlDbType.DateTime, StartDate.ToDateTime());
				cmd.AddParameter("@ImportEndDate", SqlDbType.DateTime, EndDate.ToDateTime());
				cmd.AddParameter("@PaymentStatus", SqlDbType.Char, SettlementStatus.ToString());

				return cmd.ExecuteProcedureWithReturnValue();
			}
		}

		string GetTrailLog()
		{
			var result = string.Empty;

			var logs = new DynamicBusinessObjectCollection(Factory);
			var parameters = new ZSqlParameterCollection();

			string sql = @"SELECT
								SL_Reference
							FROM
								dbo.StmALog
							WHERE
								SL_Parent = @NettingSystemPK
								AND SL_SE_NKEvent = @EventCode";

			parameters.Add("@NettingSystemPK", NettingSystem != null ? NettingSystem.PK.ToGuid() : Guid.Empty, NettingSystemSchema.PK);
			parameters.Add("@EventCode", NettingImportLogEventCode, StmALogSchema.SL_SE_NKEvent);

			logs.Load(sql, parameters);

			if (logs.Any())
			{
				var stringBuilder = new ZStringBuilder();
				foreach (DynamicBusinessObject log in logs)
				{
					stringBuilder.AppendLine((ZString)log[StmALogSchema.Constants.SL_Reference]);
				}

				result = stringBuilder.ToString();
			}
			else
			{
				result = Res.GetString("471F9825-ECDC-46D5-BB87-FE1F77BB0E78", "No transactions imported for this Netting System.");
			}

			return result;
		}

		readonly string NettingImportLogEventCode = AutoEvents.CustomisableEvent00Code;
		public const string Paid = "PAD";
		public const string Unpaid = "UPD";
		public const string ALL = "ALL";
	}
}
