using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	internal class WarehouseAdjustmentDataContextManager : WarehouseDocketDataContextManager<WhsAdjustment>, IEventDataContextManagerWithTriggeringLog
	{
		#region Shipments

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WhsAdjustmentDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WhsAdjustmentDataObjectWriter(writeManager);
		}

		#endregion

		#region Events

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var reader = new WhsAdjustmentEventExportContextBuilder(ParentBO);
				reader.AddWhsAdjustmentContextValues(result);

				result.AddRange(this.GetHoldCodeChangedEventContexts(ParentBO));
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new WhsAdjustmentEventParentFinder(factory, this, logger);
		}

		#endregion

		#region Context

		public override DataContextType DataContextType => DataContextType.WarehouseAdjustment;

		protected override string DocketTypeCode => DocketType.Codes.Adjustment;

		#endregion

		#region Recipient Roles

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
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
