using CargoWise.Types;
using Enterprise.ZArchitecture.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IRatingContractNamedAccountPivot : IPivotBusinessObject
	{
		public ZGuid RNP_OH_NamedAccount { get; set; }
	}
}
