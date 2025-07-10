using System;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class EQHCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<EquipmentHireCalculator, Xsd.EQHCalculator>
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
			get { return EquipmentHireCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(EQHCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<EquipmentHireCalculator, Xsd.EQHCalculator> CalculatorGenerator
		{
			get { return new EQHCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.EQHCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.EQHCalculator();
			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(EquipmentHireCalculator calculator)
		{
		}

		#endregion
	}
}
