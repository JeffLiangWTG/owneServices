using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IReverseDateValidation
	{
		ResourceString ValidateReverseDate(AccTransactionLines line);
	}
}
