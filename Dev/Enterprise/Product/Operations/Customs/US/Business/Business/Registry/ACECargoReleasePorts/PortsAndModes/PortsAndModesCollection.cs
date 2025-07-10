using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public class PortsAndModesCollection : NonPersistentBusinessObjectCollection<PortsAndModes>
	{
		public PortsAndModesCollection(BusinessObjectFactory factory)
			: base(factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PortsAndModes(this);
		}
	}
}
