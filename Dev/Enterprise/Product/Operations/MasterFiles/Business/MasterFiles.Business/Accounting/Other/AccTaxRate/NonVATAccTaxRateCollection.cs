using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class NonVATAccTaxRateCollection : AccTaxRateCollection
	{
		public NonVATAccTaxRateCollection(BusinessObjectFactory factory, ZString countryCode, AccTaxConfiguration taxConfiguration = null)
			: base(factory, countryCode)
		{
			TaxConfiguration = taxConfiguration;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.AddToFilter(JoinCondition.And, AccTaxRateSchema.AT_TaxSystemCode, SQLComparisonOperator.NotEqual, "");
			return filter;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			if (TaxConfiguration != null)
			{
				query.AddToFilter(AccTaxRateSchema.AT_TaxSystemCode, TaxConfiguration.ETC_TaxSystemCode);
			}
			return query;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (TaxConfiguration != null && ((AccTaxRate)selectedBusinessObject).AT_TaxSystemCode != TaxConfiguration.ETC_TaxSystemCode)
			{
				errors.Add(Res.GetString("a75dc4e9-6680-427d-bf43-0d41b76d7f30", "Tax ID chosen here is not relevant to the Tax System '{0}' of the Tax Configuration '{1}'. Please select a different Tax ID.", TaxConfiguration.ETC_TaxSystemCode, TaxConfiguration.ETC_Code));
			}
		}

		AccTaxConfiguration TaxConfiguration { get; }
	}
}
