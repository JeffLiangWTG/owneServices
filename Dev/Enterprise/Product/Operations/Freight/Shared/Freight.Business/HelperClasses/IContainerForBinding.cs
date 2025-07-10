using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public interface IContainerForBinding
	{
		CommonContainer JobContainer { get; }

		ZString ContainerNumberForBinding { get; set; }
		ZPropertyInfo ContainerNumberForBindingInfo { get; }

		ZString SealNumberForBinding { get; set; }
		ZPropertyInfo SealNumberForBindingInfo { get; }

		ZGuid RCForBinding { get; set; }
		ZPropertyInfo RCForBindingInfo { get; }

		ZString ContainerModeForBinding { get; set; }
		ZPropertyInfo ContainerModeForBindingInfo { get; }
		CodeDescriptionPairList ContainerMode_ListForBinding { get; }

		ZString DeliveryModeForBinding { get; set; }
		ZPropertyInfo DeliveryModeForBindingInfo { get; }

		ZDecimal GoodsWeightForBinding { get; set; }
		ZPropertyInfo GoodsWeightForBindingInfo { get; }

		ZString WeightUnitForBinding { get; set; }
		ZPropertyInfo WeightUnitForBindingInfo { get; }
	}
}

