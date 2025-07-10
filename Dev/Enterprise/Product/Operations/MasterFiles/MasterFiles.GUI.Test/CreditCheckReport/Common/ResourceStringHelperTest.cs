using System;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class ResourceStringHelperTest : TestCase
	{
		public void TestResourceString()
		{
			CombineAssertions(() =>
			{
				// Confirm Get Report Page
				AssertEquals("You are about to purchase a Failure Risk Report (priced as per your current price list).", ResourceStringHelper.GetOperationDetailForConfirmGetReport(CreditReportType.FailureRisk));
				AssertEquals("Cancel", ResourceStringHelper.Cancel);
				AssertEquals("Get Report", ResourceStringHelper.GetReport);
				AssertEquals("Renew", ResourceStringHelper.GetActionNameForConfirmGetReport(false));
				AssertEquals("Get Report", ResourceStringHelper.GetActionNameForConfirmGetReport(true));
				AssertEquals("Renew Comprehensive Report", ResourceStringHelper.GetTitleForConfirmGetReport(false, CreditReportType.ComprehensiveReport));

				// ComingSoonPage
				AssertEquals("Credit Reports will soon be available for CN organizations", ResourceStringHelper.GetComingSoonString("CN"));

				// Alerts Banner
				AssertEquals("Latest Credit Events", ResourceStringHelper.LatestCreditEvents);
				AssertEquals("Collection Change", ResourceStringHelper.GetCreditEventDescription(CreditEventType.CollectionChange));
				AssertEquals("Court Change", ResourceStringHelper.GetCreditEventDescription(CreditEventType.CourtChange));
				AssertEquals("Director Change", ResourceStringHelper.GetCreditEventDescription(CreditEventType.DirectorChange));
				AssertEquals("Financial Change", ResourceStringHelper.GetCreditEventDescription(CreditEventType.FinancialChange));
				AssertEquals("Public Filing Change", ResourceStringHelper.GetCreditEventDescription(CreditEventType.PublicFilingChange));
				AssertEquals("Score Change", ResourceStringHelper.GetCreditEventDescription(CreditEventType.ScoreChange));
				AssertEquals("Shareholder Change", ResourceStringHelper.GetCreditEventDescription(CreditEventType.ShareholderChange));
				AssertEquals("Status Change", ResourceStringHelper.GetCreditEventDescription(CreditEventType.StatusChange));
				AssertEquals("Recent events unavailable. Get a report for detailed analysis.", ResourceStringHelper.EventsUnavailable);
				AssertEquals("Loading credit events...", ResourceStringHelper.LoadingEvents);

				// Get Report
				AssertEquals("Choose your report", ResourceStringHelper.ChooseYourReport);
				AssertEquals("Provided by:", ResourceStringHelper.ProvidedBy);
				AssertEquals("Comprehensive Report", ResourceStringHelper.ComprehensiveReport);
				AssertEquals("Failure Risk Report", ResourceStringHelper.FailureRiskReport);
				AssertEquals("Late Payment Risk Report", ResourceStringHelper.LatePaymentRiskReport);
				AssertEquals("Commercial Bureau Enquiry Report", ResourceStringHelper.CommercialBureauEnquiryReport);
				AssertEquals("More Info", ResourceStringHelper.MoreInfo);
				AssertEquals("Confirm", ResourceStringHelper.Confirm);
				AssertEquals("Renew", ResourceStringHelper.Renew);
				AssertEquals("There have been 1 events since the last report. Renew now for current information.", ResourceStringHelper.ToolTipCaption(1));
				AssertEquals("Last Report 01-Apr-20", ResourceStringHelper.LastReportDate(new DateTime(2020, 4, 1)));
				AssertEquals("Comprehensive Report", ResourceStringHelper.GetReportCaption(CreditReportType.ComprehensiveReport));
				AssertEquals("Failure Risk Report", ResourceStringHelper.GetReportCaption(CreditReportType.FailureRisk));
				AssertEquals("Late Payment Risk Report", ResourceStringHelper.GetReportCaption(CreditReportType.LatePaymentRisk));
				AssertEquals("Commercial Bureau Enquiry Report", ResourceStringHelper.GetReportCaption(CreditReportType.CommercialBureauEnquiry));
				AssertEquals("An exception occurred when running get credit report, exception message: ABC", ResourceStringHelper.GetErrorMessage(ResourceStringHelper.GetReportAction, "ABC"));
				AssertEquals("Please save the form before getting report.", ResourceStringHelper.SaveBeforeGettingReport);

				// Company Lookup
				AssertEquals("company lookup", ResourceStringHelper.CompanyLookupAction);
				AssertEquals("Cannot find matched companies.", ResourceStringHelper.NoMatchedCompany);
				AssertEquals("Company Lookup", ResourceStringHelper.CompanyLookup);

				// Top Banner
				AssertEquals("2 events.", ResourceStringHelper.GetEventsInfo(2));
				AssertEquals("Get your reports now, protect yourself by knowing the risk.", ResourceStringHelper.StatusDetailsNoEvent);
				AssertEquals("Since your last report WiseTech Global Limited has no events recorded.", ResourceStringHelper.StatusDetailsUpToDate("WiseTech Global Limited"));
				AssertEquals("Since your last report WiseTech Global Limited has new events recorded.", ResourceStringHelper.StatusDetailsWarning("WiseTech Global Limited"));
				AssertEquals("Credit Reports are now available", ResourceStringHelper.StatusHeaderNoEvent);
				AssertEquals("Up to date", ResourceStringHelper.StatusHeaderUpToDate);
				AssertEquals("Renew Credit Report", ResourceStringHelper.StatusHeaderWarning);

				// Price Items
				AssertEquals("Business Verification", ResourceStringHelper.BusinessVerification);
				AssertEquals("Enterprise Management", ResourceStringHelper.EnterpriseManagement);
				AssertEquals("Delinquency Score", ResourceStringHelper.DelinquencyScore);
				AssertEquals("Decision Support", ResourceStringHelper.DecisionSupport);

				// Loading Form
				AssertEquals("Obtaining Commercial Bureau Enquiry Report", ResourceStringHelper.GetLoadingFormTitleForReport(CreditReportType.CommercialBureauEnquiry));
				AssertEquals("Obtaining Comprehensive Report", ResourceStringHelper.GetLoadingFormTitleForReport(CreditReportType.ComprehensiveReport));
				AssertEquals("Obtaining Failure Risk Report", ResourceStringHelper.GetLoadingFormTitleForReport(CreditReportType.FailureRisk));
				AssertEquals("Obtaining Late Payment Risk Report", ResourceStringHelper.GetLoadingFormTitleForReport(CreditReportType.LatePaymentRisk));
				AssertEquals("This process may take some time...", ResourceStringHelper.LoadingContent);

				//Confirm Organization Form
				AssertEquals("Confidence Score: 80%", ResourceStringHelper.GetCompanyItemScore(80));

				// Get Specified Report form
				AssertEquals("Get Comprehensive Report", ResourceStringHelper.GetSpecifiedReport(CreditReportType.ComprehensiveReport));
				AssertEquals("The Comprehensive Report is the most insightful report, providing everything you need from identification details through to financial stability and payment predictors. A Comprehensive Report provides analysis on a company's long-term operations, credit history, profitability and stability.", ResourceStringHelper.GetReportDescription(CreditReportType.ComprehensiveReport));
				AssertEquals("The Risk of Failure report tells you the likelihood that a company will experience severe financial distress or failure within the next 12 months. It utilizes the Failure Risk Score, which looks at over 50 variables including financial statements, company age and structure, court actions, payment history, collections and defaults to predict future performance.", ResourceStringHelper.GetReportDescription(CreditReportType.FailureRisk));
				AssertEquals("The Risk of Late Payment report includes Late Payment Score, identification data and company characteristics. The Late Payment Score is driven by the Trade Bureau of over 15 million recent trade experiences across all companies and businesses. This is Australia and New Zealand’s largest and most comprehensive database of trade payment information.", ResourceStringHelper.GetReportDescription(CreditReportType.LatePaymentRisk));
				AssertEquals("Provides real-time access to ASIC data allowing you to confirm if an entity exists and is operational. Also provides historical data such as previous company names, directors and addresses allowing you to track previous structure and characteristics.", ResourceStringHelper.GetReportDescription(CreditReportType.CommercialBureauEnquiry));
				AssertEquals("Medium to high risk decisions", ResourceStringHelper.GetRiskDecisionsCaption(CreditReportType.ComprehensiveReport));
				AssertEquals("Medium to high risk decisions", ResourceStringHelper.GetRiskDecisionsCaption(CreditReportType.FailureRisk));
				AssertEquals("Low to medium risk decisions", ResourceStringHelper.GetRiskDecisionsCaption(CreditReportType.LatePaymentRisk));
				AssertEquals("Low risk decisions", ResourceStringHelper.GetRiskDecisionsCaption(CreditReportType.CommercialBureauEnquiry));
				AssertEquals("50+ Risk factors", ResourceStringHelper.RiskDecision0Description);
				AssertEquals("Risk of Failure", ResourceStringHelper.RiskDecision1Description);
				AssertEquals("Risk of Late Payment", ResourceStringHelper.RiskDecision2Description);
				AssertEquals("When you need to know everything", ResourceStringHelper.RiskDecision3Description);
				AssertEquals("When you need to see financial information", ResourceStringHelper.RiskDecision4Description);
				AssertEquals("Basic information", ResourceStringHelper.RiskDecision5Description);
				AssertEquals("Company facts & adverse effects", ResourceStringHelper.RiskDecision6Description);
				AssertEquals("Getting paid on time", ResourceStringHelper.RiskDecision7Description);
				AssertEquals("When viability and stability are critical", ResourceStringHelper.RiskDecision8Description);
				AssertEquals("Best in market failure score", ResourceStringHelper.RiskDecision9Description);
				AssertEquals("The report will be added to your eDocs.", ResourceStringHelper.AddEDocDescription);
				AssertEquals("View sample report", ResourceStringHelper.ViewSampleReport);
				AssertEquals("Pricing level:", ResourceStringHelper.PricingLevelCaption);

				// Service or network exception
				AssertEquals("Credit report service is unavailable, please try again later.", ResourceStringHelper.GeneralServiceException);
				AssertEquals("Credit report service is unavailable, please reload the form and try again later.", ResourceStringHelper.SharedKeyExpired);
				AssertEquals("An error has occurred while completing this request. Please raise a CR9 Service Request.", ResourceStringHelper.GeneralError);

				// Credit Report Information form
				AssertEquals("Import From Credit Reports", ResourceStringHelper.CreditReportInformationFormTitle);
				AssertEquals("Action", ResourceStringHelper.Action);
				AssertEquals("Addresses", ResourceStringHelper.Addresses);
				AssertEquals("Address 1", ResourceStringHelper.Address1);
				AssertEquals("City", ResourceStringHelper.City);
				AssertEquals("Match Address", ResourceStringHelper.MatchAddress);
				AssertEquals("Brands & Company Names", ResourceStringHelper.BrandsAndCompanyNames);
				AssertEquals("New Name", ResourceStringHelper.NewBrandName);
				AssertEquals("Registration Numbers / Codes", ResourceStringHelper.RegistrationNumber);
				AssertEquals("Type", ResourceStringHelper.RegistrationNumberType);
				AssertEquals("Registration Number / Code", ResourceStringHelper.NewRegistrationNumber);
				AssertEquals("Current Registration Number / Code", ResourceStringHelper.CurrentRegistrationNumber);
				AssertEquals("Primary", ResourceStringHelper.Primary);
				AssertEquals("Website", ResourceStringHelper.Website);
				AssertEquals("New Website", ResourceStringHelper.NewWebsite);
				AssertEquals("Current Website", ResourceStringHelper.CurrentWebsite);
				AssertEquals("Skip", ResourceStringHelper.SkipCaption);
				AssertEquals("Update Organization", ResourceStringHelper.UpdateOrganizationCaption);
				AssertEquals("New Organization details have been found in the purchased Credit Report. Please select an action to update the Organization record with the Credit Report data or select Skip.", ResourceStringHelper.CreditReportInformationCaption);
			});
		}
	}
}
