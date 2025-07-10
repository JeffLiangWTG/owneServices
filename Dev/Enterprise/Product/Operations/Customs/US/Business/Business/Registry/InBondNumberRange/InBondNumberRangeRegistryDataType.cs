using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[RegistryEditor("Enterprise.Customs.US.DataRegistry.GUI.InBondNumberRangeRegistryItemEditor, Enterprise.Customs.US.GUI")]
	sealed class InBondNumberRangeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InBondNumberRange>
	{
		protected override bool ValuesAreEqualCore(InBondNumberRange a, InBondNumberRange b)
		{
			return Equals(a, b);
		}
	}
}
