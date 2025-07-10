using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.MasterFiles.GUI
{
	public class ExporterSchemePlugin : ZPlugIn
	{
		public ExporterSchemePlugin(OrgHeader parentOrganisation) : base(parentOrganisation)
		{
			header = parentOrganisation;
			TabPage.Text = Name;
		}

		readonly OrgHeader header;

		protected sealed override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override ZBool AllowPlugInDisplayWithNoLicence => true;

		protected sealed override LicenceCheckpoint LicenceCheckPoint
		{
			get
			{
				return Env.Licence.Core;
			}
		}

		protected virtual ZString ExporterSchemeLicenceCode
		{
			get
			{
				if (!SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode.IsEmpty)
				{
					return SupplyChainSecurityConfiguration.LicenceEconomicGroupingCode;
				}
				else if (SupplyChainSecurityConfiguration.IsLicensedModule && SupplyChainSecurityConfiguration.IsEnabled)
				{
					return GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}

				return ZString.Empty;
			}
		}

		public override string Name
		{
			get { return Res.GetString("a260c35a-91b9-4530-90b8-f6d4e72eed5d", "Supply Chain Security ({0})", ExporterSchemeLicenceCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : ExporterSchemeLicenceCode); }
		}

		protected sealed override string TextOverride
		{
			get { return Name; }
		}

		protected sealed override Control GetNewUserControl()
		{
			Control result = GetNewUserControlCore();
			result.Dock = DockStyle.Fill;
			return result;
		}

		protected virtual Control GetNewUserControlCore()
		{
			if (SupplyChainSecurityConfiguration.IsAddressLevelScheme)
			{
				return new AddressLevelExporterSchemeControl(SupplyChainSecurityConfiguration);
			}

			return new ExporterSchemeControl();
		}

		protected sealed override IBusiness GetBusinessEntityForPlugIn()
		{
			return header;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			return (!SupplyChainSecurityConfiguration.IsLicensedModule || SupplyChainSecurityConfiguration.IsOnlyForDevelopers || SupplyChainSecurityConfiguration.IsEnabled)
				&& base.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get
			{
				if (SupplyChainSecurityConfiguration.IsLicensedModule && !SupplyChainSecurityConfiguration.IsOnlyForDevelopers && !SupplyChainSecurityConfiguration.IsEnabled)
				{
					return Res.GetString("4b4609db-9d54-468c-ab9b-c3ae0ef4a5a0", "The company you are currently logged into has the {0} license disabled in the Registry. You can enable it by switching on the Registry item '{1}'.", Name, SupplyChainSecurityConfiguration.RegistryItemKey);
				}

				return base.PlugInNotDisplayedMessage;
			}
		}

		protected OrgHeader Header
		{
			get { return (OrgHeader)HostBusinessEntity; }
		}

		#region SupplyChainSecurityConfiguration

		ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration => supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = ObjectFactory.New<ISupplyChainSecurityConfigurationHelper>().GetConfiguration());
		ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#endregion
	}
}
