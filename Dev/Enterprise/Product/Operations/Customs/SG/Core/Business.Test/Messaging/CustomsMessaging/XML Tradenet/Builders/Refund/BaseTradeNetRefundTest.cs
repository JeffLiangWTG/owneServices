using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class BaseTradeNetRefundTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPermitNumber()
		{
			AssertEquals("RFM2020090895", Refund.PermitNumber);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplacementNumber()
		{
			AssertEquals("DP0I105282X", Refund.ReplacementNumber);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNameOfCompany()
		{
			AssertEquals(GlbCompany.CurrentCompany.CompanyName, Refund.NameOfCompany);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEntityIdentifier()
		{
			AssertEquals("XXXXXXXXE47T", Refund.EntityIdentifier);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeclarantName()
		{
			AssertEquals("QA TEST USER 1", Refund.DeclarantName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeclarantCode()
		{
			AssertEquals("XXX11116", Refund.DeclarantCode);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTelNo()
		{
			AssertEquals("+6588889999", Refund.TelNo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDateOfApproval()
		{
			AssertEquals(new ZDate(2011, 2, 14), Refund.DateOfApproval);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUniqueRef()
		{
			AssertEquals("199702247W 20200520 0800", Refund.UniqueRef);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReasonForRefund()
		{
			AssertArrayEqualsByElements(new[] { "RF35 - TEST" }, Refund.ReasonForRefund.Select(c => $"{c.Code} - {c.Message}").ToArray());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRefundMessage()
		{
			var expectedMessage = new[] { "<b><ExpandToFit>A00 </b> - APPROVED BY SINGAPORE CUSTOMS.", "<b><ExpandToFit>C02 </b> - REFUND APPROVED BY SINGAPORE CUSTOMS.", };
			AssertArrayEqualsByElements(expectedMessage, Refund.RefundMessage.Select(c => $"{c.Code}{c.Message}").ToArray());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOVRRefund()
		{
			var message = Factory.New<SGXmlEDIMessage>();
			var path = BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\XML\IPTUPT_ForOVR.XML";
			message.EM_MessageText = System.IO.File.ReadAllText(path);
			Factory.Save();
			inPaymentUpdatePermit = message.TradenetResponse.OutboundMessage.InPaymentUpdatePermit;
			AssertArrayEqualsByElements(new[] { "RF37 - OVERPAYMENT" }, Refund.ReasonForRefund.Select(c => $"{c.Code} - {c.Message}").ToArray());

			var expectedMessage = new[] { "<b><ExpandToFit>A00 </b> - APPROVED BY SINGAPORE CUSTOMS.", "<b><ExpandToFit>C02 </b> - REFUND APPROVED BY SINGAPORE CUSTOMS.", };
			AssertArrayEqualsByElements(expectedMessage, Refund.RefundMessage.Select(c => $"{c.Code}{c.Message}").ToArray());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConsignmentDetails()
		{
			Assert("Should always be empty.", !Refund.ConsignmentDetails.Any());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalGoodsAndServicesTaxRefundAmount()
		{
			AssertEquals(98.64m, Refund.TotalGoodsAndServicesTaxRefundAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalExciseDutyRefundAmount()
		{
			AssertEquals(0.32m, Refund.TotalExciseDutyRefundAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalCustomsDutyRefundAmount()
		{
			AssertEquals(95.32m, Refund.TotalCustomsDutyRefundAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalOtherTaxRefundAmount()
		{
			AssertEquals(1000.00m, Refund.TotalOtherTaxRefundAmount);
		}

		BaseTradeNetRefund Refund => refund ?? (refund = new BaseTradeNetRefund(InPaymentUpdatePermit));
		BaseTradeNetRefund refund;

		InPaymentUpdatePermit InPaymentUpdatePermit
		{
			get
			{
				if (inPaymentUpdatePermit == null)
				{
					var message = Factory.New<SGXmlEDIMessage>();
					var path = BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\XML\IPTUPT.XML";
					message.EM_MessageText = System.IO.File.ReadAllText(path);
					Factory.Save();
					inPaymentUpdatePermit = message.TradenetResponse.OutboundMessage.InPaymentUpdatePermit;
				}

				return inPaymentUpdatePermit;
			}
		}

		InPaymentUpdatePermit inPaymentUpdatePermit;
	}
}
