using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface ITaxMessagesGroupProvider
	{
		CodeDescriptionBoolRelatedItemCollection GetTaxMessageGroup();
	}
}
