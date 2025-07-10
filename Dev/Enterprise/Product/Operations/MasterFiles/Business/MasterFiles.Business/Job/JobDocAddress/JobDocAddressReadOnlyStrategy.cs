using System;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressReadOnlyStrategy : IJobDocAddressReadOnlyStrategy
	{
		public JobDocAddressReadOnlyStrategy(Func<bool> readOnly = null, Func<bool> organisationPKReadOnly = null)
		{
			ReadOnly = readOnly;
			OrganisationPKReadOnly = organisationPKReadOnly;
		}

		Func<bool> ReadOnly { get; }
		Func<bool> OrganisationPKReadOnly { get; }

		bool IJobDocAddressReadOnlyStrategy.ReadOnly => ReadOnly?.Invoke() ?? false;

		bool IJobDocAddressReadOnlyStrategy.OrganisationPKReadOnly => OrganisationPKReadOnly?.Invoke() ?? false;
	}
}
