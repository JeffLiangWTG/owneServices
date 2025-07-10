using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefTransitTimeDetailCollection : DependentBusinessObjectCollection<RefTransitTimeDetail, RefTransitTime>
	{
		public RefTransitTimeDetailCollection(RefTransitTime master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return RefTransitTimeDetailSchema.RTD_RTT_Parent; }
		}
	}
}
