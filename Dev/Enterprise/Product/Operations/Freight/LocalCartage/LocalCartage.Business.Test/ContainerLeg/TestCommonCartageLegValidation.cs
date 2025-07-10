using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	internal class TestCommonCartageLegValidation : BusinessObjectValidationTestCase
	{
		public void TestQuickGSDriver()
		{
			var provider = new RunSheetSecurityQueryProviderForTest();
			Factory.SetValue<IRunSheetSecurityQueryProvider>(() => provider);
			var leg = Factory.New<CommonCartageLeg>();
			var bill = Factory.New<GlbStaff>();
			bill.GS_Code = "CC";
			leg.QuickGSDriver = bill.GS_Code;
			AssertHasError(leg.QuickGSDriverInfo, "This leg is not authorized to be allocated to a Run Sheet.");
			provider.AuthoriseBy = "Ted";
			leg.QuickGSDriver = ZString.Empty;
			var driverGroup = Factory.NewWithValidTestData<GlbGroup>();
			var bob = Factory.New<GlbStaff>();
			bob.GS_FullName = "Bob Diver";
			bob.GS_LoginName = "Bob";
			bob.GS_Code = "Bob";
			bob.Groups.Add(driverGroup);
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driverGroup.PK.ToGuid());
			Factory.Save();
			leg = Factory.New<CommonCartageLeg>();
			Assert("Before: Should NOT be in error", !leg.QuickGSDriverInfo.HasErrors());
			leg.QuickGSDriver = bob.GS_Code;
			Assert("Before: Should be in error", leg.QuickGSDriverInfo.HasErrors());
			leg.JU_PlannedPickupTime = ZDateTime.Now;
			leg.Validation.ValidateQuickGSDriver();
			Assert("Same: Should NOT be no errors", !leg.QuickGSDriverInfo.HasErrors());
			leg.JU_PlannedPickupTime = ZDateTime.Empty;
			leg.Validation.ValidateQuickGSDriver();
			Assert("Before: Should be in error", leg.QuickGSDriverInfo.HasErrors());
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			leg.JU_EY_RunSheet = runSheet.PK;
			leg.Validation.ValidateQuickGSDriver();
			Assert("Same: Should NOT be no errors", !leg.QuickGSDriverInfo.HasErrors());
		}

		public void TestQuickRQTruck()
		{
			var provider = new RunSheetSecurityQueryProviderForTest();
			Factory.SetValue<IRunSheetSecurityQueryProvider>(() => provider);
			var leg = Factory.New<CommonCartageLeg>();
			var billsTruck = Factory.New<RefEquipment>();
			leg.QuickRQTruck = billsTruck.PK;
			AssertHasError(leg.QuickRQTruckInfo, "This leg is not authorized to be allocated to a Run Sheet.");
			provider.AuthoriseBy = "Ted";
			leg.QuickRQTruck = ZGuid.Empty;
			var bobsTruck = Factory.New<RefEquipment>();
			bobsTruck.RQ_ShortCode = "BobT";
			bobsTruck.RQ_IsVehicle = true;
			leg = Factory.New<CommonCartageLeg>();
			Assert("Before: Should NOT be in error", !leg.QuickRQTruckInfo.HasErrors());
			leg.QuickRQTruck = bobsTruck.PK;
			Assert("Before: Should be in error", leg.QuickRQTruckInfo.HasErrors());
			leg.JU_PlannedPickupTime = ZDateTime.Now;
			leg.Validation.ValidateQuickRQTruck();
			Assert("Same: Should NOT be no errors", !leg.QuickRQTruckInfo.HasErrors());
			leg.JU_PlannedPickupTime = ZDateTime.Empty;
			leg.Validation.ValidateQuickRQTruck();
			Assert("Before: Should be in error", leg.QuickRQTruckInfo.HasErrors());
			var runSheet = Factory.New<CommonWorkSheet>();
			leg.JU_EY_RunSheet = runSheet.PK;
			leg.Validation.ValidateQuickRQTruck();
			Assert("Same: Should NOT be no errors", !leg.QuickRQTruckInfo.HasErrors());
		}

		public void TestQuickOHTransportCompany()
		{
			var provider = new RunSheetSecurityQueryProviderForTest();
			Factory.SetValue<IRunSheetSecurityQueryProvider>(() => provider);
			var billCo = Factory.New<OrgHeader>();
			billCo.OH_Code = "BillCo";
			var leg = Factory.New<CommonCartageLeg>();
			leg.QuickOHTransportCompany = billCo.PK;
			AssertHasError(leg.QuickOHTransportCompanyInfo, "This leg is not authorized to be allocated to a Run Sheet.");
			provider.AuthoriseBy = "Ted";
			leg.QuickOHTransportCompany = ZGuid.Empty;
			var bobsTruckingCo = Factory.New<OrgHeader>();
			bobsTruckingCo.OH_Code = "Bob";
			bobsTruckingCo.OH_IsShippingProvider = true;
			bobsTruckingCo.OH_IsLocalTransport = true;
			Factory.Save();
			leg = Factory.New<CommonCartageLeg>();
			Assert("Before: Should NOT be in error", !leg.QuickOHTransportCompanyInfo.HasErrors());
			leg.QuickOHTransportCompany = bobsTruckingCo.PK;
			Assert("Before: Should be in error", leg.QuickOHTransportCompanyInfo.HasErrors());
			leg.JU_PlannedPickupTime = ZDateTime.Now;
			leg.Validation.ValidateQuickOHTransportCompany();
			Assert("Same: Should NOT be no errors", !leg.QuickOHTransportCompanyInfo.HasErrors());
			leg.JU_PlannedPickupTime = ZDateTime.Empty;
			leg.Validation.ValidateQuickOHTransportCompany();
			Assert("Before: Should be in error", leg.QuickOHTransportCompanyInfo.HasErrors());
			var runSheet = Factory.New<CommonWorkSheet>();
			leg.JU_EY_RunSheet = runSheet.PK;
			leg.Validation.ValidateQuickOHTransportCompany();
			Assert("Same: Should NOT be no errors", !leg.QuickOHTransportCompanyInfo.HasErrors());
		}

		public void TestQuickPlannedPickupTime()
		{
			ZDateTime now = ZDateTime.Now;
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_StartTime = now.Date;
			runSheet.EY_EndTime = now.Date.AddDays(1);
			Factory.Save();
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			leg.JU_EY_RunSheet = runSheet.PK;
			leg.Validation.ValidateQuickPlannedPickupTime();
			leg.Validation.ValidateJU_PlannedPickupTime();
			Assert("Should be errors", leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be errors", leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PlannedPickupTime = now.AddDays(5);
			leg.Validation.ValidateQuickPlannedPickupTime();
			leg.Validation.ValidateJU_PlannedPickupTime();
			Assert("Should be errors", leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be errors", leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PlannedPickupTime = now;
			leg.Validation.ValidateQuickPlannedPickupTime();
			leg.Validation.ValidateJU_PlannedPickupTime();
			Assert("Should be No errors", !leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be No errors", !leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PlannedPickupTime = ZDateTime.Empty;
			leg.Validation.ValidateQuickPlannedPickupTime();
			leg.Validation.ValidateJU_PlannedPickupTime();
			Assert("Should be errors", leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be errors", leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PickupTimeIn = now;
			leg.JU_PickupTimeOut = ZDateTime.Empty;
			leg.JU_DeliverTimeIn = ZDateTime.Empty;
			leg.JU_DeliverTimeOut = ZDateTime.Empty;
			leg.Validation.ValidateQuickPlannedPickupTime();
			leg.Validation.ValidateJU_PlannedPickupTime();
			Assert("Should be No errors", !leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be No errors", !leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PickupTimeIn = ZDateTime.Empty;
			leg.JU_PickupTimeOut = now;
			leg.JU_DeliverTimeIn = ZDateTime.Empty;
			leg.JU_DeliverTimeOut = ZDateTime.Empty;
			leg.Validation.ValidateQuickPlannedPickupTime();
			leg.Validation.ValidateJU_PlannedPickupTime();
			Assert("Should be No errors", !leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be No errors", !leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PickupTimeIn = ZDateTime.Empty;
			leg.JU_PickupTimeOut = ZDateTime.Empty;
			leg.JU_DeliverTimeIn = now;
			leg.JU_DeliverTimeOut = ZDateTime.Empty;
			leg.Validation.ValidateQuickPlannedPickupTime();
			leg.Validation.ValidateJU_PlannedPickupTime();
			Assert("Should be No errors", !leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be No errors", !leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PickupTimeIn = ZDateTime.Empty;
			leg.JU_PickupTimeOut = ZDateTime.Empty;
			leg.JU_DeliverTimeIn = ZDateTime.Empty;
			leg.JU_DeliverTimeOut = now;
			leg.Validation.ValidateQuickPlannedPickupTime();
			leg.Validation.ValidateJU_PlannedPickupTime();
			Assert("Should be No errors", !leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be No errors", !leg.QuickPlannedPickupTimeInfo.HasErrors());
		}

		public void TestQuickEstimatedDeliveryTime()
		{
			ZDateTime now = ZDateTime.Now;
			CommonWorkSheet runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_StartTime = now.Date;
			runSheet.EY_EndTime = now.Date.AddDays(1);
			Factory.Save();
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			leg.JU_EY_RunSheet = runSheet.PK;
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should be errors", leg.JU_EstimatedDeliveryTimeInfo.HasErrors());
			Assert("Should be errors", leg.QuickEstimatedDeliveryTimeInfo.HasErrors());
			leg.JU_PlannedPickupTime = now.AddDays(5);
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should have no errors", !leg.JU_EstimatedDeliveryTimeInfo.HasErrors());
			Assert("Should have no errors", !leg.QuickEstimatedDeliveryTimeInfo.HasErrors());
			leg.JU_EstimatedDeliveryTime = now.AddDays(6);
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should be errors", leg.JU_EstimatedDeliveryTimeInfo.HasErrors());
			Assert("Should be errors", leg.QuickEstimatedDeliveryTimeInfo.HasErrors());
			leg.JU_EstimatedDeliveryTime = now;
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should be errors", leg.JU_EstimatedDeliveryTimeInfo.HasErrors());
			Assert("Should be errors", leg.QuickEstimatedDeliveryTimeInfo.HasErrors());
			leg.JU_PlannedPickupTime = now.AddDays(-1);
			leg.JU_EstimatedDeliveryTime = now;
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should have no errors", !leg.JU_EstimatedDeliveryTimeInfo.HasErrors());
			Assert("Should have no errors", !leg.QuickEstimatedDeliveryTimeInfo.HasErrors());
			leg.JU_PlannedPickupTime = ZDateTime.Empty;
			leg.JU_EstimatedDeliveryTime = ZDateTime.Empty;
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should be errors", leg.JU_EstimatedDeliveryTimeInfo.HasErrors());
			Assert("Should be errors", leg.QuickEstimatedDeliveryTimeInfo.HasErrors());
			leg.JU_PickupTimeIn = now;
			leg.JU_PickupTimeOut = ZDateTime.Empty;
			leg.JU_DeliverTimeIn = ZDateTime.Empty;
			leg.JU_DeliverTimeOut = ZDateTime.Empty;
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should be No errors", !leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be No errors", !leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PickupTimeIn = ZDateTime.Empty;
			leg.JU_PickupTimeOut = now;
			leg.JU_DeliverTimeIn = ZDateTime.Empty;
			leg.JU_DeliverTimeOut = ZDateTime.Empty;
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should be No errors", !leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be No errors", !leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PickupTimeIn = ZDateTime.Empty;
			leg.JU_PickupTimeOut = ZDateTime.Empty;
			leg.JU_DeliverTimeIn = now;
			leg.JU_DeliverTimeOut = ZDateTime.Empty;
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should be No errors", !leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be No errors", !leg.QuickPlannedPickupTimeInfo.HasErrors());
			leg.JU_PickupTimeIn = ZDateTime.Empty;
			leg.JU_PickupTimeOut = ZDateTime.Empty;
			leg.JU_DeliverTimeIn = ZDateTime.Empty;
			leg.JU_DeliverTimeOut = now;
			leg.Validation.ValidateQuickEstimatedDeliveryTime();
			leg.Validation.ValidateJU_EstimatedDeliveryTime();
			Assert("Should be No errors", !leg.JU_PlannedPickupTimeInfo.HasErrors());
			Assert("Should be No errors", !leg.QuickPlannedPickupTimeInfo.HasErrors());
		}

		public void TestJU_PickupTimeIn()
		{
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeIn, JobContainerLegsSchema.JU_PickupTimeOut, LegTimeNotification.ErrorIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeIn, JobContainerLegsSchema.JU_WaitPointTimeIn, LegTimeNotification.WarningIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeIn, JobContainerLegsSchema.JU_WaitPointTimeOut, LegTimeNotification.WarningIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeIn, JobContainerLegsSchema.JU_DeliverTimeIn, LegTimeNotification.WarningIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeIn, JobContainerLegsSchema.JU_DeliverTimeOut, LegTimeNotification.WarningIfLater);
		}

		public void TestJU_PickupTimeOut()
		{
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeOut, JobContainerLegsSchema.JU_PickupTimeIn, LegTimeNotification.ErrorIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeOut, JobContainerLegsSchema.JU_WaitPointTimeIn, LegTimeNotification.WarningIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeOut, JobContainerLegsSchema.JU_WaitPointTimeOut, LegTimeNotification.WarningIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeOut, JobContainerLegsSchema.JU_DeliverTimeIn, LegTimeNotification.WarningIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_PickupTimeOut, JobContainerLegsSchema.JU_DeliverTimeOut, LegTimeNotification.WarningIfLater);
		}

		public void TestJU_WaitPointTimeIn()
		{
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeIn, JobContainerLegsSchema.JU_PickupTimeIn, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeIn, JobContainerLegsSchema.JU_PickupTimeOut, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeIn, JobContainerLegsSchema.JU_WaitPointTimeOut, LegTimeNotification.ErrorIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeIn, JobContainerLegsSchema.JU_DeliverTimeIn, LegTimeNotification.WarningIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeIn, JobContainerLegsSchema.JU_DeliverTimeOut, LegTimeNotification.WarningIfLater);
		}

		public void TestJU_WaitPointTimeOut()
		{
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeOut, JobContainerLegsSchema.JU_PickupTimeIn, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeOut, JobContainerLegsSchema.JU_PickupTimeOut, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeOut, JobContainerLegsSchema.JU_WaitPointTimeIn, LegTimeNotification.ErrorIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeOut, JobContainerLegsSchema.JU_DeliverTimeIn, LegTimeNotification.WarningIfLater);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_WaitPointTimeOut, JobContainerLegsSchema.JU_DeliverTimeOut, LegTimeNotification.WarningIfLater);
		}

		public void TestJU_DeliverTimeIn()
		{
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeIn, JobContainerLegsSchema.JU_PickupTimeIn, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeIn, JobContainerLegsSchema.JU_PickupTimeOut, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeIn, JobContainerLegsSchema.JU_WaitPointTimeIn, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeIn, JobContainerLegsSchema.JU_WaitPointTimeOut, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeIn, JobContainerLegsSchema.JU_DeliverTimeOut, LegTimeNotification.ErrorIfLater);
		}

		public void TestJU_DeliverTimeOut()
		{
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeOut, JobContainerLegsSchema.JU_PickupTimeIn, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeOut, JobContainerLegsSchema.JU_PickupTimeOut, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeOut, JobContainerLegsSchema.JU_WaitPointTimeIn, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeOut, JobContainerLegsSchema.JU_WaitPointTimeOut, LegTimeNotification.WarningIfEarlier);
			AssertLegTimeHasNotification(JobContainerLegsSchema.JU_DeliverTimeOut, JobContainerLegsSchema.JU_DeliverTimeIn, LegTimeNotification.ErrorIfEarlier);
		}

		enum LegTimeNotification
		{
			None,
			ErrorIfEarlier,
			ErrorIfLater,
			WarningIfEarlier,
			WarningIfLater,
		}

		void AssertLegTimeHasNotification(SchemaDateTimeColumn compare, SchemaDateTimeColumn compareTo, LegTimeNotification notifyType)
		{
			var today = ZDateTime.Today;
			var leg = Factory.New<CommonCartageLeg>();
			leg.HasWaitPoint = true;
			SetupLegAndTimes(leg, today, today, today, today, today, today);
			var info = leg.FindPropertyInfo(compare.Name);
			AssertEquals($"Precondition: {info.Name} should have no errors/warnings", false, info.HasNotifications());
			var compareToInfo = leg.FindPropertyInfo(compareTo.Name);
			AssertLegTimeNotifications(today, info, compareToInfo, notifyType);
			var isTestingWaitPointFields = (new[] { JobContainerLegsSchema.JU_WaitPointTimeIn, JobContainerLegsSchema.JU_WaitPointTimeOut }.Intersect(new[] { compare, compareTo })).Any();
			if (isTestingWaitPointFields)
			{
				leg.HasWaitPoint = false; // ensure no notifications with waitpoint fields if leg is not a waitpoint
				AssertLegTimeNotifications(today, info, compareToInfo, LegTimeNotification.None);
			}
		}

		void AssertLegTimeNotifications(ZDateTime today, ZPropertyInfo info, ZPropertyInfo compareToInfo, LegTimeNotification notifyType)
		{
			info.Value = today.AddDays(-1);
			var testMessage = $"{info.HumanReadableName} has the following Notifications: {info.Notifications.ToUniqueMessageListString()}";
			AssertEquals(testMessage, notifyType == LegTimeNotification.ErrorIfEarlier, info.HasError($"{info.HumanReadableName} must be the same or later than {compareToInfo.HumanReadableName}."));
			AssertEquals(testMessage, notifyType == LegTimeNotification.WarningIfEarlier, info.HasWarning($"{info.HumanReadableName} is earlier than {compareToInfo.HumanReadableName}."));
			compareToInfo.Value = today.AddDays(-1);
			info.Value = today;
			testMessage = $"{info.HumanReadableName} has the following Notifications: {info.Notifications.ToUniqueMessageListString()}";
			AssertEquals(testMessage, notifyType == LegTimeNotification.ErrorIfLater, info.HasError($"{info.HumanReadableName} must be the same or earlier than {compareToInfo.HumanReadableName}."));
			AssertEquals(testMessage, notifyType == LegTimeNotification.WarningIfLater, info.HasWarning($"{info.HumanReadableName} is later than {compareToInfo.HumanReadableName}."));
		}

		void SetupLegAndTimes(CommonCartageLeg leg, ZDateTime pickupIn, ZDateTime pickupOut, ZDateTime waitPointIn, ZDateTime waitPointOut, ZDateTime deliveryIn, ZDateTime deliveryOut)
		{
			leg.JU_PickupTimeIn = pickupIn;
			leg.JU_PickupTimeOut = pickupOut;
			leg.JU_WaitPointTimeIn = waitPointIn;
			leg.JU_WaitPointTimeOut = waitPointOut;
			leg.JU_DeliverTimeIn = deliveryIn;
			leg.JU_DeliverTimeOut = deliveryOut;
		}

		public void TestJU_E2PickupAddressID()
		{
			JobDocAddress address1 = cartage.DocAddresses.AddNew();
			address1.E2_AddressOverride = true;
			address1.E2_CompanyName = "Company1";
			address1.E2_Address1 = "Address1";
			JobDocAddress address2 = cartage.DocAddresses.AddNew();
			address2.E2_AddressOverride = true;
			address2.E2_CompanyName = "Company2";
			address2.E2_Address1 = "AddressSame";
			JobDocAddress address3 = cartage.DocAddresses.AddNew();
			address3.E2_AddressOverride = true;
			address3.E2_CompanyName = "Company3";
			address3.E2_Address1 = "AddressSame";
			leg.JU_E2PickupAddressID = address1.PK;
			leg.JU_E2WaitPointAddressID = address2.PK;
			leg.JU_E2DeliveryAddressID = address3.PK;
			leg.Validation.ValidateJU_E2PickupAddressID();
			AssertNoErrors(leg.JU_E2PickupAddressIDInfo);
			leg.JU_E2PickupAddressID = address2.PK;
			Assert(leg.JU_E2PickupAddressIDInfo.HasError("Pickup Address cannot be the same as Wait Point Address"));
			leg.JU_E2PickupAddressID = address3.PK;
			AssertNoErrors(leg.JU_E2PickupAddressIDInfo);
			leg.JU_E2WaitPointAddressID = ZGuid.Empty;
			leg.Validation.ValidateJU_E2PickupAddressID();
			Assert(leg.JU_E2PickupAddressIDInfo.HasError("Pickup Address cannot be the same as Delivery Address"));
		}

		public void TestJU_E2WaitPointAddressID()
		{
			JobDocAddress address1 = cartage.DocAddresses.AddNew();
			address1.E2_AddressOverride = true;
			address1.E2_CompanyName = "Company1";
			address1.E2_Address1 = "Address1";
			JobDocAddress address2 = cartage.DocAddresses.AddNew();
			address2.E2_AddressOverride = true;
			address2.E2_CompanyName = "Company2";
			address2.E2_Address1 = "AddressSame";
			JobDocAddress address3 = cartage.DocAddresses.AddNew();
			address3.E2_AddressOverride = true;
			address3.E2_CompanyName = "Company3";
			address3.E2_Address1 = "AddressSame";
			leg.JU_E2PickupAddressID = address1.PK;
			leg.JU_E2WaitPointAddressID = address2.PK;
			leg.JU_E2DeliveryAddressID = address3.PK;
			leg.Validation.ValidateJU_E2WaitPointAddressID();
			AssertNoErrors(leg.JU_E2WaitPointAddressIDInfo);
			leg.JU_E2WaitPointAddressID = address1.PK;
			Assert(leg.JU_E2WaitPointAddressIDInfo.HasError("Wait Point Address cannot be the same as Pickup Address"));
			leg.JU_E2WaitPointAddressID = ZGuid.Empty;
			AssertNoErrors(leg.JU_E2WaitPointAddressIDInfo);
			leg.JU_E2WaitPointAddressID = address3.PK;
			Assert(leg.JU_E2WaitPointAddressIDInfo.HasError("Wait Point Address cannot be the same as Delivery Address"));
		}

		public void TestJU_E2DeliveryAddressID()
		{
			JobDocAddress address1 = cartage.DocAddresses.AddNew();
			address1.E2_AddressOverride = true;
			address1.E2_CompanyName = "Company1";
			address1.E2_Address1 = "Address1";
			JobDocAddress address2 = cartage.DocAddresses.AddNew();
			address2.E2_AddressOverride = true;
			address2.E2_CompanyName = "Company2";
			address2.E2_Address1 = "AddressSame";
			JobDocAddress address3 = cartage.DocAddresses.AddNew();
			address3.E2_AddressOverride = true;
			address3.E2_CompanyName = "Company3";
			address3.E2_Address1 = "AddressSame";
			leg.JU_E2PickupAddressID = address1.PK;
			leg.JU_E2WaitPointAddressID = address2.PK;
			leg.JU_E2DeliveryAddressID = address3.PK;
			leg.Validation.ValidateJU_E2DeliveryAddressID();
			AssertNoErrors(leg.JU_E2DeliveryAddressIDInfo);
			leg.JU_E2DeliveryAddressID = address2.PK;
			Assert(leg.JU_E2DeliveryAddressIDInfo.HasError("Delivery Address cannot be the same as Wait Point Address"));
			leg.JU_E2DeliveryAddressID = address1.PK;
			AssertNoErrors(leg.JU_E2DeliveryAddressIDInfo);
			leg.JU_E2WaitPointAddressID = ZGuid.Empty;
			leg.Validation.ValidateJU_E2DeliveryAddressID();
			Assert(leg.JU_E2DeliveryAddressIDInfo.HasError("Delivery Address cannot be the same as Pickup Address"));
		}

		public void TestJU_EY_RunSheet()
		{
			var provider = new RunSheetSecurityQueryProviderForTest();
			Factory.SetValue<IRunSheetSecurityQueryProvider>(() => provider);
			var runSheet = Factory.New<CommonWorkSheet>();
			leg.JU_EY_RunSheet = runSheet.PK;
			AssertHasError(leg.JU_EY_RunSheetInfo, "This leg is not authorized to be allocated to a Run Sheet.");
			provider.AuthoriseBy = "Ted";
			leg.JU_EY_RunSheet = ZGuid.Empty;
			runSheet.EY_DriversName = "Bob";
			runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Canceled;
			leg.JU_EY_RunSheet = runSheet.PK;
			Assert(leg.JU_EY_RunSheetInfo.HasError("Cannot attach a Port Transport Leg to a Run Sheet with Status: Canceled"));
			runSheet.ErrorStatus = CommonWorkSheet.ErrorStatuses.Working;
			leg.Validation.ValidateJU_EY_RunSheet();
			Assert(!leg.JU_EY_RunSheetInfo.HasErrors());
			Factory.Save();
			CommonWorkSheet runSheet2 = Factory.New<CommonWorkSheet>();
			runSheet2.EY_DriversName = "Bob2";
			runSheet2.ErrorStatus = CommonWorkSheet.ErrorStatuses.Canceled;
			leg.JU_EY_RunSheet = runSheet2.PK;
			Assert(leg.JU_EY_RunSheetInfo.HasError("Cannot attach a Port Transport Leg to a Run Sheet with Status: Canceled"));
		}

		public void TestJU_MessageStatus()
		{
			leg.JU_MessageStatus = "";
			AssertNoError(leg.JU_MessageStatusInfo, "Enter a valid Message Status.");
			leg.JU_MessageStatus = "t";
			AssertHasError(leg.JU_MessageStatusInfo, "Enter a valid Message Status.");
			leg.JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Futile;
			AssertNoError(leg.JU_MessageStatusInfo, "Enter a valid Message Status.");
		}

		class RunSheetSecurityQueryProviderForTest : IRunSheetSecurityQueryProvider
		{
			void IRunSheetSecurityQueryProvider.TryAuthorise(CommonCartageLeg leg)
			{
				if (!AuthoriseBy.IsEmpty)
				{
					leg.AuthoriseRunSheet(AuthoriseBy);
				}
			}

			public ZString AuthoriseBy { get; set; }
		}

		[TestDate(2010, 2, 4, 11, 34, 0)]
		public void TestCheckIfAWorkSheetExistsWithOverlappingTimeTruckDriver_SameTruckDriver()
		{
			ZDateTime now = ZDateTime.Now;
			RefEquipment truck = Factory.New<RefEquipment>();
			truck.RQ_Registration = "tr1";
			truck.RQ_ShortCode = "tr1";
			GlbGroup driversGroup = Factory.New<GlbGroup>();
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());
			GlbStaff driver = Factory.New<GlbStaff>();
			driver.GS_Code = "dr1";
			driver.GS_LoginName = "dr1";
			driver.Groups.Add(driversGroup);
			CommonWorkSheet sheet = Factory.New<CommonWorkSheet>();
			sheet.EY_StartTime = now;
			sheet.EY_EndTime = now.AddHours(7);
			CommonCartageLeg leg = sheet.CartageLegs.AddNew();
			leg.QuickPlannedPickupTime = now.AddHours(2);
			sheet.EY_RQ_Truck = truck.PK;
			sheet.EY_GS_NKTruckDriver = driver.GS_Code;
			Factory.Save();
			CommonWorkSheet overlappingSheet = Factory.New<CommonWorkSheet>();
			overlappingSheet.EY_StartTime = now;
			overlappingSheet.EY_EndTime = now.AddHours(7);
			CommonCartageLeg leg2 = overlappingSheet.CartageLegs.AddNew();
			leg2.JU_PlannedPickupTime = now.AddHours(1);
			overlappingSheet.EY_RQ_Truck = truck.PK;
			overlappingSheet.EY_GS_NKTruckDriver = driver.GS_Code;
			Factory.Save();
			leg.QuickRQTruck = truck.PK;
			leg.QuickGSDriver = driver.GS_Code;
			leg2.QuickRQTruck = truck.PK;
			leg2.QuickGSDriver = driver.GS_Code;
			leg.Validation.ValidateAll();
			leg2.Validation.ValidateAll();
			AssertHasErrors(leg2.JU_EY_RunSheetInfo);
			AssertHasErrors(leg.JU_EY_RunSheetInfo);
		}

		[TestDate(2010, 2, 4, 11, 34, 0)]
		public void TestCheckIfAWorkSheetExistsWithOverlappingTimeTruckDriver_EmptyDriver()
		{
			ZDateTime now = ZDateTime.Now;
			RefEquipment truck = Factory.New<RefEquipment>();
			truck.RQ_Registration = "tr1";
			truck.RQ_ShortCode = "tr1";
			CommonWorkSheet overlappingSheet = Factory.New<CommonWorkSheet>();
			overlappingSheet.EY_StartTime = now;
			overlappingSheet.EY_EndTime = now.AddHours(7);
			Factory.Save();
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			leg.JU_PlannedPickupTime = now.AddHours(1);
			leg.QuickRQTruck = truck.PK;
			AssertNoErrors(leg.QuickRQTruckInfo);
		}

		[TestDate(2010, 2, 4, 11, 34, 0)]
		public void TestCheckIfAWorkSheetExistsWithOverlappingTimeTruckDriver_EmptyTruck()
		{
			ZDateTime now = ZDateTime.Now;
			GlbGroup driversGroup = Factory.New<GlbGroup>();
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			transportRegistry.TransportDriversGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, driversGroup.PK.ToGuid());
			GlbStaff driver = Factory.New<GlbStaff>();
			driver.GS_Code = "dr1";
			driver.GS_LoginName = "dr1";
			driver.Groups.Add(driversGroup);
			CommonWorkSheet overlappingSheet = Factory.New<CommonWorkSheet>();
			overlappingSheet.EY_StartTime = now;
			overlappingSheet.EY_EndTime = now.AddHours(7);
			Factory.Save();
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			leg.JU_PlannedPickupTime = now.AddHours(1);
			leg.QuickGSDriver = driver.GS_Code;
			AssertNoErrors(leg.QuickGSDriverInfo);
		}

		public void TestValidateJU_CartagePickupDemurrage()
		{
			var cartageLeg = Factory.New<CommonCartageLeg>();
			AssertTimeCannotBeNegative((ZPropertyInfoDateTime)cartageLeg.JU_CartagePickupDemurrageInfo, "Pick Up Time should be within 00:00 and 999:59.");
		}

		public void TestValidateJU_CartageDeliveryDemurrage()
		{
			var cartageLeg = Factory.New<CommonCartageLeg>();
			AssertTimeCannotBeNegative((ZPropertyInfoDateTime)cartageLeg.JU_CartageDeliveryDemurrageInfo, "Delivery Time should be within 00:00 and 999:59.");
		}

		void AssertTimeCannotBeNegative(ZPropertyInfoDateTime timeInfo, string errorMessage)
		{
			timeInfo.Value = ZDateTime.Empty;
			AssertNoError("Should not have an error when time is empty.", timeInfo, errorMessage);
			// The bound time controls use the first day of the 1900 for the lower bound, so simulate this and add negative hours to obtain a value of "-01:00"
			timeInfo.Value = TimeSpan.FromHours(-1);
			AssertHasError(timeInfo, errorMessage);
			timeInfo.Value = TimeSpan.FromHours(1);
			AssertNoError(timeInfo, errorMessage);
			// The bound time controls use the 15:59 of Feburary 11th of the current year for the upper bound. In order to simulate this, add 1 hour to this time  
			timeInfo.Value = ZDateTime.DefaultDurationEpoch.AddHours(999).AddMinutes(59).AddSeconds(1);
			AssertHasError(timeInfo, errorMessage);
		}

		public void TestJU_DistanceUnit()
		{
			var leg = Factory.New<CommonCartageLeg>();
			leg.JU_DistanceUnit = "ZZ";
			AssertHasErrorContaining("Invalid distance unit", leg.JU_DistanceUnitInfo, "Enter a valid Distance Unit");
			leg.JU_DistanceUnit = Constants.Length.Miles;
			AssertNoErrors("Valid unit", leg.JU_DistanceUnitInfo);
			leg.JU_DistanceUnit = Constants.Length.Kilometres;
			AssertNoErrors("Valid unit", leg.JU_DistanceUnitInfo);
		}

		public void TestJU_DistanceUnitIsMandatory()
		{
			var leg = Factory.New<CommonCartageLeg>();
			leg.JU_DistanceUnit = Constants.Length.Kilometres;
			AssertNoErrors("Precondition: JU_DistanceUnit is populated with valid unit here", leg.JU_DistanceUnitInfo);
			leg.JU_DistanceUnit = string.Empty;
			AssertHasErrorContaining("Should have Is Mandatory error message", leg.JU_DistanceUnitInfo, "Please enter a Distance Unit");
		}

		protected override void SetUp()
		{
			base.SetUp();
			cartage = Factory.New<CommonCartage>();
			move = cartage.LooseBookedMoves.AddNew();
			leg = move.CartageLegs.AddNew();
		}

		CommonCartage cartage;
		CommonBookedCtgMove move;
		CommonCartageLeg leg;
	}
}
