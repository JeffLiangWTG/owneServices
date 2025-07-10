using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitDataContextManager))]
	class WhsItemDispatchTransportationUnitDataContextManagerTest : WhsTransitHeaderDataContextManagerTest<WhsItemDispatchTransportationUnitDataContextManager, WhsItemDispatchTransportationUnit>
	{
		#region TestDataContextKey

		protected void TestDataContextKey()
		{
			var header = Factory.New<WhsItemDispatchTransportationUnit>();
			header.WDH_ReferenceNumber = "REFNUM";
			AssertEquals(" DataContextKey should be return WDH_ReferenceNumber.", "REFNUM", header.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion

		#region TestEventContextValues

		protected override void BuildContextEventValue<T>(IItemHeader itemHeader, UniversalEvent.ContextTypes contextKey, T value)
		{
			var dtu = itemHeader as WhsItemDispatchTransportationUnit;
			switch (contextKey)
			{
				case UniversalEvent.ContextTypes.TransportReference:
					dtu.WDH_VehicleReference = value as string;
					break;
				case UniversalEvent.ContextTypes.DepotCode:
					dtu.WDH_WW_Warehouse = (value as WhsWarehouse).PK;
					break;
				case UniversalEvent.ContextTypes.ContainerNumber:
					AddContainer(dtu);
					dtu.WDH_UnitType = value as string;
					break;
				case UniversalEvent.ContextTypes.TimeOfArrival:
					ZDateTimeOffset.TryParse(value as string, out var gateInTime);
					dtu.WDH_GateInTime = gateInTime;
					break;
				case UniversalEvent.ContextTypes.MBOLNumber:
					PopulateAdditionalReference(dtu.PK, dtu.TablePrefix, WarehouseAdditionalReferenceTypes.Codes.MasterBill, value as string);
					break;
				case UniversalEvent.ContextTypes.CarriersBookingReference:
					var dll = Factory.New<WhsItemDispatchLoadList>();
					var additionalReferenceInDispatch = dll.AdditionalReferenceNumbers.AddNew();
					additionalReferenceInDispatch.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference;
					additionalReferenceInDispatch.CE_EntryNum = value as string;
					var pivot = Factory.New<WhsItemDispatchLoadListDTUPivot>();
					pivot.WLD_WDL_TransitDispatchLoadList = dll.PK;
					pivot.WLD_WDH_TransitDispatchTransportationUnit = dtu.PK;
					break;
				default:
					break;
			}
		}

		#endregion
	}
}
