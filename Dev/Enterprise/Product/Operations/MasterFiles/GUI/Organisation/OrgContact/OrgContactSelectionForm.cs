using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgContactSelectionForm : ZChildForm
	{
		public OrgContact SelectedOrgContact { get; set; }

		public override string FormVerb => "";

		public OrgContactSelectionForm(FilteredContactsCollectionWrapper contacts) : base(contacts)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				ContactPanel.BackColor = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.TabBackgroundColor;
			}
		}

		void ContactGrid_DoubleClick(object sender, EventArgs e)
		{
			SelectedOrgContact = ContactGrid.SelectedElements.OfType<OrgContact>().FirstOrDefault();

			if (SelectedOrgContact != null)
			{
				Close();
			}
		}

		void ContactGrid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			OrgContact contact = (OrgContact)e.ObjectAtRow;
			var theme = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme;
			e.Colour = contact.OC_IsActive ? theme.GridBackgroundColor : theme.GridReadOnlyColor;
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			SelectedOrgContact = ContactGrid.SelectedElements.OfType<OrgContact>().FirstOrDefault();

			if (SelectedOrgContact == null)
			{
				string caption = Res.GetString("C38FD97E-EE12-4F99-9A6F-4AFA1C7D1141", "Warning");
				string message = Res.GetString("09DFF61D-8947-427A-9081-5C5C2B6482D5", "Please select a Contact.");
				Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			Close();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
