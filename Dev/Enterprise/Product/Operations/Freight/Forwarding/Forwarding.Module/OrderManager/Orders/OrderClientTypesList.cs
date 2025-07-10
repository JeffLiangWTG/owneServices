using Enterprise.ZArchitecture.Core;
using ClientTypesList = Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter.ClientTypesList;
using ResString = Enterprise.Freight.Forwarding.Module.ResString;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrderClientTypesList : ClientTypesList
	{
		public new class Codes : ClientTypesList.Codes
		{
			public const string Buyer = "BUY";
			public const string Supplier = "SUP";
		}

		public new class Descriptions : ClientTypesList.Descriptions
		{
			public static MultilingualString Buyer { get { return ResString.GetMultilingualString("OrderClientTypesList|Buyer", "Buyer"); } }
			public static MultilingualString Supplier { get { return ResString.GetMultilingualString("OrderClientTypesList|Supplier", "Supplier"); } }
		}

		protected override void AddClientTypes()
		{
			AddPair(Codes.Buyer, Descriptions.Buyer);
			AddPair(Codes.Supplier, Descriptions.Supplier);
			AddPair(Codes.ControllingCustomer, Descriptions.ControllingCustomer);
		}
	}
}
