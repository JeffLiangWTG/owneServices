//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSecondaryPartBOMPivotValidation
//
//    This class should be used for overriding validation in AutoOrgSecondaryPartBOMPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecondaryPartBOMPivotValidation : AutoOrgSecondaryPartBOMPivotValidation
	{
		public OrgSecondaryPartBOMPivotValidation(AutoOrgSecondaryPartBOMPivot parent)
			: base(parent)
		{
		}

		protected override void CheckOPP_OE_Component()
		{
			base.CheckOPP_OE_Component();

			if (!Parent.OPP_OE_ComponentInfo.HasErrors()
				&& Parent.OPP_OE_Component.IsValid
				&& (Parent.SecondaryPart?.ComponentUsages.Any(c => c.PK != Parent.PK && c.OPP_OE_Component == Parent.OPP_OE_Component) ?? false))
			{
				Parent.OPP_OE_ComponentInfo.AddError(CannotSelectSameBOMComponentTwice);
			}
		}

		protected override void CheckOPP_ComponentQuantity()
		{
			base.CheckOPP_ComponentQuantity();
			MandatoryValidation.CheckNotZero(Parent.OPP_ComponentQuantityInfo);
			MandatoryValidation.CheckNotNegative(Parent.OPP_ComponentQuantityInfo);

			if (!Parent.OPP_ComponentQuantityInfo.HasErrors())
			{
				var bomComponent = Parent.Component;
				if (bomComponent != null)
				{
					var componentProduct = bomComponent.Component;
					var componentStockQty = componentProduct.UnitConverter.Convert(bomComponent.OE_ComponentQty, bomComponent.OE_F3_NKPackType, componentProduct.OP_StockKeepingUnit);

					var matchingComponentQuery = new ZQuery(OrgSecondaryPartBOMPivotSchema.OPP_OE_Component, bomComponent.PK);
					matchingComponentQuery.AddToFilter(OrgSecondaryPartBOMPivotSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					var allComponentUsages = Parent.Factory.Load<OrgSecondaryPartBOMPivot>(matchingComponentQuery);

					var componentUsageTotal = (ZDecimal)(allComponentUsages.Sum(c => c.OPP_ComponentQuantity) + Parent.OPP_ComponentQuantity);
					if (componentUsageTotal > componentStockQty)
					{
						Parent.OPP_ComponentQuantityInfo.AddError(TotalComponentQtyCannotExceedTotalComponentStockQty(componentUsageTotal, componentStockQty));
					}
				}
			}
		}

		#region Error messages

		public static string CannotSelectSameBOMComponentTwice => Res.GetString("87b4b6c0-7b4b-4c93-b895-f28e54ca48ce", "Cannot Select the same BOM Component twice.");

		public static string TotalComponentQtyCannotExceedTotalComponentStockQty(ZDecimal componentUsageTotal, ZDecimal componentStockQty)
			=> Res.GetString(
				"97927602-e949-4879-b165-198b9ed6781d",
				"Cannot have Total Component Quantity used ({0}) greater than the Total Component Stock Quantity ({1}) on the BOM Component.",
				componentUsageTotal.ToStringTrimZeros(),
				componentStockQty.ToStringTrimZeros());

		#endregion
	}
}
