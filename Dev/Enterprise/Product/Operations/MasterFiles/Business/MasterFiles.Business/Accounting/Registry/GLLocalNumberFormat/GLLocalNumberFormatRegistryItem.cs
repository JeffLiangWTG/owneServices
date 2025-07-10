using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GLLocalNumberFormatRegistryItem : StronglyTypedRegistryItem<GLLocalNumberFormatCollection>
	{
		public GLLocalNumberFormatRegistryItem(string name, MultilingualString category, MultilingualString caption,
												  MultilingualString hint, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new GLLocalNumberFormatRegistryDataType(),
									 RegistryStorageFlags.System, options))
		{ }
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.GLLocalNumberFormatRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class GLLocalNumberFormatRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GLLocalNumberFormatCollection>
	{ }
}
