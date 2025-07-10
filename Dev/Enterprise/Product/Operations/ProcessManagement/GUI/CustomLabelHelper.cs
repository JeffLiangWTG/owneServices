using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.ProcessManagement.GUI
{
	internal static class CustomLabelHelper
	{
		public static void Setup(Control control, MultilingualStringRegistryItem registryItem)
		{
			var caption = registryItem.Value;
			control.GetExtension<LabelCaptionRenderer>().Caption = caption;

			var hint = control.GetExtension<HintExtension>();
			hint.Caption = caption;
			const string NiceDelimiter = " > ";
			hint.Description = Res.GetString("F0536E90-8568-49A6-8723-CAD10752281E", "This label can be customized in the Registry, under {0}",
				registryItem.Category.Replace(RegistryItemSet.Delimiter, NiceDelimiter) + NiceDelimiter + registryItem.Caption);
		}
	}
}
