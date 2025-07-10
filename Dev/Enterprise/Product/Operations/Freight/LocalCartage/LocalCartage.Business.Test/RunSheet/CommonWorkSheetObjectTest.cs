using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonWorkSheet))]
	public class CommonWorkSheetObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLegStatus()
		{
			CommonCartageLeg leg1 = Factory.New<CommonCartageLeg>();
			CommonCartageLeg leg2 = Factory.New<CommonCartageLeg>();
			CommonCartageLeg leg3 = Factory.New<CommonCartageLeg>();
			CommonWorkSheet sheet = Factory.New<CommonWorkSheet>();
			sheet.CartageLegs.AddRange(new CommonCartageLeg[] { leg1, leg2, leg3 });
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			leg3.JU_RunSheetSequence = 3;
			AssertEquals("0 / 3", sheet.LegCompletionStatus);
			leg1.JU_PickupTimeIn = ZDateTime.Now;
			leg1.JU_PickupTimeOut = ZDateTime.Now;
			leg1.JU_DeliverTimeIn = ZDateTime.Now;
			leg1.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("1 / 3", sheet.LegCompletionStatus);
		}

		public new void TestDocumentSupportableNotNull()
		{
			AssertNull(((IDocumentSupportable)Factory.New<CommonWorkSheet>()).DocumentSupporter);
		}

		public void TestOnGetCartageLegsToPrintEvent()
		{
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			bool eventFired = false;
			workSheet.OnGetCartageLegsToPrint += delegate
			{
				eventFired = true;
			};
			workSheet.RaiseOnGetCartageLegsToPrint(null);
			AssertEquals(true, eventFired);
			workSheet.OnGetCartageLegsToPrint -= delegate
			{
				eventFired = true;
			};
		}

		public void TestRaiseCartageLegAdded()
		{
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			// no exeption if no event has been hooked
			workSheet.RaiseCartageLegAdded(new WorkSheetLegLinkEventArgs(null));
			// event called if it has been hooked
			bool didCartageLegAddedCall = false;
			workSheet.OnCartageLegAdded += new EventHandler<WorkSheetLegLinkEventArgs>(delegate(object sender, WorkSheetLegLinkEventArgs e)
			{
				didCartageLegAddedCall = true;
			});
			workSheet.RaiseCartageLegAdded(new WorkSheetLegLinkEventArgs(null));
			AssertEquals("Event sould be called", true, didCartageLegAddedCall);
		}

		public void TestEY_DriversName()
		{
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("Staff Driver empty", "", workSheet.EY_DriversName);
			workSheet.EY_DriversName = "Bobby";
			AssertEquals("Non Staff entered", "Bobby", workSheet.EY_DriversName);
			GlbStaff driver = Factory.New<GlbStaff>();
			driver.GS_FullName = "Bob Diver";
			driver.GS_Code = "BOB";
			workSheet.EY_GS_NKTruckDriver = driver.GS_Code;
			AssertEquals("Staff is Bob", "Bob Diver", workSheet.EY_DriversName);
			workSheet.EY_GS_NKTruckDriver = ZString.Empty;
			AssertEquals("Staff is set to blank", "", workSheet.EY_DriversName);
		}

		public void TestTransportCompanyName()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("Trans Co Name empty", "", workSheet.TransportCompanyName);
			workSheet.EY_TransportCoName = "Bobs Trucking";
			AssertEquals("Trans Co Name entered", "Bobs Trucking", workSheet.TransportCompanyName);
			workSheet.TransportCompanyName = "Bills Trucking";
			AssertEquals("Trans Co Name entered", "Bills Trucking", workSheet.TransportCompanyName);
			AssertEquals("Trans Co Name entered", "Bills Trucking", workSheet.EY_TransportCoName);
			var truckingCo = Factory.New<OrgHeader>();
			truckingCo.OH_FullName = "Bobs Trucking Co";
			workSheet.EY_OH_TransportCo = truckingCo.PK;
			AssertEquals("Underlying Transport Co Name should be empty.", "", workSheet.EY_TransportCoName);
			AssertEquals("Transport Co name comes from Organisation", "Bobs Trucking Co", workSheet.TransportCompanyName);
			workSheet.TransportCompanyName = "Test";
			AssertEquals("Setter should do nothing when Organisation is set.", "", workSheet.EY_TransportCoName);
			AssertEquals("Setter should do nothing when Organisation is set.", "Bobs Trucking Co", workSheet.TransportCompanyName);
			workSheet.EY_OH_TransportCo = ZGuid.Empty;
			AssertEquals("Transport Co Name is set to blank", "", workSheet.TransportCompanyName);
		}

		public void TestTransportCompanyNameInfo()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var info = (ZWrappedPropertyInfo)workSheet.TransportCompanyNameInfo;
			AssertEquals(nameof(workSheet.TransportCompanyName), info.Name);
			AssertEquals(workSheet.EY_TransportCoNameInfo, info.InnerInfo);
		}

		public void TestTransportCompanyName_ReadOnly()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("Trans Co Name should not be read only by default.", false, workSheet.TransportCompanyNameInfo.ReadOnly);
			var truckingCo = Factory.New<OrgHeader>();
			truckingCo.OH_FullName = "Bobs Trucking Co";
			workSheet.EY_OH_TransportCo = truckingCo.PK;
			AssertEquals("Trans Co Name should be read only when Transport Co is set.", true, workSheet.TransportCompanyNameInfo.ReadOnly);
		}

		public void TestEY_RQ_TruckSetsTransportCo()
		{
			GlbBranch branch = Factory.New<GlbBranch>();
			OrgHeader proxy = Factory.New<OrgHeader>();
			branch.GB_OH_OrgProxy = proxy.PK;
			OrgHeader someOtherProxy = Factory.New<OrgHeader>();
			RefEquipment truck = Factory.New<RefEquipment>();
			CommonWorkSheet sheet = Factory.New<CommonWorkSheet>();
			CommonCartageLeg leg1 = sheet.CartageLegs.AddNew();
			CommonCartageLeg leg2 = sheet.CartageLegs.AddNew();
			truck.RQ_OH_Owner = proxy.PK;
			sheet.EY_RQ_Truck = truck.PK;
			AssertEquals(proxy.PK, sheet.EY_OH_TransportCo);
			sheet.EY_OH_TransportCo = someOtherProxy.PK;
			sheet.EY_RQ_Truck = ZGuid.Empty;
			AssertEquals(someOtherProxy.PK, sheet.EY_OH_TransportCo);
			sheet.EY_RQ_Truck = truck.PK;
			AssertEquals(someOtherProxy.PK, sheet.EY_OH_TransportCo);
			sheet.EY_RQ_Truck = ZGuid.Empty;
			sheet.EY_OH_TransportCo = ZGuid.Empty;
			sheet.EY_RQ_Truck = truck.PK;
			AssertEquals(proxy.PK, sheet.EY_OH_TransportCo);
			AssertEquals(proxy.PK, leg1.QuickOHTransportCompany);
			AssertEquals(proxy.PK, leg2.QuickOHTransportCompany);
		}

		public void TestEY_TruckRegistration()
		{
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("rego Co empty", "", workSheet.EY_TruckRegistration);
			workSheet.EY_TruckRegistration = "rego";
			AssertEquals("rego Co Name entered", "rego", workSheet.EY_TruckRegistration);
			RefEquipment bobsTruck = Helper.CreateTruck("BobsTruck");
			bobsTruck.RQ_Registration = "BobsRego";
			workSheet.EY_RQ_Truck = bobsTruck.PK;
			AssertEquals("rego is BobsRego", "BobsRego", workSheet.EY_TruckRegistration);
			workSheet.EY_RQ_Truck = ZGuid.Empty;
			AssertEquals("rego is set to blank", "", workSheet.EY_TruckRegistration);
		}

		public void TestEY_DriversLicence()
		{
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("Licence Co empty", "", workSheet.EY_DriversLicence);
			workSheet.EY_DriversLicence = "licence";
			AssertEquals("Licence Co Name entered", "licence", workSheet.EY_DriversLicence);
			GlbStaff david = Helper.CreateStaff("DD", "David");
			GenRegCertAccredMaintList licence = david.Certificates.AddNew();
			licence.XZ_Type = CertificateTypePairList.Codes.CA1;
			licence.XZ_RefNumber = "DavidsLicence";
			workSheet.EY_GS_NKTruckDriver = david.GS_Code;
			AssertEquals("Licence is DavidsLicence", "DavidsLicence", workSheet.EY_DriversLicence);
			workSheet.EY_GS_NKTruckDriver = string.Empty;
			AssertEquals("Licence is set to blank", "", workSheet.EY_DriversLicence);
		}

		public void TestHeading()
		{
			var year = ZDateTime.Now.Year;
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_DriversName = "Bob";
			runSheet.EY_StartTime = new ZDateTime(year, 4, 1, 0, 0, 0);
			runSheet.EY_EndTime = new ZDateTime(year, 4, 1, 23, 59, 0);
			AssertEquals("driver and midnight", "BOB - 01 Apr", runSheet.Heading);
			runSheet.EY_DriversName = "Bob";
			runSheet.EY_TruckRegistration = "Truck";
			runSheet.EY_StartTime = new ZDateTime(year, 4, 1, 0, 0, 0);
			runSheet.EY_EndTime = new ZDateTime(year, 4, 1, 23, 59, 0);
			AssertEquals("Truck changed", "BOB - TRUCK - 01 Apr", runSheet.Heading);
			runSheet.EY_DriversName = "Bob";
			runSheet.EY_TruckRegistration = "Truck";
			runSheet.EY_TransportCoName = "Tran";
			runSheet.EY_StartTime = new ZDateTime(year, 4, 1, 0, 0, 0);
			runSheet.EY_EndTime = new ZDateTime(year, 4, 1, 23, 59, 0);
			AssertEquals("Transport co changed", "BOB - TRUCK - 01 Apr", runSheet.Heading);
			runSheet.EY_DriversName = "Bob";
			runSheet.EY_TruckRegistration = "Truck";
			runSheet.EY_TransportCoName = "Tran";
			runSheet.EY_StartTime = new ZDateTime(year, 4, 1, 9, 0, 0);
			runSheet.EY_EndTime = new ZDateTime(year, 4, 2, 10, 0, 0);
			AssertEquals("Date changed", "BOB - TRUCK - 01 Apr 09:00 - 02 Apr 10:00", runSheet.Heading);
			var vehicle = Factory.New<RefEquipment>();
			vehicle.RQ_RC_RoadContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "RTRK").PK;
			vehicle.RQ_Registration = "REG";
			runSheet.EY_RQ_Truck = vehicle.PK;
			AssertEquals("Truck Type added", "BOB - REG - 01 Apr 09:00 - 02 Apr 10:00 - RTRK", runSheet.Heading);
			runSheet.EY_DriversName = "";
			runSheet.EY_TruckRegistration = "";
			runSheet.EY_RQ_Truck = ZGuid.Empty;
			AssertEquals("Transport Co Name is used when there is no Driver or Truck Rego.", "TRAN - 01 Apr 09:00 - 02 Apr 10:00", runSheet.Heading);
			var truckingCo = Factory.New<OrgHeader>();
			truckingCo.OH_FullName = "Bobs Trucking Co";
			runSheet.EY_OH_TransportCo = truckingCo.PK;
			AssertEquals("Transport Co Name is used when there is no Driver or Truck Rego.", "BOBS TRUCKING CO - 01 Apr 09:00 - 02 Apr 10:00", runSheet.Heading);
		}

		public void TestStatus()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("None", nameof(CommonWorkSheet.RunSheetStatuses.None), runSheet.Status);
			CommonCartageLeg leg1 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg2 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg3 = runSheet.CartageLegs.AddNew();
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			leg3.JU_RunSheetSequence = 3;
			AssertEquals("OK", nameof(CommonWorkSheet.RunSheetStatuses.OK), runSheet.Status);
			leg1.JU_PickupTimeIn = ZDateTime.Now;
			leg1.JU_PickupTimeOut = ZDateTime.Now;
			leg1.JU_DeliverTimeIn = ZDateTime.Now;
			leg1.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("OK", nameof(CommonWorkSheet.RunSheetStatuses.OK), runSheet.Status);
			leg2.JU_PickupTimeIn = ZDateTime.Now;
			leg2.JU_PickupTimeOut = ZDateTime.Now;
			leg2.JU_DeliverTimeIn = ZDateTime.Now;
			leg2.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("Near completion", nameof(CommonWorkSheet.RunSheetStatuses.NearCompletion), runSheet.Status);
			leg3.JU_PickupTimeIn = ZDateTime.Now;
			leg3.JU_PickupTimeOut = ZDateTime.Now;
			leg3.JU_DeliverTimeIn = ZDateTime.Now;
			leg3.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("Completed", nameof(CommonWorkSheet.RunSheetStatuses.Completed), runSheet.Status);
			leg3.JU_DeliverTimeOut = ZDateTime.Empty;
			leg3.JU_MessageStatus = Core.Constants.CartageLegDispatchStatusList.Codes.Rejected;
			AssertEquals("Error", nameof(CommonWorkSheet.RunSheetStatuses.LegError), runSheet.Status);
		}

		public void TestStatus_Grouping()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("None", nameof(CommonWorkSheet.RunSheetStatuses.None), runSheet.Status);
			CommonCartageLeg leg1 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg2 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg3 = runSheet.CartageLegs.AddNew();
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			leg3.JU_RunSheetSequence = 2;
			AssertEquals("OK", nameof(CommonWorkSheet.RunSheetStatuses.OK), runSheet.Status);
			leg1.JU_PickupTimeIn = ZDateTime.Now;
			leg1.JU_PickupTimeOut = ZDateTime.Now;
			leg1.JU_DeliverTimeIn = ZDateTime.Now;
			leg1.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("Near completion", nameof(CommonWorkSheet.RunSheetStatuses.NearCompletion), runSheet.Status);
			leg2.JU_PickupTimeIn = ZDateTime.Now;
			leg2.JU_PickupTimeOut = ZDateTime.Now;
			leg2.JU_DeliverTimeIn = ZDateTime.Now;
			leg2.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("Completed", nameof(CommonWorkSheet.RunSheetStatuses.Completed), runSheet.Status);
			leg3.JU_DeliverTimeOut = ZDateTime.Empty;
			leg3.JU_MessageStatus = Core.Constants.CartageLegDispatchStatusList.Codes.Rejected;
			AssertEquals("Error", nameof(CommonWorkSheet.RunSheetStatuses.LegError), runSheet.Status);
		}

		public void TestHumanReadableName()
		{
			var commonWorkSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("Run Sheet", commonWorkSheet.HumanReadableName);
			commonWorkSheet.EY_RunSheetNumber = "2";
			AssertEquals("Run Sheet 2", commonWorkSheet.HumanReadableName);
		}

		public void TestStatusDescription()
		{
			CommonCartage cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_AirImport, 1);
			JobDocAddress cfsAddress = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCFS, "cfs", "cfs", "2000", "sydney", "AUSYD", false);
			cfsAddress.Organisation.OH_Code = "CFS";
			cfsAddress.Address.OA_Code = "ADD CFS";
			JobDocAddress importerAddress = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "importer", "importer", "2000", "sydney", "AUSYD", false);
			importerAddress.Organisation.OH_Code = "IMP";
			importerAddress.Address.OA_Code = "ADD IMP";
			CommonCartageLeg leg = cartage.CartageLegs[0];
			leg.JU_E2PickupAddressID = cfsAddress.PK;
			leg.JU_E2DeliveryAddressID = importerAddress.PK;
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("Idle", "Idle", runSheet.StatusDescription);
			runSheet.CartageLegs.Add(leg);
			leg.JU_RunSheetSequence = 1;
			AssertEquals("Idle", "Idle", runSheet.StatusDescription);
			leg.JU_PickupTimeIn = ZDateTime.Now;
			AssertEquals("Picking Up - CFS - ADD CFS", runSheet.StatusDescription);
			leg.JU_PickupTimeOut = ZDateTime.Now;
			AssertEquals("Picked Up - CFS - ADD CFS", runSheet.StatusDescription);
			leg.JU_WaitPointTimeIn = ZDateTime.Now;
			AssertEquals("Wait Delivering", runSheet.StatusDescription);
			leg.JU_WaitPointTimeOut = ZDateTime.Now;
			AssertEquals("Wait Delivered", runSheet.StatusDescription);
			leg.JU_DeliverTimeIn = ZDateTime.Now;
			AssertEquals("Delivering - IMP - ADD IMP", runSheet.StatusDescription);
			leg.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals("Delivered - IMP - ADD IMP", runSheet.StatusDescription);
			leg.JU_MessageStatus = Core.Constants.CartageLegDispatchStatusList.Codes.Rejected;
			AssertEquals("Rejected - Delivered - IMP - ADD IMP", runSheet.StatusDescription);
		}

		public void TestGetCartageLegsInOrder()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			AssertEquals("None", nameof(CommonWorkSheet.RunSheetStatuses.None), runSheet.Status);
			CommonCartageLeg leg1 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg2 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg3 = runSheet.CartageLegs.AddNew();
			leg1.JU_RunSheetSequence = 2;
			leg2.JU_RunSheetSequence = 1;
			leg3.JU_RunSheetSequence = 3;
			CommonCartageLeg[] legs = runSheet.GetCartageLegsInOrder(ListSortDirection.Ascending);
			AssertEquals(3, legs.Length);
			AssertEquals("Ascending 1", leg2.PK, legs[0].PK);
			AssertEquals("Ascending 2", leg1.PK, legs[1].PK);
			AssertEquals("Ascending 3", leg3.PK, legs[2].PK);
			legs = runSheet.GetCartageLegsInOrder(ListSortDirection.Descending);
			AssertEquals(3, legs.Length);
			AssertEquals("Descending 1", leg3.PK, legs[0].PK);
			AssertEquals("Descending 2", leg1.PK, legs[1].PK);
			AssertEquals("Descending 3", leg2.PK, legs[2].PK);
		}

		public void TestActiveLeg()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			AssertNull(runSheet.PrimaryActiveLeg);
			CommonCartageLeg leg1 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg2 = runSheet.CartageLegs.AddNew();
			CommonCartageLeg leg3 = runSheet.CartageLegs.AddNew();
			leg1.JU_RunSheetSequence = 1;
			leg2.JU_RunSheetSequence = 2;
			leg3.JU_RunSheetSequence = 3;
			AssertNull(runSheet.PrimaryActiveLeg);
			leg1.JU_PickupTimeIn = ZDateTime.Now;
			AssertEquals(leg1.PK, runSheet.PrimaryActiveLeg.PK);
			leg1.JU_PickupTimeOut = ZDateTime.Now;
			AssertEquals(leg1.PK, runSheet.PrimaryActiveLeg.PK);
			leg1.JU_DeliverTimeIn = ZDateTime.Now;
			AssertEquals(leg1.PK, runSheet.PrimaryActiveLeg.PK);
			leg1.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals(leg1.PK, runSheet.PrimaryActiveLeg.PK);
			leg2.JU_DeliverTimeOut = ZDateTime.Now;
			AssertEquals(leg2.PK, runSheet.PrimaryActiveLeg.PK);
			leg3.JU_PickupTimeOut = ZDateTime.Now;
			AssertEquals(leg3.PK, runSheet.PrimaryActiveLeg.PK);
		}

		public void TestErrorStatus()
		{
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			AssertEquals(CommonWorkSheet.ErrorStatuses.Working, runSheet.ErrorStatus);
			Assert(runSheet.ErrorReason.IsEmpty);
			AssertEquals("Idle", runSheet.StatusDescription);
			runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Canceled;
			AssertEquals(CommonWorkSheet.ErrorStatuses.Canceled, runSheet.ErrorStatus);
			Assert(runSheet.ErrorReason.IsEmpty);
			AssertEquals("Canceled", runSheet.StatusDescription);
			runSheet.ErrorReason = "Mistake";
			AssertEquals("Mistake", runSheet.ErrorReason);
			AssertEquals("Canceled - Mistake", runSheet.StatusDescription);
			runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.DriverOutOfAction;
			AssertEquals(CommonWorkSheet.ErrorStatuses.DriverOutOfAction, runSheet.ErrorStatus);
			Assert(runSheet.ErrorReason.IsEmpty);
			AssertEquals("Driver - Out Of Action", runSheet.StatusDescription);
			runSheet.ErrorReason = "Sick";
			AssertEquals("Sick", runSheet.ErrorReason);
			AssertEquals("Driver - Out Of Action - Sick", runSheet.StatusDescription);
			runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.VehicleOutOfAction;
			AssertEquals(CommonWorkSheet.ErrorStatuses.VehicleOutOfAction, runSheet.ErrorStatus);
			Assert(runSheet.ErrorReason.IsEmpty);
			AssertEquals("Vehicle - Out Of Action", runSheet.StatusDescription);
			runSheet.ErrorReason = "Broken Down";
			AssertEquals("Broken Down", runSheet.ErrorReason);
			AssertEquals("Vehicle - Out Of Action - Broken Down", runSheet.StatusDescription);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonWorkSheet runSheet_NewFactory = newFactory.Load<CommonWorkSheet>(runSheet.PK);
			AssertEquals("Vehicle - Out Of Action", runSheet_NewFactory.ErrorStatus);
			AssertEquals("Broken Down", runSheet_NewFactory.ErrorReason);
			AssertEquals("Vehicle - Out Of Action - Broken Down", runSheet_NewFactory.StatusDescription);
			runSheet_NewFactory.ErrorStatus = CommonWorkSheet.ErrorStatuses.Working;
			newFactory.Save();
			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			CommonWorkSheet runSheet_NewFactory2 = newFactory.Load<CommonWorkSheet>(runSheet.PK);
			AssertEquals("Working", runSheet_NewFactory2.ErrorStatus);
			AssertEquals("", runSheet_NewFactory2.ErrorReason);
			AssertEquals("Idle", runSheet_NewFactory2.StatusDescription);
		}

		public void TestErrorReason()
		{
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Working;
			AssertEquals("", runSheet.ErrorReason);
			runSheet.ErrorReason = "AA";
			AssertEquals("AA", runSheet.ErrorReason);
			runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Canceled;
			runSheet.ErrorReason = "CC";
			AssertEquals("CC", runSheet.ErrorReason);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var runSheetInNewFactory = newFactory.Load<CommonWorkSheet>(runSheet.PK);
			AssertEquals(CommonWorkSheet.ErrorStatuses.Canceled, runSheetInNewFactory.ErrorStatus);
			AssertEquals("CC", runSheetInNewFactory.ErrorReason);
			runSheetInNewFactory.ErrorReason = "DD";
			AssertEquals("DD", runSheetInNewFactory.ErrorReason);
			newFactory.Save();
			var newFactoryToCheckErrorReason = new BusinessObjectFactory();
			var runsheetInNewFactoryToCheckErrorReason = newFactoryToCheckErrorReason.Load<CommonWorkSheet>(runSheet.PK);
			AssertEquals("DD", runsheetInNewFactoryToCheckErrorReason.ErrorReason);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var runSheet = Factory.New<CommonWorkSheet>();
			var leg1 = runSheet.CartageLegs.AddNew();
			var leg2 = runSheet.CartageLegs.AddNew();
			AssertContainsExactElementsInAnyOrder("Related Logs has Legs", runSheet.BusinessObjectsWithRelatedEvents, new CommonCartageLeg[] { leg1, leg2 });
		}

		public void TestIRelatedJobNumberMembers()
		{
			var cartage1 = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCFStoCYD, 1);
			cartage1.JJ_ConsignmentID = "blah";
			var cartage2 = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCFStoCYD, 1);
			cartage2.JJ_ConsignmentID = "blah2";
			var leg1 = cartage1.ContainerBookedMoves.AddNew().CartageLegs.AddNew();
			var leg2 = cartage2.ContainerBookedMoves.AddNew().CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg1);
			runSheet.CartageLegs.Add(leg2);
			var jobNumbers = ((IRelatedJobNumber)runSheet).JobNumber;
			AssertContainsExactElementsInAnyOrder(new[] { "blah", "blah2" }, jobNumbers);
		}

		public void TestICreditControlledDocumentDelivery()
		{
			var client1 = Helper.CreateOrgHeader("Client", "Addy");
			var client2 = Helper.CreateOrgHeader("Client", "Addy");
			var cartage1 = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCFStoCYD, 1);
			new JobHeader.Loader(cartage1).TryCreate();
			cartage1.LocalClientAddressPK = client1.MainAddress.PK;
			var leg1 = cartage1.ContainerBookedMoves.AddNew().CartageLegs.AddNew();
			var cartage2 = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCFStoCYD, 1);
			new JobHeader.Loader(cartage2).TryCreate();
			cartage2.LocalClientAddressPK = client2.MainAddress.PK;
			var leg2 = cartage2.ContainerBookedMoves.AddNew().CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.CartageLegs.Add(leg1);
			runSheet.CartageLegs.Add(leg2);
			Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			var creditControlled = (ICreditControlledDocumentDelivery)runSheet;
			AssertEquals(2, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(client1));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(client2));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		public void TestResetErrorStatus()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			workSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Canceled;
			workSheet.ErrorReason = "Reason";
			AssertEquals(CommonWorkSheet.ErrorStatuses.Canceled, workSheet.ErrorStatus);
			AssertEquals("Reason", workSheet.ErrorReason);
			workSheet.ResetErrorStatus();
			AssertEquals(CommonWorkSheet.ErrorStatuses.Working, workSheet.ErrorStatus);
			AssertEquals("", workSheet.ErrorReason);
		}

		public void TestHasMixedContainerMode()
		{
			var container = Factory.New<CommonContainer>();
			// CASE 1: All legs use containers
			var cartage1 = Factory.New<CommonCartage>();
			var move1 = cartage1.ContainerBookedMoves.AddNew();
			var leg11 = move1.CartageLegs.AddNew();
			var leg12 = move1.CartageLegs.AddNew();
			var workSheet1 = Factory.New<CommonWorkSheet>();
			workSheet1.CartageLegs.AddRange(new[] { leg11, leg12 });
			AssertEquals("Expecting flag set to false when all legs use containers", false, workSheet1.HasMixedContainerMode);
			// CASE 2: All legs use loose packs
			var cartage2 = Factory.New<CommonCartage>();
			var move2 = cartage2.LooseBookedMoves.AddNew();
			var leg21 = move2.CartageLegs.AddNew();
			var leg22 = move2.CartageLegs.AddNew();
			var workSheet2 = Factory.New<CommonWorkSheet>();
			workSheet2.CartageLegs.AddRange(new[] { leg21, leg22 });
			AssertEquals("Expecting flag set to false when all legs use loose packs", false, workSheet2.HasMixedContainerMode);
			// CASE 3: All legs use mixed containers and loose packs
			var cartage3 = Factory.New<CommonCartage>();
			var move31 = cartage3.ContainerBookedMoves.AddNew();
			var leg31 = move31.CartageLegs.AddNew();
			var move32 = cartage3.LooseBookedMoves.AddNew();
			var leg32 = move32.CartageLegs.AddNew();
			var workSheet3 = Factory.New<CommonWorkSheet>();
			workSheet3.CartageLegs.AddRange(new[] { leg31, leg32 });
			AssertEquals("Expecting flag set to true when all legs use mixed containers and loose packs", true, workSheet3.HasMixedContainerMode);
		}

		public void TestIGenericJobCostPlugIn_CostSupporter()
		{
			var costPlugIn = (IGenericJobCostPlugIn)Factory.New<CommonWorkSheet>();
			AssertNotNull(costPlugIn.CostSupporter);
			AssertType<CommonWorkSheetCostSupporter>(costPlugIn.CostSupporter);
		}

		public void TestIRatingSupporter_AdaptersProvider()
		{
			var ratingSupporter = (IRatingSupporter)Factory.New<CommonWorkSheet>();
			AssertNotNull(ratingSupporter.AdaptersProvider);
			AssertType<CommonWorkSheetRatingAdapterProvider>(ratingSupporter.AdaptersProvider);
		}

		public void TestIJobCostingPlugIn_Precondition()
		{
			var costingPlugIn = (IJobCostingPlugIn)Factory.New<CommonWorkSheet>();
			AssertEquals("Exchage Rate for Currency", 0m, costingPlugIn.ExchangeRateForCurrency(null, ZGuid.Empty));
			AssertEquals("Get Prepaid Collect", ZString.Empty, costingPlugIn.GetPrepaidCollect(null));
			AssertEquals("JK Unique Consign Ref", ZString.Empty, costingPlugIn.JK_UniqueConsignRef);
			AssertNull("Load Port", costingPlugIn.LoadPort);
			AssertNull("Discharge Port", costingPlugIn.DischargePort);
			AssertNull("Profit Loss Container", costingPlugIn.ProfitLossContainer);
			AssertEquals("Consol Exchange Rate", 0m, costingPlugIn.ConsolExchangeRate);
			AssertNull("Consol Currency", costingPlugIn.ConsolCurrency);
			AssertEquals("Is Master Collect", false, costingPlugIn.IsMasterCollect);
			AssertNull("Receiving Agent", costingPlugIn.ReceivingAgent);
			AssertNull("Receiving Agent AP Invoicing Party", costingPlugIn.ReceivingAgentAPInvoicingParty);
			AssertNull("Receiving Agent AR Invoicing Party", costingPlugIn.ReceivingAgentARInvoicingParty);
			AssertNull("Sending Agent", costingPlugIn.SendingAgent);
			AssertNull("Sending Agent AP Invoicing Party", costingPlugIn.SendingAgentAPInvoicingParty);
			AssertNull("Sending Agent AR Invoicing Party", costingPlugIn.SendingAgentARInvoicingParty);
			AssertEquals("Transport Mode", ZString.Empty, costingPlugIn.TransportMode);
			AssertEquals("Module", ApportionmentMethodModules.TransportBooking, costingPlugIn.Module);
			AssertEquals("ContainerMode", ZString.Empty, costingPlugIn.ContainerMode);
			AssertEquals("ConsolType", ZString.Empty, costingPlugIn.ConsolType);
			AssertEquals("Direction", ZString.Empty, costingPlugIn.Direction);
			AssertNull("Prepaid Collect List", costingPlugIn.PrepaidCollectList);
		}

		public void TestIJobCostingPlugIn_AddNewToLogs()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var costingPlugIn = (IJobCostingPlugIn)workSheet;
			costingPlugIn.AddNewToLogs(Events.CustomisableEvent00, "This is SPARTAAA!");
			AssertEquals("Expecting 1 event logged", 1, workSheet.Logs.LogsNotInDB.Length);
		}

		public void TestIJobCostingPlugIn_UniqueConsignRef()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			workSheet.EY_RunSheetNumber = "RS42";
			AssertEquals("Expecting run sheet number for the ref", "RS42", ((IJobCostingPlugIn)workSheet).JK_UniqueConsignRef);
		}

		public void TestCartageLegsRegisteredEditable()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.AddNew();
			Assert(!workSheet.IsRoot);
			Assert(!workSheet.IsRegisteredEditableChildObject(workSheet.CartageLegs));
			workSheet = Factory.New<CommonWorkSheet>();
			workSheet.IsRoot = true;
			workSheet.CartageLegs.AddNew();
			Assert(workSheet.IsRoot);
			Assert(workSheet.IsRegisteredEditableChildObject(workSheet.CartageLegs));
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
