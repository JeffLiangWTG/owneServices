using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ComplianceDocumentSupportingReasonRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<ComplianceDocumentSupportingReasonCollection, ComplianceDocumentSupportingReasonCollection>
	{
		public ComplianceDocumentSupportingReasonRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new RegistryItemImpl(name, category, caption, hint, new ComplianceDocumentSupportingReasonRegistryDataType(), storage, option))
		{
		}

		public override int MaxLength
		{
			get { return 80; }
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ComplianceDocumentSupportingRegistryItemEditor, Enterprise.Accounting.GUI")]
	class ComplianceDocumentSupportingReasonRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceDocumentSupportingReasonCollection>
	{
		public ComplianceDocumentSupportingReasonRegistryDataType()
		{
		}
	}
}
