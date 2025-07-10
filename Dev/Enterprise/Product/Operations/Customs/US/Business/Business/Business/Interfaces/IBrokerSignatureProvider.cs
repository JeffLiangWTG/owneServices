using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IBrokerSignatureProvider
	{
		Guid RegistryBranchPK { get; }
		Guid RegistryCompanyPK { get; }
		bool HasCurrentElectronicRelease { get; }

		GlbStaff CusAgent { get; }
		BusinessObjectFactory Factory { get; }
	}
}
