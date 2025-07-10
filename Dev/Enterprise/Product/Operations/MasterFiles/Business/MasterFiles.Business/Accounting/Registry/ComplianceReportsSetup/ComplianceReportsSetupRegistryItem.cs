using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ComplianceReportsSetupRegistryItem : StronglyTypedRegistryItem<ComplianceReportTypeCollection>
	{
		public ComplianceReportsSetupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ComplianceReportTypeCollection defaultValue, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new ComplianceReportsSetupCategoryDataType(), RegistryStorageFlags.System, options, defaultValue))
		{ }
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.ComplianceReportsSetupCategoryRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class ComplianceReportsSetupCategoryDataType : NonPersistentBusinessObjectRegistryDataType<ComplianceReportTypeCollection>
	{
	}
}
