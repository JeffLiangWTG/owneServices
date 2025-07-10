using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInstructionPkgDivotCollectionTest<TBusinessObject, TCollection> : ActiveBusinessObjectCollectionTestCase<TCollection>
			where TBusinessObject : DtbTransportInstructionPkgDivot
			where TCollection : DtbTransportInstructionPkgDivotCollection<TBusinessObject>
	{
	}
}
