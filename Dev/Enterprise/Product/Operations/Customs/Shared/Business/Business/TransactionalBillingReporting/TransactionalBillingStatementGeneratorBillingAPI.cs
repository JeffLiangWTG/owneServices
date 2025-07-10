using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.TransactionalBillingReporting
{
	public class TransactionalBillingStatementGeneratorBillingAPI
	{
		public TransactionalBillingStatementGeneratorBillingAPI(Integration.ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}

		public void DoEverything()
		{
			var requirement = RunNowRequirement;
			if (requirement == WhatToDo.RunNow)
			{
				toDate = CalculateToDate();
				fromDate = CalculateFromDate();
				serviceLogger.Log(Integration.LogType.Information, "Running. Querying database for results.");
				GenerateAndSendReport();
				UpdateNextOrLastRunTime();
			}
			else
			{
				serviceLogger.Log(Integration.LogType.Information, "No need to do anything - " + requirement.ToString());
			}
		}

		protected virtual ZDateTime CalculateFromDate()
		{
			var start = CustomsDataRegistry.Instance.CustomsTransactionalBillingTaskLastRunDate_BillingAPI.Value;
			if (start == DateTime.MinValue)
			{
				return ZDateTime.MinSmallDateTimeValue;
			}
			return start;
		}

		protected virtual ZDateTime CalculateToDate()
		{
			return ZDateTime.UtcNow;
		}

		void GenerateAndSendReport()
		{
			var entriesReportCsv = MakeCsvReportFromSqlStatementWithDateRange(GetEntryNumberReportSelectStatement(), ConstantStrings.CustomsEntries);
			var awbsReportCsv = MakeCsvReportFromSqlStatementWithDateRange(GetAwbsReportSelectStatement(), ConstantStrings.AirWaybills);
			var genralMessageCsv = MakeCsvReportFromSqlStatementWithDateRange(GetGenralTextMessagesReportSelectStatement(), ConstantStrings.GENRALTextMessages);
			// Add new reports (with new select statements) here as needed:  var someOtherReport = new ReportDataAndName(MakeCsvTextFromSqlStatementWithDateRange(GetSomeOtherReportSqlStatement()), "Some Report Title");

			SendReportToCargoWise(entriesReportCsv, awbsReportCsv, genralMessageCsv);
		}

		protected virtual ReportDataAndName MakeCsvReportFromSqlStatementWithDateRange(string selectStatement, string reportName)
		{
			var columnNameHelper = GetColumnNameHelper(reportName);
			var transactions = new List<BillingTransaction>();
			var licenceKey = GetRegistrationLicenceKey();
			using (var command = Db.Connection.Command(selectStatement))
			{
				AddDataParameters(command);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var transaction = columnNameHelper.GetTransaction(reader, licenceKey);
						transactions.Add(transaction);
					}
				}
			}
			var report = new ReportDataAndName("", reportName, 0, transactions);
			return report;
		}

		ProductRegistrationSimple GetRegistrationLicenceKey()
		{
			IProductRegistrationKey registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			return new ProductRegistrationSimple(registrationKey.SystemId, registrationKey.EnterpriseCode, registrationKey.ServerCode);
		}

		ReportColumnHelper GetColumnNameHelper(string reportName)
		{
			switch (reportName)
			{
				case ConstantStrings.CustomsEntries:
					return new CustomsEntriesHelper();
				case ConstantStrings.AirWaybills:
					return new AirWaybillHelper();
				case ConstantStrings.GENRALTextMessages:
					return new GenralHelper();
				default:
					throw new NotSupportedException();
			}
		}

		protected void AddDataParameters(DbCommand command)
		{
			command.AddParameter("fromDate", System.Data.SqlDbType.DateTime, fromDate.ToDateTime());
			command.AddParameter("toDate", System.Data.SqlDbType.DateTime, toDate.ToDateTime());
		}

		string GetGenralTextMessagesReportSelectStatement()
		{
			return @"  
					select	
							case EM_MessageSubType 
								when 'TXT' then 'Text message (TXT)'
								when 'BCM' then 'Broadcast (BCM)'
								when 'FBK' then 'Fallback invocation/revocation (BCM)'  -- arrives as BCM, stored as FBK
							 end  as [Message Purpose],

							 em_receiveTransmit as [Direction],

							 EI_From as [Sender],

							 case	SUBSTRING(EI_From, 1, 6)
									when 'CUKSYS' then 'CCS-UK Database'
									when 'CUKCCS' then 'CCS-UK Helpdesk'
									when 'CUKCTM' then 'Customs'			
									when 'CUKAIR' then 'Shed '  + SUBSTRING(EI_From,9, 6)
									when 'CUKFFW' then 'Agent ' + SUBSTRING(EI_From,12, 3)
							end as [Sender Explained],

							EI_To as [Recipient],

							case	substring(EI_to, 1, 6)
									when 'CUKSYS' then 'CCS-UK Database'
									when 'CUKCCS' then 'CCS-UK Helpdesk'
									when 'CUKCTM' then 'Customs'			
									when 'CUKAIR' then 'Shed '  + SUBSTRING(EI_to,9, 6)
									when 'CUKFFW' then 'Agent ' + SUBSTRING(EI_to,12, 3)	
							end as  [Recipient Explained],

							em_messagenum as [Message#], 

							ei_InterchangeNum as [Interchange#], 

							GB_Code as [Branch], 

							GC_Code as [Company], 

							Em_SystemCreateTimeUtc  as [CreatedTime]  		

					from dbo.ediMessage  inner join dbo.ediInterchange   on ei_pk = em_ei
					inner join dbo.GlbBranch  on eM_GB = GB_PK
					inner join dbo.GlbCompany  on GB_GC = GC_PK
					where em_messagetype = 'gen' and em_applicationCode = 'cuk'
					and em_systemcreateTimeUtc between @fromDate and @toDate
					order by  em_systemcreateTimeUtc desc
					 ";
		}

		string GetAwbsReportSelectStatement()
		{
			var reportAwbsForCountriesOtherThanGB = false;

			#region Select statements

			// GB is separate from AU/NZ because it has a unqiue way of making use of a master house as a worker for a mawb/basic to provide all the details that are missing from the mawb schema (weights, pieces, description etc), so it needs to be considered carefully

			var sqlForGb = @"
							-- GB 
							-- BASICS  
							select 
								CM_MAWB as MAWB,
								'' as HAWB, 
								cm_applicationCode as [Application Code],
								GB_Code as [Branch], 
								GC_Code as [Company], 
								CM_SystemCreateTimeUtc as [Job Open Date],
								CM_SystemCreateUser as [Job Opening Staff] 
 							from dbo.CusMAWB 
								inner join dbo.CusHAWB  on CM_PK = CS_cm and CS_IsMasterHouse = 1
								inner join dbo.GlbBranch  on CM_GB = GB_PK
								inner join dbo.GlbCompany  on GB_GC = GC_PK
							where cm_applicationCode = 'CUK'
								and CM_PK not in (select CS_CM from dbo.CusHAWB where CS_IsMasterHouse = 0 and CS_CM is not null)   -- excldue mawbs who havew TRUE hoses are children, i.e. exclude mawbs that are consols
								and cm_IsCtoMawb = 0 -- Brendon pls confirm
								and CM_SystemCreateTimeUtc between @fromDate and @toDate

							union 

							-- HOUSES
							select 
								CM_MAWB as MAWB,
								cs_HAWB as HAWB ,
								cm_applicationCode,
								GB_Code as [Branch], 
								GC_Code as [Company], 
								CM_SystemCreateTimeUtc as [Job Open Date],  
								CM_SystemCreateUser as [Job Opening Staff]
							from dbo.CusHAWB  
								inner join dbo.CusMAWB  on CM_PK = CS_cm and CS_IsMasterHouse = 0   	-- get us only TRUE houses, not worker helper houses
								inner join dbo.GlbBranch  on CM_GB = GB_PK
								inner join dbo.GlbCompany  on GB_GC = GC_PK
							where cm_applicationCode = 'CUK'
								and cm_IsCtoMawb = 0 -- Brendon pls confirm
								and CM_SystemCreateTimeUtc between @fromDate and @toDate

							";

			var sqlForOtherApps = @"

							--Other countries
							-- Basics 
							select		CM_MAWB as [MAWB],
										'' as  HAWB,
										CM_ApplicationCode as [Application Code],
										GB_Code as [Branch], 
										GC_Code as [Company], 
										CM_SystemCreateTimeUtc as [Job Open Date],  
										CM_SystemCreateUser  as [Job Opening Staff] 
							from dbo.CusMAWB 
									inner join dbo.GlbBranch  on CM_gb = GB_PK
									inner join dbo.GlbCompany  on GB_GC = GC_PK		
							where CM_ApplicationCode <> 'CUK' and 
								CM_PK not in (select cs_cm from dbo.CusHAWB where CS_CM is not null)
								and CM_IsCTOMAWB = 0
								and CM_SystemCreateTimeUtc between @fromDate and @toDate

								union 
								-- Houses
								select 
										CM_MAWB,
										CS_HAWB ,
										case when cm_applicationCode is null then CS_ApplicationCode else CM_ApplicationCode end ,
										GB_Code as [Branch], 
										GC_Code as [Company], 
										CS_SystemCreateTimeUtc as [Job Open Date],  
										CS_SystemCreateUser as [Job Opening Staff]
								from dbo.CusHAWB 
									left join dbo.CusMAWB  on CS_CM = CM_PK 
									left join dbo.GlbBranch  on CM_gb = GB_PK
									left join dbo.GlbCompany  on GB_GC = GC_PK		
									where 
										(
											(CS_CM is not null and CM_ApplicationCode <> 'CUK')
											or 
											(CS_CM is null and CS_ApplicationCode <> 'cuk')
										)
								  and (CM_IsCTOMAWB = 0 or CM_IsCTOMAWB is null)
								  and CS_SystemCreateTimeUtc between @fromDate and @toDate

								ORDER BY MAWB, HAWB";
			#endregion

			return reportAwbsForCountriesOtherThanGB
						? sqlForGb + " UNION " + sqlForOtherApps
						: sqlForGb;
		}

		string GetEntryNumberReportSelectStatement()
		{
			var entryTypesAndCountries = new Dictionary<ZString, List<ZString>>();

			#region Room for expansion to other countries
			/*
			 * Uncomment the below as you need to in order to bill for additional countries/entry types
			 *
			entryTypesAndCountries.Add(Core.Constants.CountryCodes.Australia,	new List<ZString>{	Common.CusEntryNumberTypes.Australia.CAN, 
																									Common.CusEntryNumberTypes.Australia.CRN, 
																									Common.CusEntryNumberTypes.Australia.ECN, 
																									Common.CusEntryNumberTypes.Australia.EX1, 
																									Common.CusEntryNumberTypes.Australia.EX2, 
																									Common.CusEntryNumberTypes.Australia.EX3, 
																									Common.CusEntryNumberTypes.Australia.EX5, 
																									Common.CusEntryNumberTypes.Australia.EX7, 
																									Common.CusEntryNumberTypes.Australia.EX9, 
																									Common.CusEntryNumberTypes.Australia.EXA, 
																									Common.CusEntryNumberTypes.Australia.EXB, 
																									Common.CusEntryNumberTypes.Australia.EXC, 
																									Common.CusEntryNumberTypes.Australia.MMN});
			entryTypesAndCountries.Add(Core.Constants.CountryCodes.HongKong,	new List<ZString>{	Common.CusEntryNumberTypes.HongKong.ExportLicense, 
																									Common.CusEntryNumberTypes.HongKong.ImportLicense});
			entryTypesAndCountries.Add(Core.Constants.CountryCodes.Iceland,		new List<ZString>{	Common.CusEntryNumberTypes.Iceland.CRN});
			entryTypesAndCountries.Add(Core.Constants.CountryCodes.Malaysia,	new List<ZString>{	Common.CusEntryNumberTypes.Malaysia.MAN});
			entryTypesAndCountries.Add(Core.Constants.CountryCodes.UnitedStates,new List<ZString>{	Common.CusEntryNumberTypes.UnitedStates.CRN, 
																									Common.CusEntryNumberTypes.UnitedStates.EntrySummary, 
																									Common.CusEntryNumberTypes.UnitedStates.FTZ, 
																									Common.CusEntryNumberTypes.UnitedStates.InBond, 
																									Common.CusEntryNumberTypes.UnitedStates.Protest});
			foreach (var customsWareSupportedCountry in BaseJobDeclaration.CustomsWareCountries)
			{
				entryTypesAndCountries.Add(customsWareSupportedCountry,			new List<ZString>{	Common.CusEntryNumberTypes.Standard.MovementReferenceNumber});
			}
			*/
			#endregion

			entryTypesAndCountries.Add(Core.Constants.CountryCodes.UnitedKingdom, new List<ZString> { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Export });

			// Don't edit below here unless you're really sure you know what you're doing. Check with Daniel or Brendon.  
			#region Do not fiddle here please
			var entrySql = @"select  distinct
								je_declarationReference as [Job number], 
								JE_MessageType + JE_MessageSubType + JE_TransportMode as [Job type] ,
								CH_BGMReference  as [BGM Reference],
								CE_EntryNum as [Entry Number],
								CE_EntryType as [Entry Type],
								ce_rn_NkCountryCode as [Entry Country],
								ce_issueDate as [Entry Date],
								GB_Code as [Branch], 
								GC_Code as [Company], 
								JE_SystemCreateTimeUtc as [Job Open Date],  
								JE_SystemCreateUser as [Job Opening Staff]
							from  dbo.jobDeclaration  
								inner join dbo.CusEntryHeader  on CH_JE = JE_PK
								inner join dbo.CusEntryNum  on (CE_ParentID = CH_PK or CE_ParentID = JE_PK)
								inner join dbo.GlbBranch  on JE_GB = GB_PK
								inner join dbo.GlbCompany  on GB_GC = GC_PK
								inner join dbo.ediMessage on EM_LinkUniqueId = CH_PK
								inner join dbo.ediInterchange on EM_EI = EI_PK
								CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInlineToReturnEmptyIfNull(JE_AddInfo, 'Gateway') as GatewayAddInfo
							where CE_Category = 'cus'
								and CE_EntryIsSystemGenerated = 1
								and ce_issueDate between @fromDate and @toDate
								and GatewayAddInfo.Value  = 'CCSUK'
								and EI_ReceiveTransmit = 'RCV'
								and (EI_From = 'CUKCTM98CHFIMP' or EI_From = 'CUKCTM98CHFEXP') -- from CHIEF via CCSUK	
								and 
								(
									";

			var countrySelects = new List<ZString>();
			foreach (var pair in entryTypesAndCountries)
			{
				countrySelects.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "(ce_rn_NkCountryCode = '{0}' and CE_EntryType in ({1}))", pair.Key, EnumerableToInList(pair.Value)));
			}

			entrySql += string.Join((NoResString)" \r\n										or\r\n 									 ", countrySelects);
			entrySql = entrySql + @"										 
								)
								ORDER BY je_declarationReference";
			#endregion
			return entrySql;
		}

		protected virtual void SendReportToCargoWise(params ReportDataAndName[] reports)
		{
			var manager = new BillingManager();
			var transactions = new List<BillingTransaction>();
			foreach (var report in reports)
			{
				transactions.AddRange(report.TransactionRows.ToArray());
			}
			manager.AddTransactions(transactions, Db.Connection);
		}

		protected virtual WhatToDo RunNowRequirement
		{
			get
			{
				// Always run, whenever the servicvce task runs
				return EnvProxy.IsHostedWithCargowise ? WhatToDo.RunNow : WhatToDo.Not_WiseCloud;
			}
		}

		string EnumerableToInList(IEnumerable<ZString> list)
		{
			var stringBuilder = new ZStringBuilder(list);
			return "'" + stringBuilder.ToStringWithDelimiterBetweenAppends("','") + "'";
		}

		protected virtual void UpdateNextOrLastRunTime()
		{
			CustomsDataRegistry.Instance.CustomsTransactionalBillingTaskLastRunDate_BillingAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, toDate.ToDateTime());
		}

		protected enum WhatToDo
		{
			RunNow,
			Not_WiseCloud,
			Not_Due_Yet
		}

		public class ProductRegistrationSimple
		{
			public ProductRegistrationSimple(string systemId, string enterpriseCode, string serverCode)
			{
				this.SystemId = systemId;
				this.EnterpriseCode = enterpriseCode;
				this.ServerCode = serverCode;
			}

			public string SystemId { get; private set; }
			public string EnterpriseCode { get; private set; }
			public string ServerCode { get; private set; }
		}

		protected ZDateTime fromDate;
		protected ZDateTime toDate;
		protected Integration.ILogger serviceLogger;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Reports to HQ only so no need to translate")]
	static class ConstantStrings
	{
		internal const string CustomsEntries = "Customs Entries";
		internal const string AirWaybills = "Air Waybills";
		internal const string GENRALTextMessages = "GENRAL Text Messages";
	}
}
