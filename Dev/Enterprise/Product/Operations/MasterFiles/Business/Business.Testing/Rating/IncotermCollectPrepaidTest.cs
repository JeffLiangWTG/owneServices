using System.Linq;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IncotermCollectPrepaidTest : TestCase
	{
		public void TestIsCollect()
		{
			foreach (var inco in collectIncos)
			{
				AssertEquals(Constants.PaymentType.Collect, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, inco));
			}

			foreach (var inco in prepaidIncos)
			{
				AssertEquals(Constants.PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, inco));
			}
		}

		public void TestIsPrepaid()
		{
			foreach (var inco in collectIncos)
			{
				AssertEquals(Constants.PaymentType.Collect, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, inco));
			}

			foreach (var inco in prepaidIncos)
			{
				AssertEquals(Constants.PaymentType.Prepaid, IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, inco));
			}
		}

		public void TestGetPaymentType()
		{
			foreach (var inco in collectIncos)
			{
				AssertEquals(inco, "CCX", GetPaymentType(inco));
			}

			foreach (var inco in prepaidIncos)
			{
				AssertEquals(inco, "PPD", GetPaymentType(inco));
			}
		}

		public void TestGetPaymentTypeCoverage()
		{
			foreach (var inco in Constants.IncoTerms.Incoterms2000.Union(Constants.IncoTerms.Incoterms2010))
			{
				AssertNotEquals("", GetPaymentType(inco));
			}
		}

		readonly string[] collectIncos = { "EXW", "FCA", "FAS", "FOB" };
		readonly string[] prepaidIncos = { "CFR", "CIF", "CPT", "CIP", "DDP", "DAF", "DES", "DEQ", "DDU", "DAP", "DAT" };

		static string GetPaymentType(string incoterm)
			=> IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, incoterm);
	}
}
