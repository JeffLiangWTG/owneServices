using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.Business.Testing
{
	public class ShipmentExtensionTest : TestCaseWithFactory
	{
		public void TestHasTransferredLog()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new List<KeyValuePair<string, string>>();
			shipment.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());
			Assert("return false when type is not ISF", !shipment.HasTransferredLog("ISF"));

			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "ISF"));
			shipment.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());
			Assert("return true when mode is null", shipment.HasTransferredLog("ISF"));
		}

		public void TestHasTransferredLog_OnlyChecksTransfers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "ISF"));

			shipment.Logs.AddNew(AutoEvents.AccreditationAttemptCommenced, parameters.ToArray());
			Assert("return false when no transfer events", !shipment.HasTransferredLog("ISF"));

			shipment.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());
			Assert("return true when transfer event present", shipment.HasTransferredLog("ISF"));
		}

		public void TestHasAirAMSTransferredLog()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new List<KeyValuePair<string, string>>();
			shipment.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());
			Assert("return false when type is not AMS", !shipment.HasAirAMSTransferredLog());

			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"));
			Assert("return false when mode is null", !shipment.HasAirAMSTransferredLog());

			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Air));
			shipment.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());

			Assert("return true when mode is air", shipment.HasAirAMSTransferredLog());
		}

		public void TestHasSeaAMSTransferredLog()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var parameters = new List<KeyValuePair<string, string>>();
			shipment.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());
			Assert("return false when type is not AMS", !shipment.HasSeaAMSTransferredLog());

			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"));
			Assert("return false when mode is null", !shipment.HasSeaAMSTransferredLog());

			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Sea));
			shipment.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());

			Assert("return true when mode is sea", shipment.HasSeaAMSTransferredLog());
		}

		public void TestHasTransferredLog_RefreshesBeforeGet()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			Assert("precondition - no logs exist", !shipment.HasTransferredLog("ISF"));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);

			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "ISF"));
			shipmentInNewFactory.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());
			newFactory.Save();

			Assert("Shipment in new factory has log", shipmentInNewFactory.HasTransferredLog("ISF"));

			Assert("Shipment in original factory has log", shipment.HasTransferredLog("ISF"));
		}

		public void TestHasAirAMSTransferredLog_RefreshesBeforeGet()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			Assert("precondition - no logs exist", !shipment.HasAirAMSTransferredLog());

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);

			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"));
			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Air));
			shipmentInNewFactory.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());
			newFactory.Save();

			Assert("Shipment in new factory has log", shipmentInNewFactory.HasAirAMSTransferredLog());

			Assert("Shipment in original factory has log", shipment.HasAirAMSTransferredLog());
		}

		public void TestHasSeaAMSTransferredLog_RefreshesBeforeGet()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			Assert("precondition - no logs exist", !shipment.HasSeaAMSTransferredLog());

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);

			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"));
			parameters.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Sea));
			shipmentInNewFactory.Logs.AddNew(AutoEvents.Transferred, parameters.ToArray());
			newFactory.Save();

			Assert("Shipment in new factory has log", shipmentInNewFactory.HasSeaAMSTransferredLog());

			Assert("Shipment in original factory has log", shipment.HasSeaAMSTransferredLog());
		}

		[TestDate(2021, 2, 2, 2, 2, 2)]
		public void TestSetAllItemsFromShipmentSecurityFilingFirstUsage()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var itemA = Factory.New<HVLVItem>();
			itemA.HVI_JS_LoadedOnShipment = shipment.PK;
			var itemB = Factory.New<HVLVItem>();
			itemB.HVI_JS_LoadedOnShipment = shipment.PK;

			shipment.SetSecurityFilingFirstUsageTimeForAllItems();
			Assert(shipment.HVLVItems.Cast<HVLVItem>().All(x => x.HVI_SecurityFilingFirstUsageTimeUtc.Equals(new ZDateTime(2021, 2, 2, 2, 2, 2))));
		}

		public void TestSetAllItemsFromShipmentLastUsageCode()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var itemA = Factory.New<HVLVItem>();
			itemA.HVI_JS_LoadedOnShipment = shipment.PK;
			var itemB = Factory.New<HVLVItem>();
			itemB.HVI_JS_LoadedOnShipment = shipment.PK;

			shipment.SetLastUsageCodeForAllItems("ACA");

			Assert(shipment.HVLVItems.All(x => x.HVI_LastUsageCode.Equals("ACA")));
		}

		public void TestGetHVLVConsignmentsWithItemLoadedOnShipment_ShouldIgnoreConsignmentsWithAllItemsLoadedOnOtherShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var otherShipment = Factory.New<ForwardingShipment>();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment.PK;

			var consignmentLoadedOnOtherShipment = Factory.New<HVLVConsignment>();
			consignmentLoadedOnOtherShipment.Items.AddNew().HVI_JS_LoadedOnShipment = otherShipment.PK;

			var consignmentPartiallyLoaded = Factory.New<HVLVConsignment>();
			consignmentPartiallyLoaded.Items.AddNew().HVI_JS_LoadedOnShipment = shipment.PK;
			consignmentPartiallyLoaded.Items.AddNew().HVI_JS_LoadedOnShipment = otherShipment.PK;

			var consignmentsWithItemLoadedOnShipment = shipment.GetHVLVConsignmentsWithItemLoadedOnShipment();
			CombineAssertions("Should only get consignments with at least one item loaded on shipment", () =>
			{
				AssertEquals(2, consignmentsWithItemLoadedOnShipment.Count());
				AssertContainsExactElementsInAnyOrder(new[] { consignment.PK, consignmentPartiallyLoaded.PK }, consignmentsWithItemLoadedOnShipment.Select(x => x.PK));
			});
		}

		public void TestGetHVLVConsignmentsWithItemLoadedOnShipment_ShouldIgnoreInactiveConsignments()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var inactiveConsignment = Factory.New<HVLVConsignment>();
			inactiveConsignment.HVC_IsActive = false;
			inactiveConsignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment = Factory.New<HVLVConsignment>();
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment.PK;

			var consignmentsWithItemLoadedOnShipment = shipment.GetHVLVConsignmentsWithItemLoadedOnShipment();
			CombineAssertions("Should only get active consignment", () =>
			{
				AssertEquals(1, consignmentsWithItemLoadedOnShipment.Count());
				AssertEquals(consignment.PK, consignmentsWithItemLoadedOnShipment.Single().PK);
			});
		}

		public void TestGetLatestCargoReportedLog_LogFromDifferentBranchButSameCompany_ReturnLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
				Factory.Save();
			}

			var log = shipment.GetLatestCargoReportedLog();
			AssertNotNull("Should find Cargo Reported event", log);
			AssertEquals("Event type should be HVLV Ready", AutoEvents.HVLVReady.Code, log.SL_SE_NKEvent);
			AssertEquals("Event reason should be Cargo Report Created", EventReferenceParameterReasons.CargoReportCreated, log.Parameters[EventReferenceParameters.Codes.Reason]);
		}

		public void TestGetLatestCargoReportedLog_LogFromDifferentCompanyButSameCountry_ReturnLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "XXX";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
				Factory.Save();
			}

			var log = shipment.GetLatestCargoReportedLog();
			AssertNotNull("Should find Cargo Reported event", log);
			AssertEquals("Event type should be HVLV Ready", AutoEvents.HVLVReady.Code, log.SL_SE_NKEvent);
			AssertEquals("Event reason should be Cargo Report Created", EventReferenceParameterReasons.CargoReportCreated, log.Parameters[EventReferenceParameters.Codes.Reason]);
		}

		public void TestGetLatestCargoReportedLog_LogFromDifferentCountry_SkipLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = "XX";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "XXX";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
				Factory.Save();
			}

			AssertNull("Should find no Cargo Reported event", shipment.GetLatestCargoReportedLog());
		}

		public void TestGetLatestCargoReportingLog_LogFromDifferentBranchButSameCompany_ReturnLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReporting));
				Factory.Save();
			}

			var log = shipment.GetLatestCargoReportingLog();
			AssertNotNull("Should find Cargo Reported event", log);
			AssertEquals("Event type should be HVLV Ready", AutoEvents.HVLVReady.Code, log.SL_SE_NKEvent);
			AssertEquals("Event reason should be Cargo Reporting", EventReferenceParameterReasons.CargoReporting, log.Parameters[EventReferenceParameters.Codes.Reason]);
		}

		public void TestGetLatestCargoReportingLog_LogFromDifferentCompanyButSameCountry_ReturnLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "XXX";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReporting));
				Factory.Save();
			}

			var log = shipment.GetLatestCargoReportingLog();
			AssertNotNull("Should find Cargo Reported event", log);
			AssertEquals("Event type should be HVLV Ready", AutoEvents.HVLVReady.Code, log.SL_SE_NKEvent);
			AssertEquals("Event reason should be Cargo Reporting", EventReferenceParameterReasons.CargoReporting, log.Parameters[EventReferenceParameters.Codes.Reason]);
		}

		public void TestGetLatestCargoReportingLog_LogFromDifferentCountry_SkipLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = "XX";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "XXX";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReporting));
				Factory.Save();
			}

			AssertNull("Should find no Cargo Reporting event", shipment.GetLatestCargoReportingLog());
		}

		public void TestIsCargoReportCreated_LogFromDifferentBranchButSameCompany_CheckLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
				Factory.Save();
			}

			AssertEquals("Should return cargo report created", true, shipment.IsCargoReportCreated());
		}

		public void TestIsCargoReportCreated_LogFromDifferentCompanyButSameCountry_CheckLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "XXX";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
				Factory.Save();
			}

			AssertEquals("Should return cargo report created", true, shipment.IsCargoReportCreated());
		}

		public void TestIsCargoReportCreated_LogFromDifferentCountry_SkipLog()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = "XX";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "XXX";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
				Factory.Save();
			}

			AssertEquals("Should return cargo report not created", false, shipment.IsCargoReportCreated());
		}
	}
}
