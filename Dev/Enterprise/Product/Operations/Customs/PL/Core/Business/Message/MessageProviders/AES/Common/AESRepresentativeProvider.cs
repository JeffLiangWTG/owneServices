using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESRepresentativeProvider : RepresentativeProvider, IAESRepresentative
{
	AESRepresentativeProvider(OrgHeader orgHeader)
		: base(orgHeader)
	{
		this.orgHeader = orgHeader;
	}
	readonly OrgHeader orgHeader;

	public new static AESRepresentativeProvider NewOrNull(OrgHeader orgHeader) => orgHeader != null ? new AESRepresentativeProvider(orgHeader) : null;

	public IPersonIdentificationNumbers IdentificationNumbers => CachedValueHelper.GetValue(ref identificationNumbers, () => new AESPersonIdentificationNumbersProvider(orgHeader.CustomsCodes));
	CachedValue<IPersonIdentificationNumbers> identificationNumbers;
}
