using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocSGRefundInfo))]
	sealed class DocSGRefundInfoTest : DocumentWrapperTestCase
	{
		public void TestConsignmentDetails()
		{
			AssertEquals(typeof(DocRefundInfoConsignmentDetails), docSGRefundInfo.ConsignmentDetails.TypeOfElements);
		}

		public void TestDateOfApproval()
		{
			AssertEquals("", docSGRefundInfo.DateOfApproval);
		}

		public void TestDeclarantCode()
		{
			AssertEquals("1619574Z", docSGRefundInfo.DeclarantCode);
		}

		public void TestDeclarantName()
		{
			AssertEquals("USER47", docSGRefundInfo.DeclarantName);
		}

		public void TestEntityIdentifier()
		{
			AssertEquals("XXXXXXXXE47T", docSGRefundInfo.EntityIdentifier);
		}

		public void TestNameOfCompany()
		{
			AssertEquals("TESTING47", docSGRefundInfo.NameOfCompany);
		}

		public void TestPermitNumber()
		{
			AssertEquals("ID6I100233B", docSGRefundInfo.RefundNumber);
		}

		public void TestReasonForRefund()
		{
			AssertEquals(0, docSGRefundInfo.ReasonForRefund.Count);
		}

		public void TestRefundMessage()
		{
			AssertEquals(0, docSGRefundInfo.RefundMessage.Count);
		}

		public void TestReplacementNumber()
		{
			AssertEquals("", docSGRefundInfo.ReplacementNumber);
		}

		public void TestTelNo()
		{
			AssertEquals("12345678", docSGRefundInfo.TelNo);
		}

		public void TestUniqueRef()
		{
			AssertEquals("XXXXXXXXE47T        200609220234", docSGRefundInfo.UniqueRef);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Refund Information", docSGRefundInfo.HumanReadableName);
			AssertEquals("Refund Information", docSGRefundInfo.ToString());
		}

		public void TestTotalPayableValue()
		{
			var permit = new InPaymentUpdatePermit();
			permit.RefundOnly = new RefundOnly();
			permit.RefundOnly.RefundSummary = new RefundOnlyRefundSummary();

			var totalTariffRefund = permit.RefundOnly.RefundSummary.TotalTariffRefund = new RefundOnlyRefundSummaryTotalTariffRefund();
			totalTariffRefund.TotalCustomsDutyRefundAmount = 100m;
			totalTariffRefund.TotalExciseDutyRefundAmount = 98.35m;
			totalTariffRefund.TotalOtherTaxRefundAmount = 0m;
			totalTariffRefund.TotalGoodsAndServicesTaxRefundAmount = 188992.5m;

			var refundInfo = DocSGRefundInfo.New(new RefundInfo(new BaseTradeNetRefund(permit), Factory), Factory);

			CombineAssertions(() =>
			{
				AssertEquals("100.00".PadLeft(16, ' '), refundInfo.TotalCustomsDutyRefundAmount);
				AssertEquals("98.35".PadLeft(16, ' '), refundInfo.TotalExciseDutyRefundAmount);
				AssertEquals("0.00".PadLeft(16, ' '), refundInfo.TotalOtherTaxRefundAmount);
				AssertEquals("188992.50".PadLeft(16, ' '), refundInfo.TotalGoodsAndServicesTaxRefundAmount);
			});
		}

		#region Implementation

		DocSGRefundInfo docSGRefundInfo
		{
			get
			{
				if (fDocSGRefundInfo == null)
				{
					fDocSGRefundInfo = DocSGRefundInfo.New(RefundInfo, Factory);
				}
				return fDocSGRefundInfo;
			}
		}
		DocSGRefundInfo fDocSGRefundInfo;

		RefundInfo RefundInfo
		{
			get
			{
				if (fRefundInfo == null)
				{
					IPTUPT iptupt = new IPTUPT();
					ZString strSample = "UNH+JTID0609220204+CUSDEC:D:05B:UN:040+IPTUPD'BGM+914:::DNG+XXXXXXXXE47T        200609220234+13'CST++2+AME'LOC+9+ESBCN'LOC+11+KZ:::KEPPEL FTZ,KEPPEL ROAD, SINGAPORE'LOC+88+KW:::KEPPEL WHARVES'LOC+164+PPW:::PASIR PANJANG WHARVES'DTM+178:20060924:102'GEI+5+:Y'MEA+ABK++CAR:13.0000'MEA+AAH++TNE:0.0900'EQD+CN+OOLU7462538:1+:::LCL40001'SEL+L356612'FTX+AAI+++SHIPPER SEAL ?:-  L356612/ES11393'FTX+ACF+++AMENDMENT OF ARRIVAL DATE'RFF+DM:IP03H0407'RFF+MS:E47T.E47T001'RFF+ABT:ID6I100233B'TDT+3++1+++++36E36:::SANDRA AZUL'DOC+704+OOLU62238710'NAD+AE+XXXXXXXXE47T++TESTING47'NAD+CG+11878780000H++ORIENT OVERSEAS CONTAINER LINE LTD'NAD+IM+XXXXXXXXE47T++TESTING47'NAD+BB'RFF+DAN:D'NAD+DT++USER47'CTA+IC+:1619574Z'COM+12345678:TE'NAD+FW+XXXXXXXXE47T++TESTING47'UNS+D'DMS+INVOICE DETAILS'MOA+39:488.28:EUR'CUX+++2.000000'TOD+++CIF'NAD+SU++1+SUPPLIER NAME'DOC+380+2.438'DTM+3:20060828:102'ALC+C'MOA+304:9.77:SGD'PCD+5:1.000'CST+1+22042111'FTX+AAA+++CORTE REAL PLATINUM:(3 BOT BOX) PROMOTION ITEM'FTX+PRD+++BODEGASVINAEXTREMENA:NA'LOC+27+ES'MEA+AAF++LTR:29.2500'MEA+AAE++LTR:29.2500'MEA+AAI++LTR:0.7500'MEA+ABA++LTR:29.2500'PAC+13+3+CAR'PAC+2+1+BOT'MOA+63:976.56'RFF+IV:2.438'RFF+AEA:ZBP0BH0QVDG'DOC+703+ESBSGS609301H03'TAX+5+:::PRF+++LTR:::95'MOA+161:277.88'TAX+7'MOA+124:62.72'UNS+S'CNT+5:1'CNT+6:1'TAX+1'MOA+63:976.56'TAX+5'MOA+161:277.88'TAX+7'MOA+124:62.72'TAX+2'MOA+9:340.60'UNT+70+JTID0609220204'UNZ+1+24386'";
					iptupt.Parse(new UNOACharacterSet(), strSample);
					fRefundInfo = new RefundInfo(iptupt, Factory);
				}
				return fRefundInfo;
			}
		}
		RefundInfo fRefundInfo;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			DocSGRefundInfo result = DocSGRefundInfo.New(RefundInfo, Factory);
			return new DocumentWrapper[] { result };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocSGRefundInfo.New(RefundInfo, Factory);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			base.SetUp();
		}

		#endregion
	}
}
