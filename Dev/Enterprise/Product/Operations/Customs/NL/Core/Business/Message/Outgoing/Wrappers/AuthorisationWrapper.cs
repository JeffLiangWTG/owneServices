using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class AuthorisationWrapper : IAuthorisation
{
	public AuthorisationWrapper(CusAuthorizationUsage cusAuthorizationUsage, int sequenceNumeric)
	{
		this.cusAuthorizationUsage = Argument.NotNull(cusAuthorizationUsage, nameof(cusAuthorizationUsage));
		this.SequenceNumeric = sequenceNumeric;
	}
	readonly CusAuthorizationUsage cusAuthorizationUsage;

	public string Id => cusAuthorizationUsage.AGC_Number;

	public string TypeCode => WrapperHelper.ConvertAuthorizationUsageCode(cusAuthorizationUsage.AGC_Code);

	public string HolderId
	{
		get
		{
			var orgHeader = cusAuthorizationUsage.Factory.Load<OrgHeader>(cusAuthorizationUsage.AGC_OH_Owner);
			return orgHeader?.GetIdentificationNumber();
		}
	}

	public int SequenceNumeric { get; }
}
