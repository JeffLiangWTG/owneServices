using System;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class NTECalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<NoteCalculator, Xsd.NTECalculator>
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
			get { return NoteCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(NTECalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<NoteCalculator, Xsd.NTECalculator> CalculatorGenerator
		{
			get { return new NTECalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.NTECalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.NTECalculator();
			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(NoteCalculator calculator)
		{
		}

		#endregion
	}
}
