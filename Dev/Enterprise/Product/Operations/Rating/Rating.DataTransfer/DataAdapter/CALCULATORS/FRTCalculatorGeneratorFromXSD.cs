using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class FRTCalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<FreightInclusiveCalculator, Xsd.FRTCalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, FreightInclusiveCalculator calculator, Xsd.FRTCalculator calculatorXSD, IValueObjectImportContext context)
		{
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, FreightInclusiveCalculator calculator, Xsd.FRTCalculator calculatorXSD, INotifications notifications)
		{
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.FRTCalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return FreightInclusiveCalculator.Code; }
		}

		#endregion
	}
}

