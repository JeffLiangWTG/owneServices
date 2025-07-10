using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class EUTaxIDDefaultingRuleCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new EUTaxIDDefaultingRule this[int x]
		{
			get { return (EUTaxIDDefaultingRule)base[x]; }
		}

		public new EUTaxIDDefaultingRule AddNew()
		{
			return (EUTaxIDDefaultingRule)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EUTaxIDDefaultingRuleCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EUTaxIDDefaultingRule();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public AccTaxRate GetRate(CostSell costOrSell, string jobDirectionCode, string taxRegistrationCode)
		{
			ZGuid taxRatePK = ZGuid.Empty;

			foreach (EUTaxIDDefaultingRule rule in this)
			{
				if (rule.JobDirection == jobDirectionCode)
				{
					switch (taxRegistrationCode)
					{
						case EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry:
							taxRatePK = costOrSell == CostSell.Cost ? rule.CostTaxRateForOrganisationRegisteredInMyCountry : rule.SellTaxRateForOrganisationRegisteredInMyCountry;
							break;
						case EUTaxIDDefaultingRule.OriginDestinationCode.OtherEUCountry:
							taxRatePK = costOrSell == CostSell.Cost ? rule.CostTaxRateForOrganisationRegisteredInOtherEUCountry : rule.SellTaxRateForOrganisationRegisteredInOtherEUCountry;
							break;
						case EUTaxIDDefaultingRule.OriginDestinationCode.NotEUCountry:
							taxRatePK = costOrSell == CostSell.Cost ? rule.CostTaxRateForNotRegisteredOrganisation : rule.SellTaxRateForNotRegisteredOrganisation;
							break;
					}
				}
			}

			return CurrentFactory.Load<AccTaxRate>(taxRatePK);
		}
	}
}
