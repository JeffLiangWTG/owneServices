using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitStoredProcedureHandler : ITransitDataObjectReaderHandler
	{
		public enum ProcedureType
		{
			CustomStatus,
			SecurityStatus
		}

		public TransitStoredProcedureHandler()
		{
		}

		UniversalShipment DataObject;

		bool IsTWX => DataObject.IsTransitWarehouseCombined();

		bool IsTWP => DataObject.HasRecipientRoleAndService(RecipientRoleType.ATW, ServiceCodeType.TWP) ||
										DataObject.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWP);

		bool IsTWR => DataObject.IsTransitWarehouseReceive();

		bool IsTWD => DataObject.IsTransitWarehouseDispatch();

		DataObjectReader Parent;

		HashSet<ZGuid> CachedCustomStatusPackageStatePKs;
		HashSet<ZGuid> CachedSecurityStatusPackageStatePKs;

		public void Initialize(UniversalShipment dataObject, IXmlImportLogger logger, DataObjectReader parent)
		{
			var dataContext = logger.TopLevelDataContext ?? dataObject.DataContext;
			var isConsol = dataContext.DataTargetCollection?.IsConsolDataTarget() ?? false;
			Parent = parent;

			if ((isConsol && (parent is WhsTransitReceiveConsolDataObjectReader || parent is WhsTransitDispatchConsolDataObjectReader)) || (!isConsol && (parent is WhsTransitReceiveConsignmentDataObjectReader || parent is WhsTransitDispatchConsignmentDataObjectReader)))
			{
				DataObject = dataObject;

				if (IsRequireToRunStoredProcedure(isConsol))
				{
					CachedCustomStatusPackageStatePKs = new HashSet<ZGuid>();
					CachedSecurityStatusPackageStatePKs = new HashSet<ZGuid>();
				}
			}
		}

		public void AddCachedPackageState(ZGuid[] packageStatePKs, ProcedureType procedureType)
		{
			if (procedureType == ProcedureType.CustomStatus && CachedCustomStatusPackageStatePKs != null)
			{
				packageStatePKs.ForEach(p => CachedCustomStatusPackageStatePKs.Add(p));
			}
			else if (procedureType == ProcedureType.SecurityStatus && CachedSecurityStatusPackageStatePKs != null)
			{
				packageStatePKs.ForEach(p => CachedSecurityStatusPackageStatePKs.Add(p));
			}
		}

		public void UpdateParent(DataObjectReader parent)
		{
			Parent = parent;
		}

		public void Execute(UniversalObjectFactory factory, UniversalShipment dataObject, IXmlImportLogger logger)
		{
			var dataContext = logger.TopLevelDataContext ?? dataObject.DataContext;
			var isConsol = dataContext.DataTargetCollection?.IsConsolDataTarget() ?? false;

			if (IsRequireToRunStoredProcedure(isConsol))
			{
				WhsWarehouse warehouse = null;
				if (CachedCustomStatusPackageStatePKs != null && CachedCustomStatusPackageStatePKs.Count > 0)
				{
					warehouse = factory.Load<WhsItemPackageState>(CachedCustomStatusPackageStatePKs.First()).Warehouse;
					factory.AddPostSaveAction(() => TransitUniversalHelper.ExecuteUpdatePackageStateAndRCNCustomStatusProcedure(warehouse, CachedCustomStatusPackageStatePKs.Select(p => p.ToGuid())));
				}

				if (CachedSecurityStatusPackageStatePKs != null && CachedSecurityStatusPackageStatePKs.Count > 0)
				{
					warehouse = warehouse ?? factory.Load<WhsItemPackageState>(CachedSecurityStatusPackageStatePKs.First()).Warehouse;
					factory.AddPostSaveAction(() => TransitUniversalHelper.ExecuteUpdatePackageStateSecurityStatusProcedure(warehouse, CachedSecurityStatusPackageStatePKs.Select(p => p.ToGuid())));
				}
			}
		}

		bool IsRequireToRunStoredProcedure(bool isConsol)
		{
			return ((IsTWR || IsTWX) && !isConsol && Parent is WhsTransitReceiveConsignmentDataObjectReader) ||
				((IsTWR || IsTWX) && isConsol && Parent is WhsTransitReceiveConsolDataObjectReader) ||
				((IsTWD || IsTWP) && !isConsol && Parent is WhsTransitDispatchConsignmentDataObjectReader) ||
				((IsTWD || IsTWP) && isConsol && Parent is WhsTransitDispatchConsolDataObjectReader);
		}
	}
}
