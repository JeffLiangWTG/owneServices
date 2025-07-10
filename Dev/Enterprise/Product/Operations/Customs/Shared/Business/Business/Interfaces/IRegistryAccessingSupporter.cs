using System;

namespace Enterprise.Customs.Business
{
	public interface IRegistryAccessingSupporter
	{
		Guid RegistryCompanyPK { get; }
		Guid RegistryBranchPK { get; }
	}
}
