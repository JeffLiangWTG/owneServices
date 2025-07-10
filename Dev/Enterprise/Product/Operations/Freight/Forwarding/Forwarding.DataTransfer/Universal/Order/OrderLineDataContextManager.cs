using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderLineDataContextManager : ShipmentDataContextManager<OrderLine>
	{
		public override DataContextType DataContextType => DataContextType.OrderManagerOrderLine;

		public override ZString DataContextKey
		{
			get
			{
				var order = ParentBO.Order;
				if (order != null)
				{
					var orderNumber = order.JD_OrderNumber.EscapeTildas();
					var buyerCode = ZString.Empty;
					var buyer = order.Buyer;
					if (buyer != null)
					{
						buyerCode = buyer.OH_Code.EscapeTildas();
					}

					if (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.Value && !string.IsNullOrEmpty(ParentBO.JO_LineReference))
					{
						return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}",
							orderNumber, order.JD_OrderNumberSplit, buyerCode,
							ParentBO.JO_LineReference.EscapeTildas());
					}
					else
					{
						return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}",
						orderNumber, order.JD_OrderNumberSplit, buyerCode,
						ParentBO.JO_LineNo, ParentBO.JO_SubLineNo);
					}
				}

				return ZString.Empty;
			}
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var orderLineEventContextReader = new OrderLineEventContextReader(ParentBO);
				orderLineEventContextReader.AddOrderContextValues(result);
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new OrderLineEventParentFinder(factory, this, logger);
		}

		public override string DefaultOutputDirectory => SystemDataRegistry.Instance.OrderExportDirectory.Value;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZDBOnlyQuery query = null;

			var dataContextKey = matchingValues.Key;
			if (!dataContextKey.IsEmpty)
			{
				var contextKey = dataContextKey.SplitIgnoringEscapedDelimiter('~', '!');

				if (contextKey.Length >= 4)
				{
					bool foundOrderNumberSplit = ZByte.TryParse(contextKey[1], out var orderNumberSplit);

					if ((foundOrderNumberSplit || contextKey[1].ToUpper() == FindAllSplitsIndicator))
					{
						var ownerOrganisationCode = matchingValues.OwnerOrganisationCode;
						var buyerCode = ownerOrganisationCode.IsEmpty ? contextKey[2].UnEscapeTildas() : ownerOrganisationCode;
						var orderNumber = contextKey[0].UnEscapeTildas();

						var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobOrderHeaderSchema.JD_OA_BuyerAddress);
						var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
						orgSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, buyerCode);
						addressSubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

						var orderSubQuery = new ZDBOnlySubQuery(typeof(Orders.Business.Order), JobOrderLineSchema.JO_JD);
						orderSubQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);

						if (foundOrderNumberSplit)
						{
							orderSubQuery.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, orderNumberSplit);
						}

						orderSubQuery.AddSubQuery(addressSubQuery, JoinCondition.And);

						if (contextKey.Length == 5)
						{
							var foundLineNumber = int.TryParse(contextKey[3], out var lineNumber);
							var foundSubLineNumber = int.TryParse(contextKey[4], out var subLineNumber);
							if (foundLineNumber && foundSubLineNumber)
							{
								query = new ZDBOnlyQuery(typeof(OrderLine));
								query.AddToFilter(JobOrderLineSchema.JO_LineNo, lineNumber);
								query.AddToFilter(JobOrderLineSchema.JO_SubLineNo, subLineNumber);
								query.AddSubQuery(orderSubQuery, JoinCondition.And);
							}
						}
						else if (contextKey.Length == 4 && OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.Value)
						{
							var lineReference = contextKey[3].UnEscapeTildas();
							if (!lineReference.IsEmpty)
							{
								query = new ZDBOnlyQuery(typeof(OrderLine));
								query.AddToFilter(JobOrderLineSchema.JO_LineReference, lineReference);
								query.AddSubQuery(orderSubQuery, JoinCondition.And);
							}
						}
					}
				}
			}

			return query;
		}

		const string FindAllSplitsIndicator = "ALL";

		public override bool ManagesShipments => true;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new OrderLineTopLevelDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new OrderLineTopLevelDataObjectWriter(writeManager);
		}
	}
}
