using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class RepresentativeProvider : IRepresentative
{
	public RepresentativeProvider(JobDocAddress docAddress, IHolderOfTheTransitProcedureWithMaxLength holderOfTheTransitProcedure)
	{
		orgHeader = Argument.NotNull(docAddress, nameof(docAddress)).Address?.Header;
		this.holderOfTheTransitProcedure = Argument.NotNull(holderOfTheTransitProcedure, nameof(holderOfTheTransitProcedure));
	}

	readonly OrgHeader orgHeader;
	readonly IHolderOfTheTransitProcedureWithMaxLength holderOfTheTransitProcedure;

	public string IdentificationNumber => CachedValueHelper.GetValue(ref identificationNumber, () => EuEoriResolver.GetRegNoWithCountryCode(orgHeader));
	CachedValue<string> identificationNumber;

	public string Status => CachedValueHelper.GetValue(ref status, () => orgHeader == null ? ZString.Empty
		: IdentificationNumber == holderOfTheTransitProcedure.IdentificationNumber ? "2" : "3");
	CachedValue<string> status;

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson, () => ContactPersonProvider.NewOrNull(orgHeader));
	CachedValue<IContactPerson> contactPerson;
}
