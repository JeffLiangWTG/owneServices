using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class AESAdditionalCodeProvider : IAdditionalCode
{
	public AESAdditionalCodeProvider(CusCodeData cusCode, int sequenceNumber)
	{
		this.cusCode = Argument.NotNull(cusCode, nameof(cusCode));
		this.sequenceNumber = sequenceNumber;
	}

	readonly CusCodeData cusCode;
	readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;
	public string Code => cusCode.CY_Code;
}
