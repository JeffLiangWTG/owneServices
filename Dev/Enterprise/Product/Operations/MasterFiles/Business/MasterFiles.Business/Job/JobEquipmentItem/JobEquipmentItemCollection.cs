using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Will be used in future WI for Equipment Combination Module.")]
	public class JobEquipmentItemCollection : ActiveBusinessObjectCollection<JobEquipmentItem>
	{
		public JobEquipmentItemCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public JobEquipmentItemCollection(BusinessObject master) : base(master)
		{
		}
	}
}
