using CargoWise.eHub.Clients.GEO.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CargoWise.eHub.Clients.GEO.Tests
{
	[TestClass()]
	public class ScriptsTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetValueOfInvoiceNumberAndValueList()
		{
			string invoiceNumberAndValueList = "1110000385:C;1110000404:D;1110000909:;";
			AssertGetValueOfInvoiceNumberAndValueList("C", invoiceNumberAndValueList, "1110000385", 0);
			AssertGetValueOfInvoiceNumberAndValueList("", invoiceNumberAndValueList, "1110000385", 1);
			AssertGetValueOfInvoiceNumberAndValueList("D", invoiceNumberAndValueList, "1110000404", 0);
			AssertGetValueOfInvoiceNumberAndValueList("", invoiceNumberAndValueList, "1110000909", 1);
			AssertGetValueOfInvoiceNumberAndValueList("", invoiceNumberAndValueList, "1110000909", 0);
			AssertGetValueOfInvoiceNumberAndValueList("", "", "1110000385", 0);
			AssertGetValueOfInvoiceNumberAndValueList("", "", "1110000385", 1);

			invoiceNumberAndValueList = "1110000385:C;1110000404:D;1110000909;";
			AssertGetValueOfInvoiceNumberAndValueList("", invoiceNumberAndValueList, "1110000909", 0);

			invoiceNumberAndValueList = "1110000385:C;1110000404:D;1110000909:";
			AssertGetValueOfInvoiceNumberAndValueList("", invoiceNumberAndValueList, "1110000909", 0);

			invoiceNumberAndValueList = "1110000385:C;1110000404:D;1110000909";
			AssertGetValueOfInvoiceNumberAndValueList("", invoiceNumberAndValueList, "1110000909", 0);

			invoiceNumberAndValueList = "";
			AssertGetValueOfInvoiceNumberAndValueList("", invoiceNumberAndValueList, "1110000909", 0);

			invoiceNumberAndValueList = "1110000385:LA:826;1110000404:LB:999;";
			AssertGetValueOfInvoiceNumberAndValueList("LA", invoiceNumberAndValueList, "1110000385", 0);
			AssertGetValueOfInvoiceNumberAndValueList("826", invoiceNumberAndValueList, "1110000385", 1);
			AssertGetValueOfInvoiceNumberAndValueList("", invoiceNumberAndValueList, "1110000385", 2);
			AssertGetValueOfInvoiceNumberAndValueList("LB", invoiceNumberAndValueList, "1110000404", 0);
			AssertGetValueOfInvoiceNumberAndValueList("999", invoiceNumberAndValueList, "1110000404", 1);
			AssertGetValueOfInvoiceNumberAndValueList("", invoiceNumberAndValueList, "1110000404", 2);
		}

		void AssertGetValueOfInvoiceNumberAndValueList(string expected, string invoiceNumberAndValueList, string invoiceNumber, int indexOfValue)
		{
			string actual = TestScripts.GetValueOfInvoiceNumberAndValueList(invoiceNumberAndValueList, invoiceNumber, indexOfValue);
			Assert.AreEqual(expected, actual);
		}

		#region Implementation

		Scripts TestScripts
		{
			get
			{
				return testScripts ?? (testScripts = new Scripts());
			}
		}

		Scripts testScripts;

		#endregion
	}
}
