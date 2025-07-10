using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class DeliveryOrderBillCollection : ActiveBusinessObjectCollection<DeliveryOrderBill>
	{
		public DeliveryOrderBillCollection(DeliveryOrderHeader bizObj)
			: base(bizObj.Factory, bizObj, new ZQuery(CusCodeDataSchema.CY_Type, DeliveryOrderHeader.Constants.CusCodeDataBill), CusCodeDataSchema.CY_ParentID)
		{
		}

		public DeliveryOrderHeader Master
		{
			get { return (DeliveryOrderHeader)Relationship.Master; }
		}

		public ActiveBusinessObjectCollection<DeliveryOrderBill> MasterBills
		{
			get
			{
				if (masterBills == null)
				{
					ZQuery query = new ZQuery(CusCodeDataSchema.CY_ParentID, Master.PK);
					query.AddToFilter(CusCodeDataSchema.CY_Type, DeliveryOrderHeader.Constants.CusCodeDataBill);
					query.AddToFilter(CusCodeDataSchema.CY_Code, Customs.Business.BillTypeList.Codes.MasterBill);
					query.FetchOnlyFromLocalCache = !Master.IsInDatabase;
					masterBills = new ActiveBusinessObjectCollection<DeliveryOrderBill>(Factory, query);
				}
				return masterBills;
			}
		}
		ActiveBusinessObjectCollection<DeliveryOrderBill> masterBills;

		public ActiveBusinessObjectCollection<DeliveryOrderBill> HouseBills
		{
			get
			{
				if (houseBills == null)
				{
					ZQuery query = new ZQuery(CusCodeDataSchema.CY_ParentID, Master.PK);
					query.AddToFilter(CusCodeDataSchema.CY_Type, DeliveryOrderHeader.Constants.CusCodeDataBill);
					query.AddToFilter(CusCodeDataSchema.CY_Code, Customs.Business.BillTypeList.Codes.HouseBill);
					query.FetchOnlyFromLocalCache = !Master.IsInDatabase;
					houseBills = new ActiveBusinessObjectCollection<DeliveryOrderBill>(Factory, query);
				}
				return houseBills;
			}
		}
		ActiveBusinessObjectCollection<DeliveryOrderBill> houseBills;

		public ZBool ShowDeliveryOrderBillsInReport => (Count != 0) &&
			!(Count == 1 && (MasterBills.Count == 1 || HouseBills.Count == 1)) &&
			!(Count == 2 && MasterBills.Count == 1 && HouseBills.Count == 1);
	}
}
