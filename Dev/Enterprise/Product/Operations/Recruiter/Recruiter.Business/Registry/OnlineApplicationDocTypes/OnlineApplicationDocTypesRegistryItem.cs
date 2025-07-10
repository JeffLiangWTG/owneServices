using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[RegistryEditor("Enterprise.Recruiter.GUI.OnlineApplicationDocTypesRegistryEditor, Enterprise.Recruiter.GUI")]
	public class OnlineApplicationDocTypesDataType : NonPersistentBusinessObjectRegistryDataType<OnlineApplicationDocTypeCollection>
	{
	}

	public class OnlineApplicationDocTypesRegistryItem : StronglyTypedRegistryItem<OnlineApplicationDocTypeCollection>
	{
		public OnlineApplicationDocTypesRegistryItem(string name, MultilingualString category)
			: base(new RegistryItemImpl(
					name,
					category,
					ResString.GetMultilingualString("cfcdcb6b-c464-4bb5-b1a0-80b16a0bd927", "Online Application Document Types"),
					ResString.GetMultilingualString("2a74ddaa-25bb-4472-b0d3-73649bd647cd", "Please specify the document types that are allowed to be uploaded onto the Careers website by the candidates."),
					new OnlineApplicationDocTypesDataType(),
					RegistryStorageFlags.System))
		{
		}
	}
}
