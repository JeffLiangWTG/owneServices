using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public abstract class TransitConditionsSupporterTest<T> : TestCaseWithFactory where T : BusinessObject, ITransitJobForRating
	{
		public virtual void TestArrivalCFS() => AssertGetNullValue(s => s.ArrivalCFS);
		public virtual void TestControllingAgent() => AssertGetNullValue(s => s.ControllingAgent);
		public virtual void TestDepartureCFS() => AssertGetNullValue(s => s.DepartureCFS);
		public virtual void TestExportBroker() => AssertGetNullValue(s => s.ExportBroker);
		public virtual void TestImportBroker() => AssertGetNullValue(s => s.ImportBroker);
		public virtual void TestReceivingAgent() => AssertGetNullValue(s => s.ReceivingAgent);
		public virtual void TestSendingAgent() => AssertGetNullValue(s => s.SendingAgent);
		public void TestHasDangerousGoods_True() => TestHasDangerousGoodsCore(true);
		public void TestHasDangerousGoods_False() => TestHasDangerousGoodsCore(false);

		void TestHasDangerousGoodsCore(bool hasDangerousGoods)
		{
			var bo = CreateParentWithPackages(hasDangerousGoods);
			Factory.Save();
			var supporter = new TransitConditionsSupporter<T>(bo);
			AssertEquals(hasDangerousGoods, supporter.HasDangerousGoods);
		}

		void AssertGetNullValue(Func<TransitConditionsSupporter<T>, OrgHeader> getValue)
		{
			var bo = Factory.NewWithValidTestData<T>();
			Factory.Save();
			var supporter = new TransitConditionsSupporter<T>(bo);
			AssertNull(getValue(supporter));
		}

		protected abstract T CreateParentWithPackages(bool hasDangerousGoods);

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		protected WhsTransitTestHelper helper;

		protected void AddUNDG(WhsItemPackageState packageState)
		{
			var undgSubstance = Helper.CreateUNDGSubstance("BCDE", "1.5D", "BCDEa");
			undgSubstance.DG_ExceptedQuantityCode = "E1";
			var undgDataItem = Helper.CreateUNDGDataItem(packageState.Package.PK, packageState.Package.TablePrefix, undgSubstance, 2, 3);

			packageState.Package.UNDGs.Add(undgDataItem);
			packageState.Package.KP_RH_NKCommodityCode = "HAZ";
		}
	}

	public class ReceiveConsignmentConditionsSupporterTest : TransitConditionsSupporterTest<WhsItemReceiveConsignment>
	{
		protected override WhsItemReceiveConsignment CreateParentWithPackages(bool hasDangerousGoods)
		{
			var now = ZDateTimeOffset.Today;
			var warehouse = Helper.CreateTRWWarehouse("ABC");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			if (hasDangerousGoods)
			{
				AddUNDG(packageState);
			}

			return rcn;
		}
	}

	public class ReceiveTransportationUnitConditionsSupporterTest : TransitConditionsSupporterTest<WhsItemReceiveTransportationUnit>
	{
		protected override WhsItemReceiveTransportationUnit CreateParentWithPackages(bool hasDangerousGoods)
		{
			var now = ZDateTimeOffset.Today;
			var warehouse = Helper.CreateTRWWarehouse("ABC");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			if (hasDangerousGoods)
			{
				AddUNDG(packageState);
			}

			return rtu;
		}
	}

	public class DispatchConsignmentConditionsSupporterTest : TransitConditionsSupporterTest<WhsItemDispatchConsignment>
	{
		protected override WhsItemDispatchConsignment CreateParentWithPackages(bool hasDangerousGoods)
		{
			var now = ZDateTimeOffset.Today;
			var warehouse = Helper.CreateTRWWarehouse("ABC");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now.AddDays(-1);
			dtu.WDH_GateOutTime = now;
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "PKG1", TransitWarehouseStatuses.Codes.Departed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			if (hasDangerousGoods)
			{
				AddUNDG(packageState);
			}

			return dcn;
		}
	}

	public class DispatchTransportationUnitConditionsSupporterTest : TransitConditionsSupporterTest<WhsItemDispatchTransportationUnit>
	{
		protected override WhsItemDispatchTransportationUnit CreateParentWithPackages(bool hasDangerousGoods)
		{
			var now = ZDateTimeOffset.Today;
			var warehouse = Helper.CreateTRWWarehouse("ABC");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now.AddDays(-1);
			dtu.WDH_GateOutTime = now;
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "PKG1", TransitWarehouseStatuses.Codes.Departed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			if (hasDangerousGoods)
			{
				AddUNDG(packageState);
			}

			return dtu;
		}
	}
}
