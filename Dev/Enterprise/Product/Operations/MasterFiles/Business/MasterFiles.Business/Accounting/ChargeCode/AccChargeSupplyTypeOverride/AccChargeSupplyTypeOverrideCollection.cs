using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeSupplyTypeOverrideCollection : DependentBusinessObjectCollection<AccChargeSupplyTypeOverride, BusinessObject>, IRegistrySettingCollection
	{
		public AccChargeSupplyTypeOverrideCollection(AccChargeCode chargeCode)
			: base(chargeCode)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(AccChargeSupplyTypeOverrideSchema.ACS_ParentID, Master.PK);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return AccChargeSupplyTypeOverrideSchema.ACS_ParentID; }
		}
	}
}
