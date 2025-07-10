using System;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public sealed class OrganizationRegistryProvider : IOrganizationRegistryProvider
	{
		public bool EnableImportFromCreditReports
		{
			get => OrganisationRegistry.Instance.EnableImportFromCreditReports.Value;
#if DEBUG
			set => OrganisationRegistry.Instance.EnableImportFromCreditReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}
	}
}
