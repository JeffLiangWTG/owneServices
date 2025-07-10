using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalShipmentOrder = Enterprise.UniversalDataBuss.DataObjects.Universal.Order;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderDataObjectReader : ShipmentDataObjectReader<Order>
	{
		public OrderDataObjectReader(UniversalShipment orderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IAttachOrders parent = null)
			: base(orderDataObject, logger, factory)
		{
			Parent = parent;
		}

		public OrderDataObjectReader(UniversalShipment orderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IOrderLineLinkManager orderLineLinkManager, IAttachOrders parent = null)
			: this(orderDataObject, logger, factory, parent)
		{
			Parent = parent;
			this.orderLineLinkManager = orderLineLinkManager;
		}

		readonly IAttachOrders Parent;
		readonly IOrderLineLinkManager orderLineLinkManager;

		List<Order> Orders;

		#region Context

		public override DataContextType DataContextType
		{
			get { return DataContextType.OrderManagerOrder; }
		}

		#endregion

		#region Matching Job

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override Order GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			Order result = null;

			var orderDataObject = dataObject.Order;
			if (orderDataObject != null)
			{
				var orderNumber = orderDataObject.OrderNumber.GetValueOrDefault();
				if (!orderNumber.IsEmpty && !BuyerPK.IsMissing)
				{
					var query = new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
					query.IgnoreActiveFilter = true;

					var buyer = factory.Load<OrgHeader>(BuyerPK);
					query.AddToFilter(JobOrderHeaderSchema.JD_IsCancelled, false);
					query.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyer == null ? ZGuid.Empty : buyer.Addresses.Select(x => x.PK));

					Orders = new List<Order>();
					Orders.AddRange(factory.Load<Order>(query));

					if (orderDataObject.OrderNumberSplit.HasValue)
					{
						result = Orders.FirstOrDefault(order => order.JD_OrderNumberSplit == orderDataObject.OrderNumberSplit.Value);
					}
					else if (Parent == null)
					{
						result = Orders.OrderByDescending(order => order.JD_OrderNumberSplit).FirstOrDefault();
					}
				}
			}

			return result;
		}

		#endregion

		#region GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(Order orderBO)
		{
			var result = ZString.Empty;
			if (orderBO != null)
			{
				if (orderBO.JD_IsCancelled)
				{
					result = Res.GetString("52692960-56bc-4989-b5d3-94c47a513e8d", "Order '{0}' matched, but it is inactive. XML has not been imported as a duplicate exists.", orderBO.JD_OrderNumberAndSplit);
				}
				else if (Parent != null)
				{
					if (IsAttachingOrderToShipment && orderBO.IsShipmentAttached)
					{
						result = orderBO.Shipment == Parent
										? GetErrorMessageIfOrderIsAttachedToShipmentAndCantBeUpdated(orderBO)
										: Res.GetString("bc52bfec-acbf-4678-98a1-54cea56c0ebb", "Order '{0}' is attached to another Shipment. Order ignored.", orderBO.JD_OrderNumberAndSplit);
					}
					else if (IsAttachingOrderToDeclaration && orderBO.IsDeclarationAttached)
					{
						result = GetErrorMessageIfOrderIsAttachedToDeclarationAndCantBeUpdated(orderBO);
					}
				}

				if (result.IsEmpty)
				{
					result = EnsureUniqueIndexDoesNotConflict(orderBO);
				}
			}

			if (orderBO == null && IsOrderHasShipmentOrDeclarationAsParentAndGivenNoOrderNumberSplit(dataObject.Order))
			{
				result = Res.GetString("7c29db33-f52c-4066-b448-aeaf33d71541", "The order has been attach to Shipment or Declaration but it is missing '{0}'.", "OrderNumberSplit");
			}

			return !result.IsEmpty
				? result
				: base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(orderBO);
		}

		protected override LogType LogTypeForReasonNotAbleToUpdate
		{
			get { return IsOrderHasShipmentOrDeclarationAsParentAndGivenNoOrderNumberSplit(dataObject.Order) ? LogType.Error : LogType.Warning; }
		}

		internal bool IsOrderHasShipmentOrDeclarationAsParentAndGivenNoOrderNumberSplit(UniversalShipmentOrder givenOrder) => givenOrder != null && !givenOrder.OrderNumberSplit.HasValue && Parent != null;

		#endregion

		#endregion

		#region Matching Fallbacks

		protected override IMatchingBusinessEntityFinder<Order> GetCombinedReferenceMatcher()
		{
			var references = Parent == null ? GetMatchingReferences() : new OrderReferences();
			return new OrderMatcher(factory.BOFactory, references, logger);
		}

		OrderReferences GetMatchingReferences()
		{
			var references = new OrderReferences();
			references.BuyerPK = BuyerPK;
			references.OriginUNLOCO = dataObject.PortOfOrigin.GetUNLOCOAsUpperCase(factory.BOFactory);
			references.DestinationUNLOCO = dataObject.PortOfDestination.GetUNLOCOAsUpperCase(factory.BOFactory);
			references.OrderNumber = dataObject.Order != null ? dataObject.Order.OrderNumber.GetValueOrDefault() : ZString.Empty;
			references.OrderNumberSplit = dataObject.Order != null ? dataObject.Order.OrderNumberSplit : null;

			references.ShippersReference = dataObject.BookingConfirmationReference.GetValueOrDefault();
			references.InvoiceNumber = dataObject.CommercialInfo != null
				&& dataObject.CommercialInfo.CommercialInvoiceCollection != null
				&& dataObject.CommercialInfo.CommercialInvoiceCollection.Count == 1
					? dataObject.CommercialInfo.CommercialInvoiceCollection[0].InvoiceNumber.GetValueOrDefault()
					: ZString.Empty;

			PopulateWaybill(references);

			return references;
		}

		void PopulateWaybill(OrderReferences references)
		{
			var wayBillNumber = dataObject.WayBillNumber.GetValueOrDefault();
			if (dataObject.TransportMode.GetCodeAsUpperCase() == Constants.TransportModes.Air)
			{
				references.HAWBNumber = wayBillNumber;
			}
			else
			{
				references.HBOLNumber = wayBillNumber;
			}
		}

		#region BuyerPK

		ZGuid BuyerPK => ConsigneeAddress?.OA_OH ?? ZGuid.Missing;

		OrgAddress ConsigneeAddress
		{
			get
			{
				if (consigneeAddress == null)
				{
					consigneeAddress = dataObject.GetMatchedOrgAddress(DocAddressType.BuyerDocumentaryAddress, logger, factory) ?? dataObject.GetMatchedOrgAddress(DocAddressType.ConsigneeDocumentaryAddress, logger, factory);
				}

				return consigneeAddress;
			}
		}

		OrgAddress consigneeAddress;

		#endregion

		#endregion

		#region PopulateBusinessObject

		bool IsAttachedToShipmentOrDeclaration(Order order)
		{
			return order.IsShipmentAttached || order.IsDeclarationAttached;
		}

		protected override void PopulateBusinessObject(Order orderBO)
		{
			if (!IsNewBO && Orders != null
				&& Orders.Any(order => !IsAttachedToShipmentOrDeclaration(order))
				&& dataObject.Order != null
				&& !dataObject.Order.OrderNumberSplit.HasValue
				&& dataObject.Order.OrderLineCollection != null
				&& dataObject.Order.OrderLineCollection.Count > 0)
			{
				var lineOrderToUpdate = Orders.Where(order => !IsAttachedToShipmentOrDeclaration(order))
																.OrderByDescending(order => order.JD_OrderNumberSplit).First();

				var linesToUpdateForOrder = getOrderLinesRequiringUpdate();

				dataObject.Order.OrderLineCollection.Clear();
				dataObject.Order.OrderLineCollection.AddRange(linesToUpdateForOrder);

				RejectOrderIfAttachedToShipmentOrDeclaration(lineOrderToUpdate);
				PopulateBO(lineOrderToUpdate, false);
			}
			else
			{
				RejectOrderIfAttachedToShipmentOrDeclaration(orderBO);
				PopulateBO(orderBO, false);
			}

			orderBO?.LogEventForMismatchedShipWindow();
		}

		List<UniversalOrderLine> getOrderLinesRequiringUpdate()
		{
			var linesToUpdateForOrder = new List<UniversalOrderLine>();

			foreach (var line in dataObject.Order.OrderLineCollection)
			{
				var newLine = line.Clone() as UniversalOrderLine;
				if (line != null && line.Status.GetCodeAsUpperCase() == Constants.OrderStatus.Cancelled)
				{
					logErrorsForNotification(newLine);
				}
				else if (line != null && line.Status.GetCodeAsUpperCase() != Constants.OrderStatus.Cancelled && line.LineNumber.HasValue)
				{
					RejectOrderIfNewQuantityIsLessThanTotalQuantities(newLine);
				}
				linesToUpdateForOrder.Add(newLine);
			}
			return linesToUpdateForOrder;
		}

		void RejectOrderIfNewQuantityIsLessThanTotalQuantities(UniversalOrderLine line)
		{
			var sum = Orders.Where(order => IsAttachedToShipmentOrDeclaration(order))
				.Sum(order => order.OrderLines.Where(orderLine => orderLine.JO_LineNo == line.LineNumber).Sum(orderLine => orderLine.JO_QtyReceived));

			if (line.OrderedQty < sum)
			{
				var errorMessage = Res.GetString("7D1F87AB-7F99-415E-8E06-577E2D7449C0",
					"Cannot update Order '{0}'{1} as quantity is less than total quantities attached to shipment(s) and/or declaration(s).",
						Orders.First().JD_OrderNumber,
						line.LineNumber.HasValue ? string.Format(CultureInfo.InvariantCulture, (NoResString)" - Line {0}", line.LineNumber.GetValueOrDefault()) : string.Empty);
				throw new DataObjectReadFailureException(errorMessage);
			}

			line.OrderedQty = line.OrderedQty - sum;
		}

		void logErrorsForNotification(UniversalOrderLine line)
		{
			var ordersForErrorNotification = Orders.Where(order => IsAttachedToShipmentOrDeclaration(order)
																										&& order.OrderLines.Any(orderLine => orderLine.JO_LineNo == line.LineNumber.GetValueOrDefault()))
																										.OrderBy(order => order.JD_OrderNumberSplit);

			foreach (var order in ordersForErrorNotification)
			{
				logger.LogBoth(LogType.Error, Res.GetString("c535df19-8303-4882-9d6d-ba472436b0a3", "Cannot update Order '{0}'{1} as it is already attached to {2}.",
						order.JD_OrderNumberAndSplit,
						line.LineNumber.HasValue ? string.Format(CultureInfo.InvariantCulture, (NoResString)" - Line {0}", line.LineNumber.GetValueOrDefault()) : string.Empty,
						order.IsShipmentAttached ?
							string.Format(CultureInfo.InvariantCulture, (NoResString)"Shipment '{0}'", (order.Shipment.JS_UniqueConsignRef)) :
							string.Format(CultureInfo.InvariantCulture, (NoResString)"Declaration '{0}'", ((Enterprise.Integration.Customs.IBaseJobDeclaration)order.Declaration).JE_DeclarationReference)));
			}
		}

		public void PopulateBO(Order orderBO, bool shouldIgnoreErrorOnBuyer)
		{
			if (orderBO != null)
			{
				ISupportDataImporting iSupportDataImporting = orderBO;

				using (new DisposableAction(
					() => iSupportDataImporting.IsImportingData = true,
					() => iSupportDataImporting.IsImportingData = false))
				{
					PopulateBuyerSupplierAndRejectOrderIfNoBuyerPresent(orderBO, shouldIgnoreErrorOnBuyer);
					PopulateBusinessObjectCore(orderBO);
					PopulateCustomFields(orderBO);
					PopulateRelatedEntities(orderBO);

					OrderImportValidationHelper.ValidateOrder(logger, orderBO);
				}
			}
		}

		string EnsureUniqueIndexDoesNotConflict(Order orderBO)
		{
			var message = string.Empty;

			if (dataObject.Order != null)
			{
				var orderNumber = dataObject.Order.OrderNumber ?? orderBO.JD_OrderNumber;
				var orderNumberSplit = dataObject.Order.OrderNumberSplit ?? orderBO.JD_OrderNumberSplit;
				var buyerAddress = ConsigneeAddress?.PK ?? orderBO.JD_OA_BuyerAddress;

				var query = new ZQuery();
				query.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
				query.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, orderNumberSplit);
				query.AddToFilter(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyerAddress);
				query.AddToFilter(JobOrderHeaderSchema.PK, SQLComparisonOperator.NotEqual, orderBO.PK);

				if (factory.BOFactory.Exists(typeof(Order), query))
				{
					if (!orderBO.IsInDatabase)
					{
						message = Res.GetString(
							"5aedcae0-4e20-45ca-8433-26aadebdba90",
							"Cannot create Order as an Order with Order Number '{0}' and Order Number Split '{1}' already exists for the Buyer Address in the import file");
					}
					else if (orderBO.JD_OA_BuyerAddress != buyerAddress)
					{
						message = Res.GetString(
							"2d41c467-798f-4341-b3b4-f9573a7a01d3",
							"Cannot update Buyer Address as an Order with Order Number '{0}' and Order Number Split '{1}' already exists for the Buyer Address in the import file");
					}
					else if (orderBO.JD_OrderNumber != orderNumber)
					{
						message = Res.GetString(
							"cbc2987e-a3de-42f1-ad57-a1a822f66fd5",
							"Cannot update Order Number as an Order with Order Number '{0}' and Order Number Split '{1}' already exists for the Buyer Address in the import file");
					}
					else if (orderBO.JD_OrderNumberSplit != orderNumberSplit)
					{
						message = Res.GetString(
							"6ca23715-756a-4f52-86d5-45e562ba7bb2",
							"Cannot update Order Number Split as an Order with Order Number '{0}' and Order Number Split '{1}' already exists for the Buyer Address in the import file");
					}

					if (!string.IsNullOrEmpty(message))
					{
						message = string.Format(message, orderNumber, orderNumberSplit);
					}
				}
			}

			return message;
		}

		#region RejectOrderIfAttachedToShipmentOrDeclarationOrActiveSupplierBooking

		void RejectOrderIfAttachedToShipmentOrDeclaration(Order orderBO)
		{
			if (!IsNewBO)
			{
				if (Parent == null)
				{
					var errorMessage = string.Empty;

					if (orderBO.IsShipmentAttached)
					{
						errorMessage = GetErrorMessageIfOrderIsAttachedToShipmentAndCantBeUpdated(orderBO);
					}
					else if (orderBO.IsDeclarationAttached)
					{
						errorMessage = GetErrorMessageIfOrderIsAttachedToDeclarationAndCantBeUpdated(orderBO);
					}

					if (!string.IsNullOrWhiteSpace(errorMessage))
					{
						throw new DataObjectReadFailureException(errorMessage);
					}
				}
			}
		}

		string GetErrorMessageIfOrderIsAttachedToShipmentAndCantBeUpdated(Order orderBO)
		{
			importErrorType = OrderImportErrorTypes.None;

			if (!orderBO.IsShipmentAttached || (orderBO.Shipment.JS_IsBooking && !orderBO.Shipment.JS_IsForwardRegistered))
			{
				return string.Empty;
			}

			string prefix;
			string surfix;

			if (Parent != null)
			{
				prefix = Res.GetString("6dfb1464-eba1-4f3b-afc5-69ac2340ec59", "Order '{0}'", orderBO.JD_OrderNumberAndSplit);
				surfix = Res.GetString("e00a2700-4343-4b2b-a5e8-3eb52ef36b1d", "and order is already attached to this Shipment");
			}
			else
			{
				prefix = Res.GetString("ed17f9b6-8238-42dc-a0d7-08080fc0cd60", "Cannot update Order because: Order '{0}'", orderBO.JD_OrderNumberAndSplit);
				surfix = Res.GetString("9cb6bd0b-460e-4df3-b3c3-b918e499d67e", "and order is already attached to Shipment '{0}'", orderBO.Shipment.JobNumber);
			}

			if ((orderBO.Buyer?.MiscServ?.OM_IMAllowAttachedOrderXMLUpdate).GetValueOrDefault())
			{
				if (orderBO.Shipment.JS_AttachedOrderXMLUpdateCutOffDateUtc.IsValid)
				{
					if (orderBO.Shipment.JS_AttachedOrderXMLUpdateCutOffDateUtc > ZDateTime.UtcNow)
					{
						return string.Empty;
					}
					else
					{
						importErrorType = OrderImportErrorTypes.CutOffDateHasPassed;
						return Res.GetString("a596593c-1b51-46eb-bd5c-7a20226609f1", "{0} cut-off date has passed {1}.", prefix, surfix);
					}
				}
				else
				{
					importErrorType = OrderImportErrorTypes.CutOffDateNotSet;
					return Res.GetString("e52b3865-427e-4ec2-8321-44d70d3c9f2d", "{0} cut-off date is not set {1}.", prefix, surfix);
				}
			}
			else
			{
				importErrorType = OrderImportErrorTypes.AlreadyAttachedToShipment;
				return Res.GetString("481e7873-9101-4f53-aa46-11dc27b7e872", "{0} is not allowed for Organization '{1}' {2}.", prefix, orderBO.Buyer?.OH_Code, surfix);
			}
		}

		string GetErrorMessageIfOrderIsAttachedToDeclarationAndCantBeUpdated(Order orderBO)
		{
			importErrorType = OrderImportErrorTypes.None;

			if (HasCommencedEvent(orderBO.Declaration))
			{
				var suffix = Parent != null
					? Res.GetString("94ef3fe4-f245-4cb9-927f-575b0bf2c3e1", "this Declaration")
					: Res.GetString("aae69664-acae-4cbd-9ae1-12b0e3b8d68d", "Declaration '{0}'", orderBO.Declaration[JobDeclarationSchema.JE_DeclarationReference]);

				importErrorType = OrderImportErrorTypes.DeclarationHasCommencedEvent;
				return Res.GetString("fd2c64ff-26a2-4b99-8962-4ad12592881d", "Cannot update Order {0} as a 'customs commenced' event exists on {1}.", orderBO.JD_OrderNumberAndSplit, suffix);
			}

			return string.Empty;
		}

		bool IsAttachingOrderToShipment => Parent != null && Parent.TableName == JobShipmentSchema.Constants.TableName;
		bool IsAttachingOrderToDeclaration => Parent != null && Parent.TableName == JobDeclarationSchema.Constants.TableName;

		bool HasCommencedEvent(BusinessObject declaration)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, declaration[JobDeclarationSchema.PK]);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { Events.CustomsCommenced.Code, Events.ExportCustomsCommenced.Code });
			return new BusinessObjectFactory().Exists(typeof(StmALog), query);
		}

		#endregion

		#region PopulateBuyerAndRejectOrderIfNoBuyerPresent

		void PopulateBuyerSupplierAndRejectOrderIfNoBuyerPresent(Order orderBO, bool shouldIgnoreErrorOnBuyer)
		{
			var addressDataObjectUsedByBuyer = GetAddressDataObjectUsedByBuyer();
			if (addressDataObjectUsedByBuyer != null)
			{
				var consigneeAddress = new OrganisationDataObjectReader(addressDataObjectUsedByBuyer, logger, factory).GetMatched(orderBO, OrganisationTypes.Consignee);
				if (consigneeAddress != null)
				{
					SetValue(orderBO, JobOrderHeaderSchema.JD_OA_BuyerAddress, consigneeAddress.PK);
				}
			}

			if (orderBO.Buyer == null && !shouldIgnoreErrorOnBuyer)
			{
				var errorMessage = new ZStringBuilder();
				errorMessage.Append(Res.GetString("16bcd779-6293-4753-9674-0c1dd29902e0", "Cannot Import Order"));

				if (addressDataObjectUsedByBuyer == null)
				{
					errorMessage.Append(Res.GetString("b09bd851-f298-4b06-8212-06473c46f7e3", "No Buyer Address was provided."));
				}
				else
				{
					addressDataObjectUsedByBuyer.AddAddressErrorMessage(Res.GetString("f30481e5-d62e-4f7a-abe3-6bbc1dd2808f", "Buyer"), errorMessage);
				}

				throw new DataObjectReadFailureException(errorMessage.ToStringWithNewLineBetweenAppends());
			}

			ReadInOrganisationAddress(dataObject.OrganizationAddressCollection, orderBO, DocAddressType.ConsignorDocumentaryAddress, OrganisationTypes.Consignor, JobOrderHeaderSchema.JD_OA_SupplierAddress);
		}

		OrganizationAddress GetAddressDataObjectUsedByBuyer()
		{
			return dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.BuyerDocumentaryAddress))
				?? dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsigneeDocumentaryAddress));
		}

		#endregion

		#region PopulateBusinessObjectCore

		void PopulateBusinessObjectCore(Order orderBO)
		{
			SetValue(orderBO, JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo, dataObject.PortOfDestination);
			SetValue(orderBO, JobOrderHeaderSchema.JD_TransportMode, dataObject.TransportMode);
			SetValue(orderBO, JobOrderHeaderSchema.JD_ContainerMode, dataObject.ContainerMode);
			SetValue(orderBO, JobOrderHeaderSchema.JD_ActualVolume, dataObject.TotalVolume);
			SetValue(orderBO, JobOrderHeaderSchema.JD_ActualWeight, dataObject.TotalWeight);
			SetValue(orderBO, JobOrderHeaderSchema.JD_AdditionalTerms, dataObject.AdditionalTerms);
			SetValue(orderBO, JobOrderHeaderSchema.JD_BookingConfRef, dataObject.BookingConfirmationReference);
			SetValue(orderBO, JobOrderHeaderSchema.JD_EstimatedExchangeRate, dataObject.FreightRate);
			SetValue(orderBO, JobOrderHeaderSchema.JD_F3_NKPackType, dataObject.OuterPacksPackageType);
			SetValue(orderBO, JobOrderHeaderSchema.JD_FirstBuyerContact, dataObject.FirstBuyerContact);
			SetValue(orderBO, JobOrderHeaderSchema.JD_IncoTerm, dataObject.ShipmentIncoTerm);

			if (dataObject.CommercialInfo != null
				&& dataObject.CommercialInfo.CommercialInvoiceCollection != null
				&& dataObject.CommercialInfo.CommercialInvoiceCollection.Count == 1)
			{
				var commercialInvoice = dataObject.CommercialInfo.CommercialInvoiceCollection[0];
				SetValue(orderBO, JobOrderHeaderSchema.JD_InvoiceDate, commercialInvoice.InvoiceDate);
				SetValue(orderBO, JobOrderHeaderSchema.JD_InvoiceNumber, commercialInvoice.InvoiceNumber);
			}

			SetValue(orderBO, JobOrderHeaderSchema.JD_OrderGoodsDescription, dataObject.GoodsDescription);

			if (dataObject.Order != null)
			{
				SetValue(orderBO, JobOrderHeaderSchema.JD_OrderNumber, dataObject.Order.OrderNumber);
				SetValue(orderBO, JobOrderHeaderSchema.JD_OrderNumberSplit, dataObject.Order.OrderNumberSplit);
				SetValue(orderBO, JobOrderHeaderSchema.JD_OrderStatus, dataObject.Order.Status);
				SetValue(orderBO, JobOrderHeaderSchema.JD_IsReleased, dataObject.Order.IsReleased);
			}

			SetValue(orderBO, JobOrderHeaderSchema.JD_Packs, dataObject.OuterPacks);
			SetValue(orderBO, JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt, dataObject.PortOfOrigin);
			SetValue(orderBO, JobOrderHeaderSchema.JD_RL_NKPortOfDischarge, dataObject.PortOfDischarge);
			SetValue(orderBO, JobOrderHeaderSchema.JD_RL_NKPortOfLoading, dataObject.PortOfLoading);
			SetValue(orderBO, JobOrderHeaderSchema.JD_RN_NKCountryOfSupply, dataObject.CountryOfSupply);
			SetValue(orderBO, JobOrderHeaderSchema.JD_RS_NKServiceLevel_NI, dataObject.ServiceLevel);
			SetValue(orderBO, JobOrderHeaderSchema.JD_RX_NKOrderCurrency, dataObject.FreightRateCurrency);
			SetValue(orderBO, JobOrderHeaderSchema.JD_SecondBuyerContact, dataObject.SecondBuyerContact);
			SetValue(orderBO, JobOrderHeaderSchema.JD_UnitOfVolume, dataObject.TotalVolumeUnit);
			SetValue(orderBO, JobOrderHeaderSchema.JD_UnitOfWeight, dataObject.TotalWeightUnit);
			SetValue(orderBO, JobOrderHeaderSchema.JD_Waybill, dataObject.WayBillNumber);

			if (dataObject.AdditionalBillCollection != null && dataObject.AdditionalBillCollection.Count > 0)
			{
				var additionalBill = dataObject.AdditionalBillCollection.Find(bill => bill.BillType?.Code.HasValue == true && bill.BillType.Code.Value == WayBillTypeList.Codes.Master);
				if (additionalBill != null)
				{
					SetValue(orderBO, JobOrderHeaderSchema.JD_MasterWaybill, additionalBill.BillNumber);
				}
			}
		}

		#endregion

		#region PopulateCustomFields

		void PopulateCustomFields(Order orderBO)
		{
			var usedCustomField = new CustomLabelsCustomizedFieldDataObjectReader(logger).PopulateCustomFields(JobOrderHeaderSchema.Instance, orderBO, dataObject, new Order.CustomLabelsProvider(orderBO, false));
			PopulateWorkflowCustomFields(orderBO, dataObject, usedCustomField);
		}

		#endregion

		#region PopulateRelatedEntities

		void PopulateRelatedEntities(Order orderBO)
		{
			PopulateDates(orderBO);
			PopulateLegs(orderBO);
			PopulateNotes(orderBO);
			PopulateOrganisations(orderBO);
			PopulateContainers(orderBO);
			PopulateOrderLines(orderBO);
		}

		#region PopulateDates

		void PopulateDates(Order orderBO)
		{
			if (dataObject.LocalProcessing != null)
			{
				SetValue(orderBO, JobOrderHeaderSchema.JD_DeliveryRequiredBy, dataObject.LocalProcessing.DeliveryRequiredBy);
			}

			if (dataObject.DateCollection != null)
			{
				foreach (var dateDataObject in dataObject.DateCollection)
				{
					switch (dateDataObject.Type)
					{
						case DateType.BookingConfirmed:
							SetValue(orderBO, JobOrderHeaderSchema.JD_BookingConfDate, dateDataObject.Value);
							break;
						case DateType.DepartureVesselCutoffDate:
							SetValue(orderBO, JobOrderHeaderSchema.JD_DepartureVesselCutoffDate, dateDataObject.Value);
							break;
						case DateType.ExWorksRequiredBy:
							SetValue(orderBO, JobOrderHeaderSchema.JD_ExWorksRequiredBy, dateDataObject.Value);
							break;
						case DateType.FollowUp:
							SetValue(orderBO, JobOrderHeaderSchema.JD_FollowUpDate, dateDataObject.Value);
							break;
						case DateType.OrderDate:
							SetValue(orderBO, JobOrderHeaderSchema.JD_OrderDate, dateDataObject.Value);
							break;
						case DateType.ShipmentWindowStart:
							SetValue(orderBO, JobOrderHeaderSchema.JD_ShipmentWindowStart, dateDataObject.Value);
							break;
						case DateType.ShipmentWindowEnd:
							SetValue(orderBO, JobOrderHeaderSchema.JD_ShipmentWindowEnd, dateDataObject.Value);
							break;
					}
				}
			}
		}

		#endregion

		#region PopulateLegs

		void PopulateLegs(Order orderBO)
		{
			if (dataObject.TransportLegCollection != null && dataObject.TransportLegCollection.Count > 0)
			{
				var legs = dataObject.TransportLegCollection.OrderBy(leg => leg.LegOrder.GetValueOrDefault()).ToArray();
				var count = legs.Length;

				var departureLeg = legs[0];
				SetValue(orderBO, JobOrderHeaderSchema.JD_DepartureVoyage, departureLeg.VoyageFlightNo);
				SetValue(orderBO, JobOrderHeaderSchema.JD_RV_NKDepartureVessel, departureLeg.VesselName);
				orderBO.JD_Milestone_E_DEP = departureLeg.EstimatedDeparture ?? ZDateTime.Empty;

				if (count == 1)
				{
					SetValue(orderBO, JobOrderHeaderSchema.JD_ArrivalVoyage, departureLeg.VoyageFlightNo);
					SetValue(orderBO, JobOrderHeaderSchema.JD_RV_NKArrivalVessel, departureLeg.VesselName);
					orderBO.JD_Milestone_E_ARV = departureLeg.EstimatedArrival ?? ZDateTime.Empty;

					SetValue(orderBO, JobOrderHeaderSchema.JD_E_ARV_1stIntermediate, ZDateTime.Empty);
					SetValue(orderBO, JobOrderHeaderSchema.JD_E_DEP_3, ZDateTime.Empty);

					ClearIntermediateVoyage(orderBO);
				}
				else
				{
					SetValue(orderBO, JobOrderHeaderSchema.JD_E_ARV_1stIntermediate, departureLeg.EstimatedArrival);

					var arrivalLeg = legs[count - 1];
					SetValue(orderBO, JobOrderHeaderSchema.JD_ArrivalVoyage, arrivalLeg.VoyageFlightNo);
					SetValue(orderBO, JobOrderHeaderSchema.JD_RV_NKArrivalVessel, arrivalLeg.VesselName);
					SetValue(orderBO, JobOrderHeaderSchema.JD_E_DEP_3, arrivalLeg.EstimatedDeparture);
					orderBO.JD_Milestone_E_ARV = arrivalLeg.EstimatedArrival ?? ZDateTime.Empty;

					if (count > 2)
					{
						var intermediateLeg = legs[1];
						SetValue(orderBO, JobOrderHeaderSchema.JD_IntermediateVoyage, intermediateLeg.VoyageFlightNo);
						SetValue(orderBO, JobOrderHeaderSchema.JD_RV_NKIntermediateVessel, intermediateLeg.VesselName);
						SetValue(orderBO, JobOrderHeaderSchema.JD_E_DEP_2, intermediateLeg.EstimatedDeparture);
						SetValue(orderBO, JobOrderHeaderSchema.JD_E_ARV_2ndIntermediate, intermediateLeg.EstimatedArrival);
					}
					else
					{
						ClearIntermediateVoyage(orderBO);
					}
				}
			}
		}

		void ClearIntermediateVoyage(Order orderBO)
		{
			SetValue(orderBO, JobOrderHeaderSchema.JD_IntermediateVoyage, ZString.Empty);
			SetValue(orderBO, JobOrderHeaderSchema.JD_RV_NKIntermediateVessel, ZString.Empty);
			SetValue(orderBO, JobOrderHeaderSchema.JD_E_DEP_2, ZDateTime.Empty);
			SetValue(orderBO, JobOrderHeaderSchema.JD_E_ARV_2ndIntermediate, ZDateTime.Empty);
		}

		#endregion

		#region PopulateNotes

		void PopulateNotes(Order orderBO)
		{
			if (dataObject.NoteCollection != null)
			{
				var noteTypesToSkip = new List<ZString> { PredefinedNoteTypes.Instance.OrderUpdateHistory.Code };

				var noteTypesWithEmptyText = dataObject.NoteCollection
					.Where(n => n.NoteText.GetValueOrDefault().Trim().IsEmpty)
					.Select(n => n.Description.GetValueOrDefault()).ToArray();
				if (noteTypesWithEmptyText.Any())
				{
					var message = Res.GetString("73372d29-751e-47fe-8a10-22fcdd7caa4d",
						"Cannot import the following notes with empty text:{0}{1}",
						System.Environment.NewLine,
						ZString.Join(",", noteTypesWithEmptyText));
					logger.LogBoth(LogType.Warning, message);

					noteTypesToSkip.AddRange(noteTypesWithEmptyText);
				}

				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, orderBO, noteTypesToSkip).ReadIntoCollection();
			}
		}

		#endregion

		#region PopulateOrganisations

		void PopulateOrganisations(Order orderBO)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var organisations = new List<OrganizationAddress>(dataObject.OrganizationAddressCollection);
				ReadInOrganisation(organisations, orderBO, DocAddressType.Carrier, OrganisationTypes.Carrier, JobOrderHeaderSchema.JD_OH_Carrier);

				ReadInGoodsAvailableAt(organisations, orderBO);
				ReadInGoodsDeliveredTo(organisations, orderBO);

				ReadInOrganisation(organisations, orderBO, DocAddressType.SendingForwarderAddress, OrganisationTypes.Forwarder, JobOrderHeaderSchema.JD_OH_SendingAgent);
				ReadInOrganisation(organisations, orderBO, DocAddressType.ReceivingForwarderAddress, OrganisationTypes.Forwarder, JobOrderHeaderSchema.JD_OH_ReceivingAgent);
				ReadInControllingCustomer(organisations, orderBO);

				var addressTypeUsedByBuyer = GetAddressDataObjectUsedByBuyer()?.AddressType ?? nameof(DocAddressType.ConsigneeDocumentaryAddress);
				foreach (var organisationDataObject in organisations)
				{
					if (!organisationDataObject.AddressType.GetValueOrDefault().EqualsIgnoringCase(addressTypeUsedByBuyer))
					{
						new OrganisationDataObjectReader(organisationDataObject, logger, factory).GetMatchedOrNew(orderBO);
					}
				}
			}
		}

		void ReadInOrganisation(List<OrganizationAddress> organisations, Order orderBO, DocAddressType addressType, OrganisationTypes organisationType, SchemaGuidColumn orgColumn)
		{
			var orgAddress = GetRelatedAddress(organisations, orderBO, addressType, organisationType);
			if (orgAddress != null)
			{
				SetValue(orderBO, orgColumn, orgAddress.OA_OH);
			}
		}

		OrgAddress GetRelatedAddress(List<OrganizationAddress> organisations, Order orderBO, DocAddressType addressType, OrganisationTypes organisationType)
		{
			OrgAddress result = null;

			var addressDataObject = organisations.FirstOrDefault(addressType.ToString());
			if (addressDataObject != null)
			{
				result = new OrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched(orderBO, organisationType);
				organisations.Remove(addressDataObject);
			}

			return result;
		}

		void ReadInControllingCustomer(List<OrganizationAddress> organisations, Order orderBO)
		{
			var controllingCustomer = organisations.FirstOrDefault(nameof(DocAddressType.ControllingCustomer));
			var legacyControllingParty = organisations.FirstOrDefault(LegacyUniversalAddressTypes.LegacyOrderControllingPartyAddressType);
			var controllingCustomerToImport = controllingCustomer ?? legacyControllingParty;

			if (controllingCustomerToImport != null)
			{
				new OrganisationDataObjectReader(controllingCustomerToImport, logger, factory).GetMatchedOrNew(orderBO, OrganisationTypes.ControllingCustomer, null, DocAddressType.ControllingCustomer);

				if (controllingCustomerToImport != null)
				{
					organisations.Remove(controllingCustomerToImport);
				}

				if (legacyControllingParty != null)
				{
					organisations.Remove(legacyControllingParty);
				}
			}
		}

		void ReadInGoodsAvailableAt(List<OrganizationAddress> organisations, Order orderBO)
		{
			var goodsAvailableAt = organisations.FirstOrDefault(nameof(DocAddressType.ConsignorPickupDeliveryAddress));
			if (goodsAvailableAt != null)
			{
				new OrganisationDataObjectReader(goodsAvailableAt, logger, factory).GetMatchedOrNew(orderBO, DocAddressType.GoodsAvailableAt);
				organisations.Remove(goodsAvailableAt);
			}
		}

		void ReadInGoodsDeliveredTo(List<OrganizationAddress> organisations, Order orderBO)
		{
			var goodsDeliveredTo = organisations.FirstOrDefault(nameof(DocAddressType.ConsigneePickupDeliveryAddress));
			if (goodsDeliveredTo != null)
			{
				new OrganisationDataObjectReader(goodsDeliveredTo, logger, factory).GetMatchedOrNew(orderBO, DocAddressType.GoodsDeliveredTo);
				organisations.Remove(goodsDeliveredTo);
			}
		}

		#endregion

		#region PopulateOrganisationAddresses

		void ReadInOrganisationAddress(List<OrganizationAddress> organisations, Order orderBO, DocAddressType addressType, OrganisationTypes organisationType, SchemaGuidColumn orgAddressColumn)
		{
			var orgAddress = GetRelatedAddress(organisations, orderBO, addressType, organisationType);
			if (orgAddress != null)
			{
				SetValue(orderBO, orgAddressColumn, orgAddress.PK);
			}
		}

		#endregion

		#region PopulateContainers

		void PopulateContainers(Order orderBO)
		{
			if (dataObject.ContainerCollection != null)
			{
				var reader = new ContainerDataObjectCollectionReader(this, orderBO, dataObject.ContainerCollection);
				reader.ReadIntoCollection();
			}
		}

		class ContainerDataObjectCollectionReader : DataObjectCollectionReader<Container, OrderContainer>
		{
			internal ContainerDataObjectCollectionReader(OrderDataObjectReader reader, Order order, DataObjectList<Container> containerDataObjects)
				: base(containerDataObjects)
			{
				Reader = reader;
				Order = order;
			}

			readonly OrderDataObjectReader Reader;
			readonly Order Order;

			protected override void AddToCollection(OrderContainer container)
			{
				Order.PlannedContainers.Add(container);
			}

			protected override OrderContainer[] BusinessObjects
			{
				get { return Order.PlannedContainers.ToArray<OrderContainer>(); }
			}

			protected override OrderContainer FindMatchingBusinessObject(Container containerDataObject)
			{
				return null;
			}

			protected override OrderContainer ReadIntoBusinessObject(Container containerDataObject, OrderContainer orderContainer)
			{
				return new OrderContainerDataObjectReader<OrderContainer>(containerDataObject, Reader.logger, Reader.factory).ReadIntoBusinessObject();
			}

			protected override void RemoveFromCollection(OrderContainer orderContainer)
			{
				Order.PlannedContainers.RemoveAndDelete(orderContainer);
			}
		}

		#endregion

		#region PopulateOrderLines

		void PopulateOrderLines(Order orderBO)
		{
			if (dataObject.Order != null && dataObject.Order.OrderLineCollection != null)
			{
				var reader = new OrderLineDataObjectCollectionReader(this, orderLineLinkManager, orderBO, dataObject.Order.OrderLineCollection);
				reader.ReadIntoCollection();
			}
		}

		class OrderLineDataObjectCollectionReader : DataObjectCollectionReader<UniversalOrderLine, OrderLine>
		{
			public OrderLineDataObjectCollectionReader(OrderDataObjectReader reader, IOrderLineLinkManager orderLineLinkManager, Order order, DataObjectList<UniversalOrderLine> orderLineDataObjects)
				: base(orderLineDataObjects)
			{
				Reader = reader;
				Order = order;
				this.orderLineLinkManager = orderLineLinkManager;
			}

			readonly OrderDataObjectReader Reader;
			readonly Order Order;
			readonly IOrderLineLinkManager orderLineLinkManager;

			protected override void AddToCollection(OrderLine orderLine)
			{
				Order.OrderLines.Add(orderLine);
			}

			protected override OrderLine[] BusinessObjects
			{
				get { return Order.OrderLines.ToArray(); }
			}

			protected override OrderLine FindMatchingBusinessObject(UniversalOrderLine orderLineDataObject)
			{
				return null;
			}

			protected override OrderLine ReadIntoBusinessObject(UniversalOrderLine orderLineDataObject, OrderLine orderLine)
			{
				var result = new OrderLineDataObjectReader(orderLineDataObject, Reader.logger, Reader.factory, Order).ReadIntoBusinessObject();

				if (result != null && orderLineLinkManager != null)
				{
					orderLineLinkManager.AllocatePackedItemLink(result, orderLineDataObject);
				}

				return result;
			}

			protected override void RemoveFromCollection(OrderLine orderLine)
			{
				CheckOrderLineIsNotInUse(orderLine);
				Order.OrderLines.Delete(orderLine);
			}

			void CheckOrderLineIsNotInUse(OrderLine orderLine)
			{
				var factory = orderLine.Factory;
				var invoiceLines = factory.Load<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_JO, orderLine.PK));
				if (invoiceLines.Any())
				{
					var invoiceHeaderPKs = invoiceLines.Select(x => x.JI_JZ).ToArray();
					var invoiceHeaders = factory.Load<Enterprise.Integration.Customs.Shared.ICommonJobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.PK, invoiceHeaderPKs));
					var declarationPKs = invoiceHeaders.Select(x => x.JZ_JE).ToArray();
					var declaration = (BusinessObject)factory.LoadTop1<Enterprise.Integration.Customs.IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declarationPKs));

					string suffix = string.Empty;
					if (declaration != null)
					{
						suffix = Res.GetString("788EED5D-C337-4612-A528-10D9FA8E671D", "on Declaration '{0}'", declaration[JobDeclarationSchema.JE_DeclarationReference]);
					}
					else
					{
						var invoiceHeader = (BusinessObject)invoiceHeaders.FirstOrDefault();
						if (invoiceHeader != null)
						{
							suffix = Res.GetString("2CBABC8B-C4FC-4371-83F0-74BDFDE390C9", "on Invoice '{0}'", invoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceNumber]);
						}
					}

					var errorMessage = Res.GetString("D11EB7ED-A5BF-4ACA-B4D3-C89FCDF50D0F", "Cannot update Order {0}.  Cannot delete Order Line {1} as it is attached to an Invoice Line {2}", Order.JD_OrderNumberAndSplit, orderLine.JO_LineNo, suffix);
					throw new DataObjectReadFailureException(errorMessage);
				}
			}
		}

		#endregion

		#endregion

		#endregion

		#region Order Import Error Types

		public OrderImportErrorTypes ImportErrorType => importErrorType;
		OrderImportErrorTypes importErrorType;

		#endregion
	}
}
