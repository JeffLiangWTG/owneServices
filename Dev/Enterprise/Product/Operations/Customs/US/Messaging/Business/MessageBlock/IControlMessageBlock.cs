using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IControlMessageBlock
	{
		string Serialise();

		string Serialise(bool humanFriendly);

		void Deserialise(string eightyCharacterBlock);
	}

	public interface IABIControlMessageBlockA : IControlMessageBlockA
	{
		ZString ReceiverSiteCode { get; set; }

		ZString ReceiverFilerCode { get; set; }

		ZDate TransmissionDate { get; set; }

		ZString ReceiverOfficeCode { get; set; }
		ZDate CurrentDate { get; set; }
	}

	public interface IAESControlMessageBlockA : IControlMessageBlockA
	{
		ZString BatchControlNumber { get; set; }
	}

	public interface IAMSControlMessageBlockA : IControlMessageBlockA
	{
		ZString Password { set; }
	}

	public interface IControlMessageBlockA : IControlMessageBlock
	{
		ZString ApplicationIdentifier { get; set; }
		ZString FilerID { get; set; }
	}

	public interface IABIControlMessageBlockB : IControlMessageBlockB
	{
		ZString ProcessingDistrictPortCode { get; set; }
		ZString EntryFilerCode { get; set; }
		ZString ProcessingOfficeCode { get; set; }
		ZString PreparerDistrictPort { get; set; }

		//StatementNumber, PreliminaryStatementDate, PaymentType, ClientBranchDesignation and ImporterOfRecord are statement-related 
		//and there is no situation where setters are needed.
		ZString StatementNumber { get; }
		ZDate PreliminaryStatementPrintDate { get; }
		ZString PaymentTypeIndicator { get; }
		ZString ClientBranchDesignation { get; }
		ZString ImporterOfRecordNumber { get; }
		ZString StatementStatus { get; }

		ZString UserData { get; set; }

		ZString PreparerIndicator { get; set; }
		ZString PreparerFilerCode { get; set; }
		ZString PreparerOfficeCode { get; set; }
	}

	public interface IAESControlMessageBlockB : IControlMessageBlockB
	{
		ZString USPPIID { get; set; }
		ZString USPPIIDType { get; set; }
		ZString USPPIName { get; set; }
	}

	public interface IAMSControlMessageBlockB : IControlMessageBlockB
	{
	}

	public interface IControlMessageBlockB : IControlMessageBlock
	{
		ZString ApplicationIdentifier { get; set; }
	}

	public interface IAMSControlMessageBlockY : IControlMessageBlockY
	{
	}

	public interface IABIControlMessageBlockY : IControlMessageBlockY
	{
		ZString ApplicationIdentifier { get; set; }

		ZString ProcessingDistrictPortCode { get; set; }

		ZString EntryFilerCode { get; set; }

		ZInt NumberOfTransactionDetailRecordsInTheBlock { get; set; }
	}

	public interface IAESControlMessageBlockY : IControlMessageBlockY
	{
	}

	public interface IControlMessageBlockY : IControlMessageBlock
	{
	}

	public interface IABIControlMessageBlockZ : IControlMessageBlockZ
	{
		ZString ReceiverSiteCode { get; set; }

		ZString ReceiverFilerCode { get; set; }

		ZDate TransmissionDate { get; set; }

		ZString ReceiverOfficeCode { get; set; }
	}

	public interface IAESControlMessageBlockZ : IControlMessageBlockZ
	{
	}

	public interface IAMSControlMessageBlockZ : IControlMessageBlockZ
	{
	}

	public interface IControlMessageBlockZ : IControlMessageBlock
	{
	}
}
