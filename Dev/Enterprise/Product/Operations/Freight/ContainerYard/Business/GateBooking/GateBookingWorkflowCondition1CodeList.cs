using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateBookingWorkflowCondition1CodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Cargo = "CAR";
			public const string FullContainer = "FCN";
			public const string EmptyContainer = "ECN";
		}

		public static class Descriptions
		{
			public static MultilingualString Cargo
				=> ResString.GetMultilingualString("GateBookingWorkflowCondition1CodeList|Cargo", "Cargo");

			public static MultilingualString FullContainer
				=> ResString.GetMultilingualString("GateBookingWorkflowCondition1CodeList|FullContainer", "Full Container");

			public static MultilingualString EmptyContainer
				=> ResString.GetMultilingualString("GateBookingWorkflowCondition1CodeList|EmptyContainer", "Empty Container");
		}

		public GateBookingWorkflowCondition1CodeList()
		{
			AddPair(Codes.Cargo, Descriptions.Cargo);
			AddPair(Codes.FullContainer, Descriptions.FullContainer);
			AddPair(Codes.EmptyContainer, Descriptions.EmptyContainer);
		}
	}
}
