using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoOrgCreditorGroup.Schema.OG_Desc)]
	[SystemDefinedValues]
	public class OrgCreditorGroup : AutoOrgCreditorGroup
	{
		public OrgCreditorGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoOrgCreditorGroup.Schema
		{
			public const string OG_IsAllowALM = "OG_IsAllowALM";
			public const string OG_IsAllowDNM = "OG_IsAllowDNM";
		}

		#endregion

		#region OG_IsAllowDNM

		public virtual ZBool OG_IsAllowDNM
		{
			get { return true; }
		}

		public virtual ZBool OG_IsAllowDNM_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region OG_IsAllowALM

		public virtual ZBool OG_IsAllowALM
		{
			get
			{
				return this.GetSystemDefinedValue<ZBool>(Schema.OG_IsAllowALM);
			}
			set
			{
				// GenAddOnColumn table automatically remove bool type rows when value is false.
				this.SetSystemDefinedValue(Schema.OG_IsAllowALM, AddOnColumnDataType.Codes.Boolean, value);
				OG_IsAllowALMInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateOG_DefaultHoldOption();
				}
			}
		}

		[ChildEditable(true)]
		public AccExchangeRateConfigurationCollection AccExchangeRateConfigurations
		{
			get
			{
				if (accExchangeRateConfigurations == null)
				{
					var localAccExchangeRateConfigurations = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsPayable, PK);
					localAccExchangeRateConfigurations.Load();
					accExchangeRateConfigurations = localAccExchangeRateConfigurations;
					RegisterEditableChildObject(accExchangeRateConfigurations);
				}
				return accExchangeRateConfigurations;
			}
		}
		AccExchangeRateConfigurationCollection accExchangeRateConfigurations;

		public ZPropertyInfo OG_IsAllowALMInfo
		{
			get { return GetZPropertyInfo(Schema.OG_IsAllowALM); }
		}

		#endregion

		#region OG_DefaultHoldOption

		[List("Lookups.HoldOptions")]
		public override ZString OG_DefaultHoldOption
		{
			get { return base.OG_DefaultHoldOption; }
			set { base.OG_DefaultHoldOption = value; }
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			var exRateConfigQuery = AccExchangeRateConfigurationsQueryProviderFactory.CreateOrganizationGroupLevelProvider(Env.CurrentCompanyPK, LedgerTypes.AccountsPayable, PK, new ZQuery()).GetQuery();
			AccExchangeRateConfigurationsHelper.LoadAndDelete(Factory, exRateConfigQuery);
			base.Delete();
		}

		#endregion

		#region BusinessObjectsWithRelatedEventsCore

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(AccExchangeRateConfigurations.Where(x => x.Level == AccExRateConfigurationLevelEnum.CreditorGroup));
				return result.ToArray();
			}
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("CC0DDCE0-EAB5-4E85-9EBC-3DBF0B29D142", "Creditor Group - {0}", CalculateShortcutName());
	}
}
