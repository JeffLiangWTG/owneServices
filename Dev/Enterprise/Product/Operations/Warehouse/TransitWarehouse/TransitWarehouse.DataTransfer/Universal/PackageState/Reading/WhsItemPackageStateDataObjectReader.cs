using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsItemPackageStateDataObjectReader : DataObjectReader<IDataObject, WhsItemPackageState>
	{
		public WhsItemPackageStateDataObjectReader(IDataObject dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsItemPackageState packageState,
			WhsItemPackageStateDTO packageStateDTO, WhsTransitPackageStatePopulateStrategy populateStrategy, WhsTransitPackageStateUpdateStrategy updateStrategy, BusinessObject parent) : base(dataObject, logger, factory)
		{
			this.targetBO = packageState;
			this.sourceDataObject = (UniversalShipment)dataObject;
			this.packageStateDTO = packageStateDTO;
			this.populateStrategy = populateStrategy;
			this.updateStrategy = updateStrategy;
			this.parent = parent;
		}

		readonly WhsItemPackageState targetBO;
		readonly UniversalShipment sourceDataObject;
		readonly WhsItemPackageStateDTO packageStateDTO;
		readonly WhsTransitPackageStatePopulateStrategy populateStrategy;
		readonly WhsTransitPackageStateUpdateStrategy updateStrategy;
		readonly BusinessObject parent;

		protected override WhsItemPackageState GetExistingBusinessObject()
		{
			return targetBO;
		}

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsItemPackageState targetBO)
		{
			switch (populateStrategy)
			{
				case WhsTransitPackageStatePopulateStrategy.New:
					PopulatePackageState(targetBO);
					break;
				case WhsTransitPackageStatePopulateStrategy.Update:
					UpdatePackageState(targetBO);
					break;
				case WhsTransitPackageStatePopulateStrategy.Delete:
					DeletePackageState(targetBO);
					break;
				case WhsTransitPackageStatePopulateStrategy.Attach:
					AttachPackageState(targetBO);
					break;
				case WhsTransitPackageStatePopulateStrategy.Detach:
					DetachPackageState(targetBO);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(populateStrategy), populateStrategy, null);
			}
		}

		void PopulatePackageState(WhsItemPackageState targetBO)
		{
			SetValue(targetBO, WhsItemPackageStateSchema.WPS_IsHandlingUnit, packageStateDTO.WPS_UnitType == PackageStateUnitType.Codes.Overpack);
			SetValue(targetBO, WhsItemPackageStateSchema.WPS_UnitType, packageStateDTO.WPS_UnitType);
			SetValue(targetBO, WhsItemPackageStateSchema.WPS_Status, TransitWarehouseStatuses.Codes.Booked);
			SetValue(targetBO, WhsItemPackageStateSchema.WPS_WW_Warehouse, packageStateDTO.WPS_WW_Warehouse);
			SetValue(targetBO, WhsItemPackageStateSchema.WPS_KP_Package, packageStateDTO.WPS_KP_Package);
			SetValue(targetBO, WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, packageStateDTO.WPS_WRC_TransitReceiveConsignment);
			SetValue(targetBO, WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, packageStateDTO.WPS_WDC_TransitDispatchConsignment);
			SetValue(targetBO, WhsItemPackageStateSchema.WPS_IsHighRisk, packageStateDTO.WPS_IsHighRisk);
			if (packageStateDTO.KP_KP_ParentPackage != null)
			{
				SetValue(targetBO.Package, PkgPackageSchema.KP_KP_ParentPackage, packageStateDTO.KP_KP_ParentPackage);
			}
		}

		void UpdatePackageState(WhsItemPackageState targetBO)
		{
			switch (updateStrategy)
			{
				case WhsTransitPackageStateUpdateStrategy.CompleteUpdate:
					break;
				case WhsTransitPackageStateUpdateStrategy.PartialUpdate:
					if (packageStateDTO.WPS_IsHighRisk != null)
					{
						SetValue(targetBO, WhsItemPackageStateSchema.WPS_IsHighRisk, packageStateDTO.WPS_IsHighRisk);
					}
					if (packageStateDTO.KP_ExternalReference != ZString.Empty)
					{
						SetValue(targetBO.Package, PkgPackageSchema.KP_ExternalReference, packageStateDTO.KP_ExternalReference);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(updateStrategy), updateStrategy, null);
			}
		}

		void DeletePackageState(WhsItemPackageState targetBO)
		{
		}

		void AttachPackageState(WhsItemPackageState targetBO)
		{
			if (parent is WhsItemReceiveASN)
			{
				SetValue(targetBO, WhsItemPackageStateSchema.WPS_WRP_ReceiveExpectedPacking, parent.PK);
			}

			if (parent is WhsItemDispatchLoadList)
			{
				SetValue(targetBO, WhsItemPackageStateSchema.WPS_WDL_LoadList, parent.PK);
			}

			if (parent is WhsItemDispatchConsignment)
			{
				SetValue(targetBO, WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, parent.PK);
			}
		}

		void DetachPackageState(WhsItemPackageState targetBO)
		{
			if (parent is WhsItemReceiveASN)
			{
				SetValue(targetBO, WhsItemPackageStateSchema.WPS_WRP_ReceiveExpectedPacking, ZGuid.Empty);
			}

			if (parent is WhsItemDispatchLoadList)
			{
				if (targetBO.WPS_Status == TransitWarehouseStatuses.Codes.FreightLoaded)
				{
					SetValue(targetBO, WhsItemPackageStateSchema.WPS_RemoveFromDTU, ZBool.True);
				}
				SetValue(targetBO, WhsItemPackageStateSchema.WPS_WDL_LoadList, ZGuid.Empty);
			}

			if (parent is WhsItemDispatchConsignment)
			{
				SetValue(targetBO, WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, ZGuid.Empty);

				if (targetBO.WPS_WDL_LoadList != ZGuid.Empty)
				{
					var dll = targetBO.DispatchLoadList;
					if (dll != null)
					{
						LoadListController.StopLoadList(factory, logger, dll, sourceDataObject);

						// updating rows where the BizO is already loaded in the same factory causes issues
						// forcefully set HasChanges to true to get around this problem until Universal no longer uses BizOs.
						dll.HasChanges = true;
					}
					SetValue(targetBO, WhsItemPackageStateSchema.WPS_WDL_LoadList, ZGuid.Empty);
				}
				var hasConsolDO = sourceDataObject.GetMatchingDataSource(DataContextType.ForwardingConsol) != null;
				if (targetBO.WPS_Status == TransitWarehouseStatuses.Codes.FreightLoaded && !hasConsolDO)
				{
					SetValue(targetBO, WhsItemPackageStateSchema.WPS_RemoveFromDTU, ZBool.True);
					SetValue(targetBO, WhsItemPackageStateSchema.WPS_WDL_LoadList, ZGuid.Empty);
				}
			}
		}

		#endregion
	}
}
