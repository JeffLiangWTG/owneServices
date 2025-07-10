using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReferenceFilterType : CodeDescriptionPairList
	{
		public ReferenceFilterType()
		{
			ICodeDescriptionPairListWithDefaultCode list = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value;
			AddPair(Any, Res.GetString("fa1093e2-bd57-4132-b6b7-2aa5d923c67a", "Any Reference"));
			AddPair(ContainerNo, Res.GetString("22aeb3d7-b0ab-4379-a856-d18c0ba92fef", "Container No."));
			AddPair(CustomerReference, Res.GetString("36b4d2d4-bc4d-42ff-b186-049224f847cf", "Customer Reference"));
			AddPair(TransportReference, Res.GetString("98ee9131-601b-4136-80ea-dc765e16b46c", "Transport Reference"));
			this.AddRange(list);
		}

		public const string Any = "ANY";
		public const string CustomerReference = "COR";
		public const string ContainerNo = "CNT";
		public const string ReceiveReference = "INW";
		public const string OrderNo = "ORD";
		public const string TransportReference = "TSR";
	}

	public static class StatusFilterTypes
	{
		public const string AnyFilterType = "ANY";
		public const string UnfinalisedFilterType = "UNF";
	}
}
