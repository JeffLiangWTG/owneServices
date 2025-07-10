using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentForRegisterEditableTest : ForwardingShipment
	{
		public ForwardingShipmentForRegisterEditableTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void RegisterEditableChildObject(IBusiness child)
		{
			base.RegisterEditableChildObject(child);

			if (child is IDependentBusinessObjectCollection dependentCollection && dependentCollection.TypeOfElements == typeof(JobConShipLink))
			{
				ConsolPivotCollectionRegisteredAsEditableChild = true;
			}
		}
		public bool ConsolPivotCollectionRegisteredAsEditableChild;
	}
}
