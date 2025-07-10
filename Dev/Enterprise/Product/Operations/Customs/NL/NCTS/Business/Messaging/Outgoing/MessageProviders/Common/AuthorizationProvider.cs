using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.NL.NCTS.Business;

public class AuthorizationProvider : INCTSAuthorization
{
	public AuthorizationProvider(CusAuthorizationUsage cusAuthorizationUsage, ZInt sequenceNumber)
	{
		this.cusAuthorizationUsage = Argument.NotNull(cusAuthorizationUsage, nameof(cusAuthorizationUsage));
		SequenceNumeric = sequenceNumber;
	}
	readonly CusAuthorizationUsage cusAuthorizationUsage;

	public int SequenceNumeric { get; }

	public string Type => cusAuthorizationUsage.CustomsCode;

	public string ReferenceNumber => cusAuthorizationUsage.AGC_Number;

	public string HolderOfAuthorisation => null;
}
