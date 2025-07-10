using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESDeclarantWithIdentificationNumbersProvider : AESDeclarantProvider, IAESDeclarantWithIdentificationNumbers
{
	protected AESDeclarantWithIdentificationNumbersProvider(OrgAddress orgAddress, JobDeclaration jobDeclaration)
		: base(orgAddress, jobDeclaration)
	{
	}

	public new static AESDeclarantWithIdentificationNumbersProvider NewOrNull(OrgAddress orgAddress, JobDeclaration jobDeclaration) => orgAddress?.Header is not null
		? new AESDeclarantWithIdentificationNumbersProvider(orgAddress, jobDeclaration)
		: null;

	public IPersonIdentificationNumbers IdentificationNumbers => CachedValueHelper.GetValue(ref identificationNumbers, () => new AESPersonIdentificationNumbersProvider(orgHeader.CustomsCodes));
	CachedValue<IPersonIdentificationNumbers> identificationNumbers;
}
