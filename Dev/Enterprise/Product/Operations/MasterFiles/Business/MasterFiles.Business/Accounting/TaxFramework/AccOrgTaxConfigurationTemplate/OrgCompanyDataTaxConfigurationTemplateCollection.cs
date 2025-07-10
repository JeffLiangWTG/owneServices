using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCompanyDataTaxConfigurationTemplateCollection : AccOrgTaxConfigurationTemplateCollection
	{
		public OrgCompanyDataTaxConfigurationTemplateCollection(BusinessObjectFactory factory, bool isReceivable)
			: base(factory)
		{
			IsReceivable = isReceivable;
			AdditionalFilter = new ZQuery(AccOrgTaxConfigurationTemplateSchema.OCT_IsActive, true);
		}

		public readonly bool IsReceivable;

		protected override void SetDefaultsForNewElementCore(AccOrgTaxConfigurationTemplate newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.OCT_IsReceivable = IsReceivable;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter().AddToFilter(AccOrgTaxConfigurationTemplateSchema.OCT_IsReceivable, IsReceivable);
		}
	}
}
