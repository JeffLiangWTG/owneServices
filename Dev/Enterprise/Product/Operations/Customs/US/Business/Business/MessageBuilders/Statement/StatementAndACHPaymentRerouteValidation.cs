//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStatementAndACHPaymentRerouteValidation
//
//    This class should be used for overriding validation in AutoStatementAndACHPaymentRerouteValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class StatementAndACHPaymentRerouteValidation : AutoStatementAndACHPaymentRerouteValidation
	{
		public StatementAndACHPaymentRerouteValidation(AutoStatementAndACHPaymentReroute parent)
			: base(parent)
		{
		}

		protected new StatementAndACHPaymentReroute Parent
		{
			get { return (StatementAndACHPaymentReroute)base.Parent; }
		}

		protected override void CheckZ9_RerouteType()
		{
			base.CheckZ9_RerouteType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.Z9_RerouteTypeInfo, Parent.Lookups.Z9_RerouteTypeList);
		}

		protected override void CheckZ9_TranmissionDate()
		{
			base.CheckZ9_TranmissionDate();
			if (Parent.Z9_TranmissionDate.IsValid)
			{
				if (IsPeriodicMonthlyStatementOrShouldGenerateNewQRBlock && !Parent.Z9_StatementNumber.IsEmpty)
				{
					Parent.Z9_TranmissionDateInfo.AddMessageError(StatementNumberSubmitted);
				}
				else if (Parent.Z9_TranmissionDate.Date > ZDate.Today || (ZDate.Today.AddDays(-NumberOfDaysInThePastAllowed) > Parent.Z9_TranmissionDate.Date))
				{
					Parent.Z9_TranmissionDateInfo.AddMessageError(InvalidTransmissionDate);
				}
			}
		}
		internal const string InvalidTransmissionDate = "Transmission date must be more than 14 days in the past.";
		internal const int NumberOfDaysInThePastAllowed = 14;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateThatThereIsARouteRequest();
		}

		void ValidateThatThereIsARouteRequest()
		{
			if (Parent.IsPeriodicMonthlyStatement)
			{
				if (!Parent.Z9_PreliminaryStatementRequest && !Parent.Z9_FinalStatementRequest)
				{
					Parent.AddRowError(NoMonthlyReroutesRequested);
				}
			}
			else
			{
				if (!Parent.Z9_PreliminaryStatementRequest && !Parent.Z9_FinalStatementRequest && !Parent.Z9_ACHPaymentRequest && !Parent.Z9_PeriodicStatementPaymentAuthorizationRequest)
				{
					Parent.AddRowError(NoDailyReroutesRequested);
				}
			}
		}
		internal const string NoDailyReroutesRequested = "Please select at least a Preliminary Statement or a Final Statement or an ACH Payment or a Periodic Statement Payment.";
		internal const string NoMonthlyReroutesRequested = "Please select at least a Preliminary Statement or a Final Statement.";

		protected override void CheckZ9_ImportOfRecordNumber()
		{
			base.CheckZ9_ImportOfRecordNumber();
			if (IsPeriodicMonthlyStatementOrShouldGenerateNewQRBlock && !Parent.Z9_ImportOfRecordNumber.IsEmpty)
			{
				if (Parent.Z9_AllDocumentsRequest)
				{
					Parent.Z9_ImportOfRecordNumberInfo.AddMessageError(ScopeIndicatorSubmitted);
				}
				else if (!Parent.Z9_StatementNumber.IsEmpty)
				{
					Parent.Z9_ImportOfRecordNumberInfo.AddMessageError(StatementNumberSubmitted);
				}
			}
		}
		internal const string ScopeIndicatorSubmitted = "If request for all ports, do not submit the Importer of Record Number, Client Branch Identifier, or Periodic Monthly Statement Number fields.";
		internal const string StatementNumberSubmitted = "If the Statement Number field is submitted, do not submit the Transmission Date, Importer of Record Number, Client Branch Identifier, or Scope Indicator fields.";

		protected override void CheckZ9_ClientBranch()
		{
			base.CheckZ9_ClientBranch();
			if (IsPeriodicMonthlyStatementOrShouldGenerateNewQRBlock && !Parent.Z9_ClientBranch.IsEmpty)
			{
				if (Parent.Z9_AllDocumentsRequest)
				{
					Parent.Z9_ClientBranchInfo.AddMessageError(ScopeIndicatorSubmitted);
				}
				else if (!Parent.Z9_StatementNumber.IsEmpty)
				{
					Parent.Z9_ClientBranchInfo.AddMessageError(StatementNumberSubmitted);
				}
			}
		}

		protected override void CheckZ9_StatementNumber()
		{
			base.CheckZ9_StatementNumber();
			if (IsPeriodicMonthlyStatementOrShouldGenerateNewQRBlock && Parent.Z9_AllDocumentsRequest && !Parent.Z9_StatementNumber.IsEmpty)
			{
				Parent.Z9_StatementNumberInfo.AddMessageError(ScopeIndicatorSubmitted);
			}
			ValidateZ9_ImportOfRecordNumber();
			ValidateZ9_ClientBranch();
			ValidateZ9_TranmissionDate();
		}

		protected override void CheckZ9_AllDocumentsRequest()
		{
			base.CheckZ9_AllDocumentsRequest();
			ValidateZ9_StatementNumber();
		}

		ZBool IsPeriodicMonthlyStatementOrShouldGenerateNewQRBlock
		{
			get { return Parent.IsPeriodicMonthlyStatement || Parent.IsACE; }
		}

		protected override void CheckZ9_ProcessingPort()
		{
			base.CheckZ9_ProcessingPort();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.Z9_ProcessingPortInfo, Parent.Lookups.Z9_ProcessingPortList);
		}
	}
}
