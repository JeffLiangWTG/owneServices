using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.Business;

public class ObligationGuaranteeWrapper : IObligationGuarantee
{
	public ObligationGuaranteeWrapper(EU.Business.Declaration.GuaranteeForDeclaration bondDetail, int sequenceNumeric)
	{
		this.bondDetail = Argument.NotNull(bondDetail, nameof(bondDetail));
		SequenceNumeric = sequenceNumeric;
	}
	readonly EU.Business.Declaration.GuaranteeForDeclaration bondDetail;

	public int SequenceNumeric { get; }

	public string SecurityDetailsCode => bondDetail.PW_BondType;

	public IReadOnlyCollection<IGuaranteeReference> GuaranteeReferences => guaranteeReferences ??=CreateListWithOneGuarantee().ToArray();
	IReadOnlyCollection<IGuaranteeReference> guaranteeReferences;

	IEnumerable<IGuaranteeReference> CreateListWithOneGuarantee()
	{
		yield return new GuaranteeReferenceWrapper(bondDetail, 1);
	}
}
