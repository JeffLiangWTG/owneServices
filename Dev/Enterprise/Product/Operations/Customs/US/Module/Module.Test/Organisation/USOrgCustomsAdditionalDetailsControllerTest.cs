using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USOrgBuyerSupplierLinkAdditionalCustomsDetailsController))]
	sealed class USOrgCustomsAdditionalDetailsControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var org = Factory.New<OrgHeader>();
			var controller = new USOrgBuyerSupplierLinkAdditionalCustomsDetailsController();
			AssertEquals(controller.GetCheckPointForDelete(org), Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults);
			AssertEquals(controller.GetCheckPointForEdit(org), Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults);
			AssertEquals(controller.GetCheckPointForNew(org), Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults);
			AssertEquals(controller.GetCheckPointForView(org), Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.OrgBuyerSupplierLinkAdditionalCustomsDetails;

		protected override Type GetBusinessObjectType() => typeof(OrgHeader);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			Factory.Save();
			return bizO;
		}
	}
}
