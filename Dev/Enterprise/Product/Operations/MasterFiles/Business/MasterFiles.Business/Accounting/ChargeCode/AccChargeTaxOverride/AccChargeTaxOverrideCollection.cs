using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeTaxOverrideCollection : DependentBusinessObjectCollection<AccChargeTaxOverride, BusinessObject>
	{
		public AccChargeTaxOverrideCollection(AccChargeCode chargeCode)
			: base(chargeCode)
		{
		}

		public AccChargeTaxOverrideCollection(AccTaxOverrideGroup taxOverrideGroup)
			: base(taxOverrideGroup)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((AccChargeTaxOverride)child).AO_TaxRegCntryOrGroup = AccChargeTaxOverride.ALL;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(AccChargeTaxOverrideSchema.AO_ParentID, Master.PK);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return AccChargeTaxOverrideSchema.AO_ParentID; }
		}
	}
}
