using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class DocPackingLineBuilder
	{
		public virtual DocPackingLine Build()
		{
			var docPackingLine = new DocPackingLine(ZGuid.NewZGuid());
			AddValidation(docPackingLine);
			return docPackingLine;
		}

		protected DocPackingLine CreatePackingLineFromPackageStates(IEnumerable<WhsItemPackageState> packageStates)
		{
			var docPackingLine = new DocPackingLine(ZGuid.NewZGuid());
			docPackingLine.AmountQuantity = GetSumQuantity(packageStates);
			docPackingLine.AmountWeight = GetSumWeight(packageStates);
			docPackingLine.Description = GetGoodsDescription(packageStates);

			AddValidation(docPackingLine);
			return docPackingLine;
		}

		protected int GetSumQuantity(IEnumerable<WhsItemPackageState> packages)
		{
			return !packages.Any()
				? 0
				: packages.Sum(p => p.Package.KP_PackageQty);
		}

		protected decimal GetSumWeight(IEnumerable<WhsItemPackageState> packages)
		{
			return !packages.Any()
				? 0
				: packages.Sum(p => Core.Constants.Weight.Convert(p.Package.KP_Weight, p.Package.KP_WeightUQ, Core.Constants.Weight.Kilograms));
		}

		protected ZString GetGoodsDescription(IEnumerable<WhsItemPackageState> packages)
		{
			var goodsDescription = ZString.Empty;

			if (packages.Any())
			{
				var packageWithGoodsDescription = packages.OrderByDescending(p => p.Inners).ThenBy(p => p.Package.KP_PackageID).FirstOrDefault(p => !p.Package.KP_GoodsDescription.IsEmpty);
				if (packageWithGoodsDescription != null)
				{
					goodsDescription = packageWithGoodsDescription.Package.KP_GoodsDescription;
				}
			}

			return goodsDescription;
		}

		protected void SetShipmentDescriptionIfNotEmpty(DocPackingLine docPackingLine, IConsignment consignment, IEnumerable<WhsItemPackageState> ovps = null)
		{
			var shipmentDescription = consignment?.AdditionalReferenceNumbers.GetForwardingShipmentDescription() ?? ZString.Empty;
			if (!shipmentDescription.IsEmpty)
			{
				docPackingLine.Description = shipmentDescription;
			}
			else
			{
				if (ovps != null)
				{
					var ovpDescription = GetGoodsDescription(ovps);
					if (!ovpDescription.IsEmpty)
					{
						docPackingLine.Description = ovpDescription;
					}
				}
			}
		}

		protected virtual void AddValidation(DocPackingLine docPackingLine)
		{
			docPackingLine.AmountQuantityInfo.AddMessageErrorIfEmpty(Res.GetString("930daa11-4681-4301-b1f5-8d74e8a785f3", "Amount Quantity is required."));
			docPackingLine.AmountQuantityInfo.AddMessageError(() => docPackingLine.AmountQuantity < -99999, Res.GetString("7690b354-e0b6-4c4d-acf4-2b51f5570849", "Amount Quantity can not be less than -99999."));
			docPackingLine.AmountQuantityInfo.AddMessageError(() => docPackingLine.AmountQuantity > 99999, Res.GetString("8811be2a-fac4-46c8-8db7-ba7c633ac5ef", "Amount Quantity can not be greater than 99999."));

			docPackingLine.AmountWeightInfo.AddMessageErrorIfEmpty(Res.GetString("7c8ddc01-8d6e-4dde-9209-cfcb236529c0", "Amount Weight is required."));
			docPackingLine.AmountWeightInfo.AddMessageError(() => docPackingLine.AmountWeight < -99999.999m, Res.GetString("53e46271-4e2d-448b-8885-aea63818c606", "Amount Weight can not be less than -99999.999."));
			docPackingLine.AmountWeightInfo.AddMessageError(() => docPackingLine.AmountWeight > 99999.999m, Res.GetString("1b973df1-b5cd-41ea-8880-0007f21195b1", "Amount Weight can not be greater than 99999.999."));
			docPackingLine.AmountWeightInfo.AddMessageError(() => docPackingLine.AmountWeight != docPackingLine.AmountWeight, Res.GetString("43971e95-a8f4-444e-a07a-4a7415eac8e6", "The decimal precision of Amount Weight can not be greater than 3 digits."));

			docPackingLine.DescriptionInfo.AddMessageErrorIfEmpty(Res.GetString("bb432407-05e0-4afb-b2db-afeed950919a", "Goods Description is required."));
		}
	}
}
