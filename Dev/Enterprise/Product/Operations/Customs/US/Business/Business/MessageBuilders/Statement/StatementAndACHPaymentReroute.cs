using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class StatementAndACHPaymentReroute : AutoStatementAndACHPaymentReroute
	{
		public StatementAndACHPaymentReroute()
			: base(new BusinessObjectFactory())
		{
		}

		public void SendRequest()
		{
			var requester = new MiscellaneousMessageRequester();
			MessageBlock qrBlock = null;
			var applicationIdentifier = ZString.Empty;

			if (IsACE)
			{
				applicationIdentifier = ACEApplicationIdentifierCodeList.Codes.StatementRequestReroute;
				qrBlock = new PMSQR()
				{
					ImporterOfRecordNumber = Z9_ImportOfRecordNumber,
					ClientBranch = Z9_ClientBranch,
					StatementNumber = Z9_StatementNumber,
					ScopeIndicator = Z9_AllDocumentsRequest ? "A" : "",
					TransmissionDateOfStatement = Z9_TranmissionDate.Date,
					PreliminaryStatementRequest = Z9_PreliminaryStatementRequest && !IsPeriodicMonthlyStatement ? "Y" : "N",
					FinalStatementRequest = Z9_FinalStatementRequest && !IsPeriodicMonthlyStatement ? "Y" : "N",
					PreliminaryPeriodicMonthlyStatementRequest = Z9_PreliminaryStatementRequest && IsPeriodicMonthlyStatement ? "Y" : "N",
					FinalPeriodicMonthlyStatementRequest = Z9_FinalStatementRequest && IsPeriodicMonthlyStatement ? "Y" : "N",
				};
			}
			else if (IsPeriodicMonthlyStatement)
			{
				applicationIdentifier = ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatementReroute;
				qrBlock = new PMSQR()
				{
					ImporterOfRecordNumber = Z9_ImportOfRecordNumber,
					ClientBranch = Z9_ClientBranch,
					StatementNumber = Z9_StatementNumber,
					ScopeIndicator = Z9_AllDocumentsRequest ? "A" : "",
					TransmissionDateOfStatement = Z9_TranmissionDate.Date,
					PreliminaryStatementRequest = Z9_PreliminaryStatementRequest ? "Y" : "N",
					FinalStatementRequest = Z9_FinalStatementRequest ? "Y" : "N"
				};
			}
			else
			{
				applicationIdentifier = ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentReroute;
				qrBlock = new DSTQR()
				{
					ImporterOfRecordNumber = Z9_ImportOfRecordNumber,
					ClientBranch = Z9_ClientBranch,
					StatementNumber = Z9_StatementNumber,
					ScopeIndicator = Z9_AllDocumentsRequest ? "A" : "",
					TransmissionDateOfStatementOrACHPaymentTransaction = Z9_TranmissionDate.Date,
					PreliminaryStatementRequest = Z9_PreliminaryStatementRequest ? "Y" : "N",
					FinalStatementRequest = Z9_FinalStatementRequest ? "Y" : "N",
					PeriodicStatementPaymentAuthorizationRequest = Z9_PeriodicStatementPaymentAuthorizationRequest ? "Y" : "N",
					ACHPaymentRequest = Z9_ACHPaymentRequest ? "Y" : "N"
				};
			}

			requester.RequestStatement(IsACE, applicationIdentifier, qrBlock, Z9_ProcessingPort);
		}

		public bool IsPeriodicMonthlyStatement
		{
			get { return Z9_RerouteType == StatementTypeList.Codes.PeriodicMonthly; }
		}

		public bool IsACE
		{
			get { return Z9_MessageType == JobApplicationCodeList.Codes.ACE; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			Z9_MessageType = JobApplicationCodeList.Codes.ACE;
			Z9_RerouteType = StatementTypeList.Codes.Daily;
			Z9_TranmissionDate = ZDate.Today;
			Z9_ProcessingPort = InputBlockControlGeneratorHelper.GetProcessingDistrictPortCode(GlbBranch.CurrentBranch);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Statement and/or ACH Payment Reroute"; }
		}
	}
}
