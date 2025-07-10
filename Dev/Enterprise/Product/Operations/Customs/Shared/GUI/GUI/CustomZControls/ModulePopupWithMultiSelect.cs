using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public class ModulePopupWithMultiSelect : EmbeddedModulePopup
	{
		public ModulePopupWithMultiSelect(ZFilterModule module)
			: base(module)
		{
		}

		protected override void HandleSelection(BusinessObject[] selectedBizObjs)
		{
			var result = new ZStringBuilder();
			foreach (BusinessObject selectedBizo in selectedBizObjs)
			{
				var supporter = selectedBizo as ICodeDescription;
				if (supporter != null)
				{
					result.Append(supporter.Code);
				}
			}
			this.FindBox.Code = result.ToStringWithDelimiterBetweenAppends(",");
			base.HandleSelection(selectedBizObjs);
		}
	}
}
