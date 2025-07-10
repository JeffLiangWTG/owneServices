using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IOrgHeaderPostingValidation
	{
		(ResourceString errorOrWarningMessage, bool isErrorMessage) CheckOrgHeaderForPosting(AccTransactionHeader transaction);
	}
}
