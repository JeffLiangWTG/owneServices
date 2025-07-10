using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterData.ServiceTask.Test
{
	class TradeInformationHelperTest : ScriptTest
	{
		[TestDate(2021, 5, 1)]
		public void TestSendTradeInformation()
		{
			var service = GetServicesForTest(ResultCode.Successful, null);

			try
			{
				service.Start();

				var glAccount = TestObjectCreator.GetGLAccountFromDB();
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var reportDate = new ZDateTime(2021, 5, 1);
				CreateInvoice(org1, glAccount, reportDate, 1);

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				CreateInvoice(org2, glAccount, reportDate, 1);

				var currentCompany = Factory.Load<GlbCompany>(EnvProxy.Instance.CurrentCompany.PK);
				currentCompany.GC_IsActive = true;

				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.CompanyName = "TestCompany";
				Factory.Save();

				var dateList = TradeInformationHelper.GetReportDateList(null, ZDateTime.Now).ToArray();
				AssertEquals(24, dateList.Length);

				var logger = new TestLogger();
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					TradeInformationHelper.SendTradeInformation(logger);

					var testCompanyLog = company.Logs.MostRecentLogByEventTime(AutoEvents.TradeInformationSend);
					AssertNull(testCompanyLog);

					var currentCompanyLogs = currentCompany.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).OrderBy(x => x.SL_EventTime).ToArray();
					AssertEquals(24, currentCompanyLogs.Length);

					for (int i = 0; i < dateList.Length; i++)
					{
						var expectedRef = i == 23 ? "Send 2 trade balance data in 2021-04 to server" : $"Send 0 trade balance data in {dateList[i]:yyyy-MM} to server";
						var currentCompanyLog = currentCompanyLogs.Single(x => x.SL_Reference == expectedRef);
						AssertNotNull(currentCompanyLog);
						var eventDate = dateList[i].AddMonths(1);
						AssertStmALog(currentCompanyLog, expectedRef, eventDate.Year, eventDate.Month, 1);
					}

					AssertContains($"Sending trade information for company '{currentCompany.CompanyName}'", logger.Logs);
					AssertContains($@"Sending trade information for company 'TestCompany'
Current company is not allowed to send trade information, please go to '{OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.HumanReadableRegistryPath()}'", logger.Logs);
				}

				logger.ClearLog();
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TradeInformationHelper.SendTradeInformation(logger);

					var testCompanyLogs = company.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).OrderBy(x => x.SL_EventTime).ToArray();
					AssertEquals(24, testCompanyLogs.Length);
					for (int i = 0; i < dateList.Length; i++)
					{
						var expectedRef = $"Send 0 trade balance data in {dateList[i]:yyyy-MM} to server";
						var testCompanyLog = testCompanyLogs.Single(x => x.SL_Reference == expectedRef);
						AssertNotNull(testCompanyLog);
						var eventDate = dateList[i].AddMonths(1);
						AssertStmALog(testCompanyLog, expectedRef, eventDate.Year, eventDate.Month, 1);
					}

					var currentCompanyLogs = currentCompany.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).ToArray();
					AssertEquals(24, currentCompanyLogs.Length);

					AssertContains($@"Sending trade information for company '{currentCompany.CompanyName}'
The latest trade information data has been sent", logger.Logs);
					AssertContains("Sending trade information for company 'TestCompany'", logger.Logs);
				}

				logger.ClearLog();
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TradeInformationHelper.SendTradeInformation(logger);

					var testCompanyLogs = company.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).ToArray();
					AssertEquals(24, testCompanyLogs.Length);

					var currentCompanyLogs = currentCompany.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).ToArray();
					AssertEquals(24, currentCompanyLogs.Length);

					AssertContains($@"Sending trade information for company '{currentCompany.CompanyName}'
The latest trade information data has been sent", logger.Logs);
					AssertContains(@"Sending trade information for company 'TestCompany'
The latest trade information data has been sent", logger.Logs);
				}
			}
			finally
			{
				if (service.IsStarted)
				{
					service.Stop();
				}
			}
		}

		public void TestSendTradeInformation_ThrowWebException()
		{
			AssertExceptionThrown<WebException>(() =>
			{
				var glAccount = TestObjectCreator.GetGLAccountFromDB();
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var reportDate = new ZDateTime(2021, 5, 1);
				CreateInvoice(org1, glAccount, reportDate, 1);

				var currentCompany = Factory.Load<GlbCompany>(EnvProxy.Instance.CurrentCompany.PK);
				currentCompany.GC_IsActive = true;
				Factory.Save();

				var logger = new TestLogger();
				serviceUrl = "http://localhost.dummy";
				using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ServiceUrlCollection))
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TradeInformationHelper.SendTradeInformation(logger);
				}
			});
		}

		[TestDate(2021, 5, 1)]
		public void TestSendTradeInformation_HasErrorInfo()
		{
			var service = GetServicesForTest(ResultCode.Failed, new ErrorInfo("The server is unavailable"));
			try
			{
				service.Start();

				var glAccount = TestObjectCreator.GetGLAccountFromDB();
				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				var reportDate = new ZDateTime(2021, 5, 1);
				CreateInvoice(org1, glAccount, reportDate, 1);

				var currentCompany = Factory.Load<GlbCompany>(EnvProxy.Instance.CurrentCompany.PK);
				currentCompany.GC_IsActive = true;
				Factory.Save();

				var logger = new TestLogger();
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TradeInformationHelper.SendTradeInformation(logger);
				}

				var log = currentCompany.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).FirstOrDefault();
				AssertNull(log);

				AssertEquals($@"Sending trade information for company '{currentCompany.CompanyName}'
The server response did not indicate success when sending trade balance data in 2019-05: The server is unavailable
", logger.Logs);
			}
			finally
			{
				if (service.IsStarted)
				{
					service.Stop();
				}
			}
		}

		public void TestSendTradeInformation_NonProductionEnvironment()
		{
			var productRegistration = ObjectFactory.Get<IProductRegistration>();
			AssertNotEquals(DatabaseTypes.Codes.Production, productRegistration.Key.DatabaseType);

			var logger = new TestLogger();
			TradeInformationHelper.SendTradeInformation(logger);
			AssertContains("Send Credit Report Trade Data Is System Level Disabled", logger.Logs);
		}

		public void TestSendTradeInformation_NotAllowedCompany()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.CompanyName = "TestCompany1";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.CompanyName = "TestCompany2";
			Factory.Save();

			var logger = new TestLogger();
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				TradeInformationHelper.SendTradeInformation(logger);

				AssertContains($@"Sending trade information for company 'TestCompany1'
Current company is not allowed to send trade information, please go to '{OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.HumanReadableRegistryPath()}'", logger.Logs);
				AssertContains($@"Sending trade information for company 'TestCompany2'
Current company is not allowed to send trade information, please go to '{OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.HumanReadableRegistryPath()}'", logger.Logs);
			}
		}

		public void TestSendTradeInformation_CurrencyIsNotValid()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.Logs.AddNew(AutoEvents.TradeInformationSend, "", ZDateTimeOffset.Now);
			company.CompanyName = "TestCompany";
			company.GC_RX_NKLocalCurrency = "AB";
			Factory.Save();

			var logger = new TestLogger();
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TradeInformationHelper.SendTradeInformation(logger);
				AssertEquals(@"Sending trade information for company 'TestCompany'
Currency must be 3 digits: 'AB'
", logger.Logs);
			}
		}

		public void TestSendTradeInformation_LatestDataHasBeenSent()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.Logs.AddNew(AutoEvents.TradeInformationSend, "", ZDateTimeOffset.Now);
			company.CompanyName = "TestCompany";
			Factory.Save();

			var logger = new TestLogger();
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TradeInformationHelper.SendTradeInformation(logger);
				AssertEquals(@"Sending trade information for company 'TestCompany'
The latest trade information data has been sent
", logger.Logs);
			}
		}

		[TestDate(2021, 5, 23)]
		public void TestSendTradeInformation_NeverSentBefore()
		{
			var service = GetServicesForTest(ResultCode.Successful, null);
			try
			{
				service.Start();

				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.CompanyName = "TestCompany";
				Factory.Save();

				var logger = new TestLogger();
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TradeInformationHelper.SendTradeInformation(logger);

					var dateList = TradeInformationHelper.GetReportDateList(null, ZDateTime.Now);
					AssertEquals(24, dateList.Count);

					var testCompanyLogs = company.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).ToArray();
					AssertNotNull(testCompanyLogs);
					AssertEquals(24, testCompanyLogs.Length);
					foreach (var reportDate in dateList)
					{
						var expectedRef = $"Send 0 trade balance data in {reportDate:yyyy-MM} to server";
						var testCompanyLog = testCompanyLogs.Single(x => x.SL_Reference == expectedRef);
						AssertNotNull(testCompanyLog);
						var eventDate = reportDate.AddMonths(1);
						AssertStmALog(testCompanyLog, expectedRef, eventDate.Year, eventDate.Month, 1);
					}

					AssertEquals(@"Sending trade information for company 'TestCompany'
", logger.Logs);
				}
			}
			finally
			{
				if (service.IsStarted)
				{
					service.Stop();
				}
			}
		}

		[TestDate(2021, 5, 23)]
		public void TestSendTradeInformation_SentLessThan24Months()
		{
			var service = GetServicesForTest(ResultCode.Successful, null);
			try
			{
				service.Start();

				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.CompanyName = "TestCompany";
				company.Logs.AddNew(AutoEvents.TradeInformationSend, "", new DateTime(2020, 11, 1));
				company.Logs.AddNew(AutoEvents.TradeInformationSend, "", new DateTime(2021, 1, 1));
				company.Logs.AddNew(AutoEvents.TradeInformationSend, "", new DateTime(2020, 12, 1));
				Factory.Save();

				var logger = new TestLogger();
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TradeInformationHelper.SendTradeInformation(logger);

					var logs = company.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).OrderBy(x => x.SL_EventTime).Where(x => x.SL_Reference.Contains("Send 0 trade balance data in")).ToArray();
					AssertEquals(4, logs.Length);
					CombineAssertions(() =>
					{
						AssertStmALog(logs[0], "Send 0 trade balance data in 2021-01 to server", 2021, 2, 1);
						AssertStmALog(logs[1], "Send 0 trade balance data in 2021-02 to server", 2021, 3, 1);
						AssertStmALog(logs[2], "Send 0 trade balance data in 2021-03 to server", 2021, 4, 1);
						AssertStmALog(logs[3], "Send 0 trade balance data in 2021-04 to server", 2021, 5, 1);
					});

					AssertContains("Sending trade information for company 'TestCompany'", logger.Logs);
				}
			}
			finally
			{
				if (service.IsStarted)
				{
					service.Stop();
				}
			}
		}

		[TestDate(2021, 5, 23)]
		public void TestSendTradeInformation_Sent1MonthBefore()
		{
			var service = GetServicesForTest(ResultCode.Successful, null);
			try
			{
				service.Start();

				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.CompanyName = "TestCompany";
				company.Logs.AddNew(AutoEvents.TradeInformationSend, "", new DateTime(2021, 4, 1));
				Factory.Save();

				var logger = new TestLogger();
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TradeInformationHelper.SendTradeInformation(logger);

					var log = company.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend)
						.OrderByDescending(x => x.SL_EventTime).FirstOrDefault();
					AssertNotNull(log);
					AssertStmALog(log, "Send 0 trade balance data in 2021-04 to server", 2021, 5, 1);

					AssertContains("Sending trade information for company 'TestCompany'", logger.Logs);
				}
			}
			finally
			{
				if (service.IsStarted)
				{
					service.Stop();
				}
			}
		}

		public void TestCreateTradeInformationRequest()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.MainAddress.Address1 = "Address1";
			org1.MainAddress.Address2 = "Address2";
			org1.MainAddress.City = "City";
			org1.MainAddress.Postcode = "123";
			org1.MainAddress.State = "State";
			org1.MainAddress.OA_RL_NKRelatedPortCode = "CN123";
			org1.MainAddress.OA_Phone = "0123456789";

			var abn = org1.CustomsCodes.AddNew("ABN", "123", "AU");
			var acn = org1.CustomsCodes.AddNew("GCR", "456", "AU");
			org1.CustomsCodes.AddNew("GST", "789", "NZ");
			org1.CustomsCodes.AddNew("CNO", "897", "NZ");
			org1.CustomsCodes.AddNew("DUN", "654", "");

			var reportDate = new ZDateTime(2021, 5, 1);
			CreateInvoice(org1, glAccount, reportDate, 1);

			var currentCompany = Factory.Load<GlbCompany>(EnvProxy.Instance.CurrentCompany.PK);
			currentCompany.OrgProxy.MainAddress.Address1 = "Testing Street";
			currentCompany.OrgProxy.MainAddress.Address2 = "Testing Avenue";
			currentCompany.OrgProxy.MainAddress.City = "Test City";
			currentCompany.OrgProxy.MainAddress.Postcode = "1010";
			currentCompany.OrgProxy.MainAddress.State = "Test State";
			currentCompany.OrgProxy.MainAddress.OA_RL_NKRelatedPortCode = "AU123";
			currentCompany.OrgProxy.MainAddress.OA_Phone = "0987654321";

			var duns = currentCompany.OrgProxy.CustomsCodes.AddNew("DUN", "789", "");
			duns.OK_OA_PremisesAddress = currentCompany.OrgProxy.MainAddress.PK;
			currentCompany.OrgProxy.CustomsCodes.AddNew("DUN", "456", "AU");
			currentCompany.OrgProxy.CustomsCodes.AddNew("ABN", "555", "AU");
			currentCompany.OrgProxy.CustomsCodes.AddNew("GCR", "666", "AU");
			currentCompany.OrgProxy.CustomsCodes.AddNew("GST", "777", "NZ");
			currentCompany.OrgProxy.CustomsCodes.AddNew("CNO", "888", "NZ");
			currentCompany.OrgProxy.CustomsCodes.AddNew("DUN", "999", "");

			Factory.Save();

			var request = TradeInformationHelper.CreateTradeInformationRequest(new DateTime(2021, 5, 1), currentCompany);

			CombineAssertions(() =>
			{
				AssertEquals(1, request.Companies.Count());

				var company = request.Companies.FirstOrDefault();
				AssertNotNull(company);
				AssertEquals(currentCompany.CompanyName, company.Name);
				AssertEquals(currentCompany.GC_RN_NKCountryCode, company.Country);
				AssertEquals(currentCompany.GetLicenceCode(), company.LicenseCode);
				AssertEquals(currentCompany.OrgProxy.PK, company.Creditor.ClientSpecifiedIdentifier);
				AssertEquals(currentCompany.OrgProxy.OH_Code, company.Creditor.Code);
				AssertEquals(currentCompany.OrgProxy.OH_FullName, company.Creditor.Name);
				AssertEquals(currentCompany.OrgProxy.MainAddress.Address1, company.Creditor.Address1);
				AssertEquals(currentCompany.OrgProxy.MainAddress.Address2, company.Creditor.Address2);
				AssertEquals(currentCompany.OrgProxy.MainAddress.City, company.Creditor.City);
				AssertEquals(currentCompany.OrgProxy.MainAddress.State, company.Creditor.State);
				AssertEquals(currentCompany.OrgProxy.MainAddress.Postcode, company.Creditor.PostCode);
				AssertEquals(currentCompany.OrgProxy.MainAddress.Country.Code, company.Creditor.Country);
				AssertEquals(currentCompany.OrgProxy.MainAddress.OA_Phone, company.Creditor.PhoneNo);

				AssertEquals(5, company.Creditor.RegistrationCodes.Count());
				AssertEquals("555", company.Creditor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.ABN).RegNumber);
				AssertEquals("666", company.Creditor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.ACN).RegNumber);
				AssertEquals("777", company.Creditor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.NCN).RegNumber);
				AssertEquals("888", company.Creditor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.NZBN).RegNumber);
				AssertEquals("999", company.Creditor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.DUNS).RegNumber);

				AssertEquals(1, company.DataUploads.Count());
				var dataUpload = company.DataUploads.First();
				AssertNotNull(dataUpload.Date);
				AssertEquals(202105, dataUpload.Date);
				AssertEquals(1, dataUpload.Summaries.Count());

				var tradeSummary = dataUpload.Summaries.FirstOrDefault();
				AssertNotNull(tradeSummary);
				AssertEquals("COD", tradeSummary.Terms);
				AssertEquals(currentCompany.LocalCurrency.RX_Code, tradeSummary.Currency);
				AssertEquals(1m, tradeSummary.Total);
				AssertEquals(1m, tradeSummary.Current);
				AssertEquals(0m, tradeSummary.Overdue1Period);
				AssertEquals(0m, tradeSummary.Overdue2Period);
				AssertEquals(0m, tradeSummary.Overdue3Period);
				AssertEquals(0m, tradeSummary.Overdue4Period);
				AssertEquals(0m, tradeSummary.Overdue4PeriodPlus);

				AssertEquals(5, tradeSummary.Debtor.RegistrationCodes.Count());
				AssertEquals("123", tradeSummary.Debtor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.ABN).RegNumber);
				AssertEquals("456", tradeSummary.Debtor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.ACN).RegNumber);
				AssertEquals("789", tradeSummary.Debtor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.NCN).RegNumber);
				AssertEquals("897", tradeSummary.Debtor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.NZBN).RegNumber);
				AssertEquals("654", tradeSummary.Debtor.RegistrationCodes.FirstOrDefault(x => x.RegType == IdentifierType.DUNS).RegNumber);

				AssertNotNull(tradeSummary.Debtor);
				AssertEquals(org1.PK, tradeSummary.Debtor.ClientSpecifiedIdentifier);
				AssertEquals(org1.OH_Code, tradeSummary.Debtor.Code);
				AssertEquals(org1.OH_FullName, tradeSummary.Debtor.Name);
				AssertEquals("Address1", tradeSummary.Debtor.Address1);
				AssertEquals("Address2", tradeSummary.Debtor.Address2);
				AssertEquals("City", tradeSummary.Debtor.City);
				AssertEquals("123", tradeSummary.Debtor.PostCode);
				AssertEquals("CN", tradeSummary.Debtor.Country);
				AssertEquals("State", tradeSummary.Debtor.State);
				AssertEquals("0123456789", tradeSummary.Debtor.PhoneNo);
			});
		}

		[TestDate(2021, 5, 23)]
		public void TestSendTradeInformation_TokenCancelled()
		{
			using (var tokenSource = new CancellationTokenSource())
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.CompanyName = "TestCompany";
				Factory.Save();

				var logger = new TestLogger();
				using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					tokenSource.Cancel();
					AssertExceptionThrown<OperationCanceledException>(() => TradeInformationHelper.SendTradeInformation(logger, tokenSource.Token));
				}
			}
		}

		[TestDate(2021, 5, 23)]
		public void TestReportDateShouldBeTheFirstDayOfEachMonth_WithNullStmLog()
		{
			var list = TradeInformationHelper.GetReportDateList(null, ZDateTime.Now).Select(x => x.ToString("yyyyMMdd")).ToArray();
			AssertArrayEqualsByElements(new[]
			{
				"20190501",
				"20190601",
				"20190701",
				"20190801",
				"20190901",
				"20191001",
				"20191101",
				"20191201",
				"20200101",
				"20200201",
				"20200301",
				"20200401",
				"20200501",
				"20200601",
				"20200701",
				"20200801",
				"20200901",
				"20201001",
				"20201101",
				"20201201",
				"20210101",
				"20210201",
				"20210301",
				"20210401"
			}, list);
		}

		[TestDate(2021, 5, 23)]
		public void TestReportDateShouldBeTheFirstDayOfEachMonth_WithNonNullStmLog()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.CompanyName = "TestCompany";
			var log = company.Logs.AddNew(AutoEvents.TradeInformationSend, "", new DateTime(2021, 1, 1));
			Factory.Save();

			var list = TradeInformationHelper.GetReportDateList(log, ZDateTime.Now).Select(x => x.ToString("yyyyMMdd")).ToArray();
			AssertArrayEqualsByElements(new[]
			{
				"20210101",
				"20210201",
				"20210301",
				"20210401"
			}, list);
		}

		[TestDate(2022, 12, 13)]
		public void TestSendTradeInformation_WhenSystemLevelIsSetToYes_RespectCompanyLevel()
		{
			AssertSendTradeInformation_SystemLevelRegistryEnabledOrNot(true);
		}

		[TestDate(2022, 12, 13)]
		public void TestSendTradeInformation_WhenSystemLevelIsSetToNo_IgnoreCompanyLevel()
		{
			AssertSendTradeInformation_SystemLevelRegistryEnabledOrNot(false);
		}

		#region Implementation

		void AssertSendTradeInformation_SystemLevelRegistryEnabledOrNot(bool enabled)
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.CompanyName = "TestCompany1";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.CompanyName = "TestCompany2";
			company2.Logs.AddNew(AutoEvents.TradeInformationSend, "", ZDateTimeOffset.Now);
			Factory.Save();

			var logger = new TestLogger();
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled))
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				TradeInformationHelper.SendTradeInformation(logger);

				var sendCompany1Log = $@"Sending trade information for company 'TestCompany1'
