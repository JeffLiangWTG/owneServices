using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsProductParamsByWhsAndClientValidationHelper : IWhsProductParamsByWhsAndClientValidationHelper
	{
		public ZString CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted(ZShort consigneeMinShelfLifeAccepted, ZShort maximumShelfLife)
		{
			var errorMessage = ZString.Empty;
			if (maximumShelfLife > 0 && consigneeMinShelfLifeAccepted > maximumShelfLife)
			{
				errorMessage = Res.GetString("3a08a349-0c78-4766-9933-925fe5d6adca", "Maximum shelf life {0} cannot be less than Minimum Shelf Life {1}.", maximumShelfLife, consigneeMinShelfLifeAccepted);
			}

			return errorMessage;
		}

		public ZString CheckMaximumShelfLifeIsValidWhenHasStock(IWhsProductParamsByWhsAndClient partParams, ZShort newMaximumShelfLife)
		{
			var productParams = Argument.NotNull(partParams, "IWhsProductParamsByWhsAndClient") as WhsProductParamsByWhsAndClient;

			var errorMessage = ZString.Empty;
			if (productParams != null && productParams.Product != null)
			{
				if (productParams.Product.IsAJulianBatchNumberAttributeUsedAndHasAnyStock(productParams.Client, productParams.Warehouse) && newMaximumShelfLife == 0)
				{
					errorMessage = Res.GetString("26a23d3e-c807-4737-86dc-227eff5b9dca", "Maximum Shelf Life cannot be 0 when there are stock for this product, client and warehouse currently in use.");
				}
				else if ((ZShort)productParams.W3_MaximumShelfLifeInfo.OriginalValue != 0 && !productParams.W3_MaximumShelfLifeInfo.OriginalValue.Equals(newMaximumShelfLife))
				{
					errorMessage = CheckCannotBeModifiedIfJulianBatchNumberAttributeUsedAndHasStockCore(productParams, productParams.W3_MaximumShelfLifeInfo, newMaximumShelfLife);
				}
			}

			return errorMessage;
		}

		#region CheckCannotBeModifiedIfJulianBatchNumberAttributeUsedAndHasStock

		public void CheckCannotBeModifiedIfJulianBatchNumberAttributeUsedAndHasStock(WhsProductParamsByWhsAndClient productParams, ZPropertyInfo info)
		{
			if (!info.HasErrors())
			{
				var errorMessage = CheckCannotBeModifiedIfJulianBatchNumberAttributeUsedAndHasStockCore(productParams, info, info.Value);
				if (!errorMessage.IsEmpty)
				{
					info.AddError(errorMessage);
				}
			}
		}

		ZString CheckCannotBeModifiedIfJulianBatchNumberAttributeUsedAndHasStockCore(WhsProductParamsByWhsAndClient productParams, ZPropertyInfo info, IZType newValue)
		{
			var errorMessage = ZString.Empty;
			var product = productParams.Product;
			if (!info.OriginalValue.Equals(newValue))
			{
				var originalClient = productParams.Factory.Load<OrgHeader>((ZGuid)productParams.W3_OHInfo.OriginalValue);
				var originalWhs = productParams.Factory.Load<WhsWarehouse>((ZGuid)productParams.W3_WWInfo.OriginalValue);

				if (product.IsAJulianBatchNumberAttributeUsedAndHasAnyStock(originalClient, originalWhs))
				{
					errorMessage = PartAttributeValidation.GetThereIsCurrentInventoryUsingJulianBatchNumbersErrorMessage(info.HumanReadableName);
				}
			}

			return errorMessage;
		}

		#endregion
	}
}
