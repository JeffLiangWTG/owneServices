using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobInvoicingAdditionalData
	{
		CustomPropertyContainer<JobCharge> GetAdditionalProperties();
	}
}