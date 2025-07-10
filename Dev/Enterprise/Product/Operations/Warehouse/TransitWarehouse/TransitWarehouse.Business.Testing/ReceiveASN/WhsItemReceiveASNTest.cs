using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class WhsItemReceiveASNTest : TestCaseWithFactory
	{
		#region TestReadOnlyFields

		static readonly string[] ReadOnlyProperties =
		{
			"WRP_WW_IntendedWarehouse",
			"WRP_SystemCreateTimeUtc",
			"WRP_SystemCreateUser",
			"WRP_SystemLastEditTimeUtc",
			"WRP_SystemLastEditUser",
			"WRP_ParentID",
			"WRP_ParentTableCode",
			"WRP_CompleteTime",
			"WRP_ReferenceNumber",
			"WRP_K0_ExpectedContainer",
		};

		public void TestReadOnly()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			foreach (var propertyName in ReadOnlyProperties)
			{
				var readOnly = ((ZPropertyInfo)typeof(WhsItemReceiveASN).GetProperty(propertyName + "Info").GetValue(asn)).ReadOnly;
				var readOnlyAction = ActionFieldAttribute.Get(typeof(WhsItemReceiveASN).GetProperty(propertyName))?.ReadOnly;
				AssertEquals(propertyName, true, readOnly);
				AssertEquals(propertyName, true, readOnlyAction);
			}
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var header = Factory.New<WhsItemReceiveASN>();
			AssertEquals(Constants.DocManagerCodes.TransitReceiveASN, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region DocumentSupporter

		public void TestDocumentSupporter()
		{
			var receiveExpectedPacking = Factory.New<WhsItemReceiveASN>();
			AssertType(typeof(WhsItemReceiveASNDocumentSupporter), receiveExpectedPacking.DocumentSupporter);
		}

		#endregion

		#region TestAutoLog

		public void TestAutoLog()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			AssertEquals(true, asn.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestOnFactorySaving_PopulateUniqueIDIfNeeded

		public void TestOnFactorySaving_PopulateUniqueIDIfNeeded()
		{
			var aSN = Factory.New<WhsItemReceiveASN>();

			bool factorySaveFailed = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				factorySaveFailed = true;
			}

			AssertEquals("FactorySaveFailed should be true.", true, factorySaveFailed);
			AssertEquals("Should not be in database.", false, aSN.IsInDatabase);
			AssertEquals("ReferenceNumber should be empty.", ZString.Empty, aSN.WRP_ReferenceNumber);

			var intendedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			aSN.WRP_WW_IntendedWarehouse = intendedWarehouse.PK;
			Factory.Save();

			AssertEquals("Should be in database.", true, aSN.IsInDatabase);
			AssertEquals("ReferenceNumber should not be empty.", false, aSN.WRP_ReferenceNumber.IsEmpty);

			var referenceNumber = aSN.WRP_ReferenceNumber;
			aSN.WRP_WW_IntendedWarehouse = ZGuid.Empty;
			factorySaveFailed = false;

			try
			{
				Factory.Save();
			}
			catch
			{
				factorySaveFailed = true;
			}

			AssertEquals("FactorySaveFailed should be true.", true, factorySaveFailed);
			AssertEquals("Should be in database.", true, aSN.IsInDatabase);
			AssertEquals("ReferenceNumber should not be empty.", referenceNumber, aSN.WRP_ReferenceNumber);
		}

		#endregion

		#region TestWRP_VehicleReference

		public void TestWRP_VehicleReference()
		{
			var aSN = Factory.New<WhsItemReceiveASN>();
			aSN.WRP_VehicleReference = "X123";
			AssertEquals("X123", aSN.WRP_VehicleReference);

			aSN.WRP_VehicleReference = "ZZZ";
			AssertEquals("ZZZ", aSN.WRP_VehicleReference);
		}

		#endregion

		#region TestWRP_ReferenceNumber

		public void TestWRP_ReferenceNumber()
		{
			var aSN = Factory.New<WhsItemReceiveASN>();
			aSN.WRP_ReferenceNumber = "TRT1";
			AssertEquals("TRT1", aSN.WRP_ReferenceNumber);

			aSN.WRP_ReferenceNumber = "TRT2";
			AssertEquals("TRT2", aSN.WRP_ReferenceNumber);
		}

		#endregion

		#region TestVesselLloydsNumber

		public void TestVesselLloydsNumber()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			var additionalReferenceInReceive = asn.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInReceive.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.VesselLloyds;
			additionalReferenceInReceive.CE_EntryNum = "Lloyds Number";

			AssertEquals("Lloyds Number", asn.VesselLloydsNumber);
		}

		#endregion

		#region TestVoyageNumber

		public void TestVoyageNumber()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			var additionalReferenceInReceive = asn.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInReceive.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber;
			additionalReferenceInReceive.CE_EntryNum = "Voyage123";

			AssertEquals("Voyage123", asn.VoyageNumber);
		}

		#endregion

		#region TestVesselName

		public void TestVesselName()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			var additionalReferenceInReceive = asn.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInReceive.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.Vessel;
			additionalReferenceInReceive.CE_EntryNum = "Vessel123";

			AssertEquals("Vessel123", asn.VesselName);
		}

		#endregion

		#region TestCarrierBookingReference

		public void TestCarrierBookingReference()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			var additionalReferenceInReceive = asn.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInReceive.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference;
			additionalReferenceInReceive.CE_EntryNum = "Booking1";

			AssertEquals("Booking1", asn.CarrierBookingReference);
		}

		#endregion

		#region TestWRP_ETA

		public void TestWRP_ETA()
		{
			var aSN = Factory.New<WhsItemReceiveASN>();
			aSN.WRP_ETA = new ZDateTime(2015, 09, 29);
			AssertEquals(new ZDateTime(2015, 09, 29), aSN.WRP_ETA);

			aSN.WRP_ETA = new ZDateTime(2015, 10, 30);
			AssertEquals(new ZDateTime(2015, 10, 30), aSN.WRP_ETA);
		}

		#endregion

		#region TestIntendedWarehouse

		public void TestIntendedWarehouse()
		{
			var aSN = Factory.New<WhsItemReceiveASN>();
			var warehouse1 = Factory.New<WhsWarehouse>();
			var whsPK1 = warehouse1.PK;
			aSN.WRP_WW_IntendedWarehouse = whsPK1;
			AssertEquals(whsPK1, aSN.WRP_WW_IntendedWarehouse);
			AssertEquals(warehouse1, aSN.IntendedWarehouse);

			var warehouse2 = Factory.New<WhsWarehouse>();
			var whsPK2 = warehouse2.PK;
			aSN.WRP_WW_IntendedWarehouse = whsPK2;
			AssertEquals(whsPK2, aSN.WRP_WW_IntendedWarehouse);
			AssertEquals(warehouse2, aSN.IntendedWarehouse);

			aSN.WRP_WW_IntendedWarehouse = ZGuid.Empty;
			AssertNull(aSN.IntendedWarehouse);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var aSN = Factory.New<WhsItemReceiveASN>();
			AssertEquals("Receive ASN", aSN.HumanReadableName);

			aSN.WRP_ReferenceNumber = "191";
			AssertEquals("Receive ASN 191", aSN.HumanReadableName);
		}

		#endregion

		#region TestIJobNumberMembers

		public void TestIJobNumberMembers()
		{
			var receiveASN = Factory.New<WhsItemReceiveASN>();
			receiveASN.WRP_ReferenceNumber = "1";
			AssertEquals("1", receiveASN.JobNumber);
		}

		#endregion

		#region TestIDocAddresses_SupportedAddressTypes

		public void TestIDocAddresses_SupportedAddressTypes()
		{
			var receiveASN = Factory.New<WhsItemReceiveASN>();
			AssertContainsExactElementsInAnyOrder(new[] { DocAddressType.TransportCompanyDocumentaryAddress }, ((IDocAddresses)receiveASN).SupportedAddressTypes);
		}

		#endregion

		#region TestIWorkflowProvider Members

		public void TestIWorkflowProviderMembers()
		{
			var receiveASN = Factory.New<WhsItemReceiveASN>();
			var newWorkflowItem = receiveASN.WorkflowItems.AddNew();
			var workflowProvider = (IWorkflowProvider)receiveASN;

			AssertEquals(WorkflowDescriptors.TransitReceiveASN, workflowProvider.WorkflowType);
			AssertEquals(newWorkflowItem, workflowProvider.WorkflowItems.Single());
			AssertEquals(typeof(ColumnValueRanker), workflowProvider.GetTemplateSelectionCriteria().GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var intendedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var receiveASN = Factory.NewWithValidTestData<WhsItemReceiveASN>();
			receiveASN.WRP_WW_IntendedWarehouse = intendedWarehouse.PK;
			Factory.Save();

			var criteriaForReceiveASN = (ColumnValueRanker)((IWorkflowProvider)receiveASN).GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { intendedWarehouse.PK, ZGuid.Empty }, criteriaForReceiveASN.GetValues(ProcessTaskTemplateSchema.P0_WW));
		}

		#endregion

		#region ITransportParentCommon Members

		public void TestTypeCode()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			AssertEquals(Constants.TransportParentTypes.TransitReceiveASN, asn.TypeCode);
		}

		#endregion

		#region IRoutingSupport Members

		public void TestRoutingSupport()
		{
			var receiveASN = Factory.New<WhsItemReceiveASN>();
			receiveASN.WRP_TransportMode = "Air";

			var routingSupport = (IRoutingSupport)receiveASN;
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
			var receiveASN = Factory.New<WhsItemReceiveASN>();

			var routingSupport = (ITransportParent)receiveASN;
			AssertNotNull(routingSupport.TransportSupporter);
			AssertNotNull(routingSupport.Transports);
			AssertEquals(Directions.Unknown, routingSupport.JobDirection);
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails }, asn.NoteTypes);
		}

		#endregion

		#region TestASNType

		public void TestASNTypeIsNON()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			AssertEquals(TransportUnitTypes.None, asn.ASNType);
		}

		public void TestASNTypeIsVEH()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, stageLocation.PK);
			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			Helper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);
			
			Factory.Save();

			AssertEquals(TransportUnitTypes.Vehicle, asn.ASNType);
		}

		public void TestASNTypeIsCNT()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, stageLocation.PK, "CNT1", "20GP");
			var rtu2 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, stageLocation.PK, "CNT2", "20GP");
			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			Helper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			Factory.Save();

			AssertEquals(TransportUnitTypes.Container, asn.ASNType);
		}

		public void TestASNTypeIsULD()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, stageLocation.PK, "ULD1", "AAA");
			var rtu2 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, stageLocation.PK, "ULD2", "AAA");
			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			Helper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			Factory.Save();

			AssertEquals(TransportUnitTypes.ULD, asn.ASNType);
		}

		public void TestASNTypeIsMIX()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, stageLocation.PK, "ULD1", "AAA");
			var rtu2 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, stageLocation.PK, "CNT2", "20GP");
			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			Helper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			Factory.Save();

			AssertEquals(TransportUnitTypes.Mix, asn.ASNType);
		}

		#endregion

		#region TestPlannedReceiveTransportationUnits

		public void TestPlannedReceiveTransportationUnits()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, stageLocation.PK, "ULD1", "AAA");
			var rtu2 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, stageLocation.PK, "CNT2", "20GP");
			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			Helper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			Factory.Save();
			AssertEquals("PlannedReceiveTransportationUnits", 2, asn.PlannedReceiveTransportationUnits.Count());

			AssertContainsExactElementsInAnyOrder(new[] { rtu1, rtu2 }, asn.PlannedReceiveTransportationUnits);
		}

		#endregion

		#region TestReceiveTransportationUnits

		public void TestReceiveTransportationUnits()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, stageLocation.PK, "ULD1", "AAA");
			Helper.CreatePackageState(rcn1, 1, "PKG", "", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn, receiveUnit: rtu1);

			var rtu2 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, stageLocation.PK, "CNT2", "20GP");
			Helper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			Factory.Save();
			AssertEquals("ReceiveTransportationUnits", 2, asn.ReceiveTransportationUnits.Count());

			AssertContainsExactElementsInAnyOrder(new[] { rtu1, rtu2 }, asn.ReceiveTransportationUnits);
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}

	#region WhsItemReceiveASNWorkflowProviderTest

	[TestedType(typeof(WhsItemReceiveASN))]
	public class WhsItemReceiveASNWorkflowProviderTest : WorkflowProviderTest<WhsItemReceiveASN, WhsItemReceiveASNProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.TransitReceiveASN; }
		}
	}

	#endregion
}
