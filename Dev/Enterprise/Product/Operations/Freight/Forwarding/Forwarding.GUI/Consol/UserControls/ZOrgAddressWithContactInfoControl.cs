using System;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ZOrgAddressWithContactInfoControl : ZOrgAddressControl
	{
		public ZOrgAddressWithContactInfoControl()
		{
			InitializeComponent();
			InitializeExtensions();
			InitializeEventHandlers();

			ContactInfoTab.TabVisible = false;
			AddressesLink.Visible = false;
			ContactsLink.Visible = false;

			AddressValidationUIHelper.SetButtonValidationStatus(addressValidationStatusButton, AddressValidationStatus.ToBeVerified, true);
			addressValidationStatusButton.Invalidate();
			addressValidationStatusButton.Refresh();

			DetailsTabControl.AllowOverlap(addressValidationStatusButton);
		}

		ZButton addressValidationStatusButton;

		void InitializeExtensions()
		{
			if (AddressDropEdit.GetExtension<INotificationExtension>() == null)
			{
				AddressDropEdit.Extensions.Add(new NotificationExtension());
			}
		}

		void InitializeEventHandlers()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				OrganisationFindBox.Validated += delegate
				{ Extensions.Get<IValidationExtension>().Validate(); };
				AddressDropEdit.SelectedIndexChanged += Validation_Action;
				AddressDropEdit.BoundValueCommitted += Validation_Action;
			}
		}

		void AddressValidationStatusButton_Click(object sender, EventArgs e)
		{
			if (ZAddress != null && ZAddress.OrgHeader != null && ZAddress.OrgAddress is IOrgAddress
				&& ZControllerFactory.Create(ControllerIDs.Organisation) is IOrganisationController controller
				&& controller.ShowForm(ZAddress.OrgHeader, OrganisationTabPages.Address, FormAction.Edit) is ZOrganisationsForm form)
			{
				form.Closed += Validation_Action;
			}
		}

		void Validation_Action(object sender, EventArgs e)
		{
			RefreshValidationStatus();
		}

		void RefreshValidationStatus()
		{
			if (!this.ReadOnly)
			{
				var isErrorSuppressed = (ZAddress as ISupportWebAddressValidation)?.IsErrorSuppressed ?? false;
				AddressValidationUIHelper.SetButtonValidationStatus(
					addressValidationStatusButton,
					CurrentAddressValidationStatus,
					isErrorSuppressed);
			}
		}

		string CurrentAddressValidationStatus => (ZAddress?.OrgAddress as IOrgAddress)?.ValidationStatus ?? string.Empty;
	}
}
