using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLineToPackLineConversionHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrderLineToPackLineConversionHelper(BusinessObjectFactory factory, ForwardingShipment shipment, IEnumerable<Order> orders)
			: base(factory)
		{
			Shipment = shipment;
			OriginalOrders = orders;

			InitializeHelper(orders, factory);
		}

		void InitializeHelper(IEnumerable<Order> orders, BusinessObjectFactory factory)
		{
			orderLines = new OrderLineCollection(factory, new AdhocCollectionRelationship(typeof(OrderLine)), true);

			if (orders != null)
			{
				foreach (Order order in orders)
				{
					orderLines.AddRange(order.OrderLines);
				}
			}

			dummyPackLines = new DummyPackLineCollection(factory);
			IsHelperBeingCancelled = false;
		}

		readonly ForwardingShipment Shipment;
		public bool IsHelperBeingCancelled { get; set; }

		#region Collections

		public OrderLineCollection OrderLines
		{
			get { return orderLines; }
		}
		OrderLineCollection orderLines;

		public DummyPackLineCollection DummyPackLines
		{
			get { return dummyPackLines; }
		}
		DummyPackLineCollection dummyPackLines;

		IEnumerable<Order> originalOrders;
		public IEnumerable<Order> OriginalOrders
		{
			get { return originalOrders; }
			set { originalOrders = value; }
		}

		#endregion

		#region Create Dummy Pack Line

		public void CreateDummyPackLine(IEnumerable<OrderLine> iEnumOrderLines)
		{
			List<OrderLine> listOrderLines = new List<OrderLine>();
			listOrderLines.AddRange(iEnumOrderLines);

			DummyPackLine dummy = CreateAndFillDummyPackLine(listOrderLines);
			DummyPackLines.Add(dummy);

			foreach (OrderLine orderLine in iEnumOrderLines)
			{
				OrderLines.RemoveFromRelationship(orderLine);
			}
		}

		DummyPackLine CreateAndFillDummyPackLine(List<OrderLine> linesToMerge)
		{
			DummyPackLine dummy = new DummyPackLine(Factory);

			if (linesToMerge.Count > 1)
			{
				string outerPackType = linesToMerge[0].JO_OuterPacksUQ != string.Empty ? linesToMerge[0].JO_OuterPacksUQ : linesToMerge[0].Order.JD_F3_NKPackType;
				bool sameOuterPackType = true;

				string innerPackType = linesToMerge[0].JO_InnerPacksUQ != string.Empty ? (string)linesToMerge[0].JO_InnerPacksUQ : Constants.PkgUnit.Package;
				bool sameInnerPackType = true;

				string volumeUnit = linesToMerge[0].JO_UnitOfVolume;
				string weightUnit = linesToMerge[0].JO_UnitOfWeight;

				string origin = linesToMerge[0].JO_RN_NKCountryOfOrigin;
				bool sameOrigin = true;

				string containerNumber = linesToMerge[0].JO_ContainerNumber;
				bool sameContainer = true;

				dummy.VolumeUnit = volumeUnit;
				dummy.WeightUnit = weightUnit;

				ZString products = "";

				foreach (OrderLine orderline in linesToMerge)
				{
					dummy.OuterPacks += orderline.JO_OuterPacks.ToZInt();
					dummy.InnerPacks += orderline.JO_InnerPacks.ToZInt();
					dummy.Volume += Core.Constants.Volume.Convert(orderline.JO_ActualVolume, orderline.JO_UnitOfVolume, volumeUnit);
					dummy.Weight += Core.Constants.Weight.Convert(orderline.JO_ActualWeight, orderline.JO_UnitOfWeight, weightUnit);

					string lineOuterPackType = orderline.JO_OuterPacksUQ != string.Empty ? orderline.JO_OuterPacksUQ : orderline.Order.JD_F3_NKPackType;
					if (outerPackType != lineOuterPackType)
					{
						sameOuterPackType = false;
					}

					string lineInnerPackType = orderline.JO_InnerPacksUQ != string.Empty ? (string)orderline.JO_InnerPacksUQ : Constants.PkgUnit.Package;
					if (innerPackType != lineInnerPackType)
					{
						sameInnerPackType = false;
					}

					if (origin != orderline.JO_RN_NKCountryOfOrigin)
					{
						sameOrigin = false;
					}

					if (containerNumber != orderline.JO_ContainerNumber)
					{
						sameContainer = false;
					}

					dummy.LinePrice += orderline.JO_LinePrice;

					if (!orderline.JO_Partno.IsEmpty && products.Length < DummyPackLine.ProductsMaxLength)
					{
						if (!products.IsEmpty)
						{
							products += ", ";
						}
						products += orderline.JO_Partno;
					}
				}

				dummy.Products = products.Length > DummyPackLine.ProductsMaxLength ? products.Substring(0, DummyPackLine.ProductsMaxLength) : products;

				dummy.OuterPackType = sameOuterPackType ? outerPackType : Constants.PkgUnit.Package;
				dummy.InnerPackType = sameInnerPackType ? innerPackType : Constants.PkgUnit.Package;

				if (sameOrigin)
				{
					dummy.Origin = origin;
				}

				if (sameContainer)
				{
					ZGuid containerPK = ContainerPKIfExist(containerNumber);
					if (containerPK != ZGuid.Empty)
					{
						dummy.ContainerNumber = containerNumber;
						dummy.ContainerPK = containerPK;
					}
				}

				dummy.MergedOrderLines.AddRange(linesToMerge);
			}

			return dummy;
		}

		public void UndoDummyPackLines(IEnumerable<DummyPackLine> dummyPackLinesToUndo)
		{
			foreach (DummyPackLine dummy in dummyPackLinesToUndo)
			{
				OrderLines.AddRange(dummy.MergedOrderLines);
				DummyPackLines.RemoveAndDelete(dummy);
			}
		}

		#endregion

		#region Create PackLines

		public string ReasonForNotAbleToConvert
		{
			get
			{
				string reason = string.Empty;

				if (Shipment.IsMasterShipmentRepresentingAllChildShipments)
				{
					reason = Res.GetString(
						"A0DD3E0F-FB1A-433B-91B0-8EE6C3226628",
						"You have attached orders to the '{0}' shipment. Order lines cannot be linked to the pack lines of the '{0}' shipment",
						this.Shipment.IsAssemblyMaster ? (NoResString)"Assembly Master" : (NoResString)"Co-Load Master");
				}
				else if (IsHelperBeingCancelled)
				{
					reason = Res.GetString("1ba88702-1cd0-4768-97c5-e1058479a7c7", "Canceled creating pack lines from order lines.");
					IsHelperBeingCancelled = false;
				}

				return reason;
			}
		}

		public void CreatePackLines()
		{
			if (string.IsNullOrEmpty(ReasonForNotAbleToConvert))
			{
				CreatePackLinesFromStandAloneOrderLines();
				CreatePackLinesFromDummyPackLines();
			}
		}

		void CreatePackLinesFromStandAloneOrderLines()
		{
			foreach (OrderLine orderline in OrderLines)
			{
				ForwardingPackLine outerPackline = Shipment.OuterPackLines.AddNew();
				string lineOuterPackType = orderline.JO_OuterPacksUQ != string.Empty ? orderline.JO_OuterPacksUQ : orderline.Order.JD_F3_NKPackType;
				FillPackLineDimensions(outerPackline, orderline.JO_OuterPackLength, orderline.JO_OuterPackHeight, orderline.JO_OuterPackWidth, orderline.JO_OuterPackUnitOfDimension);
				FillPackLineWithBasicData(outerPackline, orderline.JO_OuterPacks.ToZInt(), lineOuterPackType, orderline.JO_ActualVolume, orderline.JO_UnitOfVolume, orderline.JO_ActualWeight, orderline.JO_UnitOfWeight, orderline.JO_Description, orderline.JO_RN_NKCountryOfOrigin, orderline.JO_LinePrice);

				if (orderline.JO_InnerPacks != 0)
				{
					PackLine innerPackline = Shipment.InnerPackLines.AddNew();
					string lineInnerPackType = orderline.JO_InnerPacksUQ != string.Empty ? (string)orderline.JO_InnerPacksUQ : Constants.PkgUnit.Package;
					FillPackLineWithBasicData(innerPackline, orderline.JO_InnerPacks.ToZInt(), lineInnerPackType, 0, string.Empty, 0, string.Empty, orderline.JO_Description, orderline.JO_RN_NKCountryOfOrigin, orderline.JO_LinePrice);
				}

				outerPackline.JL_DetailedDescription = orderline.JO_AdditionalInformation;

				if (!orderline.JO_Partno.IsEmpty)
				{
					PackProduct packProduct = outerPackline.Products.AddNew();
					packProduct.D2_JO = orderline.PK;
				}

				ZGuid containerPK = ContainerPKIfExist(orderline.JO_ContainerNumber);
				if (containerPK != ZGuid.Empty)
				{
					outerPackline.JL_JC = containerPK;
				}
			}
		}

		void FillPackLineDimensions(PackLine packline, ZDecimal length, ZDecimal height, ZDecimal width, ZString unitOfDimensions)
		{
			packline.JL_Length = length;
			packline.JL_Height = height;
			packline.JL_Width = width;
			if (!unitOfDimensions.IsEmpty)
			{
				packline.JL_UnitOfDimension = unitOfDimensions;
			}
		}

		void CreatePackLinesFromDummyPackLines()
		{
			foreach (DummyPackLine dummy in DummyPackLines)
			{
				ForwardingPackLine packline = Shipment.OuterPackLines.AddNew();
				FillPackLineWithBasicData(packline, dummy.OuterPacks, dummy.OuterPackType, dummy.Volume, dummy.VolumeUnit, dummy.Weight, dummy.WeightUnit, dummy.Description, dummy.Origin, dummy.LinePrice);

				if (dummy.InnerPacks != 0)
				{
					PackLine innerPackline = Shipment.InnerPackLines.AddNew();
					FillPackLineWithBasicData(innerPackline, dummy.InnerPacks, dummy.InnerPackType, 0, string.Empty, 0, string.Empty, dummy.Description, dummy.Origin, dummy.LinePrice);
				}

				foreach (OrderLine orderline in dummy.MergedOrderLines)
				{
					if (!orderline.JO_AdditionalInformation.IsEmpty)
					{
						if (!packline.JL_DetailedDescription.IsEmpty)
						{
							packline.JL_DetailedDescription += System.Environment.NewLine;
						}
						packline.JL_DetailedDescription += orderline.Order.JD_OrderNumber + "." + orderline.JO_LineNo + ": " + orderline.JO_AdditionalInformation;
					}
					if (!orderline.JO_Partno.IsEmpty)
					{
						PackProduct packProduct = packline.Products.AddNew();
						packProduct.D2_JO = orderline.PK;
					}
				}

				if (dummy.ContainerNumber != "")
				{
					packline.JL_JC = dummy.ContainerPK;
				}
			}
		}

		void FillPackLineWithBasicData(PackLine packline, ZInt outerPacks, ZString outerPacksUnit, ZDecimal volume, ZString volumeUnit, ZDecimal weight, ZString weightUnit, ZString description, string origin, ZDecimal linePrice)
		{
			if (packline != null)
			{
				packline.JL_PackageCount = outerPacks;
				packline.JL_F3_NKPackType = outerPacksUnit;

				if (!volumeUnit.IsEmpty)
				{
					packline.JL_ActualVolumeUQ = volumeUnit;
				}
				packline.JL_ActualVolume = volume;

				if (!weightUnit.IsEmpty)
				{
					packline.JL_ActualWeightUQ = weightUnit;
				}
				packline.JL_ActualWeight = weight;

				packline.JL_Description = description.SubstringSafe(0, JobPackLinesSchema.JL_Description.MaxLength);
				packline.JL_RN_NKOrigin = origin;
				packline.JL_LinePrice = linePrice;
			}
		}

		ZGuid ContainerPKIfExist(string containerNumber)
		{
			if (Shipment.Consols.Count > 0)
			{
				ForwardingConsol consol = Shipment.Consols[0];
				bool oldAutomaticallyUpdate = consol.AutomaticallyUpdatePackLineContainers;
				consol.AutomaticallyUpdatePackLineContainers = false;

				try
				{
					foreach (ForwardingContainer container in consol.Containers)
					{
						if (container.JC_ContainerNum == containerNumber)
						{
							return container.PK;
						}
					}
				}
				finally
				{
					consol.AutomaticallyUpdatePackLineContainers = oldAutomaticallyUpdate;
				}
			}

			return ZGuid.Empty;
		}

		#endregion

		#region Spliting All Orders

		public List<Order> GetNewOrSplitOrdersFromOriginalOrdersList(CreateOrderType createOrderType)
		{
			List<Order> newOrdersList = new List<Order>();

			foreach (Order order in OriginalOrders)
			{
				if (order.IsOrderPartiallyCompleteAndIsNotAlreadySplit)
				{
					Order newOrder = order.SplitOrder(createOrderType);
					newOrdersList.Add(newOrder);
				}
			}

			return newOrdersList;
		}

		public bool ShouldShowSplitOrders
		{
			get
			{
				return OriginalOrders.Any(o => o.IsOrderPartiallyCompleteAndIsNotAlreadySplit);
			}
		}

		#endregion

		#region Unattach Orders From Shipment

		public void UnattachOrdersFromShipment()
		{
			foreach (var order in OriginalOrders)
			{
				var orderLoadedInShipmentFactory = Shipment.Factory.Load<Order>(order.PK);
				Shipment.AttachedOrders.RemoveFromRelationship(orderLoadedInShipmentFactory);
			}
		}

		#endregion
	}
}
