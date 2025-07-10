using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.GUI
{
	public partial class DocumentCommonConsolForm : DocumentOptionsForm
	{
		public DocumentCommonConsolForm(DocumentCommonConsol businessObject)
			: base(businessObject, businessObject.DataContext)
		{
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Controls

		protected override Control[] GetControls(Core.Constants.DataContext dataContext)
		{
			List<Control> arrangedControls = new List<Control>();
			DocumentConsol.SetDefaultsFromDataContext(dataContext);

			arrangedControls.Add(IncludeConsignorCheckBox);
			arrangedControls.Add(IncludeConsigneeCheckBox);
			arrangedControls.Add(IncludeCustomsBrokerCheckBox);
			arrangedControls.Add(AllShipmentsRadioButton);
			arrangedControls.Add(PackedRadioButton);
			arrangedControls.Add(UnpackedRadioButton);
			OptionsGroupBox.Text = Res.GetString("Freight|DocumentCommonConsolForm|OptionsGroupBox", "Include");
			Text = Res.GetString("Freight|DocumentCommonConsolForm|FormCaption", "Document Options");

			return arrangedControls.ToArray();
		}

		#endregion

		#region Business Object

		DocumentCommonConsol DocumentConsol
		{
			get { return (DocumentCommonConsol)BusinessEntity; }
		}

		#endregion

	}
}
