using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoOrgDebtorGroup.Schema.OJ_Desc)]
	public class OrgDebtorGroup : AutoOrgDebtorGroup
	{
		public OrgDebtorGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoOrgDebtorGroup.Schema
		{
			public const string DefaultBankAccountPK = "DefaultBankAccountPK";
			public const string OverrideRegistryCurrencyToBankSetting = "OverrideRegistryCurrencyToBankSetting";
		}

		#endregion

		#region Default Bank Account
		[List("Lookups.PayToBankAccounts")]
		public ZGuid DefaultBankAccountPK
		{
			get
			{
				if (BankDefaults.Count > 0 && BankDefaults[0].BankAccount != null && !IsDefaultLoaded && defaultBankAccountPK.IsEmpty && DefaultBankAccount.PK.IsValid)
				{
					defaultBankAccountPK = DefaultBankAccount.PK;
					IsDefaultLoaded = true;
				}
				return defaultBankAccountPK;
			}
			set
			{
				if (value != defaultBankAccountPK)
				{
					defaultBankAccountPK = value;
					DebtorGroupBankDefault.P6_AB = value;
					DefaultBankAccountPKInfo.RefreshBinding();
				}
			}
		}

		bool IsDefaultLoaded;
		ZGuid defaultBankAccountPK;

		public ZPropertyInfo DefaultBankAccountPKInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultBankAccountPK); }
		}

		public AccBankAccount DefaultBankAccount
		{
			get
			{
				AccBankAccount result = null;
				if (BankDefaults.Count > 0 && DebtorGroupBankDefault != null)
				{
					result = DebtorGroupBankDefault.BankAccount;
				}
				return result;
			}
		}

		public AccBankAccount GetBankAccountByCurrency(ZString currencyNK)
		{
			AccBankAccount result = null;
			if (BankDefaults.Count > 0 && DebtorGroupBankDefault != null && DebtorGroupBankDefault.P6_OverrideRegistryCurrencyToBankSetting)
			{
				BusinessObject[] collection = OrgDebtorGroupBankCurrentOverrideCollection.Find(new ZQuery(OrgDebtorGroupBankCurrentOverrideSchema.PB_RX_NKCurrency, currencyNK));

				if (collection != null && collection.Length > 0)
				{
					BusinessObject bankAccountByCurrency = collection[0];
					result = (AccBankAccount)bankAccountByCurrency["BankAccount"];
				}
			}
			return result;
		}

		internal OrgDebtorGroupBankDefaultCollection BankDefaults
		{
			get
			{
				if (bankDefaults == null)
				{
					bankDefaults = new OrgDebtorGroupBankDefaultCollection(this, GlbCompany.CurrentCompany);
					RegisterEditableChildObject(bankDefaults);
					bankDefaults.Load();
				}
				return bankDefaults;
			}
		}

		OrgDebtorGroupBankDefaultCollection bankDefaults;

		internal OrgDebtorGroupBankDefault DebtorGroupBankDefault
		{
			get
			{
				if (BankDefaults.Count == 0)
				{
					BankDefaults.AddNew();
				}
				return BankDefaults[0];
			}
		}

		void FillOrgDebtorGroupBankCurrentOverrideCollectionFromRegistry()
		{
			BankAccountBasedOnCurrencyCollection collection = OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.Value;
			for (int number = 0; number < collection.Count; number++)
			{
				OrgDebtorGroupBankCurrentOverrideCollection.AddNew();
				OrgDebtorGroupBankCurrentOverrideCollection[number].PB_RX_NKCurrency = collection[number].Currency;
				OrgDebtorGroupBankCurrentOverrideCollection[number].PB_AB = collection[number].BankAccount;
				OrgDebtorGroupBankCurrentOverrideCollection[number].CurrencyDescription = collection[number].CurrencyDescription;
			}
			if (OrgDebtorGroupBankCurrentOverrideCollection.Count > 1 && OrgDebtorGroupBankCurrentOverrideCollection.SortInformation != null)
			{
				OrgDebtorGroupBankCurrentOverrideCollection.Sort(OrgDebtorGroupBankCurrentOverrideCollection.SortInformation);
			}
		}

		[ChildEditable()]
		[ActionFieldFollow(true)]
		public OrgDebtorGroupBankCurrentOverrideCollection OrgDebtorGroupBankCurrentOverrideCollection
		{
			get
			{
				if (fOrgDebtorGroupBankCurrentOverrideCollection == null)
				{
					fOrgDebtorGroupBankCurrentOverrideCollection = new OrgDebtorGroupBankCurrentOverrideCollection(DebtorGroupBankDefault);
					if (OverrideRegistryCurrencyToBankSetting)
					{
						fOrgDebtorGroupBankCurrentOverrideCollection.Load(new ZQuery(OrgDebtorGroupBankCurrentOverrideSchema.PB_P6, DebtorGroupBankDefault.PK));
					}
					else
					{
						BankAccountBasedOnCurrencyCollection collection = OrganisationsDataRegistry.Instance.BankAccountsBasedOnCurrency.Value;
						for (int number = 0; number < collection.Count; number++)
						{
							fOrgDebtorGroupBankCurrentOverrideCollection.AddNew();
							fOrgDebtorGroupBankCurrentOverrideCollection[number].PB_RX_NKCurrency = collection[number].Currency;
							fOrgDebtorGroupBankCurrentOverrideCollection[number].PB_AB = collection[number].BankAccount;
							fOrgDebtorGroupBankCurrentOverrideCollection[number].CurrencyDescription = collection[number].CurrencyDescription;
						}
						fOrgDebtorGroupBankCurrentOverrideCollection.SetReadOnlyIncludingChildren(true);
					}
					RegisterEditableChildObject(fOrgDebtorGroupBankCurrentOverrideCollection);
				}
				return fOrgDebtorGroupBankCurrentOverrideCollection;
			}
		}

		OrgDebtorGroupBankCurrentOverrideCollection fOrgDebtorGroupBankCurrentOverrideCollection;

		public ZBool OverrideRegistryCurrencyToBankSetting
		{
			get
			{
				return DebtorGroupBankDefault.P6_OverrideRegistryCurrencyToBankSetting;
			}
			set
			{
				OrgDebtorGroupBankCurrentOverrideCollection.RemoveAndDeleteAll();
				FillOrgDebtorGroupBankCurrentOverrideCollectionFromRegistry();
				OrgDebtorGroupBankCurrentOverrideCollection.SetReadOnlyIncludingChildren(!value);
				OrgDebtorGroupBankCurrentOverrideCollection.RefreshBindingIncludingChildren();
				DebtorGroupBankDefault.P6_OverrideRegistryCurrencyToBankSetting = value;
				Validation.ValidateDefaultBankAccountPK();
				OverrideRegistryCurrencyToBankSettingInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OverrideRegistryCurrencyToBankSettingInfo
		{
			get { return GetZPropertyInfo(Schema.OverrideRegistryCurrencyToBankSetting); }
		}

		#endregion

		#region AccountFeeSettings
		public AccountFeeSettings AccountFeeSettings
		{
			get
			{
				if (fAccountFeeSettings == null)
				{
					fAccountFeeSettings = new AccountFeeSettings(new ZGuid(Env.CurrentCompany.PK), this);
					fAccountFeeSettings.PopulateFields();
				}
				RegisterEditableChildObject(fAccountFeeSettings);
				return fAccountFeeSettings;
			}
		}
		AccountFeeSettings fAccountFeeSettings;
		#endregion

		[ChildEditable(true)]
		public AccExchangeRateConfigurationCollection AccExchangeRateConfigurations
		{
			get
			{
				if (accExchangeRateConfigurations == null)
				{
					var localAccExchangeRateConfigurations = new AccExchangeRateConfigurationCollection(Factory, Env.CurrentCompanyPK, LedgerTypes.AccountsReceivable, PK);
					localAccExchangeRateConfigurations.Load();
					accExchangeRateConfigurations = localAccExchangeRateConfigurations;
					RegisterEditableChildObject(accExchangeRateConfigurations);
				}
				return accExchangeRateConfigurations;
			}
		}
		AccExchangeRateConfigurationCollection accExchangeRateConfigurations;

		#region Delete

		public override void Delete()
		{
			OrgDebtorGroupBankDefaultCollection allBankDefaults = new OrgDebtorGroupBankDefaultCollection(this, null);
			allBankDefaults.Load();
			allBankDefaults.RemoveAndDeleteAll();
			var exRateConfigQuery = AccExchangeRateConfigurationsQueryProviderFactory.CreateOrganizationGroupLevelProvider(Env.CurrentCompanyPK, LedgerTypes.AccountsReceivable, PK, new ZQuery()).GetQuery();
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
				result.AddRange(AccExchangeRateConfigurations.Where(x => x.Level == AccExRateConfigurationLevelEnum.DebtorGroup));
				return result.ToArray();
			}
		}

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (!OverrideRegistryCurrencyToBankSetting)
			{
				if (DebtorGroupBankDefault.P6_AB == Guid.Empty)
				{
					DebtorGroupBankDefault.Delete();
				}
				OrgDebtorGroupBankCurrentOverrideCollection.RemoveAndDeleteAll();
			}
			else
			{
				foreach (OrgDebtorGroupBankCurrentOverride bankOverride in OrgDebtorGroupBankCurrentOverrideCollection)
				{
					bankOverride.PB_P6 = DebtorGroupBankDefault.PK;
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (!OverrideRegistryCurrencyToBankSetting)
			{
				FillOrgDebtorGroupBankCurrentOverrideCollectionFromRegistry();
			}
		}
	}
}
