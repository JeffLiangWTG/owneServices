using CargoWise.EntityFramework;

namespace Enterprise.TransportCommon.Business
{
	public interface IDtbTransportInstructionCollection : IActiveBusinessObjectCollection
	{
		new DtbTransportInstruction this[int index] { get; }

		void Sequence();
	}
}
