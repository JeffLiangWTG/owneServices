using CargoWise.Types;
namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	[OutputBlock("R10")]
	public partial class RECR10 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	[OutputBlock("R15")]
	public partial class RECR15 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	[OutputBlock("R16")]
	public partial class RECR16 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	[OutputBlock("R17")]
	public partial class RECR17 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	[OutputBlock("R20")]
	public partial class RECR20 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	[OutputBlock("R21")]
	public partial class RECR21 : MessageBlock
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "called by messageBlockDecimal, which is used for unit test")]
		FillType FirstFeeFillType
		{
			get { return GetFillTypeForFeeAmount(FirstFeeClass); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "called by messageBlockDecimal, which is used for unit test")]
		FillType SecondFeeFillType
		{
			get { return GetFillTypeForFeeAmount(SecondFeeClass); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "called by messageBlockDecimal, which is used for unit test")]
		FillType ThirdFeeFillType
		{
			get { return GetFillTypeForFeeAmount(ThirdFeeClass); }
		}

		FillType GetFillTypeForFeeAmount(ZString dependentFeeCode)
		{
			return dependentFeeCode.IsEmpty ? FillType.ZeroFillUnlessEmpty : FillType.AlwaysZeroFill;
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	[OutputBlock("R89")]
	public partial class RECR89 : MessageBlock
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "called by messageBlockDecimal, which is used for unit test")]
		FillType FeeClassFillType
		{
			get { return GetFillTypeForFeeAmount(FeeClass); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "called by messageBlockDecimal, which is used for unit test")]
		FillType FeeClass1FillType
		{
			get { return GetFillTypeForFeeAmount(FeeClass1); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "called by messageBlockDecimal, which is used for unit test")]
		FillType FeeClass2FillType
		{
			get { return GetFillTypeForFeeAmount(FeeClass2); }
		}

		FillType GetFillTypeForFeeAmount(ZString dependentFeeCode)
		{
			return dependentFeeCode.IsEmpty ? FillType.ZeroFillUnlessEmpty : FillType.AlwaysZeroFill;
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	[OutputBlock("R90")]
	public partial class RECR90 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ReconciliationEntryFilingResponse)]
	[OutputBlock("R91")]
	public partial class RECR91 : MessageBlock { }
}
