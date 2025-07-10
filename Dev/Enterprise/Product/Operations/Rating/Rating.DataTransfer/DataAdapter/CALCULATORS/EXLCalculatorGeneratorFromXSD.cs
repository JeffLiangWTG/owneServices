using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class EXLCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<ExcludeCompanyTariffsCalculator, EXLCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(Business.RateLine rateLine, ExcludeCompanyTariffsCalculator calculator, EXLCalculator calculatorXSD, IValueObjectImportContext context)
		{
		}

		#endregion

		#region Export
		protected override void ExportToValueObjectCore(Business.RateLine rateLine, ExcludeCompanyTariffsCalculator calculator, EXLCalculator calculatorXSD, INotifications notifications)
		{
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.EXLCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return ExcludeCompanyTariffsCalculator.Code; }
		}

		#endregion
	}
}

