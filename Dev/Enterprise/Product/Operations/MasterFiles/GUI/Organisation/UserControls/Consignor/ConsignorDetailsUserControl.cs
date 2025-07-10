using System;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsignorDetailsUserControl : OrganisationContainerControl
	{
		public ConsignorDetailsUserControl()
		{
			InitializeComponent();
			ServiceLevelsGrid.ReadOnly = !OverrideServiceLevelsSettingCheckBox.Checked;
			SetControlVisibility();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			if (Parent == null)
			{
				return;
			}

			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				var country = Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				if (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(country))
				{
					DefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.EU.OrganisationConsignorPlugIn);
				}
				else if (country == Constants.CountryCodes.India)
				{
					DefaultsTabControl.PlugIns.Add(ControllerIDs.Customs.IN.OrganisationConsignorPlugIn);
				}
			}
		}

		void SetControlVisibility()
		{
			if (DesignModeFinder.IsDesigning)
			{
				return;
			}

			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Constants.CountryCodes.Australia:
					MergeCustomsInvoiceLinesByDropEdit.Visible = true;
					OM_IMPaymentMethodDropEdit.Visible = false;
					break;
				case Constants.CountryCodes.NewZealand:
				case Constants.CountryCodes.Singapore:
					MergeCustomsInvoiceLinesByDropEdit.Visible = false;
					OM_IMPaymentMethodDropEdit.Visible = true;
					break;
				default:
					MergeCustomsInvoiceLinesByDropEdit.Visible = false;
					OM_IMPaymentMethodDropEdit.Visible = false;
					break;
			}
		}

		void OverrideServiceLevelsSettingCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			ServiceLevelsGrid.ReadOnly = !OverrideServiceLevelsSettingCheckBox.Checked;
		}

		void cfxUpliftEditLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (!(ParentForm is ZOrganisationsForm form) || form.OrganisationsTabControl == null)
			{
				return;
			}

			form.OrganisationsTabControl.SelectedTab = form.ReceivablesTabPage;

			if (form.ReceivablesControl?.ARTabControl == null)
			{
				return;
			}

			form.ReceivablesControl.ARTabControl.SelectedIndex = 0;
		}
	}
}
