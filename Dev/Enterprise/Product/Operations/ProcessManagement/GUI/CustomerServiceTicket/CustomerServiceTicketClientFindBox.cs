using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	class CustomerServiceTicketClientFindBox : ZGuidFindBox
	{
		protected override void ShowEditOrViewForm()
		{
			var showForm = new ShowCustomerServiceTicketInEditOrViewForm(this);
			showForm.ShowEditOrViewForm();
		}
	}
}
