using System;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class WPKCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<WarehousePackCalculator, Xsd.WPKCalculator>
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
			get { return WarehousePackCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(WPKCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<WarehousePackCalculator, Xsd.WPKCalculator> CalculatorGenerator
		{
			get { return new WPKCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.WPKCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.WPKCalculator();
			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(WarehousePackCalculator calculator)
		{
		}

		#endregion
	}
}
