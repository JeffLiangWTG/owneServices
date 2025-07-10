using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	[RegistryEditor("Enterprise.Freight.GUI.AllocationMethodDefaultRegistryItemEditor, Enterprise.Freight.GUI")]
	public sealed class AllocationMethodDefaultRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AllocationMethodDefaultHeader>
	{
		public AllocationMethodDefaultRegistryDataType() { }

		public AllocationMethodDefaultRegistryDataType(AllocationMethodDefaultHeader defaultValue)
			: base(defaultValue) { }
	}
}
