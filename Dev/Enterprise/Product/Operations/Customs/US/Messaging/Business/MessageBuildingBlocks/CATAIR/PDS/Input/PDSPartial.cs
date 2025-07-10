using System;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentation)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation)]
	partial class PDSPT : MessageBlock, IPaymentAuthorisation
	{
		#region IPaymentAuthorisation Members
		ZString IPaymentAuthorisation.PayersUnitNumber
		{
			get { return PayersUnitNumber; }
			set { PayersUnitNumber = value; }
		}

		ZString IPaymentAuthorisation.PaymentType
		{
			get { return PaymentType; }
			set { PaymentType = value; }
		}

		ZString IPaymentAuthorisation.StatementFiler
		{
			get { return StatementFiler; }
			set { StatementFiler = value; }
		}

		ZString IPaymentAuthorisation.StatementBillNumber
		{
			get { return StatementNumber; }
			set { StatementNumber = value; }
		}

		ZDecimal IPaymentAuthorisation.PaymentAmount
		{
			get { return PaymentAmount; }
			set { PaymentAmount = value; }
		}

		ZString IPaymentAuthorisation.ProcessingPortCode
		{
			get { throw new InvalidOperationException(); }
		}

		ZString IPaymentAuthorisation.ClientBranchDesignation
		{
			get { return ZString.Empty; }
		}

		ZString IPaymentAuthorisation.NegationCode
		{
			get { return NegationCode; }
			set { NegationCode = value; }
		}

		ZDate IPaymentAuthorisation.NegationDate
		{
			get { return NegationDate; }
			set { NegationDate = value; }
		}

		#endregion
	}
}
