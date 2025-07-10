using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class PackProductMerger : BusinessObjectMerger<PackProduct>
	{
		public PackProductMerger(IEnumerable<PackProduct> sourceList, ForwardingPackLine newPackLine)
			: base(sourceList)
		{
			this.newPackLine = newPackLine;
		}

		readonly ForwardingPackLine newPackLine;

		public override string CheckMerger()
		{
			return string.Empty;
		}

		public override void DoMerge()
		{
			GroupAndMergeProducts(SourceList.ToList());
		}

		void GroupAndMergeProducts(List<PackProduct> sourceProducts)
		{
			var firstProduct = sourceProducts.FirstOrDefault();
			if (firstProduct != null)
			{
				var sameProducts = sourceProducts
					.Where(c => c.PK == firstProduct.PK
						|| (HaveSameValuesInComparableColumns(c, firstProduct) && IsNotDGGroups(c)))
					.ToList();

				foreach (var product in sameProducts)
				{
					product.D2_JL = newPackLine.PK;
					sourceProducts.Remove(product);
				}

				if (sameProducts.Count > 1)
				{
					MergeProducts(sameProducts);
				}

				GroupAndMergeProducts(sourceProducts);
			}
		}

		bool IsNotDGGroups(PackProduct packProduct)
		{
			return packProduct.Product == null || !packProduct.Product.UNDGs.Any();
		}

		void MergeProducts(IList<PackProduct> products)
		{
			PackProduct firstProduct = null;

			foreach (var product in products.ToArray())
			{
				if (product != null)
				{
					if (firstProduct == null)
					{
						firstProduct = product;
						firstProduct.D2_ProductQuantity = products.Sum(c => c.D2_ProductQuantity);
						firstProduct.D2_JL = newPackLine.PK;

						newPackLine.Products.Add(firstProduct);
					}
					else
					{
						product.PackLine.Products.RemoveFromRelationship(product);
						product.Delete();
					}
				}
			}
		}

		protected override IEnumerable<SchemaColumn> GetComparableColumnsCore()
		{
			return new SchemaColumn[]
			{
				JobPackProductSchema.D2_JL,
				JobPackProductSchema.D2_JO,
				JobPackProductSchema.D2_ProductCode,
				JobPackProductSchema.D2_ProductUnitOfQty
			};
		}

		protected override IEnumerable<SchemaColumn> GetIgnoredColumnsCore()
		{
			return new SchemaColumn[]
			{
				JobPackProductSchema.PK,
				JobPackProductSchema.D2_ProductQuantity,
				JobPackProductSchema.D2_SystemCreateTimeUtc,
				JobPackProductSchema.D2_SystemCreateUser,
				JobPackProductSchema.D2_SystemLastEditTimeUtc,
				JobPackProductSchema.D2_SystemLastEditUser
			};
		}
	}
}
