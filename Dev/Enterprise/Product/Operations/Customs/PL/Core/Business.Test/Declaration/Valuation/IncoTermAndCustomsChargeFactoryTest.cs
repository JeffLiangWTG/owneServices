using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class IncoTermAndCustomsChargeFactoryTest : EUIncoTermAndCustomsChargeFactoryTest
{
	public override void TestFactoryType()
	{
		AssertEquals(typeof(IncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
	}

	public override void TestGetAllIncoTerms()
	{
		AssertEquals("Count", 17, incoTermAndChargeFactory.GetAllIncoTerms().Length);
	}

	public override void TestGetAllCharges()
	{
		var expectedCharges = new[]
		{
			"AB",
			"AD",
			"AE",
			"AF",
			"AG",
			"AH",
			"AI",
			"AJ",
			"AK",
			"AL",
			"AN",
			"BA",
			"BB",
			"BC",
			"BD",
			"BE",
			"BF",
			"BG",
			"071",
			"072",
			"073",
			"074",
			"080",
			"1ST",
			"2ST"
		};

		AssertContainsExactElementsInAnyOrder(expectedCharges, incoTermAndChargeFactory.GetAllCharges().Select(_ => _.Code));
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"\Enterprise\Product\Operations\Customs\PL\Core\Business.Test\Declaration\Valuation\TestFiles\IncoTermAndCustomsChargeConfiguration.csv";

	public override void TestGetCharge()
	{
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AL, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AL, PLCustomsChargeTypeList.Descriptions.AL));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AB, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AB, PLCustomsChargeTypeList.Descriptions.AB));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AD, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AD, PLCustomsChargeTypeList.Descriptions.AD));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AE, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AE, PLCustomsChargeTypeList.Descriptions.AE));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AF, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AF, PLCustomsChargeTypeList.Descriptions.AF));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AG, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AG, PLCustomsChargeTypeList.Descriptions.AG));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AH, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AH, PLCustomsChargeTypeList.Descriptions.AH));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AI, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AI, PLCustomsChargeTypeList.Descriptions.AI));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AJ, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AJ, PLCustomsChargeTypeList.Descriptions.AJ));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AK, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AK, PLCustomsChargeTypeList.Descriptions.AK));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.AN, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.AN, PLCustomsChargeTypeList.Descriptions.AN));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.BA, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.BA, PLCustomsChargeTypeList.Descriptions.BA));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.BB, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.BB, PLCustomsChargeTypeList.Descriptions.BB));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.BD, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.BD, PLCustomsChargeTypeList.Descriptions.BD));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.BE, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.BE, PLCustomsChargeTypeList.Descriptions.BE));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.BF, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.BF, PLCustomsChargeTypeList.Descriptions.BF));
		AssertGetCharge(PLCustomsChargeTypeList.Codes.BG, new CustomsChargeCode(PLCustomsChargeTypeList.Codes.BG, PLCustomsChargeTypeList.Descriptions.BG));
		AssertGetCharge(PLCustomsChargeTypeList.Codes._071V, new CustomsChargeCode(PLCustomsChargeTypeList.Codes._071V, PLCustomsChargeTypeList.Descriptions._071V));
		AssertGetCharge(PLCustomsChargeTypeList.Codes._072X, new CustomsChargeCode(PLCustomsChargeTypeList.Codes._072X, PLCustomsChargeTypeList.Descriptions._072X));
		AssertGetCharge(PLCustomsChargeTypeList.Codes._073V, new CustomsChargeCode(PLCustomsChargeTypeList.Codes._073V, PLCustomsChargeTypeList.Descriptions._073V));
		AssertGetCharge(PLCustomsChargeTypeList.Codes._074A, new CustomsChargeCode(PLCustomsChargeTypeList.Codes._074A, PLCustomsChargeTypeList.Descriptions._074A));
		AssertGetCharge(PLCustomsChargeTypeList.Codes._080B, new CustomsChargeCode(PLCustomsChargeTypeList.Codes._080B, PLCustomsChargeTypeList.Descriptions._080B));
		AssertGetCharge(PLCustomsChargeTypeList.Codes._1STW, new CustomsChargeCode(PLCustomsChargeTypeList.Codes._1STW, PLCustomsChargeTypeList.Descriptions._1STW));
		AssertGetCharge(PLCustomsChargeTypeList.Codes._2STW, new CustomsChargeCode(PLCustomsChargeTypeList.Codes._2STW, PLCustomsChargeTypeList.Descriptions._2STW));
	}

	public void TestChargeOverridden()
	{
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AL, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AL).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AB, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AB).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AD, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AD).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AE, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AE).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AF, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AF).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AG, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AG).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AH, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AH).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AI, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AI).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AJ, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AJ).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AK, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AK).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.AN, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.AN).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.BA, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.BA).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.BB, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.BB).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.BD, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.BD).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.BE, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.BE).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.BF, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.BF).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.BC, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.BC).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions.BG, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes.BG).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions._071V, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes._071V).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions._072X, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes._072X).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions._073V, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes._073V).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions._074A, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes._074A).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions._080B, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes._080B).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions._1STW, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes._1STW).Description);
		AssertEquals(PLCustomsChargeTypeList.Descriptions._2STW, incoTermAndChargeFactory.GetCharge(PLCustomsChargeTypeList.Codes._2STW).Description);
	}

	protected override string GetCountryContext() => Enterprise.Core.Constants.CountryCodes.Poland;

	protected override string FreightToEUBorderCode => PLCustomsChargeTypeList.Codes.AK;
	protected override string FreightAfterEUBorderCode => PLCustomsChargeTypeList.Codes._071V;

	public void TestCharge_1STW()
	{
		TestCharge(PLCustomsChargeTypeList.Codes._1STW, false, true, false, true, true, true, false, false, true);

		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = MessageTypeList.Codes.Import;
		var header = dec.Invoices.AddNew();
		header.JZ_IncoTerm = JZIncoTermList.Codes.EXW;
		var groupCharge1 = header.GroupCharges.AddNew();

		groupCharge1.J7_ChargeType = PLCustomsChargeTypeList.Codes._1STW;
		TestInvoiceApplicationCharge(groupCharge1, PLCustomsChargeTypeList.Codes._1STW, false, false, true);

		var charge1 = header.Charges.AddNew();
		charge1.J7_ChargeType = PLCustomsChargeTypeList.Codes._1STW;
		TestInvoiceCharge(charge1, PLCustomsChargeTypeList.Codes._1STW, false, false, true);

		dec.JE_MessageType = MessageTypeList.Codes.Export;
		var groupCharge2 = header.GroupCharges.AddNew();
		groupCharge2.J7_ChargeType = PLCustomsChargeTypeList.Codes._1STW;
		TestInvoiceApplicationCharge(groupCharge2, PLCustomsChargeTypeList.Codes._1STW, false, false, true);

		var charge2 = header.Charges.AddNew();
		charge2.J7_ChargeType = PLCustomsChargeTypeList.Codes._1STW;
		TestInvoiceCharge(charge2, PLCustomsChargeTypeList.Codes._1STW, false, false, true);
	}

	public void TestCharge_071V() => TestCharge(PLCustomsChargeTypeList.Codes._071V, false, true, true, true, false, true, false, false, true);

	public void TestCharge(string customsChargeCode,
		bool isDutiable, bool isDutiableDeemedForThisCharge,
		bool isVATible, bool isVATibleDeemedForThisCharge,
		bool isStatisticalValueApplicable, bool isStatisticalValueApplicableDeemed,
		bool isIncludedInITOTIfDeemed, bool isIncludedInITOTDeemedForThisCharge,
		bool isIncoTermNeutral
	)
	{
		var charge = incoTermAndChargeFactory.GetCharge(customsChargeCode);

		CombineAssertions(() =>
		{
			AssertEquals("customsChargeCode", customsChargeCode, charge.Code);
			AssertEquals("isDutiable", isDutiable, charge.IsDutiable);
			AssertEquals("isDutiableDeemedForThisCharge", isDutiableDeemedForThisCharge, charge.IsDutiableDeemedForThisCharge);
			AssertEquals("isVATible", isVATible, charge.IsVATible);
			AssertEquals("isVATibleDeemedForThisCharge", isVATibleDeemedForThisCharge, charge.IsVATibleDeemedForThisCharge);
			AssertEquals("isStatisticalValueApplicable", isStatisticalValueApplicable, charge.IsStatisticalValueApplicable);
			AssertEquals("isStatisticalValueApplicableDeemed", isStatisticalValueApplicableDeemed, charge.IsStatisticalValueApplicableDeemed);
			AssertEquals("isIncludedInITOTIfDeemed", isIncludedInITOTIfDeemed, charge.IsIncludedInITOTIfDeemed);
			AssertEquals("isIncludedInITOTDeemedForThisCharge", isIncludedInITOTDeemedForThisCharge, charge.IsIncludedInITOTDeemedForThisCharge);
			AssertEquals("isIncoTermNeutral", isIncoTermNeutral, charge.IsIncoTermNeutral);
		});
	}

	public void TestInvoiceApplicationCharge(EU.Business.Declaration.InvoiceApportionCharge invoiceCharge,
		string customsChargeCode, bool isDutiable, bool isGSTApplicable, bool isStatisticalValueApplicable)
	{
		CombineAssertions(() =>
		{
			AssertEquals(customsChargeCode, invoiceCharge.J7_ChargeType);
			AssertEquals(isDutiable, invoiceCharge.J7_IsDutiable);
			AssertEquals(isGSTApplicable, invoiceCharge.J7_IsGSTApplicable);
			AssertEquals(isStatisticalValueApplicable, invoiceCharge.J7_IsStatisticalValueApplicable);
			AssertNoNotifications(invoiceCharge.J7_ChargeTypeInfo);
			AssertNoNotifications(invoiceCharge.J7_IsDutiableInfo);
			AssertNoNotifications(invoiceCharge.J7_IsGSTApplicableInfo);
			AssertNoNotifications(invoiceCharge.J7_IsStatisticalValueApplicableInfo);
		});
	}

	public void TestInvoiceCharge(InvoiceCharge invoiceCharge,
		string customsChargeCode, bool isDutiable, bool isGSTApplicable, bool isStatisticalValueApplicable)
	{
		CombineAssertions(() =>
		{
			AssertEquals(customsChargeCode, invoiceCharge.J7_ChargeType);
			AssertEquals(isDutiable, invoiceCharge.J7_IsDutiable);
			AssertEquals(isGSTApplicable, invoiceCharge.J7_IsGSTApplicable);
			AssertEquals(isStatisticalValueApplicable, invoiceCharge.J7_IsStatisticalValueApplicable);
			AssertNoNotifications(invoiceCharge.J7_ChargeTypeInfo);
			AssertNoNotifications(invoiceCharge.J7_IsDutiableInfo);
			AssertNoNotifications(invoiceCharge.J7_IsGSTApplicableInfo);
			AssertNoNotifications(invoiceCharge.J7_IsStatisticalValueApplicableInfo);
		});
	}

	protected override void AssertAfterEUBorderCharge(JobComInvCharge charge)
	{
		Assert("J7_IsDutiable", !charge.J7_IsDutiable);
		Assert("J7_IsGSTApplicable", charge.J7_IsGSTApplicable);
		Assert("J7_IsStatisticalValueApplicable", !charge.J7_IsStatisticalValueApplicable);
	}
}
