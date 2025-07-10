using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class AEOMutualRecognitionPartyWrapper : IAEOMutualRecognitionParty
{
	public AEOMutualRecognitionPartyWrapper(CusReference cusReference, int sequenceNumeric)
	{
		this.cusReference = Argument.NotNull(cusReference, nameof(cusReference));
		SequenceNumeric = sequenceNumeric;
	}
	readonly CusReference cusReference;

	public string Id => cusReference.CFR_Reference;

	public string RoleCode => cusReference.CFR_Code;

	public int SequenceNumeric { get; }
}
