using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Header = Enterprise.Warehouse.Transactions.DataTransfer.WhsDocketConstants.HeaderRecord;
using Line = Enterprise.Warehouse.Transactions.DataTransfer.WhsDocketConstants.LineRecord;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderDataConverter : WhsDocketDataConverter
	{
		public WhsOrderDataConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void PopulateDocketHeader(Xsd.WhsDocket xsdWhsDocket, FlatFileDataRow row)
		{
			base.PopulateDocketHeader(xsdWhsDocket, row);

			xsdWhsDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
			Xsd.WhsCustomerOrderDetail customerOrderDetail = (Xsd.WhsCustomerOrderDetail)xsdWhsDocket.DocketDetail.Item;

			customerOrderDetail.OrderType = row[Header.OrderType];
			customerOrderDetail.DateRequired = row.GetFieldAsZDateTime(Header.DateRequired, "yyyyMMdd");
			customerOrderDetail.TotalBillToInvoiceAmount = row.GetFieldAsZDecimal(Header.TotalBillToInvoiceAmount);

			if (!row[Header.ConsigneeCode].IsEmpty || !row[Header.ConsigneeName].IsEmpty)
			{
				PopulateXsdDocAddress(customerOrderDetail.Consignee, Xsd.DocAddressAddressType.CEA, row[Header.ConsigneeCode], row[Header.ConsigneeName],
									row[Header.ConsigneeAddress1], row[Header.ConsigneeAddress2],
									row[Header.ConsigneeCity],
									row[Header.ConsigneeState],
									row[Header.ConsigneePostCode],
									row[Header.ConsigneeISOCountryCode],
									row[Header.ConsigneeUNLOCO],
									row[Header.ConsigneeEmailAddress],
									row[Header.ConsigneePhone],
									row[Header.ConsigneeContact]
									);
			}
			if (!row[Header.GoodsBilledToCode].IsEmpty || !row[Header.GoodsBilledToName].IsEmpty)
			{
				PopulateXsdDocAddress(customerOrderDetail.GoodsBilledTo, Xsd.DocAddressAddressType.GBA, row[Header.GoodsBilledToCode], row[Header.GoodsBilledToName],
									row[Header.GoodsBilledToAddress1], row[Header.GoodsBilledToAddress2],
									row[Header.GoodsBilledToCity],
									row[Header.GoodsBilledToState],
									row[Header.GoodsBilledToPostCode],
									row[Header.GoodsBilledToISOCountryCode],
									row[Header.GoodsBilledToUNLOCO],
									row[Header.GoodsBilledToEmailAddress],
									row[Header.GoodsBilledToPhone],
									row[Header.GoodsBilledToContact]
									);
			}
		}

		protected override void PopulateDocketLine(Xsd.WhsDocketLine xsdWhsDocketLine, FlatFileDataRow row)
		{
			base.PopulateDocketLine(xsdWhsDocketLine, row);
			xsdWhsDocketLine.LineAttributes.CustomAttribute1 = row[Line.CustomAttribute1];
			xsdWhsDocketLine.LineAttributes.CustomAttribute2 = row[Line.CustomAttribute2];
			xsdWhsDocketLine.LineAttributes.CustomAttribute3 = row[Line.CustomAttribute3];
			xsdWhsDocketLine.Item = new Xsd.WhsCustomerOrderLineDetail();
			Xsd.WhsCustomerOrderLineDetail customerOrderLineDetail = (Xsd.WhsCustomerOrderLineDetail)xsdWhsDocketLine.Item;

			customerOrderLineDetail.IsSpecified = true;
			customerOrderLineDetail.ConsigneeOrBuyerProductCode = row[Line.ConsigneeOrBuyerProductCode];
			customerOrderLineDetail.ConsigneeOrBuyerProductDescription = row[Line.ConsigneeOrBuyerProductDescription];
			customerOrderLineDetail.Pricing.IsSpecified = true;
			customerOrderLineDetail.Pricing.RecommendedUnitPrice = row.GetFieldAsZDecimal(Line.RecommendedUnitPrice);
			customerOrderLineDetail.Pricing.UnitDiscount = row.GetFieldAsZDecimal(Line.UnitDiscount);
			customerOrderLineDetail.Pricing.UnitDiscountAmount = row.GetFieldAsZDecimal(Line.UnitDiscountAmount);
			customerOrderLineDetail.Pricing.UnitPriceAfterDiscount = row.GetFieldAsZDecimal(Line.UnitPriceAfterDiscount);
			customerOrderLineDetail.Pricing.ExtendedPrice = row.GetFieldAsZDecimal(Line.ExtendedPrice);
		}
	}
}
