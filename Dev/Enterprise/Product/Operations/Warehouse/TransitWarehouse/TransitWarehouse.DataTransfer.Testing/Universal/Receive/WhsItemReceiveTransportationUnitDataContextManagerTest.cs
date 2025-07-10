using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitDataContextManager))]
	class WhsItemReceiveTransportationUnitDataContextManagerTest : WhsTransitHeaderDataContextManagerTest<WhsItemReceiveTransportationUnitDataContextManager, WhsItemReceiveTransportationUnit>
	{
		#region TestDataContextKey

		protected void TestDataContextKey()
		{
			var unit = Factory.New<WhsItemReceiveTransportationUnit>();
			unit.WRH_ReferenceNumber = "REFNUM";
			AssertEquals(" DataContextKey should be return WRH_ReferenceNumber.", "REFNUM", unit.GetUniversalDataContextManager().DataContextKey);
		}

		#endregion

		#region TestEventContextValues

		protected override void BuildContextEventValue<T>(IItemHeader itemHeader, UniversalEvent.ContextTypes contextKey, T value)
		{
			var rtu = itemHeader as WhsItemReceiveTransportationUnit;
			switch (contextKey)
			{
				case UniversalEvent.ContextTypes.TransportReference:
					rtu.WRH_VehicleReference = value as string;
					break;
				case UniversalEvent.ContextTypes.DepotCode:
					rtu.WRH_WW_Warehouse = (value as WhsWarehouse).PK;
					break;
				case UniversalEvent.ContextTypes.ContainerNumber:
					AddContainer(rtu);
					rtu.WRH_UnitType = value as string;
					break;
				case UniversalEvent.ContextTypes.TimeOfArrival:
					ZDateTimeOffset.TryParse(value as string, out var gateInTime);
					rtu.WRH_GateInTime = gateInTime;
					break;
				case UniversalEvent.ContextTypes.MBOLNumber:
					PopulateAdditionalReference(rtu.PK, rtu.TablePrefix, WarehouseAdditionalReferenceTypes.Codes.MasterBill, value as string);
					break;
				case UniversalEvent.ContextTypes.CarriersBookingReference:
					var asn = Factory.New<WhsItemReceiveASN>();
					var additionalReferenceInReceive = asn.AdditionalReferenceNumbers.AddNew();
					additionalReferenceInReceive.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference;
					additionalReferenceInReceive.CE_EntryNum = value as string;
					var pivot = Factory.New<WhsItemReceiveASNRTUPivot>();
					pivot.WAR_WRP_TransitReceiveASN = asn.PK;
					pivot.WAR_WRH_TransitReceiveTransportationUnit = rtu.PK;
					break;
				default:
					break;
			}
		}

		#endregion

		protected override bool ManagerChecksDataTargetToImport => false;
	}
}
