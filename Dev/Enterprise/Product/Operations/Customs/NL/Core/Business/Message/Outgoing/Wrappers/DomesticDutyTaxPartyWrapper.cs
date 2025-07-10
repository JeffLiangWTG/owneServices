using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class DomesticDutyTaxPartyWrapper : IDomesticDutyTaxParty
{
	public DomesticDutyTaxPartyWrapper(CusReference reference, int sequenceNumeric)
	{
		SequenceNumeric = sequenceNumeric;
		Id = Argument.NotNull(reference, nameof(reference)).CFR_Reference;
		RoleCode = reference.CFR_Code;
	}

	public DomesticDutyTaxPartyWrapper(CusSupportingInfo supportingInfo, int sequenceNumeric)
	{
		SequenceNumeric = sequenceNumeric;
		Id = Argument.NotNull(supportingInfo, nameof(supportingInfo)).CSI_ReferenceNumber;
		RoleCode = (!supportingInfo.CSI_Procedure.IsEmpty ? supportingInfo.CSI_Procedure : supportingInfo.CSI_Code);
	}

	public string Id { get; }

	public string RoleCode { get; }

	public int SequenceNumeric { get; }
}
