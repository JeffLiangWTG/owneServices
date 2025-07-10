using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IAdditionalReferenceNumberTypeProvider
	{
		CodeDescriptionPairList GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode);
	}
}
