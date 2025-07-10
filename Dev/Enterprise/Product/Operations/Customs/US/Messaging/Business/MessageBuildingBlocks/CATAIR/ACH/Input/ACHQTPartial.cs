using System;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AutomatedClearinghouse)]
	public partial class ACHQT : MessageBlock, IPaymentAuthorisation
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
			get { return StatementBillNumber; }
			set { StatementBillNumber = value; }
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
			get;
			set;
		}

		ZDate IPaymentAuthorisation.NegationDate
		{
			get;
			set;
		}

		#endregion
	}
}
