using System;
using System.Xml;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgTaxConfigurationModuleFilter : ModuleTextFilter
	{
		public OrgTaxConfigurationModuleFilter(ZString description)
			: base(description, EmptyQuery, new CodeDescriptionPairList())
		{
		}

		#region Properties

		#region TaxConfiguration

		[ResourceStringData("9763a62c-c93b-4396-9461-39a6bc4912be", Caption = "Tax Configuration")]
		[List(nameof(TaxConfigurations))]
		public ZGuid TaxConfiguration
		{
			get => taxConfiguration;
			set
			{
				if (SetNonPersistentPropertyValue(TaxConfigurationInfo, ref taxConfiguration, value))
				{
					Validation.ValidateTaxConfiguration();
				}
			}
		}
		ZGuid taxConfiguration;

		public ZPropertyInfo TaxConfigurationInfo => GetZPropertyInfo(nameof(TaxConfiguration));

		public AccTaxConfigurationCollection TaxConfigurations => taxConfigurations ?? (taxConfigurations = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetCompanyTaxConfigurations(new ReadOnlyBusinessObjectFactory(), GlbCompany.CurrentCompany));
		AccTaxConfigurationCollection taxConfigurations;
		#endregion

		#region ConfigurationStatus

		[ResourceStringData("079f0372-65d8-4b62-9d04-433c076c070e", Caption = "Status")]
		[List(nameof(ConfigurationStatusOptions))]
		public ZString ConfigurationStatus
		{
			get => configurationStatus;
			set
			{
				if (SetNonPersistentPropertyValue(ConfigurationStatusInfo, ref configurationStatus, value))
				{
					Validation.ValidateConfigurationStatus();
					Validation.ValidateTaxConfiguration();
				}
			}
		}

		ZString configurationStatus;

		public ZPropertyInfo ConfigurationStatusInfo => GetZPropertyInfo(nameof(ConfigurationStatus));

		public CodeDescriptionPairList ConfigurationStatusOptions
		{
			get
			{
				if (configurationStatusOptions == null)
				{
					configurationStatusOptions = new CodeDescriptionPairList();
					configurationStatusOptions.AddPair(StatusActive, ResString.GetMultilingualString("390B9F8D-3611-4743-AA60-5D0E7577D186", "Show Active only"));
					configurationStatusOptions.AddPair(StatusInactive, ResString.GetMultilingualString("BC502BC9-8E62-41B1-A9D4-C2527287C29A", "Show Inactive only"));
					configurationStatusOptions.AddPair(StatusAll, ResString.GetMultilingualString("D64B60D0-A053-4B96-8193-0B2EC43941C1", "Show all configured records"));
					configurationStatusOptions.AddPair(StatusNotConfigured, ResString.GetMultilingualString("4A6BC877-EBC8-448F-AAC0-53E8938EC4A6", "Hide all configured records"));
				}

				return configurationStatusOptions;
			}
		}
		CodeDescriptionPairList configurationStatusOptions;

		internal static string StatusActive => Res.GetString("1CEE6E50-CEA7-43C0-88E5-9DC9E1DB20E1", "Active");
		internal static string StatusInactive => Res.GetString("9E45670B-9C91-4140-B5D1-EBE41BFE2E1A", "Inactive");
		internal static string StatusAll => Res.GetString("92440810-7457-44FA-AAA6-65BB528049A6", "All");
		internal static string StatusNotConfigured => Res.GetString("86970F02-18A3-4636-8836-BA8FEA9013E7", "Not Configured");

		#endregion

		#endregion

		#region Validation

		public new OrgTaxConfigurationModuleFilterValidation Validation
		{
			get { return (OrgTaxConfigurationModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgTaxConfigurationModuleFilterValidation(this);
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}

			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

			ZDBOnlySubQuery orgTaxConfigurationQuery;
			if (ConfigurationStatus == StatusNotConfigured)
			{
				orgTaxConfigurationQuery = new ZDBOnlySubQuery(typeof(AccOrgTaxConfiguration), AccOrgTaxConfigurationSchema.OTC_OB, notIn: true);
			}
			else
			{
				orgTaxConfigurationQuery = new ZDBOnlySubQuery(typeof(AccOrgTaxConfiguration), AccOrgTaxConfigurationSchema.OTC_OB);
			}

			orgTaxConfigurationQuery.AddToFilter(AccOrgTaxConfigurationSchema.OTC_ETC, TaxConfiguration);

			if (ConfigurationStatus == StatusActive)
			{
				orgTaxConfigurationQuery.AddToFilter(AccOrgTaxConfigurationSchema.OTC_IsActive, true);
			}
			else if (ConfigurationStatus == StatusInactive)
			{
				orgTaxConfigurationQuery.AddToFilter(AccOrgTaxConfigurationSchema.OTC_IsActive, false);
			}

			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK.ToGuid());
			orgCompanyDataQuery.AddSubQuery(orgTaxConfigurationQuery, JoinCondition.And);

			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);

			return orgHeaderQuery;
		}

		static ZQuery EmptyQuery(ZString value)
		{
			return new ZQuery();
		}

		#endregion

		#region XML serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString(nameof(TaxConfiguration), TaxConfiguration.ToString());
			writer.WriteElementString(nameof(ConfigurationStatus), ConfigurationStatus);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == nameof(TaxConfiguration))
			{
				TaxConfiguration = new Guid(reader.ReadElementString(nameof(TaxConfiguration)));
			}

			if (reader.Name == nameof(ConfigurationStatus))
			{
				ConfigurationStatus = reader.ReadElementString(nameof(ConfigurationStatus));
			}
		}

		#endregion

		protected override bool IsEmptyCore => base.IsEmptyCore && TaxConfiguration.IsEmpty && ConfigurationStatus.IsEmpty;

		protected override void ClearCore()
		{
			base.ClearCore();

			TaxConfiguration = ZGuid.Empty;
			ConfigurationStatus = ZString.Empty;
		}
	}
}
