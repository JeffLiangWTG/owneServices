using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WhsPickAvailableInventorySplitByUOM : WhsPickAvailableInventorySplitBase
	{
		public WhsPickAvailableInventorySplitByUOM(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public new abstract class Schema : WhsPickAvailableInventorySplitBase.Schema
		{
			public const string OrderReference = "OrderReference";
			public const string PackQuantity = "PackQuantity";
			public const string PackQuantityUQ = "PackQuantityUQ";
			public const string UOMType = "UOMType";
		}

		#endregion

		#region Properties

		// read only

		#region OrderReference

		[ResourceStringData("WhsPickAvailableInventorySplitByUOM|OrderReference", Caption = "Order")]
		public ZString OrderReference { get; private set; }

		public ZPropertyInfo OrderReferenceInfo => GetZPropertyInfo(Schema.OrderReference);

		#endregion

		#region PackQuantity

		[ResourceStringData("WhsPickAvailableInventorySplitByUOM|PackQuantity", Caption = "Pack Qty")]
		public ZDecimal PackQuantity { get; private set; }

		public ZPropertyInfo PackQuantityInfo => GetZPropertyInfo(Schema.PackQuantity);

		#endregion

		#region PackQuantityUQ

		[ResourceStringData("WhsPickAvailableInventorySplitByUOM|PackQuantityUQ", Caption = "Pack UQ")]
		public ZString PackQuantityUQ { get; private set; }

		public ZPropertyInfo PackQuantityUQInfo => GetZPropertyInfo(Schema.PackQuantityUQ);

		#endregion

		#region UOMType

		[ResourceStringData("WhsPickAvailableInventorySplitByUOM|UOMType", Caption = "UOM Type")]
		public ZString UOMType { get; private set; }

		public ZPropertyInfo UOMTypeInfo => GetZPropertyInfo(Schema.UOMType);

		#endregion

		#endregion

		//

		#region SetData

		public void SetData(ZString orderReference, ZDecimal packQty, ZString packUQ, ZString uomType, IPickLinePair[] pickLinePairs, WhsPickAvailableInventory whsPickAvailableInventory)
		{
			SetData(pickLinePairs, whsPickAvailableInventory);

			OrderReference = orderReference;
			PackQuantity = packQty;
			PackQuantityUQ = packUQ;
			UOMType = uomType;
		}

		#endregion

		#region Lookups

		protected override WhsPickAvailableInventorySplitBaseLookups GetNewLookups()
		{
			return new WhsPickAvailableInventorySplitByUOMLookups(this);
		}

		#endregion

		#region Validation

		public override WhsPickAvailableInventorySplitBaseValidation GetNewValidation()
		{
			return new WhsPickAvailableInventorySplitByUOMValidation(this);
		}

		#endregion
	}
}
