using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eManifest.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eManifest.DataTransfer.Universal
{
	public class eManifestLineDataContextManager : ShipmentDataContextManager<SupplierBookingLine>
	{
		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new SupplierBookingLineWriter(writeManager);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderTrackingNumber, ParentBO.DL_OrderTrackingNumber);
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new SupplierBookingLineEventParentFinder(factory, this, logger);
		}

		public override ZString DataContextKey
		{
			get
			{
				var uniqueConsignRef = ParentBO.Shipment != null ? ParentBO.Shipment.JS_UniqueConsignRef : ZString.Empty;
				return string.Format("{0}~{1}", uniqueConsignRef.EscapeTildas(), ParentBO.DL_ConsigneeReference.EscapeTildas());
			}
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.eManifestLine; }
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var context = matchingValues.Key.SplitIgnoringEscapedDelimiter('~', '!');
			if (context.Length != 2)
			{
				return null;
			}

			var shipmentReference = context[0].UnEscapeTildas();
			var houseBillNumber = context[1].UnEscapeTildas();

			var shipmentQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>(), SupplierBookingLineSchema.DL_JS_ApprovedShipment);
			shipmentQuery.AddToFilter(JobShipmentSchema.JS_UniqueConsignRef, shipmentReference);

			var result = new ZDBOnlyQuery(typeof(SupplierBookingLine));
			result.AddToFilter(SupplierBookingLineSchema.DL_ConsigneeReference, houseBillNumber);
			result.AddSubQuery(shipmentQuery, JoinCondition.And);

			return result;
		}
	}
}
