using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESAuthorisationNumberProvider : IAuthorisationNumber
{
	public AESAuthorisationNumberProvider(CusAuthorizationUsage authorisationUsage, int sequenceNumber)
	{
		this.authorisationUsage = Argument.NotNull(authorisationUsage, nameof(authorisationUsage));
		this.sequenceNumber = sequenceNumber;
	}

	readonly CusAuthorizationUsage authorisationUsage;
	readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;

	public string Type => CachedValueHelper.GetValue(ref type, GetAuthorizationType);
	CachedValue<string> type;

	public string ReferenceNumber => authorisationUsage.EffectiveReferenceNumber;

	public string HolderOfAuthorisation => authorisationUsage.Owner.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

	string GetAuthorizationType()
	{
		return authorisationUsage.CustomsCode.IsEmpty ? MessageProviderHelper.ReturnNullIfEmpty(authorisationUsage.AGC_Code) : authorisationUsage.CustomsCode.ToString();
	}
}
