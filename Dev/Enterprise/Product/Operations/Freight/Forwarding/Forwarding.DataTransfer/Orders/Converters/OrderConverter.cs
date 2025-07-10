
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderConverter : FlatFileConverter<Xsd.Orders>
	{
		public OrderConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		protected override void MapImport(Xsd.Orders ordersValue, FlatFileDataRowCollection fileLines)
		{
			Xsd.Order orderXsd = null;
			foreach (FlatFileDataRow dataRow in fileLines)
			{
				OrderFlatFileDataRow orderDataRow = new OrderFlatFileDataRow(dataRow);

				switch (orderDataRow.RecordType)
				{
					case RecordTypes.POH:
						orderXsd = ordersValue.Order.AddNew();
						ProcessOrderHeader(orderXsd, new OrderHeaderFlatFileDataRow(dataRow));
						break;
					case RecordTypes.POL:
						if (orderXsd == null)
						{
							Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("4d9e2dc5-6e1c-4827-b2b8-5bd8b83566f6", "Invalid file format")));
						}
						else
						{
							ProcessOrderLine(orderXsd, new OrderLineFlatFileDataRow(dataRow));
						}
						break;
					case RecordTypes.PLD:
						if (OrderLine == null)
						{
							Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("7c2799b7-d6cc-4f3b-94c4-cec29a08200a", "Invalid file format")));
						}
						else
						{
							ProcessOrderLineDelivery(new OrderLineDeliveryFlatFileDataRow(dataRow));
						}
						break;
					case RecordTypes.INV:
						Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("d1c8c0d3-6905-47c2-a2b3-a2cd0decc1b8", "Invalid file format")));
						break;
				}
			}
		}

		#region Order Header

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ProcessOrderHeader(Xsd.Order orderValue, OrderHeaderFlatFileDataRow dataRow)
		{
			orderValue.OrderIdentifier.OrderNumber = dataRow.OrderNumber;
			orderValue.OrderDetail.InvoiceNumber = dataRow.SupplierInvoiceNumber;
			if (!dataRow.SupplierInvoiceDate.IsEmpty && dataRow.SupplierInvoiceDate.IsValid && dataRow.SupplierInvoiceDate.IsValidSmallDateTime)
			{
				orderValue.OrderDetail.InvoiceDate = dataRow.SupplierInvoiceDate;
			}
			orderValue.OrderDetail.TransportMode = OrderTransportModeToXmlCodeMappings.Instance.GetExternalCode(dataRow.TransportMode.Trim().Left(3), "TransportMode", Notification);
			orderValue.OrderDetail.TransportModeSpecified = true;
			orderValue.OrderDetail.ContainerMode = OrderContainerModeToXmlCodeMappings.Instance.GetExternalCode(dataRow.ContainerMode.Trim().Left(3), "ContainerMode", Notification);
			orderValue.OrderDetail.ContainerModeSpecified = true;
			if (!dataRow.OrderDate.IsEmpty && dataRow.OrderDate.IsValid && dataRow.OrderDate.IsValidSmallDateTime)
			{
				orderValue.OrderDetail.OrderDateTime = dataRow.OrderDate;
			}
			if (!dataRow.ExWorksRequiredBy.IsEmpty && dataRow.ExWorksRequiredBy.IsValid && dataRow.ExWorksRequiredBy.IsValidSmallDateTime)
			{
				orderValue.OrderDetail.ExWorksRequiredBy = dataRow.ExWorksRequiredBy;
			}
			if (!dataRow.DeliveryRequiredBy.IsEmpty && dataRow.DeliveryRequiredBy.IsValid && dataRow.DeliveryRequiredBy.IsValidSmallDateTime)
			{
				orderValue.OrderDetail.DeliveryRequiredBy = dataRow.DeliveryRequiredBy;
			}
			orderValue.OrderDetail.OrderStatus = dataRow.OrderStatus.Trim().Left(3);
			orderValue.OrderDetail.OrderTotal = Xsd.FinancialValue.FromAmountAndCurrencyCode(dataRow.TotalOrderAmount, dataRow.OrderCurrency);
			orderValue.OrderDetail.ExchangeRate = dataRow.EstimatedExchangeRate;
			orderValue.OrderDetail.ExchangeRateSpecified = true;
			orderValue.OrderDetail.Incoterm = dataRow.INCOTerm;
			orderValue.OrderDetail.AdditionalTerms = dataRow.AdditionalTerms;
			orderValue.OrderDetail.Description = dataRow.OrderGoodsDescription;
			orderValue.OrderDetail.ConfirmNumber = dataRow.ConfirmationNumber;
			if (!dataRow.ConfirmationDate.IsEmpty && dataRow.ConfirmationDate.IsValid && dataRow.ConfirmationDate.IsValidSmallDateTime)
			{
				orderValue.OrderDetail.ConfirmDate = dataRow.ConfirmationDate;
			}
			orderValue.OrderDetail.CountryOfOrigin = dataRow.CountryOfOrigin;

			orderValue.OrderDetail.ShipmentPlanning = new Xsd.OrderOrderDetailShipmentPlanning();
			orderValue.OrderDetail.ShipmentPlanning.Packs.Value = dataRow.TottalPacks;
			orderValue.OrderDetail.ShipmentPlanning.Packs.DimensionType = dataRow.PackType.Trim().Left(3);
			orderValue.OrderDetail.ShipmentPlanning.Volume.Value = dataRow.Volume;
			orderValue.OrderDetail.ShipmentPlanning.Volume.DimensionType = dataRow.UnitOfVolum.Trim().Left(2);
			orderValue.OrderDetail.ShipmentPlanning.Weight.Value = dataRow.Weight;
			orderValue.OrderDetail.ShipmentPlanning.Weight.DimensionType = dataRow.UnitOfWeight.Trim().Left(2);
			orderValue.OrderDetail.ShipmentPlanning.HouseBill = dataRow.HouseBill;
			orderValue.OrderDetail.ShipmentPlanning.DepartureVoyageFlight = dataRow.DepartureVoyageFlight;
			orderValue.OrderDetail.ShipmentPlanning.DepartureVessel = dataRow.DepartureVesselFlight;
			orderValue.OrderDetail.ShipmentPlanning.GoodsOrigin = Xsd.UNLOCO.FromPortCode(Factory, dataRow.GoodsOrigin);
			orderValue.OrderDetail.ShipmentPlanning.GoodsDestination = Xsd.UNLOCO.FromPortCode(Factory, dataRow.GoodsDestination);
			orderValue.OrderDetail.ShipmentPlanning.GoodsAvailAt = dataRow.GoodsAvaliableAt;
			orderValue.OrderDetail.ShipmentPlanning.GoodsDelivTo = dataRow.GoodsDeliveredTo;
			orderValue.OrderDetail.ShipmentPlanning.LoadPort = Xsd.UNLOCO.FromPortCode(Factory, dataRow.LoadPort);
			orderValue.OrderDetail.ShipmentPlanning.DischargePort = Xsd.UNLOCO.FromPortCode(Factory, dataRow.DischargePort);
			orderValue.OrderDetail.ShipmentPlanning.SendingAgent = new Xsd.Organisation();
			orderValue.OrderDetail.ShipmentPlanning.SendingAgent.OwnerCode = dataRow.SendingAgent;
			orderValue.OrderDetail.ShipmentPlanning.ReceivingAgent = new Xsd.Organisation();
			orderValue.OrderDetail.ShipmentPlanning.ReceivingAgent.OwnerCode = dataRow.ReceivingAgent;

			orderValue.OrderDetail.Supplier.OwnerCode = dataRow.SupplierCode;
			orderValue.OrderDetail.Supplier.OrganisationDetails.Name = dataRow.SupplierCompanyName;
			orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress supplierAddress = orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses.AddNew();
			supplierAddress.AddressLine1 = dataRow.SupplierAddress1;
			supplierAddress.AddressLine2 = dataRow.SupplierAddress2;
			supplierAddress.CityOrSuburb = dataRow.SupplierCity;
			supplierAddress.StateOrProvince = dataRow.SupplierState;
			supplierAddress.PostCode = dataRow.SupplierPostCode;
			supplierAddress.Location = Xsd.UNLOCO.FromPortCode(Factory, dataRow.SupplierUNLOCO);
			supplierAddress.Email = dataRow.SupplierEmailAddress;
			Xsd.TelephoneNumber supplierPhone = supplierAddress.TelephoneNumbers.AddNew();
			supplierPhone.Value = dataRow.SupplierPhone;

			orderValue.OrderDetail.Buyer.OwnerCode = dataRow.BuyerCode;
			orderValue.OrderDetail.Buyer.OrganisationDetails.Name = dataRow.BuyerCompanyName;
			orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress buyerAddress = orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses.AddNew();
			buyerAddress.AddressLine1 = dataRow.BuyerAddress1;
			buyerAddress.AddressLine2 = dataRow.BuyerAddress2;
			buyerAddress.CityOrSuburb = dataRow.BuyerCity;
			buyerAddress.StateOrProvince = dataRow.BuyerState;
			buyerAddress.PostCode = dataRow.BuyerPostCode;
			buyerAddress.Location = Xsd.UNLOCO.FromPortCode(Factory, dataRow.BuyerUNLOCO);
			buyerAddress.Email = dataRow.BuyerEmailAddress;
			Xsd.TelephoneNumber buyerPhone = buyerAddress.TelephoneNumbers.AddNew();
			buyerPhone.Value = dataRow.BuyerPhone;
			ZStringBuilder noteText = new ZStringBuilder();
			noteText.AppendIfNotEmpty(dataRow.GoodsHandlingNotes);
			noteText.AppendIfNotEmpty(dataRow.DGAdditionalHandlingNotes);
			noteText.AppendIfNotEmpty(dataRow.SpecialInstructions);
			noteText.AppendIfNotEmpty(dataRow.DeliveryInstructions);

			if (!noteText.IsEmpty)
			{
				Xsd.NotesNote specialInstructionsNote = orderValue.Notes.AddNew();
				specialInstructionsNote.NoteType = Xsd.NotesNoteNoteType.SpecialInstructions;
				specialInstructionsNote.NoteData = noteText.ToStringWithNewLineBetweenAppends();
			}

			#region Custom Fields

			if (!dataRow.CustomDate1.IsEmpty && dataRow.CustomDate1.IsValid && dataRow.CustomDate1.IsValidSmallDateTime)
			{
				orderValue.OrderDetail.Custom.Date1 = dataRow.CustomDate1;
			}
			if (!dataRow.CustomDate2.IsEmpty && dataRow.CustomDate2.IsValid && dataRow.CustomDate2.IsValidSmallDateTime)
			{
				orderValue.OrderDetail.Custom.Date2 = dataRow.CustomDate2;
			}

			orderValue.OrderDetail.Custom.Flag1 = dataRow.CustomFlag1;
			orderValue.OrderDetail.Custom.Flag1Specified = true;
			orderValue.OrderDetail.Custom.Flag2 = dataRow.CustomFlag2;
			orderValue.OrderDetail.Custom.Flag2Specified = true;
			orderValue.OrderDetail.Custom.Flag3 = dataRow.CustomFlag3;
			orderValue.OrderDetail.Custom.Flag3Specified = true;
			orderValue.OrderDetail.Custom.Flag4 = dataRow.CustomFlag4;
			orderValue.OrderDetail.Custom.Flag4Specified = true;
			orderValue.OrderDetail.Custom.Flag5 = dataRow.CustomFlag5;
			orderValue.OrderDetail.Custom.Flag5Specified = true;

			orderValue.OrderDetail.Custom.Text1 = dataRow.CustomAttrib1;
			orderValue.OrderDetail.Custom.Text2 = dataRow.CustomAttrib2;
			orderValue.OrderDetail.Custom.Text3 = dataRow.CustomAttrib3;
			orderValue.OrderDetail.Custom.Text4 = dataRow.CustomAttrib4;
			orderValue.OrderDetail.Custom.Text5 = dataRow.CustomAttrib5;

			if (!dataRow.CustomDecimal1.IsEmpty)
			{
				orderValue.OrderDetail.Custom.Decimal1 = dataRow.CustomDecimal1;
				orderValue.OrderDetail.Custom.Decimal1Specified = true;
			}
			if (!dataRow.CustomDecimal2.IsEmpty)
			{
				orderValue.OrderDetail.Custom.Decimal2 = dataRow.CustomDecimal2;
				orderValue.OrderDetail.Custom.Decimal2Specified = true;
			}
			if (!dataRow.CustomDecimal3.IsEmpty)
			{
				orderValue.OrderDetail.Custom.Decimal3 = dataRow.CustomDecimal3;
				orderValue.OrderDetail.Custom.Decimal3Specified = true;
			}
			if (!dataRow.CustomDecimal4.IsEmpty)
			{
				orderValue.OrderDetail.Custom.Decimal4 = dataRow.CustomDecimal4;
				orderValue.OrderDetail.Custom.Decimal4Specified = true;
			}
			if (!dataRow.CustomDecimal5.IsEmpty)
			{
				orderValue.OrderDetail.Custom.Decimal5 = dataRow.CustomDecimal5;
				orderValue.OrderDetail.Custom.Decimal5Specified = true;
			}

			orderValue.OrderDetail.Custom.Contact1 = dataRow.CustomContact1;
			orderValue.OrderDetail.Custom.Contact2 = dataRow.CustomContact2;

			#endregion
		}

		#endregion

		#region Order Line

		void ProcessOrderLine(Xsd.Order orderValue, OrderLineFlatFileDataRow dataRow)
		{
			OrderLine = orderValue.OrderLines.AddNew();// = new Xsd.OrderOrderLineCollection();

			OrderLine.OrderLineNo = dataRow.ClientOrderLineNumber;
			if (!dataRow.ClientOrderSubLineNumber.IsEmpty)
			{
				OrderLine.OrderSubLineNo = dataRow.ClientOrderSubLineNumber;
				OrderLine.OrderSubLineNoSpecified = true;
			}
			OrderLine.OrderLineDetail.Product = dataRow.SupplierProductCode;
			OrderLine.OrderLineDetail.Description = dataRow.SupplierProductDescription;
			OrderLine.OrderLineDetail.LineStatus = dataRow.LineStatus;
			OrderLine.OrderLineDetail.LineStatusSpecified = true;
			if (!dataRow.LineDropDate.IsEmpty && dataRow.LineDropDate.IsValid && dataRow.LineDropDate.IsValidSmallDateTime)
			{
				OrderLine.OrderLineDetail.DropDate = dataRow.LineDropDate;
			}
			OrderLine.OrderLineDetail.QtyOrdered.Value = dataRow.ProductQuantityOrdered;
			OrderLine.OrderLineDetail.QtyOrdered.DimensionType = dataRow.ProductUQ;
			OrderLine.OrderLineDetail.InnerPacks.Value = dataRow.InnerPacks;
			OrderLine.OrderLineDetail.OuterPacks.Value = dataRow.OuterPacks;

			OrderLine.OrderLineDetail.ItemPrice.Value = dataRow.ProductUnitPrice;
			OrderLine.OrderLineDetail.LinePrice.Value = dataRow.ProductLinePrice;

			#region Custom Fields

			OrderLine.OrderLineDetail.PartAttrib1 = dataRow.PartAttribute1;
			OrderLine.OrderLineDetail.PartAttrib2 = dataRow.PartAttribute2;
			OrderLine.OrderLineDetail.PartAttrib3 = dataRow.PartAttribute3;
			OrderLine.OrderLineDetail.Custom.Text1 = dataRow.CustomAttribute1;
			OrderLine.OrderLineDetail.Custom.Text2 = dataRow.CustomAttribute2;
			OrderLine.OrderLineDetail.Custom.Text3 = dataRow.CustomAttribute3;
			OrderLine.OrderLineDetail.Custom.Text4 = dataRow.CustomAttribute4;
			OrderLine.OrderLineDetail.Custom.Text5 = dataRow.CustomAttribute5;
			OrderLine.OrderLineDetail.Custom.Text6 = dataRow.CustomAttribute6;
			OrderLine.OrderLineDetail.Custom.CustomText1 = dataRow.CustomTextBlob;
			OrderLine.OrderLineDetail.Custom.Flag1 = dataRow.CustomFlag1;
			OrderLine.OrderLineDetail.Custom.Flag1Specified = true;
			OrderLine.OrderLineDetail.Custom.Flag2 = dataRow.CustomFlag2;
			OrderLine.OrderLineDetail.Custom.Flag2Specified = true;
			OrderLine.OrderLineDetail.Custom.Flag3 = dataRow.CustomFlag3;
			OrderLine.OrderLineDetail.Custom.Flag3Specified = true;
			OrderLine.OrderLineDetail.Custom.Flag4 = dataRow.CustomFlag4;
			OrderLine.OrderLineDetail.Custom.Flag4Specified = true;
			OrderLine.OrderLineDetail.Custom.Flag5 = dataRow.CustomFlag5;
			OrderLine.OrderLineDetail.Custom.Flag5Specified = true;
			if (!dataRow.CustomDate1.IsEmpty && dataRow.CustomDate1.IsValid && dataRow.CustomDate1.IsValidSmallDateTime)
			{
				OrderLine.OrderLineDetail.Custom.Date1 = dataRow.CustomDate1;
			}
			if (!dataRow.CustomDate2.IsEmpty && dataRow.CustomDate2.IsValid && dataRow.CustomDate2.IsValidSmallDateTime)
			{
				OrderLine.OrderLineDetail.Custom.Date2 = dataRow.CustomDate2;
			}
			if (!dataRow.CustomDate3.IsEmpty && dataRow.CustomDate3.IsValid && dataRow.CustomDate3.IsValidSmallDateTime)
			{
				OrderLine.OrderLineDetail.Custom.Date3 = dataRow.CustomDate3;
			}
			if (!dataRow.CustomDate4.IsEmpty && dataRow.CustomDate4.IsValid && dataRow.CustomDate4.IsValidSmallDateTime)
			{
				OrderLine.OrderLineDetail.Custom.Date4 = dataRow.CustomDate4;
			}
			if (!dataRow.CustomDate5.IsEmpty && dataRow.CustomDate5.IsValid && dataRow.CustomDate5.IsValidSmallDateTime)
			{
				OrderLine.OrderLineDetail.Custom.Date5 = dataRow.CustomDate5;
			}
			if (!dataRow.CustomDecimal1.IsEmpty)
			{
				OrderLine.OrderLineDetail.Custom.Decimal1 = dataRow.CustomDecimal1;
				OrderLine.OrderLineDetail.Custom.Decimal1Specified = true;
			}
			if (!dataRow.CustomDecimal2.IsEmpty)
			{
				OrderLine.OrderLineDetail.Custom.Decimal2 = dataRow.CustomDecimal2;
				OrderLine.OrderLineDetail.Custom.Decimal2Specified = true;
			}
			if (!dataRow.CustomDecimal3.IsEmpty)
			{
				OrderLine.OrderLineDetail.Custom.Decimal3 = dataRow.CustomDecimal3;
				OrderLine.OrderLineDetail.Custom.Decimal3Specified = true;
			}
			if (!dataRow.CustomDecimal4.IsEmpty)
			{
				OrderLine.OrderLineDetail.Custom.Decimal4 = dataRow.CustomDecimal4;
				OrderLine.OrderLineDetail.Custom.Decimal4Specified = true;
			}
			if (!dataRow.CustomDecimal5.IsEmpty)
			{
				OrderLine.OrderLineDetail.Custom.Decimal5 = dataRow.CustomDecimal5;
				OrderLine.OrderLineDetail.Custom.Decimal5Specified = true;
			}

			#endregion

			OrderLine.OrderLineDetail.ContainerNumber = dataRow.ContainerNumber;
			OrderLine.OrderLineDetail.ContainerPackingOrder = dataRow.ContainerPuckingOrder;
			OrderLine.OrderLineDetail.QtyInvoiced.Value = dataRow.UnitQtyInvoiced;
			OrderLine.OrderLineDetail.QtyReceived.Value = dataRow.UnitQuantityReceived;
			OrderLine.OrderLineDetail.Volume = dataRow.ActualVolume;
			OrderLine.OrderLineDetail.Weight = dataRow.ActualWeight;
			OrderLine.OrderLineDetail.VolumeType = dataRow.VolumeUnit;
			OrderLine.OrderLineDetail.WeightType = dataRow.WeightUnit;
			OrderLine.OrderLineDetail.SpecialInstructions = dataRow.SpecialInstractions;
			OrderLine.OrderLineDetail.AdditionalInformation = dataRow.AdditionalInformation;
		}

		#endregion

		#region Order Line Delivery

		void ProcessOrderLineDelivery(OrderLineDeliveryFlatFileDataRow dataRow)
		{
			Xsd.OrderOrderLineOrderLineDelivery delivery = OrderLine.OrderLineDeliveries.AddNew();
			delivery.DeliveryDetails.DelPort = Xsd.UNLOCO.FromPortCode(Factory, dataRow.DeliveryPort);
			delivery.DeliveryDetails.AddressFreeText = dataRow.DeliveryPointAddressShortCode;
			delivery.DeliveryDetails.QtyAllocated = (ZDecimal)dataRow.QtyDelivered;

			#region Custom Fields

			delivery.DeliveryDetails.Custom.Text1 = dataRow.CustomAttribute1;
			delivery.DeliveryDetails.Custom.Text2 = dataRow.CustomAttribute2;
			delivery.DeliveryDetails.Custom.Text3 = dataRow.CustomAttribute3;
			delivery.DeliveryDetails.Custom.Text4 = dataRow.CustomAttribute4;
			delivery.DeliveryDetails.Custom.Text5 = dataRow.CustomAttribute5;

			delivery.DeliveryDetails.Custom.Decimal1 = dataRow.CustomNumber1;
			delivery.DeliveryDetails.Custom.Decimal1Specified = true;

			delivery.DeliveryDetails.Custom.Decimal2 = dataRow.CustomNumber2;
			delivery.DeliveryDetails.Custom.Decimal2Specified = true;

			delivery.DeliveryDetails.Custom.Decimal3 = dataRow.CustomNumber3;
			delivery.DeliveryDetails.Custom.Decimal3Specified = true;

			delivery.DeliveryDetails.Custom.Decimal4 = dataRow.CustomNumber4;
			delivery.DeliveryDetails.Custom.Decimal4Specified = true;

			delivery.DeliveryDetails.Custom.Decimal5 = dataRow.CustomNumber5;
			delivery.DeliveryDetails.Custom.Decimal5Specified = true;

			delivery.DeliveryDetails.Custom.Flag1 = dataRow.CustomFlag1;
			delivery.DeliveryDetails.Custom.Flag1Specified = true;

			delivery.DeliveryDetails.Custom.Flag2 = dataRow.CustomFlag2;
			delivery.DeliveryDetails.Custom.Flag2Specified = true;

			delivery.DeliveryDetails.Custom.Flag3 = dataRow.CustomFlag3;
			delivery.DeliveryDetails.Custom.Flag3Specified = true;

			delivery.DeliveryDetails.Custom.Flag4 = dataRow.CustomFlag4;
			delivery.DeliveryDetails.Custom.Flag4Specified = true;

			delivery.DeliveryDetails.Custom.Flag5 = dataRow.CustomFlag5;
			delivery.DeliveryDetails.Custom.Flag5Specified = true;

			delivery.DeliveryDetails.Custom.Date1 = dataRow.CustomDate1;
			delivery.DeliveryDetails.Custom.Date2 = dataRow.CustomDate2;
			delivery.DeliveryDetails.Custom.Date3 = dataRow.CustomDate3;
			delivery.DeliveryDetails.Custom.Date4 = dataRow.CustomDate4;
			delivery.DeliveryDetails.Custom.Date5 = dataRow.CustomDate5;

			#endregion
		}

		#endregion

		Xsd.OrderOrderLine OrderLine;
	}
}
