using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface IAuthorityToActDeclarationProvider
	{
		ZString CountryCode { get; }
		GlbCompany Company { get; }
		JobDocsAndCartage DocsAndCartage { get; }
		Guid RegistryCompanyPK { get; }
	}
}
