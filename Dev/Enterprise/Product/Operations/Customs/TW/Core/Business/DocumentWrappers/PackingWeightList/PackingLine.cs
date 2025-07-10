using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public class PackingLine : NonPersistentBusinessObject
	{
		internal PackingLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PackingLine(BasePackage package, BusinessObjectFactory factoryToWrap)
			: this(package, factoryToWrap, package.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>())
		{
		}

		public PackingLine(BasePackage package, BusinessObjectFactory factoryToWrap, IEnumerable<InvoiceLinePackagePivot> invoiceLinesPivotCollection) : this(factoryToWrap)
		{
			fPackage = package;
			goodsDescriptionsList = new List<DescriptionInvoiceQuantitiesInfo>();
			GroupPackingLines = new List<PackingLine>();
			if (invoiceLinesPivotCollection != null)
			{
				var firstInvoiceLine = invoiceLinesPivotCollection.Select(x => x?.InvoiceLine as JobComInvoiceLine).Where(x => x != null && !x.JI_Group.IsEmpty).OrderByDescending(x => x.JI_LineNo).FirstOrDefault();
				if (firstInvoiceLine != null)
				{
					GoodsGrouping = firstInvoiceLine.JI_Group;
				}

				foreach (var invoiceLinePivot in invoiceLinesPivotCollection)
				{
					var invoiceLine = (JobComInvoiceLine)invoiceLinePivot.InvoiceLine;
					var description = invoiceLine.JI_DeclarationGoodsDescription;

					var invoiceUQ = invoiceLinePivot.InvoiceLine.JI_InvoiceUQ;
					var pivot = (ICusQuantityPivot)invoiceLinePivot;

					var invoiceQuantityInfo = new InvoiceQuantityInfo(invoiceUQ, pivot.Quantity);

					AddGoodsDescription(package, factoryToWrap, description, invoiceQuantityInfo);
				}
			}
		}

		public ZString OnlyGoodsDescriptionAndQuantity { get; private set; }

		internal List<PackingLine> GroupPackingLines;

		internal void AddGoodsDescriptions(DescriptionInvoiceQuantitiesInfo descriptionInvoiceQuantitiesInfo)
		{
			goodsDescriptionsList.Add(descriptionInvoiceQuantitiesInfo);
		}

		void AddGoodsDescription(BasePackage package, BusinessObjectFactory factoryToWrap, ZString description, InvoiceQuantityInfo invoiceQuantityInfo)
		{
			var descriptionInvoiceQuantitiesInfo = new DescriptionInvoiceQuantitiesInfo(description);
			descriptionInvoiceQuantitiesInfo.AddOrUpdateInvoiceQuantitiesList(invoiceQuantityInfo);
			if (!goodsDescriptionsList.Any())
			{
				AddGoodsDescriptions(descriptionInvoiceQuantitiesInfo);
			}
			else
			{
				var groupPackingLine = new PackingLine(package, factoryToWrap, null) { OnlyGoodsDescriptionAndQuantity = "Y" };
				groupPackingLine.AddGoodsDescriptions(descriptionInvoiceQuantitiesInfo);
				GroupPackingLines.Add(groupPackingLine);
			}
		}

		bool AverageIsCalculated => PackQtyInfo > 1;

		readonly BasePackage fPackage;
		readonly List<DescriptionInvoiceQuantitiesInfo> goodsDescriptionsList;

		#region Pack No
		public ZString PackNoInfo => !OnlyGoodsDescriptionAndQuantity.IsEmpty ? ZString.Empty : fPackage.CW_MarksAndNos;
		#endregion

		#region Goods Description and Invoice Quantities
		internal class InvoiceQuantityInfo
		{
			public InvoiceQuantityInfo(ZString unitQty, ZDecimal quantity)
			{
				UnitQty = unitQty;
				Quantity = quantity;
			}

			public ZString UnitQty { get; }
			public ZDecimal Quantity { get; set; }
		}

		internal class DescriptionInvoiceQuantitiesInfo
		{
			public DescriptionInvoiceQuantitiesInfo(ZString description)
			{
				Description = description;
				InvoiceQuantitiesList = new List<InvoiceQuantityInfo>();
			}

			public ZString Description { get; }
			public List<InvoiceQuantityInfo> InvoiceQuantitiesList { get; }

			public void AddOrUpdateInvoiceQuantitiesList(InvoiceQuantityInfo newInvoiceQuantityInfo)
			{
				foreach (var invoiceQuantity in InvoiceQuantitiesList)
				{
					if (invoiceQuantity.UnitQty == newInvoiceQuantityInfo.UnitQty)
					{
						invoiceQuantity.Quantity += newInvoiceQuantityInfo.Quantity;
						return;
					}
				}
				InvoiceQuantitiesList.Add(newInvoiceQuantityInfo);
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				var result = ZString.Empty;
				foreach (var descriptionInvoiceQuantityInfo in goodsDescriptionsList)
				{
					result += descriptionInvoiceQuantityInfo.Description;
					int linebreaks = AverageIsCalculated ? 2 : 1;
					result += new ZString('\n', descriptionInvoiceQuantityInfo.InvoiceQuantitiesList.Count * linebreaks);
				}
				result = result.TrimEnd();
				return result;
			}
		}

		public ZString GoodsGrouping { get; set; }

		public ZString QuantityInfo
		{
			get
			{
				var result = new ZStringBuilder();
				foreach (var descriptionInvoiceQuantityInfo in goodsDescriptionsList)
				{
					foreach (var invoiceQuantityInfo in descriptionInvoiceQuantityInfo.InvoiceQuantitiesList)
					{
						result.AppendIfNotEmpty(DocumentWrapperHelper.GetDetailInfo(invoiceQuantityInfo.Quantity, invoiceQuantityInfo.UnitQty, PackQtyInfo));
					}
				}
				return result.ToStringWithNewLineBetweenAppends();
			}
		}
		#endregion

		#region Net Weight
		public ZString NetWeightInfo => !OnlyGoodsDescriptionAndQuantity.IsEmpty ? ZString.Empty : DocumentWrapperHelper.GetDetailInfo(NetWeight, NetWeightUQ, PackQtyInfo);

		public ZDecimal NetWeight => fPackage.CW_NetWeight;

		public ZString NetWeightUQ => fPackage.CW_NetWeightUQ;
		#endregion

		#region Gross Wegiht
		public ZString GrossWeightInfo => !OnlyGoodsDescriptionAndQuantity.IsEmpty ? ZString.Empty : DocumentWrapperHelper.GetDetailInfo(GrossWeight, GrossWeightUQ, PackQtyInfo);

		public ZDecimal GrossWeight => fPackage.CW_GrossWeight;

		public ZString GrossWeightUQ => fPackage.CW_GrossWeightUQ;
		#endregion

		#region Volume
		public ZString VolumeInfo => DocumentWrapperHelper.GetPackageVolumeInfo(OnlyGoodsDescriptionAndQuantity, Volume, VolumeUQ, PackQtyInfo, Dimension);

		public ZDecimal Volume => fPackage.CW_Volume;

		ZString Dimension => DocumentWrapperHelper.GetPackageDimension(fPackage.CW_Length, fPackage.CW_Width, fPackage.CW_Height, fPackage.CW_DimensionUQ);

		public ZString VolumeUQ => DocumentWrapperHelper.GetPackageVolumeUQInfo(fPackage.CW_VolumeUQ);
		#endregion

		#region Pack Qty And Type
		public ZString PackTypeInfo => fPackage.CW_PackType;

		public ZInt PackQtyInfo => fPackage.CW_PackQty;
		#endregion

		#region Pivot Quantity
		public IDictionary<ZString, ZDecimal> PivotQuantityInfo
		{
			get
			{
				var result = new Dictionary<ZString, ZDecimal>();
				foreach (var descriptionInvoiceQuantityInfo in goodsDescriptionsList)
				{
					foreach (var invoiceQuantityInfo in descriptionInvoiceQuantityInfo.InvoiceQuantitiesList)
					{
						if (result.ContainsKey(invoiceQuantityInfo.UnitQty))
						{
							result[invoiceQuantityInfo.UnitQty] += invoiceQuantityInfo.Quantity;
						}
						else
						{
							result.Add(invoiceQuantityInfo.UnitQty, invoiceQuantityInfo.Quantity);
						}
					}
				}
				return result;
			}
		}
		#endregion
	}
}
