using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CPT_116_PaymentMethodList))]
	sealed class CPT_116_PaymentMethodListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetPaymentMethodList()
		{
			foreach (var controllingAgency in new string[] { "20", "CI", "2Q", "DN", "VP", "CD", "IF", "DH", "AA" })
			{
				var paymentMethodList1 = CPT_116_PaymentMethodList.GetPaymentMethodList(Factory, controllingAgency);
				var paymentMethodList2 = CPT_116_PaymentMethodList.GetPaymentMethodList(Factory, controllingAgency);
				NUnit.Framework.Assert.That(paymentMethodList1, NUnit.Framework.Is.EqualTo(paymentMethodList2), "Should have cached.");
				if (controllingAgency == "20" || controllingAgency == "CI" || controllingAgency == "2Q")
				{
					NUnit.Framework.Assert.That(paymentMethodList1.Count, NUnit.Framework.Is.EqualTo(8), "The count of PaymentMethodList is 8 when Controlling Agency is 20, CI, or 2Q");
					NUnit.Framework.Assert.That(paymentMethodList1.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3, 4, 5, 6, 7, 8"));
					NUnit.Framework.Assert.That(paymentMethodList1.ElementsAsString, NUnit.Framework.Is.EqualTo(@"1 - 現金
2 - 銀行本票或即期支票
3 - 郵局匯票
4 - 使用擔保額度銀行帳戶扣款
5 - 匯款
6 - e政府繳費平台
7 - 轉帳
8 - 虛擬帳號繳款"));
				}
				else if (controllingAgency == "DN")
				{
					NUnit.Framework.Assert.That(paymentMethodList1.Count, NUnit.Framework.Is.EqualTo(2), "The count of PaymentMethodList is 2 when Controlling Agency is DN");
					NUnit.Framework.Assert.That(paymentMethodList1.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2"));
					NUnit.Framework.Assert.That(paymentMethodList1.ElementsAsString, NUnit.Framework.Is.EqualTo(@"1 - 銀行繳款
2 - 自動扣款"));
				}
				else if (controllingAgency == "VP")
				{
					NUnit.Framework.Assert.That(paymentMethodList1.Count, NUnit.Framework.Is.EqualTo(4), "The count of PaymentMethodList is 4 when Controlling Agency is VP");
					NUnit.Framework.Assert.That(paymentMethodList1.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3, 4"));
					NUnit.Framework.Assert.That(paymentMethodList1.ElementsAsString, NUnit.Framework.Is.EqualTo(@"1 - 臨櫃
2 - 銀行轉帳
3 - 虛擬帳號
4 - 指定帳戶扣款"));
				}
				else if (controllingAgency == "CD" || controllingAgency == "IF" || controllingAgency == "DH")
				{
					NUnit.Framework.Assert.That(paymentMethodList1.Count, NUnit.Framework.Is.EqualTo(7), "The count of PaymentMethodList is 7 when Controlling Agency is CD , IF or DH");
					NUnit.Framework.Assert.That(paymentMethodList1.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 21, 3, 4, 5, 6"));
					NUnit.Framework.Assert.That(paymentMethodList1.ElementsAsString, NUnit.Framework.Is.EqualTo(@"1 - 現金
2 - 本地支票
21 - 外埠支票
3 - 匯票
4 - 信用擔保額度
5 - 銀行轉帳
6 - 多元繳費"));
				}
				else
				{
					NUnit.Framework.Assert.That(paymentMethodList1.Count, NUnit.Framework.Is.EqualTo(0), "The count of PaymentMethodList is 0 when Controlling Agency is not 20, CI, 2Q, DN, VP, CD, IF or DH");
				}
			}
		}
	}
}
