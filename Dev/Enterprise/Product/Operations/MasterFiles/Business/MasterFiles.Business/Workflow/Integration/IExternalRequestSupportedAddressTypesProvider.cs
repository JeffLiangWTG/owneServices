using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IExternalRequestSupportedAddressTypesProvider
	{
		CodeDescriptionPairList SupportedAssigneeAddressTypes { get; }
		CodeDescriptionPairList SupportedReviewerAddressTypes { get; }
	}
}
