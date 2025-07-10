using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CREJobComInvoiceLineWrapper : ICREConsignmentItem
	{
		public CREJobComInvoiceLineWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine cannot be null");
		}
		readonly JobComInvoiceLine invoiceLine;

		ZShort ICREConsignmentItem.SequenceNumber => invoiceLine.JI_LineNo;

		ZBool ICREConsignmentItem.IsEmptyContainer => false;

		ZString ICREConsignmentItem.GoodsDescription => invoiceLine.JI_Description;

		IEnumerable<ICommodity> ICREConsignmentItem.Identifiers
		{
			get
			{
				foreach (CommodityProduct commodityProduct in invoiceLine.CommodityProducts)
				{
					yield return new CommodityDetails(commodityProduct.NZ_ProductID, commodityProduct.NZ_ProductIDType);
				}
			}
		}

		ZDecimal ICREConsignmentItem.Value => invoiceLine.JI_LinePrice;

		ZString ICREConsignmentItem.Currency => invoiceLine.JI_RX_NKLinePriceCurr;

		IEnumerable<IClassification> ICREConsignmentItem.Classifications
		{
			get
			{
				foreach (CommodityLine classification in invoiceLine.CommodityLines)
				{
					yield return new ClassificationDetails(classification.NZ_Classification, classification.NZ_ClassificationType);
				}
			}
		}

		ZDecimal ICREConsignmentItem.GrossWeightInKg => invoiceLine.GrossWeightInKG;

		ZString ICREConsignmentItem.GoodsOriginCountry => invoiceLine.JI_CountryOfOrigin;

		ZInt ICREConsignmentItem.PackageQty => invoiceLine.JI_InvoiceQuantity.ToZInt();

		ZString ICREConsignmentItem.PackageType => invoiceLine.JI_InvoiceUQ;

		ZString ICREConsignmentItem.ContainerNumber
		{
			get
			{
				var result = ZString.Empty;
				var invoiceLineContainers = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Where(x => x.IsForInvoiceLine).OrderBy(x => x.ContainerNumber);
				if (invoiceLineContainers.Any())
				{
					result = invoiceLineContainers.First().ContainerNumber;
				}
				return result;
			}
		}

		ZString ICREConsignmentItem.UNDGHazardousGoodsCode
		{
			get
			{
				var undgs = invoiceLine.UNDGs;
				return undgs.Count > 0 && undgs.FirstItemForBinding[0].UNDGSubstance != null ? undgs.FirstItemForBinding[0].UNDGSubstance.DG_Code : ZString.Empty;
			}
		}

		class CommodityDetails : ICommodity
		{
			public CommodityDetails(ZString number, ZString type)
			{
				CommodityNumber = number;
				CommodityType = type;
			}

			public ZString CommodityNumber { get; }

			public ZString CommodityType { get; }
		}

		class ClassificationDetails : IClassification
		{
			public ClassificationDetails(ZString classification, ZString typeCode)
			{
				Classification = classification;
				ClassificationTypeCode = typeCode;
			}

			public ZString Classification { get; }

			public ZString ClassificationTypeCode { get; }
		}
	}
}
