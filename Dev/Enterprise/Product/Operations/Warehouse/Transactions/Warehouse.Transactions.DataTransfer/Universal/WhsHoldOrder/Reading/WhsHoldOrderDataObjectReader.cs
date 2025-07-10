using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsHoldOrderDataObjectReader : WhsDataObjectReader<WhsHoldOrder>
	{
		internal WhsHoldOrderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseHoldOrder; }
		}

		#endregion

		#region GetNewBusinessObject

		protected override WhsHoldOrder GetNewBusinessObject()
		{
			return new WhsHoldOrder(factory.BOFactory);
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsHoldOrder targetBO)
		{
			var warehousePK = WarehousePK;
			var clientAddress = ClientAddress;
			var clientOrg = factory.Load<OrgHeader>(clientAddress.GetValue(OrgAddressSchema.OA_OH)); // need Client OrgHeader BizO for Product Matcher on Lines Reader

			// Not using SetValue as WhsHoldOrder is non persistent and not backed by SchemaColumns
			targetBO.WarehousePK = warehousePK;
			targetBO.ClientPK = clientOrg != null ? clientOrg.PK : ZGuid.Empty;

			PopulateRelatedEntitites(targetBO, clientOrg);
			var finalised = targetBO.Finalise();

			if (!finalised)
			{
				throw new DataObjectReadFailureException(BuildFinaliseErrorMessage(targetBO));
			}
		}

		#region BuildFinaliseErrorMessage

		static string BuildFinaliseErrorMessage(WhsHoldOrder targetBO)
		{
			var errorBuilder = new ZStringBuilder(Res.GetString("2cfbc750-6c22-40c5-b538-6912a3cef0db", "Cannot import Hold Order:"));

			foreach (var line in targetBO.Lines.Where(l => l.HasRowErrors))
			{
				errorBuilder.AppendIfNotEmpty(Res.GetString("de64c443-bf02-4904-ba2a-b99801d8639e", "Line Error: "), line.RowErrors.ToStringContents(e => e.Message));
			}

			return errorBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region PopulateRelatedEntitites

		void PopulateRelatedEntitites(WhsHoldOrder targetBO, OrgHeader client)
		{
			var orderLineCollection = dataObject.Order != null ? dataObject.Order.OrderLineCollection : null;
			if (orderLineCollection != null)
			{
				var holdOrderLineCollectionReader = new WhsHoldOrderLinesDataObjectCollectionReader(targetBO, orderLineCollection.ToArray(), this, client);
				holdOrderLineCollectionReader.ReadIntoCollection();
			}
		}

		#endregion

		#endregion

		#region WhsHoldOrderLinesDataObjectCollectionReader

		class WhsHoldOrderLinesDataObjectCollectionReader : DataObjectCollectionReader<OrderLine, WhsHoldOrderLine>
		{
			internal WhsHoldOrderLinesDataObjectCollectionReader(WhsHoldOrder holdOrder, OrderLine[] orderLineDataObjects, WhsHoldOrderDataObjectReader reader, OrgHeader client)
				: base(orderLineDataObjects)
			{
				HoldOrder = Argument.NotNull(holdOrder, "holdOrder");
				Reader = Argument.NotNull(reader, "reader");
				Client = Argument.NotNull(client, "client");
			}

			readonly WhsHoldOrder HoldOrder;
			readonly WhsHoldOrderDataObjectReader Reader;
			readonly OrgHeader Client;

			protected override void AddToCollection(WhsHoldOrderLine holdOrderLine)
			{
				HoldOrder.Lines.Add(holdOrderLine);
			}

			protected override void RemoveFromCollection(WhsHoldOrderLine holdOrderLine)
			{
				HoldOrder.Lines.Remove(holdOrderLine);
			}

			protected override WhsHoldOrderLine[] BusinessObjects
			{
				get { return HoldOrder.Lines.Cast<WhsHoldOrderLine>().ToArray(); }
			}

			protected override WhsHoldOrderLine ReadIntoBusinessObject(OrderLine orderLineDataObject, WhsHoldOrderLine businessObject)
			{
				return new WhsHoldOrderLineDataObjectReader(orderLineDataObject, Client, Reader.logger, Reader.factory).ReadIntoBusinessObject();
			}

			protected override WhsHoldOrderLine FindMatchingBusinessObject(OrderLine orderLineDataObject)
			{
				return null;
			}
		}

		#endregion

		#region GetCombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<WhsHoldOrder> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override WhsHoldOrder GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		#endregion
	}
}
