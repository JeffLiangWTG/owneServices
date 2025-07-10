using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderManagerOrderDataContextManager : ShipmentDataContextManager<Order>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.OrderManagerOrder; }
		}

		public override ZString DataContextKey
		{
			get
			{
				var orderNumber = ParentBO.JD_OrderNumber.EscapeTildas();
				var buyerCode = ZString.Empty;
				var buyer = ParentBO.Buyer;
				if (buyer != null)
				{
					buyerCode = buyer.OH_Code.EscapeTildas();
				}

				return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}", orderNumber, ParentBO.JD_OrderNumberSplit, buyerCode);
			}
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var orderEventContextReader = new OrderEventContextReader(ParentBO);
				orderEventContextReader.AddOrderContextValues(result);
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new OrderEventParentFinder(factory, this, logger);
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.OrderExportDirectory.Value; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZDBOnlyQuery query = null;

			var dataContextKey = matchingValues.Key;
			if (!dataContextKey.IsEmpty)
			{
				var contextKey = dataContextKey.SplitIgnoringEscapedDelimiter('~', '!');
				if (contextKey.Length == 3)
				{
					ZByte orderNumberSplit = ZByte.Zero;
					bool foundOrderNumberSplit = ZByte.TryParse(contextKey[1], out orderNumberSplit);

					if (foundOrderNumberSplit || contextKey[1].ToUpper() == FindAllSplitsIndicator)
					{
						var ownerOrganisationCode = matchingValues.OwnerOrganisationCode;
						var buyerCode = ownerOrganisationCode.IsEmpty ? contextKey[2].UnEscapeTildas() : ownerOrganisationCode;
						var orderNumber = contextKey[0].UnEscapeTildas();

						var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobOrderHeaderSchema.JD_OA_BuyerAddress);
						var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
						orgSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, buyerCode);
						addressSubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

						query = new ZDBOnlyQuery(typeof(Order));
						query.AddToFilter(JobOrderHeaderSchema.JD_IsCancelled, false);
						query.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
						if (foundOrderNumberSplit)
						{
							query.AddToFilter(JobOrderHeaderSchema.JD_OrderNumberSplit, orderNumberSplit);
						}

						query.AddSubQuery(addressSubQuery, JoinCondition.And);
					}
				}
			}

			return query;
		}

		const string FindAllSplitsIndicator = "ALL";

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new OrderDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new OrderDataObjectWriter(writeManager);
		}
	}
}

