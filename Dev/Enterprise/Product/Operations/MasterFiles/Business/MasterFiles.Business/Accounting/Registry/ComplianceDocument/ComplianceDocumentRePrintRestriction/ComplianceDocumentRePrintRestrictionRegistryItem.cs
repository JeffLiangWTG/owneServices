using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ComplianceDocumentRePrintRestrictionRegistryItem : StronglyTypedRegistryItem<ComplianceDocumentRePrintRestrictionCollection>
	{
		public ComplianceDocumentRePrintRestrictionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new ComplianceDocumentRePrintRestrictionRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ComplianceDocumentRePrintRestrictionRegistryItemEditor, Enterprise.Accounting.GUI")]
	class ComplianceDocumentRePrintRestrictionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceDocumentRePrintRestrictionCollection>
	{
		public ComplianceDocumentRePrintRestrictionRegistryDataType()
		{
		}
	}
}
