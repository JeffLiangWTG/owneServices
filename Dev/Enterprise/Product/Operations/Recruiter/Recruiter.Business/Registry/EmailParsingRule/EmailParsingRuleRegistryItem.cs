using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	public sealed class EmailParsingRuleRegistryItem : StronglyTypedRegistryItem<EmailParsingRuleCollection>
	{
		public EmailParsingRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, EmailParsingRuleCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new EmailParsingRuleRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Recruiter.GUI.EmailParsingRuleRegistryItemEditor, Enterprise.Recruiter.GUI")]
	public class EmailParsingRuleRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EmailParsingRuleCollection>
	{
	}
}

