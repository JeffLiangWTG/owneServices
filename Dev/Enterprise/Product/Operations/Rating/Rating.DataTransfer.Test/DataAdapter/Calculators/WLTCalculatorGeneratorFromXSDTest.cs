using System;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class WLTCalculatorGeneratorFromXSDTest : RateCalculatorGeneratorFromXSDTest<WarehouseLocationTypeCalculator, Xsd.WLTCalculator>
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
			get { return WarehouseLocationTypeCalculator.Code; }
		}

		protected override Type ExpectedGeneratorType
		{
			get { return typeof(WLTCalculatorGeneratorFromXSD); }
		}

		protected override RateCalculatorGeneratorFromXSD<WarehouseLocationTypeCalculator, Xsd.WLTCalculator> CalculatorGenerator
		{
			get { return new WLTCalculatorGeneratorFromXSD(); }
		}

		#endregion

		#region Data For Test

		protected override Xsd.WLTCalculator CalculatorXSDForTest(bool specified)
		{
			calculatorXSD = new Xsd.WLTCalculator();
			return calculatorXSD;
		}

		protected override void SetTestDataCalculatorForTest(WarehouseLocationTypeCalculator calculator)
		{
		}

		#endregion
	}
}
