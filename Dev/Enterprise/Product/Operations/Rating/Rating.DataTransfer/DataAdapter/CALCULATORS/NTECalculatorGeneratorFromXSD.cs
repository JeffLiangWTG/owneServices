using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators
{
	public class NTECalculatorGeneratorFromXSD : RateCalculatorGeneratorFromXSD<NoteCalculator, Xsd.NTECalculator>
	{
		#region Import

		protected override void ImportFromValueObjectCore(RateLine rateLine, NoteCalculator calculator, Xsd.NTECalculator calculatorXSD, IValueObjectImportContext context)
		{
			ImportRateItemWithDescAmtOrRate(calculator, calculatorXSD.RateItems, false, context);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(RateLine rateLine, NoteCalculator calculator, Xsd.NTECalculator calculatorXSD, INotifications notifications)
		{
			ExportRateItemWithDescAmtOrRate(calculator, calculatorXSD.RateItems, false, notifications);
		}

		#endregion

		#region Implementation

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.NTECalculatorSchema; }
		}

		protected override ZString CalculatorType
		{
			get { return NoteCalculator.Code; }
		}

		#endregion
	}
}

