//GatePassShipmentDependentCollection
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassShipmentDependentCollection : CFSShipmentDependentCollection
	{
		public GatePassShipmentDependentCollection(BusinessObjectFactory factory, CFSContainer parent)
			: base(factory, parent)
		{
		}

		public new GatePassShipment this[int index]
		{
			get { return (GatePassShipment)(Elements[index]); }
		}

		public new GatePassShipment AddNew()
		{
			GatePassShipment newShipment = (GatePassShipment)base.AddNew();
			return newShipment;
		}

		public override void Add(BusinessObject businessObject)
		{
			GatePassShipment newShipment = (GatePassShipment)businessObject;
			base.Add(newShipment);
			newShipment.ParentContainerRegistration = fParent;
		}
	}
}
