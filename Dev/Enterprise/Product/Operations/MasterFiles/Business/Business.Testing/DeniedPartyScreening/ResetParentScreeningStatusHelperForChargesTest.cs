using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ResetParentScreeningStatusHelperForChargesTest : TestCaseWithFactory
	{
		public void TestAssertAndEnsureSingletonForIResetParentScreeningStatusHelperForCharges()
		{
			var helper = ObjectFactory.Get<IResetParentScreeningStatusHelperForCharges>();
			AssertEquals(typeof(ResetParentScreeningStatusHelperForCharges), helper.GetType());
			var helperFromSecondCall = ObjectFactory.Get<IResetParentScreeningStatusHelperForCharges>();
			AssertEquals(helper, helperFromSecondCall);
			var helperType = typeof(ResetParentScreeningStatusHelperForCharges);

			var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			Assert("Singleton implementation should be stateless", helperType.GetFields(bindingFlags).Length == 0);
			Assert("Singleton implementation should be stateless", helperType.GetProperties(bindingFlags).Length == 0);
		}

		public void TestResetParentScreeningStatusWithoutChangeOrgChange()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 100m;
			Factory.Save();

			var shipment = Factory.New<IForwardingShipment>();
			charge.JR_DisplaySequence = 1;
			new ResetParentScreeningStatusHelperForCharges().ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge });
			AssertEquals(false, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);
		}

		public void TestResetParentScreeningStatusWithChangeOrgChange()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 100m;
			Factory.Save();

			var shipment = Factory.New<IForwardingShipment>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			charge.JR_OH_CostAccount = org.PK;
			new ResetParentScreeningStatusHelperForCharges().ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge });
			AssertEquals(true, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);
		}

		public void TestResetParentScreeningStatus_ChangeWithOrgDeleted()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var jobHeader = Factory.Load<JobHeader>(charge.JR_JH);
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.IsManuallyCreated = true;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 100m;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			charge.JR_OH_CostAccount = org.PK;
			charge.JR_JH = jobHeader.PK;
			Factory.Save();
			charge.Delete();
			AssertEquals(true, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);
		}

		public void TestResetParentScreeningStatus_ChangeWithOrgAdded()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 100m;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			charge.JR_OH_CostAccount = org.PK;

			var shipment = Factory.New<IForwardingShipment>();
			new ResetParentScreeningStatusHelperForCharges().ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge });
			AssertEquals(true, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);
		}

		public void TestResetParentScreeningStatus_ChangeWithoutOrgDeleted()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 100m;
			Factory.Save();

			charge.Delete();
			var shipment = Factory.New<IForwardingShipment>();
			new ResetParentScreeningStatusHelperForCharges().ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge });
			AssertEquals(false, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);
		}

		public void TestResetParentScreeningStatusWithoutIncludeInvoicingPartiesParent()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 100m;
			Factory.Save();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			charge.JR_OH_CostAccount = org.PK;
			var consol = Factory.New<IForwardingConsol>();
			new ResetParentScreeningStatusHelperForCharges().ResetJobParentScreeningStatus(true, consol as IJobHeaderParent, new[] { charge });
			AssertEquals(false, ((IShouldUpdateScreeningStatus)consol).ShouldUpdateScreeningStatus);
		}

		public void TestResetParentScreeningStatusForJobClearedShipment()
		{
			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			charge1.JR_AC = chargeCode.PK;
			charge1.JR_OSCostAmt = 100m;
			charge1.JR_OSSellAmt = 100m;
			Factory.Save();
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			charge1.JR_OH_CostAccount = org.PK;
			var helper = new ResetParentScreeningStatusHelperForCharges();
			helper.ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge1 });
			AssertEquals(false, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			charge1.JR_OH_CostAccount = org.PK;
			helper.ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge1 });
			AssertEquals(false, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			charge1.JR_OH_CostAccount = org.PK;
			helper.ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge1 });
			AssertEquals(false, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			charge1.JR_OH_CostAccount = org.PK;
			helper.ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge1 });
			AssertEquals(true, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);

			((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus = false;
			charge1.JR_OH_CostAccount = ZGuid.Empty;
			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			charge1.JR_OH_SellAccount = org.PK;
			helper.ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge1 });
			AssertEquals(false, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			charge1.JR_OH_SellAccount = org.PK;
			helper.ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge1 });
			AssertEquals(false, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			charge1.JR_OH_SellAccount = org.PK;
			helper.ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge1 });
			AssertEquals(false, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);

			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			charge1.JR_OH_CostAccount = org.PK;
			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			charge1.JR_OH_SellAccount = org.PK;
			helper.ResetJobParentScreeningStatus(true, shipment as IJobHeaderParent, new[] { charge1 });
			AssertEquals(true, ((IShouldUpdateScreeningStatus)shipment).ShouldUpdateScreeningStatus);
		}
	}
}
