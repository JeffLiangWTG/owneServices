using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVConsignmentCollection : DependentBusinessObjectCollection<CusUSLVConsignment, CusUSLVClearance>
	{
		public CusUSLVConsignmentCollection(CusUSLVClearance cusUSLVClearance)
			: base(cusUSLVClearance)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusUSLVConsignmentSchema.ULB_ULH; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is CusUSLVConsignment consignment && Master != null && (Master.ULH_TransportMode == TransportTypeList.Codes.Road || Master.ULH_TransportMode == TransportTypeList.Codes.Mail))
			{
				consignment.ULB_NonAMSIndicator = true;
			}
		}
	}
}
