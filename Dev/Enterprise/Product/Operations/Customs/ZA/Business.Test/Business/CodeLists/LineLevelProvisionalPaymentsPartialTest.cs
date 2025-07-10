using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ProvisionalPaymentTypesHelperTestCase : TestCase
	{
		public void TestTypeLists()
		{
			var tester = ProvisionalPaymentTypesHelper.GetTypesForRateType("PEN");
			AssertEquals(2, tester.Count());
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.FOR));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PEN));

			tester = ProvisionalPaymentTypesHelper.GetTypesForRateType("PRP");
			AssertEquals(6, tester.Count());
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPA));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPC));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPG));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPR));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPT));
			Assert(tester.Contains(HeaderLevelProvisionalPayments.Codes.PPE));

			tester = ProvisionalPaymentTypesHelper.GetTypesForRateType("DTY");
			AssertEquals(0, tester.Count());

			tester = ProvisionalPaymentTypesHelper.GetTypesForRateType("XXX");
			AssertEquals(0, tester.Count());

			tester = ProvisionalPaymentTypesHelper.GetTypesForRateType("");
			AssertEquals(0, tester.Count());
		}

		public void TestGetTypesForRateTypeCoversAll()
		{
			var allCodes = new LineLevelProvisionalPayments().GetAllCodes().Union(new HeaderLevelProvisionalPayments().GetAllCodes()).OrderBy(x => x).ToArray();
			var coveredCodes = ProvisionalPaymentTypesHelper.GetTypesForRateType(Universal.Constants.RateTypes.Penalty);
			coveredCodes = coveredCodes.Union(ProvisionalPaymentTypesHelper.GetTypesForRateType(Universal.Constants.RateTypes.ProvisionalPayment));
			AssertArrayEqualsByElements(allCodes, coveredCodes.OrderBy(x => x).Select(x => x.ToString()).ToArray());
		}

		public void TestGetAllCodes()
		{
			var factory = new BusinessObjectFactory();
			var tester = factory.GetAllProvisionalPaymentTypes();
			AssertEquals(8, tester.Count());
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.FOR));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PEN));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPA));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPC));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPG));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPR));
			Assert(tester.Contains(LineLevelProvisionalPayments.Codes.PPT));
			Assert(tester.Contains(HeaderLevelProvisionalPayments.Codes.PPE));
		}

		public void TestGetFullProvisionalPaymentTypeList()
		{
			var factory = new BusinessObjectFactory();
			var tester = factory.GetFullProvisionalPaymentTypeList();
			AssertEquals("FOR, PEN, PPA, PPR, PPC, PPE, PPG, PPT", tester.CodesAsString);
		}
	}
}
