using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.GUI
{
	public partial class DocumentShipmentForm : DocumentOptionsForm
	{
		public DocumentShipmentForm(DocumentShipment businessObject)
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
			DocumentShipment.SetDefaultsFromDataContext();
			arrangedControls.Add(ConsignorRadioButton);
			arrangedControls.Add(ConsigneeRadioButton);
			arrangedControls.Add(NoneRadioButton);
			arrangedControls.Add(NumberOfLabelsToPrintPanel);
			OptionsGroupBox.Text = Res.GetString("Freight|DocumentShipmentForm|OptionsGroupBox", "Include");
			Text = Res.GetString("Freight|DocumentShipmentForm|FormCaption", "Document Options");

			return arrangedControls.ToArray();
		}

		#endregion

		#region Business Object

		DocumentShipment DocumentShipment
		{
			get { return (DocumentShipment)BusinessEntity; }
		}

		#endregion
	}
}
