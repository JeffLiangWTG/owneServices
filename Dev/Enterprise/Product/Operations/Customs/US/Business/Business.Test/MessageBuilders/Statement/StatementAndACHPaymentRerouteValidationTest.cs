using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class StatementAndACHPaymentRerouteValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2015, 08, 04)]
		public void TestCheckZ9_ImportOfRecordNumber()
		{
			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACS;
			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;
			reroute.Z9_AllDocumentsRequest = true;
			reroute.Z9_ImportOfRecordNumber = "12345";
			AssertHasMessageError(reroute.Z9_ImportOfRecordNumberInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);
			reroute.Z9_AllDocumentsRequest = false;
			reroute.Z9_StatementNumber = "1234";
			AssertNoMessageError(reroute.Z9_ImportOfRecordNumberInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);
			AssertHasMessageError(reroute.Z9_ImportOfRecordNumberInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);
			reroute.Z9_ImportOfRecordNumber = ZString.Empty;
			AssertNoMessageError(reroute.Z9_ImportOfRecordNumberInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);

			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;
			reroute.Z9_StatementNumber = "1234";
			reroute.Z9_ImportOfRecordNumber = "12345";
			AssertNoMessageError(reroute.Z9_ImportOfRecordNumberInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);

			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACE;
			reroute.Z9_AllDocumentsRequest = true;
			reroute.Validation.ValidateZ9_ImportOfRecordNumber();
			AssertHasMessageError(reroute.Z9_ImportOfRecordNumberInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);

			reroute.Z9_AllDocumentsRequest = false;
			reroute.Validation.ValidateZ9_ImportOfRecordNumber();
			AssertHasMessageError(reroute.Z9_ImportOfRecordNumberInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);
			AssertNoMessageError(reroute.Z9_ImportOfRecordNumberInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);

			reroute.Z9_StatementNumber = ZString.Empty;
			reroute.Validation.ValidateZ9_ImportOfRecordNumber();
			AssertNoMessageError(reroute.Z9_ImportOfRecordNumberInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);
		}

		[TestDate(2015, 08, 04)]
		public void TestCheckZ9_ClientBranch()
		{
			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACS;
			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;
			reroute.Z9_AllDocumentsRequest = true;
			reroute.Z9_ClientBranch = "12";
			AssertHasMessageError(reroute.Z9_ClientBranchInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);
			reroute.Z9_AllDocumentsRequest = false;
			reroute.Z9_StatementNumber = "1234";
			AssertNoMessageError(reroute.Z9_ClientBranchInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);
			AssertHasMessageError(reroute.Z9_ClientBranchInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);
			reroute.Z9_ClientBranch = ZString.Empty;
			AssertNoMessageError(reroute.Z9_ClientBranchInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);

			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;
			reroute.Z9_StatementNumber = "1234";
			reroute.Z9_ClientBranch = "12";
			AssertNoMessageError(reroute.Z9_ClientBranchInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);

			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACE;
			reroute.Z9_AllDocumentsRequest = true;
			reroute.Validation.ValidateZ9_ClientBranch();
			AssertHasMessageError(reroute.Z9_ClientBranchInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);

			reroute.Z9_AllDocumentsRequest = false;
			reroute.Validation.ValidateZ9_ClientBranch();
			AssertHasMessageError(reroute.Z9_ClientBranchInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);
			AssertNoMessageError(reroute.Z9_ClientBranchInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);

			reroute.Z9_StatementNumber = ZString.Empty;
			reroute.Validation.ValidateZ9_ClientBranch();
			AssertNoMessageError(reroute.Z9_ClientBranchInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);
		}

		[TestDate(2015, 08, 04)]
		public void TestCheckZ9_StatementNumber()
		{
			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACS;
			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;
			reroute.Z9_AllDocumentsRequest = true;
			reroute.Z9_StatementNumber = "12";
			AssertHasMessageError(reroute.Z9_StatementNumberInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);
			reroute.Z9_StatementNumber = ZString.Empty;
			AssertNoMessageError(reroute.Z9_StatementNumberInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);

			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACE;
			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;
			reroute.Z9_StatementNumber = "12";
			AssertHasMessageError(reroute.Z9_StatementNumberInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);

			reroute.Z9_StatementNumber = ZString.Empty;
			AssertNoMessageError(reroute.Z9_StatementNumberInfo, StatementAndACHPaymentRerouteValidation.ScopeIndicatorSubmitted);
		}

		[TestDate(2015, 08, 04)]
		public void TestCheckZ9_RerouteType()
		{
			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACS;
			reroute.Z9_RerouteType = "Z";
			AssertHasMessageError(reroute.Z9_RerouteTypeInfo, ListValidation.InvalidCodeMessageError);

			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;
			AssertNoMessageError(reroute.Z9_RerouteTypeInfo, ListValidation.InvalidCodeMessageError);

			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;
			AssertNoMessageError(reroute.Z9_RerouteTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZ9_TranmissionDate()
		{
			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACS;
			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;
			reroute.Z9_StatementNumber = ZString.Empty;
			reroute.Z9_TranmissionDate = ZDate.Today.AddDays(1);
			AssertHasMessageError(reroute.Z9_TranmissionDateInfo, StatementAndACHPaymentRerouteValidation.InvalidTransmissionDate);
			AssertNoMessageError(reroute.Z9_TranmissionDateInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);

			reroute.Z9_TranmissionDate = ZDate.Today.AddDays(-StatementAndACHPaymentRerouteValidation.NumberOfDaysInThePastAllowed);
			AssertNoMessageError(reroute.Z9_TranmissionDateInfo, StatementAndACHPaymentRerouteValidation.InvalidTransmissionDate);

			reroute.Z9_TranmissionDate = ZDate.Today.AddDays(-1 - StatementAndACHPaymentRerouteValidation.NumberOfDaysInThePastAllowed);
			AssertHasMessageError(reroute.Z9_TranmissionDateInfo, StatementAndACHPaymentRerouteValidation.InvalidTransmissionDate);

			reroute.Z9_TranmissionDate = ZDate.Empty;
			AssertNoMessageError(reroute.Z9_TranmissionDateInfo, StatementAndACHPaymentRerouteValidation.InvalidTransmissionDate);

			reroute.Z9_TranmissionDate = ZDate.Today;
			AssertNoMessageError(reroute.Z9_TranmissionDateInfo, StatementAndACHPaymentRerouteValidation.InvalidTransmissionDate);
			AssertNoErrors(reroute.Z9_TranmissionDateInfo);

			reroute.Z9_StatementNumber = "1234";
			AssertHasMessageError(reroute.Z9_TranmissionDateInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);

			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACE;
			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;
			reroute.Validation.ValidateZ9_TranmissionDate();
			AssertHasMessageError(reroute.Z9_TranmissionDateInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);

			reroute.Z9_StatementNumber = ZString.Empty;
			reroute.Validation.ValidateZ9_TranmissionDate();
			AssertNoMessageError(reroute.Z9_TranmissionDateInfo, StatementAndACHPaymentRerouteValidation.StatementNumberSubmitted);
		}

		public void TestValidateThatThereIsARouteRequest()
		{
			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACS;
			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;

			reroute.Validation.ValidateAll();
			AssertHasRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_PreliminaryStatementRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_PreliminaryStatementRequest = false;
			reroute.Z9_FinalStatementRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_FinalStatementRequest = false;
			reroute.Z9_ACHPaymentRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_ACHPaymentRequest = false;
			reroute.Z9_PeriodicStatementPaymentAuthorizationRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_PeriodicStatementPaymentAuthorizationRequest = false;
			reroute.RunPreSaveValidation();
			AssertHasRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_RerouteType = StatementTypeList.Codes.PeriodicMonthly;

			reroute.Validation.ValidateAll();
			AssertHasRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoMonthlyReroutesRequested);

			reroute.Z9_PreliminaryStatementRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoMonthlyReroutesRequested);

			reroute.Z9_PreliminaryStatementRequest = false;
			reroute.Z9_FinalStatementRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoMonthlyReroutesRequested);

			reroute.Z9_FinalStatementRequest = false;
			reroute.Z9_ACHPaymentRequest = true;
			reroute.RunPreSaveValidation();
			AssertHasRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoMonthlyReroutesRequested);

			reroute.Z9_ACHPaymentRequest = false;
			reroute.Z9_PeriodicStatementPaymentAuthorizationRequest = true;
			reroute.RunPreSaveValidation();
			AssertHasRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoMonthlyReroutesRequested);

			reroute.Z9_PeriodicStatementPaymentAuthorizationRequest = false;
			reroute.RunPreSaveValidation();
			AssertHasRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoMonthlyReroutesRequested);

			reroute.Z9_MessageType = JobApplicationCodeList.Codes.ACE;
			reroute.Z9_RerouteType = StatementTypeList.Codes.Daily;

			reroute.Validation.ValidateAll();
			AssertHasRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_PreliminaryStatementRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_PreliminaryStatementRequest = false;
			reroute.Z9_FinalStatementRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_FinalStatementRequest = false;
			reroute.Z9_ACHPaymentRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_ACHPaymentRequest = false;
			reroute.Z9_PeriodicStatementPaymentAuthorizationRequest = true;
			reroute.RunPreSaveValidation();
			AssertNoRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);

			reroute.Z9_PeriodicStatementPaymentAuthorizationRequest = false;
			reroute.RunPreSaveValidation();
			AssertHasRowErrorContaining(reroute, StatementAndACHPaymentRerouteValidation.NoDailyReroutesRequested);
		}

		public void TestCheckZ9_ProcessingPort()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "Test Name", startDate, endDate);
			newFactory.Save();

			var reroute = new StatementAndACHPaymentReroute();
			reroute.Z9_ProcessingPort = "";
			AssertHasMessageErrorContaining(reroute.Z9_ProcessingPortInfo, MandatoryValidation.YouHaveNotEntered);

			reroute.Z9_ProcessingPort = "~~~~";
			AssertHasMessageError(reroute.Z9_ProcessingPortInfo, "The code you have selected is not in the list.");

			reroute.Z9_ProcessingPort = "1234";
			AssertNoMessageError(reroute.Z9_ProcessingPortInfo, "The code you have selected is not in the list.");
		}
	}
}
