using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingPackLineValueObjectDataAdapter<TBusinessObject, TValueObject> : PackLineValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : ForwardingPackLine
		where TValueObject : Xsd.Package
	{
		#region Import

		protected override void ImportFromValueObjectCore(TBusinessObject packLine, TValueObject value, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(packLine, value, context);
			ImportProducts(packLine, value);
		}

		void ImportProducts(ForwardingPackLine packline, Xsd.Package packageValue)
		{
			if (packageValue.PackageProducts.IsSpecified && packageValue.PackageProducts.Count > 0)
			{
				packline.Products.DeleteAll();

				foreach (Xsd.PackageProduct productValue in packageValue.PackageProducts)
				{
					if (!productValue.ProductCode.IsEmpty)
					{
						PackProduct packProduct = packline.Products.AddNew();
						packProduct.D2_ProductCode = productValue.ProductCode.SubstringSafe(0, JobPackProductSchema.D2_ProductCode.MaxLength);

						if (productValue.ProductQuantity.IsSpecified)
						{
							packProduct.D2_ProductQuantity = productValue.ProductQuantity.Value;
						}

						if (!productValue.ProductQuantity.DimensionType.IsEmpty)
						{
							packProduct.D2_ProductUnitOfQty = productValue.ProductQuantity.DimensionType.SubstringSafe(0, JobPackProductSchema.D2_ProductUnitOfQty.MaxLength);
						}
					}
				}
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(TBusinessObject packline, TValueObject value, IValueObjectExportContext context)
		{
			base.ExportToValueObjectCore(packline, value, context);
			ExportProducts(packline, value);
		}

		void ExportProducts(ForwardingPackLine packline, Xsd.Package packageValue)
		{
			foreach (PackProduct packProduct in packline.Products)
			{
				Xsd.PackageProduct productXSD = packageValue.PackageProducts.AddNew();
				productXSD.ProductCode = packProduct.D2_ProductCode;
				productXSD.ProductQuantity = Xsd.DimensionValue.FromAmountAndUnit(packProduct.D2_ProductQuantity, packProduct.D2_ProductUnitOfQty);
			}
		}

		#endregion
	}
}

