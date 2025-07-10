using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class GuaranteesProvider : IGuarantees
	{
		public GuaranteesProvider(GuaranteeForDeclaration guarantee)
		{
			Guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}
		GuaranteeForDeclaration Guarantee { get; }

		public string GuaranteesType => Guarantee.PW_BondType;
		public decimal GuaranteesRatio => Guarantee.EntryInstruction != null ? Guarantee.EntryInstruction.ZG_GuaranteeRatio.RoundAmount() : ZDecimal.Zero;
		public decimal LetterOfBankGuaranteeAmount => Guarantee.PW_BondType == GuaranteeTypeList.Codes.NAKIT || Guarantee.PW_BondType == GuaranteeTypeList.Codes.DIGER ? ZDecimal.Zero : Guarantee.PW_BondAmount.RoundAmount();
		public decimal GuaranteeAmountForCash => Guarantee.PW_BondType == GuaranteeTypeList.Codes.NAKIT ? Guarantee.PW_BondAmount.RoundAmount() : ZDecimal.Zero;
		public decimal OtherAmount => Guarantee.PW_BondType == GuaranteeTypeList.Codes.DIGER ? Guarantee.PW_BondAmount.RoundAmount() : ZDecimal.Zero;
		public string GlobalGuaranteeNo => Guarantee.PW_BondNumber;
		public string Description => Guarantee.PW_GuaranteeDescription;
		public string OtherAmountReference => Guarantee.PW_BondType == GuaranteeTypeList.Codes.DIGER ? Guarantee.PW_GuaranteeDescription : ZString.Empty;
	}
}
