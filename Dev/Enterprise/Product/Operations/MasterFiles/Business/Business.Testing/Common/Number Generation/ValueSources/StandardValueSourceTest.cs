using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StandardValueSourceTest : TestCaseWithFactory
	{
		public void TestBranch()
		{
			AssertEquals("BNE", ValueProviders[Keys.BranchCode].GetValue(Generator, ""));
		}

		public void TestClientCoded()
		{
			AssertEquals("BOB1", ValueProviders[Keys.ClientCoded1].GetValue(Generator, "BOB1"));
			AssertEquals("JOE1", ValueProviders[Keys.ClientCoded1].GetValue(Generator, "JOE1"));

			AssertEquals("BOB2", ValueProviders[Keys.ClientCoded2].GetValue(Generator, "BOB2"));
			AssertEquals("JOE2", ValueProviders[Keys.ClientCoded2].GetValue(Generator, "JOE2"));

			AssertEquals("BOB3", ValueProviders[Keys.ClientCoded3].GetValue(Generator, "BOB3"));
			AssertEquals("JOE3", ValueProviders[Keys.ClientCoded3].GetValue(Generator, "JOE3"));
		}

		public void TestCompany()
		{
			AssertEquals("EDI", ValueProviders[Keys.CompanyCode].GetValue(Generator, ""));
		}

		[TestDate(2014, 02, 1)]
		public void TestMonthAs2Digits_Feb()
		{
			AssertEquals("02", ValueProviders[Keys.MonthAs2Digits].GetValue(Generator, ""));
		}

		[TestDate(2014, 12, 1)]
		public void TestMonthAs2Digits_Dec()
		{
			AssertEquals("12", ValueProviders[Keys.MonthAs2Digits].GetValue(Generator, ""));
		}

		[TestDate(2005, 1, 1)]
		public void TestMonthAsLetter_Jan()
		{
			AssertEquals("Jan = 'A'", "A", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 2, 1)]
		public void TestMonthAsLetter_Feb()
		{
			AssertEquals("Feb = 'B'", "B", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 3, 1)]
		public void TestMonthAsLetter_Mar()
		{
			AssertEquals("Mar = 'C'", "C", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 4, 1)]
		public void TestMonthAsLetter_Apr()
		{
			AssertEquals("Apr = 'D'", "D", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 5, 1)]
		public void TestMonthAsLetter_May()
		{
			AssertEquals("May = 'E'", "E", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 6, 1)]
		public void TestMonthAsLetter_Jun()
		{
			AssertEquals("Jun = 'F'", "F", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 7, 1)]
		public void TestMonthAsLetter_Jul()
		{
			AssertEquals("Jul = 'G'", "G", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 8, 1)]
		public void TestMonthAsLetter_Aug()
		{
			AssertEquals("Aug = 'H'", "H", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 9, 1)]
		public void TestMonthAsLetter_Sep()
		{
			AssertEquals("Sep = 'I'", "I", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 10, 1)]
		public void TestMonthAsLetter_Oct()
		{
			AssertEquals("Oct = 'J'", "J", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 11, 1)]
		public void TestMonthAsLetter_Nov()
		{
			AssertEquals("Nov = 'K'", "K", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2005, 12, 1)]
		public void TestMonthAsLetter_Dec()
		{
			AssertEquals("Dec = 'L'", "L", ValueProviders[Keys.MonthAsLetter].GetValue(Generator, ""));
		}

		[TestDate(1999, 1, 1)]
		public void TestYearAs1Digit_1999()
		{
			AssertEquals("9", ValueProviders[Keys.YearAsDigit].GetValue(Generator, "1"));
			AssertEquals("99", ValueProviders[Keys.YearAsDigit].GetValue(Generator, "2"));
			AssertEquals("1999", ValueProviders[Keys.YearAsDigit].GetValue(Generator, "4"));
		}

		[TestDate(2005, 1, 1)]
		public void TestYearAs2Digits_2005()
		{
			AssertEquals("5", ValueProviders[Keys.YearAsDigit].GetValue(Generator, "1"));
			AssertEquals("05", ValueProviders[Keys.YearAsDigit].GetValue(Generator, "2"));
			AssertEquals("2005", ValueProviders[Keys.YearAsDigit].GetValue(Generator, "4"));
		}

		[TestDate(2016, 1, 1)]
		public void TestYearAs4Digits_2016()
		{
			AssertEquals("6", ValueProviders[Keys.YearAsDigit].GetValue(Generator, "1"));
			AssertEquals("16", ValueProviders[Keys.YearAsDigit].GetValue(Generator, "2"));
			AssertEquals("2016", ValueProviders[Keys.YearAsDigit].GetValue(Generator, "4"));
		}

		[TestDate(2001, 1, 1)]
		public void TestYearAsLetter_2001()
		{
			AssertEquals("A", ValueProviders[Keys.YearAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2002, 1, 1)]
		public void TestYearAsLetter_2002()
		{
			AssertEquals("B", ValueProviders[Keys.YearAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2026, 1, 1)]
		public void TestYearAsLetter_2026()
		{
			AssertEquals("Z", ValueProviders[Keys.YearAsLetter].GetValue(Generator, ""));
		}

		[TestDate(1999, 1, 1)]
		public void TestYearAsLetter_1999()
		{
			AssertEquals("Y", ValueProviders[Keys.YearAsLetter].GetValue(Generator, ""));
		}

		[TestDate(2030, 1, 1)]
		public void TestYearAsLetter_2030()
		{
			AssertEquals("D", ValueProviders[Keys.YearAsLetter].GetValue(Generator, ""));
		}

		public void TestUniversalOfficeCode()
		{
			OrgHeader branchOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader companyOrg = Factory.NewWithValidTestData<OrgHeader>();

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = companyOrg.PK;

			GlbBranch branch = company.Branches.AddNew();
			branch.GB_OH_OrgProxy = branchOrg.PK;

			Generator.Context = new NumberGeneratorContext(company.PK, branch.PK, ZGuid.Empty);

			AssertEquals("No UOC set", "", ValueProviders[Keys.UniversalOfficeCode].GetValue(Generator, ""));

			SetUOC(companyOrg, "Blah");
			AssertEquals("UOC on Company", "Blah", ValueProviders[Keys.UniversalOfficeCode].GetValue(Generator, ""));

			SetUOC(branchOrg, "Npaj");
			AssertEquals("UOC on Branch overrides UOC on company", "Npaj", ValueProviders[Keys.UniversalOfficeCode].GetValue(Generator, ""));
		}

		[TestDate(2021, 1, 1)]
		public void TestQuarter_Q1()
		{
			AssertEquals("Q1", ValueProviders[Keys.Quarter].GetValue(Generator, ""));
		}
		[TestDate(2021, 4, 1)]
		public void TestQuarter_Q2()
		{
			AssertEquals("Q2", ValueProviders[Keys.Quarter].GetValue(Generator, ""));
		}
		[TestDate(2021, 7, 1)]
		public void TestQuarter_Q3()
		{
			AssertEquals("Q3", ValueProviders[Keys.Quarter].GetValue(Generator, ""));
		}
		[TestDate(2021, 10, 1)]
		public void TestQuarter_Q4()
		{
			AssertEquals("Q4", ValueProviders[Keys.Quarter].GetValue(Generator, ""));
		}

		NumberGeneratorValueProviderCollection ValueProviders
		{
			get
			{
				if (valueProviders == null)
				{
					valueProviders = new NumberGeneratorValueProviderCollection();
					valueProviders.AddRange(new StandardValueSource());
				}
				return valueProviders;
			}
		}
		NumberGeneratorValueProviderCollection valueProviders;

		NumberGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new NumberGenerator();
					generator.Factory = Factory;
					generator.Context = new NumberGeneratorContext();
				}
				return generator;
			}
		}
		NumberGenerator generator;

		void SetUOC(OrgHeader org, ZString value)
		{
			OrgCusCode code = org.CustomsCodes.AddNew();
			code.OK_CodeType = "UOC";
			code.OK_CustomsRegNo = value;
		}
	}
}
