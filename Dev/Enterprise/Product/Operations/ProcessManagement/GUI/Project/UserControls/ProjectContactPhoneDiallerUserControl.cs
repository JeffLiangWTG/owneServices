using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class ProjectContactPhoneDiallerUserControl : ContactPhoneDiallerUserControl
	{
		public ProjectContactPhoneDiallerUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			InitializeProject();
		}

		#region Project

		Project project;

		void InitializeProject()
		{
			var form = FindForm() as ZForm;
			if (form != null)
			{
				project = form.BusinessEntity as Project;
				if (project != null)
				{
					project.WKP_OA_ClientAddressInfo.ValueChanged += Project_BranchAddressInfo_ValueChanged;
				}
			}
		}

		void Project_BranchAddressInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshControls();
		}

		#endregion

		#region PhoneDialInfo

		protected override ContactPhoneDialInfoBuilder GetNewDialInfoBuilder()
		{
			return new ProjectPhoneDialInfoBuilder(project);
		}

		class ProjectPhoneDialInfoBuilder : ContactPhoneDialInfoBuilder
		{
			public ProjectPhoneDialInfoBuilder(Project project)
			{
				this.project = project;
			}

			readonly Project project;

			protected override PhoneDialInfo GetOfficePhoneDialInfo(OrgContact contact, OrgHeader org)
			{
				return GetContactBranchPhoneDialInfo(contact) ?? GetClientAddressPhoneDialInfo() ?? GetContactOrgPhoneDialInfo(org);
			}

			PhoneDialInfo GetContactBranchPhoneDialInfo(OrgContact contact)
			{
				if (contact != null)
				{
					var contactBranch = contact.BranchAddress;
					if (contactBranch != null)
					{
						var contactBranchPhone = contactBranch.OA_Phone;
						if (!contactBranchPhone.IsEmpty)
						{
							return new PhoneDialInfo(contactBranchPhone.ToString(), OfficeDescription);
						}
					}
				}

				return null;
			}

			PhoneDialInfo GetClientAddressPhoneDialInfo()
			{
				if (project != null && !project.IsDeleted)
				{
					var clientAddress = project.ClientAddress;
					if (clientAddress != null)
					{
						var clientAddressPhone = clientAddress.OA_Phone;
						if (!clientAddressPhone.IsEmpty)
						{
							return new PhoneDialInfo(clientAddressPhone.ToString(), OfficeDescription);
						}
					}
				}

				return null;
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (project != null)
			{
				project.WKP_OA_ClientAddressInfo.ValueChanged -= Project_BranchAddressInfo_ValueChanged;
			}

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
	}
}
