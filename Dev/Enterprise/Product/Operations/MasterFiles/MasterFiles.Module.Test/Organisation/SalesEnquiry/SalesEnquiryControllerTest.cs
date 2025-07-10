using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesEnquiryController))]
	sealed class SalesEnquiryControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(SalesEnquiry);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.SalesEnquiry;
		}

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<SalesEnquiry>();
			bizObjWithoutAccess.O1_GS_NKRepAssigned = "U00";
			CRMSecurityProviderTest<SalesEnquiry>.AssertController(new SalesEnquiryController(), bizObjWithoutAccess, Env.Security.InquiryManagerCRMSecurity);
		}

		#endregion

		public void TestUrlsCanBeOpenedByAnyCompany()
		{
			Assert("Enquiry hyperlinks should not be restricted to the current company", !Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
