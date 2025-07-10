using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccInvMsgFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();
			collection.AddFiltersForTranslatableText("English Description", AccInvMsgSchema.A9_EnglishMsg, typeof(AccInvMsg), ResString.GetMultilingualString("Accounting|AccInvMsgFilter|EnglishDescription", "English Description"));
			collection.AddTextFilter("Local Language Description", AccInvMsgSchema.A9_LocalMsg).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccInvMsgFilter|LocalLanguageDescription", "Local Language Description");
			return collection;
		}
	}
}
