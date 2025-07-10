using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI
{
	public class CustomsEmbeddedModulePopup : FilterCollectionEmbeddedModulePopup
	{
		public CustomsEmbeddedModulePopup(ZFilterModule module)
			: base(module)
		{
		}

		protected override void OnOkButtonClicked()
		{
			var selectedBizObjs = GetSelectedBusinessObjects();

			if (selectedBizObjs != null && selectedBizObjs.Length > 0)
			{
				if (EmbeddedModulePopupOKButtonStrategy != null)
				{
					EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(selectedBizObjs);
				}
			}
			else if (IsSelectionMandatory)
			{
				Globals.Message.ShowError("Please select an item from the grid.");
			}
			else
			{
				Close();
			}
		}

		protected override void HandleSelection(BusinessObject[] selectedBizObjs)
		{
			OnSelected(selectedBizObjs);
			Close();
		}
	}
}
