using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.ImporterSecurityFiling
{
	public class TrackingCusISFBillCollection : ActiveBusinessObjectCollection<CusISFBill>
	{
		public TrackingCusISFBillCollection(CusISFHeader header)
			: base(header.Factory, header, GetCollectionSpecificFilter(header.Factory), CusISFBillSchema.BB_BF)
		{
		}

		static ZQuery GetCollectionSpecificFilter(BusinessObjectFactory factory)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(JoinCondition.Or, CusISFBillSchema.BB_BillType, BillTypeList.Codes.HouseBillOfLading);
			result.AddToFilter(JoinCondition.Or, CusISFBillSchema.BB_BillType, BillTypeList.Codes.MasterBillOfLading);
			result.AddToFilter(JoinCondition.Or, CusISFBillSchema.BB_BillType, BillTypeList.Codes.OceanBillOfLading);
			result.AddToFilter(JoinCondition.Or, CusISFBillSchema.BB_BillType, ZString.Empty);
			return result;
		}

		public CusISFHeader Master
		{
			get { return (CusISFHeader)Relationship.Master; }
		}

		protected override void OnAdded(CusISFBill businessObject)
		{
			base.OnAdded(businessObject);
			if (!IsNonCommittedElement(businessObject) && !Master.ReferenceDatas.Contains(businessObject))
			{
				Master.ReferenceDatas.Relationship.AddToRelationship(businessObject);
			}
		}
	}
}
