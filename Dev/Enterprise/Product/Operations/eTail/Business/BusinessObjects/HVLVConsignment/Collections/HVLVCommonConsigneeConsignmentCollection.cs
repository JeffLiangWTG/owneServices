using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVCommonConsigneeConsignmentCollection : BusinessObjectCollection<HVLVConsignment>, IHVLVConsignmentCollection
	{
		public HVLVCommonConsigneeConsignmentCollection(HVLVConsignment consignment)
			: base(consignment.Factory)
		{
			this.consignment = consignment;
		}

		readonly HVLVConsignment consignment;

		public HVLVConsignment ParentConsignment => consignment;
		IHVLVConsignment IHVLVConsignmentCollection.this[int i] => (IHVLVConsignment)Elements[i];
		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result;
			if (!consignment.HVC_HCH_Header.IsEmpty)
			{
				result = new ZQuery(HVLVConsignmentSchema.HVC_HCH_Header, consignment.HVC_HCH_Header);
			}
			else
			{
				result = new ZQuery(HVLVConsignmentSchema.HVC_HVH_BookingHeader, consignment.HVC_HVH_BookingHeader);
			}

			result.AddToFilter(HVLVConsignmentSchema.HVC_ClusterKey, consignment.HVC_ClusterKey);

			return result;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = new ZQuery();
			result.AddToFilter(HVLVConsignmentSchema.HVC_IsActive, true);
			result.AddToFilter(HVLVConsignmentSchema.HVC_ConsigneeName, consignment.HVC_ConsigneeName);
			result.AddToFilter(HVLVConsignmentSchema.HVC_ConsigneeAddress1, consignment.HVC_ConsigneeAddress1);
			result.AddToFilter(HVLVConsignmentSchema.HVC_ConsigneeAddress2, consignment.HVC_ConsigneeAddress2);
			result.AddToFilter(HVLVConsignmentSchema.HVC_ConsigneePostcode, consignment.HVC_ConsigneePostcode);
			result.AddToFilter(HVLVConsignmentSchema.HVC_ConsigneeCity, consignment.HVC_ConsigneeCity);
			result.AddToFilter(HVLVConsignmentSchema.HVC_ConsigneeState, consignment.HVC_ConsigneeState);
			result.AddToFilter(HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode, consignment.HVC_RN_NKConsigneeCountryCode);
			return result;
		}

		protected override bool ElementCanBeAdded(BusinessObject bizO)
		{
			var element = bizO as HVLVConsignment;
			return element.DirectionOfTrade == consignment.DirectionOfTrade;
		}
	}
}
