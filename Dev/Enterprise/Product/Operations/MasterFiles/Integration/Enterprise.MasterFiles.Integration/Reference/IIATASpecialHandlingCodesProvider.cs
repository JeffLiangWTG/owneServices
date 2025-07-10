using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration.Reference
{
	public interface IIATASpecialHandlingCodesProvider
	{
		CodeDescriptionPairList GetCodeDescriptionPairList();
	}
}
