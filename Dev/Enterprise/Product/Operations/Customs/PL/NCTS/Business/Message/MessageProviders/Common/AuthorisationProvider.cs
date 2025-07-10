using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class AuthorisationProvider : IAuthorisation
{
	public AuthorisationProvider(CusAuthorizationUsage cusAuthorizationUsage, ZInt sequenceNumber)
	{
		this.cusAuthorizationUsage = Argument.NotNull(cusAuthorizationUsage, nameof(cusAuthorizationUsage));
		SequenceNumber = sequenceNumber.ToString();
	}

	readonly CusAuthorizationUsage cusAuthorizationUsage;

	public string SequenceNumber { get; }

	public string AuthorisationType => CachedValueHelper.GetValue(ref authorisationType, GetAuthorisationType);
	CachedValue<string> authorisationType;

	public string ReferenceNumber => cusAuthorizationUsage.AGC_Number;

	string GetAuthorisationType()
	{
		var code = cusAuthorizationUsage.AGC_Code;
		var customsValue = ZString.Empty;
		if (!code.IsEmpty)
		{
			customsValue = EUUniversalLookupsHelper.GetUCCAuthorizationCodeCustomsValue(cusAuthorizationUsage.Factory, code);
		}
		return customsValue.IsEmpty ? code : customsValue;
	}
}
