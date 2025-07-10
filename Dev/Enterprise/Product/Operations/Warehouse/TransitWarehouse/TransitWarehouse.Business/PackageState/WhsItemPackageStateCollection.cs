using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemPackageStateCollection : ActiveBusinessObjectCollection<WhsItemPackageState>
	{
		public WhsItemPackageStateCollection(WhsItemReceiveConsignment consignment)
			: base(consignment.Factory, consignment, null, WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment)
		{
		}

		public WhsItemPackageStateCollection(WhsItemDispatchTransportationUnit transportationUnit)
			: base(transportationUnit.Factory, transportationUnit, null, WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader)
		{
		}

		public WhsItemPackageStateCollection(WhsItemReceiveTransportationUnit transportationUnit)
			: base(transportationUnit.Factory, transportationUnit, null, WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader)
		{
		}

		public WhsItemPackageStateCollection(WhsItemDispatchConsignment consignment)
			: base(consignment.Factory, consignment, null, WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment)
		{
		}

		public WhsItemPackageStateCollection(WhsItemReceiveASN receiveASN)
			: base(receiveASN.Factory, receiveASN, null, WhsItemPackageStateSchema.WPS_WRP_ReceiveExpectedPacking)
		{
		}

		public WhsItemPackageStateCollection(WhsItemDispatchLoadList dispatchLoadList)
			: base(dispatchLoadList.Factory, dispatchLoadList, null, WhsItemPackageStateSchema.WPS_WDL_LoadList)
		{
		}

		public WhsItemPackageStateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		/// <summary>
		/// This constructor is only used to return Avaliable to Attach packages for initial loading of the Attach Packages form.
		/// After filtering is applied this grid is refreshed via Filter property in FilterBusinessObjects.
		/// </summary>
		/// <param name="planner"></param>		
		public WhsItemPackageStateCollection(TransitWarehousePackagePlanner planner)
			: base(planner.Factory, new AdhocCollectionRelationship(typeof(WhsItemPackageState)))
		{
			if (!planner.IsSkipWhsItemPackageStateCollection)
			{
				AddAvailablePackages(planner.Parent?.JobNumber ?? "");
			}
		}

		void AddAvailablePackages(string jobNumber)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			var additionalReference = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.ICusEntryNumber), WhsItemPackageStateSchema.PK, CusEntryNumSchema.CE_ParentID);
			additionalReference.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			additionalReference.AddToFilter(CusEntryNumSchema.CE_EntryNum, jobNumber);
			query.AddSubQuery(additionalReference, JoinCondition.And);

			var availablePackageStates = new ZQuery(WhsItemPackageStateSchema.WPS_Status, new[] { AttachablePackageStateStatuses.Codes.ARV, AttachablePackageStateStatuses.Codes.CTT, AttachablePackageStateStatuses.Codes.PIC, AttachablePackageStateStatuses.Codes.PUT, AttachablePackageStateStatuses.Codes.STA });

			AddRange(Factory.Load<WhsItemPackageState>(availablePackageStates).Except(Factory.Load<WhsItemPackageState>(query)));
		}

		/// <summary>
		/// This constructor is only used for loading already attached packages. 
		/// </summary>
		/// <param name="planner"></param>
		/// <param name="jobNumber"></param>
		public WhsItemPackageStateCollection(TransitWarehousePackagePlanner planner, string jobNumber)
			: base(planner.Factory, new AdhocCollectionRelationship(typeof(WhsItemPackageState)))
		{
			AddPackagesAlreadyAttached(jobNumber, planner.Warehouse.PK);
		}

		void AddPackagesAlreadyAttached(string shipmentNumber, ZGuid warehousePK)
		{
			var query = TransitWarehouseHelper.GetAttachedPackagesQuery(shipmentNumber, warehousePK);
			AddRange(Factory.Load<WhsItemPackageState>(query));
		}
	}
}
