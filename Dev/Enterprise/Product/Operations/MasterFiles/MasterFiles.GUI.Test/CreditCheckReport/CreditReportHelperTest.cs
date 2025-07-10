using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.ROPE.Model;
using static Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CreditReportHelperTest : TestCaseWithFactory
	{
		public void TestSaveIdentifiers_NotSilent()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUSYD";
			header.OH_Code = "MYORGSYD";
			AssertEquals("Precondition: ", 0, header.CustomsCodes.Count);

			AssertEquals(true, header.SaveIdentifiers(
				new List<Identifier>
				{
					new Identifier
					{
						ID = "223344556",
						Type = IdentifierType.DUNS
					},
					new Identifier
					{
						ID = "333666999",
						Type = IdentifierType.ACN
					}
				}, false));
			AssertEquals(2, header.CustomsCodes.Count);

			var customCode0 = header.CustomsCodes[0];
			var customCode1 = header.CustomsCodes[1];
			CombineAssertions(() =>
			{
				AssertEquals("PK", header.PK, customCode0.OK_OH);
				AssertEquals("DUN", customCode0.OK_CodeType);
				AssertEquals("223344556", customCode0.OK_CustomsRegNo);
				AssertEquals("AU", customCode0.OK_RN_NKCodeCountry);

				AssertEquals("PK", header.PK, customCode1.OK_OH);
				AssertEquals("GCR", customCode1.OK_CodeType);
				AssertEquals("333666999", customCode1.OK_CustomsRegNo);
				AssertEquals("AU", customCode1.OK_RN_NKCodeCountry);
			});

			AssertEquals(true, header.SaveIdentifiers(
				new List<Identifier>
				{
					new Identifier
					{
						ID = "66 015 286 036",
						Type = IdentifierType.ABN
					}
				}, false));
			AssertEquals(3, header.CustomsCodes.Count);

			var customCode2 = header.CustomsCodes[2];
			CombineAssertions(() =>
			{
				AssertEquals("PK", header.PK, customCode2.OK_OH);
				AssertEquals("ABN", customCode2.OK_CodeType);
				AssertEquals("66 015 286 036", customCode2.OK_CustomsRegNo);
				AssertEquals("AU", customCode2.OK_RN_NKCodeCountry);
			});
		}

		public void TestSaveIdentifiers_Silent_OnlySaveDUNS()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: ", 0, header.CustomsCodes.Count);
				AssertEquals(false, header.SaveIdentifiers(
					new List<Identifier>
					{
						new Identifier
						{
							ID = "223344556",
							Type = IdentifierType.ABN
						},
					}, true));

				AssertEquals(0, header.CustomsCodes.Count);
			});
		}

		public void TestSaveIdentifiers_Silent_DoNotSave_WhenDUNSExists()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.CustomsCodes.AddNew("DUN", "223344556", "AU");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: ", 1, header.CustomsCodes.Count);
				AssertEquals("Precondition: ", "223344556", header.CustomsCodes.Cast<OrgCusCode>().Single().OK_CustomsRegNo);
				AssertEquals("Precondition: ", "DUN", header.CustomsCodes.Cast<OrgCusCode>().Single().OK_CodeType);

				AssertEquals(false, header.SaveIdentifiers(
					new List<Identifier>
					{
						new Identifier
						{
							ID = "0123456789",
							Type = IdentifierType.DUNS
						},
					}, true));

				AssertEquals(1, header.CustomsCodes.Count);
				AssertEquals("223344556", header.CustomsCodes.Cast<OrgCusCode>().Single().OK_CustomsRegNo);
				AssertEquals("DUN", header.CustomsCodes.Cast<OrgCusCode>().Single().OK_CodeType);
			});
		}

		public void TestSaveIdentifiers_DoNotSave_WhenHaveMultipleIdentifies()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: ", 0, header.CustomsCodes.Count);
				AssertEquals(false, header.SaveIdentifiers(
					new List<Identifier>
					{
						new Identifier
						{
							ID = "0123456789",
							Type = IdentifierType.DUNS
						},
						new Identifier
						{
							ID = "987654321",
							Type = IdentifierType.ABN
						},
					}, true));
				AssertEquals(0, header.CustomsCodes.Count);
			});
		}

		public void TestSaveIdentifiers_Silent_Successful()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: ", 0, header.CustomsCodes.Count);
				AssertEquals(true, header.SaveIdentifiers(
					new List<Identifier>
					{
						new Identifier
						{
							ID = "0123456789",
							Type = IdentifierType.DUNS
						},
					}, true));
				AssertEquals(1, header.CustomsCodes.Count);
				AssertEquals("0123456789", header.CustomsCodes.Cast<OrgCusCode>().Single().OK_CustomsRegNo);
				AssertEquals("DUN", header.CustomsCodes.Cast<OrgCusCode>().Single().OK_CodeType);
			});
		}

		[TestDate(2020, 4, 28)]
		public void TestSaveAndShowReport()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";
			registrationKey.SystemIdForTest = "SYD1";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "USORD";
			org.OH_FullName = "New Dummy Name 1";

			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "AU";
			glbCompany.GC_Code = "DUM";

			var glbBranch = Factory.NewWithValidTestData<GlbBranch>();
			glbBranch.GB_GC = glbCompany.PK;
			glbBranch.GB_Code = "TST";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "TSF";
			staff.GS_GB_HomeBranch = glbBranch.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), glbBranch.PK.ToGuid(), Guid.Empty))
			{
				Env.Security.GetDocumentTypeUploadCheckPoint(RefDocTypes.CreditReport).IsAllowed = true;
				AssertNull(org.GetLogs().Find(u => u.SL_Reference.StartsWith(CreditReportHelper.BuyCreditReportReference)).FirstOrDefault());
				AssertExceptionThrown<FormatException>(() => org.SaveAndShowReport(CreditReportType.ComprehensiveReport, "dummy", ".pdf", true, "123", "", "", ""));

				IeDoc ieDoc = null;
				AssertNoExceptionThrown(() => ieDoc = org.SaveAndShowReport(CreditReportType.ComprehensiveReport, Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy")), ".pdf", false, "123", "", "", ""));
				AssertEquals(true, (ieDoc as BusinessObject)?.IsInDatabase ?? false);

				var log = org.GetLogs().Find(u => u.SL_Reference.StartsWith(CreditReportHelper.BuyCreditReportReference)).FirstOrDefault();

				var priceItemCode = new CreditCheckCodeMapping().ReportTypePriceItemCodeMapping[(CreditReportType.ComprehensiveReport, true)];
				var stmUsageDataCollection = new BillingManager().GetTransactions(Factory, 10).ToList();
				var xml = BillingManager.GetTransactionXml(stmUsageDataCollection.Single());
				var expectedXml = string.Format(CultureInfo.InvariantCulture,
					@"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"">
  <BillableCount>1</BillableCount>
  <Branch>{0}</Branch>
  <Category>CRD</Category>
  <ClientID>{1}</ClientID>
  <ClientNumber>{2}</ClientNumber>
  <ClientStaffCode>{3}</ClientStaffCode>
  <PriceItemCode>{4}</PriceItemCode>
  <Reference1>{5}</Reference1>
  <Reference2>123</Reference2>
  <Reference3>US</Reference3>
  <Reference4>{6}</Reference4>
  <Reference5>New Dummy Name 1</Reference5>
  <ReportingSource>CW1</ReportingSource>
  <ServiceOccuredUTC>2020-04-28T00:00:00Z</ServiceOccuredUTC>
  <Version>1</Version>
  <AdditionalRefs xsi:nil=""true"" />
</BillingTransaction>",
					"TST", "ENTDUMSVR", "SYD1.DUM", "TSF", priceItemCode, org.PK, "AU");

				CombineAssertions(() =>
				{
					AssertEquals("Report exist", 1, org.DocManagerInfo().AllEDocs.Count);
					AssertEquals("Comprehensive Report_" + ZDateTime.UtcToday.Date + ".pdf", ieDoc.FileName);
					AssertEquals(RefDocTypes.CreditReport, ieDoc.DocType);
					AssertEquals(RefDocTypeDescriptions.CreditReport, ieDoc.Description);
					AssertEquals(true, ieDoc.IsPublished);
					AssertEquals("dummy", ieDoc.ImageData.ToUTF8());

					AssertNotNull(log);
					AssertEquals(AutoEvents.CreditCheckEvent.Code, log.SL_SE_NKEvent);
					AssertEquals(new ZDateTime(2020, 4, 28), log.SL_PostedTimeUtc);
					AssertEquals(CreditReportHelper.RenewCode, log.Parameters[Params.Codes.Type]);
					AssertEquals(ReportType.Code.ComprehensiveReport, log.Parameters[Params.Codes.Reason]);
					AssertEquals("Comprehensive Report_" + ZDateTime.UtcToday.Date + ".pdf", log.Parameters[Params.Codes.Name]);
					AssertEquals("Event Type: Renewed Report | Reason: Comprehensive Report", log.DisplayEventReference);
					AssertXMLEquals("Billing Transaction Created", expectedXml, xml);
				});

				org.SaveAndShowReport(CreditReportType.CommercialBureauEnquiry, Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy")), ".pdf", true, "123", "", "", "");
				var logs = org.GetLogs();
				log = logs.Find(u => u.Parameters.ContainsKey(Params.Codes.Type) && u.Parameters[Params.Codes.Type] == CreditReportHelper.BuyCode).FirstOrDefault();
				AssertNotNull(log);
				AssertEquals("Event Type: Bought Report | Reason: Commercial Bureau Enquiry", log.DisplayEventReference);
			}
		}

		[TestDate(2020, 05, 13)]
		public void TestSaveAndShowReport_NoDuns()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertExceptionThrown<DeveloperNotificationException>(() => org.SaveAndShowReport(CreditReportType.ComprehensiveReport, "dummy", ".pdf", true, null, "", "", ""));
		}

		public void TestSaveAndShowReport_AddsCreditScores()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			org.SaveAndShowReport(CreditReportType.ComprehensiveReport, Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy")), ".pdf", true, "123", "60", "10", "30");

			AssertEquals(org.MiscServ.OM_CCCreditRating, "60");
			AssertEquals(org.MiscServ.OM_CCFailureRiskScore, ZShort.Parse("10"));
			AssertEquals(org.MiscServ.OM_CCLatePaymentScore, ZShort.Parse("30"));

			org.SaveAndShowReport(CreditReportType.ComprehensiveReport, Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy")), ".pdf", true, "123", null, null, null);

			AssertEquals(org.MiscServ.OM_CCCreditRating, "60");
			AssertEquals(org.MiscServ.OM_CCFailureRiskScore, ZShort.Parse("10"));
			AssertEquals(org.MiscServ.OM_CCLatePaymentScore, ZShort.Parse("30"));
		}

		public void TestGetPurchasedReports()
		{
			var startDate = ZDateTime.UtcNow;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			AssertEquals(0, orgHeader.GetPurchasedReports().Length);
			orgHeader.AddBuyCreditReportEvent(ReportType.Code.ComprehensiveReport, true, "ABC1.pdf");
			orgHeader.AddBuyCreditReportEvent(ReportType.Code.LatePaymentRiskReport, true, "ABC111.pdf");
			Factory.Save();
			Thread.Sleep(100);

			orgHeader.AddBuyCreditReportEvent(ReportType.Code.ComprehensiveReport, true, "ABC2.pdf");
			orgHeader.AddBuyCreditReportEvent(ReportType.Code.LatePaymentRiskReport, true, "ABC222.pdf");
			Factory.Save();
			Thread.Sleep(100);

			orgHeader.AddBuyCreditReportEvent(ReportType.Code.LatePaymentRiskReport, true, "ABC3.pdf");
			orgHeader.AddBuyCreditReportEvent(ReportType.Code.CommercialBureauEnquiryReport, true, "ABC333.pdf");
			Factory.Save();
			Thread.Sleep(100);

			var purchasedReports = orgHeader.GetPurchasedReports();
			AssertEquals(3, purchasedReports.Length);
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					(CreditReportType.LatePaymentRisk, ReportType.Desc.LatePaymentRiskReport , "ABC3.pdf"),
					(CreditReportType.CommercialBureauEnquiry, ReportType.Desc.CommercialBureauEnquiryReport , "ABC333.pdf"),
					(CreditReportType.ComprehensiveReport, ReportType.Desc.ComprehensiveReport , "ABC2.pdf")
				},
				purchasedReports.Select(u => (u.CreditReportType, u.ReportType, u.FileName)));

			orgHeader.AddBuyCreditReportEvent(ReportType.Code.FailureRiskReport, true, "ABC4.pdf");
			Factory.Save();

			purchasedReports = orgHeader.GetPurchasedReports();
			AssertEquals(4, purchasedReports.Length);
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					(CreditReportType.LatePaymentRisk, ReportType.Desc.LatePaymentRiskReport , "ABC3.pdf"),
					(CreditReportType.CommercialBureauEnquiry, ReportType.Desc.CommercialBureauEnquiryReport , "ABC333.pdf"),
					(CreditReportType.ComprehensiveReport, ReportType.Desc.ComprehensiveReport , "ABC2.pdf"),
					(CreditReportType.FailureRisk, ReportType.Desc.FailureRiskReport , "ABC4.pdf")
				},
				purchasedReports.Select(u => (u.CreditReportType, u.ReportType, u.FileName)));

			var endDate = ZDateTime.UtcNow;
			Assert(purchasedReports.All(u => u.BuyReportDate >= startDate && u.BuyReportDate <= endDate));
		}

		public void TestConstants()
		{
			// Please do not change these constants value, it's used for find out which credit reports are purchased by the client
			AssertEquals("Comprehensive Report", ReportType.Desc.ComprehensiveReport);
			AssertEquals("Failure Risk Report", ReportType.Desc.FailureRiskReport);
			AssertEquals("Late Payment Risk Report", ReportType.Desc.LatePaymentRiskReport);
			AssertEquals("Commercial Bureau Enquiry Report", ReportType.Desc.CommercialBureauEnquiryReport);
		}

		[RequiresSTA]
		public void TestShowReportWhenFileIsNotExisted()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new ZForm(orgHeader))
			{
				orgHeader.ShowLatestReport(form, CreditReportType.ComprehensiveReport);
				AssertEquals("The Comprehensive Report does not exist.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 4, 28)]
		public void TestBuyReportDisplayEventReference()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "USORD";
			org.OH_FullName = "New Dummy Name 1";
			Factory.Save();

			var logs = org.GetLogs().Find(u => u.SL_Reference.StartsWith(CreditReportHelper.BuyCreditReportReference));
			AssertEquals(0, logs.Count());

			org.SaveAndShowReport(CreditReportType.ComprehensiveReport, Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy")), ".pdf", true, "123", "", "", "");
			org.SaveAndShowReport(CreditReportType.FailureRisk, Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy")), ".pdf", true, "123", "", "", "");
			org.SaveAndShowReport(CreditReportType.LatePaymentRisk, Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy")), ".pdf", true, "123", "", "", "");
			org.SaveAndShowReport(CreditReportType.CommercialBureauEnquiry, Convert.ToBase64String(Encoding.UTF8.GetBytes("dummy")), ".pdf", true, "123", "", "", "");

			logs = org.GetLogs().Find(u => u.SL_Reference.StartsWith(CreditReportHelper.BuyCreditReportReference));

			CombineAssertions(() =>
			{
				AssertEquals(4, logs.Count());
				Assert(logs.Any(l => l.DisplayEventReference == "Event Type: Bought Report | Reason: Comprehensive Report"));
				Assert(logs.Any(l => l.DisplayEventReference == "Event Type: Bought Report | Reason: Late Payment Risk"));
				Assert(logs.Any(l => l.DisplayEventReference == "Event Type: Bought Report | Reason: Failure Risk Report"));
				Assert(logs.Any(l => l.DisplayEventReference == "Event Type: Bought Report | Reason: Commercial Bureau Enquiry"));
			});
		}
	}
}
