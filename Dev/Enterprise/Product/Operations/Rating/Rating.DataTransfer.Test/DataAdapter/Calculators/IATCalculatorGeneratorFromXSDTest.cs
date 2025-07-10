using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class IATCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<PackageCountCalculator, Xsd.IATCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Xsd.IATCalculator calculatorXSD = CalculatorXSDForTest(false);
			Buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Buffer);

			#region Test (Specified = false)

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator is an Agency Caluclator", typeof(PackageCountCalculator), RateLineForCalculatorXSDTest.Calculator.GetType());

			AssertEquals("Calculator 's BaseCharge", 0m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's RatePerKG", 0m, CalculatorGenerated.PerKG);
			AssertEquals("Calculator 's FirstPackage", 0m, CalculatorGenerated.FirstPackageRate);
			AssertEquals("Calculator 's AdditionalPackage", 0m, CalculatorGenerated.AddtionalPackageRate);

			#endregion

			#region Test (Specified = true)

			calculatorXSD = CalculatorXSDForTest(true);

			CalculatorGenerator.ImportFromValueObject(RateLineForCalculatorXSDTest, calculatorXSD, context);

			AssertEquals("Calculator 's BaseCharge", 1.2m, CalculatorGenerated.BaseRate);
			AssertEquals("Calculator 's RatePerKG", 1.3m, CalculatorGenerated.PerKG);
			AssertEquals("Calculator 's FirstPackage", 1.4m, CalculatorGenerated.FirstPackageRate);
			AssertEquals("Calculator 's AdditionalPackage", 1.5m, CalculatorGenerated.AddtionalPackageRate);

			#endregion
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Xsd.IATCalculator calculatorXSD = new Xsd.IATCalculator();

			CalculatorGenerator.ExportToValueObject(RateLineForCalculatorTest, calculatorXSD, new NotificationBuffer());

			AssertEquals("Calculator 's BaseCharge", 1.2m, calculatorXSD.BaseCharge);
			AssertEquals("Calculator 's BaseCharge Specified", true, calculatorXSD.BaseChargeSpecified);
			AssertEquals("Calculator 's RatePerKG", 1.3m, calculatorXSD.RatePerKG);
			AssertEquals("Calculator 's RatePerKG Specified", true, calculatorXSD.RatePerKGSpecified);
			AssertEquals("Calculator 's FirstPackage", 1.4m, calculatorXSD.FirstPackage);
			AssertEquals("Calculator 's FirstPackage Specified", true, calculatorXSD.FirstPackageSpecified);
			AssertEquals("Calculator 's AdditionalPackage", 1.5m, calculatorXSD.AdditionalPackage);
			AssertEquals("Calculator 's AdditionalPackage Specified", true, calculatorXSD.AdditionalPackageSpecified);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return PackageCountCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(IATCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<PackageCountCalculator, Xsd.IATCalculator> CalculatorGenerator
		{
			get { return new IATCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.IATCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.IATCalculator();

			if (specified)
			{
				calculatorXSD.BaseCharge = 1.2m;
				calculatorXSD.RatePerKG = 1.3m;
				calculatorXSD.FirstPackage = 1.4m;
				calculatorXSD.AdditionalPackage = 1.5m;
			}

			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(PackageCountCalculator calculator)
		{
			calculator.BaseRate = 1.2m;
			calculator.PerKG = 1.3m;
			calculator.FirstPackageRate = 1.4m;
			calculator.AddtionalPackageRate = 1.5m;
		}

		#endregion
	}
}
