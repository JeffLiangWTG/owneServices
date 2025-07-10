using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[RegistryEditor("Enterprise.Customs.US.DataRegistry.GUI.EntryFilerRegistryItemEditor, Enterprise.Customs.US.GUI")]
	sealed class EntryFilerRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EntryFiler>
	{
		protected override bool ValuesAreEqualCore(EntryFiler a, EntryFiler b)
		{
			return Equals(a, b);
		}
	}
}
