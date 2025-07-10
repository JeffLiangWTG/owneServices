using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Will be used in future WI for Equipment Combination Module.")]
	public class JobEquipmentCollection : ActiveBusinessObjectCollection<JobEquipment>
	{
		public JobEquipmentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public JobEquipmentCollection(BusinessObject master) : base(master)
		{
		}
	}
}
