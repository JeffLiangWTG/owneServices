using CargoWise.Types;
namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG01")]
	public partial class PGAPG01 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG04")]
	public partial class PGAPG04 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG06")]
	public partial class PGAPG06 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG15")]
	public partial class PGAPG15 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG16")]
	public partial class PGAPG16 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG25")]
	public partial class PGAPG25 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG27")]
	public partial class PGAPG27 : MessageBlock, ILaceyActContainer
	{
		ZString ILaceyActContainer.ContainerEquipmentID
		{
			get { return ContainerEquipmentID; }
			set { ContainerEquipmentID = value; }
		}

		ZString ILaceyActContainer.ContainerEquipmentID1
		{
			get { return ContainerEquipmentID1; }
			set { ContainerEquipmentID1 = value; }
		}

		ZString ILaceyActContainer.ContainerEquipmentID2
		{
			get { return ContainerEquipmentID2; }
			set { ContainerEquipmentID2 = value; }
		}
	}
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG02")]
	public partial class PGAPG02 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG03")]
	public partial class PGAPG03 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG07")]
	public partial class PGAPG07 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG08")]
	public partial class PGAPG08 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG09")]
	public partial class PGAPG09 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG10")]
	public partial class PGAPG10 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG11")]
	public partial class PGAPG11 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG12")]
	public partial class PGAPG12 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG13")]
	public partial class PGAPG13 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG14")]
	public partial class PGAPG14 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG17")]
	public partial class PGAPG17 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG18")]
	public partial class PGAPG18 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG19")]
	public partial class PGAPG19 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG20")]
	public partial class PGAPG20 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG21")]
	public partial class PGAPG21 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG22")]
	public partial class PGAPG22 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG23")]
	public partial class PGAPG23 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG24")]
	public partial class PGAPG24 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG26")]
	public partial class PGAPG26 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG28")]
	public partial class PGAPG28 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG29")]
	public partial class PGAPG29 : MessageBlock
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("PG30")]
	public partial class PGAPG30 : MessageBlock
	{
	}
}