Current company is not allowed to send trade information, please go to '{OrganisationsDataRegistry.Instance.EnableTradeBalanceInformationSent.HumanReadableRegistryPath()}'";
				var sendCompany2Log = @"Sending trade information for company 'TestCompany2'
The latest trade information data has been sent";
				var systemLevelDisabledLog = "Send Credit Report Trade Data Is System Level Disabled";

				if (enabled)
				{
					AssertContains(sendCompany1Log, logger.Logs);
					AssertContains(sendCompany2Log, logger.Logs);
					AssertNotContains(systemLevelDisabledLog, logger.Logs);
				}
				else
				{
					AssertNotContains(sendCompany1Log, logger.Logs);
					AssertNotContains(sendCompany2Log, logger.Logs);
					AssertContains(systemLevelDisabledLog, logger.Logs);
				}
			}
		}

		public static void AssertStmALog(StmALog stmALog, string expectedReference, int expectedYear, int expectedMonth, int expectedDay)
		{
			AssertEquals(expectedReference, stmALog.SL_Reference);
			AssertEquals(expectedYear, stmALog.SL_EventTime.Year);
			AssertEquals(expectedMonth, stmALog.SL_EventTime.Month);
			AssertEquals(expectedDay, stmALog.SL_EventTime.Day);
		}

		internal static HttpServiceForTest GetServicesForTest(ResultCode resultCode, ErrorInfo errorInfo, string url)
		{
			var response = new TradeInformationResponse()
			{
				ResultCode = resultCode,
				ErrorInfo = errorInfo,
			};

			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "POST" },
				Processor = (uri, request) =>
				{
					return new Tuple<int, string>(200, JsonSerializer.Serialize(response));
				},
				Uri = new Uri(url),
				ContentType = "application/json"
			};
		}

		HttpServiceForTest GetServicesForTest(ResultCode resultCode, ErrorInfo errorInfo)
		{
			return GetServicesForTest(resultCode, errorInfo, serviceUrl);
		}

		CodeDescriptionBoolWithSingleTrueCollection ServiceUrlCollection
		{
			get
			{
				var collection = new CodeDescriptionBoolWithSingleTrueCollection();
				collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)serviceUrl, Bool = true });
				return collection;
			}
		}

		InvoicingBase CreateInvoice(OrgHeader org, AccGLHeader glAccount, ZDateTime? dueDate = null, decimal amount = 10M)
		{
			invoiceNumber++;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), $"INV{invoiceNumber}", TestObjectCreator.AUD, 1M, amount, 0M, amount, 0M, org, glAccount.PK);
			invoice.AH_PostDate = new ZDateTime(2021, 4, 1);
			invoice.AH_DueDate = dueDate ?? new ZDateTime(2021, 5, 1);

			return invoice;
		}

		class TestLogger : ILogger
		{
			public string Logs => builder.ToString();

			readonly StringBuilder builder = new StringBuilder();

			public void Log(LogType type, string message)
			{
				Log(type, message, null);
			}

			public void Log(LogType type, string message, Exception ex)
			{
				builder.AppendLine(message);
			}

			public void ClearLog()
			{
				builder.Clear();
			}
		}

		int invoiceNumber;
		string serviceUrl;
		IDisposable serviceUrlDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			invoiceNumber = 0;
			serviceUrl = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";

			serviceUrlDisposable = OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ServiceUrlCollection);

			var companies = Factory.Load<GlbCompany>(new ZQuery());
			foreach (GlbCompany company in companies)
			{
				company.GC_IsActive = false;
			}

			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			serviceUrlDisposable?.Dispose();
		}

		#endregion
	}
}
