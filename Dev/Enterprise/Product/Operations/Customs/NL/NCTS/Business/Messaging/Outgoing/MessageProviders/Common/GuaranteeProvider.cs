using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.NCTS.Business;

public class GuaranteeProvider : IGuarantee
{
	protected readonly EU.NCTS.Business.NctsGuarantee nctsGuarantee;

	public GuaranteeProvider(EU.NCTS.Business.NctsGuarantee nctsGuarantee, ZInt sequence)
	{
		this.nctsGuarantee = Argument.NotNull(nctsGuarantee, nameof(nctsGuarantee));
		SequenceNumeric = sequence;
	}

	public int SequenceNumeric { get; }

	public string GuaranteeType => nctsGuarantee.PW_BondType;

	public virtual string OtherGuaranteeReference => nctsGuarantee.PW_BondNumber2;

	public IReadOnlyCollection<IGuaranteeReference> GuaranteeReferences
	{
		get
		{
			if (guaranteeReferences == null)
			{
				guaranteeReferences = new List<IGuaranteeReference>();
				if (!nctsGuarantee.PW_BondNumber.IsEmpty || !nctsGuarantee.PW_Password.IsEmpty || !nctsGuarantee.PW_BondAmount.IsEmpty || !nctsGuarantee.PW_RX_NKCurrency.IsEmpty)
				{
					guaranteeReferences.Add(GetGuaranteeReference(nctsGuarantee, 1));
				}
			}
			return guaranteeReferences;
		}
	}
	List<IGuaranteeReference> guaranteeReferences;

	protected virtual IGuaranteeReference GetGuaranteeReference(EU.NCTS.Business.NctsGuarantee nctsGuarantee, ZInt sequence) => new GuaranteeReferenceProvider(nctsGuarantee, sequence);
}
