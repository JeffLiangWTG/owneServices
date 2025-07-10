using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class ProductPackageTotals
	{
		public ProductPackageTotals(IEnumerable<ICalculateProductPackageTotals> lines)
		{
			Lines = Argument.NotNull(lines, nameof(lines));
			IsCalculated = new Lazy<bool>(() =>
			{
				CalculateTotalPackagesForEachProduct();
				return true;
			});
		}

		IEnumerable<ICalculateProductPackageTotals> Lines { get; }
		Lazy<bool> IsCalculated { get; }

		public IEnumerable<Dictionary<ProductWithAttributes, long>> GetProductPackTotals(string packageGroupId)
		{
			_ = IsCalculated.Value;
			return PackTotals[packageGroupId.ToUpper()].Values;
		}

		public IEnumerable<ProductWithAttributes> GetProducts(string packageId)
		{
			_ = IsCalculated.Value;
			return ProductsInPackage[packageId];
		}

		Dictionary<string, Dictionary<ZGuid, Dictionary<ProductWithAttributes, long>>> PackTotals => packTotals ?? (packTotals = new Dictionary<string, Dictionary<ZGuid, Dictionary<ProductWithAttributes, long>>>());
		Dictionary<string, Dictionary<ZGuid, Dictionary<ProductWithAttributes, long>>> packTotals;

		Dictionary<string, HashSet<ProductWithAttributes>> ProductsInPackage => productsInPackage ?? (productsInPackage = new Dictionary<string, HashSet<ProductWithAttributes>>());
		Dictionary<string, HashSet<ProductWithAttributes>> productsInPackage;

		void CalculateTotalPackagesForEachProduct()
		{
			foreach (var line in Lines)
			{
				AddPackageQtyForPackageGroup(line);
			}
		}

		void AddPackageQtyForPackageGroup(ICalculateProductPackageTotals line)
		{
			if (line.PerPackageQty > 0m && line.ProductPK.IsValid)
			{
				var totalPacksByProduct = GetMatchingPackageGroup(line);
				AddProductToPackageGroup(line, totalPacksByProduct);

				var productAndAttributesKey = GetProductWithAttributes(line);
				AddProductsInPackage(line.PackageGroupID, productAndAttributesKey);
			}
		}

		Dictionary<ProductWithAttributes, long> GetMatchingPackageGroup(ICalculateProductPackageTotals line)
		{
			var packageGroupID = line.PackageGroupID.ToUpper();
			if (!PackTotals.TryGetValue(packageGroupID, out var totalPacksByPackageGroup))
			{
				PackTotals[packageGroupID] = totalPacksByPackageGroup = new Dictionary<ZGuid, Dictionary<ProductWithAttributes, long>>();
			}

			if (!totalPacksByPackageGroup.TryGetValue(line.LocationPK, out var totalPacksByProduct))
			{
				totalPacksByPackageGroup[line.LocationPK] = totalPacksByProduct = new Dictionary<ProductWithAttributes, long>();
			}

			return totalPacksByProduct;
		}

		void AddProductToPackageGroup(ICalculateProductPackageTotals line, Dictionary<ProductWithAttributes, long> totalPacksByProduct)
		{
			if (line.PerPackageQty > 0m)
			{
				var productWithAttributes = GetProductWithAttributes(line);
				totalPacksByProduct.TryGetValue(productWithAttributes, out var result);
				// there is validation to make sure that the units are always divisible by the per package qty, so this division should always be an integer value
				totalPacksByProduct[productWithAttributes] = result + (long)(line.Units / line.PerPackageQty);
			}
		}

		void AddProductsInPackage(string packageGroupId, ProductWithAttributes productAndAttributesKey)
		{
			if (!ProductsInPackage.TryGetValue(packageGroupId, out var productsPerPackage))
			{
				ProductsInPackage[packageGroupId] = productsPerPackage = new HashSet<ProductWithAttributes>();
			}

			productsPerPackage.Add(productAndAttributesKey);
		}

		static ProductWithAttributes GetProductWithAttributes(ICalculateProductPackageTotals line)
		{
			return new ProductWithAttributes(line.ClientPK, line.ProductPK,
				line.PartAttrib1, line.PartAttrib2, line.PartAttrib3, line.SerialNumber, line.ExpiryDate, line.PackingDate, "", line.AllocationKey, 0m);
		}
	}
}
