using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	internal class WarehouseReceiveDataContextManager : WarehouseOrderAndReceiveDataContextManager<WhsReceive, WhsReceiveLine>, IEventDataContextManagerWithTriggeringLog
	{
		#region Context

		public override DataContextType DataContextType => DataContextType.WarehouseReceive;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
			=> recipientRoles.Any(o => o.Code == RecipientRoleType.WIN || o.Code == RecipientRoleType.BWI);

		protected override string DocketTypeCode => DocketType.Codes.Receive;

		#endregion

		#region Event

		#region EventContextValues

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var whsReceiveContextReader = new WhsReceiveEventExportContextBuilder(ParentBO);
				whsReceiveContextReader.AddWhsDocketContextValues(result);

				result.AddRange(this.GetHoldCodeChangedEventContexts(ParentBO));
			}

			return result;
		}

		#endregion

		#region GetEventParentFinder

		protected override WhsOrderAndReceiveEventParentFinder<WhsReceive> GetEventParentFinderCore(BusinessObjectFactory factory, IXmlImportLogger logger)
			=> new WhsReceiveEventParentFinder(factory, this, logger);

		#endregion

		#region OnUniversalEventAddedCore

		protected override void CancelDocket(WhsReceive receive)
		{
			receive.ClearLocations();
			receive.CancelReactivateDocket();
		}

		protected override CustomsDataSourceHelper<WhsReceive> GetNewCustomsHelper(UniversalEvent eventAdded, IDataContextDataObject topLevelDataObject)
		{
			return new CustomsDataSourceHelperForReceive(eventAdded, topLevelDataObject);
		}

		#endregion

		#endregion

		#region Shipment

		protected override WhsOrderAndReceiveDataObjectReader<WhsReceive, WhsReceiveLine> GetShipmentDataObjectReaderCore(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WhsReceiveDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WhsReceiveDataObjectWriter(writeManager);
		}

		#endregion

		// interfaces

		#region IEventDataContextManagerWithTriggeringLog Members

		BaseStmALog IEventDataContextManagerWithTriggeringLog.TriggeringLogForUseInPopulatingEventContext
		{
			get;
			set;
		}

		#endregion
	}
}
