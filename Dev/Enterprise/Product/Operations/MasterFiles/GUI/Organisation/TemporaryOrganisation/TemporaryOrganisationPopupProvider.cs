using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public class TemporaryOrganisationPopupProvider
	{
		public TemporaryOrganisationPopupProvider(ITemporaryOrganisationFindBox findBox)
		{
			if (findBox == null)
			{
				throw new ArgumentNullException(nameof(findBox));
			}

			this.findBox = findBox;
		}

		readonly ITemporaryOrganisationFindBox findBox;

		#region Show

		TemporaryOrganisationsPopup orgPopup;
		public void ShowTemporaryOrgPopup(OrganisationEmdeddedModulePopup parentFindBoxPopup)
		{
			if (AllowNewTemporaryOrganisations)
			{
				if (NewTemporaryOrganisationsSecurityAllowed)
				{
					if (orgPopup == null || orgPopup.IsDisposed)
					{
						orgPopup = GetNewTempOrgPopup();
						orgPopup.ParentFindBoxPopUp = parentFindBoxPopup;
						orgPopup.Closed += new EventHandler(OrgPopup_Closed);

						Form parentForm = parentFindBoxPopup ?? findBox.ParentForm;
						orgPopup.ShowModal(findBox, parentForm);
					}
				}
				else
				{
					EnvProxy.Instance.Security.OrgDetailsNewIsTemporaryOrg.ShowError();
				}
			}
		}

		public OrganisationEmdeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
		{
			return AllowNewTemporaryOrganisations && NewTemporaryOrganisationsSecurityAllowed ? new OrganisationEmdeddedModulePopup(module) : null;
		}

		#endregion

		#region Popup Closed

		public event EventHandler PopupClosed;

		void OrgPopup_Closed(object sender, EventArgs e)
		{
			OnPopupClosed();

			TemporaryOrganisationsPopup orgPopup = sender as TemporaryOrganisationsPopup;
			if (orgPopup != null)
			{
				orgPopup.Closed -= new EventHandler(OrgPopup_Closed);
			}
			else
			{
				ErrorReporter.ReportOnce("TemporaryOrganisationPopupProvider.OrgPopup_Closed", "Sender was not able to be cast to a TemporaryOrganisationPopup.");
			}
		}

		void OnPopupClosed()
		{
			if (PopupClosed != null)
			{
				PopupClosed(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Implementation

		bool AllowNewTemporaryOrganisations
		{
			get
			{
				var list = List;
				return list != null && list.AllowNewTemporaryOrganisations;
			}
		}

		bool NewTemporaryOrganisationsSecurityAllowed
		{
			get { return EnvProxy.Instance.Security.OrgDetailsNewIsTemporaryOrg.IsAllowed; }
		}

		public delegate TemporaryOrganisationsPopup GetNewTempOrgPopupDelegate(IOrgHeaderCollection list, IOrgHeader businessEntity);
		public static readonly Overridable<GetNewTempOrgPopupDelegate> OverridableNewDelegate = new Overridable<GetNewTempOrgPopupDelegate>();

		TemporaryOrganisationsPopup GetNewTempOrgPopup()
		{
			IOrgHeaderCollection list = List;

			IOrganisationDefaultProvider orgFieldDefaultProvider = list as IOrganisationDefaultProvider;

			if (orgFieldDefaultProvider != null)
			{
				orgFieldDefaultProvider.ShouldSetValuesFromConditionalDefaults = (findBox.Code == OrgHeader.UnmatchedOrganisationCode);
			}

			var overridden = OverridableNewDelegate.Value;
			if (overridden == null)
			{
				return new TemporaryOrganisationsPopup(list, TemporaryOrganisationCreator.GetNewTemporaryOrganisation(new BusinessObjectFactory(), list));
			}
			else
			{
				return overridden(list, TemporaryOrganisationCreator.GetNewTemporaryOrganisation(new BusinessObjectFactory(), list));
			}
		}

		IOrgHeaderCollection List
		{
			get { return findBox.List; }
		}

		#endregion
	}
}
