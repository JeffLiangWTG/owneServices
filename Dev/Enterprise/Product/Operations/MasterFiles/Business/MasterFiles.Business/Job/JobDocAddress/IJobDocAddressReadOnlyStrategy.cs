namespace Enterprise.MasterFiles.Business
{
	public interface IJobDocAddressReadOnlyStrategy
	{
		bool ReadOnly { get; }
		bool OrganisationPKReadOnly { get; }
	}
}
