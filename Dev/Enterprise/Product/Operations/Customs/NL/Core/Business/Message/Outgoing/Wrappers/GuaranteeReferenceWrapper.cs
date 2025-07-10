using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class GuaranteeReferenceWrapper : IGuaranteeReference
{
	public GuaranteeReferenceWrapper(EU.Business.Declaration.GuaranteeForDeclaration bondDetail, int sequenceNumeric)
	{
		this.bondDetail = Argument.NotNull(bondDetail, nameof(bondDetail));
		SequenceNumeric = sequenceNumeric;
	}
	readonly EU.Business.Declaration.GuaranteeForDeclaration bondDetail;

	public int SequenceNumeric { get; }

	public decimal Amount => bondDetail.PW_BondAmount;

	public string Currency => bondDetail.PW_RX_NKCurrency;

	public string Id => !bondDetail.PW_BondNumber2.Equals(NLConstants.GuaranteeReferenceTypes.Guarantee) ? bondDetail.PW_BondNumber : ZString.Empty;

	public string AccessCode => bondDetail.PW_Password;

	public string ReferenceId => bondDetail.PW_BondNumber2.Equals(NLConstants.GuaranteeReferenceTypes.Guarantee) ? bondDetail.PW_BondNumber : ZString.Empty;

	public string GuaranteeOffice => bondDetail.PW_BondFiledPort;

	public string GRN => string.Empty;
}
