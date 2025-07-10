using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.GUI
{
	public partial class DocumentShipmentWithUniqueIDForm : DocumentOptionsForm
	{
		public DocumentShipmentWithUniqueIDForm(DocumentShipment businessObject)
			: base(businessObject, businessObject.DataContext)
		{
		}

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Controls
		protected override Control[] GetControls(Core.Constants.DataContext dataContext)
		{
			List<Control> arrangedControls = new List<Control>();
			DocumentShipment.SetDefaultsFromDataContext();
			arrangedControls.Add(consignorRadioButton);
			arrangedControls.Add(consigneeRadioButton);
			arrangedControls.Add(noneRadioButton);
			arrangedControls.Add(labelsToPrintPanel);
			OptionsGroupBox.Text = Res.GetString("Freight|DocumentShipmentWithUniqueIDForm|OptionsGroupBox", "Include");
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
