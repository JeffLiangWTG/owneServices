using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IPaymentAuthorisation
	{
		ZString PayersUnitNumber { get; set; }
		ZString PaymentType { get; set; }
		ZString StatementFiler { get; set; }
		ZString StatementBillNumber { get; set; }
		ZDecimal PaymentAmount { get; set; }

		//for some reason customs expect this to be the preparer port code....ie RLF processing port 1704 and entry port 1704 and
		//preparer district port is 2904, then 2904 should go in position 4-7 of the B Block
		ZString ProcessingPortCode { get; }

		ZString ClientBranchDesignation { get; }

		ZString NegationCode { get; set; }
		ZDate NegationDate { get; set; }
	}
}
