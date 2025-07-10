using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	sealed class JobInvoicingSecurityHelperTest : TestCaseWithFactory
	{
		public void TestGetSecurityCheckPoint()
		{
			JobInvoicingSecurityHelper securityTestHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			SecurityCheckpoint checkPoint = securityTestHelper.GetInvSecurity(SecurityCore.PostDSB);
			AssertNotNull(checkPoint);
			AssertEquals("MaintainShipmentJobInvoicingInvPostDSB", checkPoint.Code);

			securityTestHelper = new JobInvoicingSecurityHelper((SecurityCheckpoint)null);
			checkPoint = securityTestHelper.GetInvSecurity(SecurityCore.PostDSB);
			AssertNull(checkPoint);
		}

		public void TestGetIsAllowedForSecurityCheckPoint()
		{
			JobInvoicingSecurityHelper securityTestHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			SecurityCheckpoint checkPoint = securityTestHelper.GetInvSecurity(SecurityCore.PostDSB);
			checkPoint.IsAllowed = false;

			Assert(!securityTestHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostDSB));

			securityTestHelper = new JobInvoicingSecurityHelper((SecurityCheckpoint)null, false);
			Assert(!securityTestHelper.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PostDSB));
		}

		public void TestGetErrorTextForSecurityCheckPoint()
		{
			JobInvoicingSecurityHelper securityTestHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			SecurityCheckpoint checkPoint = securityTestHelper.GetInvSecurity(SecurityCore.PostDSB);
			checkPoint.IsAllowed = false;

			string errorText = securityTestHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.PostDSB);
			string expectedErrorText = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Shipments -> Billing -> Invoicing -> Post Disbursement Charges";
			AssertEquals(expectedErrorText, errorText);

			securityTestHelper = new JobInvoicingSecurityHelper((SecurityCheckpoint)null, false);
			errorText = securityTestHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.PostDSB);
			AssertEquals(securityTestHelper.securityNotFoundErrorMessage, errorText);
		}

		public void TestShowError()
		{
			JobInvoicingSecurityHelper securityTestHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainShipmentJobInvoicing);
			SecurityCheckpoint checkPoint = securityTestHelper.GetInvSecurity(SecurityCore.PostDSB);
			checkPoint.IsAllowed = false;

			string errorText = securityTestHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.PostDSB);

			securityTestHelper.ShowError(SecurityCore.PostDSB);
			AssertEquals(errorText, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestShowErrorWithEmptySecurity()
		{
			new JobInvoicingSecurityHelper((SecurityCheckpoint)null, false).ShowError(SecurityCore.PostDSB);
		}
	}
}
