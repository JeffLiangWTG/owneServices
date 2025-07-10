using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaxConfigurationTemplateGUIHelper
	{
		public TaxConfigurationTemplateGUIHelper(OrgCompanyData companyData, bool isReceivable)
		{
			Argument.NotNull(companyData, nameof(companyData));

			CompanyData = companyData;
			IsReceivable = isReceivable;
		}

		#region Property

		readonly bool IsReceivable;
		readonly OrgCompanyData CompanyData;

		ZPropertyInfo TemplatePropertyInfo => IsReceivable ? CompanyData.OB_OCT_ARTaxTemplateInfo : CompanyData.OB_OCT_APTaxTemplateInfo;

		AccOrgTaxConfigurationTemplate Template => IsReceivable ? CompanyData.ARTaxTemplate : CompanyData.APTaxTemplate;

		AccOrgTaxConfigurationCollectionByLedger TargetOrgTaxConfigList => IsReceivable ?
			CompanyData.AROrgTaxConfigurations :
			CompanyData.APOrgTaxConfigurations;

		#endregion

		public void RedefaultOrgTaxConfigurationFormTemplate()
		{
			((IBusinessObjectInternals)TemplatePropertyInfo.BizObj).Validate(TemplatePropertyInfo);
			if (TemplatePropertyInfo.HasErrors())
			{
				Globals.Message.ShowWarning(TemplatePropertyInfo.Notifications.ToUniqueMessageListString());
			}
			else if (Template == null)
			{
				Globals.Message.ShowWarning(Res.GetString("A973294C-C813-40A3-BFDB-5D191A5673A9", @"Re-defaulting the Tax Configuration values on this organization is not possible because the Tax Configuration Template field is empty.
Please link a Tax Configuration Template to this organization before attempting to re-default"));
			}
			else
			{
				var caption = Res.GetString("52A47AE3-379D-42AD-B4B0-88BF6AF7475D", "Re-default Organization Tax Configuration Form Template");
				var message = Res.GetString("556B681B-7F5E-4AF2-BD75-868FD18E12F8", @"The Tax Configurations populated in the grid below will be updated using the values defined against the Template you have chosen.
Do you want to proceed? 
· Select Yes to re-default Tax Configuration settings on this organization.
· Select No to cancel this action.");

				if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					var helper = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccOrgTaxConfigurationTemplateDataHelper();
					helper.UpdateTaxConfigurationsForOrgnization(TargetOrgTaxConfigList, Template.AccOrgTaxConfigurations);
				}
			}
		}

		public void OrgTaxConfigurationTemplateChanged()
		{
			if (!((ZGuid)TemplatePropertyInfo.OriginalValue).IsEmpty && ((ZGuid)TemplatePropertyInfo.Value).IsEmpty)
			{
				var caption = Res.GetString("449092D4-72ED-43ED-A388-C6550867F795", "Tax Configuration Template was removed");

				var message = Res.GetString("CCF3C6FC-9718-4FDD-9B89-FB27B6074457", @"You have removed the Tax Configuration Template recorded against this organization and chosen to leave this field empty. Removing the Template from this organization does NOT make any changes to the Tax Configurations currently populated against this organization. 
Do you want to proceed?
· Select Yes to proceed and save without a related Template. This change would mean that Tax Configurations on this organization will now need to be maintained manually.
· Select No to cancel this action.");

				if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					TemplatePropertyInfo.Value = TemplatePropertyInfo.OriginalValue;
				}
			}
		}
	}
}
