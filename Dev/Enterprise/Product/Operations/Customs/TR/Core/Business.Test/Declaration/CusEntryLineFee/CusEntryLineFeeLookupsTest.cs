using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class CusEntryLineFeeLookupsTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestMethodOfPaymentList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(MethodOfPaymentList.Codes.B, MethodOfPaymentList.Descriptions.B),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.C, MethodOfPaymentList.Descriptions.C),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.D, MethodOfPaymentList.Descriptions.D),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.E, MethodOfPaymentList.Descriptions.E),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.G, MethodOfPaymentList.Descriptions.G),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.J, MethodOfPaymentList.Descriptions.J),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.L, MethodOfPaymentList.Descriptions.L),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.M, MethodOfPaymentList.Descriptions.M),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.N, MethodOfPaymentList.Descriptions.N),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.P, MethodOfPaymentList.Descriptions.P),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.R, MethodOfPaymentList.Descriptions.R),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.Y, MethodOfPaymentList.Descriptions.Y),
				new CodeDescriptionPair(MethodOfPaymentList.Codes.Z, MethodOfPaymentList.Descriptions.Z),
			}, lookups.MethodOfPaymentList);
		}

		public void TestMethodOfCalculationList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(MethodOfCalculationList.Codes.CW1, MethodOfCalculationList.Descriptions.CW1),
				new CodeDescriptionPair(MethodOfCalculationList.Codes.Gumruk, MethodOfCalculationList.Descriptions.Gumruk),
			}, lookups.MethodOfCalculationList);
		}

		public void TestNationalFeeTypeCodeList()
		{
			var typeList = lookups.NationalFeeTypeCodeList;
			AssertEquals("CodesAsString", "40", typeList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusEntryLineFee = Factory.New<CusEntryLineFee>();
			lookups = (CusEntryLineFeeLookups)cusEntryLineFee.Lookups;
		}

		protected CusEntryLineFee cusEntryLineFee;
		protected CusEntryLineFeeLookups lookups;
	}
}
