using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class CodeFindBoxWithMultiSelect : ZCodeFindBox
	{
		public CodeFindBoxWithMultiSelect() : base()
		{
			AllowModuleMultiSelect = true;
		}

		protected override ZArchitecture.GUI.Internal.EmbeddedModulePopup CreateEmbeddedPopup(ZArchitecture.Modules.ZFilterModule module)
		{
			return new ModulePopupWithMultiSelect(module);
		}
	}
}
