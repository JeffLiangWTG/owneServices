using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESPersonIdentificationNumbersProvider : IPersonIdentificationNumbers
{
	public AESPersonIdentificationNumbersProvider(OrgCusCodeCollection cusCodeCodeCollection)
	{
		this.cusCodeCodeCollection = cusCodeCodeCollection;
	}

	readonly OrgCusCodeCollection cusCodeCodeCollection;
	const int RegonLength = 14;

	public string NIP => CachedValueHelper.GetValue(ref tin,
		() => cusCodeCodeCollection.GetCustomsRegNo(OrgCusCode.PolandCodeTypes.NIP, Core.Constants.CountryCodes.Poland));
	CachedValue<string> tin;

	public string Regon => CachedValueHelper.GetValue(ref regon, GetRegon);
	CachedValue<string> regon;

	public string PESEL => CachedValueHelper.GetValue(ref pesel,
		() => cusCodeCodeCollection.GetCustomsRegNo(OrgCusCode.PolandCodeTypes.PES, Core.Constants.CountryCodes.Poland));
	CachedValue<string> pesel;

	public string OtherIdentificationNumber => CachedValueHelper.GetValue(ref otherIdentificationNumber,
		() => cusCodeCodeCollection.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, Core.Constants.CountryCodes.Poland));
	CachedValue<string> otherIdentificationNumber;

	string GetRegon()
	{
		var customsRegNo = cusCodeCodeCollection.GetCustomsRegNo(OrgCusCode.CodeTypes.GovBusinessCode, Core.Constants.CountryCodes.Poland);
		return customsRegNo.IsEmpty ? null : customsRegNo.PadRight(RegonLength, '0').ToString();
	}
}
