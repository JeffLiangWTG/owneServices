using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	internal class ForwardersCargoReceiptDetailBuilder : ForwardersCargoReceiptBuilder
	{
		public ForwardersCargoReceiptDetailBuilder(ForwardingShipment shipment, IDocDataObjectParameters parameters) : base(shipment, parameters)
		{
		}

		internal ForwardersCargoReceiptDetail Build()
		{
			var wrapper = base.Build(new ForwardersCargoReceiptDetail());

			if (shipment == null)
			{
				return wrapper;
			}

			wrapper.DescriptionOfGoods = shipment.JS_GoodsDescription;
			wrapper.DetailedDescriptionOfGoods = shipment.DetailedGoodsDescriptionNoteText;

			PopulateContainerPackingInfos(wrapper);

			wrapper.ValidateAllIncludingChildren();

			return wrapper;
		}

		void PopulateContainerPackingInfos(ForwardersCargoReceiptDetail wrapper)
		{
			var consol = shipment.Consols.FirstOrDefault() as ForwardingConsol;

			var packingInfos = new List<ContainerPackingInfo>();

			if (consol != null)
			{
				var containerGroups = shipment.OuterPackLines.Cast<ForwardingPackLine>()
					.GroupBy(p => GetGroupKey(p, consol));

				foreach (var group in containerGroups.OrderBy(g => g.Key))
				{
					var info = CreateContainerPackingInfo(group);
					packingInfos.Add(info);
				}
			}

			wrapper.ContainerPackingInfos = packingInfos;

			PopulateTotalOfContainerPackingInfos(wrapper);
		}

		void PopulateTotalOfContainerPackingInfos(ForwardersCargoReceiptDetail wrapper)
		{
			wrapper.OrderLineCount = (from info in wrapper.ContainerPackingInfos
									  from line in info.PackedOrderLines
									  where !line.OrderLineNumber.IsEmpty
									  select line).Count();

			wrapper.ItemNumberCount = (from info in wrapper.ContainerPackingInfos
									   from line in info.PackedOrderLines
									   where !line.ProductCode.IsEmpty
									   select line.ProductCode).Count();

			wrapper.PackageCount = shipment.JS_OuterPacks;
			wrapper.PackageType = new CodeDescription(shipment.Lookups.PackTypes) { Code = shipment.JS_F3_NKPackType };

			wrapper.GrossVolume = new Measurement()
			{
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = Constants.Volume.CubicMetres
				}
			};

			wrapper.GrossVolume.Value = (from info in wrapper.ContainerPackingInfos
										 from line in info.PackedOrderLines
										 select line.GrossVolume.Value).DefaultIfEmpty(0).Sum(val => val);

			wrapper.GrossWeight = new Measurement()
			{
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Constants.Weight.Kilograms
				}
			};

			wrapper.GrossWeight.Value = (from info in wrapper.ContainerPackingInfos
										 from line in info.PackedOrderLines
										 select line.GrossWeight.Value).DefaultIfEmpty(0).Sum(val => val);

			wrapper.PackedQuantity = (from info in wrapper.ContainerPackingInfos
									  from line in info.PackedOrderLines
									  select line.PackedQuantity).DefaultIfEmpty(0).Sum(val => val);

			var unitOfQuantityList = (from info in wrapper.ContainerPackingInfos
									  from line in info.PackedOrderLines
									  select (line.UnitOfQuantity?.Code.IsEmpty ?? ZBool.True) ? (ZString)Constants.PkgUnit.Unit : line.UnitOfQuantity.Code).Distinct();

			wrapper.UnitOfQuantity = new CodeDescription(shipment.Lookups.PackTypes)
			{
				Code = unitOfQuantityList.Count() == 1 ? unitOfQuantityList.FirstOrDefault() : (ZString)Constants.PkgUnit.Unit
			};
		}

		static Tuple<ZInt, ZString?> GetGroupKey(ForwardingPackLine packline, ForwardingConsol consol)
		{
			var container = packline.GetContainer(consol);
			if (container != null)
			{
				if (container.JC_ContainerNum.IsEmpty)
				{
					return new Tuple<ZInt, ZString?>(2, $"{container.JC_ContainerCount} x {container.RefContainer?.RC_Code ?? ZString.Empty}");
				}

				return new Tuple<ZInt, ZString?>(1, container.JC_ContainerNum);
			}
			return new Tuple<ZInt, ZString?>(3, null);
		}

		ContainerPackingInfo CreateContainerPackingInfo(IGrouping<Tuple<ZInt, ZString?>, ForwardingPackLine> group)
		{
			var info = new ContainerPackingInfo();
			info.ContainerNumber = group.Key.Item2 ?? ZString.Empty;

			var packedOrderLines = new List<PackedOrderLine>();

			foreach (var line in group)
			{
				var packedOrderLine = new PackedOrderLine();
				packedOrderLine.GoodsDescription = line.JL_Description;
				packedOrderLine.MarksAndNos = line.JL_MarksAndNumbers;

				var orderLine = line.LoadListLine?.SupplierBookingLine.OrderLine;

				if (orderLine != null)
				{
					packedOrderLine.OrderLineNumber = new ZString($"{orderLine.Order.JD_OrderNumber}-{orderLine.JO_LineNo}");
					packedOrderLine.PackedQuantity = line.LoadListLine.CLL_PackedQuantity;
					packedOrderLine.UnitOfQuantity = new CodeDescription(orderLine.Lookups.PackTypes) { Code = orderLine.JO_F3_NKPackType };
				}

				packedOrderLine.PackageCount = line.JL_PackageCount;
				packedOrderLine.PackageType = new CodeDescription(line.Lookups.PackTypes) { Code = line.JL_F3_NKPackType };
				packedOrderLine.ProductCode = line.Products.Cast<PackProduct>()?.FirstOrDefault()?.D2_ProductCode ?? ZString.Empty;

				packedOrderLine.GrossWeight = new Measurement()
				{
					Value = Constants.Weight.Convert(line.JL_ActualWeight, line.JL_ActualWeightUQ, Constants.Weight.Kilograms),
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Constants.Weight.Kilograms
					}
				};

				packedOrderLine.GrossVolume = new Measurement()
				{
					Value = Constants.Volume.Convert(line.JL_ActualVolume, line.JL_ActualVolumeUQ, Constants.Volume.CubicMetres),
					Unit = new CodeDescription(context.VolumeUnits)
					{
						Code = Constants.Volume.CubicMetres
					}
				};

				packedOrderLines.Add(packedOrderLine);
				info.PackedOrderLines = packedOrderLines;
			}

			return info;
		}
	}
}
