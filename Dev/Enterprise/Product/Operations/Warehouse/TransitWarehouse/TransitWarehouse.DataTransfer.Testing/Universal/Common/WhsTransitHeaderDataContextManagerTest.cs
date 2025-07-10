using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public abstract class WhsTransitHeaderDataContextManagerTest<TContextManager, TBizO> : ShipmentDataContextManagerTestCase<TContextManager, TBizO>
			where TContextManager : DataContextManager<TBizO>, IShipmentDataContextManager, new()
			where TBizO : BusinessObject, IItemHeader
	{
		#region TestEventContextValues

		public void TestEventContextValues()
		{
			var warehouse = CreateWarehouse("T1");
			var headerWithContextValue = Factory.New<TBizO>();

			BuildContextEventValue(headerWithContextValue, UniversalEvent.ContextTypes.DepotCode, warehouse);

			BuildContextEventValue(headerWithContextValue, UniversalEvent.ContextTypes.TransportReference, "Veh");

			BuildContextEventValue(headerWithContextValue, UniversalEvent.ContextTypes.ContainerNumber, "CNT");

			BuildContextEventValue(headerWithContextValue, UniversalEvent.ContextTypes.MBOLNumber, "M1");

			BuildContextEventValue(headerWithContextValue, UniversalEvent.ContextTypes.CarriersBookingReference, "Booking1");

			var gateInTime = ZDateTimeOffset.Now;
			BuildContextEventValue(headerWithContextValue, UniversalEvent.ContextTypes.TimeOfArrival, gateInTime.ToString());

			AssertEventContextValues(headerWithContextValue, "Veh", "T1", masterBillNumber: "M1", containerNumber: "Veh", gateInTime: gateInTime, carrierBookingReference: "Booking1");
		}

		WhsWarehouse CreateWarehouse(string code)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Code = code;
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;
			return warehouse;
		}

		protected void AddContainer(TBizO header)
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.KJ_ParentID = header.PK;
			var package = Factory.NewWithValidTestData<PkgPackage>();
			package.KP_KJ_ParentPackageJob = packageJob.PK;
			package.KP_F3_NKPackType = "CNT";
			var container = Factory.NewWithValidTestData<PkgPackageContainer>();
			container.K0_Seal1 = "Seal 1";
			container.K0_KP_Package = package.PK;

			var extension = Factory.New<PkgPackageExtension>();
			extension.KPN_KP_Package = package.PK;
			extension.KPN_ParentID = header.PK;
			extension.KPN_ParentTableCode = header.TablePrefix;
		}

		static void AssertEventContextValues(TBizO headerWithContextValue, string transportReference, string depotCode, string masterBillNumber, string containerNumber, ZDateTimeOffset gateInTime, string carrierBookingReference)
		{
			string eventContextValues = $@"
TransportReference - {transportReference}
MBOLNumber - {masterBillNumber}
ContainerNumber - {containerNumber}
DepotCode - {depotCode}
TimeOfArrival - {gateInTime}
CarriersBookingReference - {carrierBookingReference}";

			AssertMultilineASCIIEquals("manager.EventContextValues", eventContextValues.Trim(), (headerWithContextValue.GetUniversalDataContextManager() as IEventDataContextManager).EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		protected abstract void BuildContextEventValue<T>(IItemHeader itemHeader, UniversalEvent.ContextTypes contextKey, T value);

		protected void PopulateAdditionalReference(ZGuid parentPK, string tableName, string refType, ZString value)
		{
			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNum.CE_ParentID = parentPK;
			entryNum.CE_ParentTable = tableName;
			entryNum.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entryNum.CE_EntryType = refType;
			entryNum.CE_EntryNum = value;
		}

		#endregion

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => "";
	}
}
