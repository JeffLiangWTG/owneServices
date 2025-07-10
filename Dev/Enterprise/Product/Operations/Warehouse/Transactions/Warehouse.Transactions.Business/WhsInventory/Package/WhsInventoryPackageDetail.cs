using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryPackageDetail : NonPersistentBusinessObject
	{
		#region Constructors

		internal WhsInventoryPackageDetail()
			: base()
		{
		}

		public WhsInventoryPackageDetail(WhsOrder order, string packageID, ZBool isTote)
			: base()
		{
			Order = order;
			PackageID = packageID;
			IsTote = isTote;
		}

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string PackageID = nameof(PackageID);
			public const string IsTote = nameof(IsTote);
			public const string OrderNumber = nameof(OrderNumber);
		}

		#endregion

		#region Properties

		#region PackageID

		[ResourceStringData("30ebc80e-fa19-4b43-b21a-7c72a12f6f61", Caption = "Package ID", ShortCaption = "ID")]
		public ZString PackageID { get; }

		public ZPropertyInfo PackageIDInfo => GetZPropertyInfo(Schema.PackageID);

		#endregion

		#region IsTote

		[ResourceStringData("28b4d067-bee4-42ec-823a-bf3c87016870", Caption = "Is Tote")]
		public ZBool IsTote { get; }

		public ZPropertyInfo IsToteInfo => GetZPropertyInfo(Schema.IsTote);

		#endregion

		#region Order Number

		public WhsOrder Order { get; }

		[ResourceStringData("1fb4ea79-959b-4537-a9d4-3f81759e5263", Caption = "Order Number")]
		public ZString OrderNumber => Order?.WD_ExternalReference ?? ZString.Empty;

		public ZPropertyInfo OrderNumberInfo => GetZPropertyInfo(Schema.OrderNumber);

		#endregion

		#endregion
	}
}
