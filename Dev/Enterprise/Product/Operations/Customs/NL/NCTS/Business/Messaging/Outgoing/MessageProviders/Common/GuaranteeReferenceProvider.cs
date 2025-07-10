using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.NL.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.NL.NCTS.Business;

public class GuaranteeReferenceProvider : IGuaranteeReference
{
	readonly EU.NCTS.Business.NctsGuarantee nctsGuarantee;

	public GuaranteeReferenceProvider(EU.NCTS.Business.NctsGuarantee nctsGuarantee, ZInt sequence)
	{
		this.nctsGuarantee = Argument.NotNull(nctsGuarantee, nameof(nctsGuarantee));
		SequenceNumeric = sequence;
	}

	public int SequenceNumeric { get; }

	public virtual string GRN => nctsGuarantee.PW_BondNumber;

	public string AccessCode => nctsGuarantee.PW_Password;

	public decimal Amount => nctsGuarantee.PW_BondAmount.Round(2);

	public string Currency
	{
		get
		{
			if (!Amount.IsZero())
			{
				var result = Core.Constants.CurrencyCodes.EuropeanUnion;
				if (!nctsGuarantee.PW_RX_NKCurrency.IsEmpty)
				{
					result = nctsGuarantee.PW_RX_NKCurrency;
				}
				else if (nctsGuarantee.CusGuarantee is CusGuaranteeHeader cusGuarantee && !cusGuarantee.CPH_UnitOfMeasure.IsEmpty)
				{
					result = cusGuarantee.CPH_UnitOfMeasure.ToString();
				}
				return result;
			}

			return null;
		}
	}

	public string Id => string.Empty;

	public string ReferenceId => string.Empty;

	public string GuaranteeOffice => string.Empty;
}
