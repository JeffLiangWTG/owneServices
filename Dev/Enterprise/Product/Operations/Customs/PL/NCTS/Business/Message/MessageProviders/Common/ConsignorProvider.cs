using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class ConsignorProvider : ConsigneeProvider, IConsignor
{
	public ConsignorProvider(JobDocAddress docAddress, bool isInPhase5TransitionPeriod)
		: base(docAddress, isInPhase5TransitionPeriod)
	{
	}

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson, () => ContactPersonProvider.NewOrNull(OrgHeader));
	CachedValue<IContactPerson> contactPerson;
}
