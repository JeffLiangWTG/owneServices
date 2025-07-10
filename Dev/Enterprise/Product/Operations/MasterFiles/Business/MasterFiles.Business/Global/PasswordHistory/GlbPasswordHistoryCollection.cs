using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPasswordHistoryCollection : DependentBusinessObjectCollection<GlbPasswordHistory, BusinessObject>
	{
		public GlbPasswordHistoryCollection(IGlbPasswordHistoryParent parent) : base(parent.BusinessEntity)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => GlbPasswordHistorySchema.PWH_ParentID;

		public override void Load(ZQuery filter)
		{
			base.Load(filter);
			Sort(GlbPasswordHistorySchema.PWH_SystemCreateTimeUtc.Name);
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			Sort(GlbPasswordHistorySchema.PWH_SystemCreateTimeUtc.Name);
		}
	}
}
