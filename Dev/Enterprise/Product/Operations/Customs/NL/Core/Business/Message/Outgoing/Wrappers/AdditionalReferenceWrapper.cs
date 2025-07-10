using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class AdditionalReferenceWrapper : IAdditionalReference
{
	public AdditionalReferenceWrapper(CusReference reference, int sequenceNumeric)
	{
		SequenceNumeric = sequenceNumeric;
		Id = Argument.NotNull(reference, nameof(reference)).CFR_Reference;
		Code = reference.CFR_Code;
	}

	public AdditionalReferenceWrapper(CusSupportingInfo supportingInfo, int sequenceNumeric)
	{
		SequenceNumeric = sequenceNumeric;
		Id = Argument.NotNull(supportingInfo, nameof(supportingInfo)).CSI_ReferenceNumber;
		Code = (!supportingInfo.CSI_Procedure.IsEmpty ? supportingInfo.CSI_Procedure : supportingInfo.CSI_Code);
	}

	public string Id { get; }

	public string Code { get; }

	public int SequenceNumeric { get; }

	public string CCQualifierCode => null;
}
