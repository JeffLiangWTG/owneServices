using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	public class MNRWorkOrderApprovalParty : AutoMNRWorkOrderApprovalParty
	{
		public MNRWorkOrderApprovalParty(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("WorkOrderHeader")]
		public override ZGuid MNA_MWO_WorkOrder { get => base.MNA_MWO_WorkOrder; set => base.MNA_MWO_WorkOrder = value; }

		public MNRWorkOrderHeader WorkOrderHeader
		{
			get => Factory.Load<MNRWorkOrderHeader>(MNA_MWO_WorkOrder);
		}

		#endregion

		#region Implementation

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			MNA_ApprovalResult = "APP";
			MNA_ApprovalParty = "OWN";
			MNA_ApprovalRequestedTime = ZDateTimeOffset.Now;
			MNA_SystemCreateTimeUtc = ZDateTime.Now;
			MNA_SystemCreateUser = "USR";
			MNA_SystemLastEditTimeUtc = ZDateTime.Now;
			MNA_SystemLastEditUser = "USR";
		}
#endif

		#endregion
	}
}
