using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(SupervisorOverrides))]
	public class SupervisorOverridesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckMessageErrors()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var user = Factory.New<GlbStaff>();
			user.FillWithValidTestData();
			user.GS_Code = "YYY";
			user.GS_LoginName = "admin";
			user.GS_IsController = true;
			user.StaffPlainTextPassword = "password";
			user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			group.Staff.Add(user);
			Factory.Save();

			var shipment = Factory.New<CusUSLVClearance>();
			var consignment1 = shipment.CusUSLVConsignments.AddNew();
			SetUpConsignmentWithValidTestData(consignment1);

			var consignment2 = shipment.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 10;
			consignment2.FirstCusUSLVItemTariff = "2517.10.0015";
			consignment2.FirstCusUSLVItemCountryOfOrigin = "AU";
			consignment2.FirstCusUSLVItemLineValue = 5.0;
			SetUpConsignmentWithValidTestData(consignment2);

			shipment.PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode.Add);
			shipment.RunPreSaveValidation();

			var supervisorOverrides = new SupervisorOverrides(shipment, SupervisorOverridesContext.SendingMessages);
			Env.Security.AllowMessageErrors.IsAllowed = false;
			supervisorOverrides.CreateMessages();
			AssertEquals(0, supervisorOverrides.UnAuthorisedMessagesForLog.Count);

			var consignmentsForMessaging = shipment.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>();
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment1.PK).SendToCustoms = true;
			supervisorOverrides = new SupervisorOverrides(shipment, SupervisorOverridesContext.SendingMessages);
			supervisorOverrides.CreateMessages();
			AssertEquals(1, supervisorOverrides.UnAuthorisedMessagesForLog.Count);

			supervisorOverrides = new SupervisorOverrides(consignment2, SupervisorOverridesContext.SendingMessages);
			supervisorOverrides.CreateMessages();
			AssertEquals(0, supervisorOverrides.UnAuthorisedMessagesForLog.Count);

			supervisorOverrides = new SupervisorOverrides(consignment1, SupervisorOverridesContext.SendingMessages);
			supervisorOverrides.CreateMessages();
			AssertEquals(1, supervisorOverrides.UnAuthorisedMessagesForLog.Count);
		}

		public void TestCheckMessageErrorsForCusUSLVClearanceMessageWrapper()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var user = Factory.New<GlbStaff>();
			user.FillWithValidTestData();
			user.GS_Code = "YYY";
			user.GS_LoginName = "admin";
			user.GS_IsController = true;
			user.StaffPlainTextPassword = "password";
			user.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			group.Staff.Add(user);
			Factory.Save();

			var shipment = Factory.New<CusUSLVClearance>();
			var consignment1 = shipment.CusUSLVConsignments.AddNew();
			SetUpConsignmentWithValidTestData(consignment1);

			var consignment2 = shipment.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 10;
			consignment2.FirstCusUSLVItemTariff = "2517.10.0015";
			consignment2.FirstCusUSLVItemCountryOfOrigin = "AU";
			consignment2.FirstCusUSLVItemLineValue = 5.0;
			SetUpConsignmentWithValidTestData(consignment2);

			shipment.PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode.Add);
			shipment.RunPreSaveValidation();

			var clearanceWrapper = new CusUSLVClearanceMessageWrapper(shipment);

			Env.Security.AllowMessageErrors.IsAllowed = false;
			var supervisorOverrides = new SupervisorOverrides(clearanceWrapper, SupervisorOverridesContext.SendingMessages);
			supervisorOverrides.CreateMessages();
			AssertEquals("Should check message errors for no consignments as none is marked as 'SendToCustoms'", 0, supervisorOverrides.UnAuthorisedMessagesForLog.Count);

			clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().ForEach(consignment => consignment.SendToCustoms = true);
			supervisorOverrides.CreateMessages();
			AssertEquals("Should check message errors for all consignments under parent clearance", 1, supervisorOverrides.UnAuthorisedMessagesForLog.Count);

			clearanceWrapper = new CusUSLVClearanceMessageWrapper(shipment, consignment2);
			clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().ForEach(consignment => consignment.SendToCustoms = true);
			supervisorOverrides = new SupervisorOverrides(clearanceWrapper, SupervisorOverridesContext.SendingMessages);
			supervisorOverrides.CreateMessages();
			AssertEquals("Should check message errors for one selected consignment", 0, supervisorOverrides.UnAuthorisedMessagesForLog.Count);

			clearanceWrapper = new CusUSLVClearanceMessageWrapper(shipment, new[] { consignment1, consignment2 });
			clearanceWrapper.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().ForEach(consignment => consignment.SendToCustoms = true);
			supervisorOverrides = new SupervisorOverrides(clearanceWrapper, SupervisorOverridesContext.SendingMessages);
			supervisorOverrides.CreateMessages();
			AssertEquals("Should check message errors for all selected consignments", 1, supervisorOverrides.UnAuthorisedMessagesForLog.Count);
		}

		#region Implementation

		CusUSLVClearance Clearance
		{
			get
			{
				if (clearance == null)
				{
					clearance = Factory.New<CusUSLVClearance>();
				}
				return clearance;
			}
		}
		CusUSLVClearance clearance;

		RefCusTaxOrFee deminimus;

		protected override void SetUp()
		{
			base.SetUp();
			if (deminimus == null)
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				deminimus = helper.CreateTaxOrFee("DEM", 1000m, Core.Constants.CountryCodes.UnitedStates);
				Factory.Save();
			}
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SupervisorOverrides(Clearance, SupervisorOverridesContext.SendingMessages);
		}

		#endregion

		void SetUpConsignmentWithValidTestData(CusUSLVConsignment consignment)
		{
			consignment.ULB_SellerName = "someone";
			consignment.ULB_SellerCity = "somewhere";
			consignment.ULB_SellerAddress1 = "somewhere";
			consignment.ULB_RN_NKSellerCountry = "AU";
			consignment.ULB_ConsigneeName = "Ian";
			consignment.ULB_ConsigneeCity = "syd";
			consignment.ULB_ConsigneeAddress1 = "xx";
			consignment.ULB_RN_NKConsigneeCountry = "AU";
		}

		#endregion
	}
}
