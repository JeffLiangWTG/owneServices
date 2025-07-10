using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EUTaxIDDefaultingRegistryItem : StronglyTypedRegistryItem<EUTaxIDDefaultingRuleCollection>
	{
		public EUTaxIDDefaultingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new EUTaxIDDefaultingRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		public class EUTaxIDDefaultingRegistryItemImpl : RegistryItemImpl
		{
			public EUTaxIDDefaultingRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new EUTaxIDDefaultingRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				bool use20100101Rules = ZDateTime.Today >= new ZDateTime(2010, 1, 1);
				var factory = new BusinessObjectFactory();
				ZGuid gst = AccTaxRate.Helper.FindTaxRatePK(factory, AccTaxRate.Helper.MainGSTTaxRegistryID, companyPK);
				ZGuid gstRev = AccTaxRate.Helper.FindTaxRatePK(factory, AccTaxRate.Helper.MainGSTReverseTaxRegistryID, companyPK);
				ZGuid notReport = AccTaxRate.Helper.FindTaxRatePK(factory, AccTaxRate.Helper.MainNotReportableTaxRegistryID, companyPK);
				ZGuid freeGst = AccTaxRate.Helper.FindTaxRatePK(factory, AccTaxRate.Helper.MainFreeGSTTaxRegistryID, companyPK);
				ZGuid freeGstRev = AccTaxRate.Helper.FindTaxRatePK(factory, AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, companyPK);

				EUTaxIDDefaultingRuleCollection collection = new EUTaxIDDefaultingRuleCollection();

				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.SameAsOrigin,
					CostTaxRateForOrganisationRegisteredInMyCountry = gst,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = gstRev,
					CostTaxRateForNotRegisteredOrganisation = gstRev,
					SellTaxRateForOrganisationRegisteredInMyCountry = gst,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = use20100101Rules ? gstRev : gst,
					SellTaxRateForNotRegisteredOrganisation = gst
				});
				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.SameAsOrigin,
					CostTaxRateForOrganisationRegisteredInMyCountry = use20100101Rules ? gst : notReport,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = use20100101Rules ? gstRev : notReport,
					CostTaxRateForNotRegisteredOrganisation = use20100101Rules ? gstRev : notReport,
					SellTaxRateForOrganisationRegisteredInMyCountry = use20100101Rules ? gst : notReport,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = use20100101Rules ? gstRev : notReport,
					SellTaxRateForNotRegisteredOrganisation = use20100101Rules ? gst : notReport
				});
				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry,
					CostTaxRateForOrganisationRegisteredInMyCountry = gst,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = gstRev,
					CostTaxRateForNotRegisteredOrganisation = gstRev,
					SellTaxRateForOrganisationRegisteredInMyCountry = gst,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = use20100101Rules ? gstRev : notReport,
					SellTaxRateForNotRegisteredOrganisation = use20100101Rules ? gst : notReport
				});
				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry,
					CostTaxRateForOrganisationRegisteredInMyCountry = freeGst,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = use20100101Rules ? freeGst : freeGstRev,
					CostTaxRateForNotRegisteredOrganisation = use20100101Rules ? freeGst : freeGstRev,
					SellTaxRateForOrganisationRegisteredInMyCountry = freeGst,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = freeGst,
					SellTaxRateForNotRegisteredOrganisation = freeGst
				});
				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry,
					CostTaxRateForOrganisationRegisteredInMyCountry = gst,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = gstRev,
					CostTaxRateForNotRegisteredOrganisation = gstRev,
					SellTaxRateForOrganisationRegisteredInMyCountry = gst,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = use20100101Rules ? gstRev : notReport,
					SellTaxRateForNotRegisteredOrganisation = use20100101Rules ? gst : notReport
				});
				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry,
					CostTaxRateForOrganisationRegisteredInMyCountry = freeGst,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = use20100101Rules ? freeGst : freeGstRev,
					CostTaxRateForNotRegisteredOrganisation = use20100101Rules ? freeGst : freeGstRev,
					SellTaxRateForOrganisationRegisteredInMyCountry = freeGst,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = freeGst,
					SellTaxRateForNotRegisteredOrganisation = freeGst
				});
				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry,
					CostTaxRateForOrganisationRegisteredInMyCountry = gst,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = gstRev,
					CostTaxRateForNotRegisteredOrganisation = gstRev,
					SellTaxRateForOrganisationRegisteredInMyCountry = gst,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = use20100101Rules ? gstRev : notReport,
					SellTaxRateForNotRegisteredOrganisation = gst
				});
				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry,
					CostTaxRateForOrganisationRegisteredInMyCountry = notReport,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = notReport,
					CostTaxRateForNotRegisteredOrganisation = notReport,
					SellTaxRateForOrganisationRegisteredInMyCountry = notReport,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = notReport,
					SellTaxRateForNotRegisteredOrganisation = notReport
				});
				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry,
					CostTaxRateForOrganisationRegisteredInMyCountry = notReport,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = notReport,
					CostTaxRateForNotRegisteredOrganisation = notReport,
					SellTaxRateForOrganisationRegisteredInMyCountry = notReport,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = notReport,
					SellTaxRateForNotRegisteredOrganisation = notReport
				});
				collection.Add(new EUTaxIDDefaultingRule
				{
					Origin = EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry,
					Destination = EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry,
					CostTaxRateForOrganisationRegisteredInMyCountry = notReport,
					CostTaxRateForOrganisationRegisteredInOtherEUCountry = notReport,
					CostTaxRateForNotRegisteredOrganisation = notReport,
					SellTaxRateForOrganisationRegisteredInMyCountry = notReport,
					SellTaxRateForOrganisationRegisteredInOtherEUCountry = notReport,
					SellTaxRateForNotRegisteredOrganisation = notReport
				});

				return collection;
			}
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.EUTaxIDDefaultingRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class EUTaxIDDefaultingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EUTaxIDDefaultingRuleCollection>
	{
	}
}
