using Enterprise.MasterFiles.Module;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.GUI
{
	public class ShowCustomerServiceTicketInEditOrViewForm : ShowSelectedInEditOrViewForm
	{
		public ShowCustomerServiceTicketInEditOrViewForm(IShowEditOrViewForm parent)
			: base(parent) { }

		protected override void ShowNewFormForEmptyCode(ZFilterModule module)
		{
			var ticket = (WorkRequest)parent.DataSource;
			var organization = ticket?.ClientOrganisation;

			if (organization != null)
			{
				var controller = (OrganisationController)ZControllerFactory.Instance.GetControllerForBizo(organization);
				controller.ShowForm(organization, OrganisationTabPages.Contacts, FormAction.Edit);
			}
			else
			{
				Globals.Message.Show(Res.GetString("e3a83978-0ffb-43e3-8d70-f1a50aa84ba4", "Please select an Organization first."));
			}
		}

		protected override void ShowNewFormForNonExistentCode(ZFilterModule module)
		{
			Globals.Message.Show(Res.GetString("99a3f321-0f55-4453-addc-36422cf86562", "No contact [{0}] exists for the specified Organization.", parent.SearchCode.Trim()));
		}
	}
}
