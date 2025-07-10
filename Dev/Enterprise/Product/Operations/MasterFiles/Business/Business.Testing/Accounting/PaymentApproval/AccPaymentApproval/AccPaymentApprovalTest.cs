using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccPaymentApproval))]
	public class AccPaymentApprovalTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesAccPaymentApproval()
		{
			AccPaymentApproval approval = (AccPaymentApproval)Factory.New(GetExpectedBusinessObjectType());

			var exList = new List<string>
				{
					nameof(approval.AV_PayExRate)
				};

			var tester = new DecimalPlacesAttributeTester(approval);
			tester.CheckExchangeRate(exList, nameof(approval.ExchangeRateDecimals));
		}

		public void TestThreeApprovalInfosAreReadOnly()
		{
			AccPaymentApproval approval = (AccPaymentApproval)Factory.New(GetExpectedBusinessObjectType());

			AssertEquals("FirstApprovalInfo should be readonly", true, approval.AV_GS_NKApproval1stInfo.ReadOnly);
			AssertEquals("SecondApprovalInfo should be readonly", true, approval.AV_GS_NKApproval2ndInfo.ReadOnly);
			AssertEquals("ThirdApprovalInfo should be readonly", true, approval.AV_GS_NKApproval3rdInfo.ReadOnly);
		}

		public void TestAV_Code()
		{
			AccPaymentApproval approval = (AccPaymentApproval)Factory.New(GetExpectedBusinessObjectType());
			AssertEquals(approval.PK.ToStringKey(), approval.AV_Code);
		}

		public void TestHumanReadableShortcutName()
		{
			var approval = (AccPaymentApproval)Factory.New(GetExpectedBusinessObjectType());

			approval.AV_OH = ZGuid.Empty;
			approval.AV_PaymentDate = ZDateTime.Empty;
			approval.AV_PaymentComment = ZString.Empty;
			AssertEquals(string.Empty, approval.HumanReadableShortcutName);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "Test Code";
			approval.AV_OH = org.PK;

			AssertEquals("Test Code", approval.HumanReadableShortcutName);

			approval.AV_PaymentDate = new ZDateTime(1979, 12, 12);
			AssertEquals("Test Code - 12-Dec-79", approval.HumanReadableShortcutName);

			approval.AV_PaymentComment = "Comment";
			AssertEquals("Test Code - 12-Dec-79 - Comment", approval.HumanReadableShortcutName);
		}
	}
}
