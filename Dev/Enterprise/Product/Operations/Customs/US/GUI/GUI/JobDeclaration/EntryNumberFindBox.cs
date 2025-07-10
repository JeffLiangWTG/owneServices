using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class EntryNumberFindBox : ZCodeFindBox
	{
		internal EmbeddedModulePopup CreateEmbeddedPopupInternal(ZFilterModule module) => CreateEmbeddedPopup(module);
		protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
		{
			if (module is ZFilterGridModule filterGridModule)
			{
				filterGridModule.AllowLoadTemplateRecords = AllowTemplateRecords;
			}

			return new CustomsEmbeddedModulePopup(module);
		}
	}
}
