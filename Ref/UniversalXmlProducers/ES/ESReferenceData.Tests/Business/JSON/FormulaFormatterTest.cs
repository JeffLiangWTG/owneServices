using System.Collections.Generic;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

[TestFixture]
public class FormulaFormatterTest
{
	List<RefCusMapUOMSchema.MapCodes> UOMMapCodesList
		=> [
				new () { ZZM_CustomsValue = "MI", ZZM_CW1orCommercialValue = "MIL" },
				new () { ZZM_CustomsValue = "HL", ZZM_CW1orCommercialValue = "HLT" }
			];

	void AssertFormulaFormatted(string jsonFormula, string expectedFormattedFormula, bool isAdValorem = true)
	{
		var formulaFormatted = FormulaFormatter.ParseJsonFormula(jsonFormula, UOMMapCodesList, isAdValorem);
		Assert.That(formulaFormatted, Is.EqualTo(expectedFormattedFormula));
	}

	[Test]
	public void TestParseAdValorem()
	{
		AssertFormulaFormatted("76 %", "0.76*VFD");
		AssertFormulaFormatted("5,8 %", "0.058*VFD");
		AssertFormulaFormatted("5.7 %", "0.057*VFD");
		AssertFormulaFormatted("5.76588 %", "0.05766*VFD");
		AssertFormulaFormatted("5.d7 %", string.Empty);
	}

	[Test]
	public void TestParseSpecific()
	{
		AssertFormulaFormatted("35 EUR MI", "35*[MIL]");
		AssertFormulaFormatted("34.5 EUR MI", "34.5*[MIL]");
		AssertFormulaFormatted("23,2 EUR MI", "23.2*[MIL]");
		AssertFormulaFormatted("35 EUR XX", "35*[XX]");
	}

	[Test]
	public void TestParseMixed()
	{
		AssertFormulaFormatted("16 % + 50,7 EUR HL", "0.16*VFD + 50.7*[HLT]");
		AssertFormulaFormatted("16 % + 50,7 EUR HL", "0.16*PVP + 50.7*[HLT]", false);
		AssertFormulaFormatted("22 % + 5 EUR MI + 5%", string.Empty);
		AssertFormulaFormatted("9 % + 20 EUR XX", "0.09*VFD + 20*[XX]");
	}

	[Test]
	public void TestParseMaximum()
	{
		AssertFormulaFormatted("8 % MAX 2.8 EUR HL", "MIN(0.08*VFD, 2.8*[HLT])");
		AssertFormulaFormatted("8 % MAX 2,7 EUR HL", "MIN(0.08*VFD, 2.7*[HLT])");
		AssertFormulaFormatted("8 % MAX 72 EUR HL", "MIN(0.08*VFD, 72*[HLT])");
		AssertFormulaFormatted("8.2 % MAX 15 EUR HL", "MIN(0.082*VFD, 15*[HLT])");
		AssertFormulaFormatted("23,3 % MAX 15 EUR HL", "MIN(0.233*VFD, 15*[HLT])");
		AssertFormulaFormatted("8 % MAX 72 EUR XX", "MIN(0.08*VFD, 72*[XX])");
		AssertFormulaFormatted("8d % MAX 72 EUR HL", string.Empty);
		AssertFormulaFormatted("8 % MAX 72 EUR HL MAX 7 %", string.Empty);
		AssertFormulaFormatted("8 % MAX 72 EUR HL MAX 2 EUR HL", string.Empty);
	}

	[Test]
	public void TestParseMinimum()
	{
		AssertFormulaFormatted("15 % MIN 18 EUR MI", "MAX(0.15*VFD, 18*[MIL])");
		AssertFormulaFormatted("15 % MIN 3.2 EUR MI", "MAX(0.15*VFD, 3.2*[MIL])");
		AssertFormulaFormatted("15 % MIN 42,5 EUR MI", "MAX(0.15*VFD, 42.5*[MIL])");
		AssertFormulaFormatted("7.2 % MIN 18 EUR MI", "MAX(0.072*VFD, 18*[MIL])");
		AssertFormulaFormatted("6,33 % MIN 18 EUR MI", "MAX(0.0633*VFD, 18*[MIL])");
		AssertFormulaFormatted("15 % MIN 18 EUR XX", "MAX(0.15*VFD, 18*[XX])");
		AssertFormulaFormatted("15d % MIN 18 EUR MI", string.Empty);
		AssertFormulaFormatted("15 % MIN 18 EUR MI MIN 5 %", string.Empty);
		AssertFormulaFormatted("15 % MIN 18 EUR MI MIN 3.5 EUR HL", string.Empty);
	}

	[Test]
	public void TestParseMaximumAndMinimum()
	{
		AssertFormulaFormatted("4,5 % MIN 0,3 EUR HL MAX 0,8 EUR HL", "MIN(MAX(0.045*VFD, 0.3*[HLT]), 0.8*[HLT])");
		AssertFormulaFormatted("3.5 % MIN 0,3 EUR HL MAX 0,8 EUR HL", "MIN(MAX(0.035*VFD, 0.3*[HLT]), 0.8*[HLT])");
		AssertFormulaFormatted("42.52 % MIN 0,3 EUR HL MAX 0,8 EUR HL", "MIN(MAX(0.4252*VFD, 0.3*[HLT]), 0.8*[HLT])");
		AssertFormulaFormatted("20 % MIN 0.4 EUR HL MAX 0,8 EUR HL", "MIN(MAX(0.2*VFD, 0.4*[HLT]), 0.8*[HLT])");
		AssertFormulaFormatted("20 % MIN 4 EUR XX MAX 0,8 EUR HL", "MIN(MAX(0.2*VFD, 4*[XX]), 0.8*[HLT])");
		AssertFormulaFormatted("20 % MIN 4 EUR HL MAX 0,8 EUR XX", "MIN(MAX(0.2*VFD, 4*[HLT]), 0.8*[XX])");
		AssertFormulaFormatted("4,5 % MIN 0,3 EUR HL MAX 0,8 EUR HL MIN 0.4 EUR HL", string.Empty);
		AssertFormulaFormatted("4,5 % MIN 0,3 EUR HL MAX 0,8 EUR HL MIN 0,4 EUR HL", string.Empty);
		AssertFormulaFormatted("4,5 % MIN 0,3 EUR HL MAX 0,8 EUR HL MIN 5 %", "MIN(MAX(0.045*VFD, 0.3*[HLT]), 0.8*[HL MIN 5 %])");
		AssertFormulaFormatted("4,5 % MIN 0,3 EUR HL MAX 0,8 EUR HL MAX 0,4 EUR HL", string.Empty);
		AssertFormulaFormatted("4,5 % MIN 0,3 EUR HL MAX 0,8 EUR HL MAX 4 %", string.Empty);
		AssertFormulaFormatted("4,5 % MIN 0.4 EUR HL MIN 0,3 EUR HL MAX 0,8 EUR HL", string.Empty);
	}

	[Test]
	public void TestParseFree()
	{
		AssertFormulaFormatted("0 %", "0");
	}

	[Test]
	public void TestParseExciseRateBasedOnPVP()
	{
		AssertFormulaFormatted("51 %", "0.51*PVP", false);
		AssertFormulaFormatted("51.1 %", "0.511*PVP", false);
		AssertFormulaFormatted("51,2 %", "0.512*PVP", false);
		AssertFormulaFormatted("5d %", string.Empty, false);
	}
}
