using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Integration
{
	public interface IOneStopCarrierCodePairListProvider
	{
		CodeDescriptionPairList GetOneStopCarrierCodePairListForFilter();
	}
}
