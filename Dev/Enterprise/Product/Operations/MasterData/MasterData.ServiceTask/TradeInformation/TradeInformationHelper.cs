using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.CreditCheck;
using WTG.ROPE.Model;

namespace Enterprise.MasterData.ServiceTask
{
	public static class TradeInformationHelper
	{
		const string ShortYearMonthFormat = "yyyyMM";
		const string FullYearMonthFormat = "yyyy-MM";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log message not seen by user")]
		public static void SendTradeInformation(ILogger logger, CancellationToken cancellationToken = default)
		{
			if (TradeBalanceEnabled)
			{
				var factory = new BusinessObjectFactory();
				var serviceWrapper = new CreditCheckServiceWrapper();
				var companies = factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true));
				foreach (var company in companies)
				{
					cancellationToken.ThrowIfCancellationRequested();

					logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Sending trade information for company '{0}'", company.CompanyName));

					if (company.GC_RX_NKLocalCurrency.Trim().Length == 3)
					{
						var companyEnabled = OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
						if (companyEnabled)
						{
							var log = company.Logs.MostRecentLogByEventTime(AutoEvents.TradeInformationSend);

							var nowDateTimeUtc = ZDateTime.UtcNow;
							var reportDateList = GetReportDateList(log, nowDateTimeUtc);
							if (reportDateList.Any())
							{
								foreach (var reportDate in reportDateList)
								{
									var request = CreateTradeInformationRequest(reportDate, company);
									var result = CreditCheckService.SaveTradeInformation(serviceWrapper, request, OrganisationsDataRegistry.Instance.SaveTradeInformationTimeout.Value);
									if (result.ResultCode == ResultCode.Successful)
									{
										string message;
										var tradesCount = request.Companies.First().DataUploads.Sum(x => x.Summaries.Count()).ToString("N0");
										message = string.Format(CultureInfo.InvariantCulture, "Send {0} trade balance data in {1} to server", tradesCount, reportDate.ToString(FullYearMonthFormat));

										company.Logs.AddNew(AutoEvents.TradeInformationSend, message, reportDate.AddMonths(1));
										factory.Save();
									}
									else
									{
										logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "The server response did not indicate success when sending trade balance data in {0}: {1}", reportDate.ToString(FullYearMonthFormat), result.ErrorInfo?.Message));
										break;
									}
								}
							}
							else
							{
								logger.Log(LogType.Information, "The latest trade information data has been sent");
							}
						}
						else
						{
							logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Current company is not allowed to send trade information, please go to '{0}'", OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.HumanReadableRegistryPath()));
						}
					}
					else
					{
						logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Currency must be 3 digits: '{0}'", company.GC_RX_NKLocalCurrency));
					}
				}
			}
			else
			{
				logger.Log(LogType.Information, "Send Credit Report Trade Data Is System Level Disabled");
			}
		}

		static bool TradeBalanceEnabled
		{
			get
			{
				var productRegistration = ObjectFactory.Get<IProductRegistration>();
				return OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
					&& (productRegistration.Key.DatabaseType == DatabaseTypes.Codes.Production || OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.Value);
			}
		}

		public static List<DateTime> GetReportDateList(StmALog log, ZDateTime nowDateTime)
		{
			var startReportDate = nowDateTime.AddMonths(-24);
			if (log != null)
			{
				var eventTime = log.SL_EventTime;
				if (eventTime != DateTime.MinValue)
				{
					startReportDate = eventTime.ToDateTime();
				}
			}

			var dateTimeList = new List<DateTime>();
			var endReportDate = nowDateTime.AddMonths(-1);
			while (true)
			{
				if (new DateTime(startReportDate.Year, startReportDate.Month, 1).CompareTo(new DateTime(endReportDate.Year, endReportDate.Month, 1)) <= 0)
				{
					dateTimeList.Add(new DateTime(startReportDate.Year, startReportDate.Month, 1));
					startReportDate = startReportDate.AddMonths(1);
				}
				else
				{
					break;
				}

				if (dateTimeList.Count == 24)
				{
					break;
				}
			}

			return dateTimeList;
		}

		internal static TradeInformationRequestV2 CreateTradeInformationRequest(DateTime reportDate, GlbCompany company)
		{
			var request = new TradeInformationRequestV2
			{
				Companies = new List<TradeCompany>()
				{
					new TradeCompany
					{
						Name = company.CompanyName,
						LicenseCode = company.GetLicenceCode(),
						Country = company.GC_RN_NKCountryCode,
						Creditor = new TradeOrganization
						{
							ClientSpecifiedIdentifier = company.OrgProxy?.PK.ToGuid() ?? Guid.Empty,
							Name = company.OrgProxy?.OH_FullName ?? string.Empty,
							Code = company.OrgProxy?.OH_Code ?? string.Empty,
							PhoneNo = company.OrgProxy?.MainAddress?.OA_Phone ?? string.Empty,
							Address1 = company.OrgProxy?.MainAddress?.OA_Address1 ?? string.Empty,
							Address2 = company.OrgProxy?.MainAddress?.OA_Address2 ?? string.Empty,
							City = company.OrgProxy?.MainAddress?.OA_City ?? string.Empty ,
							State = company.OrgProxy?.MainAddress?.OA_State ?? string.Empty ,
							PostCode = company.OrgProxy?.MainAddress?.OA_PostCode ?? string.Empty,
							Country = company.OrgProxy?.MainAddress?.Country?.Code ?? string.Empty,
							RegistrationCodes = GetTradeRegistrationCodeList(company.OrgProxy)
						},
						DataUploads = GetTradeDataUploadList(Convert.ToInt32(reportDate.ToString(ShortYearMonthFormat)), GetTradeInformationData(company.PK.ToGuid(), reportDate), company.GC_RX_NKLocalCurrency)
					}
				}
			};

			return request;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static DataTable GetTradeInformationData(Guid companyPk, DateTime date)
		{
			using (var cmd = Db.Connection.Command("TradeInformationSP")) // Have to use stored procedure instead of BusinessObjectFactory
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandTimeout = 300;
				cmd.AddParameter("@ReportDate", SqlDbType.DateTime, date);
				cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, companyPk);
				return DataUtils.GetDataTableFromCommand(cmd);
			}
		}

		static string GetPrimaryDuns(OrgHeader header)
		{
			return header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x =>
				string.IsNullOrEmpty(x.OK_RN_NKCodeCountry) &&
				x.OK_CodeType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem &&
				x.OK_OA_PremisesAddress.IsEmpty)?.OK_CustomsRegNo ?? string.Empty;
		}

		static List<TradeDataUpload> GetTradeDataUploadList(int reportDate, DataTable tradeDateInformationData, string currency)
		{
			var result = new List<TradeDataUpload>() { new TradeDataUpload() { Date = reportDate, Summaries = GetTradeSummaryList(tradeDateInformationData, currency) } };

			return result;
		}

		static List<TradeRegistrationCode> GetTradeRegistrationCodeList(DataRow row)
		{
			var result = new List<TradeRegistrationCode>();
			AddRegistrationCode(row, IdentifierType.ABN, result);
			AddRegistrationCode(row, IdentifierType.ACN, result);
			AddRegistrationCode(row, IdentifierType.DUNS, result);
			AddRegistrationCode(row, IdentifierType.NCN, result);
			AddRegistrationCode(row, IdentifierType.NZBN, result);

			return result;
		}

		static List<TradeRegistrationCode> GetTradeRegistrationCodeList(OrgHeader orgHeader)
		{
			var result = new List<TradeRegistrationCode>();
			var orgCustomCodes = orgHeader?.CustomsCodes;

			if (orgCustomCodes != null)
			{
				AddRegistrationCode(orgCustomCodes, IdentifierType.ABN, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Core.Constants.CountryCodes.Australia, result);
				AddRegistrationCode(orgCustomCodes, IdentifierType.ACN, OrgCusCode.CodeTypes.CorporationCode, Core.Constants.CountryCodes.Australia, result);
				AddRegistrationCode(orgCustomCodes, IdentifierType.NCN, OrgCusCode.CodeTypes.GSTCode, Core.Constants.CountryCodes.NewZealand, result);
				AddRegistrationCode(orgCustomCodes, IdentifierType.NZBN, OrgCusCode.CodeTypes.CompanyNumber, Core.Constants.CountryCodes.NewZealand, result);

				var primaryDuns = GetPrimaryDuns(orgHeader);

				if (!string.IsNullOrEmpty(primaryDuns))
				{
					result.Add(new TradeRegistrationCode(IdentifierType.DUNS, primaryDuns));
				}
			}

			return result;
		}

		static void AddRegistrationCode(DataRow row, IdentifierType identifierType, List<TradeRegistrationCode> result)
		{
			if (!row.IsNull(identifierType.ToString()))
			{
				result.Add(new TradeRegistrationCode(identifierType, GetStringValue(row, identifierType.ToString())));
			}
		}

		static void AddRegistrationCode(OrgCusCodeCollection orgCustomCodes, IdentifierType identifierType, string codeType, string countryCode, List<TradeRegistrationCode> result)
		{
			ZString customsRegNo = orgCustomCodes.GetCustomsRegNoMatchingCountryAndCodes(countryCode, codeType);
			if (!customsRegNo.IsEmpty)
			{
				result.Add(new TradeRegistrationCode(identifierType, customsRegNo));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to use constant")]
		static List<TradeSummary> GetTradeSummaryList(DataTable tradeInformationData, string currency)
		{
			var result = new List<TradeSummary>();

			foreach (DataRow row in tradeInformationData.Rows)
			{
				var debtorCountry = GetStringValue(row, "AddressCountry");
				result.Add(new TradeSummary
				{
					Currency = currency,
					Terms = GetStringValue(row, "InvoiceTerm"),
					Total = GetDecimalValue(row, "Balance"),
					Current = GetDecimalValue(row, "PeriodCurrent"),
					Overdue1Period = GetDecimalValue(row, "Period1Total"),
					Overdue2Period = GetDecimalValue(row, "Period2Total"),
					Overdue3Period = GetDecimalValue(row, "Period3Total"),
					Overdue4Period = GetDecimalValue(row, "Period4Total"),
					Overdue4PeriodPlus = GetDecimalValue(row, "Period4PlusTotal"),
					Debtor = new TradeOrganization
					{
						ClientSpecifiedIdentifier = GetGuidValue(row, "AccountPK"),
						Name = GetStringValue(row, "AccountName"),
						Code = GetStringValue(row, "AccountCode"),
						RegistrationCodes = GetTradeRegistrationCodeList(row),
						Address1 = GetStringValue(row, "Address1"),
						Address2 = GetStringValue(row, "Address2"),
						City = GetStringValue(row, "City"),
						PostCode = GetStringValue(row, "PostCode"),
						State = GetStringValue(row, "State"),
						Country = string.IsNullOrEmpty(debtorCountry) ? GetStringValue(row, "CountryCode") : debtorCountry,
						PhoneNo = GetStringValue(row, "ContactPhoneNo"),
					}
				});
			}

			return result;
		}

		static string GetStringValue(DataRow row, string columnName)
		{
			return row.IsNull(columnName) ? string.Empty : row[columnName].ToString();
		}
		static decimal GetDecimalValue(DataRow row, string columnName)
		{
			return row.IsNull(columnName) ? 0 : decimal.Parse(row[columnName].ToString());
		}

		static Guid GetGuidValue(DataRow row, string columnName)
		{
			return row.IsNull(columnName) ? Guid.Empty : Guid.Parse(row[columnName].ToString());
		}
	}
}
