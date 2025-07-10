using System;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class HRTCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<HousebillReleaseTypeCalculator, Xsd.HRTCalculator>
	{
		#region Override ImportCore Method

		protected override void ImportFromValueObjectCore()
		{
			Assert("Tested in base class", true);
		}

		#endregion

		#region Override ExportCore Method

		protected override void ExportToValueObjectCore()
		{
			Assert("Tested in base class", true);
		}

		#endregion

		#region Override Members

		protected override ZString CalculatorType
		{
			get { return HousebillReleaseTypeCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(HRTCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<HousebillReleaseTypeCalculator, Xsd.HRTCalculator> CalculatorGenerator
		{
			get { return new HRTCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.HRTCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.HRTCalculator();
			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(HousebillReleaseTypeCalculator calculator)
		{
		}

		#endregion
	}
}
