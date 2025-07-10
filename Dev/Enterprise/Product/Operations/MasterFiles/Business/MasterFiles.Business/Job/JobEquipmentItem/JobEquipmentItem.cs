using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobEquipmentItem : AutoJobEquipmentItem
	{
		public JobEquipmentItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public JobEquipment ParentJobEquipment
		{
			get { return Factory.Load<JobEquipment>(JEI_JEQ_JobEquipment); }
		}

		[RelatedBusinessObject(nameof(ParentJobEquipment))]
		public override ZGuid JEI_JEQ_JobEquipment
		{
			get { return base.JEI_JEQ_JobEquipment; }
			set { base.JEI_JEQ_JobEquipment = value; }
		}
	}
}
