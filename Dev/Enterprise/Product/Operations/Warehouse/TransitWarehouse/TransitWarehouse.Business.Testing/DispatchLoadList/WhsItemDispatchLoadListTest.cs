using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemDispatchLoadListTest : TestCaseWithFactory
	{
		#region TestReadOnlyFields

		static readonly string[] ReadOnlyProperties =
		{
			"WDL_WW_Warehouse",
			"WDL_SystemCreateTimeUtc",
			"WDL_SystemCreateUser",
			"WDL_SystemLastEditTimeUtc",
			"WDL_SystemLastEditUser",
			"WDL_ParentID",
			"WDL_ParentTableCode",
		};

		public void TestReadOnly()
		{
			var dll = Factory.New<WhsItemDispatchLoadList>();
			foreach (var propertyName in ReadOnlyProperties)
			{
				var readOnly = ((ZPropertyInfo)typeof(WhsItemDispatchLoadList).GetProperty(propertyName + "Info").GetValue(dll)).ReadOnly;
				var readOnlyAction = ActionFieldAttribute.Get(typeof(WhsItemDispatchLoadList).GetProperty(propertyName))?.ReadOnly;
				AssertEquals(propertyName, true, readOnly);
				AssertEquals(propertyName, true, readOnlyAction);
			}
		}

		#endregion

		#region ITransportParentCommon Members

		public void TestTypeCode()
		{
			var loadList = Factory.New<WhsItemDispatchLoadList>();
			AssertEquals(Constants.TransportParentTypes.TransitDispatchLoadList, loadList.TypeCode);
		}

		#endregion

		#region TestIsFinalised

		public void TestIsReadyToStage()
		{
			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();
			AssertEquals(false, dispatchLoadList.WDL_IsReadyToStage);

			dispatchLoadList.WDL_IsReadyToStage = true;
			AssertEquals(true, dispatchLoadList.WDL_IsReadyToStage);
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			var location1 = Factory.New<WhsLocation>();
			var location1PK = location1.PK;

			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();
			dispatchLoadList.WDL_WL_StagingLocation = location1PK;
			AssertEquals(location1PK, dispatchLoadList.WDL_WL_StagingLocation);
			AssertEquals(location1, dispatchLoadList.Location);

			var location2 = Factory.New<WhsLocation>();
			var location2PK = location2.PK;
			dispatchLoadList.WDL_WL_StagingLocation = location2PK;
			AssertEquals(location2PK, dispatchLoadList.WDL_WL_StagingLocation);
			AssertEquals(location2, dispatchLoadList.Location);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Factory.New<WhsWarehouse>();
			var warehouse1PK = warehouse1.PK;

			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();
			dispatchLoadList.WDL_WW_Warehouse = warehouse1PK;
			AssertEquals(warehouse1PK, dispatchLoadList.WDL_WW_Warehouse);
			AssertEquals(warehouse1, dispatchLoadList.Warehouse);

			var warehouse2 = Factory.New<WhsWarehouse>();
			var warehouse2PK = warehouse2.PK;
			dispatchLoadList.WDL_WW_Warehouse = warehouse2PK;
			AssertEquals(warehouse2PK, dispatchLoadList.WDL_WW_Warehouse);
			AssertEquals(warehouse2, dispatchLoadList.Warehouse);
		}

		#endregion

		#region TestReferenceNumber

		public void TestReferenceNumber()
		{
			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();
			dispatchLoadList.WDL_JobID = "TH123";
			AssertEquals("TH123", dispatchLoadList.WDL_JobID);

			dispatchLoadList.WDL_JobID = "TH321";
			AssertEquals("TH321", dispatchLoadList.WDL_JobID);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();
			AssertEquals("Dispatch Load List", dispatchLoadList.HumanReadableName);

			dispatchLoadList.WDL_JobID = "Test";
			AssertEquals("Dispatch Load List Test", dispatchLoadList.HumanReadableName);
		}

		#endregion

		#region TestAutoLog

		public void TestAutoLog()
		{
			var loadlist = Factory.New<WhsItemDispatchLoadList>();
			AssertEquals(true, loadlist.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestIsFinalised

		[TestDate(2020, 12, 22)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsFinalised()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Departed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Departed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			var packageState3 = Helper.CreatePackageState(rcn, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Finalized, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			packageState1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			packageState2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			packageState3.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var now = ZDateTimeOffset.Now;
			dtu.WDH_GateInTime = now.AddHours(-2);
			dtu.WDH_LoadCompleteTime = now.AddHours(-1);
			dtu.WDH_GateOutTime = now.AddHours(-1);
			dtu.WDH_VehicleReference = "DTU1";
			Factory.Save();

			AssertEquals(false, dll.IsFinalised);

			packageState1.WPS_Status = TransitWarehouseStatuses.Codes.Finalized;
			AssertEquals(false, dll.IsFinalised);

			dll.IsFinalised = true;
			AssertEquals("DTU should be finalised", new ZDateTimeOffset(2020, 12, 22, 10, 0, 0, new TimeSpan(10, 0, 0)), dtu.WDH_FinalisedTime);
			AssertEquals(TransitWarehouseStatuses.Codes.Finalized, packageState2.WPS_Status);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			AssertEquals("Should not create finalise log for package", 0, packageState3.Package.Logs.Find(logQuery).Length);
			var finalizeLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ItemDocumentJobFinalised.Code);
			var logs = packageState2.Package.Logs.Find(finalizeLogQuery);
			AssertEquals("Should create finalise log for package", 1, logs.Length);
			AssertEquals("P2|FAC=CFS|TYP=Finalised|WHS=WHS", logs.SingleOrDefault().SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), logs.SingleOrDefault().SL_EventTime);
		}

		public void TestIsFinalised_DTUIsNotGateOut()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			Factory.Save();

			AssertEquals("Precondition: DTU is not gate out", ZDateTimeOffset.Empty, dtu.WDH_GateOutTime);

			dll.IsFinalised = true;
			AssertEquals("DTU should not be finalised", ZDateTimeOffset.Empty, dtu.WDH_FinalisedTime);
			AssertEquals("Package Status should not be changed when DTU is not gate out", TransitWarehouseStatuses.Codes.FreightLoaded, packageState.WPS_Status);
		}

		public void TestIsFinalised_DTUIsNotFinalised()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var dateTimeNow = DateTimeOffset.Now;
			var dateTimeOffset = new DateTimeOffset(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, dateTimeNow.Hour, dateTimeNow.Minute, 0, 0, dateTimeNow.Offset);
			dtu.WDH_GateInTime = dateTimeOffset.AddHours(-2);
			dtu.WDH_LoadCompleteTime = dateTimeOffset.AddHours(-1);
			dtu.WDH_GateOutTime = dateTimeOffset.AddHours(-1);
			dtu.WDH_FinalisedTime = dateTimeOffset;
			dtu.WDH_VehicleReference = "DTU1";
			Factory.Save();

			AssertEquals("Precondition: DTU is finalised", false, dtu.WDH_FinalisedTime.IsEmpty);

			dll.IsFinalised = true;
			AssertEquals("DTU should not be changed", false, dtu.WDH_FinalisedTime.IsEmpty);
			AssertEquals("Package Status should not be changed when DTU is finalised before", TransitWarehouseStatuses.Codes.FreightLoaded, packageState.WPS_Status);
		}

		public void TestIsFinalised_DTUHasMultipleDLLs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);

			packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			Factory.Save();

			dll.IsFinalised = true;
			AssertEquals("DTU should not be finalised when it has multiple DLLs", ZDateTimeOffset.Empty, dtu.WDH_FinalisedTime);
			AssertEquals("Package Status should not be changed when DTU has multiple DLLs", TransitWarehouseStatuses.Codes.FreightLoaded, packageState.WPS_Status);
		}

		#endregion

		public void TestRefreshAdditionalReferenceNumbers()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			Helper.CreateAdditionalReference(dll, "MB1", WarehouseAdditionalReferenceTypes.Codes.MasterBill);

			Factory.Save();

			AssertEquals(1, dll.AdditionalReferenceNumbers.Count);

			Helper.CreateAdditionalReference(dll, "HB1", WarehouseAdditionalReferenceTypes.Codes.HouseBill);
			AssertEquals(1, dll.AdditionalReferenceNumbers.Count);

			dll.RefreshAdditionalReferenceNumbers();
			AssertEquals(2, dll.AdditionalReferenceNumbers.Count);
		}

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();
			AssertEquals(Constants.DocManagerCodes.TransitDispatchLoadList, ((IDocManagerSupport)dispatchLoadList).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestIDocumentSupportable

		public void TestIDocumentSupportable()
		{
			var loadlist = Factory.New<WhsItemDispatchLoadList>();
			AssertNotNull(((IDocumentSupportable)loadlist).DocumentSupporter);
		}

		#endregion

		#region TestIWorkflowProvider Members

		public void TestIWorkflowProviderMembers()
		{
			var header = Factory.New<WhsItemDispatchLoadList>();
			var newWorkflowItem = header.WorkflowItems.AddNew();
			var workflowProvider = (IWorkflowProvider)header;

			AssertEquals(WorkflowDescriptors.TransitDispatchLoadList, workflowProvider.WorkflowType);
			AssertContainsExactElementsInAnyOrder(new[] { newWorkflowItem }, workflowProvider.WorkflowItems);
			AssertEquals(typeof(ColumnValueRanker), workflowProvider.GetTemplateSelectionCriteria().GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var intendedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var dispatchLoadListWithWarehouse = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			dispatchLoadListWithWarehouse.WDH_WW_Warehouse = intendedWarehouse.PK;
			Factory.Save();

			var criteriaForDispatchLoadListWithWarehouse = (ColumnValueRanker)((IWorkflowProvider)dispatchLoadListWithWarehouse).GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { intendedWarehouse.PK, ZGuid.Empty }, criteriaForDispatchLoadListWithWarehouse.GetValues(ProcessTaskTemplateSchema.P0_WW));
		}

		#endregion

		#region IRoutingSupport Members

		public void TestRoutingSupport()
		{
			var loadList = Factory.New<WhsItemDispatchLoadList>();
			loadList.WDL_TransportMode = "Air";

			var routingSupport = (IRoutingSupport)loadList;
			AssertNotNull(routingSupport.TransportsIncludingRelated);
			AssertNotNull(routingSupport.Transports);
			AssertNullOrEmpty(routingSupport.AdditionalETAUpdateMsg);
			AssertNullOrEmpty(routingSupport.AdditionalETDUpdateMsg);
			AssertEquals("Air", routingSupport.TransportMode);
		}

		#endregion

		#region ITransportParent Members

		public void TestTransportParent()
		{
			var loadList = Factory.New<WhsItemDispatchLoadList>();

			var routingSupport = (ITransportParent)loadList;
			AssertNotNull(routingSupport.TransportSupporter);
			AssertNotNull(routingSupport.Transports);
			AssertEquals(Directions.Unknown, routingSupport.JobDirection);
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var loadlist = Factory.New<WhsItemDispatchLoadList>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails,
				PredefinedNoteTypes.Instance.DispatchInstructionWarning
			}, loadlist.NoteTypes);
		}

		#endregion

		#region TestCarrierBookingReference

		public void TestCarrierBookingReference()
		{
			var loadlist = Factory.New<WhsItemDispatchLoadList>();
			Helper.CreateAdditionalReference(loadlist, "Booking1", WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference);
			Helper.CreateAdditionalReference(loadlist, "HB1", WarehouseAdditionalReferenceTypes.Codes.HouseBill);

			AssertEquals("Booking1", loadlist.CarrierBookingReference);
		}

		#endregion

		#region LoadListType

		public void TestLoadListTypeIsNON()
		{
			var loadlist = Factory.New<WhsItemDispatchLoadList>();
			AssertEquals(TransportUnitTypes.None, loadlist.LoadListType);
		}

		public void TestLoadListTypeIsVEH()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			Factory.Save();

			AssertEquals(TransportUnitTypes.Vehicle, dll.LoadListType);
		}

		public void TestLoadListTypeIsCNT()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu1 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, "CNT1", "20GP");
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "CNT2", "20GP");
			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			Factory.Save();

			AssertEquals(TransportUnitTypes.Container, dll.LoadListType);
		}

		public void TestLoadListTypeIsULD()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu1 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, "ULD1", "AAA");
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "ULD2", "AAA");
			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			Factory.Save();

			AssertEquals(TransportUnitTypes.ULD, dll.LoadListType);
		}

		public void TestLoadListTypeIsMIX()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu1 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, "ULD1", "AAA");
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "CNT2", "20GP");
			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			Factory.Save();

			AssertEquals(TransportUnitTypes.Mix, dll.LoadListType);
		}

		#endregion

		#region IJobCostingPlugIn Member

		public void TestIJobCostingPlugIn()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			AssertEquals("JK_UniqueConsignRef", "DLL1", dll.JK_UniqueConsignRef);
			AssertEquals("ConsolExchangeRate", 0m, dll.ConsolExchangeRate);
			AssertEquals("ContainerMode", ZString.Empty, dll.ContainerMode);
			AssertEquals("ConsolType", ZString.Empty, dll.ConsolType);
			AssertEquals("Direction", ZString.Empty, dll.Direction);
			AssertEquals("Module", ApportionmentMethodModules.TransitWarehouse, dll.Module);
			AssertEquals("ExchangeRateForCurrency", 0m, dll.ExchangeRateForCurrency(null, ZGuid.Empty));
			AssertEquals("GetPrepaidCollect", ZString.Empty, dll.GetPrepaidCollect(null));

			AssertNull("LoadPort", dll.LoadPort);
			AssertNull("DischargePort", dll.DischargePort);
			AssertNull("ProfitLossContainer", dll.ProfitLossContainer);
			AssertNull("ConsolCurrency", dll.ConsolCurrency);
			AssertNull("ReceivingAgent", dll.ReceivingAgent);
			AssertNull("ReceivingAgentAPInvoicingParty", dll.ReceivingAgentAPInvoicingParty);
			AssertNull("ReceivingAgentARInvoicingParty", dll.ReceivingAgentARInvoicingParty);
			AssertNull("SendingAgent", dll.SendingAgent);
			AssertNull("SendingAgentAPInvoicingParty", dll.SendingAgentAPInvoicingParty);
			AssertNull("SendingAgentARInvoicingParty", dll.SendingAgentARInvoicingParty);
			AssertNull("PrepaidCollectList", dll.PrepaidCollectList);

			AssertNotNull("costSupporter", dll.CostSupporter);
			Assert("IsMasterCollect", !dll.IsMasterCollect);
		}

		#endregion

		#region TestDocAddresses

		public void TestJobDocAddresses()
		{
			var loadlist = Factory.New<WhsItemDispatchLoadList>();
			AssertEquals(DocAddressType.Creditor, loadlist.Creditor.DocAddressType);
		}

		public void TestSupportedAddressTypes()
		{
			var loadlist = Factory.New<WhsItemDispatchLoadList>() as IDocAddresses;
			AssertContainsExactElementsInAnyOrder(new DocAddressType[] { DocAddressType.Creditor }, loadlist.SupportedAddressTypes);
		}

		#endregion

		#region TestCreditor

		public void TestCreditor()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var creditor = Helper.CreateClient("CDT");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, creditor: creditor);

			AssertEquals("CDT", dll.Creditor.CompanyName);
		}

		#endregion

		#region Implementation

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}

	#region WhsItemDispatchLoadListWorkflowProviderTest

	[TestedType(typeof(WhsItemDispatchLoadList))]
	public class WhsItemDispatchLoadListWorkflowProviderTest : WorkflowProviderTest<WhsItemDispatchLoadList, WhsItemDispatchLoadListProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.TransitDispatchLoadList; }
		}
	}

	#endregion
}
