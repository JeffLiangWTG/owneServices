using CargoWise.Application;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsBondedChangeOfInventoryDataObjectReader : WhsDataObjectReader<WhsBondedChangeOfInventory>
	{
		internal WhsBondedChangeOfInventoryDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		#region DataContextType

		public override DataContextType DataContextType => DataContextType.WarehouseBondedChangeOfInventory;

		#endregion

		#region GetNewBusinessObject

		protected override WhsBondedChangeOfInventory GetNewBusinessObject()
		{
			return new WhsBondedChangeOfInventory(factory.BOFactory);
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsBondedChangeOfInventory targetBO)
		{
			CheckNotUSCustoms();

			var existingReceive = (WhsReceive)((ITopLevelDataObjectReader)new WhsReceiveDataObjectReader(dataObject, logger, new UniversalObjectFactory())).GetExistingBusinessObject();

			if (logger.TopLevelDataContext.IsWarehouseBondedChangeOfRegime())
			{
				var customsChangeOfRegimeDetails = dataObject.GetWarehouseCustomsDetailsChangeOfRegime(logger.TopLevelDataContext);
				if (customsChangeOfRegimeDetails == null)
				{
					throw new DataObjectReadFailureException(Res.GetString("22030784-64ed-4ab4-aee4-5d61e224b4bf", "Customs Details for Change of Regime import could not be found for Country Code: {0}.", logger.TopLevelDataContext.CountryCodeToImportInto));
				}
				else if (existingReceive != null
					&& !existingReceive.Warehouse.WW_IsVirtualWarehouse
					&& existingReceive.StartedReceiving)
				{
					throw new DataObjectReadFailureException(Res.GetString("464e9ae8-a18f-4ab3-a300-c545f7619310", "Cannot amend receive for Change of Regime if original receive has started receiving in Physical Warehouse."));
				}
			}

			var holdStock = logger.TopLevelDataContext.ContainsHoldCode();
			var isStockWithdrawn = existingReceive != null && ObjectFactory.New<IWhsReceiveCustomsAmendmentChecker>().IsStockWithdrawn(existingReceive);
			if (isStockWithdrawn)
			{
				if (holdStock)
				{
					throw new DataObjectReadFailureException("Cannot hold stock. Stock related properties affected.");
				}
				else
				{
					targetBO.Order = (WhsOrder)((IChangeOfInventoryGetExistingBusinessObject)new WhsOrderDataObjectReader(dataObject, logger, new UniversalObjectFactory())).GetExistingBusinessObject();
				}
			}
			else
			{
				targetBO.Order = new WhsOrderDataObjectReader(dataObject, logger, factory).ReadIntoBusinessObject();
			}

			var factoryToUse = holdStock && existingReceive == null ? new UniversalObjectFactory() : factory;
			targetBO.Receive = new WhsReceiveDataObjectReader(dataObject, logger, factoryToUse, targetBO.Order).ReadIntoBusinessObject();
		}

		#endregion

		#region CheckNotUSCustoms

		void CheckNotUSCustoms()
		{
			if (logger.TopLevelDataContext.CountryCodeToImportInto == Core.Constants.CountryCodes.UnitedStates)
			{
				throw new DataObjectReadFailureException(Res.GetString("WhsBondedChangeOfOwnershipDataObjectReader|CheckNotUSCustoms", "Change of Inventory for US Customs is not supported."));
			}
		}

		#endregion

		#region GetCombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<WhsBondedChangeOfInventory> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#endregion

		#region GetExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override WhsBondedChangeOfInventory GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		#endregion

		#region CustomsHelper

		protected CustomsDataSourceHelper<WhsReceive> GetNewCustomsDataSourceHelper()
		{
			return new CustomsDataSourceHelperForReceive(dataObject, logger.TopLevelDataContext);
		}

		#endregion
	}
}
