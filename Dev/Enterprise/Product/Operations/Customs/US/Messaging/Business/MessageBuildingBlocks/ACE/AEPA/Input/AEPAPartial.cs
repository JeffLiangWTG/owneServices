using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG19")]
	public partial class AEPAPG19 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG20")]
	public partial class AEPAPG20 : IPGABlock
	{
	}
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	[OutputBlock("PG00")]
	public partial class AEPAPG00 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG01")]
	public partial class AEPAPG01 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG02")]
	public partial class AEPAPG02 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG04")]
	public partial class AEPAPG04 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ParticipatingGovernmentAgencies)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG05")]
	public partial class AEPAPG05 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG06")]
	public partial class AEPAPG06 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG07")]
	public partial class AEPAPG07 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG08")]
	public partial class AEPAPG08 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG10")]
	public partial class AEPAPG10 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG13")]
	public partial class AEPAPG13 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG14")]
	public partial class AEPAPG14 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG17")]
	public partial class AEPAPG17 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG18")]
	public partial class AEPAPG18 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG21")]
	public partial class AEPAPG21 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG22")]
	public partial class AEPAPG22 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG23")]
	public partial class AEPAPG23 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG24")]
	public partial class AEPAPG24 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG25")]
	public partial class AEPAPG25 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG26")]
	public partial class AEPAPG26 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG27")]
	public partial class AEPAPG27 : ILaceyActContainer, IPGABlock
	{
		ZString ILaceyActContainer.ContainerEquipmentID
		{
			get { return ContainerNumberEquipmentID; }
			set { ContainerNumberEquipmentID = value; }
		}

		ZString ILaceyActContainer.ContainerEquipmentID1
		{
			get { return ContainerNumberEquipmentID1; }
			set { ContainerNumberEquipmentID1 = value; }
		}

		ZString ILaceyActContainer.ContainerEquipmentID2
		{
			get { return ContainerNumberEquipmentID2; }
			set { ContainerNumberEquipmentID2 = value; }
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG28")]
	public partial class AEPAPG28 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG29")]
	public partial class AEPAPG29 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG30")]
	public partial class AEPAPG30 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG31")]
	public partial class AEPAPG31 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG32")]
	public partial class AEPAPG32 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG33")]
	public partial class AEPAPG33 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG34")]
	public partial class AEPAPG34 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG35")]
	public partial class AEPAPG35 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG50")]
	public partial class AEPAPG50 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG51")]
	public partial class AEPAPG51 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG55")]
	public partial class AEPAPG55 : IPGABlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoRelease)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrection)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PGADataCorrectionResponse)]
	[OutputBlock("PG60")]
	public partial class AEPAPG60 : IPGABlock
	{
	}
}
