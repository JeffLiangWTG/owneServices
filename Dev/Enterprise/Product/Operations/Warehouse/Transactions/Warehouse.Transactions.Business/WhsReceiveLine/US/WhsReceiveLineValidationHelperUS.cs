using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in WhsReceiveLineValidationUS
	public class WhsReceiveLineValidationHelperUS : WhsDocketLineValidationHelperUS<WhsReceiveLine>
	{
		#region Constructor

		public WhsReceiveLineValidationHelperUS(WhsReceiveLine docketLine)
			: base(docketLine)
		{
		}

		#endregion

		#region Messages

		protected override string UnitsNotDivisibleByPerPackageQtyErrorMessage => Res.GetString("e3352cbf-f847-430a-88e0-e4b8c9dfd8a3", "Units received must be divisible by Per Group Quantity.");

		#endregion

		#region SplitQuantity

		public static void CheckSplitQuantity(ZPropertyInfo propertyInfo, ZDecimal docketLineUnits, ZDecimal perPackageQty, ZString packageGroupID)
		{
			var splitQuantity = (ZDecimal)propertyInfo.Value;
			CheckSplitQuantityIsDivisibleByPerPackageQty(propertyInfo, splitQuantity, perPackageQty);
			CheckPackageGroupIsFullySplit(propertyInfo, splitQuantity, perPackageQty, docketLineUnits, packageGroupID);
		}

		static void CheckSplitQuantityIsDivisibleByPerPackageQty(ZPropertyInfo propertyInfo, ZDecimal splitQuantity, ZDecimal perPackageQty)
		{
			if (!propertyInfo.HasErrors()
				&& perPackageQty > 0m
				&& splitQuantity % perPackageQty != 0m)
			{
				propertyInfo.AddError(Res.GetString("53331e88-dc60-451f-8154-c33cac72d307", "Split Quantity must be divisible by Per Group Quantity."));
			}
		}

		static void CheckPackageGroupIsFullySplit(ZPropertyInfo propertyInfo, ZDecimal splitQuantity, ZDecimal perPackageQty, ZDecimal units, ZString packageGroupID)
		{
			if (!propertyInfo.HasErrors()
				&& !packageGroupID.IsEmpty
				&& perPackageQty > 0m
				&& splitQuantity > 0m
				&& splitQuantity != units)
			{
				propertyInfo.AddError(Res.GetString("a05df521-a5b4-429b-a2bf-23ab9fb7e43c", "You must Split either all or none of the inventory when in a Package Group."));
			}
		}

		#endregion

		protected override IValidateParentWithLines ProductPackageTotalsParent => LineAttributes.Docket;

		#region GetHelper

		protected override WhsValidationHelperUS<WhsReceiveLine> GetHelper(WhsReceiveLine lineAttributes)
		{
			return new WhsReceiveLineValidationHelperUS(lineAttributes);
		}

		#endregion
	}
}
