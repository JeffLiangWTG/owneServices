using System;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public static class ResourceStringHelper
	{
		// Confirm Get Report Page
		public static string GetOperationDetailForConfirmGetReport(CreditReportType reportType) =>
			Res.GetString(
				"21498F8A-A9DC-40A3-B7D0-E6B30700389C",
				"You are about to purchase a {0} (priced as per your current price list).",
				GetReportCaption(reportType));

		public static string Cancel => Res.GetString("E1456BBC-331A-4E10-BA93-BBC667EF7D72", "Cancel");

		public static string GetTitleForConfirmGetReport(bool isGet, CreditReportType reportType) => $"{GetActionNameForConfirmGetReport(isGet)} {GetReportCaption(reportType)}";

		public static string GetActionNameForConfirmGetReport(bool isGet) => isGet ? GetReport : Renew;

		// Coming Soon Page
		public static string GetComingSoonString(string country) => Res.GetString("771818A3-ADBF-4005-99FE-7AF0C7922768", "Credit Reports will soon be available for {0} organizations", country);

		// Not available Page
		public static string UnavailableString => Res.GetString("87bfb285-01f0-43f4-b113-145c0c3cddbb", "Credit Reports are unavailable. No organization was selected.");

		// Alerts Banner
		public static string LatestCreditEvents => Res.GetString("A7852799-0C0E-4D2B-AB78-F2DE8B8DB085", "Latest Credit Events");

		public static string GetCreditEventDescription(CreditEventType type)
		{
			switch (type)
			{
				case CreditEventType.CollectionChange:
					return Res.GetString("49FC1903-71D7-4F3A-8BCC-6B11B2999DE4", "Collection Change");
				case CreditEventType.CourtChange:
					return Res.GetString("20356110-8397-496D-AEA3-E22587F45899", "Court Change");
				case CreditEventType.DirectorChange:
					return Res.GetString("F5BB2E35-EF18-4BFD-84EC-6CF63BA61751", "Director Change");
				case CreditEventType.FinancialChange:
					return Res.GetString("CC06FFBB-5F6B-4DDF-A83F-0EFC89D4EB0C", "Financial Change");
				case CreditEventType.PublicFilingChange:
					return Res.GetString("6B5986F1-9590-440A-9C46-F00508559F1D", "Public Filing Change");
				case CreditEventType.ScoreChange:
					return Res.GetString("EF3988AC-5D9C-42DB-90DD-AB186A510FDF", "Score Change");
				case CreditEventType.ShareholderChange:
					return Res.GetString("C48BCC2C-C048-46E0-92BB-42123785E0C2", "Shareholder Change");
				case CreditEventType.StatusChange:
					return Res.GetString("E8C9F3D4-EC08-4D75-B7BE-78F430FA2A7E", "Status Change");
				default:
					return Res.GetString("F6147874-FBBD-4929-84D1-47BA562474F8", "Unknown Credit Event Type");
			}
		}

		public static string EventsUnavailable => Res.GetString("A9FA8684-B3FF-40AD-B6E2-7E35420E4E1E", "Recent events unavailable. Get a report for detailed analysis.");

		public static string LoadingEvents => Res.GetString("AAC6ED1D-3997-417D-B9BC-2DEE374FE663", "Loading credit events...");

		// Get Report
		public static string ChooseYourReport => Res.GetString("AA4AFFBD-E479-4C61-BF98-A64AD014B725", "Choose your report");

		public static string ProvidedBy => Res.GetString("25096EDA-1E57-437A-81B8-D576C63C1041", "Provided by:");

		public static string ComprehensiveReport => Res.GetString("7D272AA4-EFE5-421D-8C41-28994581FB6C", "Comprehensive Report");

		public static string FailureRiskReport => Res.GetString("7E4D15EE-51CB-4D25-9A7C-F7062BAF724D", "Failure Risk Report");

		public static string GetReportAction => Res.GetString("EEEED9EE-099C-4E87-8FCF-2F5BE5A86629", "get credit report");

		public static string GetErrorMessage(string actionName, string exceptionMessage) => Res.GetString("5E16B763-9C32-44C1-834B-968F1C2CAC34", "An exception occurred when running {0}, exception message: {1}", actionName, exceptionMessage);

		public static string LatePaymentRiskReport => Res.GetString("0913298B-5C6E-48F2-A54B-AF610CFBB347", "Late Payment Risk Report");

		public static string CommercialBureauEnquiryReport => Res.GetString("10E6D163-C3BF-4201-BEC5-BD5D0D5894E7", "Commercial Bureau Enquiry Report");

		public static string MoreInfo => Res.GetString("CE6B1D1A-1A62-4AD2-8E0A-EB861EF32116", "More Info");

		public static string GetReport => Res.GetString("4B0E3326-4467-4364-86DA-F9E112D28808", "Get Report");

		public static string Confirm => Res.GetString("FBDF9C9E-4B40-4820-9FFB-04E32BE29B0C", "Confirm");

		public static string Renew => Res.GetString("A24FB137-6420-47C3-9602-C256D0F3B3EE", "Renew");

		public static string ToolTipCaption(int eventCount) => Res.GetString("2F30F426-C5EA-450F-8EB9-8E277C537D78", "There have been {0} events since the last report. Renew now for current information.", eventCount);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "reference ZDatetime from ZArchitecture will result in cyclic dependency")]
		public static string LastReportDate(DateTime date) => Res.GetString("F6B76491-994E-4A82-9452-804060230CB2", "Last Report {0}", date.ToString("dd-MMM-yy"));

		public static string UnknownReportType => Res.GetString("573EF389-A129-4CE7-9A0A-E1487E493E22", "Unknown Report Type");

		public static string SaveBeforeGettingReport => Res.GetString("266882C0-E3B3-423F-9F63-82966D4E966D", "Please save the form before getting report.");

		public static string GetReportCaption(CreditReportType type)
		{
			switch (type)
			{
				case CreditReportType.ComprehensiveReport:
					return ComprehensiveReport;
				case CreditReportType.FailureRisk:
					return FailureRiskReport;
				case CreditReportType.LatePaymentRisk:
					return LatePaymentRiskReport;
				case CreditReportType.CommercialBureauEnquiry:
					return CommercialBureauEnquiryReport;
				default:
					return UnknownReportType;
			}
		}

		// Company Lookup
		public static string Title => Res.GetString("E9F9FF32-1A63-4ED2-87F3-FE2546D2273E", "Confirm the organization");

		public static string CompanyLookupAction => Res.GetString("4603F6EC-28D8-470B-B267-02EA33C3658D", "company lookup");

		public static string NoMatchedCompany => Res.GetString("E70A3D81-EA4B-4BCF-ABFF-F5D838EE6741", "Cannot find matched companies.");

		// Top Banner
		public static string GetEventsInfo(int eventsCount) => Res.GetString("4481D641-FEE1-418A-A9C1-36017444D8B8", "{0} events.", eventsCount);

		public static string StatusDetailsNoEvent => Res.GetString("D25CA5CC-77CF-4015-BE8A-4BD9FE502822", "Get your reports now, protect yourself by knowing the risk.");

		public static string StatusDetailsUpToDate(string organizationName) => Res.GetString("6bbcd3b2-b8e5-479a-8e4d-d893b345e89b", "Since your last report {0} has no events recorded.", organizationName);

		public static string StatusDetailsWarning(string organizationName) => Res.GetString("544527ee-060d-463d-ac8c-7242e44b48e2", "Since your last report {0} has new events recorded.", organizationName);

		public static string StatusHeaderNoEvent => Res.GetString("bbdcb8c9-6c47-4739-b83e-ea6fa4ca4dea", "Credit Reports are now available");

		public static string StatusHeaderUpToDate => Res.GetString("935241D1-CAE7-4298-AF03-82688CFAB4EB", "Up to date");

		public static string StatusHeaderWarning => Res.GetString("45973930-258E-4F04-B026-C8F8134F35ED", "Renew Credit Report");

		// Price Items
		public static string BusinessVerification => Res.GetString("A838A562-79D1-4E42-B66D-7A8FAB717639", "Business Verification");

		public static string EnterpriseManagement => Res.GetString("3C13EC99-9097-487D-A367-8CF5C80E65FE", "Enterprise Management");

		public static string DelinquencyScore => Res.GetString("1124F672-0D3E-49B9-9A0A-5C8210D1D6E1", "Delinquency Score");

		public static string DecisionSupport => Res.GetString("6A0DB676-2D51-4DD9-90DD-61B9321C61F0", "Decision Support");

		// Loading Form
		public static string GetLoadingFormTitleForReport(CreditReportType creditReportType)
		{
			return Res.GetString("F0325A6E-3918-4E0D-8FF9-186848D28755", "Obtaining {0}", GetReportCaption(creditReportType));
		}

		//Confirm Organization Form
		public static string GetCompanyItemScore(int companyItemScore) => Res.GetString("D819B2CA-B9A3-4253-9B58-A50B25104089", "Confidence Score: {0}%", companyItemScore);

		public static string LoadingContent => Res.GetString("55B73581-901D-4D13-891D-D0C901D1CB10", "This process may take some time...");

		public static string CompanyLookup => Res.GetString("06743050-112E-4A69-839A-5D05B4DB6A47", "Company Lookup");

		// Get Specified Report form
		public static string GetSpecifiedReport(CreditReportType type) => Res.GetString("DB5ED841-F8C3-4672-B36D-BDAD44A4F441", "Get {0}", GetReportCaption(type));

		public static string GetReportDescription(CreditReportType type)
		{
			switch (type)
			{
				case CreditReportType.ComprehensiveReport:
					return Res.GetString("F0DC298A-89A5-42CB-A3C6-000EB1C5E4B9", "The Comprehensive Report is the most insightful report, providing everything you need from identification details through to financial stability and payment predictors. A Comprehensive Report provides analysis on a company's long-term operations, credit history, profitability and stability.");
				case CreditReportType.FailureRisk:
					return Res.GetString("AC2C5FA8-B933-4C85-8484-E2302D34B2D6", "The Risk of Failure report tells you the likelihood that a company will experience severe financial distress or failure within the next 12 months. It utilizes the Failure Risk Score, which looks at over 50 variables including financial statements, company age and structure, court actions, payment history, collections and defaults to predict future performance.");
				case CreditReportType.LatePaymentRisk:
					return Res.GetString("FDBF97F3-E6BB-4E39-A575-BDE5E9D8F48C", "The Risk of Late Payment report includes Late Payment Score, identification data and company characteristics. The Late Payment Score is driven by the Trade Bureau of over 15 million recent trade experiences across all companies and businesses. This is Australia and New Zealand’s largest and most comprehensive database of trade payment information.");
				case CreditReportType.CommercialBureauEnquiry:
					return Res.GetString("F3FE1019-FD08-4F29-98FC-6685F828EF18", "Provides real-time access to ASIC data allowing you to confirm if an entity exists and is operational. Also provides historical data such as previous company names, directors and addresses allowing you to track previous structure and characteristics.");
				default:
					return UnknownReportType;
			}
		}

		public static string GetRiskDecisionsCaption(CreditReportType type)
		{
			switch (type)
			{
				case CreditReportType.ComprehensiveReport:
				case CreditReportType.FailureRisk:
					return Res.GetString("23FFEFD9-6E8C-4125-960C-C44953E7F2A4", "Medium to high risk decisions");
				case CreditReportType.LatePaymentRisk:
					return Res.GetString("7D66FFEC-1DD6-4501-8122-868BD3FFAF5D", "Low to medium risk decisions");
				case CreditReportType.CommercialBureauEnquiry:
					return Res.GetString("57B6D9FB-6053-4797-8E43-B19BA4F2821C", "Low risk decisions");
				default:
					return UnknownReportType;
			}
		}

		public static string RiskDecision0Description => Res.GetString("860C562B-DC3C-4450-92FB-D043F52903D8", "50+ Risk factors");

		public static string RiskDecision1Description => Res.GetString("52737DE9-46CF-440F-9CC5-293F7CC4459D", "Risk of Failure");

		public static string RiskDecision2Description => Res.GetString("CFD51955-BF74-4BEE-B749-D765ED59651D", "Risk of Late Payment");

		public static string RiskDecision3Description => Res.GetString("06CD1CF9-ABFE-4A22-ADBC-413E525D83C0", "When you need to know everything");

		public static string RiskDecision4Description => Res.GetString("9D4FD5A3-259F-41EF-A68C-77DA6F0E06A8", "When you need to see financial information");

		public static string RiskDecision5Description => Res.GetString("745D9CF9-D0FC-4D85-AD87-57AFD1E7581A", "Basic information");

		public static string RiskDecision6Description => Res.GetString("FD1E798A-F1A5-4C48-B020-655C36957D87", "Company facts & adverse effects");

		public static string RiskDecision7Description => Res.GetString("0C7C1414-C7BC-4D79-9C3A-9ED46E84FEE3", "Getting paid on time");

		public static string RiskDecision8Description => Res.GetString("D817A02B-6D4D-4D03-B518-5EFFCCA50583", "When viability and stability are critical");

		public static string RiskDecision9Description => Res.GetString("C68C1C98-0F92-4F8F-BB7C-00773B393900", "Best in market failure score");

		public static string AddEDocDescription => Res.GetString("2D6731CC-FF5C-4416-9BA6-11C869648DD9", "The report will be added to your eDocs.");

		public static string ViewSampleReport => Res.GetString("570B79C5-A727-44AC-989B-2A06B5B87F89", "View sample report");

		public static string PricingLevelCaption => Res.GetString("66E470B4-27A2-4C09-962F-018B37C91A7D", "Pricing level:");

		public static string GeneralServiceException => Res.GetString("545B6733-E703-47B0-B859-02B6A165B2DC", "Credit report service is unavailable, please try again later.");

		public static string ProductUnavailabilityDateCheckFailed(string[] parameters) => Res.GetString("36D0682B-8A53-4CCB-9369-D87AF74FE9F0", "The {0} report type that you have requested is not currently available for {1}. You may wish to order a different report type.\r\n\r\nIf you would like to request this report be made available.Please raise a CR9 service request.", parameters[0], parameters[1]);

		public static string ProductUnavailabilityUnincorporated(string[] parameters) => Res.GetString("2595E848-E144-4815-89E8-75BC09A86A78", "The {0} report type is unavailable for unincorporated entries. Please select a different report.", parameters[0]);

		public static string NoMatchForSuppliedDUNS => Res.GetString("B4C721CA-AA74-4A75-B2E8-09B01125851D", "The credit bureau is unable to find the provided registration code.\r\n\r\nPlease review if the number is valid. If it is and you would like to order a report, please raise a CR9 service request.");

		public static string UnmatchedUNLOCOAndDUNS => Res.GetString("4D2135DE-4F44-43AE-824F-3246146A75FB", "The Main Address UNLOCO and DUNS number's country/region do not match. Please review and change accordingly.\r\n\r\nIf it is and you would like to order a report, please raise a CR9 service request.");

		public static string SharedKeyExpired => Res.GetString("6504AC6D-D3F6-410E-9F5D-BBF6D7C012D6", "Credit report service is unavailable, please reload the form and try again later.");

		public static string GeneralError => Res.GetString("590D8A07-A9DF-4A77-A87A-B1FAB6029CCE", "An error has occurred while completing this request. Please raise a CR9 Service Request.");

		// Credit Report Information form
		public static string CreditReportInformationFormTitle => Res.GetString("41B52979-7214-4BD5-B361-47CC5005B5E7", "Import From Credit Reports");

		public static string Action => Res.GetString("D2995884-CD66-4D48-9C37-23AD44285D07", "Action");

		public static string Addresses => Res.GetString("C5A2D07F-811C-480D-9E33-15E11BAEB612", "Addresses");

		public static string Address1 => Res.GetString("2CB7371D-8B6B-4CF2-AA55-DB00DF6CA899", "Address 1");

		public static string City => Res.GetString("0D7A319C-D364-4448-BE2B-62CD2AEDE01B", "City");

		public static string MatchAddress => Res.GetString("41CDEE10-9B98-42E5-AE97-9DD124C9BE9A", "Match Address");

		public static string BrandsAndCompanyNames => Res.GetString("9C2C5AD0-72B0-4CBB-A4FB-A4F6ADFAA80F", "Brands & Company Names");

		public static string NewBrandName => Res.GetString("F6F88754-7FD3-4E93-8B20-22ECDDDAAEAC", "New Name");

		public static string RegistrationNumber => Res.GetString("4451C215-35E5-4408-AC61-21F6454B6E36", "Registration Numbers / Codes");

		public static string RegistrationNumberType => Res.GetString("22E90890-FE56-481D-AA25-E21750886CD7", "Type");

		public static string NewRegistrationNumber => Res.GetString("997710D9-0437-412D-98CC-3E770F78F2A8", "Registration Number / Code");

		public static string CurrentRegistrationNumber => Res.GetString("35AE78B0-55B2-4C15-B171-4A4D50610A3F", "Current Registration Number / Code");

		public static string Primary => Res.GetString("70D31399-B7F0-4F97-B62E-8227DA83D47D", "Primary");

		public static string Website => Res.GetString("EB43F9B3-9DDB-4DEA-981A-7F3BFC200338", "Website");

		public static string NewWebsite => Res.GetString("157F0E97-5CA1-4C0C-83B8-3B94A41A521C", "New Website");

		public static string CurrentWebsite => Res.GetString("284C2BCD-E90D-480A-9460-AEABA4396FB4", "Current Website");

		public static string SkipCaption => Res.GetString("EA9EA060-8B68-493F-ABC2-D2BEB929A5CC", "Skip");

		public static string UpdateOrganizationCaption => Res.GetString("3E6E1098-6D4C-4C92-87F3-E1E9B4129297", "Update Organization");

		public static string CreditReportInformationCaption => Res.GetString("32C4D2CE2-7981-4E05-B140-5FCEADE1B91E", "New Organization details have been found in the purchased Credit Report. Please select an action to update the Organization record with the Credit Report data or select Skip.");
	}
}
