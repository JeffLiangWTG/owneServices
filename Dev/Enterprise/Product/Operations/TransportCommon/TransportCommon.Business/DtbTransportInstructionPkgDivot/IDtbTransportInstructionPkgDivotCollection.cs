using CargoWise.EntityFramework;

namespace Enterprise.TransportCommon.Business
{
	public interface IDtbTransportInstructionPkgDivotCollection : IActiveBusinessObjectCollection
	{
		new DtbTransportInstructionPkgDivot this[int index] { get; }
	}
}
