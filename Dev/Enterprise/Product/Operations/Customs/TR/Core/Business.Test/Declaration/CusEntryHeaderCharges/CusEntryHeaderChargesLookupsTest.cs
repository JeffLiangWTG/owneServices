using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class CusEntryHeaderChargesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentTypesList()
		{
			cusEntryHeaderCharges.C1_ChargeType = "EXU";

			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(PaymentTypeCodesList.Codes.K, PaymentTypeCodesList.Descriptions.K),
				new CodeDescriptionPair(PaymentTypeCodesList.Codes.C, PaymentTypeCodesList.Descriptions.C),
			}, lookups.PaymentTypeCodesList);
		}

		public void TestPaymentTypesListForStampDuty()
		{
			cusEntryHeaderCharges.C1_ChargeType = "89";

			var paymentTypeCodesList = lookups.PaymentTypeCodesList;

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
			}, lookups.PaymentTypeCodesList);
		}

		public void TestPaymentTypesListChargeTypeEmpty()
		{
			cusEntryHeaderCharges.C1_ChargeType = ZString.Empty;
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<CodeDescriptionPairList>(), lookups.PaymentTypeCodesList);
		}

		public void TestTaxOrFeeCodeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateTaxOrFee("TR1", 0.1, Core.Constants.CountryCodes.Turkey, 0.1, 0.1, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				helper.CreateTaxOrFee("TR2", 0.2, Core.Constants.CountryCodes.Turkey, 0.2, 0.2, "OTH", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				helper.CreateTaxOrFee("TR3", 0.3, Core.Constants.CountryCodes.Turkey, 0.3, 0.3, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				Factory.Save();

				AssertEquals("TR1, TR2", lookups.TaxOrFeeCodeList.CodesAsString);
			}
		}

		public void TestRateOverrideReasonCodeList()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair(RateOverrideReasonCodeList.Codes.Additional, RateOverrideReasonCodeList.Descriptions.Additional);
			expectedList.AddPair(RateOverrideReasonCodeList.Codes.Override, RateOverrideReasonCodeList.Descriptions.Override);

			AssertEquals("Should have 2 element", 2, lookups.RateOverrideReasonCodeList.Count);
			AssertContainsExactElementsInAnyOrder("RateOverrideReasonCodeList should have OVR and ADD", expectedList, lookups.RateOverrideReasonCodeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusEntryHeaderCharges = Factory.New<CusEntryHeaderCharges>();
			lookups = cusEntryHeaderCharges.Lookups;
		}

		protected CusEntryHeaderCharges cusEntryHeaderCharges;
		protected CusEntryHeaderChargesLookups lookups;
	}
}
